<#
.SYNOPSIS
    Builds the HouseBills installer (artifacts\installer\HouseBills-Setup-<version>.exe).

.DESCRIPTION
    1. Publishes the WPF app self-contained for win-x64 (target PCs need no .NET install).
    2. Downloads Microsoft's SqlLocalDB.msi and the VC++ 2015-2022 x64 Redistributable (which LocalDB needs) once
       into artifacts\prereqs (signatures verified), for bundling.
    3. Compiles installer\HouseBills.iss with Inno Setup 6 (winget install JRSoftware.InnoSetup).

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File installer\build-installer.ps1 -Version 1.0.0
#>
param(
    [string]$Version = '1.0.0'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$artifacts = Join-Path $root 'artifacts'
$publishDir = Join-Path $artifacts 'publish'
$prereqDir = Join-Path $artifacts 'prereqs'
$localDbMsi = Join-Path $prereqDir 'SqlLocalDB.msi'
$vcRedist = Join-Path $prereqDir 'vc_redist.x64.exe'
$vcRedistUrl = 'https://aka.ms/vs/17/release/vc_redist.x64.exe'

# Microsoft's SQL Server Express bootstrapper; it can download just the LocalDB MSI.
$sqlExpressBootstrapperUrl = 'https://go.microsoft.com/fwlink/p/?linkid=2216019'

function Assert-MicrosoftSigned([string]$Path) {
    $signature = Get-AuthenticodeSignature $Path
    if ($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -notlike 'CN=Microsoft Corporation*') {
        throw "$Path is not validly signed by Microsoft (status: $($signature.Status))."
    }
}

function Find-InnoSetupCompiler {
    $candidates = @(
        "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
    )
    $found = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
    if (-not $found) {
        $command = Get-Command ISCC.exe -ErrorAction SilentlyContinue
        if ($command) { $found = $command.Source }
    }
    if (-not $found) {
        throw 'Inno Setup 6 was not found. Install it with: winget install JRSoftware.InnoSetup'
    }
    return $found
}

Write-Host "Publishing HouseBills $Version (self-contained win-x64)..."
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
dotnet publish (Join-Path $root 'src\HouseBills.Wpf') -c Release -r win-x64 --self-contained -p:Version=$Version -o $publishDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE." }

if (-not (Test-Path $localDbMsi)) {
    # Only needed once per machine; the MSI is cached in artifacts\prereqs afterwards.
    Write-Host 'Downloading the SQL Server LocalDB installer (Microsoft''s download tool asks for administrator approval)...'
    New-Item -ItemType Directory -Force $prereqDir | Out-Null
    $bootstrapper = Join-Path $prereqDir 'SQL2022-SSEI-Expr.exe'
    Invoke-WebRequest $sqlExpressBootstrapperUrl -OutFile $bootstrapper -UseBasicParsing
    Assert-MicrosoftSigned $bootstrapper

    $download = Start-Process $bootstrapper -ArgumentList '/ACTION=Download', "/MEDIAPATH=$prereqDir", '/MEDIATYPE=LocalDB', '/QUIET' -Wait -PassThru
    if ($download.ExitCode -ne 0) { throw "LocalDB download failed with exit code $($download.ExitCode)." }

    $downloaded = Get-ChildItem $prereqDir -Recurse -Filter 'SqlLocalDB.msi' | Select-Object -First 1
    if (-not $downloaded) { throw 'The bootstrapper did not produce SqlLocalDB.msi.' }
    Move-Item $downloaded.FullName $localDbMsi -Force
}
Assert-MicrosoftSigned $localDbMsi

# LocalDB's SQL Writer service needs the VC++ runtime, which a clean Windows doesn't have (the MSI fails with 1603).
if (-not (Test-Path $vcRedist)) {
    Write-Host 'Downloading the Visual C++ Redistributable...'
    New-Item -ItemType Directory -Force $prereqDir | Out-Null
    Invoke-WebRequest $vcRedistUrl -OutFile $vcRedist -UseBasicParsing
}
Assert-MicrosoftSigned $vcRedist

Write-Host 'Compiling installer...'
$iscc = Find-InnoSetupCompiler
& $iscc "/DAppVersion=$Version" "/DPublishDir=$publishDir" "/DLocalDbMsi=$localDbMsi" "/DVcRedist=$vcRedist" "/O$(Join-Path $artifacts 'installer')" (Join-Path $PSScriptRoot 'HouseBills.iss')
if ($LASTEXITCODE -ne 0) { throw "Inno Setup failed with exit code $LASTEXITCODE." }

Write-Host "Installer: $(Join-Path $artifacts "installer\HouseBills-Setup-$Version.exe")"

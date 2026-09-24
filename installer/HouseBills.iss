; Inno Setup script for HouseBills. Build with installer\build-installer.ps1 (it publishes the app and
; fetches the LocalDB MSI first), or directly: ISCC /DAppVersion=1.0.0 installer\HouseBills.iss

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#ifndef PublishDir
  #define PublishDir "..\artifacts\publish"
#endif
#ifndef LocalDbMsi
  #define LocalDbMsi "..\artifacts\prereqs\SqlLocalDB.msi"
#endif
#ifndef VcRedist
  #define VcRedist "..\artifacts\prereqs\vc_redist.x64.exe"
#endif
#define AppExe "HouseBills.Wpf.exe"

[Setup]
; Keep AppId stable forever: it is how upgrades and uninstall find the installed app.
AppId={{44A16390-4973-42D5-B6FE-5658DF2FDAAF}
AppName=HouseBills
AppVersion={#AppVersion}
AppVerName=HouseBills {#AppVersion}
AppPublisher=HouseBills
VersionInfoVersion={#AppVersion}
DefaultDirName={autopf}\HouseBills
DisableProgramGroupPage=yes
; Admin rights are needed to install LocalDB (a per-machine MSI).
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
; Windows 10 1809 or later, as required by .NET 10.
MinVersion=10.0.17763
WizardStyle=modern
Compression=lzma2/max
SolidCompression=yes
OutputBaseFilename=HouseBills-Setup-{#AppVersion}
UninstallDisplayIcon={app}\{#AppExe}
SetupIconFile=..\src\HouseBills.Wpf\Assets\HouseBills.ico
UninstallDisplayName=HouseBills
; Close a running HouseBills before upgrading its files.
CloseApplications=yes
InfoBeforeFile=InstallInfo.txt

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Extracted on demand in PrepareToInstall, only when LocalDB is missing.
Source: "{#LocalDbMsi}"; Flags: dontcopy
Source: "{#VcRedist}"; Flags: dontcopy

[Icons]
Name: "{autoprograms}\HouseBills"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\HouseBills"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
; runasoriginaluser: the database lives in the (non-admin) user's LocalDB instance, so first start must run as them.
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,HouseBills}"; Flags: nowait postinstall skipifsilent runasoriginaluser

[Code]
const
  LocalDbVersionsKey = 'SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions';
  MinLocalDbMajorVersion = 13; { SQL Server 2016 }
  ErrorSuccessRebootInitiated = 1641;
  ErrorSuccessRebootRequired = 3010;
  ErrorNewerVersionInstalled = 1638;

function IsLocalDbInstalled: Boolean;
var
  Versions: TArrayOfString;
  I, DotPos: Integer;
begin
  Result := False;
  if not RegGetSubkeyNames(HKLM64, LocalDbVersionsKey, Versions) then
    Exit;
  for I := 0 to GetArrayLength(Versions) - 1 do
  begin
    DotPos := Pos('.', Versions[I]);
    if (DotPos > 1) and (StrToIntDef(Copy(Versions[I], 1, DotPos - 1), 0) >= MinLocalDbMajorVersion) then
    begin
      Result := True;
      Exit;
    end;
  end;
end;

// Runs a prerequisite installer. Returns '' on success, otherwise a message for the user.
// Its log goes to %TEMP% (not the setup's temp folder, which is deleted) so a failure can be diagnosed afterwards.
function RunPrerequisite(const Name, FileName, Executable, Params, LogFile: String; var NeedsRestart: Boolean): String;
var
  ResultCode: Integer;
  FilePath, LogPath, Args: String;
begin
  Result := '';
  FilePath := ExpandConstant('{tmp}\') + FileName;
  LogPath := ExpandConstant('{%TEMP}\') + LogFile;
  Args := Params;
  StringChangeEx(Args, '{file}', FilePath, True);
  StringChangeEx(Args, '{log}', LogPath, True);
  WizardForm.PreparingLabel.Caption := 'Installing ' + Name + '. This can take a few minutes...';
  WizardForm.PreparingLabel.Visible := True;
  ExtractTemporaryFile(FileName);
  Log('Installing ' + Name + '; log: ' + LogPath);
  if not Exec(Executable, Args, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Result := 'The ' + Name + ' installer could not be started: ' + SysErrorMessage(ResultCode);
    Exit;
  end;

  Log(Format('%s installer exit code: %d', [Name, ResultCode]));
  if (ResultCode = ErrorSuccessRebootRequired) or (ResultCode = ErrorSuccessRebootInitiated) then
    NeedsRestart := True
  else if (ResultCode <> 0) and (ResultCode <> ErrorNewerVersionInstalled) then
    Result := Format('%s could not be installed (error %d), so HouseBills was not installed.' + #13#10 + 'Details: %s', [Name, ResultCode, LogPath]);
end;

{ Installs prerequisites before any app files are copied, so a failure leaves nothing half-installed. }
function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  Result := '';
  if IsLocalDbInstalled then
    Exit;

  { LocalDB's SQL Writer service needs the VC++ runtime; without it the LocalDB MSI fails with 1603.
    vc_redist is a no-op (1638) when the same or a newer runtime is already installed. }
  Result := RunPrerequisite('Microsoft Visual C++ Redistributable', 'vc_redist.x64.exe',
    ExpandConstant('{tmp}\vc_redist.x64.exe'), '/install /quiet /norestart /log "{log}"',
    'HouseBills-VCRedist-install.log', NeedsRestart);
  if Result <> '' then
    Exit;

  Result := RunPrerequisite('Microsoft SQL Server Express LocalDB', 'SqlLocalDB.msi',
    ExpandConstant('{sys}\msiexec.exe'), '/i "{file}" /qn /norestart IACCEPTSQLLOCALDBLICENSETERMS=YES /l*v "{log}"',
    'HouseBills-LocalDB-install.log', NeedsRestart);
end;

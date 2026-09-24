<#
.SYNOPSIS
    Draws the HouseBills icon (a house holding a bill) and writes a multi-size .ico.

.DESCRIPTION
    Each size is drawn separately (not scaled) so small sizes stay crisp; detail lines are dropped below 32 px.
    The .ico stores PNG-compressed images, which Windows Vista+ supports at every size.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File tools\generate-icon.ps1
#>
param(
    [string]$OutFile = (Join-Path (Split-Path $PSScriptRoot -Parent) 'src\HouseBills.Wpf\Assets\HouseBills.ico')
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$sizes = 16, 24, 32, 48, 64, 128, 256
$blueTop = [System.Drawing.Color]::FromArgb(255, 0x1E, 0x88, 0xE5)
$blueBottom = [System.Drawing.Color]::FromArgb(255, 0x0D, 0x47, 0xA1)
$green = [System.Drawing.Color]::FromArgb(255, 0x2E, 0xA0, 0x43)

function New-RoundedRect([single]$x, [single]$y, [single]$w, [single]$h, [single]$r) {
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = 2 * $r
    $path.AddArc($x, $y, $d, $d, 180, 90)
    $path.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
    $path.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90)
    $path.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    $path.CloseFigure()
    return $path
}

function New-IconImage([int]$size) {
    $s = [single]$size
    $bmp = New-Object System.Drawing.Bitmap $size, $size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.Clear([System.Drawing.Color]::Transparent)

    # Background tile.
    $inset = [Math]::Max(0.5, $s * 0.03)
    $tile = New-RoundedRect $inset $inset ($s - 2 * $inset) ($s - 2 * $inset) ($s * 0.2)
    $gradient = New-Object System.Drawing.Drawing2D.LinearGradientBrush (New-Object System.Drawing.PointF 0, 0), (New-Object System.Drawing.PointF 0, $s), $blueTop, $blueBottom
    $g.FillPath($gradient, $tile)

    # House: roof + walls as one white pentagon.
    $white = [System.Drawing.Brushes]::White
    $left = $s * 0.20; $right = $s * 0.80; $eaves = $s * 0.46; $peak = $s * 0.16; $floor = $s * 0.84
    $house = [System.Drawing.PointF[]]@(
        (New-Object System.Drawing.PointF ($s * 0.5), $peak),
        (New-Object System.Drawing.PointF ($s * 0.88), $eaves),
        (New-Object System.Drawing.PointF $right, $eaves),
        (New-Object System.Drawing.PointF $right, $floor),
        (New-Object System.Drawing.PointF $left, $floor),
        (New-Object System.Drawing.PointF $left, $eaves),
        (New-Object System.Drawing.PointF ($s * 0.12), $eaves))
    $g.FillPolygon($white, $house)

    # Bill inside the house.
    $billX = $s * 0.34; $billY = $s * 0.50; $billW = $s * 0.32; $billH = $s * 0.28
    $billBrush = New-Object System.Drawing.SolidBrush $blueBottom
    $g.FillPath($billBrush, (New-RoundedRect $billX $billY $billW $billH ([Math]::Max(0.5, $s * 0.03))))
    if ($size -ge 32) {
        $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::White), ([Math]::Max(1, $s * 0.03))
        foreach ($row in 0.33, 0.58) {
            $y = $billY + $billH * $row
            $g.DrawLine($pen, $billX + $billW * 0.18, $y, $billX + $billW * 0.82, $y)
        }
        $pen.Dispose()
    }

    # Green "paid" check badge, bottom right (too small to read at 16 px).
    if ($size -lt 24) {
        $g.Dispose()
        return $bmp
    }

    $badge = $s * 0.30
    $bx = $s * 0.66; $by = $s * 0.64
    $g.FillEllipse((New-Object System.Drawing.SolidBrush $green), $bx, $by, $badge, $badge)
    $g.DrawEllipse((New-Object System.Drawing.Pen ([System.Drawing.Color]::White), ([Math]::Max(1, $s * 0.025))), $bx, $by, $badge, $badge)
    $checkPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::White), ([Math]::Max(1.2, $s * 0.04))
    $checkPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $checkPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $checkPen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $g.DrawLines($checkPen, [System.Drawing.PointF[]]@(
        (New-Object System.Drawing.PointF ($bx + $badge * 0.27), ($by + $badge * 0.52)),
        (New-Object System.Drawing.PointF ($bx + $badge * 0.44), ($by + $badge * 0.69)),
        (New-Object System.Drawing.PointF ($bx + $badge * 0.74), ($by + $badge * 0.34))))

    $g.Dispose()
    return $bmp
}

# Classic icon image: BITMAPINFOHEADER (height doubled for the mask), bottom-up 32-bit BGRA pixels, then an
# all-zero 1-bit AND mask (transparency comes from the alpha channel).
function ConvertTo-IconDib([System.Drawing.Bitmap]$bmp) {
    $size = $bmp.Width
    $stream = New-Object System.IO.MemoryStream
    $w = New-Object System.IO.BinaryWriter $stream
    $w.Write([uint32]40); $w.Write([int32]$size); $w.Write([int32]($size * 2))
    $w.Write([uint16]1); $w.Write([uint16]32); $w.Write([uint32]0); $w.Write([uint32]($size * $size * 4))
    $w.Write([int32]0); $w.Write([int32]0); $w.Write([uint32]0); $w.Write([uint32]0)
    for ($y = $size - 1; $y -ge 0; $y--) {
        for ($x = 0; $x -lt $size; $x++) {
            $c = $bmp.GetPixel($x, $y)
            $w.Write([byte]$c.B); $w.Write([byte]$c.G); $w.Write([byte]$c.R); $w.Write([byte]$c.A)
        }
    }
    $maskRowBytes = [int]([Math]::Ceiling($size / 32.0) * 4)
    $w.Write((New-Object byte[] ($maskRowBytes * $size)))
    $w.Flush()
    return , $stream.ToArray()
}

# ICO file: ICONDIR header, one ICONDIRENTRY per image, then the image data.
# 256 px is stored as PNG (standard); smaller sizes as classic bitmaps, which every icon loader understands.
$pngs = foreach ($size in $sizes) {
    $bmp = New-IconImage $size
    if ($size -ge 256) {
        $stream = New-Object System.IO.MemoryStream
        $bmp.Save($stream, [System.Drawing.Imaging.ImageFormat]::Png)
        $data = $stream.ToArray()
    } else {
        $data = ConvertTo-IconDib $bmp
    }
    $bmp.Dispose()
    , $data
}

New-Item -ItemType Directory -Force (Split-Path $OutFile) | Out-Null
$out = New-Object System.IO.BinaryWriter ([System.IO.File]::Create($OutFile))
$out.Write([uint16]0); $out.Write([uint16]1); $out.Write([uint16]$sizes.Count)
$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $dimension = if ($sizes[$i] -ge 256) { 0 } else { $sizes[$i] }
    $out.Write([byte]$dimension); $out.Write([byte]$dimension)
    $out.Write([byte]0); $out.Write([byte]0)
    $out.Write([uint16]1); $out.Write([uint16]32)
    $out.Write([uint32]$pngs[$i].Length); $out.Write([uint32]$offset)
    $offset += $pngs[$i].Length
}
foreach ($png in $pngs) { $out.Write($png) }
$out.Close()
Write-Host "Wrote $OutFile ($((Get-Item $OutFile).Length) bytes, sizes: $($sizes -join ', '))"

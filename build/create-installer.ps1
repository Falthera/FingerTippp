param(
    [Parameter(Mandatory=$true)] [string] $PublishDir,
    [Parameter(Mandatory=$false)] [string] $OutputDir = "artifacts"
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $PublishDir)) {
    Write-Error "Publish directory '$PublishDir' not found."
    exit 1
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

$wxs = Join-Path $PSScriptRoot 'installer.wxs'
$wixobj = Join-Path $PSScriptRoot 'installer.wixobj'
$msi = Join-Path (Resolve-Path $OutputDir).Path 'Fingertippp.msi'

Write-Host "Building MSI from $PublishDir"

$candle = "${env:ProgramFiles}\WiX Toolset v3.11\bin\candle.exe"
$light = "${env:ProgramFiles}\WiX Toolset v3.11\bin\light.exe"

if (-not (Test-Path $candle) -or -not (Test-Path $light)) {
    # try common alternate path
    $candle = "C:\Program Files (x86)\WiX Toolset v3.11\bin\candle.exe"
    $light = "C:\Program Files (x86)\WiX Toolset v3.11\bin\light.exe"
}

if (-not (Test-Path $candle) -or -not (Test-Path $light)) {
    Write-Error "WiX tools (candle.exe/light.exe) not found. Ensure WiX Toolset is installed."
    exit 1
}

& $candle -out $wixobj $wxs
if ($LASTEXITCODE -ne 0) { throw "candle failed" }

& $light -out $msi $wixobj -ext WixUIExtension
if ($LASTEXITCODE -ne 0) { throw "light failed" }

Write-Host "MSI created at: $msi"
Write-Host "Copying EXE publish output to $OutputDir"
Copy-Item -Path (Join-Path $PublishDir '*') -Destination $OutputDir -Recurse -Force

Write-Host "Installer artifacts available in $OutputDir"

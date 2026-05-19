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

function Find-WixTool {
    param([string]$exeName)
    # 1) Check PATH
    $cmd = Get-Command $exeName -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    # 2) Common ProgramFiles locations (try wildcard versions)
    $candidates = @(
        "$env:ProgramFiles\WiX Toolset*\bin\$exeName",
        "C:\Program Files (x86)\WiX Toolset*\bin\$exeName",
        "$env:ProgramFiles\WiX Toolset\bin\$exeName",
        "$env:ProgramFiles(x86)\WiX Toolset\bin\$exeName"
    )
    foreach ($p in $candidates) {
        $found = Get-ChildItem -Path $p -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($found) { return $found.FullName }
    }

    # 3) Chocolatey install locations
    $chocoPaths = @(
        "$env:ProgramData\chocolatey\lib\wixtoolset\tools\*\$exeName",
        "$env:ProgramData\chocolatey\lib\wixtoolset3\tools\*\$exeName",
        "$env:ProgramData\chocolatey\lib\wix\tools\*\$exeName"
    )
    foreach ($p in $chocoPaths) {
        $found = Get-ChildItem -Path $p -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($found) { return $found.FullName }
    }

    return $null
}

$candle = Find-WixTool -exeName 'candle.exe'
$light = Find-WixTool -exeName 'light.exe'

if (-not $candle -or -not $light) {
    Write-Error "WiX tools (candle.exe/light.exe) not found. Ensure WiX Toolset is installed or provide WiX binaries in PATH."
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

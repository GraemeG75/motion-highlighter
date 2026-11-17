<#
.SYNOPSIS
    Builds the Motion Highlighter application and packages it into an installer.
.DESCRIPTION
    Compiles the application in Release mode, publishes the binaries, and generates
    the installer using Inno Setup (ISCC.exe) if available, or creates a deployable package.
#>

param(
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir
$PublishDir = Join-Path $ScriptDir "publish"
$OutputDir = Join-Path $ScriptDir "output"
$IssScript = Join-Path $ScriptDir "MotionHighlighter.iss"

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host " Building Motion Highlighter Installer v$Version " -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# 1. Clean previous build output
if (Test-Path $PublishDir) {
    Remove-Item -Recurse -Force $PublishDir
}
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

# 2. Publish .NET Windows application
Write-Host "`n[1/3] Publishing MotionHighlighter ($Configuration)..." -ForegroundColor Yellow
$ProjectFile = Join-Path $RepoRoot "MotionHighlighter\MotionHighlighter.csproj"
& dotnet publish $ProjectFile -c $Configuration -r win-x64 --self-contained false -p:PublishSingleFile=false -o $PublishDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

# 3. Locate Inno Setup compiler (ISCC.exe)
Write-Host "`n[2/3] Checking for Inno Setup Compiler..." -ForegroundColor Yellow
$IsccPaths = @(
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    "C:\Program Files\Inno Setup 6\ISCC.exe",
    (Get-Command iscc -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source)
)

$Iscc = $IsccPaths | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1

if ($Iscc) {
    Write-Host "Found Inno Setup compiler: $Iscc" -ForegroundColor Green
    Write-Host "`n[3/3] Compiling installer..." -ForegroundColor Yellow
    & $Iscc "/DMyAppVersion=$Version" "/DMyAppSourceDir=$PublishDir" "/O$OutputDir" $IssScript
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Inno Setup compilation failed with exit code $LASTEXITCODE"
        exit $LASTEXITCODE
    }
    $SetupExe = Join-Path $OutputDir "MotionHighlighter-Setup.exe"
    Write-Host "`n✓ Installer created successfully at: $SetupExe" -ForegroundColor Green
} else {
    Write-Host "Inno Setup compiler not found in standard paths." -ForegroundColor Yellow
    Write-Host "Creating deployable archive and setup script..." -ForegroundColor Yellow

    $ZipPath = Join-Path $OutputDir "MotionHighlighter-v$Version-win-x64.zip"
    if (Test-Path $ZipPath) {
        Remove-Item -Force $ZipPath
    }
    Compress-Archive -Path "$PublishDir\*" -DestinationPath $ZipPath -Force

    # Create helper install script
    $InstallScript = @"
# Motion Highlighter Local Installer
`$InstallDir = Join-Path `$env:LOCALAPPDATA "Programs\MotionHighlighter"
Write-Host "Installing Motion Highlighter to `$InstallDir..." -ForegroundColor Cyan
if (-not (Test-Path `$InstallDir)) { New-Item -ItemType Directory -Path `$InstallDir -Force | Out-Null }
Copy-Item -Path "`$PSScriptRoot\publish\*" -Destination `$InstallDir -Recurse -Force
Write-Host "Installation complete! Launching Motion Highlighter..." -ForegroundColor Green
Start-Process (Join-Path `$InstallDir "MotionHighlighter.exe")
"@
    Set-Content -Path (Join-Path $OutputDir "Install-Local.ps1") -Value $InstallScript -Encoding UTF8

    Write-Host "`n✓ Deployable package created at: $ZipPath" -ForegroundColor Green
}

Write-Host "`nBuild process complete!" -ForegroundColor Cyan

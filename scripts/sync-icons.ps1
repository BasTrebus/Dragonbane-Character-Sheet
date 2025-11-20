<#
.SYNOPSIS
    Sync IconKitchen outputs into MAUI platform folders and wwwroot.

.DESCRIPTION
    Copies icon files from the IconKitchen output folder (by default `doc/IconKitchen-Output`)
    into the platform resource folders under `Platforms/` and the web icons into `wwwroot/`.

.PARAMETER Action
    'copy' (default) performs the copy. 'check' verifies the expected files exist.

.PARAMETER Source
    Path to the IconKitchen output folder. Default: 'doc\IconKitchen-Output'

.EXAMPLE
    .\sync-icons.ps1 -Action copy
    Copies icons into platform folders.

.EXAMPLE
    .\sync-icons.ps1 -Action check
    Verifies expected icon files are present in platform folders.
#>

param(
    [ValidateSet('copy','check')]
    [string]$Action = 'copy',
    [string]$Source = 'doc\IconKitchen-Output'
)

Set-StrictMode -Version Latest
Write-Host "Script started. Action='$Action', Source='$Source'"

function Resolve-RepoPath([string]$relative) {
    return Join-Path -Path (Get-Location).Path -ChildPath $relative
}

$srcRoot = Resolve-Path -LiteralPath $Source -ErrorAction SilentlyContinue
if (-not $srcRoot) {
    Write-Error "Source folder '$Source' not found. Please generate IconKitchen output or point -Source to it.";
    exit 2
}
$srcRoot = $srcRoot.ProviderPath

# Targets
$androidSrc = Join-Path $srcRoot 'android\res'
$iosSrc = Join-Path $srcRoot 'ios'
$macSrc = Join-Path $srcRoot 'macos'
$webSrc = Join-Path $srcRoot 'web'

$androidDest = Resolve-RepoPath 'Platforms\Android\Resources'
$iosDest = Resolve-RepoPath 'Platforms\iOS\Assets.xcassets\AppIcon.appiconset'
$macDest = Resolve-RepoPath 'Platforms\MacCatalyst\Assets.xcassets\AppIcon.appiconset'
$webDest = Resolve-RepoPath 'wwwroot'

function Ensure-Dir($path) {
    if (-not (Test-Path $path)) { New-Item -ItemType Directory -Path $path -Force | Out-Null }
}

if ($Action -eq 'copy') {
    Write-Host "Copying icons from: $srcRoot"

    # Android
    if (Test-Path $androidSrc) {
        Ensure-Dir $androidDest
        Write-Host "Copying Android resources from $androidSrc to $androidDest"
        Copy-Item -Path (Join-Path $androidSrc '*') -Destination $androidDest -Recurse -Force
    } else { Write-Warning "Android source not found: $androidSrc" }

    # iOS
    if (Test-Path $iosSrc) {
        Ensure-Dir $iosDest
        Write-Host "Copying iOS appiconset from $iosSrc to $iosDest"
        # IconKitchen iOS outputs are typically in the root of the ios folder
        Copy-Item -Path (Join-Path $iosSrc '*') -Destination $iosDest -Recurse -Force
    } else { Write-Warning "iOS source not found: $iosSrc" }

    # MacCatalyst (AppIcon.icns)
    if (Test-Path $macSrc) {
        Ensure-Dir $macDest
        Write-Host "Copying macOS/macCatalyst resources from $macSrc to $macDest"
        Copy-Item -Path (Join-Path $macSrc '*') -Destination $macDest -Recurse -Force
    } else { Write-Warning "MacCatalyst source not found: $macSrc" }

    # Web icons
    if (Test-Path $webSrc) {
        Ensure-Dir $webDest
        Write-Host "Copying web icons from $webSrc to $webDest"
        Copy-Item -Path (Join-Path $webSrc '*') -Destination $webDest -Recurse -Force
    } else { Write-Warning "Web source not found: $webSrc" }

    Write-Host 'Copy completed.'
    exit 0
}

if ($Action -eq 'check') {
    $errors = @()
    # Android expected file
    $androidCheck = Join-Path $androidDest 'mipmap-xxxhdpi\ic_launcher.png'
    if (-not (Test-Path $androidCheck)) { $errors += "Missing Android icon: $androidCheck" }

    # iOS expected files
    $iosCheck = Join-Path $iosDest 'Contents.json'
    if (-not (Test-Path $iosCheck)) { $errors += "Missing iOS Contents.json in appiconset: $iosCheck" }

    # MacCatalyst expected file
    $macCheck = Join-Path $macDest 'AppIcon.icns'
    if (-not (Test-Path $macCheck)) { $errors += "Missing macOS AppIcon.icns: $macCheck" }

    # Web expected file
    $webCheck = Join-Path $webDest 'icon-192.png'
    if (-not (Test-Path $webCheck)) { $errors += "Missing web icon: $webCheck" }

    if ($errors.Count -gt 0) {
        Write-Error "Icon check failed:`n$(($errors -join "`n"))"
        exit 1
    } else {
        Write-Host 'All expected icon files are present.'
        exit 0
    }
}

Write-Warning "Unknown action: $Action"
exit 3

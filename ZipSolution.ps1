# ZipSolution.ps1
# Scans the current directory tree for relevant solution/project/source files
# and creates a zip named after the solution file.
# Overwrites any existing zip with the same name.

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

# Find the solution file (*.sln or *.slnx), preferring *.slnx if present
$solutionFile = Get-ChildItem -Path $root -File -Filter *.sln* |
    Sort-Object @{
        Expression = {
            switch ($_.Extension.ToLower()) {
                ".slnx" { 0 }
                ".sln"  { 1 }
                default { 2 }
            }
        }
    }, Name |
    Select-Object -First 1

if (-not $solutionFile) {
    throw "No .slnx or .sln file found in $root"
}

$zipPath = Join-Path $root ($solutionFile.BaseName + ".zip")

# Remove old zip if it exists
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

# File patterns worth including
$includePatterns = @(
    "*.sln",
    "*.slnx",
    "*.cs",
    "*.csproj",
    "*.props",
    "*.targets",
    "*.config",
    "*.json",
    "*.xml",
    "*.xaml",
    "*.resx",
    "*.txt",
    "*.md",
    "*.editorconfig",
    "*.ruleset",
    "*.runsettings",
    "*.yaml",
    "*.yml"
)

# Directory names to skip
$excludeDirs = @(
    ".git",
    ".vs",
    "bin",
    "obj",
    "packages",
    "TestResults",
    "node_modules"
)

function IsExcludedPath {
    param([string]$FullName)

    foreach ($dir in $excludeDirs) {
        if ($FullName -match [regex]::Escape("\$dir\")) {
            return $true
        }
    }
    return $false
}

# Collect matching files while excluding junk folders
$files = foreach ($pattern in $includePatterns) {
    Get-ChildItem -Path $root -Recurse -File -Filter $pattern |
        Where-Object { -not (IsExcludedPath $_.FullName) }
}

# De-duplicate in case files match multiple patterns
$files = $files | Sort-Object FullName -Unique

if (-not $files -or $files.Count -eq 0) {
    throw "No matching files found to zip."
}

# Compress using relative paths
$stagingDir = Join-Path $root "__zip_staging__"
if (Test-Path $stagingDir) {
    Remove-Item $stagingDir -Recurse -Force
}
New-Item -ItemType Directory -Path $stagingDir | Out-Null

try {
    foreach ($file in $files) {
        $relativePath = $file.FullName.Substring($root.Length).TrimStart('\')
        $destPath = Join-Path $stagingDir $relativePath
        $destDir = Split-Path $destPath -Parent

        if (-not (Test-Path $destDir)) {
            New-Item -ItemType Directory -Path $destDir -Force | Out-Null
        }

        Copy-Item $file.FullName $destPath
    }

    Compress-Archive -Path (Join-Path $stagingDir "*") -DestinationPath $zipPath -Force
    Write-Host "Created zip: $zipPath"
}
finally {
    if (Test-Path $stagingDir) {
        Remove-Item $stagingDir -Recurse -Force
    }
}
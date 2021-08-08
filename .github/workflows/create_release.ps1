Param(
    [Parameter(Mandatory=$true)]
    [string] $ParentDirectory,
    [Parameter(Mandatory=$true)]
    [string] $BuildDirectory,
    [string] $version = "1.0.0"
)

Clear-Host

# ensure we dont have trailing back slashes, so we dont double up
$ParentDirectory = $ParentDirectory.TrimEnd("\\")
$BuildDirectory = $BuildDirectory.TrimEnd("\\")

$temp = $env:TEMP
$rulesDir = "$temp\SqlServer.Rules"
$releaseDir = [System.IO.Path]::Combine($temp, "release")
$docsDir = [System.IO.Path]::Combine($ParentDirectory, "docs")

if (!(Test-Path $rulesDir)) {
    Write-Host "Creating $rulesDir"
    New-Item $rulesDir -ItemType Directory | Out-Null
}

if (!(Test-Path $releaseDir)) {
    Write-Host "Creating $releaseDir"
    New-Item $releaseDir -ItemType Directory | Out-Null
}

Copy-Item -path "$BuildDirectory\*.*" -Destination $rulesDir

$releaseName = "release.zip" # removing version until I can figure out how to get the version tags from master
$compress = @{
  Path = $docsDir, $rulesDir, "$ParentDirectory\README.md"
  CompressionLevel = "Fastest"
  DestinationPath = "$releaseDir\$releaseName"
}

Compress-Archive @compress -Force -Verbose 

Write-Host "Release written to: " -NoNewline
Write-Host "$releaseDir\$releaseName" -ForegroundColor Yellow

echo "GITHUB_RELEASE_PATH=$releaseDir\$releaseName" | Out-File -FilePath $env:GITHUB_ENV -Encoding utf8 -Append

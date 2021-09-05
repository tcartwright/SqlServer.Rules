﻿Param(
    [Parameter(Mandatory=$true)]
    [string] 
    $ParentDirectory,
    [Parameter(Mandatory=$true)]
    [string] 
    $BuildDirectory,
    [Parameter(Mandatory=$true)]
    [string]
    $releasePath
)

Clear-Host

# ensure we dont have trailing back slashes, so we dont double up
$ParentDirectory = $ParentDirectory.TrimEnd("\\")
$BuildDirectory = $BuildDirectory.TrimEnd("\\")

$temp = $env:TEMP
$rulesDir = "$temp\SqlServer.Rules"

$docsDir = [System.IO.Path]::Combine($ParentDirectory, "docs")

if (!(Test-Path $rulesDir)) {
    Write-Host "Creating $rulesDir"
    New-Item $rulesDir -ItemType Directory | Out-Null
}


Copy-Item -path "$BuildDirectory\*.*" -Destination $rulesDir

$compress = @{
  Path = $docsDir, $rulesDir, "$ParentDirectory\README.md"
  CompressionLevel = "Fastest"
  DestinationPath = $releasePath
}

Compress-Archive @compress -Force -Verbose 

Write-Host "Release written to: $releasePath" 
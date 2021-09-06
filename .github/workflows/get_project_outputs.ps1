Param(
    [Parameter(Mandatory=$true, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)]
    [string] $ProjectPath, 
    [string] $Configuration = "Release",
    [string] $Platform = "AnyCPU",
    [string] $TargetFramework = "^net.*"
)
Clear-Host

[xml]$csproj = (Get-Content -Path $ProjectPath -Raw) 

$framework = $csproj.Project.PropertyGroup.TargetFramework -split ";|," | Where-Object { $_ -match $TargetFramework } | Select-Object -First 1

if ([string]::IsNullOrWhiteSpace($framework)) {
    throw "Invalid framework"
    exit 1
}

$releaseConfig = $csproj.Project.PropertyGroup | Where-Object { $_.Condition -ieq  " '`$(Configuration)|`$(Platform)' == '$Configuration|$Platform' " }

if ($releaseConfig) {
    $outputPath = $releaseConfig.OutputPath
} else {
    # attempt 2, maybe they moved the a global PropertyGroup
    $outputPath = $csproj.Project.PropertyGroup.OutputPath
}

if ([string]::IsNullOrWhiteSpace($outputPath)) {
    throw "Could not determine the outputpath."
    exit 1
}

$assemblyName = $csproj.Project.PropertyGroup.AssemblyName | Select-Object -First 1

if ([string]::IsNullOrWhiteSpace($assemblyName)) {
    # if not set, assume the project name
    $assemblyName = [System.IO.Path]::GetFileNameWithoutExtension($ProjectPath) 
}

$outputPath = [System.IO.Path]::Combine([System.IO.Path]::GetDirectoryName($ProjectPath), $outputPath, $framework)
$outputDll = [System.IO.Path]::Combine($outputPath, "$assemblyName.dll")

echo "BUILD_OUTPUT_PATH=$outputPath" | Out-File -FilePath $env:GITHUB_ENV -Encoding utf8 -Append
echo "BUILD_OUTPUT_DLL=$outputDll" | Out-File -FilePath $env:GITHUB_ENV -Encoding utf8 -Append


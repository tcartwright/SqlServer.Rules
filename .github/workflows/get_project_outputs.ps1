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
$assemblyName = $csproj.Project.PropertyGroup.AssemblyName | Select-Object -First 1

if ([string]::IsNullOrWhiteSpace($assemblyName)) {
    # if not set, assume the project name
    $assemblyName = [System.IO.Path]::GetFileNameWithoutExtension($ProjectPath) 
}

$outputPath = [System.IO.Path]::Combine([System.IO.Path]::GetDirectoryName($ProjectPath), $releaseConfig.OutputPath, $framework)
$outputDll = [System.IO.Path]::Combine($outputPath, "$assemblyName.dll")

echo "GITHUB_BUILD_OUTPUT_PATH=$outputPath" | Out-File -FilePath $env:GITHUB_ENV -Encoding utf8 -Append
echo "GITHUB_BUILD_OUTPUT_DLL=$outputDll" | Out-File -FilePath $env:GITHUB_ENV -Encoding utf8 -Append


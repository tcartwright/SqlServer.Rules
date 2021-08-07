[CmdletBinding()]
param (
    [Parameter(Mandatory=$true, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)]
    [string]
    $solutionPath,
    [Parameter(ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)]
    [ValidateSet("Debug", "Release")]
    [string]
    $configuration = "Release"

)

Clear-Host

# TIM C: I extrapolated how to enter the Develeloper Shell from the shortcut that installs with VS 2019 called: "Developer PowerShell for VS 2019". 
# I added the take on VSWHERE so it will work for any version 2017+

#. "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" -?

$instance = (. "$(${env:ProgramFiles(x86)})\Microsoft Visual Studio\Installer\vswhere.exe" -latest -format json | ConvertFrom-Json)

$common7Path = ([System.IO.FileInfo]$instance.productPath).Directory.Parent.FullName
$instanceId = $instance.instanceId

Import-Module "$common7Path\Tools\Microsoft.VisualStudio.DevShell.dll"; 
Enter-VsDevShell $instanceId


Write-Host "Building $solutionPath" -ForegroundColor Yellow
. msbuild $solutionPath /target:ReBuild /interactive:false /nologo /p:platform="any cpu" /p:configuration="$configuration" /clp:Summary /warnAsError:NU1605,CS1570 | Write-Host


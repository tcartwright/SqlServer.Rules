#Requires -RunAsAdministrator

# backup in case the requires is ignored by older PS versions.
$principal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (!($principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator))) {
    throw "Administrator rights required"
    exit 1;
}

Clear-Host

$scriptRoot = [System.IO.Directory]::GetParent($MyInvocation.MyCommand.Definition).FullName

$buildPath = "$scriptRoot\..\..\SqlServer.Rules\bin\Release\net472\"
$dllPath = "$($buildPath)SqlServer.Rules.dll"

if (!(Test-Path -Path $dllPath)) {
    . "$scriptRoot\BuildSolution.ps1" -solutionPath "$scriptRoot\..\..\SqlServer.Rules.sln"
    if ($LASTEXITCODE -ne 0) {
        throw "Error building solution"; 
        exit 1;
    }
    # test the path once more, just in case someone changed the output path or dll name
    if (!(Test-Path -Path $dllPath)) {
        throw "Could not find SqlServer.Rules.dll even after building. Check the output path and file name are correct."
        exit 1;
    }
}

$instances = (. "$(${env:ProgramFiles(x86)})\Microsoft Visual Studio\Installer\vswhere.exe" -format json | ConvertFrom-Json)

foreach ($instance in $instances) {
    Write-Host "Deploying rules to: " -NoNewline
    Write-Host "$($instance.displayName)" -ForegroundColor Green
    $path = "$($instance.installationPath)\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\"

    $extensionDirs = (Get-ChildItem -Path $path -Directory).FullName
    foreach ($extensionDir in $extensionDirs) {
        $extensionPath = "$extensionDir\Extensions\SqlServer.Rules"
        Write-Host "Pushing rules to: " -NoNewline
        Write-Host $extensionPath -ForegroundColor Yellow

        New-Item -Path $extensionPath -ItemType Directory -ErrorAction SilentlyContinue | Out-Null
        
        #ensure we strip of any trailing back slashes as that will arbitrarily escape one of the wrapper quotes, breaking the command 
        robocopy """$($buildPath.TrimEnd("\\"))"""  """$($extensionPath.TrimEnd("\\"))""" /s /mir /nc /nfl /ns /ndl /np /njh
    }
}

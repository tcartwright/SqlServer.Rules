#Requires -RunAsAdministrator

# backup in case the requires is ignored by older PS versions.
$principal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (!($principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator))) {
    throw "Administrator rights required"
    exit 1;
}

Clear-Host

$scriptRoot = [System.IO.Directory]::GetParent($MyInvocation.MyCommand.Definition).FullName

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
        robocopy """$scriptRoot"""  """$($extensionPath.TrimEnd("\\"))""" /s /mir /nc /nfl /ns /ndl /np /njh
    }
}

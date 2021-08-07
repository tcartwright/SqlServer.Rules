[CmdletBinding()]
param (
    [Parameter(Mandatory=$true, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)]
    [string]
    $path
)

#stub for generate docs

$docs = [System.IO.Path]::Combine($path, "docs")

if (!(Test-Path -Path $docs -PathType Container)) {
    New-Item $docs -ItemType Directory 
}

"#Hello World! The time is now: $(Get-Date -Format "h:mm:ss tt")" | Out-File -FilePath "$($docs)README.MD" -Force -Encoding ascii 

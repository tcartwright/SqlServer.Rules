[CmdletBinding()]
param (
    [Parameter(Mandatory=$true, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)]
    [string]
    $path
)

#stub for generate docs

$docs = [System.IO.Path]::Combine($path, "docs")

if (!(Test-Path -Path $docs -PathType Container)) {
    New-Item $docs -ItemType Directory | out-null
}

#Write dummy file to docs folder
"#Hello World! The time is now: $(Get-Date -Format "h:mm:ss tt")" | Out-File -FilePath "$($docs)\README.MD" -Force -Encoding ascii 

Get-Content -Path "$($docs)\README.MD" -Raw


Add-Type -Path "$path\SqlServer.Rules\bin\Debug\net472\SqlServer.Rules.dll" -IgnoreWarnings -ErrorAction SilentlyContinue 


$assembly = ([appdomain]::CurrentDomain.GetAssemblies())|?{$_.Modules.name.contains("SqlServer.Rules.dll")}
$types = $assembly.GetTypes() | Where-Object { $_.GetCustomAttributes([Microsoft.SqlServer.Dac.CodeAnalysis.ExportCodeAnalysisRuleAttribute], $false) -ne $null -and $_.IsPublic }

if (!$types -or $types.Count -eq 0) {
    throw "No types found that match the criteria"
    exit
}

$assemblyPath = ([uri]$assembly.CodeBase).AbsolutePath
$xmlPath = [System.IO.Path]::Combine(  
    [System.IO.Path]::GetDirectoryName($assemblyPath),
    "$([System.IO.Path]::GetFileNameWithoutExtension($assemblyPath)).xml"
)

[xml]$xml = Get-Content -Path $xmlPath -Raw


foreach ($type in $types) {
    $ctor = $type.GetConstructor([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance, $null, [System.Type]::EmptyTypes, $null);
    $ruleObj = $ctor.Invoke($null);
    $types = $ruleObj.SupportedElementTypes | Select-Object -ExpandProperty Name

    
    $ruleAttr = $type.GetCustomAttributes([Microsoft.SqlServer.Dac.CodeAnalysis.ExportCodeAnalysisRuleAttribute], $false)
    $typeXml = ($xml.doc.members.member | Where-Object { $_.name -eq "T:$($type.FullName)" })
    if ($typeXml) {
        $typeXml.friendlyName #custom element I added
        $typeXml.summary
        $typeXml.example

    }
    $type.Name
    $ruleAttr.Id
    $ruleAttr.DisplayName
    $ruleAttr.Description
    $ruleAttr.RuleScope
    $ruleAttr.Category
    Write-Host "Applies to:" -ForegroundColor Yellow
    $types
    ""
}

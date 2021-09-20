Param(
    [Parameter(Mandatory=$true)]
    [string] $ParentDirectory,

    [Parameter(Mandatory=$true)]
    [string] $RulesDllPath
)
Clear-Host
Set-StrictMode -Version 3.0

function Remove-LeadingWhitespace {
    Param(
        [string] $Text
    )
    # speed this up. no need to split if essentially completely empty, just return empty
    if ([string]::IsNullOrWhiteSpace($Text)) {
        return [string]::Empty;
    }

    $Lines = $Text.Split([Environment]::NewLine)
    $Result = ""
    $LeadingWhitespace = 0

    if ($Lines.Count -gt 0) {
        $FoundLeadingWhitespace = $false

        foreach ($Line in $Lines) {
            if (-not $FoundLeadingWhitespace -and -not [String]::IsNullOrWhiteSpace($Line)) {
                $LeadingWhitespace = $Line.Length - $Line.TrimStart().Length
                $FoundLeadingWhitespace = $true
            }

            if ($Line.Length -gt $LeadingWhitespace) {
                $Result += "$($Line.Substring($LeadingWhitespace))`r`n"
            }
            else {
                $Result += "$Line`r`n"
            }
        }
    }

    return $Result.TrimEnd()
}

function Get-SplitPascalCase {
    Param(
        $PascalText
    )
    $PascalCaseRegEx = "(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])"

    return [Regex]::Replace($PascalText, $PascalCaseRegEx, " ")
}

function Get-SqlRulesAssembly {
    Param(
        [System.IO.FileInfo] $AssemblyFileInfo
    )

    $Assembly = [System.AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.Modules.Name -imatch $AssemblyFileInfo.Name }

    if ($null -eq $Assembly) {
        Add-Type -Path $AssemblyFileInfo.FullName
    
        $Assembly = [System.AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.Modules.Name -imatch $AssemblyFileInfo.Name }
    }

    return $Assembly
}

function Get-SqlRulesTypes {
    Param(
        [System.Reflection.Assembly]$Assembly
    )
    
    $Types = $Assembly.GetTypes() | Where-Object {
        $_.GetCustomAttributes([Microsoft.SqlServer.Dac.CodeAnalysis.ExportCodeAnalysisRuleAttribute], $false) -ne $null `
            -and $_.IsPublic
    }
    
    if ($null -eq $Types -or $Types.Count -eq 0) {
        throw "No types found that match the criteria"
        exit 1
    }

    return $Types
}

function Get-XmlDocumentForRules {
    Param(
        [System.IO.FileInfo]$AssemblyFileInfo
    )

    $XmlPath = [System.IO.Path]::Combine($AssemblyFileInfo.DirectoryName, "$($AssemblyFileInfo.BaseName).xml")
    
    if (-not (Test-Path -LiteralPath $XmlPath)) {
        throw "Unable to find XML file at '$XmlPath'"
        exit 1
    }
    
    Write-Information "Loading rule xml: $XmlPath"
    [xml]$Xml = Get-Content -Path $XmlPath -Raw

    return $Xml
}

function Get-SqlRules {
    Param(
        [xml]$Xml,
        [Type[]]$Types
    )

    $Rules = New-Object System.Collections.ArrayList

    foreach ($Type in $Types) {
        Write-Information "Getting rule: $($Type.Name)"
        $RuleObject = [PSCustomObject] @{
            Category              = ""
            ClassName             = ""
            NameSpace             = ""
            FriendlyName          = ""
            RuleId                = ""
            RuleScope             = ""
            Summary               = ""
            Description           = ""
            Example               = ""
            IsIgnorable           = $false
            SupportedElementTypes = @()
        }
    
        $RuleAttr = $Type.GetCustomAttributes([Microsoft.SqlServer.Dac.CodeAnalysis.ExportCodeAnalysisRuleAttribute], $false)
    
        $TypeXml = ($Xml.doc.members.member | Where-Object { $_.name -ieq "T:$($Type.FullName)" })
    
        if ($TypeXml) {
            $RuleObject.FriendlyName = ($TypeXml.FriendlyName).Trim()
            $RuleObject.Summary = (Remove-LeadingWhitespace -Text $TypeXml.summary).Trim()
            $RuleObject.Example = (Remove-LeadingWhitespace -Text $TypeXml.ExampleMd).Trim()
            $RuleObject.IsIgnorable = $TypeXml.IsIgnorable
        }
        else {
            # TODO: Handle the case where the XML doesn't exist for a rule.
        }
    
        if ([String]::IsNullOrWhiteSpace($RuleObject.FriendlyName)) {
            $RuleObject.FriendlyName = Get-SplitPascalCase -PascalText $Type.Name
        }
        
        $RuleObject.NameSpace = $Type.Namespace
        $RuleObject.ClassName = $Type.Name
        $RuleObject.RuleId = ($RuleAttr.Id).Split('.')[1]
        $RuleObject.Description = $RuleAttr.Description
        $RuleObject.RuleScope = $RuleAttr.RuleScope
        $RuleObject.Category = $RuleAttr.Category
    
        if ($RuleObject.RuleScope -ieq "Element") {
            $Constructor = $Type.GetConstructor([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance, `
                    $null, `
                    [System.Type]::EmptyTypes, `
                    $null)
            $RuleInstance = $Constructor.Invoke($null)
            
            $elementTypes = New-Object System.Collections.ArrayList
            foreach ($ApplicableType in $RuleInstance.SupportedElementTypes) {
                $elementTypes.Add((Get-SplitPascalCase -PascalText $ApplicableType.Name)) | Out-Null
            }
            $RuleObject.SupportedElementTypes = ($elementTypes.ToArray() | Sort-Object)
        }
        else {
            $RuleObject.SupportedElementTypes += "Model"
        }
    
        $Rules.Add($RuleObject) | Out-Null
    }

    return $Rules.ToArray()
}

function New-TableOfContents {
    Param(
        [string]$ParentDirectory,
        [PSCustomObject[]]$RuleObjects
    )

    $spaces = " " * 2

    $categories = $RuleObjects | Group-Object { $_.Category } | Sort-Object -Property Key

    $StringBuilder = [System.Text.StringBuilder]::new()
    [void]$StringBuilder.AppendLine("[This document is automatically generated. All changed made to it WILL be lost]: <>$spaces")

    [void]$StringBuilder.AppendLine("$spaces")
    [void]$StringBuilder.AppendLine("# Table of Contents$spaces")
    [void]$StringBuilder.AppendLine("$spaces")

    foreach ($category in $categories) {
        [void]$StringBuilder.AppendLine("$spaces")
        [void]$StringBuilder.AppendLine("## $($category.Name)$spaces")
        [void]$StringBuilder.AppendLine("$spaces")

        [void]$StringBuilder.AppendLine("| Rule Id | Friendly Name | Ignorable | Description | Example? |")
        [void]$StringBuilder.AppendLine("|----|----|----|----|----|")
        foreach ($rule in ($category.Group | Sort-Object -Property RuleId )) {
            $ruleId = "[$($Rule.RuleId)]($($category.Name)/$($Rule.RuleId).md)"
            [void]$StringBuilder.AppendLine("| $ruleId | $($Rule.FriendlyName -replace "\|", "&#124;") | $(if ($Rule.IsIgnorable) { "Yes" } else { " " }) | $($Rule.Description -replace "\|", "&#124;") | $(if ([string]::IsNullOrWhiteSpace($Rule.Example)) { " " } else { "Yes" }) |")
        }
    }

    $FilePath = "$ParentDirectory\docs\"
    New-Item -Path $FilePath -Force -ItemType Directory | Out-Null

    $StringBuilder.ToString() | Out-File "$FilePath\table_of_contents.md" -Force -Encoding ascii
}

function New-MdFromRuleObject {
    Param(
        [string] $ParentDirectory,
        [object] $RuleObject,
        [string] $AssemblyName
    )

    $StringBuilder = [System.Text.StringBuilder]::new()

    # MD EOL - When you do want to insert a <br /> break tag using Markdown, you end a line with two or more spaces, then type return.
    $spaces = " " * 2
    [void]$StringBuilder.AppendLine("[This document is automatically generated. All changed made to it WILL be lost]: <>$spaces")

    [void]$StringBuilder.AppendLine("$spaces")
    [void]$StringBuilder.AppendLine("# SQL Server Rule: $($RuleObject.RuleId)$spaces")
    [void]$StringBuilder.AppendLine("$spaces")
    [void]$StringBuilder.AppendLine("|    |    |")
    [void]$StringBuilder.AppendLine("|----|----|")
    [void]$StringBuilder.AppendLine("| Assembly | $AssemblyName$spaces |")
    [void]$StringBuilder.AppendLine("| Namespace | $($RuleObject.Namespace) |")
    [void]$StringBuilder.AppendLine("| Class | $($RuleObject.ClassName) |")

    [void]$StringBuilder.AppendLine("$spaces")
    [void]$StringBuilder.AppendLine("## Rule Information$spaces")
    [void]$StringBuilder.AppendLine("$spaces")

    [void]$StringBuilder.AppendLine("|    |    |")
    [void]$StringBuilder.AppendLine("|----|----|")
    [void]$StringBuilder.AppendLine("| Id | $($RuleObject.RuleId) |")
    [void]$StringBuilder.AppendLine("| Friendly Name | $($RuleObject.FriendlyName) |")
    [void]$StringBuilder.AppendLine("| Category | $($RuleObject.Category) |")
    [void]$StringBuilder.AppendLine("| Ignorable | $($RuleObject.IsIgnorable) |")
    [void]$StringBuilder.AppendLine("| Applicable Types | $($RuleObject.SupportedElementTypes | Select-Object -First 1)  |")
    $ruleElementTypes = $RuleObject.SupportedElementTypes | Select-Object -Skip 1
    if ($ruleElementTypes) {
        [void]$StringBuilder.AppendLine("|   | $($ruleElementTypes -join " |`r`n|   | ") |") 
    }

    [void]$StringBuilder.AppendLine("$spaces")
    [void]$StringBuilder.AppendLine("## Description$spaces")
    [void]$StringBuilder.AppendLine("$spaces")
    [void]$StringBuilder.AppendLine("$($RuleObject.Description)$spaces")

    [void]$StringBuilder.AppendLine("$spaces")
    [void]$StringBuilder.AppendLine("## Summary$spaces")
    [void]$StringBuilder.AppendLine("$spaces")
    [void]$StringBuilder.AppendLine("$($RuleObject.Summary)$spaces")

    if (-not [String]::IsNullOrWhiteSpace("$($RuleObject.Example)$spaces")) {
        [void]$StringBuilder.AppendLine("$spaces")
        [void]$StringBuilder.AppendLine("### Examples$spaces")
        [void]$StringBuilder.AppendLine("$spaces")
        [void]$StringBuilder.Append("$($RuleObject.Example)$spaces")
    }

    [void]$StringBuilder.AppendLine("")

    $FilePath = "$ParentDirectory\docs\$($RuleObject.Category)\"
    New-Item -Path $FilePath -Force -ItemType Directory | Out-Null

    $StringBuilder.ToString() | Out-File "$FilePath\$($RuleObject.RuleId).md" -Force -Encoding ascii
}

function Publish-Markdown {
    Param(
        [string] $ParentDirectory,
        [string] $AssemblyPath
    )

    $assemblyFI = New-Object System.IO.FileInfo ($AssemblyPath)

    if (-not $assemblyFI.Exists) {
        throw "The path to the assembly for the rules does not exist."
    }

    [System.Reflection.Assembly] $Assembly = Get-SqlRulesAssembly -AssemblyFileInfo $assemblyFI
    [Type[]] $Types = Get-SqlRulesTypes -Assembly $Assembly
    [xml] $Xml = Get-XmlDocumentForRules -AssemblyFileInfo $assemblyFI
    $SqlRules = Get-SqlRules -Xml $Xml -Types $Types

    foreach ($SqlRule in $SqlRules) {
        New-MdFromRuleObject -ParentDirectory $ParentDirectory -RuleObject $SqlRule -AssemblyName $assemblyFI.Name
    }
    
    New-TableOfContents -ParentDirectory $ParentDirectory -RuleObjects $SqlRules
}


Publish-Markdown -ParentDirectory $ParentDirectory -AssemblyPath $RulesDllPath

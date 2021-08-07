Param(
    [string] $BuildDirectory,
    [string] $ParentDirectory
)

function Remove-LeadingWhitespace {
    Param(
        [string] $Text
    )

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
        [string] $BuildDirectory
    )
    $Assembly = [System.AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.Modules.Name.Contains("SqlServer.Rules.dll") }

    if ($null -eq $Assembly) {
        Add-Type -Path "$BuildDirectory\SqlServer.Rules.dll"
    
        $Assembly = [System.AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.Modules.Name.Contains("SqlServer.Rules.dll") }
    }

    return $Assembly
}

function Get-SqlRulesTypes {
    Param(
        $Assembly
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
        $Assembly
    )

    $AssemblyPath = ([uri] $Assembly.CodeBase).AbsolutePath

    $XmlPath = [System.IO.Path]::Combine(
        [System.IO.Path]::GetDirectoryName($AssemblyPath), 
        "$([System.IO.Path]::GetFileNameWithoutExtension($AssemblyPath)).xml")
    
    if (-not (Test-Path -LiteralPath $XmlPath)) {
        throw "Unable to find XML file at '$XmlPath'"
        exit 1
    }
    
    [xml]$Xml = Get-Content -Path $XmlPath -Raw

    return $Xml
}

function Get-SqlRules {
    Param(
        $Xml,
        $Types
    )

    $Rules = @()

    foreach ($Type in $Types) {
        $RuleObject = [PSCustomObject] @{
            Category              = ""
            ClassName             = ""
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
            $RuleObject.FriendlyName = $TypeXml.FriendlyName
            $RuleObject.Summary = Remove-LeadingWhitespace -Text $TypeXml.summary
            $RuleObject.Example = Remove-LeadingWhitespace -Text $TypeXml.ExampleMd
            $RuleObject.IsIgnorable = $TypeXml.IsIgnorable
        }
        else {
            # TODO: Handle the case where the XML doesn't exist for a rule.
        }
    
        if ([String]::IsNullOrWhiteSpace($RuleObject.FriendlyName)) {
            $RuleObject.FriendlyName = Get-SplitPascalCase -PascalText $Type.Name
        }
    
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
            
            foreach ($ApplicableType in $RuleInstance.SupportedElementTypes) {
                $RuleObject.SupportedElementTypes += Get-SplitPascalCase -PascalText $ApplicableType.Name
            }
        }
        else {
            $RuleObject.SupportedElementTypes += "Model"
        }
    
        $Rules += $RuleObject
    }

    return $Rules
}

function New-TableOfContents {
    Param(
        $ParentDirectory,
        $RuleObjects
    )

    $TableOfContents = @{}

    foreach ($Rule in $RuleObjects) {
        if ($TableOfContents.ContainsKey($Rule.Category)) {
            $TableOfContents[$Rule.Category] += $Rule
        }
        else {
            $TableOfContents.Add($Rule.Category, @($Rule))
        }
    }

    $OrderedCategories = $TableOfContents.Keys | Sort-Object

    $StringBuilder = [System.Text.StringBuilder]::new()
    [void]$StringBuilder.AppendLine("# Table of Contents")

    foreach ($Category in $OrderedCategories) {
        [void]$StringBuilder.AppendLine("## $Category")

        for ($i = 0; $i -lt $TableOfContents[$Category].Count; $i += 1) {
            $Rule = $TableOfContents[$Category][$i]

            [void]$StringBuilder.AppendLine("$($i + 1). [$($Rule.FriendlyName)]($Category/$($Rule.RuleId).md)")
        }
    }

    $FilePath = "$ParentDirectory\docs\"
    New-Item -Path $FilePath -Force -ItemType Directory | Out-Null

    $StringBuilder.ToString() | Out-File "$FilePath\TableOfContents.md" -Force
}

function New-MdFromRuleObject {
    Param(
        [string] $ParentDirectory,
        $RuleObject
    )

    $StringBuilder = [System.Text.StringBuilder]::new()

    [void]$StringBuilder.AppendLine("[This document is automatically generated. All changed made to it WILL be lost]: <>")
    [void]$StringBuilder.AppendLine("# $($RuleObject.FriendlyName)")
    [void]$StringBuilder.AppendLine("Namespace: SqlServer.Rules.$($RuleObject.Category)  ")
    [void]$StringBuilder.AppendLine("Assembly: SqlServer.Rules.dll  ")
    [void]$StringBuilder.AppendLine("Ignorable: $($RuleObject.IsIgnorable)  ")

    [void]$StringBuilder.AppendLine("Applicable Types: $($RuleObject.SupportedElementTypes -join ', ')  ")
    [void]$StringBuilder.AppendLine("")

    [void]$StringBuilder.AppendLine($RuleObject.Description)

    [void]$StringBuilder.AppendLine("## Summary")
    [void]$StringBuilder.AppendLine($RuleObject.Summary)

    if (-not [String]::IsNullOrWhiteSpace($RuleObject.Example)) {
        [void]$StringBuilder.AppendLine("## Examples")
        [void]$StringBuilder.Append($RuleObject.Example)
    }

    $FilePath = "$ParentDirectory\docs\$($RuleObject.Category)\"
    New-Item -Path $FilePath -Force -ItemType Directory | Out-Null

    $StringBuilder.ToString() | Out-File "$FilePath\$($RuleObject.RuleId).md" -Force
}

function Publish-Markdown {
    Param(
        [string] $BuildDirectory,
        [string] $ParentDirectory
    )

    $Assembly = Get-SqlRulesAssembly -BuildDirectory $BuildDirectory
    $Types = Get-SqlRulesTypes -Assembly $Assembly
    $Xml = Get-XmlDocumentForRules -Assembly $Assembly
    $Rules = Get-SqlRules -Xml $Xml -Types $Types

    foreach ($Rule in $Rules) {
        New-MdFromRuleObject -ParentDirectory $ParentDirectory -RuleObject $Rule
    }
    
    New-TableOfContents -ParentDirectory $ParentDirectory -RuleObjects $Rules
}

Publish-Markdown -BuildDirectory $BuildDirectory -ParentDirectory $ParentDirectory

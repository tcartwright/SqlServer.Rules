# Yet Another Ruleset for SQL Server DataTools

## Overview

Just what it says on the box: A library of SQL best practices as extended [database code analysis rules](https://docs.microsoft.com/en-us/sql/ssdt/overview-of-extensibility-for-database-code-analysis-rules?view=sql-server-ver15) checked at build.

For example code see [here](https://github.com/microsoft/DACExtensions/tree/master/RuleSamples)

## Organization

- SqlServer.Dac - This hold visitors and other utility code
- SqlServer.Rules - This holds the rules derived from `SqlCodeAnalysisRule`
- SqlServer.Rules.Report - Library for evaluating a rule and serializing the result.
- SqlServer.Rules.Generator - a quick console app to report on all rules in a Sql Project.
- SqlServer.Rules.SolutionGenerator - a quick to do a build and evaluate the rules on a Sql Solution
- TestDatabase - a small Sql Solution to test with

## Debug / Test

1) Ensure `SqlServer.Rules.SolutionGenerator` is "Set as Startup Project"
1) Set Command line arguments:
    1) Open up the properties for the `SqlServer.Rules.SolutionGenerator` project
    1) Select the Debug tab
    1) Enter one of the following scenarios:
      1) To debug the test harness db: ```--build --reportDirectory ".\Files" --solution "..\..\..\TestDatabase\TestDatabase.sln"```
      1) To debug a Sql project: ```--build --reportDirectory ".\Files" --solution "{path}.sln"```
1) Add a break point in the `SqlServer.Rules` project in any of the rules you wish to debug

Note: if you need to debug the loading of the library see [here](/SqlServer.Rules/README.md)

## Install

Follow the instructions on MS Docs [Install Static Code Analysis Rule](https://docs.microsoft.com/en-us/sql/ssdt/walkthrough-author-custom-static-code-analysis-rule-assembly?view=sql-server-ver15#install-a-static-code-analysis-rule).

The build should be installed to the template path
`%VSBIN%\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\{SqlEngine}\Extensions`
So for Visual Studio 2017 with DAC version 150 the path might be
> `C:\Program Files (x86)\Microsoft Visual Studio\`***`2017`***`\Enterprise\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\`***`150`***`\Extensions\SqlServer.Rules`

**NOTES:**

- You will need to copy the binaries to every permutaion of Visual Studio version and dac version that you wish to use the rules for.  
- When you have code analysis enabled and have compiled the project Visual Studio places a hard lock on the rule binaries. To updates them or remove them you will need to close Visual Studio.

## Project Configuration

- Once the rules are compiled and installed to the appropriate directory then you can open up your SSDT project and enable code analysis by following these instructions: https://docs.microsoft.com/en-us/sql/ssdt/database-project-settings?view=sql-server-ver15#bkmk_code_analysis
- After code analysis is enabled perform a rebuild. Any of the rules that are broke will show up as build warnings.
  - You can double click any of the warnings to be taken to the code location where the rule was broken at.
- You can also optionally:
  - Enable / disable rules. 
  - Set certain rules as errors so they will actually throw build errors.

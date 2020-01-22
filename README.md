# Yet Another Ruleset for SQL Server DataTools (YAR, SSDT 🏴‍☠️)

## Overview
Just what it says on the box: A library of SQL best practices as extended [database code analysis rules](https://docs.microsoft.com/en-us/sql/ssdt/overview-of-extensibility-for-database-code-analysis-rules?view=sql-server-ver15) checked at build.

For example code see [here](https://github.com/microsoft/DACExtensions/tree/master/RuleSamples)

## Organization
- SqlServer.Dac - This hold visitors and other utility code
- SqlServer.Rules - This holds the rules derived from `SqlCodeAnalysisRule`
- SqlServer.Rules.Report - Library for evaluating a rule and serializing the result.
- SqlServer.Rules.Generator - a quick console app to report on all rules in a Sql Project.
- SqlServer.Rules.SolutionGenrator - a quick to do a build and evaluate the rules on a Sql Solution
- TestDatabase - a small Sql Solution to test with

## Debug / Test
1) Ensure `SqlServer.Rules.SolutionGenrator` is "Set as Startup Project"
2) Set Command line arguments:
    1) Open up the properties for the `SqlServer.Rules.SolutionGenerator` project
    2) Select the Debug tab
    3) Enter one of the following scenarios:
    	* To debug the test harness db:
    	```--build --reportDirectory ".\Files" --solution "..\..\..\TestDatabase\TestDatabase.sln"```
    	* To debug a Sql project:
    	```--build --reportDirectory ".\Files" --solutions "{path}.sln"```
3) Add a break point in the `SqlServer.Rules` project in any of the rules you wish to debug

Note: if you need to debug the loading of the library see [here](/SqlServer.Rules/README.md)

## Install
Following the instructions on MS Docs [Install Static Code Analysis Rule](https://docs.microsoft.com/en-us/sql/ssdt/walkthrough-author-custom-static-code-analysis-rule-assembly?view=sql-server-ver15#install-a-static-code-analysis-rule).

The build is installed to template path
`%VSBIN%\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\{SqlEngine}\Extensions`
So for VS 2017 with SSDT the path might be
```
SET VSBIN=C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise
%VSBIN%\Common7\Tools\..\IDE\Extensions\Microsoft\SQLDB\DAC\150\Extensions\MyRules
```

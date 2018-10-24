

1) Open up the properties for the SqlServer.Rules.SolutionGenerator project
2) Select the debug tab
3) Enter one of the following scenarios:

	To debug the test harness db:
	--b --d ".\Files" --s "..\..\..\TestDatabase\TestDatabase.sln"

	To debug a Sql project:
	--b --d ".\Files" --s "{path}.sln"

4) Add a break point in the SqlServer.Rules project in any of the rules you wish to debug

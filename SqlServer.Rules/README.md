# Project SqlServer.Rules
This project implements the Static code analysis

## Dependencies
* the `sqlserver.rules` project **must** have its .NET 4.6.1 to load properly into Visual Studio 2015.
* The latest `Microsoft.SqlServer.DacFx.x64` dependency uses .NET 4.6 that is currently the minimum framework that can be selected

How to debug extension loading issues into Visual Studio:
        https://social.msdn.microsoft.com/Forums/en-US/5c84ab8e-b50b-4ecd-86da-866ac3bb2248/known-issue-with-ssdt-extensibility-in-current-release?forum=ssdt

## Gathering an Event Log
For diagnostic purposes we would like you to gather an event log for the issue that you are experiencing in SSDT.
In order to gather a log and send it to a member of the team, please follow the steps below.

1. From an Administrator command prompt run:
    ```powershell
    logman create trace -n DacFxDebug -p "Microsoft-SQLServerDataTools" 0x800 -o "%LOCALAPPDATA%\DacFxDebug.etl" -ets
    logman create trace -n SSDTDebug -p "Microsoft-SQLServerDataToolsVS" 0x800 -o "%LOCALAPPDATA%\SSDTDebug.etl" -ets
    ```
2. Run whatever the target/issue scenario is in SSDT. Go back to the command prompt and run the following commands:
    ```powershell
    logman stop DacFxDebug -ets
    logman stop SSDTDebug -ets
    ```
3. The resulting ETL can be navigated to using Windows Explorer:
    * `%LOCALAPPDATA%\SSDTDebug.etl`
    * `%LOCALAPPDATA%\DacFxDebug.etl`

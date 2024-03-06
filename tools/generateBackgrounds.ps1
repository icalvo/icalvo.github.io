Get-ChildItem ..\src\CommandLine\input\music\works -Directory `
| ForEach-Object {
    .\generateBackground.ps1 $_
}
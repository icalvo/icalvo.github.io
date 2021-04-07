Get-ChildItem ..\music -Directory `
| ForEach-Object {
    .\generateBackground.ps1 $_
}
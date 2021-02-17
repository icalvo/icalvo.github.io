Get-ChildItem ..\music -Directory `
| ForEach-Object {
    .\generate_background.ps1 $_
}
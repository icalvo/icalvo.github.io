$publicPath = Resolve-Path $PSScriptRoot\..\src\CommandLine\_public\

browser-sync start -s "$publicPath" -w
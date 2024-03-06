$projectPath = Resolve-Path $PSScriptRoot\..\src\CommandLine\CommandLine.csproj
$previewPath = Resolve-Path $PSScriptRoot\..\src\CommandLine\_public

del -r -fo $previewPath
dotnet run build --project $projectPath
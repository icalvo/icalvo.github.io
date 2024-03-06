$projectPath = Resolve-Path $PSScriptRoot\..\src\CommandLine\CommandLine.csproj
dotnet run build --project $projectPath
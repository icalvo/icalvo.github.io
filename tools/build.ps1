$projectPath = Resolve-Path $PSScriptRoot\..\src\CommandLine\CommandLine.csproj
$previewPath = Join-Path (Resolve-Path $PSScriptRoot\..\src\CommandLine) '_public'
if (Test-Path $previewPath)
{
  Remove-Item -Recurse -Force $previewPath
}
dotnet run build --project $projectPath

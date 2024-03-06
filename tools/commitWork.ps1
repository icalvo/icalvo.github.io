Param(
    [Parameter(Mandatory=$true)][string]$work
)

Push-Location $PSScriptRoot

$target = Resolve-Path (Join-Path $PSScriptRoot "..\src\CommandLine\input\music\works\$work")

$filesToAdd = Join-Path $target "*"
git add $filesToAdd
$message = Read-Host "Commit message"
git commit -m "$message"

Pop-Location

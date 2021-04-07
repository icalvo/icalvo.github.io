Param(
    [Parameter(Mandatory=$true)][string]$work
)

Push-Location $PSScriptRoot

.\config.ps1

$target = Resolve-Path (Join-Path $PSScriptRoot "..\music\$work")

$filesToAdd = Join-Path $target "*"
git add $filesToAdd
$message = Read-Host "Commit message"
git commit -m "$message"

Pop-Location

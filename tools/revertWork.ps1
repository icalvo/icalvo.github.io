Param(
    [Parameter(Mandatory=$true)][string]$work
)

Push-Location $PSScriptRoot

$target = Resolve-Path (Join-Path $PSScriptRoot "..\music\$work")

git restore --source=HEAD --staged --worktree -- $target
git clean -d -- $target

Pop-Location

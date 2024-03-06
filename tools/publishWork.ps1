Param(
    [Parameter(Mandatory=$true)][string]$work,
    [Parameter(Mandatory=$false)][Switch]$skipPendingCommitsCheck
)

Push-Location $PSScriptRoot

$target = Resolve-Path (Join-Path $PSScriptRoot "..\src\CommandLine\input\music\works\$work")

if (-not $skipPendingCommitsCheck)
{
    if ((git status --porcelain | Where-Object { -not $_.Contains("music/works/$work") }).Count -gt 0) {
        Write-Host "There are pending things to commit outside of $work, please solve them before publishing new things. Otherwise add the switch -notCheckPendingCommits"
        exit
    }
    else {
        Write-Host "Nothing pending to commit outside of $work"
    }
}

if ((git status --porcelain | Where-Object { $_.Contains("music/works/$work") }).Count -eq 0)
{
    Write-Host "No differences detected, nothing to do."
    exit
}

$answer = Read-Host "Want to go ahead an commit/push everything?"

if ($answer -eq 'y') {
    .\commitWork.ps1 $work

    git push
}

Pop-Location

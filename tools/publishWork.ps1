Param(
    [Parameter(Mandatory=$true)][string]$work,
    [Parameter(Mandatory=$false)][Switch]$notCheckPendingCommits
)

Push-Location $PSScriptRoot

.\config.ps1

if (-not $notCheckPendingCommits)
{
    git status | Tee-Object -Variable gitStatusOutput

    if ($gitStatusOutput[-1] -notlike "*nothing to commit*") {
        Write-Host "There are pending things to commit, please solve them before publishing new things."
        exit
    }
}

.\copyWork.ps1 $work

git status | Tee-Object -Variable gitStatusOutput > $null

if ($gitStatusOutput[-1] -like "*nothing to commit*") {
    Write-Host "No differences detected, nothing to do."
    exit
}
.\generate_background.ps1 $work

$answer = Read-Host "Want to go ahead an commit/push everything?"

if ($answer -eq 'y') {
    .\commitWork.ps1 $work

    git push
}

Pop-Location

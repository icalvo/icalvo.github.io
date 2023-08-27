Param(
    [Parameter(Mandatory=$true)][string]$work,
    [Parameter(Mandatory=$false)][Switch]$skipPendingCommitsCheck
)

Push-Location $PSScriptRoot

.\config.ps1
$target = Resolve-Path (Join-Path $PSScriptRoot "..\music\$work")

if (-not $skipPendingCommitsCheck)
{
    git status -- $target | Tee-Object -Variable gitStatusOutput > $null

    if ($gitStatusOutput[-1] -notlike "*nothing to commit*") {
        Write-Host "There are pending things to commit, please solve them before publishing new things. Otherwise add the switch -notCheckPendingCommits"
        exit
    }
    else {
        Write-Host "Nothing pending to commit"
    }
}

.\copyWork.ps1 $work

git status -- $target | Tee-Object -Variable gitStatusOutput > $null

if ($gitStatusOutput[-1] -like "*nothing to commit*") {
    Write-Host "No differences detected, nothing to do."
    exit
}
.\generateBackground.ps1 $work

$answer = Read-Host "Want to go ahead an commit/push everything?"

if ($answer -eq 'y') {
    .\commitWork.ps1 $work

    git push
}

Pop-Location

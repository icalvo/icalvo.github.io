Param(
    [Parameter(Mandatory=$true)][string]$sourceDirectory,
    [Parameter(Mandatory=$true)][string]$work,
    [Parameter(Mandatory=$false)][Switch]$notCheckPendingCommits
)

Push-Location $PSScriptRoot

$source = Join-Path $sourceDirectory $work
$target = Resolve-Path (Join-Path $PSScriptRoot "..\music\$work")

if (-not $notCheckPendingCommits)
{
    git status | Tee-Object -Variable gitStatusOutput

    if ($gitStatusOutput[-1] -notlike "*nothing to commit*") {
        Write-Host "There are pending things to commit, please solve them before publishing new things."
        exit
    }
}

robocopy $source $target /MIR /XF *.wav /XF *.sib /XF *.xml
.\generate_background.ps1 $target.ToString()

git status | Tee-Object -Variable gitStatusOutput

if ($gitStatusOutput[-1] -like "*nothing to commit*") {
    exit
}

$answer = Read-Host "Want to go ahead an commit/push everything?"

if ($answer -eq 'y') {
    git add *
    $message = Read-Host "Commit message"
    git commit -m "$message"
    git push
}

Pop-Location

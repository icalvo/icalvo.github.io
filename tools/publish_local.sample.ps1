# Copy this script to `tools/publish_local.ps1` and set your personal working dir. It will be ignored by git.

Param(
    [Parameter(Mandatory=$true)][string]$work,
    [Parameter(Mandatory=$false)][Switch]$notCheckPendingCommits
)

$myWorkingDir = "C:\Your\Personal\Sibelius\Working\Directory"

Push-Location $PSScriptRoot
.\publish.ps1 $myWorkingDir $work -notCheckPendingCommits:$notCheckPendingCommits
Pop-Location

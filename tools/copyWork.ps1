Param(
    [Parameter(Mandatory=$true)][string]$work
)

Push-Location $PSScriptRoot

$config = .\config.ps1
$myWorkingDir = $config.myWorkingDir
$source = Join-Path $myWorkingDir $work
$target = Resolve-Path (Join-Path $PSScriptRoot "..\music\$work")

robocopy $source $target /MIR /XF *.wav /XF *.sib /XF *.xml

Pop-Location

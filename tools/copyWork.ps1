Param(
    [Parameter(Mandatory=$true)][string]$work
)
function Invoke-Call {
    param (
        [scriptblock]$ScriptBlock,
        [int]$MaxValidExitCode = 0,
        [string]$ErrorAction = $ErrorActionPreference
    )
    echo $ScriptBlock
    & @ScriptBlock | Tee-Object -Variable output > $null
    if (($lastexitcode -gt $maxValidExitCode) -and $ErrorAction -eq "Stop") {
        throw "[$ScriptBlock] failed with exit code $lastexitcode"
    }

    return $output
}

function Get-GitAction() {
    param (
        [string]$file,
        $gitStatusOutput
    )
    $action = "none"
    if (($gitStatusOutput | Where-Object { $_ -like "M *$file*" }).length -ne 0) {
        $action = "modified"
    }
    if (($gitStatusOutput | Where-Object { $_ -like "A *$file*" }).length -ne 0) {
        $action = "added"
    }

    echo "ACTION ON ${file}: $action"
    return $action
}

$ErrorActionPreference = "Stop"

Push-Location $PSScriptRoot

try {
    $config = .\config.ps1
    $myWorkingDir = $config.myWorkingDir
    $source = Join-Path $myWorkingDir $work
    $unresolved = Join-Path $PSScriptRoot "..\music\$work"

    if (-not (Test-Path $unresolved)) {
        mkdir $unresolved
    }
    $target = Resolve-Path $unresolved

    Invoke-Call { robocopy $source $target /MIR /XF *.wav /XF *.sib /XF *.xml } -MaxValidExitCode 8
    Invoke-Call { git add $target }
    $gitStatusOutput = Invoke-Call { git status -s -uall -- $target }
    $actionOnIndex = Get-GitAction "index.markdown" $gitStatusOutput
    $actionOnPdf = Get-GitAction "${work}_full_parts.pdf" $gitStatusOutput
    $actionOnMp3 = Get-GitAction "$work.mp3" $gitStatusOutput
    if ($actionOnIndex -eq "none") {
        throw "No update on index.markdown"
    }
    if ($actionOnPdf -eq "none") {
        throw "PDF was not generated"
    }
    if ($actionOnMp3 -eq "none") {
        throw "MP3 was not generated"
    }

    $sourceIndexFile = "$source\index.markdown"
    $targetIndexFile = "$target\index.markdown"
    
    $sourceIndexContent = Get-Content $sourceIndexFile
    $today = [DateTime]::Now.ToString("yyyy-MM-dd")
    if ($actionOnIndex -eq "added") {
        # Make sure that the header date is Today; change it to Today in source and copy again.
        $sourceIndexContent -replace "date: \d\d\d\d-\d\d-\d\d", "date: $today" | Set-Content $sourceIndexFile
        Copy-Item $sourceIndexFile $targetIndexFile
    }
    else {
        # Make sure that there is a changelog for Today; ask for it otherwise; abort if not provided.
        if (($sourceIndexContent | Where-Object { $_ -like "* ${today}:" }).length -eq 0) {
            $log = Read-Host "No changelog has been provided for today; please provide one"
            if ($sourceIndexContent -match '(?<=\r\n)\z') {
                Add-Content $sourceIndexFile "* ${today}: $log"
            }
            else {
                Add-Content $sourceIndexFile "`r`n* ${today}: $log"
            }

            Copy-Item $sourceIndexFile $targetIndexFile
        }
    }
    

}
finally {
    Pop-Location
}



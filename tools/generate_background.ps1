[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [System.IO.DirectoryInfo]
    $workDirectory
)

Push-Location $workDirectory
$pdf = "$($workDirectory.Name)_full_parts.pdf"
$output = 'background.png'
$pdfExists = Test-Path $pdf -PathType Leaf
if (-not $pdfExists -and (Test-Path "index.markdown")) {
    Write-Host "$pdf does not exist"

    $dmatches = Get-Content "index.markdown" | Select-String "default_audio_movement:\s*(.*)"
    if ($dmatches) {
        $defaultMovement = $dmatches.Matches.Groups[1].Value
        Write-Host "Default movement: $defaultMovement"
        $defaultMovementList = Get-ChildItem -r "$($defaultMovement)_full_parts.pdf"
        if ($defaultMovementList.Length -gt 0) {
            $pdf = $defaultMovementList[0]
            Write-Host "$pdf does exist"
            $pdfExists = $true
        }
        else {
            Write-Host "$pdf does not exist"
            $pdfExists = $false
        }
    }
    else {
        Write-Host "No default movement defined"
    }
}
if ($pdfExists) {
    $inputPage = "$pdf[0]"
    $minColor = magick "$inputPage" -background white -alpha remove -crop 100%x15%+0+0 +repage -format "%[min]" info:

    if ($minColor -eq '65535') {
        Write-Host "First page is not score, choosing page 3 instead"
        $inputPage = "$pdf[2]"
    }

    Write-Host "Generating background for $pdf..."
    magick `
        "$inputPage" `
        -density 300 `
        -background white `
        -alpha remove `
        -distort SRT '-5' `
        -resize 400 `
        -crop 340x340+10+10 +repage `
        -background gray `
        -vignette 0x100+0+0 `
        -fill "rgb(255,240,100)" -colorize 20 `
        -channel RGB -function polynomial 0.6,0.4 `
        -attenuate 0.3 +noise Gaussian `
        "$output"
}
else {
    Write-Host "Copying default background..."
    Copy-Item .\..\..\assets\img\score_background.png "$output"
}

Pop-Location
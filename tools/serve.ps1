$publicPath = Join-Path (Resolve-Path $PSScriptRoot\..\src\CommandLine) '_public'
if (-not (Test-Path $publicPath)) {
  Write-Error "Site output not found at $publicPath. Run tools/build.ps1 first."
  exit 1
}

# Uses tools/bs-config.js: serve _public, reload on html/css/js only (not media).
browser-sync start --config (Join-Path $PSScriptRoot 'bs-config.js')

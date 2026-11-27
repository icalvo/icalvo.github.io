# beforeShellExecution: deny git commit / git push in this repo.
$ErrorActionPreference = 'Stop'

$raw = [Console]::In.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($raw)) {
    Write-Output '{"permission":"deny","agent_message":"Empty hook input; blocking for safety.","user_message":"Git hook received empty input; command blocked."}'
    exit 0
}

try {
    $payload = $raw | ConvertFrom-Json
}
catch {
    Write-Output '{"permission":"deny","agent_message":"Hook JSON parse failed; blocking for safety.","user_message":"Git hook failed to parse input; command blocked."}'
    exit 0
}

$command = [string]$payload.command
# Match git[/git.exe] with optional -C/--git-dir/--work-tree/-c, then commit|push.
$denyPattern = '(?i)\bgit(\.exe)?\b(?:\s+(?:-C\s+\S+|--git-dir=\S+|--work-tree=\S+|-c\s+\S+))*\s+(commit|push)\b'

if ($command -match $denyPattern) {
    Write-Output '{"permission":"deny","user_message":"git commit and git push are blocked in this repo (Cursor hook). Commit/push yourself.","agent_message":"Blocked by project hook: do not run git commit or git push. Leave publishing to the user."}'
    exit 0
}

Write-Output '{"permission":"allow"}'
exit 0

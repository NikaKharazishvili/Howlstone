# Claude Code hook (see settings.json). After each Edit or Write: cuts newlines at the end of that hand-written file (not Unity's own YAML: Unity writes them back), then reports broken code rules to Claude
# When Claude stops: cuts the same newlines in every changed file, also the ones changed with shell tools
$hook = [IO.StreamReader]::new([Console]::OpenStandardInput()).ReadToEnd() | ConvertFrom-Json
$handWritten = '(\.(cs|shader|hlsl|cginc|compute|md|uss|uxml|tss|json|txt|ps1|inputactions)|\.gitignore)$'

function CutFinalNewlines($path) {
    if ($path -notmatch $handWritten -or -not (Test-Path -LiteralPath $path)) { return }
    $bytes = [IO.File]::ReadAllBytes($path)
    $end = $bytes.Length
    while ($end -gt 0 -and $bytes[$end - 1] -in (10, 13)) { $end-- } # \n and \r
    if ($end -lt $bytes.Length) { $file = [IO.File]::Open($path, 'Open'); $file.SetLength($end); $file.Close() }
}

if ($hook.hook_event_name -eq 'Stop') {
    Set-Location $env:CLAUDE_PROJECT_DIR
    foreach ($file in Get-ChildItem CLAUDE.md, README.md, .gitignore, Documentation, .claude, Assets\Scripts -Recurse -File -ErrorAction SilentlyContinue | Where-Object { $_.FullName.Substring((Get-Location).Path.Length) -notmatch '[\\/]Nikson[\\/]' }) { CutFinalNewlines $file.FullName }
    exit
}

$path = $hook.tool_input.file_path
CutFinalNewlines $path
$months = 'January|February|March|April|May|June|July|August|September|October|November|December'
$datePattern = "\b\d{1,2}\s+($months)\b|\b($months)\s+\d{1,2}\b|\b($months)\s+20\d\d\b|\b20\d\d-\d\d-\d\d\b"
$written = if ($hook.tool_input.new_string) { $hook.tool_input.new_string } else { $hook.tool_input.content }
if ($path -match '[\\/](Documentation|\.claude[\\/]rules)[\\/].*\.md$' -and $written -match $datePattern) { [Console]::Error.WriteLine("${path}: Never write a date in a doc or a rule (no day, month or year): say what is true now, and what was tried and why"); exit 2 }
if ($path -match '[\\/]Documentation[\\/].*\.md$' -and (Test-Path -LiteralPath $path)) {
    # A doc that lists a file that is gone (renamed, moved or deleted) is reported, never changed
    $missing = [regex]::Matches([IO.File]::ReadAllText($path), '`(Assets/[^`*<>]+\.\w+)`') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique | Where-Object { -not (Test-Path -LiteralPath (Join-Path $env:CLAUDE_PROJECT_DIR $_)) }
    if ($missing) { [Console]::Error.WriteLine("${path} names files that do not exist (fix the path, or say in the doc that the file is gone): " + (($missing | Select-Object -First 10) -join ', ')); exit 2 }
}
if ($path -notmatch '\.cs$' -or -not (Test-Path -LiteralPath $path)) { exit }
$rules = @(
    @{ Pattern = '(?-i)^\s*(\[[^\]]*\]\s*)*((static|readonly|const|new|unsafe|volatile)\s+)*private\s+(?!(set|get|init)\b)'; Message = 'Never write private before a field or method (it is the default). A private set in a property is fine' },
    @{ Pattern = '\\u[0-9A-Fa-f]{4}|[\uE000-\uF8FF]'; Message = 'Write special characters as (char)0xE014' },
    @{ Pattern = '#[0-9A-Fa-f]{6}\b'; Message = 'Never use a hex code for a color' },
    @{ Pattern = '\bColor\w*\s+\w+\s*=\s*new\('; Message = 'Name the type when creating a color: new Color(...), not new(...)' },
    @{ Pattern = '//.*\.\s*$'; Message = 'A comment must not end with a period' },
    @{ Pattern = '//.{201,}'; Message = 'A comment is max 200 chars' }
)
if ($path -match '[\\/]Assets[\\/]Scripts[\\/](?!Nikson[\\/])') {
    $rules += @{ Pattern = '\(([^()]*,\s*)?params\s+[\w(]'; Message = 'A params method makes a new array at each call: fine when called rarely, never from code that runs every frame' }
}
$lines = [IO.File]::ReadAllLines($path)
$errors = @(); $run = 0
for ($i = 0; $i -lt $lines.Count; $i++) {
    foreach ($rule in $rules) { if ($lines[$i] -match $rule.Pattern) { $errors += "$path line $($i + 1): $($rule.Message)" } }
    $run = if ($lines[$i] -match '^\s*//') { $run + 1 } else { 0 }
    if ($run -eq 3) { $errors += "$path line $($i + 1): A comment is max 2 lines" }
}
if ($errors) { [Console]::Error.WriteLine((($errors | Select-Object -First 10) -join "`n")); exit 2 }
param(
    [string]$TargetDir = (Join-Path $HOME ".gemini\config\skills")
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $TargetDir)) {
    New-Item -ItemType Directory -Path $TargetDir -Force | Out-Null
}

$sourceSkillsDir = Join-Path $PSScriptRoot "skills"
$skillFolders = Get-ChildItem -Path $sourceSkillsDir -Directory

foreach ($folder in $skillFolders) {
    $dest = Join-Path $TargetDir $folder.Name
    Write-Host "Installing skill '$($folder.Name)' -> $dest"
    Copy-Item -Path $folder.FullName -Destination $dest -Recurse -Force
}

Write-Host "`nSuccessfully installed $($skillFolders.Count) skills into $TargetDir" -ForegroundColor Green

param(
    [ValidateSet("demo_kitchen", "from_json", "design_skp", "export_viyar", "install_plugin", "catalog")]
    [string]$Action = "from_json",
    [string]$SpecFile = "",
    [string]$OutputDir = ""
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$WorkspaceRoot = Split-Path -Parent $PSScriptRoot
$DllPath = Join-Path $WorkspaceRoot "dist\ViyarEngine.dll"
$DefaultOutputRoot = Join-Path $WorkspaceRoot "output"
$LiveQueueFile = Join-Path $DefaultOutputRoot "sketchup_live_queue.json"

$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$cs1 = Join-Path $PSScriptRoot "ViyarEngine.cs"
$cs2 = Join-Path $PSScriptRoot "ViyarStudioGenerator.cs"
$cs3 = Join-Path $PSScriptRoot "ViyarSketchUpSkpExporter.cs"
$distDir = Split-Path -Parent $DllPath
if (-not (Test-Path $distDir)) { New-Item -ItemType Directory -Force -Path $distDir | Out-Null }

# Ensure UTF-8 BOM on C# files for csc.exe
foreach ($f in @($cs1, $cs2, $cs3)) {
    $bytes = [System.IO.File]::ReadAllBytes($f)
    if ($bytes.Length -ge 3 -and -not ($bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)) {
        $text = [System.IO.File]::ReadAllText($f, [System.Text.Encoding]::UTF8)
        [System.IO.File]::WriteAllText($f, $text, (New-Object System.Text.UTF8Encoding($true)))
    }
}

try {
    & $csc /nologo /target:library /out:$DllPath $cs1 $cs2 $cs3 2>$null
} catch {}
[System.Reflection.Assembly]::Load([System.IO.File]::ReadAllBytes($DllPath)) | Out-Null

function Install-SketchUpPlugin {
    $pluginSrc = Join-Path $WorkspaceRoot "sketchup_plugin"
    $rbzPath = Join-Path $WorkspaceRoot "dist\viyar_ai_bridge.rbz"
    $zipTemp = Join-Path $WorkspaceRoot "dist\viyar_ai_bridge_temp.zip"
    if (Test-Path $zipTemp) { Remove-Item $zipTemp -Force }
    if (Test-Path $rbzPath) { Remove-Item $rbzPath -Force }

    Compress-Archive -Path (Join-Path $pluginSrc "*") -DestinationPath $zipTemp -Force
    Move-Item -Path $zipTemp -Destination $rbzPath -Force

    $targets = @(
        "$env:APPDATA\SketchUp\SketchUp 2023\SketchUp\Plugins",
        "$env:APPDATA\SketchUp\SketchUp 2024\SketchUp\Plugins",
        "C:\ProgramData\SketchUp\SketchUp 2023\SketchUp\Plugins"
    )
    $installedPaths = @()
    foreach ($t in $targets) {
        try {
            if (-not (Test-Path $t)) { New-Item -ItemType Directory -Force -Path $t | Out-Null }
            Copy-Item -Path (Join-Path $pluginSrc "viyar_ai_bridge.rb") -Destination $t -Force
            $subDir = Join-Path $t "viyar_ai_bridge"
            if (-not (Test-Path $subDir)) { New-Item -ItemType Directory -Force -Path $subDir | Out-Null }
            Copy-Item -Path (Join-Path $pluginSrc "viyar_ai_bridge\main.rb") -Destination $subDir -Force
            $installedPaths += $t
        } catch {
            Write-Warning ("Could not copy to " + $t + " : " + $_.Exception.Message)
        }
    }
    return @{
        rbz_package = $rbzPath
        installed_in = $installedPaths
    }
}

if ($Action -eq "install_plugin") {
    $res = Install-SketchUpPlugin
    $res | ConvertTo-Json -Depth 5
    exit 0
}

if ($Action -eq "catalog") {
    Get-Content (Join-Path $WorkspaceRoot "catalog\viyar_catalog.json") -Raw -Encoding UTF8
    exit 0
}

if ($Action -eq "demo_kitchen") {
    $out = if ($OutputDir) { $OutputDir } else { Join-Path $DefaultOutputRoot "kitchen_modern_2400" }
    $proj = [ViyarAgent.ViyarBuilder]::BuildDemoKitchenProject()
    $files = [ViyarAgent.ViyarBuilder]::ExportSketchUpOnly($proj, $out)
    $syncStatus = [ViyarAgent.ViyarBuilder]::SyncToSketchUpLive($files["json"], $LiveQueueFile)

    @{
        mode = "SKETCHUP_DESIGN_ONLY"
        project_name = $proj.ProjectName
        modules_count = $proj.Modules.Count
        panels_count = $proj.Panels.Count
        total_drilling_holes = $proj.TotalDrillingHoles
        sketchup_sync = $syncStatus
        files = $files
    } | ConvertTo-Json -Depth 6
    exit 0
}

if ($Action -eq "from_json" -or $Action -eq "design_skp" -or $Action -eq "export_viyar") {
    if (-not $SpecFile -or -not (Test-Path $SpecFile)) {
        throw ("SpecFile not found: " + $SpecFile)
    }
    $spec = Get-Content $SpecFile -Raw -Encoding UTF8 | ConvertFrom-Json
    $projName = if ($spec.project_name) { [string]$spec.project_name } else { "Viyar_Project" }
    $safeDir = ($projName -replace '[^\w\-]', '_')
    $out = if ($OutputDir) { $OutputDir } else { Join-Path $DefaultOutputRoot $safeDir }

    $mods = New-Object "System.Collections.Generic.List[ViyarAgent.ModuleInput]"
    foreach ($m in $spec.modules) {
        $mod = New-Object ViyarAgent.ModuleInput
        if ($m.id) { $mod.Id = [string]$m.id }
        if ($m.name) { $mod.Name = [string]$m.name }
        if ($m.type) { $mod.Type = [string]$m.type }
        if ($m.sub_type) { $mod.SubType = [string]$m.sub_type }
        if ($null -ne $m.width_mm) { $mod.WidthMm = [double]$m.width_mm }
        if ($null -ne $m.height_mm) { $mod.HeightMm = [double]$m.height_mm }
        if ($null -ne $m.depth_mm) { $mod.DepthMm = [double]$m.depth_mm }
        if ($null -ne $m.origin_x_mm) { $mod.OriginXMm = [double]$m.origin_x_mm }
        if ($null -ne $m.origin_y_mm) { $mod.OriginYMm = [double]$m.origin_y_mm }
        if ($null -ne $m.origin_z_mm) { $mod.OriginZMm = [double]$m.origin_z_mm }
        if ($null -ne $m.leg_height_mm) { $mod.LegHeightMm = [double]$m.leg_height_mm }
        if ($null -ne $m.shelves_count) { $mod.ShelvesCount = [int]$m.shelves_count }
        if ($null -ne $m.doors_count) { $mod.DoorsCount = [int]$m.doors_count }
        if ($null -ne $m.drawers_count) { $mod.DrawersCount = [int]$m.drawers_count }
        if ($null -ne $m.sections_count) { $mod.SectionsCount = [int]$m.sections_count }
        if ($null -ne $m.led_groove) { $mod.LedGroove = [bool]$m.led_groove }
        if ($null -ne $m.handleless_overhang_mm) { $mod.HandlelessOverhangMm = [double]$m.handleless_overhang_mm }
        if ($m.body_material_id) { $mod.BodyMaterialId = [string]$m.body_material_id }
        if ($m.facade_material_id) { $mod.FacadeMaterialId = [string]$m.facade_material_id }
        if ($m.back_material_id) { $mod.BackMaterialId = [string]$m.back_material_id }
        $mods.Add($mod)
    }

    $proj = [ViyarAgent.ViyarBuilder]::BuildProject($projName, $mods)

    if ($Action -eq "export_viyar") {
        $files = [ViyarAgent.ViyarBuilder]::ExportViyarOnly($proj, $out)
        @{
            mode = "VIYAR_EXPORT_ONLY"
            project_name = $proj.ProjectName
            modules_count = $proj.Modules.Count
            panels_count = $proj.Panels.Count
            total_area_sqm = $proj.TotalAreaSqM
            total_edge_m = $proj.TotalEdgeLengthM
            total_drilling_holes = $proj.TotalDrillingHoles
            sheets_count = $proj.SheetLayouts.Count
            estimated_cost_uah = $proj.EstimatedTotalCostUah
            files = $files
        } | ConvertTo-Json -Depth 6
        exit 0
    }

    # Default (from_json / design_skp): ONLY generate SketchUp .skp model without converting to Viyar formats
    $files = [ViyarAgent.ViyarBuilder]::ExportSketchUpOnly($proj, $out)
    $syncStatus = [ViyarAgent.ViyarBuilder]::SyncToSketchUpLive($files["json"], $LiveQueueFile)

    @{
        mode = "SKETCHUP_DESIGN_ONLY"
        project_name = $proj.ProjectName
        modules_count = $proj.Modules.Count
        panels_count = $proj.Panels.Count
        total_drilling_holes = $proj.TotalDrillingHoles
        sketchup_sync = $syncStatus
        files = $files
    } | ConvertTo-Json -Depth 6
    exit 0
}

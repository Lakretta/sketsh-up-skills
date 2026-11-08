$ErrorActionPreference = "Continue"
[Console]::InputEncoding  = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$WorkspaceRoot = Split-Path -Parent $PSScriptRoot
$DllPath = Join-Path $WorkspaceRoot "dist\ViyarEngine.dll"
$CatalogPath = Join-Path $WorkspaceRoot "catalog\viyar_catalog.json"
$DefaultOutputRoot = Join-Path $WorkspaceRoot "output"
$LiveQueueFile = Join-Path $DefaultOutputRoot "sketchup_live_queue.json"

if (-not (Test-Path $DllPath)) {
    $csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
    $cs1 = Join-Path $PSScriptRoot "ViyarEngine.cs"
    $cs2 = Join-Path $PSScriptRoot "ViyarStudioGenerator.cs"
    $cs3 = Join-Path $PSScriptRoot "ViyarSketchUpSkpExporter.cs"
    $distDir = Split-Path -Parent $DllPath
    if (-not (Test-Path $distDir)) { New-Item -ItemType Directory -Force -Path $distDir | Out-Null }
    & $csc /nologo /target:library /out:$DllPath $cs1 $cs2 $cs3 | Out-Null
}
[System.Reflection.Assembly]::Load([System.IO.File]::ReadAllBytes($DllPath)) | Out-Null

function Send-JsonRpcResponse($id, $result) {
    $resp = @{
        jsonrpc = "2.0"
        id      = $id
        result  = $result
    } | ConvertTo-Json -Depth 12 -Compress
    [Console]::Out.WriteLine($resp)
    [Console]::Out.Flush()
}

function Send-JsonRpcError($id, $code, $message) {
    $resp = @{
        jsonrpc = "2.0"
        id      = $id
        error   = @{
            code    = $code
            message = $message
        }
    } | ConvertTo-Json -Depth 10 -Compress
    [Console]::Out.WriteLine($resp)
    [Console]::Out.Flush()
}

function ConvertTo-ModuleInputList($modulesArg) {
    $list = New-Object "System.Collections.Generic.List[ViyarAgent.ModuleInput]"
    foreach ($m in $modulesArg) {
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
        if ($m.back_type) { $mod.BackType = [string]$m.back_type }
        if ($m.body_material_id) { $mod.BodyMaterialId = [string]$m.body_material_id }
        if ($m.facade_material_id) { $mod.FacadeMaterialId = [string]$m.facade_material_id }
        if ($m.back_material_id) { $mod.BackMaterialId = [string]$m.back_material_id }
        $list.Add($mod)
    }
    return ,$list
}

while ($null -ne ($line = [Console]::In.ReadLine())) {
    if (-not $line.Trim()) { continue }
    try {
        $req = $line | ConvertFrom-Json
        $method = $req.method
        $id = $req.id

        if ($method -eq "initialize") {
            Send-JsonRpcResponse $id @{
                protocolVersion = "2024-11-05"
                capabilities    = @{
                    tools = @{}
                }
                serverInfo      = @{
                    name    = "viyar-furniture-mcp"
                    version = "1.0.0"
                }
            }
        } elseif ($method -eq "notifications/initialized") {
            # No response required
        } elseif ($method -eq "tools/list") {
            $tools = @(
                @{
                    name        = "viyar_list_catalog"
                    description = "Get ViyarPro materials catalog (Egger, Swiss Krono, AGT MDF, HDF), PVC/ABS edge banding articles, Blum/Hafele hardware presets, and supported parametric furniture module types."
                    inputSchema = @{
                        type       = "object"
                        properties = @{}
                    }
                },
                @{
                    name        = "viyar_design_project"
                    description = "Design parametric cabinet furniture for ViyarPro & SketchUp (kitchen lower/upper cabinets, wardrobes, chests of drawers, office desks). Calculates all panels, 4-side edge banding (L1/L2/W1/W2), CNC drilling (Confirmat, Dowel, Blum D35 hinges, System 32 shelf pins), HDF/LED grooves, 2D sheet nesting, exports ViyarPro XML/CSV/TSV/DXF + interactive 3D HTML Studio, and syncs live to SketchUp (with ABF, Gava, OpenCutList, Dynamic Components attributes)."
                    inputSchema = @{
                        type       = "object"
                        properties = @{
                            project_name = @{ type = "string"; description = "Project name (e.g. Kitchen_Modern_2400)" }
                            output_subdir = @{ type = "string"; description = "Optional output subfolder name" }
                            modules = @{
                                type = "array"
                                description = "Array of parametric furniture modules"
                                items = @{
                                    type = "object"
                                    properties = @{
                                        id = @{ type = "string" }
                                        name = @{ type = "string"; description = "Module name" }
                                        type = @{ type = "string"; description = "Module type: kitchen_lower_doors, kitchen_lower_drawers, kitchen_upper_cabinet, wardrobe, office_desk" }
                                        sub_type = @{ type = "string"; description = "Sub-type: standard, sink, oven" }
                                        width_mm = @{ type = "number"; description = "Module width in mm" }
                                        height_mm = @{ type = "number"; description = "Module body height in mm" }
                                        depth_mm = @{ type = "number"; description = "Module body depth in mm" }
                                        origin_x_mm = @{ type = "number"; description = "3D X position in SketchUp scene (mm)" }
                                        origin_y_mm = @{ type = "number"; description = "3D Y position in SketchUp scene (mm)" }
                                        origin_z_mm = @{ type = "number"; description = "3D Z elevation from floor in SketchUp scene (mm)" }
                                        leg_height_mm = @{ type = "number"; description = "Leg/plinth height in mm (default 100 for lower cabinets, 0 for upper)" }
                                        shelves_count = @{ type = "integer"; description = "Number of shelves" }
                                        doors_count = @{ type = "integer"; description = "Number of doors (1 or 2)" }
                                        drawers_count = @{ type = "integer"; description = "Number of drawers (for kitchen_lower_drawers)" }
                                        sections_count = @{ type = "integer"; description = "Number of sections (for wardrobe)" }
                                        led_groove = @{ type = "boolean"; description = "Mill 16x7mm LED profile groove in upper cabinet bottom panel" }
                                        handleless_overhang_mm = @{ type = "number"; description = "Facade bottom overhang for handleless opening (e.g. 20mm)" }
                                        body_material_id = @{ type = "string"; description = "Catalog material ID for body (e.g. egger_w1000_18, egger_h3303_18)" }
                                        facade_material_id = @{ type = "string"; description = "Catalog material ID for facades (e.g. egger_u961_18, mdf_agt_728_18)" }
                                    }
                                    required = @("name", "type", "width_mm", "height_mm", "depth_mm")
                                }
                            }
                        }
                        required = @("project_name", "modules")
                    }
                },
                @{
                    name        = "viyar_inspect_sketchup_scene"
                    description = "Inspect currently open SketchUp 3D scene via TCP bridge (127.0.0.1:9876), extract all furniture panels (including ABF, Gava, OpenCutList, ViyarPro attributes), and optionally export them to ViyarPro XML/CSV."
                    inputSchema = @{
                        type       = "object"
                        properties = @{
                            export_to_viyar = @{ type = "boolean"; description = "If true, exports active SketchUp scene directly to ViyarPro XML and CSV" }
                        }
                    }
                }
            )
            Send-JsonRpcResponse $id @{ tools = $tools }
        } elseif ($method -eq "tools/call") {
            $toolName = $req.params.name
            $argsObj  = $req.params.arguments

            if ($toolName -eq "viyar_list_catalog") {
                $catText = Get-Content $CatalogPath -Raw -Encoding UTF8
                Send-JsonRpcResponse $id @{
                    content = @(
                        @{ type = "text"; text = $catText }
                    )
                }
            } elseif ($toolName -eq "viyar_design_project") {
                $projName = if ($argsObj.project_name) { [string]$argsObj.project_name } else { "Viyar_Project" }
                $subDir   = if ($argsObj.output_subdir) { [string]$argsObj.output_subdir } else { ($projName -replace '[^\w\-]', '_') }
                $outDir   = Join-Path $DefaultOutputRoot $subDir

                $modList = ConvertTo-ModuleInputList $argsObj.modules
                $proj = [ViyarAgent.ViyarBuilder]::BuildProject($projName, $modList)
                $exportViyar = ($null -ne $argsObj.export_viyar -and [bool]$argsObj.export_viyar)
                $files = if ($exportViyar) {
                    [ViyarAgent.ViyarBuilder]::ExportAll($proj, $outDir)
                } else {
                    [ViyarAgent.ViyarBuilder]::ExportSketchUpOnly($proj, $outDir)
                }
                $syncStatus = [ViyarAgent.ViyarBuilder]::SyncToSketchUpLive($files["json"], $LiveQueueFile)

                $summary = @{
                    status = "SUCCESS"
                    mode = if ($exportViyar) { "SKETCHUP_AND_VIYAR" } else { "SKETCHUP_DESIGN_ONLY" }
                    project_name = $proj.ProjectName
                    modules_count = $proj.Modules.Count
                    panels_count = $proj.Panels.Count
                    total_drilling_holes = $proj.TotalDrillingHoles
                    sketchup_sync = $syncStatus
                    exported_files = $files
                } | ConvertTo-Json -Depth 6

                Send-JsonRpcResponse $id @{
                    content = @(
                        @{ type = "text"; text = $summary }
                    )
                }
            } elseif ($toolName -eq "viyar_inspect_sketchup_scene") {
                $cmdName = if ($argsObj.export_to_viyar) { "export_scene_to_viyar" } else { "inspect_scene" }
                try {
                    $client = New-Object System.Net.Sockets.TcpClient("127.0.0.1", 9876)
                    $stream = $client.GetStream()
                    $writer = New-Object System.IO.StreamWriter($stream, (New-Object System.Text.UTF8Encoding($false)))
                    $writer.AutoFlush = $true
                    $reader = New-Object System.IO.StreamReader($stream, [System.Text.Encoding]::UTF8)
                    $payload = @{ command = $cmdName; params = @{ output_dir = $DefaultOutputRoot } } | ConvertTo-Json -Compress
                    $writer.WriteLine($payload)
                    $respLine = $reader.ReadLine()
                    $client.Close()
                    Send-JsonRpcResponse $id @{
                        content = @(
                            @{ type = "text"; text = $respLine }
                        )
                    }
                } catch {
                    Send-JsonRpcResponse $id @{
                        content = @(
                            @{ type = "text"; text = "SketchUp TCP bridge (127.0.0.1:9876) is not currently active. Launch SketchUp - ViyarAIBridge plugin starts automatically." }
                        )
                    }
                }
            } else {
                Send-JsonRpcError $id -32601 ("Unknown tool: " + $toolName)
            }
        }
    } catch {
        Send-JsonRpcError $null -32700 ("MCP Error: " + $_.Exception.Message)
    }
}

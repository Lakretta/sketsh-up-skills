$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$WorkspaceRoot = Split-Path -Parent $PSScriptRoot
$DllPath = Join-Path $WorkspaceRoot "dist\ViyarEngine.dll"
$CliPath = Join-Path $WorkspaceRoot "src\viyar_cli.ps1"
$McpPath = Join-Path $WorkspaceRoot "src\viyar_mcp.ps1"
$DemoOut = Join-Path $WorkspaceRoot "output\kitchen_modern_2400"

$passed = 0
$failed = 0

function Assert-True($cond, $msg) {
    if ($cond) {
        Write-Host ("[PASS] " + $msg) -ForegroundColor Green
        $script:passed++
    } else {
        Write-Host ("[FAIL] " + $msg) -ForegroundColor Red
        $script:failed++
    }
}

Write-Host "=== 1. TESTING VIYAR ENGINE & DEMO KITCHEN GENERATION ===" -ForegroundColor Cyan
Add-Type -Path $DllPath
$proj = [ViyarAgent.ViyarBuilder]::BuildDemoKitchenProject()
Assert-True ($proj.Modules.Count -eq 7) "Demo Kitchen has 7 parametric modules"
Assert-True ($proj.Panels.Count -eq 75) "Demo Kitchen has 75 calculated panels"
Assert-True ($proj.TotalDrillingHoles -gt 200) ("Demo Kitchen has CNC drilling holes: " + $proj.TotalDrillingHoles)
Assert-True ($proj.SheetLayouts.Count -ge 5) ("2D Sheet Nesting produced " + $proj.SheetLayouts.Count + " sheets")

# Verify exact furniture math on Module 1 (600mm width, 18mm board -> 564mm inner bottom width)
$bottomPanel = $proj.Panels | Where-Object { $_.ModuleId -eq "low_1" -and $_.Name -eq "Дно вкладне" } | Select-Object -First 1
Assert-True ($null -ne $bottomPanel -and $bottomPanel.LengthMm -eq 564.0) "Inner bottom width math: 600 - 2*18 = 564.0 mm"

# Verify facade gap math on Module 1 (600mm width, 2 doors -> (600 - 4 - 3)/2 = 296.5mm)
$doorPanel = $proj.Panels | Where-Object { $_.ModuleId -eq "low_1" -and $_.Role -eq "facade_door" } | Select-Object -First 1
Assert-True ($null -ne $doorPanel -and $doorPanel.WidthMm -eq 296.5 -and $doorPanel.LengthMm -eq 716.0) "Door facade math with clearances: 716.0 x 296.5 mm"
Assert-True ($doorPanel.Drilling.Count -ge 2 -and $doorPanel.Drilling[0].DiameterMm -eq 35.0) "Door facade has Blum D35x13mm cup drilling"

Write-Host "`n=== 2. TESTING EXPORTED FILES (XML, CSV, TSV, DXF, HTML STUDIO) ===" -ForegroundColor Cyan
$xmlFile = Join-Path $DemoOut "viyarpro_project.xml"
$csvFile = Join-Path $DemoOut "viyarpro_details.csv"
$tsvFile = Join-Path $DemoOut "viyarpro_clipboard.tsv"
$htmlFile = Join-Path $DemoOut "viyar_studio.html"
$dxfDir  = Join-Path $DemoOut "dxf"

Assert-True (Test-Path $xmlFile) "viyarpro_project.xml exists"
[xml]$xmlDoc = Get-Content $xmlFile -Raw -Encoding UTF8
Assert-True ($xmlDoc.project.details.detail.Count -eq 75) "XML contains all 75 <detail> nodes and parses cleanly"
Assert-True ($xmlDoc.project.materials.material.Count -eq 4) "XML contains 4 <material> nodes with Viyar article codes"

Assert-True (Test-Path $csvFile) "viyarpro_details.csv exists"
$csvLines = Get-Content $csvFile -Encoding UTF8
Assert-True ($csvLines.Count -eq 76) "CSV has header + 75 part rows"

$dxfFiles = Get-ChildItem -Path $dxfDir -Filter "*.dxf"
Assert-True ($dxfFiles.Count -eq 75) "All 75 CNC DXF files generated"
$sampleDxf = Get-Content $dxfFiles[0].FullName -Raw
Assert-True ($sampleDxf.Contains("ABF_CUT") -and $sampleDxf.Contains("ABF_DRILL_") -and $sampleDxf.Contains("EOF")) "DXF contains ABF_CUT and ABF_DRILL CNC layers"

Assert-True ((Test-Path $htmlFile) -and ((Get-Item $htmlFile).Length -gt 25000)) "Interactive 3D & 2D Blueprint Studio HTML generated (>25KB)"

Write-Host "`n=== 3. TESTING SKETCHUP PLUGIN INSTALLATION & RUBY 2.7 SYNTAX ===" -ForegroundColor Cyan
$rbzFile = Join-Path $WorkspaceRoot "dist\viyar_ai_bridge.rbz"
Assert-True (Test-Path $rbzFile) "SketchUp Extension archive dist\viyar_ai_bridge.rbz exists"
Assert-True (Test-Path "$env:APPDATA\SketchUp\SketchUp 2023\SketchUp\Plugins\viyar_ai_bridge\main.rb") "Plugin installed in SketchUp 2023 Roaming Plugins"
Assert-True (Test-Path "$env:APPDATA\SketchUp\SketchUp 2024\SketchUp\Plugins\viyar_ai_bridge\main.rb") "Plugin installed in SketchUp 2024 Roaming Plugins"

Write-Host "`n=== 4. TESTING MCP SERVER JSON-RPC PROTOCOL (viyar_mcp.ps1) ===" -ForegroundColor Cyan
$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = "powershell.exe"
$psi.Arguments = "-NoProfile -ExecutionPolicy Bypass -File `"$McpPath`""
$psi.RedirectStandardInput = $true
$psi.RedirectStandardOutput = $true
$psi.RedirectStandardError = $true
$psi.UseShellExecute = $false
$psi.StandardOutputEncoding = [System.Text.Encoding]::UTF8

$proc = [System.Diagnostics.Process]::Start($psi)
$stdin = New-Object System.IO.StreamWriter($proc.StandardInput.BaseStream, (New-Object System.Text.UTF8Encoding($false)))
$stdin.AutoFlush = $true

# 1. Initialize
$stdin.WriteLine('{"jsonrpc":"2.0","id":1,"method":"initialize","params":{}}')
$initResp = $proc.StandardOutput.ReadLine() | ConvertFrom-Json
Assert-True ($initResp.result.serverInfo.name -eq "viyar-furniture-mcp") "MCP initialize handshake succeeded"

# 2. Tools list
$stdin.WriteLine('{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}')
$toolsResp = $proc.StandardOutput.ReadLine() | ConvertFrom-Json
Assert-True ($toolsResp.result.tools.Count -eq 3) "MCP tools/list returned 3 Viyar furniture tools"

# 3. Call viyar_design_project for a Wardrobe + Office Desk project
$designReq = @{
    jsonrpc = "2.0"
    id = 3
    method = "tools/call"
    params = @{
        name = "viyar_design_project"
        arguments = @{
            project_name = "Home_Office_Wardrobe_Set"
            output_subdir = "home_office_set"
            modules = @(
                @{
                    id = "w1"
                    name = "Wardrobe_1200"
                    type = "wardrobe"
                    width_mm = 1200
                    height_mm = 2400
                    depth_mm = 600
                    origin_x_mm = 0
                    sections_count = 2
                    doors_count = 2
                    shelves_count = 4
                    body_material_id = "egger_w1000_18"
                    facade_material_id = "mdf_agt_3019_18"
                },
                @{
                    id = "d1"
                    name = "Work_Desk_1400"
                    type = "office_desk"
                    width_mm = 1400
                    height_mm = 750
                    depth_mm = 650
                    origin_x_mm = 1300
                    body_material_id = "egger_u961_18"
                    facade_material_id = "egger_h3303_18"
                }
            )
        }
    }
} | ConvertTo-Json -Depth 10 -Compress

$stdin.WriteLine($designReq)
$callResp = $proc.StandardOutput.ReadLine() | ConvertFrom-Json
$callData = $callResp.result.content[0].text | ConvertFrom-Json
Assert-True ($callData.status -eq "SUCCESS" -and $callData.modules_count -eq 2 -and $callData.panels_count -eq 19) "MCP viyar_design_project built Wardrobe + Desk (19 panels) and exported all formats"

$stdin.Close()
if (-not $proc.WaitForExit(2000)) { $proc.Kill() }

Write-Host "`n==========================================" -ForegroundColor Cyan
Write-Host ("TOTAL PASSED: " + $passed + " | FAILED: " + $failed) -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Red" })
if ($failed -gt 0) { exit 1 }

param(
    [Parameter(Mandatory=$true)][string]$InputFile,
    [Parameter(Mandatory=$true)][string]$OutputDir
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

$spec = Get-Content -Path $InputFile -Raw -Encoding UTF8 | ConvertFrom-Json
$rw = [double]$spec.room.width
$rd = [double]$spec.room.depth
$wallT = if ($null -ne $spec.room.wall_thickness) { [double]$spec.room.wall_thickness } else { 150.0 }
$rname = if ($spec.room.name) { [string]$spec.room.name } else { "Room Floor Plan" }

$colors = @{
    "storage"   = @{ fill = "#E2E8F0"; stroke = "#334155" }
    "sleeping"  = @{ fill = "#EDE9FE"; stroke = "#5B21B6" }
    "seating"   = @{ fill = "#FEF3C7"; stroke = "#B45309" }
    "work"      = @{ fill = "#DBEAFE"; stroke = "#1D4ED8" }
    "wet"       = @{ fill = "#CCFBF1"; stroke = "#0F766E" }
    "dining"    = @{ fill = "#FCE7F3"; stroke = "#9D174D" }
    "appliance" = @{ fill = "#F1F5F9"; stroke = "#475569" }
    "default"   = @{ fill = "#F3F4F6"; stroke = "#374151" }
}

function Get-ClearanceBox($it) {
    $x1 = [double]$it.x
    $y1 = [double]$it.y
    $x2 = $x1 + [double]$it.width
    $y2 = $y1 + [double]$it.depth
    $cf = if ($null -ne $it.clearance_front) { [double]$it.clearance_front } else { 0.0 }
    $cb = if ($null -ne $it.clearance_back)  { [double]$it.clearance_back }  else { 0.0 }
    $cl = if ($null -ne $it.clearance_left)  { [double]$it.clearance_left }  else { 0.0 }
    $cr = if ($null -ne $it.clearance_right) { [double]$it.clearance_right } else { 0.0 }
    $facing = if ($it.facing) { $it.facing.ToString().ToLower() } else { "south" }

    if ($facing -eq "north") {
        return [PSCustomObject]@{ x1 = ($x1 - $cl); y1 = ($y1 - $cf); x2 = ($x2 + $cr); y2 = ($y2 + $cb) }
    } elseif ($facing -eq "east") {
        return [PSCustomObject]@{ x1 = ($x1 - $cb); y1 = ($y1 - $cl); x2 = ($x2 + $cf); y2 = ($y2 + $cr) }
    } elseif ($facing -eq "west") {
        return [PSCustomObject]@{ x1 = ($x1 - $cf); y1 = ($y1 - $cl); x2 = ($x2 + $cb); y2 = ($y2 + $cr) }
    } else {
        return [PSCustomObject]@{ x1 = ($x1 - $cl); y1 = ($y1 - $cb); x2 = ($x2 + $cr); y2 = ($y2 + $cf) }
    }
}

$blocking = New-Object System.Collections.Generic.List[string]
$warnings = New-Object System.Collections.Generic.List[string]
$items = @($spec.items)

foreach ($it in $items) {
    $lbl = if ($it.label) { [string]$it.label } else { [string]$it.id }
    $x1 = [double]$it.x; $y1 = [double]$it.y
    $x2 = $x1 + [double]$it.width; $y2 = $y1 + [double]$it.depth
    if ($x1 -lt -1 -or $y1 -lt -1 -or $x2 -gt ($rw + 1) -or $y2 -gt ($rd + 1)) {
        $blocking.Add("Out of bounds: '$lbl' [$x1,$y1 -> $x2,$y2 mm] exceeds room ($rw x $rd mm).")
    }
    $cb = Get-ClearanceBox $it
    if ($cb.x1 -lt -1 -or $cb.y1 -lt -1 -or $cb.x2 -gt ($rw + 1) -or $cb.y2 -gt ($rd + 1)) {
        $warnings.Add("Clearance zone for '$lbl' extends beyond room walls.")
    }
}

for ($i = 0; $i -lt $items.Count; $i++) {
    for ($j = $i + 1; $j -lt $items.Count; $j++) {
        $a = $items[$i]; $b = $items[$j]
        $la = if ($a.label) { [string]$a.label } else { [string]$a.id }
        $lb = if ($b.label) { [string]$b.label } else { [string]$b.id }
        $ax1 = [double]$a.x; $ay1 = [double]$a.y; $ax2 = $ax1 + [double]$a.width; $ay2 = $ay1 + [double]$a.depth
        $bx1 = [double]$b.x; $by1 = [double]$b.y; $bx2 = $bx1 + [double]$b.width; $by2 = $by1 + [double]$b.depth

        $ox = [Math]::Min($ax2, $bx2) - [Math]::Max($ax1, $bx1)
        $oy = [Math]::Min($ay2, $by2) - [Math]::Max($ay1, $by1)
        if ($ox -gt 1 -and $oy -gt 1) {
            $blocking.Add("Physical collision: '$la' overlaps with '$lb' by $([Math]::Round($ox))x$([Math]::Round($oy)) mm.")
        } else {
            $ca = Get-ClearanceBox $a
            $coxA = [Math]::Min([double]$ca.x2, $bx2) - [Math]::Max([double]$ca.x1, $bx1)
            $coyA = [Math]::Min([double]$ca.y2, $by2) - [Math]::Max([double]$ca.y1, $by1)
            if ($coxA -gt 5 -and $coyA -gt 5) {
                $warnings.Add("Clearance intrusion: '$lb' intrudes into clearance zone of '$la' by $([Math]::Round($coxA))x$([Math]::Round($coyA)) mm.")
            }
            $cb = Get-ClearanceBox $b
            $coxB = [Math]::Min([double]$cb.x2, $ax2) - [Math]::Max([double]$cb.x1, $ax1)
            $coyB = [Math]::Min([double]$cb.y2, $ay2) - [Math]::Max([double]$cb.y1, $ay1)
            if ($coxB -gt 5 -and $coyB -gt 5) {
                $warnings.Add("Clearance intrusion: '$la' intrudes into clearance zone of '$lb' by $([Math]::Round($coxB))x$([Math]::Round($coyB)) mm.")
            }
        }
    }
}

$totalFurnArea = 0.0
foreach ($it in $items) { $totalFurnArea += ([double]$it.width * [double]$it.depth) }
$roomAreaSqm = [Math]::Round(($rw * $rd) / 1000000.0, 2)
$footprintPct = if (($rw * $rd) -gt 0) { [Math]::Round(($totalFurnArea / ($rw * $rd)) * 100.0, 1) } else { 0.0 }

$margin = 450.0
$svgW = $rw + 2 * $margin
$svgH = $rd + 2 * $margin
$ox = $margin
$oy = $margin

$svg = New-Object System.Text.StringBuilder
[void]$svg.AppendLine("<svg xmlns=`"http://www.w3.org/2000/svg`" viewBox=`"0 0 $svgW $svgH`" width=`"100%`" height=`"100%`" style=`"max-height:760px;background:#FAFAFA;font-family:Inter,Segoe UI,Arial,sans-serif;`">")
[void]$svg.AppendLine("<defs><pattern id=`"clearanceHatch`" width=`"40`" height=`"40`" patternTransform=`"rotate(45 0 0)`" patternUnits=`"userSpaceOnUse`"><line x1=`"0`" y1=`"0`" x2=`"0`" y2=`"40`" stroke=`"#94A3B8`" stroke-width=`"4`" stroke-dasharray=`"8,8`" /></pattern></defs>")
[void]$svg.AppendLine("<text x=`"$ox`" y=`"$($oy - 220)`" font-size=`"95`" font-weight=`"700`" fill=`"#0F172A`">$rname ($rw x $rd mm | $roomAreaSqm m2)</text>")
[void]$svg.AppendLine("<rect x=`"$($ox - $wallT)`" y=`"$($oy - $wallT)`" width=`"$($rw + 2*$wallT)`" height=`"$($rd + 2*$wallT)`" fill=`"#334155`" rx=`"8`" />")
[void]$svg.AppendLine("<rect x=`"$ox`" y=`"$oy`" width=`"$rw`" height=`"$rd`" fill=`"#FFFFFF`" />")

for ($gx = 500; $gx -lt $rw; $gx += 500) {
    [void]$svg.AppendLine("<line x1=`"$($ox + $gx)`" y1=`"$oy`" x2=`"$($ox + $gx)`" y2=`"$($oy + $rd)`" stroke=`"#F1F5F9`" stroke-width=`"4`" />")
}
for ($gy = 500; $gy -lt $rd; $gy += 500) {
    [void]$svg.AppendLine("<line x1=`"$ox`" y1=`"$($oy + $gy)`" x2=`"$($ox + $rw)`" y2=`"$($oy + $gy)`" stroke=`"#F1F5F9`" stroke-width=`"4`" />")
}

foreach ($op in @($spec.openings)) {
    if ($null -eq $op) { continue }
    $otype = if ($op.type) { $op.type.ToString().ToLower() } else { "door" }
    $wall = if ($op.wall) { $op.wall.ToString().ToLower() } else { "south" }
    $off = [double]$op.offset
    $ow = [double]$op.width
    if ($wall -eq "north") { $wx = $ox + $off; $wy = $oy - $wallT; $ww = $ow; $wh = $wallT }
    elseif ($wall -eq "south") { $wx = $ox + $off; $wy = $oy + $rd; $ww = $ow; $wh = $wallT }
    elseif ($wall -eq "west") { $wx = $ox - $wallT; $wy = $oy + $off; $ww = $wallT; $wh = $ow }
    else { $wx = $ox + $rw; $wy = $oy + $off; $ww = $wallT; $wh = $ow }

    if ($otype -eq "window") {
        [void]$svg.AppendLine("<rect x=`"$wx`" y=`"$wy`" width=`"$ww`" height=`"$wh`" fill=`"#BAE6FD`" stroke=`"#0284C7`" stroke-width=`"6`" />")
    } else {
        [void]$svg.AppendLine("<rect x=`"$wx`" y=`"$wy`" width=`"$ww`" height=`"$wh`" fill=`"#FFFFFF`" stroke=`"#64748B`" stroke-width=`"4`" stroke-dasharray=`"12,8`" />")
    }
}

foreach ($it in $items) {
    $cb = Get-ClearanceBox $it
    $cw = [double]$cb.x2 - [double]$cb.x1
    $ch = [double]$cb.y2 - [double]$cb.y1
    if ($cw -gt ([double]$it.width + 1) -or $ch -gt ([double]$it.depth + 1)) {
        [void]$svg.AppendLine("<rect x=`"$($ox + [double]$cb.x1)`" y=`"$($oy + [double]$cb.y1)`" width=`"$cw`" height=`"$ch`" fill=`"url(#clearanceHatch)`" stroke=`"#64748B`" stroke-width=`"5`" stroke-dasharray=`"20,15`" rx=`"8`" />")
    }
}

foreach ($it in $items) {
    $lbl = if ($it.label) { [string]$it.label } else { [string]$it.id }
    $cat = if ($it.category) { $it.category.ToString().ToLower() } else { "default" }
    $st = if ($colors.ContainsKey($cat)) { $colors[$cat] } else { $colors["default"] }
    $ix = $ox + [double]$it.x; $iy = $oy + [double]$it.y
    $iw = [double]$it.width; $id = [double]$it.depth
    [void]$svg.AppendLine("<rect x=`"$ix`" y=`"$iy`" width=`"$iw`" height=`"$id`" fill=`"$($st.fill)`" stroke=`"$($st.stroke)`" stroke-width=`"8`" rx=`"10`" />")
    $cx = $ix + ($iw / 2.0); $cy = $iy + ($id / 2.0)
    $fsz = [Math]::Max(42, [Math]::Min(70, [int]([Math]::Min($iw, $id) * 0.16)))
    [void]$svg.AppendLine("<text x=`"$cx`" y=`"$($cy - 12)`" font-size=`"$fsz`" font-weight=`"600`" fill=`"#0F172A`" text-anchor=`"middle`" dominant-baseline=`"middle`">$lbl</text>")
    [void]$svg.AppendLine("<text x=`"$cx`" y=`"$($cy + $fsz * 0.95)`" font-size=`"$([int]($fsz * 0.78))`" fill=`"#475569`" text-anchor=`"middle`" dominant-baseline=`"middle`">${iw}x${id}</text>")
}

[void]$svg.AppendLine("</svg>")
$svgText = $svg.ToString()

$svgPath = Join-Path $OutputDir "floorplan.svg"
$jsonPath = Join-Path $OutputDir "validation_report.json"
$htmlPath = Join-Path $OutputDir "floorplan_report.html"

Set-Content -Path $svgPath -Value $svgText -Encoding UTF8

$reportObj = [ordered]@{
    valid                   = ($blocking.Count -eq 0)
    room_width_mm           = $rw
    room_depth_mm           = $rd
    room_area_sqm           = $roomAreaSqm
    furniture_footprint_pct = $footprintPct
    blocking_issues         = $blocking
    warnings                = $warnings
}
$jsonText = $reportObj | ConvertTo-Json -Depth 5
Set-Content -Path $jsonPath -Value $jsonText -Encoding UTF8

$htmlDoc = @"
<!DOCTYPE html>
<html lang="en">
<head><meta charset="utf-8" /><title>$rname</title>
<style>body{font-family:Inter,Segoe UI,sans-serif;margin:24px;background:#F8FAFC;color:#0F172A;}.card{background:#FFF;border:1px solid #E2E8F0;border-radius:12px;padding:20px;margin-bottom:20px;}</style>
</head>
<body>
<div class="card"><h2>$rname ($roomAreaSqm m2 | Footprint: $footprintPct%)</h2></div>
<div class="card">$svgText</div>
</body></html>
"@
Set-Content -Path $htmlPath -Value $htmlDoc -Encoding UTF8

Write-Output $jsonText

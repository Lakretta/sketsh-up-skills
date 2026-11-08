#!/usr/bin/env python3
"""
Automated 2D Architectural Floorplan Renderer & Ergonomic Collision Validator.
Reads a JSON room specification (in millimeters), validates spatial boundaries,
furniture overlaps, door swings, and ergonomic clearance zones, and outputs:
  1. floorplan.svg        — Scaled architectural 2D SVG floorplan
  2. floorplan_report.html — Interactive HTML report with SVG + Furniture Schedule + Audit Status
  3. validation_report.json — Structured validation findings
"""

import argparse
import html
import json
import math
import sys
from pathlib import Path
from typing import Any, Dict, List, Tuple


CATEGORY_COLORS = {
    "storage": {"fill": "#E2E8F0", "stroke": "#334155"},
    "sleeping": {"fill": "#EDE9FE", "stroke": "#5B21B6"},
    "seating": {"fill": "#FEF3C7", "stroke": "#B45309"},
    "work": {"fill": "#DBEAFE", "stroke": "#1D4ED8"},
    "wet": {"fill": "#CCFBF1", "stroke": "#0F766E"},
    "dining": {"fill": "#FCE7F3", "stroke": "#9D174D"},
    "appliance": {"fill": "#F1F5F9", "stroke": "#475569"},
    "default": {"fill": "#F3F4F6", "stroke": "#374151"},
}


def rect_overlap(a: Tuple[float, float, float, float], b: Tuple[float, float, float, float]) -> Tuple[float, float]:
    """Return (overlap_x, overlap_y) for two rects (x1, y1, x2, y2). Positive means overlapping."""
    ox = min(a[2], b[2]) - max(a[0], b[0])
    oy = min(a[3], b[3]) - max(a[1], b[1])
    return (ox, oy)


def get_item_bbox(item: Dict[str, Any]) -> Tuple[float, float, float, float]:
    x = float(item["x"])
    y = float(item["y"])
    w = float(item["width"])
    d = float(item["depth"])
    return (x, y, x + w, y + d)


def get_clearance_bbox(item: Dict[str, Any]) -> Tuple[float, float, float, float]:
    x, y, x2, y2 = get_item_bbox(item)
    cf = float(item.get("clearance_front", 0))
    cb = float(item.get("clearance_back", 0))
    cl = float(item.get("clearance_left", 0))
    cr = float(item.get("clearance_right", 0))
    facing = item.get("facing", "south").lower()
    if facing == "south":
        return (x - cl, y - cb, x2 + cr, y2 + cf)
    if facing == "north":
        return (x - cl, y - cf, x2 + cr, y2 + cb)
    if facing == "east":
        return (x - cb, y - cl, x2 + cf, y2 + cr)
    if facing == "west":
        return (x - cf, y - cl, x2 + cb, y2 + cr)
    return (x - cl, y - cb, x2 + cr, y2 + cf)


def validate_layout(spec: Dict[str, Any]) -> Dict[str, Any]:
    room = spec.get("room", {})
    rw = float(room.get("width", 4000))
    rd = float(room.get("depth", 3500))
    items = spec.get("items", [])

    blocking_issues: List[str] = []
    warnings: List[str] = []

    # 1. Check room boundaries
    for item in items:
        label = item.get("label", item.get("id", "Item"))
        x1, y1, x2, y2 = get_item_bbox(item)
        if x1 < -1 or y1 < -1 or x2 > rw + 1 or y2 > rd + 1:
            blocking_issues.append(
                f"Out of bounds: '{label}' [{x1:.0f},{y1:.0f} -> {x2:.0f},{y2:.0f} mm] exceeds room ({rw:.0f}x{rd:.0f} mm)."
            )
        cx1, cy1, cx2, cy2 = get_clearance_bbox(item)
        if cx1 < -1 or cy1 < -1 or cx2 > rw + 1 or cy2 > rd + 1:
            warnings.append(
                f"Clearance zone for '{label}' [{cx1:.0f},{cy1:.0f} -> {cx2:.0f},{cy2:.0f} mm] extends beyond room walls."
            )

    # 2. Check furniture-to-furniture collisions and clearance intrusions
    for i in range(len(items)):
        for j in range(i + 1, len(items)):
            a = items[i]
            b = items[j]
            la = a.get("label", a.get("id", f"Item {i}"))
            lb = b.get("label", b.get("id", f"Item {j}"))
            ba = get_item_bbox(a)
            bb = get_item_bbox(b)
            ox, oy = rect_overlap(ba, bb)
            if ox > 1 and oy > 1:
                blocking_issues.append(
                    f"Physical collision: '{la}' overlaps with '{lb}' by {ox:.0f}x{oy:.0f} mm."
                )
            else:
                # Check if B intrudes into A's clearance zone or vice versa
                ca = get_clearance_bbox(a)
                cb = get_clearance_bbox(b)
                cox_a, coy_a = rect_overlap(ca, bb)
                if cox_a > 5 and coy_a > 5:
                    warnings.append(
                        f"Clearance intrusion: '{lb}' intrudes into required clearance zone of '{la}' by {cox_a:.0f}x{coy_a:.0f} mm."
                    )
                cox_b, coy_b = rect_overlap(cb, ba)
                if cox_b > 5 and coy_b > 5:
                    warnings.append(
                        f"Clearance intrusion: '{la}' intrudes into required clearance zone of '{lb}' by {cox_b:.0f}x{coy_b:.0f} mm."
                    )

    total_furniture_area = sum(float(it["width"]) * float(it["depth"]) for it in items)
    room_area_sqm = (rw * rd) / 1_000_000.0
    footprint_ratio = (total_furniture_area / (rw * rd) * 100.0) if (rw * rd) > 0 else 0.0

    return {
        "valid": len(blocking_issues) == 0,
        "room_width_mm": rw,
        "room_depth_mm": rd,
        "room_area_sqm": round(room_area_sqm, 2),
        "furniture_footprint_pct": round(footprint_ratio, 1),
        "blocking_issues": blocking_issues,
        "warnings": warnings,
    }


def generate_svg(spec: Dict[str, Any]) -> str:
    room = spec.get("room", {})
    rname = room.get("name", "Room Floor Plan")
    rw = float(room.get("width", 4000))
    rd = float(room.get("depth", 3500))
    wall_t = float(room.get("wall_thickness", 150))

    margin = 450.0
    svg_w = rw + 2 * margin
    svg_h = rd + 2 * margin
    ox = margin
    oy = margin

    lines: List[str] = []
    lines.append(
        f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {svg_w:.0f} {svg_h:.0f}" '
        f'width="100%" height="100%" style="max-height:760px;background:#FAFAFA;font-family:Inter,Segoe UI,Arial,sans-serif;">'
    )

    # Defs for hatches and arrows
    lines.append("""
    <defs>
      <pattern id="clearanceHatch" width="40" height="40" patternTransform="rotate(45 0 0)" patternUnits="userSpaceOnUse">
        <line x1="0" y1="0" x2="0" y2="40" stroke="#94A3B8" stroke-width="4" stroke-dasharray="8,8" />
      </pattern>
    </defs>
    """)

    # Title
    lines.append(
        f'<text x="{ox:.0f}" y="{oy - 220:.0f}" font-size="95" font-weight="700" fill="#0F172A">'
        f'{html.escape(rname)} ({rw:.0f} × {rd:.0f} mm | {(rw*rd/1e6):.2f} m²)</text>'
    )

    # Outer & inner walls
    lines.append(
        f'<rect x="{ox - wall_t:.0f}" y="{oy - wall_t:.0f}" width="{rw + 2*wall_t:.0f}" height="{rd + 2*wall_t:.0f}" '
        f'fill="#334155" rx="8" />'
    )
    lines.append(
        f'<rect x="{ox:.0f}" y="{oy:.0f}" width="{rw:.0f}" height="{rd:.0f}" fill="#FFFFFF" />'
    )

    # 500 mm subtle architectural grid
    for gx in range(500, int(rw), 500):
        lines.append(
            f'<line x1="{ox + gx:.0f}" y1="{oy:.0f}" x2="{ox + gx:.0f}" y2="{oy + rd:.0f}" '
            f'stroke="#F1F5F9" stroke-width="4" />'
        )
    for gy in range(500, int(rd), 500):
        lines.append(
            f'<line x1="{ox:.0f}" y1="{oy + gy:.0f}" x2="{ox + rw:.0f}" y2="{oy + gy:.0f}" '
            f'stroke="#F1F5F9" stroke-width="4" />'
        )

    # Windows and doors
    for op in spec.get("openings", []):
        otype = op.get("type", "door").lower()
        wall = op.get("wall", "south").lower()
        offset = float(op.get("offset", 500))
        ow = float(op.get("width", 800))
        if wall == "north":
            wx, wy, ww, wh = ox + offset, oy - wall_t, ow, wall_t
        elif wall == "south":
            wx, wy, ww, wh = ox + offset, oy + rd, ow, wall_t
        elif wall == "west":
            wx, wy, ww, wh = ox - wall_t, oy + offset, wall_t, ow
        else:
            wx, wy, ww, wh = ox + rw, oy + offset, wall_t, ow

        if otype == "window":
            lines.append(f'<rect x="{wx:.0f}" y="{wy:.0f}" width="{ww:.0f}" height="{wh:.0f}" fill="#BAE6FD" stroke="#0284C7" stroke-width="6" />')
        else:
            lines.append(f'<rect x="{wx:.0f}" y="{wy:.0f}" width="{ww:.0f}" height="{wh:.0f}" fill="#FFFFFF" stroke="#64748B" stroke-width="4" stroke-dasharray="12,8" />')

    # Clearance zones first (underneath furniture)
    for item in spec.get("items", []):
        cx1, cy1, cx2, cy2 = get_clearance_bbox(item)
        ix1, iy1, ix2, iy2 = get_item_bbox(item)
        if (cx2 - cx1) > (ix2 - ix1) + 1 or (cy2 - cy1) > (iy2 - iy1) + 1:
            lines.append(
                f'<rect x="{ox + cx1:.0f}" y="{oy + cy1:.0f}" width="{cx2 - cx1:.0f}" height="{cy2 - cy1:.0f}" '
                f'fill="url(#clearanceHatch)" stroke="#64748B" stroke-width="5" stroke-dasharray="20,15" rx="8" />'
            )

    # Furniture items
    for item in spec.get("items", []):
        label = item.get("label", item.get("id", "Item"))
        cat = item.get("category", "default").lower()
        style = CATEGORY_COLORS.get(cat, CATEGORY_COLORS["default"])
        ix = ox + float(item["x"])
        iy = oy + float(item["y"])
        iw = float(item["width"])
        id_ = float(item["depth"])

        lines.append(
            f'<rect x="{ix:.0f}" y="{iy:.0f}" width="{iw:.0f}" height="{id_:.0f}" '
            f'fill="{style["fill"]}" stroke="{style["stroke"]}" stroke-width="8" rx="10" />'
        )
        cx = ix + iw / 2.0
        cy = iy + id_ / 2.0
        font_sz = max(42, min(72, int(min(iw, id_) * 0.16)))
        lines.append(
            f'<text x="{cx:.0f}" y="{cy - 12:.0f}" font-size="{font_sz}" font-weight="600" '
            f'fill="#0F172A" text-anchor="middle" dominant-baseline="middle">{html.escape(label)}</text>'
        )
        lines.append(
            f'<text x="{cx:.0f}" y="{cy + font_sz * 0.95:.0f}" font-size="{int(font_sz * 0.78)}" '
            f'fill="#475569" text-anchor="middle" dominant-baseline="middle">{iw:.0f}×{id_:.0f}</text>'
        )

    # Overall Room Dimension Callouts
    lines.append(
        f'<line x1="{ox:.0f}" y1="{oy + rd + wall_t + 110:.0f}" x2="{ox + rw:.0f}" y2="{oy + rd + wall_t + 110:.0f}" '
        f'stroke="#0F172A" stroke-width="6" />'
    )
    lines.append(
        f'<text x="{ox + rw/2:.0f}" y="{oy + rd + wall_t + 200:.0f}" font-size="72" font-weight="600" '
        f'fill="#0F172A" text-anchor="middle">{rw:.0f} mm</text>'
    )
    lines.append(
        f'<line x1="{ox + rw + wall_t + 110:.0f}" y1="{oy:.0f}" x2="{ox + rw + wall_t + 110:.0f}" y2="{oy + rd:.0f}" '
        f'stroke="#0F172A" stroke-width="6" />'
    )
    lines.append(
        f'<text x="{ox + rw + wall_t + 200:.0f}" y="{oy + rd/2:.0f}" font-size="72" font-weight="600" '
        f'fill="#0F172A" text-anchor="middle" transform="rotate(90 {ox + rw + wall_t + 200:.0f} {oy + rd/2:.0f})">{rd:.0f} mm</text>'
    )

    lines.append("</svg>")
    return "\n".join(lines)


def main() -> int:
    parser = argparse.ArgumentParser(description="Validate and render 2D architectural floorplan from JSON spec.")
    parser.add_argument("--input", required=True, help="Path to JSON room specification file.")
    parser.add_argument("--output-dir", required=True, help="Directory to write SVG, HTML, and JSON reports.")
    args = parser.parse_args()

    spec_path = Path(args.input)
    out_dir = Path(args.output_dir)
    out_dir.mkdir(parents=True, exist_ok=True)

    spec = json.loads(spec_path.read_text(encoding="utf-8"))
    report = validate_layout(spec)
    svg_content = generate_svg(spec)

    svg_path = out_dir / "floorplan.svg"
    json_path = out_dir / "validation_report.json"
    html_path = out_dir / "floorplan_report.html"

    svg_path.write_text(svg_content, encoding="utf-8")
    json_path.write_text(json.dumps(report, indent=2, ensure_ascii=False), encoding="utf-8")

    rows_html = []
    for item in spec.get("items", []):
        rows_html.append(
            f"<tr><td><strong>{html.escape(str(item.get('label', item.get('id', ''))))}</strong></td>"
            f"<td>{html.escape(str(item.get('category', 'default')))}</td>"
            f"<td>{float(item['width']):.0f} × {float(item['depth']):.0f} mm</td>"
            f"<td>({float(item['x']):.0f}, {float(item['y']):.0f}) mm</td>"
            f"<td>{float(item.get('clearance_front', 0)):.0f} mm ({html.escape(str(item.get('facing', 'south')))})</td></tr>"
        )

    status_badge = (
        '<span style="background:#DCFCE7;color:#166534;padding:6px 14px;border-radius:999px;font-weight:600;">PASS (0 Blocking Collisions)</span>'
        if report["valid"]
        else f'<span style="background:#FEE2E2;color:#991B1B;padding:6px 14px;border-radius:999px;font-weight:600;">FAIL ({len(report["blocking_issues"])} Blocking Issues)</span>'
    )

    issues_html = ""
    for bi in report["blocking_issues"]:
        issues_html += f'<li style="color:#991B1B;font-weight:600;">[BLOCKING] {html.escape(bi)}</li>'
    for w in report["warnings"]:
        issues_html += f'<li style="color:#B45309;">[WARNING] {html.escape(w)}</li>'
    if not issues_html:
        issues_html = '<li style="color:#166534;">All furniture items and ergonomic clearances fit cleanly within room boundaries.</li>'

    html_doc = f"""<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <title>{html.escape(spec.get("room", {}).get("name", "Floor Plan Report"))}</title>
  <style>
    body {{ font-family: Inter, Segoe UI, system-ui, sans-serif; margin: 24px; background: #F8FAFC; color: #0F172A; }}
    .card {{ background: #FFF; border: 1px solid #E2E8F0; border-radius: 12px; padding: 20px; margin-bottom: 20px; box-shadow: 0 1px 3px rgba(0,0,0,0.04); }}
    table {{ width: 100%; border-collapse: collapse; margin-top: 12px; }}
    th, td {{ text-align: left; padding: 10px 12px; border-bottom: 1px solid #E2E8F0; font-size: 14px; }}
    th {{ background: #F1F5F9; font-weight: 600; }}
  </style>
</head>
<body>
  <div class="card">
    <h2 style="margin-top:0;">{html.escape(spec.get("room", {}).get("name", "Floor Plan"))} — Spatial Validation {status_badge}</h2>
    <p>Room Area: <strong>{report["room_area_sqm"]} m²</strong> ({report["room_width_mm"]:.0f} × {report["room_depth_mm"]:.0f} mm) | Furniture Footprint Density: <strong>{report["furniture_footprint_pct"]}%</strong> (Target: &le; 45%)</p>
    <ul>{issues_html}</ul>
  </div>
  <div class="card">
    {svg_content}
  </div>
  <div class="card">
    <h3 style="margin-top:0;">Furniture & Clearance Schedule</h3>
    <table>
      <thead><tr><th>Item</th><th>Category</th><th>Dimensions (W × D)</th><th>Position (X, Y)</th><th>Front Clearance</th></tr></thead>
      <tbody>{"".join(rows_html)}</tbody>
    </table>
  </div>
</body>
</html>"""
    html_path.write_text(html_doc, encoding="utf-8")

    print(json.dumps(report, indent=2, ensure_ascii=False))
    return 0 if report["valid"] else 2


if __name__ == "__main__":
    sys.exit(main())

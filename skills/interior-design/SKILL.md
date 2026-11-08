---
name: interior-design
description: >-
  Comprehensive interior design, architectural space planning, floor plan auditing, ergonomics, multi-layered lighting
  (IES standards), electrical layouts, color/material palettes (including Egger/Kronospan decors), 2D floorplan
  generation with collision checking, and SketchUp-to-AI photorealistic rendering workflows. Use this skill whenever
  the user asks to plan or redesign a room or apartment, audit a floor plan, calculate furniture clearances and
  circulation paths, design lighting/switches/sockets, choose interior styles and materials, render a 2D floorplan, or
  create interior visualizations.
---

# Unified Interior Design & Architectural Space Planning Skill

This skill combines professional architectural space planning (**ASID / NCIDQ / Neufert** ergonomics), systematic **Floor Plan Auditing**, **IES Lighting & Electrical Engineering**, **Color & Material Specification** (bridged with Viyar/Egger/Kronospan decors), **Automated 2D Floorplan Validation & Rendering**, and **SketchUp / AI Visualization Workflows**.

---

## 1. Core Workflows Overview

Select the appropriate workflow(s) based on the user's request:

| User Goal | Primary Workflow | Reference / Helper to Load |
| :--- | :--- | :--- |
| **Plan a room or apartment layout from scratch** | Workflow A: Space Planning & Ergonomics | [ergonomics-and-clearances.md](./references/ergonomics-and-clearances.md) |
| **Review/critique an existing floor plan (image, PDF, CAD)** | Workflow B: Floor Plan Audit & Compliance | [floor-plan-audit-checklist.md](./references/floor-plan-audit-checklist.md) |
| **Validate room geometry & generate a 2D floorplan diagram** | Workflow C: Automated 2D Floorplan & Collision Check | [render_floorplan.ps1](./scripts/render_floorplan.ps1) |
| **Design lighting, switches, sockets, or material/color palettes** | Workflow D: Lighting (IES), Electrical & Materials | [lighting-electrical-and-materials.md](./references/lighting-electrical-and-materials.md) |
| **Generate 3D renders, SketchUp views, or room restyling** | Workflow E: SketchUp & AI Visualization | [visualization-and-rendering.md](./references/visualization-and-rendering.md) |
| **Model custom cabinetry in SketchUp or find real store items** | Workflow F: Ecosystem Handoff (`viyar-furniture-designer` / `visual-shopping`) | Section 7 below |

---

## 2. Workflow A: Space Planning, Zoning & Ergonomics

Always work in **millimeters (`mm`)** (or centimeters when explicitly requested by the user) and never guess room proportions without verifying clearances.

1. **Spatial Intake & Fixed Constraints**:
   - Record overall internal dimensions (`Width × Depth × Ceiling Height`), wall thicknesses, window sill heights (`H_sill`), door openings and swing directions, radiators/heating manifolds, electrical panel location, and plumbing risers (`водопровід / каналізація`).
2. **Functional Zoning**:
   - Divide the layout into distinct functional zones: **Entry/Dirty Zone** (isolated so street footwear traffic never crosses living/sleeping paths), **Day/Social Zone** (living, dining, kitchen), **Night/Private Zone** (bedrooms, master bath), **Work Zone** (acoustic & glare control), and **Utility/Storage Zone** (target **12–18%** of total floor area dedicated to built-in storage).
3. **Circulation Hierarchy**:
   - **Primary circulation arteries**: `>= 900–1000 mm` clear width.
   - **Secondary room pathways**: `>= 750–850 mm` clear width.
   - **Tight pass-throughs (e.g., around bed foot or dining chairs)**: `>= 600 mm` absolute minimum.
4. **Furniture Sizing & Clearance Verification**:
   - Load [ergonomics-and-clearances.md](./references/ergonomics-and-clearances.md) to verify room-specific clearances (kitchen work triangle, wardrobe door swings + drawer pull-outs, bathroom sanitary spacing, TV viewing distance, and desk ergonomics).

---

## 3. Workflow B: Systematic Floor Plan Review & Audit

When the user provides a floor plan (image, screenshot, PDF, or dimensions) for evaluation:

1. Read [floor-plan-audit-checklist.md](./references/floor-plan-audit-checklist.md) and inspect the plan room-by-room across 7 audit dimensions:
   - **Entry & Circulation Flow** (dirty-zone isolation, hallway width, visual axis on entry).
   - **Door Swings & Collision Matrix** (door-to-door clashes, door-to-cabinet clashes, bathroom door outward opening safety).
   - **Ergonomic Clearances** (bedside passages, kitchen aisle width, toilet/shower clearances, dining pull-back zones).
   - **Storage & Built-In Niche Engineering** (wardrobe depth `>= 600–650 mm` clear internal niche, ceiling-height mezzanine splits, washer/dryer stacking tower `650×650 mm`).
   - **Daylight, Acoustics & Zoning** (quiet zones buffered from wet walls/elevators by wardrobes, desk perpendicular to windows).
   - **Plumbing & Wet-Zone Feasibility** (drain slopes `20–30 mm` per meter for `Ø50 mm` pipes and `15–20 mm` per meter for `Ø110 mm` toilets; wet zones stays within legal boundaries).
2. Present findings structured into three severity tiers:
   - **Blocking Issues**: Physical collisions, impossible clearances (`< 550 mm`), door/drawer traps, or plumbing violations that must be fixed.
   - **Recommended Adjustments**: Layout tweaks that significantly improve daily comfort, storage capacity, or ergonomics.
   - **Design Opportunities**: High-impact enhancements (e.g., recessed niches, flush-to-ceiling wardrobes, pocket doors, visual sightline upgrades).

---

## 4. Workflow C: Automated 2D Floorplan Generation & Collision Validation

To mathematically validate a room layout and generate a scaled 2D architectural floorplan (`SVG` + `HTML` preview):

1. Create a JSON room specification file containing:
   - `room`: `name`, `width` (mm), `depth` (mm), `wall_thickness` (mm, default `150`).
   - `openings`: doors (`wall`, `offset`, `width`, `swing`) and windows (`wall`, `offset`, `width`, `sill_height`).
   - `items`: furniture and fixtures (`id`, `label`, `category`, `x`, `y`, `width`, `depth`, `clearance_front`, `clearance_left`, `clearance_right`).
2. Run the validation and rendering script:
   ```powershell
   powershell.exe -NoProfile -ExecutionPolicy Bypass -File "C:\Users\lakre\.gemini\config\skills\interior-design\scripts\render_floorplan.ps1" -InputFile "path\to\room_spec.json" -OutputDir "path\to\output"
   ```
3. Check the console output from the script:
   - If the validator reports **out-of-bounds items**, **furniture overlaps**, or **door-swing / clearance collisions**, adjust the coordinates in the JSON spec and re-run until `0 collisions` are reported.
   - Share the generated scaled SVG/HTML diagram and dimension table with the user.

---

## 5. Workflow D: Multi-Layered Lighting (IES), Electrical & Material Palettes

When designing lighting schemes, electrical layouts, or color/material palettes, read [lighting-electrical-and-materials.md](./references/lighting-electrical-and-materials.md):

1. **Four-Layer Lighting Architecture (IES Standards)**:
   - Always combine **Ambient (General)** + **Task (Worktop / Reading / Mirror)** + **Accent (Wall washers, niches, 3D panels, art)** + **Night / Orientation** lighting on independent switch circuits.
   - Specify target **Lux** levels, **Color Temperature (`2700K`–`4000K`)**, **CRI (`>= 90`)**, and beam angles (`24°–36°` accent vs `60°+` general).
2. **Electrical & Switch Placement (European / Ukrainian Standards)**:
   - Provide exact mounting heights (`Z` from finished floor) and horizontal offsets from doors, beds, desks, and wet zones (`IP44 / IP65` protection zones in bathrooms).
3. **60-30-10 Color & Material Specification with Viyar / Egger / Kronospan Mapping**:
   - Define **60% Dominant** (walls, ceilings, large floor planes), **30% Secondary** (cabinetry facades, curtains, large upholstery), and **10% Accent** (hardware, feature stone/wood, decor).
   - Map all cabinetry and wall-panel surfaces directly to real **Egger** and **Kronospan** decors (with Viyar article codes) so the interior concept is immediately manufacturable.

---

## 6. Workflow E: SketchUp & AI Visualization

When generating interior renders, restyling room photos, or preparing SketchUp scenes for rendering, read [visualization-and-rendering.md](./references/visualization-and-rendering.md):

1. **SketchUp-to-AI Render Pipeline**:
   - Configure architectural camera views (two-point perspective, `1100–1300 mm` eye height, `24–35 mm` focal length) and build structured geometry-preserving prompts.
2. **Empty Room Virtual Staging & 2D-to-3D Cutaway Renders**:
   - Use structured architectural prompts (style archetype, material roughness/reflectance, IES lighting atmosphere, camera optics) and invoke the `image-generator` subagent when visual concepts are requested.

---

## 7. Workflow F: Ecosystem Integration (`viyar-furniture-designer` & `visual-shopping`)

This skill acts as the **architectural master planner** that feeds exact specifications into your specialized skills:

- **Handoff to `viyar-furniture-designer`**:
  - Once room layout and niche dimensions are finalized, pass exact outer bounding boxes (`W × D × H` in mm), construction deductions (`10–15 mm` installation clearance for built-in niches, ceiling split at `Z = 2100–2200 mm` for tall wardrobes + mezzanines), and selected Egger/Kronospan decor codes to `viyar-furniture-designer` to generate the 3D `.skp` model with ABF fasteners.
- **Handoff to `visual-shopping`**:
  - When sourcing freestanding furniture (chairs, sofas, pendants, sconces, rugs) from moodboards or reference renders, invoke `visual-shopping` to find matching in-stock items in online stores.

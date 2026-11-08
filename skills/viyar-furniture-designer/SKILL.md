---
name: viyar-furniture-designer
description: >-
  Parametric 3D furniture design in SketchUp (.skp) with ABF Solutions (ABF) engineering standards, plus separate
  on-demand conversion to Viyar (ViyarPro). Use this skill whenever the user asks to design furniture in SketchUp
  (TV unit, hallway table, bedroom wardrobe with mezzanine, bathroom vanity, shelving, drawers, kitchen, desk, or 3D
  milled wall panels), apply ABF board naming/layers/materials/CNC fasteners, or (separately, only when explicitly
  requested) convert a finished project into Viyar format.
---

# Viyar Furniture Designer & SketchUp ABF Engineering Skill

This skill encodes the complete furniture engineering, 3D modeling, and **ABF (`ABF Solutions`)** conventions extracted from the reference production projects in `D:\Project\SketshUp\design` (`hall_wall_table.skp`, `badroom_wardrobe_actual_fittings.skp`, `bathroom_1_lables.skp`, `bathroom2.skp`, `Drawer with fittings.skp`, `wardrobe.skp`, `wall_panel_3D_v2.skp`).

---

## 1. CRITICAL WORKFLOW RULE: SketchUp Design vs. Viyar Conversion

- **Never convert to Viyar format during SketchUp design**:
  - When the user asks to design, model, or modify furniture in SketchUp, your job is **exclusively** to design the 3D `.skp` model (`viyar_model.skp` + `project_data.json` / `sketchup_builder.rb` for live SketchUp sync).
  - Do **NOT** generate Viyar `.project`, `XML`, `CSV`, `TSV`, `DXF`, or `HTML` files as part of the SketchUp design workflow.
- **Viyar conversion is a separate, standalone operation**:
  - Converting a SketchUp / ABF project into Viyar format (`-Action export_viyar` or ABF `flatten.dxf` -> `.project`) must **only** be executed as a separate step when the user explicitly asks to export/convert an approved design for Viyar ordering.

---

## 2. Multi-Stage SketchUp Furniture Design Workflow (Learned from `D:\Project\SketshUp\design`)

Across real projects (`badroom_wardrobe.skp` -> `badroom_wardrobe_actual.skp` -> `badroom_wardrobe_actual_fittings.skp`, and `bathroom_1.skp` -> `bathroom_1_shelfs.skp` -> `bathroom_1_lables.skp`), furniture design in SketchUp progresses through three engineering stages:

### Stage 1: 3D Concept & Structural Layout
- Every furniture part (board) is modeled as a clean **6-face manifold `SUGroup` solid (`f=6`)** at its exact **finished dimensions (`Габаритний розмір`)** in millimeters (`+X` right, `+Y` depth back, `+Z` up).
- Functional visual layers may be used during layout exploration (`Right carcas`, `Pull-out module`, `Right facade`, `Left facade`, `Розміри` / `Measure`) alongside a human scale reference (`Heather.skp`) and room/wall context (`wall_panel_3D_v2.skp`, tile textures).

### Stage 2: ABF Board Registration, Layering & Viyar Material Binding
- **Top-Level Board Hierarchy**:
  - All manufactureable boards are placed as top-level `SUGroup`s in `model.entities` (or finalized assembly groups) and prefixed with two underscores and an index:
    `__<PartNumber>. <english section & part name>`
    *(e.g., `__1. tv table top`, `__2. side table left front`, `__29. right door`, `__53. right drawer 1 front`, `__65. left wardrobe left side`)*.
- **Standard Layer Assignment**:
  - Carcass panels, shelves, plinths, and drawer box sides/bottoms/backs -> `Layer0`.
  - Exterior doors and outer drawer fronts -> `Facade` (or `Фасад`).
  - Inner drawer fronts (inside behind hinged wardrobe doors) -> `Внутрішній фасад`.
  - Dimension callouts -> `Розміри` or `Measure`.
- **Viyar-Coded SketchUp Material Naming**:
  - Assign the material directly to the **board `SUGroup`** (leaving individual faces inside the group on the default material `''`).
  - Use one of the two official Viyar/ABF material naming formats so downstream converters recognize the Viyar catalog article and sheet thickness:
    1. **Format A (`<brand>_<code_name>_SU_$<ViyarArticle>`)**:
       - `kronospan_k009_gorih_tem_SU_$28863` — Kronospan K009 PW Dark Select Walnut 18 mm (`2800x2070x18`)
       - `kronospan_k009_gorih_tem_SU_$1` — Kronospan K009 38 mm spliced worktop (`bathroom_1.skp`)
       - `egger_u963_dіamant_sіriy_SU_$50394` — Egger U963 ST9 Diamond Grey / Anthracite 18 mm (`2800x2070x18`)
       - `egger_h1180_dub_galifaks_SU_$81190` — Egger H1180 ST37 Natural Halifax Oak 18 mm (`2800x2070x18`)
       - `egger_w1000_biliy_prem_SU_$12764` — Egger W1000 ST9 Premium White 18 mm (`2800x2070x18`)
       - `kronospan_hdf_3mm_SU_$148575` — Kronospan 3 mm HDF back panel (`2800x2070x3`)
    2. **Format B (`<Decor Name>/ARTICLE<ViyarArticle>`)**:
       - `ДСП 0162 PE/ARTICLE28848` — Kronospan 0162 PE Graphite Grey **16 mm** (`badroom_wardrobe_actual_fittings.skp` inner carcass & drawers)
       - `U 960 ST9/ARTICLE33941` — Egger U960 ST9 Onyx Grey **18 mm** (`badroom_wardrobe_actual_fittings.skp` outer facades)

### Stage 3: ABF Attributes, Edge Banding, Labels & 3D CNC Fasteners
- **Board Group `Dict[ABF]` Attributes**:
  - `board-index` (`int`), `is-board = true` (`bool`), `label-rotation = 0` (`int`), and optional `edge-band-types` array.
- **Face `Dict[ABF]` Attributes**:
  - On the primary CNC/labeled face (`SUFaceRef`): `is-cnced-face = true`, `is-labeled-face = true`.
  - On every edge-banded narrow face (`SUFaceRef`): `edge-band-id = 0` (`int`).
- **Child `_ABF_Label` Group**:
  - Placed on layer `ABF_Label` at the center of the board's labeled face (`is-label = true`, `label-height = 2`, `label-scale = 1.575`) with grain-direction arrow edges.
- **Child Fastener Groups & Viyar-Coded CNC Layers**:
  All fastener circles (`24` segments) and depth boxes reside in child `SUGroup`s on `Layer0`, while their internal edges are placed on exact Viyar-coded CNC layers:

  | Fastener / Operation | Child `SUGroup` Name | Edge CNC Layer Name | Radius / Size | `Dict[ABF]` Key Attributes |
  | :--- | :--- | :--- | :--- | :--- |
  | **Minifix Cam (18 mm board)** | `_ABF_minifixBolts` | `D15-DEPTH13.8-$61281` | `R = 7.5 mm` (`Ø15`, depth `13.8 mm`, `34 mm` from edge) | `is-minifix-part-b=true`, `setting-name="Мініфікс - Hettich (18 мм)"` |
  | **Minifix Cam (16 mm board)** | `_ABF_minifixBolts` | `D15-DEPTH13-$59042` | `R = 7.5 mm` (`Ø15`, depth `13.0 mm`, `34 mm` from edge) | `is-minifix-part-b=true`, `setting-name="Мініфікс - Hettich (16 мм)"` |
  | **Minifix Bolt (Face)** | `_ABF_minifixPlasticBase` | `D5-DEPTH11.5-$57722` | `R = 2.5 mm` (`Ø5`, depth `11.5 mm`) | `is-minifix-part-a=true`, `setting-name="Мініфікс - Hettich (18/16 мм)"` |
  | **Minifix Edge Bore** | `_ABF_sideDrill` + `_ABF_sideDrillDepth` | `Dside8` | `R = 4.0 mm` (`Ø8`, depth box `33x8 mm`) | `is-side-drill=true`, `is-side-drill-depth=true` |
  | **Confirmat Countersunk** | `_ABF_screwInside` | `D7-DEPTHF-CTRSINK-$44817` | `R = 3.5 mm` (`Ø7` through + countersink) | `is-minifix-part-a=true`, `is-screw=true`, `setting-name="Конфірмат зенкер Muller"` |
  | **Confirmat Non-Countersunk** | `_ABF_screwInside` | `D7-DEPTHF-$44817` | `R = 3.5 mm` (`Ø7` through, no countersink) | `is-minifix-part-a=true`, `is-screw=true`, `setting-name="Конфірмат Muller"` |
  | **Confirmat End-Grain Bore** | `_ABF_sideDrill` + `_ABF_sideDrillDepth` | `Dside4.5` | `R = 2.25 mm` (`Ø4.5`, depth box `34x4.5 mm`) | `is-minifix-part-b=true`, `is-side-drill=true` |
  | **Hinge Cup Ø35 (Muller 12mm)** | `_ABF_hingeCup` | `DEPTH12-ARTICLE97743` + `DEPTH3.0-ARTICLE46331` | `R = 17.5 mm` at `21.5 mm` + two `R = 1 mm` pilots at `±22.5, 31 mm` | `is-hinge-part-b=true`, `setting-name="завіса, внутрішня, 18 мм"` |
  | **Hinge Cup Ø35 (Slide-On 13mm)** | `_ABF_hingeCup` | `DEPTH13-ARTICLE96055` + `DEPTH3.0-ARTICLE46331` | `R = 17.5 mm` (`Ø35`, depth `13 mm`) + two `R = 1 mm` pilots | `is-hinge-part-b=true`, `setting-name="Slide-On 110°, внутрішня 18мм"` |
  | **Hinge Mounting Plate** | `_ABF_hingeMountingPlate` | `DEPTH3.0-ARTICLE46331` | Two `R = 1.0 mm` (`Ø2`, depth `3 mm`) spaced `32 mm` apart | `is-hinge-part-a=true` |
  | **Shelf Pin Hole Ø5** | `_ABF_minifixPlasticBase` | `D5-DEPTH11.5-$57722` | `R = 2.5 mm` (`Ø5`, depth `11.5 mm`) | `setting-name="Полицетримач оцинкований 5x16"` |
  | **Drawer Slide Euroscrew Ø5** | `_ABF_minifixPlasticBase` | `D5-DEPTH8.5-$52636` | `R = 2.5 mm` (`Ø5`, depth `8.5 mm`) | `setting-name="L400 (33) 52505 - Ящ"` or `"L450 (33) 52505 - Ящ"` |
  | **Tip-On / Push Latch Pilot Ø2** | `_ABF_minifixPlasticBase` | `D2-DEPTH3-$11361` / `D2-DEPTH3-$11361-$96090` | `R = 1.0 mm` (`Ø2`, depth `3 mm`) | Pilot holes for Tip-On / push-to-open latches |

---

## 3. Structural & Joinery Rules by Furniture Category

### A. Board Thickness Selection Rules (`16 mm` vs `18 mm` vs `26/38 mm` vs `3 mm`)
1. **18 mm LDSP/MDF**: Standard for exterior carcasses, visible sides, open shelves, TV consoles (`hall_wall_table.skp`), bathroom vanities (`bathroom2.skp`), and all exterior hinged doors/drawer fronts. Uses `D15-DEPTH13.8-$61281` Minifix cams (`Z-center = 9.0 mm`).
2. **16 mm LDSP (`ДСП 0162 PE/ARTICLE28848`)**: Used for high-density internal wardrobe carcasses, internal dividers, and drawer boxes (`badroom_wardrobe_actual_fittings.skp`) to reduce weight and cost while keeping exterior facades at `18 mm`. Uses `D15-DEPTH13-$59042` Minifix cams (`Z-center = 8.0 mm`).
3. **26 mm / 36 mm / 38 mm Thick Tops**:
   - Tall wardrobe top covers (`__21. left top top`, `__22. right top top` in `badroom_wardrobe_actual_fittings.skp`) use `26 mm` panels.
   - Bathroom vanity countertops (`__24. table top` in `bathroom_1.skp`) use `38 mm` spliced/postformed worktops (`kronospan_k009_gorih_tem_SU_$1`).
4. **3 mm HDF (`kronospan_hdf_3mm_SU_$148575`)**: Used for cabinet back panels and lightweight drawer bottoms (`hall_wall_table.skp`).

### B. Drawer Engineering Rules (`Drawer with fittings.skp`, `badroom_wardrobe`, `bathroom2.skp`, `hall_wall_table.skp`)
Two distinct drawer box constructions are used depending on load:
1. **Heavy-Duty Structural Chipboard Bottom (`Drawer with fittings.skp`, `badroom_wardrobe`, `bathroom2.skp`)**:
   - The drawer bottom is a **16 mm or 18 mm LDSP panel** placed **between** the left and right drawer sides (`W_bottom = W_drawer_inner`) and fastened through the drawer sides with Confirmats (`D7-DEPTHF-CTRSINK-$44817` + `Dside4.5`).
   - The drawer front attaches to the front edges of the left/right drawer sides **and** the structural bottom using **Minifix** (`D15` cam + `Dside8` + `D5` bolt).
   - **Inner Wardrobe Drawers (`Внутрішній фасад`)**:
     - Placed behind hinged doors on layer `Внутрішній фасад`.
     - Include a vertical **false side / spacer panel** (`__44, __45` `16x530x2184 mm`) on the hinge side so the pull-out drawer clears the `110°` door hinges!
     - Inner drawer fronts feature a top CNC cutout/bevel (`f=69` faces) as an integrated finger pull instead of protruding handles.
2. **Standard 3 mm HDF Bottom (`hall_wall_table.skp`)**:
   - Left/right drawer sides (`350x18x100 mm`) encapsulate the front/back box panels (`376x18x100 mm`), with a `3 mm` HDF bottom (`412x350x3 mm`) nailed/screwed underneath (`Z` below the box frame) and an outer facade attached via Minifix.
3. **Ball-Bearing Slide Clearance**:
   - Always deduct **`13.0 mm` per side (`26.0 mm` total)** between the carcass opening width and the outer width of the drawer box for ball-bearing slides (`L300 (37) 176548`, `L400 (33) 52505`, `L450 (33) 52505`).

### C. Tall Wardrobes with Mezzanines (`Антресоль` — `badroom_wardrobe_actual_fittings.skp` & `wardrobe.skp`)
- **Vertical Split Rule**:
  - Standard Viyar LDSP sheets are `2800x2070 mm` (maximum clean part length after perimeter trim is `<= 2770 mm`).
  - For ceiling-height wardrobes (`H = 2850..2910 mm`), always split the carcass vertically at **`Z = 2100..2200 mm`** into:
    1. **Plinth Base (`Цоколь`)**: `69..88 mm` high (`front`, `back`, `left`, `right`, and center support ribs under vertical dividers).
    2. **Main Lower Wardrobe**: From `Z = 88 mm` to `Z = 2200 mm` (`H_side = 2112 mm`), with full-height doors (`H_door = 2095..2121.5 mm`) using **3 hinges per door** (`Z = 100 mm`, `Z = H/2`, `Z = H - 100 mm`).
    3. **Stacked Upper Mezzanine (`top` / `Антресоль`)**: Independent upper carcass boxes (`Z = 2200..2877 mm`, `H = 650..677 mm`) resting on the main wardrobe top, with separate mezzanine doors (`H = 677..682 mm`) using **2 hinges per door** (`~94 mm` from top and bottom).
- **Fastener Pairing Rule**:
  - Hidden joints (plinths, top mezzanine floor/ceiling, drawer boxes, rear stretchers) -> **Confirmats** (`D7-DEPTHF-CTRSINK-$44817` + `Dside4.5`).
  - Visible exterior sides, face-mounted drawer fronts, and intermediate shelves where screw heads must not show on the outside -> **Minifix** (`D15` + `Dside8` + `D5`) from the hidden/underside face.

### D. Wall-Hung TV Console & Hallway Table (`hall_wall_table.skp`)
- **Top Panel Overhang & Inset Facades**:
  - Top panel (`__1. tv table top`) spans the full width and depth (`1800x400x18 mm`), resting on top of the vertical sides (`Z = 432..450 mm`), fastened from below via Minifix bolts (`D5-DEPTH11.5-$57722`).
  - Left/right outer sides (`__3, __4`) run from `Z = 0` to `Z = 432 mm` (`400x432x18 mm`).
  - Bottom panel (`__2. tv table down`, `1764x380x18 mm`) sits **between** the left and right sides (`X = 18..1782 mm`) and is **recessed `20 mm` from the front (`Y = 20..400 mm`)** so that the **inset** doors and drawer fronts (`Y = 0..18 mm`, with `2 mm` clearance gap) sit flush with the front of the side panels!

### E. Bathroom Vanities, Plumbing Cutouts & 3D Milled Panels (`bathroom_1_lables.skp`, `bathroom2.skp`, `wall_panel_3D_v2.skp`)
- Shelves inside plumbing/boiler cabinets (`bathroom_1_shelfs.skp`) and L-shaped joined boards (`__82. Union`) are modeled with actual 3D polygonal cutouts (`f = 19..210` faces) for pipes/valves while keeping `Dict[ABF]` board and face attributes intact.
- 3D decorative slatted wall panels (`wall_panel_3D_v2.skp`, `19x435x2211 mm`, `f = 203` faces) are modeled with true 3D milled grooves for visual rendering and CNC export.

---

## 4. CLI & MCP Execution Commands

### A. SketchUp 3D Design ONLY (Default — No Viyar Conversion)
```powershell
# Design a furniture project and generate ONLY the native SketchUp .skp model (viyar_model.skp)
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "D:\Project\Antigravity\Scratch\viyar-furniture-agent\src\viyar_cli.ps1" -Action design_skp -SpecFile "path\to\spec.json" -OutputDir "path\to\output"
```

### B. Separate On-Demand Viyar Conversion (Run ONLY When Explicitly Requested)
```powershell
# Convert a specification into ViyarPro XML, CSV, TSV, DXF, and HTML Studio (separate from SketchUp design)
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "D:\Project\Antigravity\Scratch\viyar-furniture-agent\src\viyar_cli.ps1" -Action export_viyar -SpecFile "path\to\spec.json" -OutputDir "path\to\output"
```

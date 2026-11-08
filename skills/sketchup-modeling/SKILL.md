---
name: sketchup-modeling
description: >-
  3D architectural and interior room modeling in SketchUp (.skp), SketchUp Ruby API plugin/script development, C API
  geometry generation, scene/camera/tag organization, and SketchUp-to-AI render preparation. Use this skill whenever
  the user asks to model rooms, walls, floors, openings, or architectural elements in SketchUp, write or debug SketchUp
  Ruby scripts (.rb), automate .skp scene/camera exports, or organize SketchUp tags/layers and materials.
---

# SketchUp 3D Architectural Modeling & Ruby API Engineering Skill

This skill governs 3D architectural room modeling, **SketchUp Ruby API** scripting, native `.skp` geometry rules, tag/layer organization, and scene/camera automation for SketchUp.

> **Note on Custom Cabinetry & Viyar/ABF Projects**: When modeling manufacturable board furniture, wardrobes, bathroom vanities, TV units, or 3D milled wall panels with CNC fasteners and Viyar material codes, always activate and follow `viyar-furniture-designer` alongside this skill.

---

## 1. Coordinate System, Units & Geometry Rules in SketchUp

1. **Millimeter Precision & Internal Units**:
   - User-facing dimensions are always specified in **millimeters (`mm`)**.
   - **CRITICAL Ruby API Rule**: SketchUp's internal unit is **inches (`1 inch = 25.4 mm`)**. In Ruby scripts, ALWAYS use `.mm` numeric extensions (e.g., `1800.mm`, `18.mm`) when passing coordinates or lengths to `Geom::Point3d.new` or `pushpull`.
   - **CRITICAL C API (`SketchUpAPI.dll`) Rule**: Raw C API structs (`SUPoint3D`, `SUVector3D`) take **inches** (`mm / 25.4`), never raw millimeters.
2. **Right-Handed 3D Axes**:
   - `+X` (Red axis): Width (Left to Right).
   - `+Y` (Green axis): Depth (Front to Back).
   - `+Z` (Blue axis): Height (Floor `Z = 0` upwards to Ceiling).
3. **Manifold Solids (`SUGroup` / `ComponentDefinition`)**:
   - Never leave loose, ungrouped faces or edges in the root `model.entities`.
   - Wrap every architectural element (wall segment, floor slab, ceiling, door assembly, window frame, furniture board) in its own named **Group** or **Component** immediately upon creation so geometry never sticks (`sticky geometry` prevention).
   - Ensure all 3D boxes are closed 6-face manifold solids with outward-facing front normals (`normal.dot(outward) > 0`). Notice that a horizontal face drawn on `Z = 0` in SketchUp defaults to normal `(0, 0, -1)` — call `face.reverse!` before `face.pushpull(height.mm)` if needed so extrusions go upward (`+Z`).

---

## 2. Standard Architectural Tag (Layer) Hierarchy

Organize every room or apartment `.skp` model using a clean, toggleable Tag/Layer structure so views and cross-sections can be exported cleanly:

| Tag / Layer Name | Contents | Default Visibility |
| :--- | :--- | :--- |
| `Layer0` (`Untagged`) | All raw edges and faces inside groups/components **MUST** stay on `Layer0` | Always ON |
| `Arch_Floor` | Floor slabs, tile/parquet finishes, plinths | ON |
| `Arch_Walls_Ext` | Exterior perimeter walls and window sills | ON |
| `Arch_Walls_Int` | Interior partitions and niches | ON |
| `Arch_Ceiling` | Ceiling slab, drop ceilings, curtain track recesses | Toggle OFF for top-down axonometric views |
| `Arch_Doors_Windows` | Door frames, door leaves, glazing, radiators | ON |
| `Furn_Carcass` / `Layer0` | Furniture carcasses and internal shelving | ON |
| `Facade` / `Фасад` | Exterior cabinet doors and outer drawer fronts | Toggle OFF to inspect cabinet interiors |
| `Внутрішній фасад` | Inner pull-out drawer fronts behind hinged doors | ON |
| `MEP_Lighting_Elec` | Light fixtures, magnetic tracks, switches, sockets, plumbing fixtures | ON |
| `Розміри` / `Measure` | Linear 3D dimension callouts (`SUDimensionLinear`) and text labels | Toggle ON for technical drawings |

---

## 3. SketchUp Ruby API Best Practices & Safety Rules

When generating or modifying `.rb` scripts for SketchUp (`Sketchup.active_model`):

1. **Undo Operation Wrapping (Mandatory)**:
   Always wrap model modifications in a single undoable transaction with `disable_ui = true` for performance:
   ```ruby
   model = Sketchup.active_model
   model.start_operation('Build Room Layout', true)
   begin
     # Create groups, entities, materials, and tags here
     model.commit_operation
   rescue => e
     model.abort_operation
     UI.messagebox("Error: #{e.message}")
     raise e
   end
   ```
2. **Safe Group Construction**:
   Create the group first, then add faces inside `group.entities` (never create faces in `model.entities` and try to group them afterward):
   ```ruby
   grp = model.active_entities.add_group
   grp.name = name
   pts = [
     Geom::Point3d.new(x.mm, y.mm, z.mm),
     Geom::Point3d.new((x + w).mm, y.mm, z.mm),
     Geom::Point3d.new((x + w).mm, (y + d).mm, z.mm),
     Geom::Point3d.new(x.mm, (y + d).mm, z.mm)
   ]
   face = grp.entities.add_face(pts)
   face.reverse! if face.normal.z < 0
   face.pushpull(h.mm)
   ```
3. **Modern UI Dialogs**:
   Always use `UI::HtmlDialog` (Chromium-based) instead of the deprecated `UI::WebDialog`.
4. **Entity Validity Checks**:
   In observers or iterative loops, always check `entity.valid? && !entity.deleted?` before reading properties.

---

## 4. Automated Scene & Camera Setup for Presentations and Renders

Create standardized SketchUp `Pages` (Scenes) in every architectural/furniture project:

1. `01_Iso_Overview` — Isometric 3D overview with `Arch_Ceiling` hidden.
2. `02_Front_Elevation` — Parallel projection front view with `Розміри` (`Measure`) layer enabled.
3. `03_Internal_Structure` — Front-angled view with `Facade` (`Фасад`) layer hidden to inspect internal shelving, drawers, and CNC fasteners.
4. `04_Top_Plan_Cut` — Top-down parallel projection horizontal section cut at `Z = 1200 mm`.
5. `05_Render_Cam_1` — Perspective camera at `Z = 1200 mm` eye height, `FOV = 65°` (`28 mm` lens), two-point perspective enabled for AI/V-Ray/Enscape rendering.

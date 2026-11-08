# SketchUp & AI Interior Visualization Workflow Reference

Use this guide when converting **SketchUp (`.skp`) models**, **2D floor plans**, or **empty room photos** into photorealistic architectural visualizations.

---

## 1. SketchUp Camera & Scene Preparation (`SketchUp Render Workflow`)

Before exporting a view from SketchUp or generating an AI render based on a 3D model, enforce these architectural photography standards:

1. **Two-Point Perspective (`Двоточкова перспектива`)**:
   - All vertical lines (walls, wardrobe edges, door frames) must remain **strictly vertical (`90°`)**. Never tilt the camera steeply up or down without enabling Two-Point Perspective.
2. **Camera Eye Height (`Z_camera`)**:
   - **Standard living / bedroom / kitchen view**: **`1100–1300 mm`** from finished floor (chest/seated-to-standing transition height used by professional architectural photographers like *ArchDaily / Dezeen*).
   - **Kitchen countertop / island focus**: **`1250–1350 mm`**.
   - **Small bathroom / WC view**: **`1150–1250 mm`** from doorway looking diagonally to maximize spatial depth.
3. **Focal Length (`Field of View`)**:
   - **Full Room Overview**: `24 mm – 28 mm` (`FOV 65°–75°`). Avoid wider than `22 mm` (`> 80°`) as it stretches furniture near frame edges.
   - **Vignette / Furniture Hero Shot** (e.g., hallway console, bathroom vanity, wardrobe detail): `35 mm – 50 mm` (`FOV 40°–54°`).

---

## 2. Structured Prompt Engineering for Photorealistic Interior Renders

When invoking `image-generator` or formulating prompts for external rendering tools (Midjourney, Flux, ControlNet / Krea / Magnific), always assemble prompts using this **6-Block Architectural Formula**:

```text
[1. Shot Type & Architectural Style] + [2. Room Geometry & Key Furniture Massing] +
[3. Exact Surface Materials & Finishes] + [4. Multi-Layered IES Lighting & Atmosphere] +
[5. Camera Optics & Composition] + [6. Realism & Quality Constraints]
```

### Prompt Template A: Photorealistic Room Visualization (Hero Shot)
> *"Architectural interior photography of a [Style: e.g., Warm Minimalist / Japandi] [Room Type: e.g., master bedroom / hallway / bathroom], two-point perspective with strictly vertical wall lines. [Geometry & Furniture: e.g., Full-height built-in matte onyx-grey wardrobe with upper mezzanine on the left, recessed open niche in dark walnut wood grain with integrated warm LED strip, floating wall-mounted console table]. [Materials: Ultra-matte Egger Diamond Grey cabinetry with 2mm shadow gaps, bookmatched dark walnut veneer, large-format warm travertine porcelain floor tiles, microcement walls]. [Lighting: Soft diffused late-afternoon natural daylight from left window combined with 3000K recessed linear LED cove lighting and subtle grazing light over vertical 3D fluted wall panels, physically accurate global illumination and soft contact shadows]. Shot on Phase One IQ4 150MP, 28mm tilt-shift lens at f/8, eye-level 1200mm height, ArchDaily editorial style, photorealistic textures, zero distortion."*

### Prompt Template B: 2D Floor Plan to 3D Axonometric Cutaway Dollhouse
> *"Isometric 3D architectural cutaway floor plan rendering of a modern [Dimensions: e.g., 65 sqm two-bedroom apartment / 4.2m × 3.6m bedroom], top-down 45-degree axonometric view with 1.2m cutaway exterior walls in clean matte white. Accurate furniture proportions showing [list zones & furniture: e.g., queen bed with two nightstands, ceiling-height built-in wardrobe along north wall, work desk near window]. Warm oak parquet flooring in living zones, large-format grey porcelain tile in hallway and bathroom, soft studio daylight with crisp ambient occlusion shadows on a clean neutral background, architectural presentation board quality."*

### Prompt Template C: Layer-by-Layer Empty Room Virtual Staging
When restyling an existing room photo or empty shell:
1. **Preserve Structural Anchors**: Explicitly lock window positions, ceiling beams, radiator recesses, and doorway openings.
2. **Specify Envelope First**: Flooring material (herringbone oak / microcement / porcelain tile) + wall LRV and finish.
3. **Add Built-In Millwork**: Specify built-in cabinetry flush with niches.
4. **Add Freestanding Pieces & Soft Furnishings**: Sofa/bed, rug (must extend `200–300 mm` under sofa/bed front legs), curtains from ceiling track to floor (`-10 mm` kiss clearance).

---

## 3. Visual QA Checklist for Generated Renders

Before presenting a generated visualization as a final design reference, verify:
- **Ergonomic Plausibility**: Do doors and drawers have physical space to open? Are switches and fixtures at realistic heights?
- **Material Consistency**: Does wood grain run vertically on tall doors (`H > 2000 mm`) and horizontally on wide drawer fronts or worktops where intended?
- **Lighting Realism**: Are LED strips concealed inside profiles/recesses (visible glow without exposed diode dots)?

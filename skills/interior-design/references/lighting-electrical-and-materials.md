# Multi-Layered Lighting (IES), Electrical & Material Specification Reference

---

## 1. Multi-Layered Lighting Architecture (IES Standards)

Every room must combine **at least three independent lighting layers** wired to separate switch channels/dimmers:

1. **Layer 1 — Ambient (General Illumination)**:
   - Uniform, glare-free base light across the room (magnetic track flood modules, recessed anti-glare downlights with deep baffle `UGR < 16`, or indirect ceiling perimeter coves).
2. **Layer 2 — Task (Functional Work Illumination)**:
   - High-intensity, shadow-free light directed onto work surfaces (under-cabinet LED strips over kitchen worktop, vertical side lighting at bathroom mirror, focused reading sconces at bedside, desk lamp).
3. **Layer 3 — Accent (Architectural Depth & Mood)**:
   - Grazing light across textured walls, 3D milled panels (`wall_panel_3D_v2.skp`), curtain pocket backlighting, open wardrobe display shelves, and dining table pendant cluster.
4. **Layer 4 — Night / Orientation Lighting**:
   - Low-glare `2700K` LED strips under floating bathroom vanities, hallway shoe benches, or motion-triggered (`PIR`) plinth nightlights (`10–15%` output).

### Target Illuminance (`Lux`), Color Temperature (`K`) & CRI by Room

| Room / Zone | Target Illuminance (`Lux`) | Color Temp (`Kelvin`) | Min CRI (`Ra`) | Recommended Fixture Strategy |
| :--- | :--- | :--- | :--- | :--- |
| **Entry Hallway** | `150–200 lx` (Floor) / `300 lx` (Mirror) | `3000K` | `>= 90` | Recessed anti-glare spots + vertical mirror backlight + shoe shelf LED |
| **Living Room** | `150–200 lx` (Ambient) / `300 lx` (Reading) | `2700K–3000K` | `>= 90` | Magnetic track system + curtain cove LED + TV wall / console accent |
| **Kitchen Worktop** | `400–500 lx` (Task) / `200 lx` (General) | `3000K–3500K` | `>= 90` | Continuous recessed LED profile under upper cabinets (`12–15 W/m`) + ceiling tracks |
| **Dining Table** | `200–300 lx` on tabletop | `2700K–3000K` | `>= 95` | Low-hung pendant (`700–800 mm` above tabletop) on dimmer |
| **Bedroom** | `100–150 lx` (General) / `250 lx` (Bedside) | `2700K` (Warm) | `>= 90` | Perimeter/curtain indirect light + directional reading sconces + wardrobe internal LED |
| **Bathroom / Vanity** | `200 lx` (General) / `400–500 lx` (Face) | `3000K–3500K` | `>= 90` | `IP44/IP65` recessed spots + symmetric mirror face light + floating vanity under-glow |
| **Home Office / Desk** | `400–500 lx` on work surface | `3500K–4000K` | `>= 90` | Asymmetric monitor lightbar / task lamp + soft background wall wash |

---

## 2. Electrical Outlets, Switches & Mounting Heights (`Z` from Finished Floor)

All coordinates are specified from the **finished floor level (`FFL / Чиста підлога`)** to the center of the electrical box:

| Zone / Device | Height (`Z`, `mm`) | Horizontal Offset / Rule |
| :--- | :--- | :--- |
| **Wall Switches (Main Rooms)** | `850–900` | `150 mm` from door frame on the **door handle side** |
| **General Wall Sockets** | `250–300` | At least `150 mm` from door/window frames |
| **Bedside Switches & Sockets** | `600–650` | `100–150 mm` above bedside table top (`W_bed/2 + 250..350 mm` from bed centerline); include **2-way pass-through switch (`прохідний вимикач`)** for main bedroom light |
| **Bedside Reading Sconce Output** | `1150–1300` | Centered above bedside table or integrated into headboard panel |
| **Work Desk Sockets**: | | |
| — Under desk (PC tower, UPS, router) | `300` | `3–4` sockets + `RJ45 Ethernet` port |
| — Above desk (Monitor, laptop charger) | `850–900` | `100–150 mm` above `750 mm` desktop |
| **TV Media Zone (`Тумба під ТВ`)**: | | |
| — Inside/behind lower TV console | `300–350` | Power + `RJ45` + `50 mm` concealed cable channel (`кабель-канал` inside wall) |
| — Behind wall-mounted TV | `1100–1200` | Hidden behind TV bracket (`2–3` sockets + `RJ45` + cable channel exit) |
| **Kitchen Appliances (`Кухня`)**: | | |
| — Plinth Zone (Fridge, Dishwasher, Oven) | `50–80` | Inside plinth (`Цоколь`) so appliances push flush against the wall |
| — Worktop Backsplash Sockets | `1050–1150` | `150–200 mm` above worktop; minimum `600 mm` away from sink edge and hob |
| — Built-in Hood (`Витяжка`) | `2000–2200` | Above upper cabinet top or offset `50 mm` from hood duct axis |
| — Under-Cabinet LED Driver Output | `2150–2250` | Hidden on top of upper cabinets or inside utility shelf |
| **Bathroom (`Санвузол` — `IP44` Criteria)**: | | |
| — Vanity Sockets (Hairdryer, Shaver, Brush) | `950–1050` | At least `600 mm` horizontally from shower/bathtub edge (`Zone 3`) |
| — Mirror Backlight & Anti-Fog Cable | `1400–1600` | Behind mirror center |
| — Washer & Dryer Sockets | `300–1000` | On adjacent accessible side wall inside utility cabinet (`IP44` with hinged lid) |
| — Electric Towel Warmer (`Рушникосушка`) | `900–1100` | Concealed connection box matching warmer bottom bracket |

---

## 3. Color Theory (60-30-10) & Viyar / Egger / Kronospan Material Mapping

Always structure interior color and material palettes using the **60-30-10 Balance Rule** and **LRV (Light Reflectance Value)**:

1. **60% Dominant Envelope (High LRV `60–85%`)**:
   - Walls, ceilings, and main flooring that establish spatial brightness.
2. **30% Secondary Architectural Mass (Medium LRV `25–55%`)**:
   - Built-in wardrobes, kitchen facades, wood textures, curtains, and large sofa upholstery.
3. **10% Accent & Tactile Contrast (Low or High Contrast LRV `5–20%`)**:
   - Matte black/brushed bronze hardware, stone veins, dark walnut niches, 3D slatted wall panels, and decorative lighting.

### Production-Ready Furniture Decor Palettes (Egger & Kronospan / Viyar Codes)

When specifying custom furniture or wall panels, recommend these verified **Viyar / ABF** board decors so the design translates directly into `viyar-furniture-designer`:

| Interior Style Archetype | Primary Facade / Carcass Decor | Wood / Warm Secondary Decor | Accent / Contrast Decor |
| :--- | :--- | :--- | :--- |
| **Warm Minimalism / Japandi** | **Egger W1000 ST9** Premium White (`egger_w1000_biliy_prem_SU_$12764`) or Cashmere Grey (`U702 ST9`) | **Egger H1180 ST37** Natural Halifax Oak (`egger_h1180_dub_galifaks_SU_$81190`) | **Kronospan K009 PW** Dark Select Walnut (`kronospan_k009_gorih_tem_SU_$28863`) |
| **Contemporary Dark / Architectural** | **Egger U963 ST9** Diamond Grey / Anthracite (`egger_u963_dіamant_sіriy_SU_$50394`) | **Kronospan K009 PW** Dark Select Walnut (`kronospan_k009_gorih_tem_SU_$28863`) | **Kronospan 0162 PE** Graphite Grey 16 mm inner carcasses (`ДСП 0162 PE/ARTICLE28848`) |
| **Soft Modern / Monochrome** | **Egger U960 ST9** Onyx Grey (`U 960 ST9/ARTICLE33941`) | **Egger H1180 ST37** Natural Halifax Oak (`egger_h1180_dub_galifaks_SU_$81190`) | **Egger W1000 ST9** Premium White (`egger_w1000_biliy_prem_SU_$12764`) |
| **Scandinavian Light** | **Egger W1000 ST9** Premium White (`egger_w1000_biliy_prem_SU_$12764`) | **Egger H1180 ST37** Natural Halifax Oak (`egger_h1180_dub_galifaks_SU_$81190`) | **Egger U963 ST9** Diamond Grey (`egger_u963_dіamant_sіriy_SU_$50394`) |

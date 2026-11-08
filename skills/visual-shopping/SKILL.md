---
name: visual-shopping
description: >-
  Personalized search for ONLY in-stock items in online stores using a reference image, screenshot, or clipboard photo,
  with automatic matching against the user's body profile parameters (default: height 178 cm, weight 65 kg,
  clothing size M / EU 38 / UA 46, shoe size 41; profile is customizable). Use this skill IMMEDIATELY whenever
  the user asks to find an item from an image/screenshot or to view/update their profile parameters.
---

# Visual Shopping v2.0 — Personalized Computer Vision Search for In-Stock Items

All tools, the profile database, and the Python script are located in `D:\Project\Antigravity\Scratch\visual-shopping`.

---

## 1. User Personal Profile (`profiles.json`)

By default, the active **`default`** profile is configured with the following parameters:
- **Height:** `178 cm` *(accounts for adequate sleeve length, elongated silhouette, and pants inseam `L32–L34`)*
- **Weight:** `65 kg`
- **Clothing Size:** typically **`M`** *(compatible size labels: `M`, `S/M`, `M/L`, `EU 38`, `UA 46`, `42-46`, `44-48`, `One Size`; automatically filters out incompatible single sizes such as `XS`, `XXS`, `XXL`)*
- **Shoe Size:** **`41`** *(insole length ~`26.5 cm`)*

### Profile Management Commands:
```powershell
# Show current profile
& "D:\Project\Antigravity\Scratch\visual-shopping\.venv\Scripts\python.exe" "D:\Project\Antigravity\Scratch\visual-shopping\visual_shop.py" profile show

# Update current profile parameters
& "D:\Project\Antigravity\Scratch\visual-shopping\.venv\Scripts\python.exe" "D:\Project\Antigravity\Scratch\visual-shopping\visual_shop.py" profile set --height 178 --weight 65 --size M --shoe-size 41
```

---

## 2. How Visual Search v2.0 Works (Computer Vision + Stock Verification)

1. **Direct Visual Parsing (Bing Images `iusc` + DDGS):** Collects 80–120 real product cards with photos from Ukrainian online stores (including automatic bypass of `challenge_passed` protection on Horoshop-based stores).
2. **Computer Vision Similarity Scoring (`visual_score` 0..100%):**
   - Downloads the preview image of each candidate and compares it against the cropped reference across 4 criteria:
     - Fabric color tone and warmth excluding studio background (`42%`)
     - 64-bin item color histogram (`23%`)
     - 6x6 spatial grid of silhouette and luminance (`20%`)
     - Texture smoothness vs. raised patterns/cables/prints (`15%`)
3. **Structural Feature Filtering (`--must-have` and `--exclude-words`):**
   - Pass `--must-have` for mandatory structural details (e.g., for a turtleneck sweater: `--must-have "горл|комір|гольф|хомут|водолазк"` to filter out crewnecks and V-necks).
   - The script automatically filters out unwanted patterns (cable-knit, stripes, sequins, prints, zippers, vests) if they are absent in the reference image.
4. **Strict In-Stock and Size (`M` / `41`) Filtering:**
   - Inspects `JSON-LD Product`, `Schema.org Microdata (itemprop="availability")`, and `OpenGraph`. Rejects all out-of-stock items and general category pages without a specific product.
5. **Agent Visual Self-Verification (`visual_comparison_<timestamp>.jpg`):**
   - The script generates a collage at `output/visual_comparison_<timestamp>.jpg` showing the reference image alongside the top matched products.
   - **MANDATORY: Read this image file via `read_file` before replying to the user** to visually verify that the recommended products genuinely resemble the reference item!

---

## 3. Pipeline Execution Command

```powershell
& "D:\Project\Antigravity\Scratch\visual-shopping\.venv\Scripts\python.exe" "D:\Project\Antigravity\Scratch\visual-shopping\visual_shop.py" pipeline `
  --image "<path_to_file_or_URL>" `
  --crop "<ymin,xmin,ymax,xmax>" `
  --item-name "<Item Name>" `
  --item-type <auto|clothing|shoes|jeans|other> `
  --must-have "<regex_for_mandatory_cut_feature>" `
  --queries "<Query 1>" "<Query 2>" "<Query 3>" `
  --category <fashion|furniture|electronics|all> `
  --region <ua|global>
```

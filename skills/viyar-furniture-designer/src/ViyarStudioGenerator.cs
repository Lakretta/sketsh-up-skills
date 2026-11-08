using System;
using System.IO;
using System.Text;

namespace ViyarAgent
{
    public static class ViyarStudioGenerator
    {
        public static string GenerateHtmlStudio(FurnitureProject proj, string outputDir)
        {
            string jsonPath = Path.Combine(outputDir, "project_data.json");
            if (!File.Exists(jsonPath))
            {
                ViyarBuilder.WriteProjectJson(proj, jsonPath);
            }
            string projectJson = File.ReadAllText(jsonPath, Encoding.UTF8);
            string htmlPath = Path.Combine(outputDir, "viyar_studio.html");

            string html = GetHtmlTemplate().Replace("__PROJECT_JSON_PLACEHOLDER__", projectJson);
            File.WriteAllText(htmlPath, html, new UTF8Encoding(true));
            return htmlPath;
        }

        private static string GetHtmlTemplate()
        {
            return @"<!DOCTYPE html>
<html lang=""uk"">
<head>
<meta charset=""UTF-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
<title>Viyar AI Agent Studio — 3D Модель, Креслення ЧПК та Розкрій ВіЯр</title>
<style>
  :root {
    --bg: #0f172a;
    --panel: #1e293b;
    --panel-alt: #334155;
    --text: #f8fafc;
    --muted: #94a3b8;
    --accent: #38bdf8;
    --accent-2: #f97316;
    --success: #22c55e;
    --border: #334155;
  }
  * { box-sizing: border-box; margin: 0; padding: 0; }
  body {
    font-family: 'Segoe UI', system-ui, -apple-system, sans-serif;
    background: var(--bg);
    color: var(--text);
    display: flex;
    flex-direction: column;
    height: 100vh;
    overflow: hidden;
  }
  header {
    background: #0b1120;
    border-bottom: 1px solid var(--border);
    padding: 12px 20px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
  }
  .brand {
    display: flex;
    align-items: center;
    gap: 12px;
  }
  .brand-badge {
    background: linear-gradient(135deg, #f97316, #ea580c);
    color: #fff;
    font-weight: 800;
    padding: 6px 12px;
    border-radius: 8px;
    font-size: 14px;
    letter-spacing: 0.5px;
  }
  .brand h1 { font-size: 18px; font-weight: 700; }
  .stats-bar {
    display: flex;
    gap: 18px;
    font-size: 13px;
    color: var(--muted);
  }
  .stat-pill {
    background: var(--panel);
    border: 1px solid var(--border);
    padding: 6px 12px;
    border-radius: 6px;
  }
  .stat-pill b { color: var(--accent); font-size: 14px; }
  .tabs {
    display: flex;
    background: var(--panel);
    border-bottom: 1px solid var(--border);
    padding: 0 20px;
    gap: 4px;
  }
  .tab-btn {
    background: transparent;
    color: var(--muted);
    border: none;
    padding: 12px 18px;
    font-size: 14px;
    font-weight: 600;
    cursor: pointer;
    border-bottom: 3px solid transparent;
    transition: all 0.15s;
  }
  .tab-btn:hover { color: var(--text); background: rgba(255,255,255,0.03); }
  .tab-btn.active {
    color: var(--accent);
    border-bottom-color: var(--accent);
    background: rgba(56,189,248,0.08);
  }
  .tab-content {
    display: none;
    flex: 1;
    overflow: hidden;
  }
  .tab-content.active {
    display: flex;
  }
  /* 3D Viewer Layout */
  .viewer-layout {
    display: flex;
    width: 100%;
    height: 100%;
  }
  .canvas-wrap {
    flex: 1;
    position: relative;
    background: radial-gradient(circle at center, #1e293b 0%, #0f172a 100%);
  }
  #canvas3d {
    width: 100%;
    height: 100%;
    display: block;
    cursor: grab;
  }
  #canvas3d:active { cursor: grabbing; }
  .controls-overlay {
    position: absolute;
    top: 16px;
    left: 16px;
    background: rgba(15, 23, 42, 0.88);
    backdrop-filter: blur(8px);
    border: 1px solid var(--border);
    border-radius: 10px;
    padding: 14px;
    width: 300px;
    display: flex;
    flex-direction: column;
    gap: 10px;
  }
  .ctrl-row {
    display: flex;
    flex-direction: column;
    gap: 4px;
    font-size: 12px;
  }
  .ctrl-row label {
    display: flex;
    justify-content: space-between;
    color: var(--muted);
  }
  input[type=range] { width: 100%; accent-color: var(--accent); }
  .btn-row { display: flex; gap: 6px; flex-wrap: wrap; }
  .btn {
    background: var(--panel-alt);
    color: var(--text);
    border: 1px solid #475569;
    padding: 6px 11px;
    border-radius: 6px;
    font-size: 12px;
    font-weight: 600;
    cursor: pointer;
  }
  .btn:hover { background: #475569; }
  .btn-primary {
    background: #0284c7;
    border-color: #38bdf8;
    color: #fff;
  }
  .btn-primary:hover { background: #0369a1; }
  .sidebar-right {
    width: 360px;
    background: var(--panel);
    border-left: 1px solid var(--border);
    padding: 16px;
    overflow-y: auto;
    display: flex;
    flex-direction: column;
    gap: 14px;
  }
  .card {
    background: rgba(15,23,42,0.6);
    border: 1px solid var(--border);
    border-radius: 8px;
    padding: 12px;
  }
  .card h3 {
    font-size: 13px;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    color: var(--accent);
    margin-bottom: 8px;
  }
  /* 2D Blueprint Tab */
  .blueprint-layout {
    display: flex;
    width: 100%;
    height: 100%;
  }
  .part-list-sidebar {
    width: 320px;
    background: var(--panel);
    border-right: 1px solid var(--border);
    overflow-y: auto;
    padding: 10px;
  }
  .part-item {
    padding: 9px 12px;
    border-radius: 6px;
    cursor: pointer;
    border: 1px solid transparent;
    margin-bottom: 4px;
    font-size: 13px;
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
  .part-item:hover { background: rgba(255,255,255,0.05); }
  .part-item.active {
    background: rgba(56,189,248,0.14);
    border-color: var(--accent);
  }
  .blueprint-main {
    flex: 1;
    padding: 20px;
    overflow-y: auto;
    display: flex;
    flex-direction: column;
    gap: 16px;
  }
  .svg-container {
    background: #0b1120;
    border: 1px solid var(--border);
    border-radius: 10px;
    padding: 16px;
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 430px;
  }
  /* Tables */
  .table-page {
    flex: 1;
    padding: 20px;
    overflow-y: auto;
  }
  table {
    width: 100%;
    border-collapse: collapse;
    font-size: 13px;
    background: var(--panel);
    border-radius: 8px;
    overflow: hidden;
  }
  th, td {
    padding: 9px 12px;
    text-align: left;
    border-bottom: 1px solid var(--border);
  }
  th {
    background: #0b1120;
    color: var(--accent);
    font-weight: 600;
    position: sticky;
    top: 0;
  }
  tr:hover td { background: rgba(255,255,255,0.03); }
  .badge-edge {
    display: inline-block;
    padding: 2px 6px;
    border-radius: 4px;
    font-size: 11px;
    background: rgba(249,115,22,0.18);
    color: #fdba74;
    border: 1px solid rgba(249,115,22,0.4);
  }
  .badge-edge.thin {
    background: rgba(56,189,248,0.15);
    color: #7dd3fc;
    border-color: rgba(56,189,248,0.35);
  }
  .sheet-card {
    background: var(--panel);
    border: 1px solid var(--border);
    border-radius: 10px;
    padding: 16px;
    margin-bottom: 20px;
  }
</style>
</head>
<body>

<header>
  <div class=""brand"">
    <span class=""brand-badge"">ВІЯР AI STUDIO</span>
    <div>
      <h1 id=""projTitle"">Проект Меблів</h1>
      <div style=""font-size:12px;color:var(--muted)"">Сумісно з SketchUp (ABF / Gava / OpenCutList) та ViyarPro</div>
    </div>
  </div>
  <div class=""stats-bar"">
    <div class=""stat-pill"">Модулів: <b id=""statModules"">0</b></div>
    <div class=""stat-pill"">Деталей: <b id=""statPanels"">0</b></div>
    <div class=""stat-pill"">Площа: <b id=""statArea"">0</b> м²</div>
    <div class=""stat-pill"">Крайка: <b id=""statEdge"">0</b> м.п.</div>
    <div class=""stat-pill"">Отворів ЧПК: <b id=""statHoles"">0</b></div>
    <div class=""stat-pill"">Оцінка ВіЯр: <b id=""statCost"" style=""color:#22c55e"">0 грн</b></div>
  </div>
</header>

<div class=""tabs"">
  <button class=""tab-btn active"" onclick=""switchTab('tab3d', this)"">🧊 3D Модель (SketchUp Preview)</button>
  <button class=""tab-btn"" onclick=""switchTab('tab2d', this)"">📐 2D Креслення та Присадка ЧПК</button>
  <button class=""tab-btn"" onclick=""switchTab('tabCutlist', this)"">📋 Специфікація та Експорт ViyarPro</button>
  <button class=""tab-btn"" onclick=""switchTab('tabNesting', this)"">🧩 Карти Розкрою Листів та Фурнітура</button>
</div>

<!-- TAB 1: 3D MODEL -->
<div id=""tab3d"" class=""tab-content active"">
  <div class=""viewer-layout"">
    <div class=""canvas-wrap"">
      <canvas id=""canvas3d""></canvas>
      <div class=""controls-overlay"">
        <div style=""font-weight:700;font-size:13px;color:var(--accent)"">Керування 3D Сценою</div>
        <div class=""ctrl-row"">
          <label><span>Вибух-схема (Рознесення деталей)</span><span id=""lblExplode"">0%</span></label>
          <input type=""range"" id=""rngExplode"" min=""0"" max=""100"" value=""0"" oninput=""update3DParams()"">
        </div>
        <div class=""ctrl-row"">
          <label><span>Відкрити фасади / шухляди</span><span id=""lblOpen"">0%</span></label>
          <input type=""range"" id=""rngOpen"" min=""0"" max=""100"" value=""0"" oninput=""update3DParams()"">
        </div>
        <div class=""btn-row"">
          <button class=""btn"" onclick=""toggleXray()"">🔍 Рентген / Присадка</button>
          <button class=""btn"" onclick=""toggleFacades()"">🚪 Сховати фасади</button>
          <button class=""btn"" onclick=""resetCamera()"">🎯 Скинути камеру</button>
        </div>
        <div style=""font-size:11px;color:var(--muted);line-height:1.4"">
          💡 ЛКМ + рух: обертання | Колесо: масштаб | Клік по деталі: вибір та огляд присадки
        </div>
      </div>
    </div>
    <div class=""sidebar-right"">
      <div class=""card"">
        <h3>Обрана деталь у 3D</h3>
        <div id=""selectedPartBox"" style=""font-size:13px;line-height:1.6;color:var(--muted)"">
          Натисніть на будь-яку деталь у 3D моделі, щоб переглянути її точні габарити, крайкування та отвори ЧПК.
        </div>
      </div>
      <div class=""card"">
        <h3>Модулі проекту</h3>
        <div id=""modulesListBox"" style=""display:flex;flex-direction:column;gap:6px;font-size:13px;""></div>
      </div>
      <div class=""card"">
        <h3>Швидкий експорт</h3>
        <div style=""display:flex;flex-direction:column;gap:8px;"">
          <button class=""btn btn-primary"" onclick=""copyViyarClipboard()"">📋 Скопіювати таблицю для ViyarPro</button>
          <button class=""btn"" onclick=""downloadJsonFile()"">⬇️ Завантажити JSON проекту (для SketchUp)</button>
        </div>
      </div>
    </div>
  </div>
</div>

<!-- TAB 2: 2D CNC BLUEPRINTS -->
<div id=""tab2d"" class=""tab-content"">
  <div class=""blueprint-layout"">
    <div class=""part-list-sidebar"" id=""blueprintPartList""></div>
    <div class=""blueprint-main"">
      <div style=""display:flex;justify-content:space-between;align-items:center;"">
        <div>
          <h2 id=""bpTitle"" style=""font-size:18px;"">Креслення деталі</h2>
          <div id=""bpSubtitle"" style=""font-size:13px;color:var(--muted)""></div>
        </div>
        <div id=""bpEdgesInfo"" style=""display:flex;gap:8px;flex-wrap:wrap;""></div>
      </div>
      <div class=""svg-container"" id=""bpSvgBox""></div>
      <div class=""card"">
        <h3>Таблиця свердління (Присадка) та Фрезерування (Пази)</h3>
        <div id=""bpOpsTable""></div>
      </div>
    </div>
  </div>
</div>

<!-- TAB 3: CUTLIST & VIYARPRO EXPORT -->
<div id=""tabCutlist"" class=""tab-content"">
  <div class=""table-page"">
    <div style=""display:flex;justify-content:space-between;align-items:center;margin-bottom:14px;"">
      <h2>Повна специфікація деталей (Формат ВіЯр Про)</h2>
      <div class=""btn-row"">
        <button class=""btn btn-primary"" onclick=""copyViyarClipboard()"">📋 Скопіювати TSV у буфер обміну</button>
      </div>
    </div>
    <table>
      <thead>
        <tr>
          <th>№</th>
          <th>Модуль</th>
          <th>Назва деталі</th>
          <th>Довжина (L), мм</th>
          <th>Ширина (W), мм</th>
          <th>Товщ., мм</th>
          <th>Текстура</th>
          <th>Матеріал (Артикул ВіЯр)</th>
          <th>Крайка L1 (Верх)</th>
          <th>Крайка L2 (Низ)</th>
          <th>Крайка W1 (Ліво)</th>
          <th>Крайка W2 (Право)</th>
          <th>ЧПК Обробки</th>
          <th>Креслення</th>
        </tr>
      </thead>
      <tbody id=""cutlistTbody""></tbody>
    </table>
  </div>
</div>

<!-- TAB 4: SHEET NESTING & HARDWARE BOM -->
<div id=""tabNesting"" class=""tab-content"">
  <div class=""table-page"">
    <h2 style=""margin-bottom:12px;"">Специфікація фурнітури та комплектуючих ВіЯр</h2>
    <table style=""margin-bottom:28px;"">
      <thead>
        <tr>
          <th>Артикул ВіЯр</th>
          <th>Найменування фурнітури</th>
          <th>Кількість</th>
          <th>Од. вим.</th>
          <th>Ціна за од., грн</th>
          <th>Сума, грн</th>
        </tr>
      </thead>
      <tbody id=""hwTbody""></tbody>
    </table>

    <h2 style=""margin-bottom:12px;"">Карти розкрою плитних матеріалів (з урахуванням пропилу 4 мм та обрізки 15 мм)</h2>
    <div id=""sheetsContainer""></div>
  </div>
</div>

<script>
const PROJECT = __PROJECT_JSON_PLACEHOLDER__;

// Populate Header Stats
document.getElementById('projTitle').textContent = PROJECT.project_name;
document.getElementById('statModules').textContent = PROJECT.modules.length;
document.getElementById('statPanels').textContent = PROJECT.panels.length;
document.getElementById('statArea').textContent = PROJECT.total_area_sqm;
document.getElementById('statEdge').textContent = PROJECT.total_edge_m;
document.getElementById('statHoles').textContent = PROJECT.total_drilling_holes;
document.getElementById('statCost').textContent = PROJECT.estimated_cost_uah.toLocaleString('uk-UA') + ' грн';

function switchTab(tabId, btn) {
  document.querySelectorAll('.tab-content').forEach(el => el.classList.remove('active'));
  document.querySelectorAll('.tab-btn').forEach(el => el.classList.remove('active'));
  document.getElementById(tabId).classList.add('active');
  btn.classList.add('active');
  if (tabId === 'tab3d') requestAnimationFrame(render3D);
}

// Populate Modules List
const modBox = document.getElementById('modulesListBox');
PROJECT.modules.forEach(m => {
  const d = document.createElement('div');
  d.style.cssText = 'padding:8px;background:rgba(255,255,255,0.04);border-radius:6px;border:1px solid #334155;';
  d.innerHTML = `<b>${m.name}</b> <span style=""color:#38bdf8"">(${m.width_mm}×${m.height_mm}×${m.depth_mm} мм)</span><br><span style=""font-size:11px;color:#94a3b8"">Тип: ${m.type} | Позиція X:${m.origin_x_mm} Z:${m.origin_z_mm}</span>`;
  modBox.appendChild(d);
});

// Populate Cutlist Table
const cutTbody = document.getElementById('cutlistTbody');
function formatEdgeBadge(code) {
  if (!code) return '<span style=""color:#475569"">—</span>';
  const isThick = code.startsWith('2.0') || code.startsWith('1.0');
  return `<span class=""badge-edge ${isThick ? '' : 'thin'}"">${code}</span>`;
}
PROJECT.panels.forEach((p, idx) => {
  const tr = document.createElement('tr');
  tr.innerHTML = `
    <td><b>${p.part_number}</b></td>
    <td>${p.module_name}</td>
    <td><b>${p.name}</b></td>
    <td>${p.length_mm}</td>
    <td>${p.width_mm}</td>
    <td>${p.thickness_mm}</td>
    <td>${p.has_grain ? 'Так ↕' : 'Ні'}</td>
    <td>${p.material_name} <span style=""color:#38bdf8"">[${p.material_article}]</span></td>
    <td>${formatEdgeBadge(p.edges.l1)}</td>
    <td>${formatEdgeBadge(p.edges.l2)}</td>
    <td>${formatEdgeBadge(p.edges.w1)}</td>
    <td>${formatEdgeBadge(p.edges.w2)}</td>
    <td>Отв: <b>${p.drilling.length}</b> | Пазів: <b>${p.grooves.length}</b></td>
    <td><button class=""btn"" onclick=""openBlueprintForPart(${idx})"">📐 Креслення</button></td>
  `;
  cutTbody.appendChild(tr);
});

// Populate Hardware Table
const hwTbody = document.getElementById('hwTbody');
PROJECT.hardware_bom.forEach(h => {
  const tr = document.createElement('tr');
  tr.innerHTML = `
    <td><b style=""color:#38bdf8"">${h.viyar_article}</b></td>
    <td>${h.name}</td>
    <td><b>${h.quantity}</b></td>
    <td>${h.unit}</td>
    <td>${h.unit_price_uah.toFixed(2)}</td>
    <td><b style=""color:#22c55e"">${h.total_price_uah.toFixed(2)} грн</b></td>
  `;
  hwTbody.appendChild(tr);
});

// Populate Sheet Nesting Maps
const sheetsContainer = document.getElementById('sheetsContainer');
PROJECT.sheet_layouts.forEach(sh => {
  const card = document.createElement('div');
  card.className = 'sheet-card';
  const scale = 0.28;
  const svgW = sh.sheet_length_mm * scale;
  const svgH = sh.sheet_width_mm * scale;
  let rectsHtml = '';
  sh.placements.forEach(pl => {
    const x = pl.x_mm * scale;
    const y = pl.y_mm * scale;
    const w = pl.length_mm * scale;
    const h = pl.width_mm * scale;
    rectsHtml += `
      <g>
        <rect x=""${x}"" y=""${y}"" width=""${w}"" height=""${h}"" fill=""rgba(56,189,248,0.22)"" stroke=""#38bdf8"" stroke-width=""1.2"" />
        <text x=""${x + w/2}"" y=""${y + h/2}"" fill=""#f8fafc"" font-size=""10"" text-anchor=""middle"" dominant-baseline=""middle"">
          #${pl.part_number} (${Math.round(pl.length_mm)}x${Math.round(pl.width_mm)})
        </text>
      </g>`;
  });
  card.innerHTML = `
    <div style=""display:flex;justify-content:space-between;margin-bottom:10px;"">
      <div><b>Лист #${sh.sheet_index}: ${sh.material_name}</b> <span style=""color:#38bdf8"">[Арт. ${sh.material_article}]</span> — Формат ${sh.sheet_length_mm}×${sh.sheet_width_mm} мм</div>
      <div>Заповнення листа: <b style=""color:#22c55e"">${sh.utilization_percent}%</b> (Деталей: ${sh.placements.length})</div>
    </div>
    <div style=""overflow-x:auto;background:#0b1120;padding:12px;border-radius:8px;"">
      <svg width=""${svgW}"" height=""${svgH}"" viewBox=""0 0 ${svgW} ${svgH}"" style=""background:#1e293b;border:1px solid #475569"">
        ${rectsHtml}
      </svg>
    </div>
  `;
  sheetsContainer.appendChild(card);
});

// ============================================================================
// 2D CNC BLUEPRINT RENDERER
// ============================================================================
const bpList = document.getElementById('blueprintPartList');
PROJECT.panels.forEach((p, idx) => {
  const item = document.createElement('div');
  item.className = 'part-item' + (idx === 0 ? ' active' : '');
  item.id = 'bpItem_' + idx;
  item.onclick = () => selectBlueprintPart(idx);
  item.innerHTML = `
    <div><b>#${p.part_number} ${p.name}</b><div style=""font-size:11px;color:#94a3b8"">${p.module_name}</div></div>
    <span style=""color:#38bdf8;font-weight:600"">${p.length_mm}×${p.width_mm}</span>
  `;
  bpList.appendChild(item);
});

function openBlueprintForPart(idx) {
  const btn = document.querySelectorAll('.tab-btn')[1];
  switchTab('tab2d', btn);
  selectBlueprintPart(idx);
}

function selectBlueprintPart(idx) {
  document.querySelectorAll('.part-item').forEach(el => el.classList.remove('active'));
  const activeItem = document.getElementById('bpItem_' + idx);
  if (activeItem) activeItem.classList.add('active');

  const p = PROJECT.panels[idx];
  document.getElementById('bpTitle').textContent = `Деталь #${p.part_number}: ${p.name} (${p.length_mm} × ${p.width_mm} × ${p.thickness_mm} мм)`;
  document.getElementById('bpSubtitle').textContent = `Модуль: ${p.module_name} | Матеріал: ${p.material_name} [Арт. ${p.material_article}]`;

  const edgesBox = document.getElementById('bpEdgesInfo');
  edgesBox.innerHTML = `
    <span class=""stat-pill"">L1 (Верх): <b>${p.edges.l1 || 'Без крайки'}</b></span>
    <span class=""stat-pill"">L2 (Низ): <b>${p.edges.l2 || 'Без крайки'}</b></span>
    <span class=""stat-pill"">W1 (Ліво): <b>${p.edges.w1 || 'Без крайки'}</b></span>
    <span class=""stat-pill"">W2 (Право): <b>${p.edges.w2 || 'Без крайки'}</b></span>
  `;

  // Render SVG Blueprint
  const pad = 75;
  const maxW = 780;
  const maxH = 360;
  const scale = Math.min(maxW / Math.max(p.length_mm, 100), maxH / Math.max(p.width_mm, 100));
  const drawW = p.length_mm * scale;
  const drawH = p.width_mm * scale;
  const svgW = drawW + pad * 2;
  const svgH = drawH + pad * 2;

  let svg = `<svg width=""${svgW}"" height=""${svgH}"" viewBox=""0 0 ${svgW} ${svgH}"">`;
  // Background Grid
  svg += `<rect x=""${pad}"" y=""${pad}"" width=""${drawW}"" height=""${drawH}"" fill=""#1e293b"" stroke=""#94a3b8"" stroke-width=""2"" />`;

  // Edge Banding Highlights
  const edgeColor = (code) => {
    if (!code) return null;
    return (code.startsWith('2.0') || code.startsWith('1.0')) ? '#f97316' : '#38bdf8';
  };
  if (edgeColor(p.edges.l1)) svg += `<line x1=""${pad}"" y1=""${pad}"" x2=""${pad+drawW}"" y2=""${pad}"" stroke=""${edgeColor(p.edges.l1)}"" stroke-width=""6"" />`;
  if (edgeColor(p.edges.l2)) svg += `<line x1=""${pad}"" y1=""${pad+drawH}"" x2=""${pad+drawW}"" y2=""${pad+drawH}"" stroke=""${edgeColor(p.edges.l2)}"" stroke-width=""6"" />`;
  if (edgeColor(p.edges.w1)) svg += `<line x1=""${pad}"" y1=""${pad}"" x2=""${pad}"" y2=""${pad+drawH}"" stroke=""${edgeColor(p.edges.w1)}"" stroke-width=""6"" />`;
  if (edgeColor(p.edges.w2)) svg += `<line x1=""${pad+drawW}"" y1=""${pad}"" x2=""${pad+drawW}"" y2=""${pad+drawH}"" stroke=""${edgeColor(p.edges.w2)}"" stroke-width=""6"" />`;

  // Grooves
  p.grooves.forEach(g => {
    if (g.orientation === 'along_length') {
      const gy = pad + g.offset_mm * scale;
      const gh = Math.max(g.width_mm * scale, 4);
      svg += `<rect x=""${pad}"" y=""${gy}"" width=""${drawW}"" height=""${gh}"" fill=""rgba(249,115,22,0.35)"" stroke=""#f97316"" stroke-dasharray=""4,3"" />`;
      svg += `<text x=""${pad + drawW/2}"" y=""${gy - 5}"" fill=""#fdba74"" font-size=""11"" text-anchor=""middle"">Паз ${g.width_mm}×${g.depth_mm} мм (відступ ${g.offset_mm} мм)</text>`;
    }
  });

  // Drilling Holes
  p.drilling.forEach((d, i) => {
    if (d.side === 'face_top' || d.side === 'face_bottom') {
      const cx = pad + d.x_mm * scale;
      const cy = pad + d.y_mm * scale;
      const r = Math.max((d.diameter_mm / 2) * scale, 5);
      const col = d.diameter_mm >= 30 ? '#ef4444' : '#22c55e';
      svg += `<circle cx=""${cx}"" cy=""${cy}"" r=""${r}"" fill=""rgba(34,197,94,0.25)"" stroke=""${col}"" stroke-width=""2"" />`;
      svg += `<line x1=""${cx-r-3}"" y1=""${cy}"" x2=""${cx+r+3}"" y2=""${cy}"" stroke=""${col}"" stroke-width=""1"" />`;
      svg += `<line x1=""${cx}"" y1=""${cy-r-3}"" x2=""${cx}"" y2=""${cy+r+3}"" stroke=""${col}"" stroke-width=""1"" />`;
      svg += `<text x=""${cx + r + 4}"" y=""${cy - 3}"" fill=""#f8fafc"" font-size=""10"">Ø${d.diameter_mm}×${d.depth_mm} (${d.x_mm};${d.y_mm})</text>`;
    } else {
      // Edge hole marker on W1 or W2
      const isLeft = d.side === 'edge_w1';
      const ex = isLeft ? pad : (pad + drawW);
      const ey = pad + d.x_mm * scale;
      svg += `<circle cx=""${ex}"" cy=""${ey}"" r=""4.5"" fill=""#38bdf8"" stroke=""#fff"" stroke-width=""1.5"" />`;
      svg += `<text x=""${isLeft ? ex - 8 : ex + 8}"" y=""${ey + 3}"" fill=""#7dd3fc"" font-size=""10"" text-anchor=""${isLeft ? 'end' : 'start'}"">Торець Ø${d.diameter_mm}×${d.depth_mm} (Y=${d.x_mm})</text>`;
    }
  });

  // Dimensions Labels
  svg += `<text x=""${pad + drawW/2}"" y=""${pad - 22}"" fill=""#38bdf8"" font-size=""13"" font-weight=""bold"" text-anchor=""middle"">L = ${p.length_mm} мм (Крайка L1: ${p.edges.l1 || 'немає'})</text>`;
  svg += `<text x=""${pad + drawW/2}"" y=""${pad + drawH + 30}"" fill=""#38bdf8"" font-size=""13"" font-weight=""bold"" text-anchor=""middle"">Крайка L2: ${p.edges.l2 || 'немає'}</text>`;
  svg += `<text x=""${pad - 18}"" y=""${pad + drawH/2}"" fill=""#38bdf8"" font-size=""13"" font-weight=""bold"" text-anchor=""middle"" transform=""rotate(-90, ${pad - 18}, ${pad + drawH/2})"">W = ${p.width_mm} мм</text>`;
  svg += `<circle cx=""${pad}"" cy=""${pad}"" r=""4"" fill=""#f97316"" /><text x=""${pad+6}"" y=""${pad+14}"" fill=""#fdba74"" font-size=""10"">(0,0)</text>`;
  svg += `</svg>`;
  document.getElementById('bpSvgBox').innerHTML = svg;

  // Operations Table
  let opsHtml = `<table><thead><tr><th>Тип обробки</th><th>Сторона</th><th>Координата X (мм)</th><th>Координата Y / Відступ (мм)</th><th>Параметри (Ø / Шир × Глиб)</th><th>Призначення</th></tr></thead><tbody>`;
  p.grooves.forEach(g => {
    opsHtml += `<tr><td><b style=""color:#f97316"">Фрезерування паза</b></td><td>${g.side}</td><td>${g.start_mm}..${g.end_mm}</td><td>Відступ ${g.offset_mm} мм</td><td>Шир ${g.width_mm} × Глиб ${g.depth_mm} мм</td><td>${g.purpose}</td></tr>`;
  });
  p.drilling.forEach(d => {
    opsHtml += `<tr><td><b style=""color:#22c55e"">Свердління отвору</b></td><td>${d.side}</td><td>${d.x_mm}</td><td>${d.y_mm}</td><td>Ø${d.diameter_mm} × Глиб ${d.depth_mm} мм</td><td>${d.purpose}</td></tr>`;
  });
  if (p.grooves.length === 0 && p.drilling.length === 0) {
    opsHtml += `<tr><td colspan=""6"" style=""color:#94a3b8;text-align:center"">Деталь без додаткових ЧПК свердлінь (тільки порізка та крайкування)</td></tr>`;
  }
  opsHtml += `</tbody></table>`;
  document.getElementById('bpOpsTable').innerHTML = opsHtml;
}
if (PROJECT.panels.length > 0) selectBlueprintPart(0);

// ============================================================================
// 3D INTERACTIVE CANVAS RENDERER (ZERO EXTERNAL DEPENDENCIES)
// ============================================================================
const canvas = document.getElementById('canvas3d');
const ctx = canvas.getContext('2d');
let camRotX = -0.38;
let camRotY = 0.62;
let camZoom = 0.28;
let camPanX = 0;
let camPanY = 80;
let explodeFactor = 0;
let openFactor = 0;
let xrayMode = false;
let hideFacades = false;
let selectedPartNumber = null;
let projectedPolys = [];

// Compute Scene Center
let minX = Infinity, minY = Infinity, minZ = Infinity;
let maxX = -Infinity, maxY = -Infinity, maxZ = -Infinity;
PROJECT.panels.forEach(p => {
  minX = Math.min(minX, p.x_mm); maxX = Math.max(maxX, p.x_mm + p.dx_mm);
  minY = Math.min(minY, p.y_mm); maxY = Math.max(maxY, p.y_mm + p.dy_mm);
  minZ = Math.min(minZ, p.z_mm); maxZ = Math.max(maxZ, p.z_mm + p.dz_mm);
});
const centerX = (minX + maxX) / 2;
const centerY = (minY + maxY) / 2;
const centerZ = (minZ + maxZ) / 2;

function update3DParams() {
  explodeFactor = parseInt(document.getElementById('rngExplode').value, 10) / 100.0;
  openFactor = parseInt(document.getElementById('rngOpen').value, 10) / 100.0;
  document.getElementById('lblExplode').textContent = Math.round(explodeFactor * 100) + '%';
  document.getElementById('lblOpen').textContent = Math.round(openFactor * 100) + '%';
  render3D();
}
function toggleXray() { xrayMode = !xrayMode; render3D(); }
function toggleFacades() { hideFacades = !hideFacades; render3D(); }
function resetCamera() { camRotX = -0.38; camRotY = 0.62; camZoom = 0.28; camPanX = 0; camPanY = 80; render3D(); }

let isDragging = false, lastMX = 0, lastMY = 0, dragMoved = false;
canvas.addEventListener('mousedown', e => { isDragging = true; dragMoved = false; lastMX = e.clientX; lastMY = e.clientY; });
window.addEventListener('mouseup', () => { isDragging = false; });
window.addEventListener('mousemove', e => {
  if (!isDragging) return;
  const dx = e.clientX - lastMX;
  const dy = e.clientY - lastMY;
  if (Math.abs(dx) > 2 || Math.abs(dy) > 2) dragMoved = true;
  lastMX = e.clientX; lastMY = e.clientY;
  if (e.shiftKey) {
    camPanX += dx; camPanY += dy;
  } else {
    camRotY += dx * 0.008;
    camRotX = Math.max(-1.4, Math.min(1.4, camRotX + dy * 0.008));
  }
  render3D();
});
canvas.addEventListener('wheel', e => {
  e.preventDefault();
  camZoom *= (e.deltaY < 0) ? 1.1 : 0.9;
  camZoom = Math.max(0.06, Math.min(1.8, camZoom));
  render3D();
}, { passive: false });

canvas.addEventListener('click', e => {
  if (dragMoved) return;
  const rect = canvas.getBoundingClientRect();
  const mx = e.clientX - rect.left;
  const my = e.clientY - rect.top;
  // Hit test from front to back
  for (let i = projectedPolys.length - 1; i >= 0; i--) {
    const poly = projectedPolys[i];
    if (pointInPoly(mx, my, poly.pts)) {
      selectedPartNumber = poly.panel.part_number;
      showSelectedPartDetails(poly.panel);
      render3D();
      return;
    }
  }
});

function pointInPoly(x, y, pts) {
  let inside = false;
  for (let i = 0, j = pts.length - 1; i < pts.length; j = i++) {
    const xi = pts[i].x, yi = pts[i].y;
    const xj = pts[j].x, yj = pts[j].y;
    const intersect = ((yi > y) !== (yj > y)) && (x < (xj - xi) * (y - yi) / (yj - yi + 0.0001) + xi);
    if (intersect) inside = !inside;
  }
  return inside;
}

function showSelectedPartDetails(p) {
  const idx = PROJECT.panels.findIndex(item => item.part_number === p.part_number);
  const box = document.getElementById('selectedPartBox');
  box.innerHTML = `
    <div style=""color:#f8fafc;font-size:15px;font-weight:700;margin-bottom:4px"">#${p.part_number} ${p.name}</div>
    <div>Модуль: <b style=""color:#38bdf8"">${p.module_name}</b></div>
    <div>Габарит: <b>${p.length_mm} × ${p.width_mm} × ${p.thickness_mm} мм</b></div>
    <div>Матеріал: <b>${p.material_name}</b> [${p.material_article}]</div>
    <div>Крайки: L1: <b>${p.edges.l1 || '—'}</b> | L2: <b>${p.edges.l2 || '—'}</b> | W1: <b>${p.edges.w1 || '—'}</b> | W2: <b>${p.edges.w2 || '—'}</b></div>
    <div>Свердлінь ЧПК: <b>${p.drilling.length}</b> | Пазів: <b>${p.grooves.length}</b></div>
    <button class=""btn btn-primary"" style=""margin-top:8px;width:100%"" onclick=""openBlueprintForPart(${idx})"">📐 Відкрити 2D креслення деталі</button>
  `;
}

function projectPoint(x, y, z, w, h) {
  // Center coordinates (SketchUp: X right, Y depth, Z up)
  let rx = x - centerX;
  let ry = -(z - centerZ);
  let rz = y - centerY;

  // Rotate around Y
  const cosY = Math.cos(camRotY), sinY = Math.sin(camRotY);
  const x1 = rx * cosY - rz * sinY;
  const z1 = rx * sinY + rz * cosY;

  // Rotate around X
  const cosX = Math.cos(camRotX), sinX = Math.sin(camRotX);
  const y2 = ry * cosX - z1 * sinX;
  const z2 = ry * sinX + z1 * cosX;

  const fov = 2600;
  const dist = 3400;
  const scale = (fov / Math.max(400, dist + z2)) * camZoom;
  return {
    x: w / 2 + x1 * scale + camPanX,
    y: h / 2 + y2 * scale + camPanY,
    depth: z2
  };
}

function shadeColor(hex, factor) {
  const c = hex.replace('#', '');
  const r = Math.min(255, Math.max(0, Math.round(parseInt(c.substring(0,2), 16) * factor)));
  const g = Math.min(255, Math.max(0, Math.round(parseInt(c.substring(2,4), 16) * factor)));
  const b = Math.min(255, Math.max(0, Math.round(parseInt(c.substring(4,6), 16) * factor)));
  return `rgb(${r},${g},${b})`;
}

function render3D() {
  const rect = canvas.parentElement.getBoundingClientRect();
  if (rect.width === 0) return;
  canvas.width = rect.width;
  canvas.height = rect.height;
  const W = canvas.width, H = canvas.height;
  ctx.clearRect(0, 0, W, H);
  projectedPolys = [];

  PROJECT.panels.forEach(p => {
    if (hideFacades && p.role.indexOf('facade') !== -1) return;

    const pCx = p.x_mm + p.dx_mm / 2;
    const pCy = p.y_mm + p.dy_mm / 2;
    const pCz = p.z_mm + p.dz_mm / 2;

    let offX = (pCx - centerX) * explodeFactor * 0.45;
    let offY = (pCy - centerY) * explodeFactor * 0.45;
    let offZ = (pCz - centerZ) * explodeFactor * 0.45;

    if (p.role.indexOf('facade') !== -1 || p.role.indexOf('drawer') !== -1) {
      offY -= openFactor * 260;
    }

    const x0 = p.x_mm + offX, x1 = x0 + p.dx_mm;
    const y0 = p.y_mm + offY, y1 = y0 + p.dy_mm;
    const z0 = p.z_mm + offZ, z1 = z0 + p.dz_mm;

    const v = [
      projectPoint(x0, y0, z0, W, H),
      projectPoint(x1, y0, z0, W, H),
      projectPoint(x1, y1, z0, W, H),
      projectPoint(x0, y1, z0, W, H),
      projectPoint(x0, y0, z1, W, H),
      projectPoint(x1, y0, z1, W, H),
      projectPoint(x1, y1, z1, W, H),
      projectPoint(x0, y1, z1, W, H)
    ];

    const faces = [
      { idx: [0,1,2,3], shade: 0.72 }, // Bottom
      { idx: [4,5,6,7], shade: 1.05 }, // Top
      { idx: [0,1,5,4], shade: 0.98 }, // Front
      { idx: [2,3,7,6], shade: 0.75 }, // Back
      { idx: [0,3,7,4], shade: 0.85 }, // Left
      { idx: [1,2,6,5], shade: 0.88 }  // Right
    ];

    faces.forEach(f => {
      const pts = f.idx.map(i => v[i]);
      const avgDepth = (pts[0].depth + pts[1].depth + pts[2].depth + pts[3].depth) / 4;
      projectedPolys.push({ pts, depth: avgDepth, shade: f.shade, panel: p });
    });
  });

  // Painter's algorithm sort (back to front)
  projectedPolys.sort((a, b) => b.depth - a.depth);

  projectedPolys.forEach(poly => {
    const isSel = poly.panel.part_number === selectedPartNumber;
    ctx.beginPath();
    ctx.moveTo(poly.pts[0].x, poly.pts[0].y);
    for (let i = 1; i < poly.pts.length; i++) ctx.lineTo(poly.pts[i].x, poly.pts[i].y);
    ctx.closePath();

    if (xrayMode) {
      ctx.fillStyle = isSel ? 'rgba(249,115,22,0.35)' : 'rgba(56,189,248,0.12)';
      ctx.strokeStyle = isSel ? '#f97316' : 'rgba(56,189,248,0.55)';
      ctx.lineWidth = isSel ? 2.2 : 0.9;
    } else {
      ctx.fillStyle = isSel ? '#f97316' : shadeColor(poly.panel.color_hex || '#E2E8F0', poly.shade);
      ctx.strokeStyle = isSel ? '#fff' : '#1e293b';
      ctx.lineWidth = isSel ? 2.0 : 0.8;
    }
    ctx.fill();
    ctx.stroke();
  });
}

window.addEventListener('resize', render3D);
setTimeout(render3D, 50);

function copyViyarClipboard() {
  let tsv = 'Назва деталі\tДовжина\tШирина\tКількість\tТекстура\tКрайка Верх (L1)\tКрайка Низ (L2)\tКрайка Ліво (W1)\tКрайка Право (W2)\tМатеріал\n';
  PROJECT.panels.forEach(p => {
    tsv += `${p.name} (${p.module_name})\t${p.length_mm}\t${p.width_mm}\t1\t${p.has_grain ? 'Так' : 'Ні'}\t${p.edges.l1}\t${p.edges.l2}\t${p.edges.w1}\t${p.edges.w2}\t${p.material_name} [${p.material_article}]\n`;
  });
  navigator.clipboard.writeText(tsv).then(() => {
    alert('✅ Таблицю деталювання скопійовано у буфер обміну (TSV формат для ViyarPro / Excel)!');
  });
}

function downloadJsonFile() {
  const blob = new Blob([JSON.stringify(PROJECT, null, 2)], { type: 'application/json' });
  const a = document.createElement('a');
  a.href = URL.createObjectURL(blob);
  a.download = PROJECT.project_name + '_viyar_agent.json';
  a.click();
}
</script>
</body>
</html>";
        }
    }
}

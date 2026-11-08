using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace ViyarAgent
{
    public class MaterialSpec
    {
        public string Id { get; set; }
        public string ViyarArticle { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Brand { get; set; }
        public double ThicknessMm { get; set; }
        public int SheetLengthMm { get; set; }
        public int SheetWidthMm { get; set; }
        public bool HasGrain { get; set; }
        public string ColorHex { get; set; }
        public double PriceUahPerSheet { get; set; }
        public string DefaultEdgeBody { get; set; }
        public string DefaultEdgeFront { get; set; }
        public string SketchUpMaterialName { get; set; }
    }

    public class EdgeSpec
    {
        public string Id { get; set; }
        public string ViyarArticle { get; set; }
        public string Name { get; set; }
        public double ThicknessMm { get; set; }
        public double WidthMm { get; set; }
        public string CodeShort { get; set; }
        public double PriceUahPerM { get; set; }
    }

    public class DrillHole
    {
        public string Side { get; set; } // face_top, face_bottom, edge_l1, edge_l2, edge_w1, edge_w2
        public double XMm { get; set; }
        public double YMm { get; set; }
        public double DiameterMm { get; set; }
        public double DepthMm { get; set; }
        public string Purpose { get; set; }
    }

    public class GrooveSlot
    {
        public string Side { get; set; } // face_top, face_bottom
        public string Orientation { get; set; } // along_length, along_width
        public double OffsetMm { get; set; }
        public double WidthMm { get; set; }
        public double DepthMm { get; set; }
        public double StartMm { get; set; }
        public double EndMm { get; set; }
        public string Purpose { get; set; }
    }

    public class PanelEdges
    {
        public string L1 { get; set; }
        public string L2 { get; set; }
        public string W1 { get; set; }
        public string W2 { get; set; }

        public PanelEdges()
        {
            L1 = "";
            L2 = "";
            W1 = "";
            W2 = "";
        }
    }

    public class FurniturePanel
    {
        public int PartNumber { get; set; }
        public string ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string Name { get; set; }
        public string AbfName { get; set; }
        public string Role { get; set; }
        public double LengthMm { get; set; }
        public double WidthMm { get; set; }
        public double SawLengthMm { get; set; }
        public double SawWidthMm { get; set; }
        public double ThicknessMm { get; set; }

        // 3D World Coordinates in SketchUp (mm)
        public double XMm { get; set; }
        public double YMm { get; set; }
        public double ZMm { get; set; }
        public double DXMm { get; set; }
        public double DYMm { get; set; }
        public double DZMm { get; set; }

        public string MaterialId { get; set; }
        public string MaterialName { get; set; }
        public string MaterialArticle { get; set; }
        public string ColorHex { get; set; }
        public bool HasGrain { get; set; }

        public PanelEdges Edges { get; set; }
        public List<DrillHole> Drilling { get; set; }
        public List<GrooveSlot> Grooves { get; set; }
        public string Notes { get; set; }

        public FurniturePanel()
        {
            Edges = new PanelEdges();
            Drilling = new List<DrillHole>();
            Grooves = new List<GrooveSlot>();
            Notes = "";
        }
    }

    public class ModuleInput
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }
        public double WidthMm { get; set; }
        public double HeightMm { get; set; }
        public double DepthMm { get; set; }
        public double OriginXMm { get; set; }
        public double OriginYMm { get; set; }
        public double OriginZMm { get; set; }

        public double LegHeightMm { get; set; }
        public int ShelvesCount { get; set; }
        public int DoorsCount { get; set; }
        public int DrawersCount { get; set; }
        public int SectionsCount { get; set; }
        public bool LedGroove { get; set; }
        public double HandlelessOverhangMm { get; set; }
        public string BackType { get; set; }
        public string FastenerType { get; set; }

        public string BodyMaterialId { get; set; }
        public string FacadeMaterialId { get; set; }
        public string BackMaterialId { get; set; }

        public ModuleInput()
        {
            SubType = "standard";
            WidthMm = 600;
            HeightMm = 720;
            DepthMm = 510;
            OriginXMm = 0;
            OriginYMm = 0;
            OriginZMm = 0;
            LegHeightMm = 100;
            ShelvesCount = 1;
            DoorsCount = 2;
            DrawersCount = 3;
            SectionsCount = 2;
            LedGroove = false;
            HandlelessOverhangMm = 0;
            BackType = "groove";
            FastenerType = "confirmat_dowel";
            BodyMaterialId = "egger_w1000_18";
            FacadeMaterialId = "egger_u961_18";
            BackMaterialId = "hdf_white_3";
        }
    }

    public class HardwareLine
    {
        public string ViyarArticle { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public int Quantity { get; set; }
        public double UnitPriceUah { get; set; }
        public double TotalPriceUah { get { return Math.Round(Quantity * UnitPriceUah, 2); } }
    }

    public class NestedPartPlacement
    {
        public int PartNumber { get; set; }
        public string Name { get; set; }
        public double XMm { get; set; }
        public double YMm { get; set; }
        public double PlacedLengthMm { get; set; }
        public double PlacedWidthMm { get; set; }
        public bool Rotated { get; set; }
    }

    public class SheetLayout
    {
        public int SheetIndex { get; set; }
        public string MaterialId { get; set; }
        public string MaterialName { get; set; }
        public string MaterialArticle { get; set; }
        public int SheetLengthMm { get; set; }
        public int SheetWidthMm { get; set; }
        public double UtilizationPercent { get; set; }
        public List<NestedPartPlacement> Placements { get; set; }

        public SheetLayout()
        {
            Placements = new List<NestedPartPlacement>();
        }
    }

    public class FurnitureProject
    {
        public string ProjectName { get; set; }
        public string ClientName { get; set; }
        public string CreatedAt { get; set; }
        public List<ModuleInput> Modules { get; set; }
        public List<FurniturePanel> Panels { get; set; }
        public List<MaterialSpec> MaterialsUsed { get; set; }
        public List<HardwareLine> HardwareBom { get; set; }
        public List<SheetLayout> SheetLayouts { get; set; }

        public double TotalAreaSqM { get; set; }
        public double TotalEdgeLengthM { get; set; }
        public int TotalDrillingHoles { get; set; }
        public double EstimatedTotalCostUah { get; set; }

        public FurnitureProject()
        {
            ProjectName = "Проект_Меблів_ВіЯр";
            ClientName = "Замовник";
            CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            Modules = new List<ModuleInput>();
            Panels = new List<FurniturePanel>();
            MaterialsUsed = new List<MaterialSpec>();
            HardwareBom = new List<HardwareLine>();
            SheetLayouts = new List<SheetLayout>();
        }
    }

    public static class ViyarCatalog
    {
        public static Dictionary<string, MaterialSpec> Materials = new Dictionary<string, MaterialSpec>(StringComparer.OrdinalIgnoreCase);
        public static Dictionary<string, EdgeSpec> Edges = new Dictionary<string, EdgeSpec>(StringComparer.OrdinalIgnoreCase);

        static ViyarCatalog()
        {
            InitDefaults();
        }

        public static void InitDefaults()
        {
            AddMat("egger_w1000_18", "12764", "ЛДСП Egger W1000 ST9 Білий преміум 18мм", "ldsp", "Egger", 18.0, 2800, 2070, false, "#F3F4F6", 2650, "pvc_05x22_white", "abs_20x22_white", "egger_w1000_biliy_prem_SU_$12764");
            AddMat("egger_w980_18", "81042", "ЛДСП Egger W980 SM Білий платиновий 18мм", "ldsp", "Egger", 18.0, 2800, 2070, false, "#ECEEEF", 2380, "pvc_05x22_white", "abs_20x22_white", "egger_w980_biliy_plat_SU_$81042");
            AddMat("egger_u961_18", "81101", "ЛДСП Egger U961 ST2 Графіт сірий 18мм", "ldsp", "Egger", 18.0, 2800, 2070, false, "#3B3E42", 2890, "abs_08x22_graphite", "abs_20x22_graphite", "egger_u961_grafit_siriy_SU_$81101");
            AddMat("egger_u963_18", "50394", "ДСП лам. Egger U963 Діамант сірий (Антрацит) ST9 2800х2070х18мм", "ldsp", "Egger", 18.0, 2800, 2070, false, "#414246", 3150, "abs_08x22_graphite", "abs_08x22_graphite", "egger_u963_dіamant_sіriy_SU_$50394");
            AddMat("egger_u708_18", "81088", "ЛДСП Egger U708 ST9 Світло-сірий 18мм", "ldsp", "Egger", 18.0, 2800, 2070, false, "#C6C9CC", 2720, "abs_08x22_grey", "abs_20x22_grey", "egger_u708_svitlo_siriy_SU_$81088");
            AddMat("egger_h3303_18", "81214", "ЛДСП Egger H3303 ST10 Дуб Гамільтон натуральний 18мм", "ldsp", "Egger", 18.0, 2800, 2070, true, "#B98B56", 3150, "abs_08x22_oak", "abs_20x22_oak", "egger_h3303_dub_gamilton_SU_$81214");
            AddMat("egger_h1180_18", "81190", "ЛДСП Egger H1180 ST37 Дуб Галіфакс натуральний 18мм", "ldsp", "Egger", 18.0, 2800, 2070, true, "#9D7142", 4120, "abs_08x22_oak", "abs_20x22_oak", "egger_h1180_dub_galifaks_SU_$81190");
            AddMat("kronospan_k009_18", "28863", "ДСП лам. Kronospan K009 PW Горіх Селект Темний 2800х2070х18мм", "ldsp", "Kronospan", 18.0, 2800, 2070, true, "#634632", 2680, "abs_08x22_oak", "abs_08x22_oak", "kronospan_k009_gorih_tem_SU_$28863");
            AddMat("kronospan_k009_36", "28863-36", "ДСП зрощене 36мм (18+18мм) Kronospan K009 PW Горіх Селект Темний", "ldsp_spliced", "Kronospan", 36.0, 2800, 2070, true, "#634632", 5360, "abs_20x22_oak", "abs_20x22_oak", "kronospan_k009_gorih_36mm_SU_$28863");
            AddMat("kronospan_0162_pe_16", "28848", "ДСП лам. Kronospan 0162 PE Графіт Сірий 2800х2070х16мм", "ldsp", "Kronospan", 16.0, 2800, 2070, false, "#4B4E53", 2290, "abs_08x22_graphite", "abs_08x22_graphite", "ДСП 0162 PE/ARTICLE28848");
            AddMat("egger_u960_st9_18", "33941", "ДСП лам. Egger U960 ST9 Онікс сірий 2800х2070х18мм", "ldsp", "Egger", 18.0, 2800, 2070, false, "#52555A", 3090, "abs_08x22_graphite", "abs_08x22_graphite", "U 960 ST9/ARTICLE33941");
            AddMat("worktop_k009_38", "1", "Стільниця Kronospan K009 Горіх Селект Темний 38мм (зрощена)", "worktop", "Kronospan", 38.0, 2800, 600, true, "#634632", 4200, "abs_20x22_oak", "abs_20x22_oak", "kronospan_k009_gorih_tem_SU_$1");
            AddMat("swiss_krono_white_16", "45210", "ЛДСП Swiss Krono Німфея Альба Біла 16мм", "ldsp", "Swiss Krono", 16.0, 2750, 1830, false, "#F8F9FA", 1890, "pvc_05x22_white", "abs_20x22_white", "swiss_krono_nimfeya_SU_$45210");
            AddMat("mdf_agt_728_18", "94312", "МДФ AGT 728 Soft Touch Сірий шовк 18мм", "mdf", "AGT", 18.0, 2800, 1220, false, "#787D82", 4450, "abs_10x22_agt728", "abs_10x22_agt728", "agt_728_siriy_shovk_SU_$94312");
            AddMat("mdf_agt_3019_18", "94520", "МДФ AGT 3019 Supramat Кашемір 18мм", "mdf", "AGT", 18.0, 2800, 1220, false, "#C5B9AE", 4850, "abs_10x22_cashmere", "abs_10x22_cashmere", "agt_3019_kashemir_SU_$94520");
            AddMat("hdf_white_3", "148575", "ДВП/ХДФ 3 мм Kronospan К009 Горіх селект темний / Біла 2800х2070", "hdf", "Kronospan", 3.0, 2800, 2070, false, "#6A4E3B", 890, "none", "none", "kronospan_hdf_3mm_SU_$148575");

            AddEdge("none", "", "Без крайки", 0.0, 0.0, "", 0.0);
            AddEdge("pvc_05x22_white", "19005", "Крайка ПВХ Maag Біла 22х0.5 мм ПУР", 0.5, 22.0, "0.5х22 ПУР (19005)", 12.5);
            AddEdge("abs_08x22_white", "22011", "Крайка АБС Egger W1000 22х0.8 мм ПУР", 0.8, 22.0, "0.8х22 ПУР (22011)", 24.0);
            AddEdge("abs_20x22_white", "22014", "Крайка АБС Egger W1000 22х2.0 мм ПУР", 2.0, 22.0, "2.0х22 ПУР (22014)", 38.0);
            AddEdge("abs_08x22_graphite", "89704", "Крайка ПВХ Rehau Антрацит 22х0.8 мм ПУР", 0.8, 22.0, "0.8х22 ПУР (89704)", 25.0);
            AddEdge("abs_20x22_graphite", "22190", "Крайка АБС Egger U961 Графіт 22х2.0 мм ПУР", 2.0, 22.0, "2.0х22 ПУР (22190)", 41.0);
            AddEdge("abs_08x22_grey", "22140", "Крайка АБС Egger U708 Світло-сіра 22х0.8 мм ПУР", 0.8, 22.0, "0.8х22 ПУР (22140)", 25.0);
            AddEdge("abs_20x22_grey", "22142", "Крайка АБС Egger U708 Світло-сіра 22х2.0 мм ПУР", 2.0, 22.0, "2.0х22 ПУР (22142)", 40.0);
            AddEdge("abs_08x22_oak", "34549", "Крайка АБС Дуб Галіфакс / Горіх 22х0.8 мм ПУР", 0.8, 22.0, "0.8х22 ПУР (34549)", 28.0);
            AddEdge("abs_20x22_oak", "22310", "Крайка АБС Egger H1180 Дуб Галіфакс 22х2.0 мм ПУР", 2.0, 22.0, "2.0х22 ПУР (22310)", 45.0);
            AddEdge("abs_10x22_agt728", "94399", "Крайка АБС AGT 728 Сірий шовк 22х1.0 мм ПУР", 1.0, 22.0, "1.0х22 ПУР (94399)", 55.0);
            AddEdge("abs_10x22_cashmere", "94599", "Крайка АБС AGT 3019 Кашемір 22х1.0 мм ПУР", 1.0, 22.0, "1.0х22 ПУР (94599)", 58.0);
        }

        private static void AddMat(string id, string art, string name, string type, string brand, double t, int sl, int sw, bool grain, string hex, double price, string eb, string ef, string suName)
        {
            Materials[id] = new MaterialSpec
            {
                Id = id, ViyarArticle = art, Name = name, Type = type, Brand = brand,
                ThicknessMm = t, SheetLengthMm = sl, SheetWidthMm = sw, HasGrain = grain,
                ColorHex = hex, PriceUahPerSheet = price, DefaultEdgeBody = eb, DefaultEdgeFront = ef,
                SketchUpMaterialName = suName
            };
        }

        private static void AddEdge(string id, string art, string name, double t, double w, string code, double price)
        {
            Edges[id] = new EdgeSpec
            {
                Id = id, ViyarArticle = art, Name = name, ThicknessMm = t, WidthMm = w, CodeShort = code, PriceUahPerM = price
            };
        }

        public static MaterialSpec GetMaterial(string id)
        {
            if (!string.IsNullOrEmpty(id) && Materials.ContainsKey(id)) return Materials[id];
            return Materials["egger_w1000_18"];
        }

        public static string GetEdgeCode(string edgeId)
        {
            if (!string.IsNullOrEmpty(edgeId) && Edges.ContainsKey(edgeId)) return Edges[edgeId].CodeShort;
            return "";
        }
    }

    public static class ViyarBuilder
    {
        public static FurnitureProject BuildDemoKitchenProject()
        {
            var mods = new List<ModuleInput>
            {
                new ModuleInput
                {
                    Id = "low_1", Name = "Н1 Тумба під мийку 600", Type = "kitchen_lower_doors", SubType = "sink",
                    WidthMm = 600, HeightMm = 720, DepthMm = 510, OriginXMm = 0, OriginYMm = 0, OriginZMm = 0, LegHeightMm = 100,
                    BodyMaterialId = "egger_w1000_18", FacadeMaterialId = "egger_u961_18"
                },
                new ModuleInput
                {
                    Id = "low_2", Name = "Н2 Тумба з 3 шухлядами Blum Tandem 600", Type = "kitchen_lower_drawers",
                    WidthMm = 600, HeightMm = 720, DepthMm = 510, OriginXMm = 600, OriginYMm = 0, OriginZMm = 0, LegHeightMm = 100, DrawersCount = 3,
                    BodyMaterialId = "egger_w1000_18", FacadeMaterialId = "egger_u961_18"
                },
                new ModuleInput
                {
                    Id = "low_3", Name = "Н3 Тумба робоча з полицею 600", Type = "kitchen_lower_doors",
                    WidthMm = 600, HeightMm = 720, DepthMm = 510, OriginXMm = 1200, OriginYMm = 0, OriginZMm = 0, LegHeightMm = 100, ShelvesCount = 1,
                    BodyMaterialId = "egger_w1000_18", FacadeMaterialId = "egger_u961_18"
                },
                new ModuleInput
                {
                    Id = "tall_4", Name = "П1 Пенал кухонний високий 600х2140", Type = "wardrobe",
                    WidthMm = 600, HeightMm = 2140, DepthMm = 560, OriginXMm = 1800, OriginYMm = 0, OriginZMm = 0, LegHeightMm = 100, DoorsCount = 1, SectionsCount = 1, ShelvesCount = 4,
                    BodyMaterialId = "egger_w1000_18", FacadeMaterialId = "egger_u961_18"
                },
                new ModuleInput
                {
                    Id = "up_1", Name = "В1 Шафа верхня 600 (з пазом під LED)", Type = "kitchen_upper_cabinet",
                    WidthMm = 600, HeightMm = 720, DepthMm = 320, OriginXMm = 0, OriginYMm = 190, OriginZMm = 1420, LegHeightMm = 0, ShelvesCount = 2, DoorsCount = 2, LedGroove = true, HandlelessOverhangMm = 20,
                    BodyMaterialId = "egger_w1000_18", FacadeMaterialId = "egger_h3303_18"
                },
                new ModuleInput
                {
                    Id = "up_2", Name = "В2 Шафа верхня 600 (з пазом під LED)", Type = "kitchen_upper_cabinet",
                    WidthMm = 600, HeightMm = 720, DepthMm = 320, OriginXMm = 600, OriginYMm = 190, OriginZMm = 1420, LegHeightMm = 0, ShelvesCount = 2, DoorsCount = 2, LedGroove = true, HandlelessOverhangMm = 20,
                    BodyMaterialId = "egger_w1000_18", FacadeMaterialId = "egger_h3303_18"
                },
                new ModuleInput
                {
                    Id = "up_3", Name = "В3 Шафа верхня 600 (з пазом під LED)", Type = "kitchen_upper_cabinet",
                    WidthMm = 600, HeightMm = 720, DepthMm = 320, OriginXMm = 1200, OriginYMm = 190, OriginZMm = 1420, LegHeightMm = 0, ShelvesCount = 2, DoorsCount = 2, LedGroove = true, HandlelessOverhangMm = 20,
                    BodyMaterialId = "egger_w1000_18", FacadeMaterialId = "egger_h3303_18"
                }
            };
            return BuildProject("Кухня_Модерн_ВіЯр_2400мм", mods);
        }

        public static FurnitureProject BuildProject(string projectName, List<ModuleInput> modules)
        {
            var proj = new FurnitureProject { ProjectName = projectName, Modules = modules };
            int partSeq = 1;
            var hwTotals = new Dictionary<string, HardwareLine>();

            foreach (var m in modules)
            {
                if (string.IsNullOrEmpty(m.Id)) m.Id = "mod_" + (proj.Modules.IndexOf(m) + 1);
                if (string.IsNullOrEmpty(m.Name)) m.Name = m.Type + "_" + m.WidthMm;

                switch ((m.Type ?? "kitchen_lower_doors").ToLowerInvariant())
                {
                    case "kitchen_lower_doors":
                        BuildKitchenLowerDoors(proj, m, ref partSeq, hwTotals);
                        break;
                    case "kitchen_lower_drawers":
                        BuildKitchenLowerDrawers(proj, m, ref partSeq, hwTotals);
                        break;
                    case "kitchen_upper_cabinet":
                        BuildKitchenUpperCabinet(proj, m, ref partSeq, hwTotals);
                        break;
                    case "wardrobe":
                        BuildWardrobe(proj, m, ref partSeq, hwTotals);
                        break;
                    case "office_desk":
                        BuildOfficeDesk(proj, m, ref partSeq, hwTotals);
                        break;
                    case "tv_unit":
                        BuildTvUnit(proj, m, ref partSeq, hwTotals);
                        break;
                    case "wall_shelves":
                        BuildWallShelves(proj, m, ref partSeq, hwTotals);
                        break;
                    default:
                        BuildKitchenLowerDoors(proj, m, ref partSeq, hwTotals);
                        break;
                }
            }

            // Collect unique materials used
            var usedIds = proj.Panels.Select(p => p.MaterialId).Distinct().ToList();
            foreach (var mid in usedIds)
            {
                proj.MaterialsUsed.Add(ViyarCatalog.GetMaterial(mid));
            }

            proj.HardwareBom = hwTotals.Values.OrderBy(h => h.Name).ToList();
            ComputeProjectMetricsAndNesting(proj);
            return proj;
        }

        private static void AddHardware(Dictionary<string, HardwareLine> map, string art, string name, string unit, int qty, double price)
        {
            if (qty <= 0) return;
            if (!map.ContainsKey(art))
            {
                map[art] = new HardwareLine { ViyarArticle = art, Name = name, Unit = unit, Quantity = 0, UnitPriceUah = price };
            }
            map[art].Quantity += qty;
        }

        // =====================================================================
        // MODULE 1: KITCHEN LOWER CABINET WITH DOORS / SINK / OVEN
        // =====================================================================
        private static void BuildKitchenLowerDoors(FurnitureProject proj, ModuleInput m, ref int seq, Dictionary<string, HardwareLine> hw)
        {
            var matBody = ViyarCatalog.GetMaterial(m.BodyMaterialId);
            var matFacade = ViyarCatalog.GetMaterial(m.FacadeMaterialId);
            var matHdf = ViyarCatalog.GetMaterial(m.BackMaterialId);

            double T = matBody.ThicknessMm; // 18mm
            double W = m.WidthMm;
            double H = m.HeightMm; // Body height e.g. 720mm
            double D = m.DepthMm;  // Body depth e.g. 510mm
            double ox = m.OriginXMm;
            double oy = m.OriginYMm;
            double oz = m.OriginZMm + m.LegHeightMm; // Elevated on legs

            string eBody = ViyarCatalog.GetEdgeCode(matBody.DefaultEdgeBody);
            string eFront = ViyarCatalog.GetEdgeCode(matBody.DefaultEdgeFront);
            string eFacade = ViyarCatalog.GetEdgeCode(matFacade.DefaultEdgeFront);

            bool hasGroove = (m.BackType == "groove" && m.SubType != "sink");
            double innerW = W - 2 * T;
            double stretchyW = 100.0;

            // 1 & 2. Left & Right Side Panels (Боковина ліва та права)
            for (int sideIdx = 0; sideIdx < 2; sideIdx++)
            {
                bool isLeft = (sideIdx == 0);
                var pSide = CreatePanel(ref seq, m, isLeft ? "Боковина ліва" : "Боковина права", "body",
                    H, D, T,
                    isLeft ? ox : (ox + W - T), oy, oz,
                    T, D, H,
                    matBody, eFront, eBody, eBody, eBody);

                if (hasGroove)
                {
                    pSide.Grooves.Add(new GrooveSlot
                    {
                        Side = "face_top", Orientation = "along_length",
                        OffsetMm = 16.0, WidthMm = 4.0, DepthMm = 8.0, StartMm = 0, EndMm = H,
                        Purpose = "Паз під задню стінку ХДФ 3мм (4х8 мм, відступ 16мм)"
                    });
                }

                // Drilling for bottom panel (at Z = 9mm from bottom)
                AddHorizontalJointFaceHoles(pSide, 9.0, D, true);
                // Drilling for top stretchers (at Z = H - 9mm)
                pSide.Drilling.Add(new DrillHole { Side = "face_top", XMm = H - 9.0, YMm = D - 50.0, DiameterMm = 5.0, DepthMm = T, Purpose = "Конфірмат царги передньої" });
                pSide.Drilling.Add(new DrillHole { Side = "face_top", XMm = H - 9.0, YMm = 50.0, DiameterMm = 5.0, DepthMm = T, Purpose = "Конфірмат царги задньої" });

                // Shelf pin holes (Система 32: 3 levels per shelf)
                if (m.SubType != "sink" && m.ShelvesCount > 0)
                {
                    for (int s = 1; s <= m.ShelvesCount; s++)
                    {
                        double shelfZ = Math.Round((H / (m.ShelvesCount + 1)) * s, 1);
                        foreach (double offsetZ in new[] { -32.0, 0.0, 32.0 })
                        {
                            pSide.Drilling.Add(new DrillHole { Side = "face_top", XMm = shelfZ + offsetZ, YMm = D - 60.0, DiameterMm = 5.0, DepthMm = 12.0, Purpose = "Полицетримач D5x12 (Система 32)" });
                            pSide.Drilling.Add(new DrillHole { Side = "face_top", XMm = shelfZ + offsetZ, YMm = 60.0, DiameterMm = 5.0, DepthMm = 12.0, Purpose = "Полицетримач D5x12 (Система 32)" });
                        }
                    }
                }

                proj.Panels.Add(pSide);
            }

            // 3. Bottom Panel (Дно вкладне)
            double bottomDepth = hasGroove ? D : D;
            var pBottom = CreatePanel(ref seq, m, "Дно вкладне", "body",
                innerW, bottomDepth, T,
                ox + T, oy, oz,
                innerW, bottomDepth, T,
                matBody, eFront, eBody, "", "");
            if (hasGroove)
            {
                pBottom.Grooves.Add(new GrooveSlot
                {
                    Side = "face_top", Orientation = "along_length",
                    OffsetMm = 16.0, WidthMm = 4.0, DepthMm = 8.0, StartMm = 0, EndMm = innerW,
                    Purpose = "Паз під задню стінку ХДФ 3мм (4х8 мм)"
                });
            }
            AddEdgeConfirmatsAndDowels(pBottom, bottomDepth);
            proj.Panels.Add(pBottom);

            // 4 & 5. Top Front & Rear Stretchers (Царга передня та задня)
            var pStretchFront = CreatePanel(ref seq, m, "Царга верхня передня", "stretcher",
                innerW, stretchyW, T,
                ox + T, oy, oz + H - T,
                innerW, stretchyW, T,
                matBody, eFront, eBody, "", "");
            AddStretcherEdgeHoles(pStretchFront);
            proj.Panels.Add(pStretchFront);

            var pStretchRear = CreatePanel(ref seq, m, "Царга верхня задня", "stretcher",
                innerW, stretchyW, T,
                ox + T, oy + D - stretchyW, oz + H - T,
                innerW, stretchyW, T,
                matBody, eBody, eBody, "", "");
            if (hasGroove)
            {
                pStretchRear.Grooves.Add(new GrooveSlot
                {
                    Side = "face_bottom", Orientation = "along_length",
                    OffsetMm = 16.0, WidthMm = 4.0, DepthMm = 8.0, StartMm = 0, EndMm = innerW,
                    Purpose = "Паз під задню стінку ХДФ 3мм"
                });
            }
            AddStretcherEdgeHoles(pStretchRear);
            proj.Panels.Add(pStretchRear);

            // 6. Removable Shelves (Полиці знімні)
            if (m.SubType != "sink" && m.ShelvesCount > 0)
            {
                double shelfW = innerW - 2.0; // 2mm clearance for shelf pins
                double shelfD = D - 40.0;
                for (int s = 1; s <= m.ShelvesCount; s++)
                {
                    double shelfZ = Math.Round((H / (m.ShelvesCount + 1)) * s, 1);
                    var pShelf = CreatePanel(ref seq, m, "Полиця знімна #" + s, "shelf",
                        shelfW, shelfD, T,
                        ox + T + 1.0, oy + 10.0, oz + shelfZ,
                        shelfW, shelfD, T,
                        matBody, eFront, eBody, eBody, eBody);
                    proj.Panels.Add(pShelf);
                    AddHardware(hw, "10820", "Полицетримач 5 мм з кільцем нікель", "шт", 4, 1.2);
                }
            }

            // 7. HDF Back Panel (Задня стінка ХДФ 3мм)
            if (m.BackType != "none" && m.SubType != "sink")
            {
                double hdfH = hasGroove ? (H - 2 * T + 14.0) : (H - 2.0);
                double hdfW = hasGroove ? (innerW + 14.0) : (W - 2.0);
                var pHdf = CreatePanel(ref seq, m, "Задня стінка ХДФ", "hdf_back",
                    hdfH, hdfW, matHdf.ThicknessMm,
                    ox + T - 7.0, oy + D - 19.0, oz + T - 7.0,
                    hdfW, matHdf.ThicknessMm, hdfH,
                    matHdf, "", "", "", "");
                proj.Panels.Add(pHdf);
            }

            // 8. Doors / Facades (Фасади розпашні з присадкою під петлі Blum D35)
            int doors = m.DoorsCount <= 0 ? (W >= 550 ? 2 : 1) : m.DoorsCount;
            double fH = H - 4.0;
            double fW = doors == 1 ? (W - 4.0) : Math.Round((W - 7.0) / 2.0, 1);
            for (int d = 0; d < doors; d++)
            {
                double fx = ox + 2.0 + d * (fW + 3.0);
                var pDoor = CreatePanel(ref seq, m, doors == 1 ? "Фасад" : (d == 0 ? "Фасад лівий" : "Фасад правий"), "facade_door",
                    fH, fW, matFacade.ThicknessMm,
                    fx, oy - matFacade.ThicknessMm - 1.0, oz + 2.0,
                    fW, matFacade.ThicknessMm, fH,
                    matFacade, eFacade, eFacade, eFacade, eFacade);

                // Add Blum D35x13mm hinge cup drilling
                int hingesPerDoor = fH > 1000 ? 3 : 2;
                double hingeEdgeOffset = 22.5;
                double cupY = (d == 0) ? hingeEdgeOffset : (fW - hingeEdgeOffset);
                pDoor.Drilling.Add(new DrillHole { Side = "face_bottom", XMm = 100.0, YMm = cupY, DiameterMm = 35.0, DepthMm = 13.0, Purpose = "Чашка петлі Blum CLIP top D35x13 (верхня)" });
                pDoor.Drilling.Add(new DrillHole { Side = "face_bottom", XMm = fH - 100.0, YMm = cupY, DiameterMm = 35.0, DepthMm = 13.0, Purpose = "Чашка петлі Blum CLIP top D35x13 (нижня)" });
                if (hingesPerDoor >= 3)
                {
                    pDoor.Drilling.Add(new DrillHole { Side = "face_bottom", XMm = Math.Round(fH / 2.0, 1), YMm = cupY, DiameterMm = 35.0, DepthMm = 13.0, Purpose = "Чашка петлі Blum CLIP top D35x13 (середня)" });
                }

                proj.Panels.Add(pDoor);
                AddHardware(hw, "71B3550", "Петля накладна Blum CLIP top BLUMOTION 110° + планка", "компл", hingesPerDoor, 165.0);
            }

            AddHardware(hw, "10412", "Конфірмат 5х50 мм цинк", "шт", 8, 0.85);
            AddHardware(hw, "10501", "Шкант буковий 8х30 мм", "шт", 8, 0.45);
            AddHardware(hw, "14010", "Ніжка кухонна регульована H=100 мм з кліпсою", "шт", 4, 18.0);
        }

        // =====================================================================
        // MODULE 2: KITCHEN LOWER CABINET WITH DRAWERS / CHEST OF DRAWERS
        // =====================================================================
        private static void BuildKitchenLowerDrawers(FurnitureProject proj, ModuleInput m, ref int seq, Dictionary<string, HardwareLine> hw)
        {
            var matBody = ViyarCatalog.GetMaterial(m.BodyMaterialId);
            var matFacade = ViyarCatalog.GetMaterial(m.FacadeMaterialId);
            var matHdf = ViyarCatalog.GetMaterial(m.BackMaterialId);

            double T = matBody.ThicknessMm;
            double W = m.WidthMm;
            double H = m.HeightMm;
            double D = m.DepthMm;
            double ox = m.OriginXMm;
            double oy = m.OriginYMm;
            double oz = m.OriginZMm + m.LegHeightMm;

            string eBody = ViyarCatalog.GetEdgeCode(matBody.DefaultEdgeBody);
            string eFront = ViyarCatalog.GetEdgeCode(matBody.DefaultEdgeFront);
            string eFacade = ViyarCatalog.GetEdgeCode(matFacade.DefaultEdgeFront);

            double innerW = W - 2 * T;
            int drawersCount = m.DrawersCount <= 0 ? 3 : m.DrawersCount;

            // 1 & 2. Left & Right Sides
            for (int sideIdx = 0; sideIdx < 2; sideIdx++)
            {
                bool isLeft = (sideIdx == 0);
                var pSide = CreatePanel(ref seq, m, isLeft ? "Боковина ліва" : "Боковина права", "body",
                    H, D, T,
                    isLeft ? ox : (ox + W - T), oy, oz,
                    T, D, H,
                    matBody, eFront, eBody, eBody, eBody);
                pSide.Grooves.Add(new GrooveSlot
                {
                    Side = "face_top", Orientation = "along_length",
                    OffsetMm = 16.0, WidthMm = 4.0, DepthMm = 8.0, StartMm = 0, EndMm = H,
                    Purpose = "Паз під задню стінку ХДФ 3мм (4х8 мм)"
                });
                AddHorizontalJointFaceHoles(pSide, 9.0, D, true);
                pSide.Drilling.Add(new DrillHole { Side = "face_top", XMm = H - 9.0, YMm = D - 50.0, DiameterMm = 5.0, DepthMm = T, Purpose = "Конфірмат царги передньої" });
                pSide.Drilling.Add(new DrillHole { Side = "face_top", XMm = H - 9.0, YMm = 50.0, DiameterMm = 5.0, DepthMm = T, Purpose = "Конфірмат царги задньої" });
                proj.Panels.Add(pSide);
            }

            // 3. Bottom Panel
            var pBottom = CreatePanel(ref seq, m, "Дно вкладне", "body",
                innerW, D, T, ox + T, oy, oz, innerW, D, T, matBody, eFront, eBody, "", "");
            pBottom.Grooves.Add(new GrooveSlot { Side = "face_top", Orientation = "along_length", OffsetMm = 16.0, WidthMm = 4.0, DepthMm = 8.0, StartMm = 0, EndMm = innerW, Purpose = "Паз під ХДФ" });
            AddEdgeConfirmatsAndDowels(pBottom, D);
            proj.Panels.Add(pBottom);

            // 4 & 5. Top Stretchers
            var pStretchF = CreatePanel(ref seq, m, "Царга верхня передня", "stretcher", innerW, 100.0, T, ox + T, oy, oz + H - T, innerW, 100.0, T, matBody, eFront, eBody, "", "");
            AddStretcherEdgeHoles(pStretchF);
            proj.Panels.Add(pStretchF);

            var pStretchR = CreatePanel(ref seq, m, "Царга верхня задня", "stretcher", innerW, 100.0, T, ox + T, oy + D - 100.0, oz + H - T, innerW, 100.0, T, matBody, eBody, eBody, "", "");
            AddStretcherEdgeHoles(pStretchR);
            proj.Panels.Add(pStretchR);

            // 6. HDF Back Panel
            var pHdf = CreatePanel(ref seq, m, "Задня стінка ХДФ", "hdf_back",
                H - 2 * T + 14.0, innerW + 14.0, matHdf.ThicknessMm,
                ox + T - 7.0, oy + D - 19.0, oz + T - 7.0,
                innerW + 14.0, matHdf.ThicknessMm, H - 2 * T + 14.0, matHdf, "", "", "", "");
            proj.Panels.Add(pHdf);

            // 7. Drawers (Фасади шухляд + Деталі короба кожної шухляди під напрямні L=450мм)
            double runnerNomL = D >= 500 ? 450.0 : 400.0;
            double boxDepth = runnerNomL - 10.0; // 440mm
            double totalGaps = 4.0 + (drawersCount - 1) * 3.0;
            double availFacadeH = H - totalGaps;

            double currentZ = oz + 2.0;
            for (int i = 0; i < drawersCount; i++)
            {
                // Make top drawer slightly smaller if 3 drawers (140mm top + two larger bottom)
                double fH;
                if (drawersCount == 3 && H >= 700)
                {
                    fH = (i == 2) ? 140.0 : Math.Round((availFacadeH - 140.0) / 2.0, 1);
                }
                else
                {
                    fH = Math.Round(availFacadeH / drawersCount, 1);
                }

                double fW = W - 4.0;
                int dNum = i + 1;

                // Drawer Facade
                var pFacade = CreatePanel(ref seq, m, "Фасад шухляди #" + dNum, "facade_drawer",
                    fW, fH, matFacade.ThicknessMm,
                    ox + 2.0, oy - matFacade.ThicknessMm - 1.0, currentZ,
                    fW, matFacade.ThicknessMm, fH,
                    matFacade, eFacade, eFacade, eFacade, eFacade);
                pFacade.Drilling.Add(new DrillHole { Side = "face_bottom", XMm = 50.0, YMm = fH / 2.0, DiameterMm = 5.0, DepthMm = 12.0, Purpose = "Кріплення фасаду шухляди (ліве)" });
                pFacade.Drilling.Add(new DrillHole { Side = "face_bottom", XMm = fW - 50.0, YMm = fH / 2.0, DiameterMm = 5.0, DepthMm = 12.0, Purpose = "Кріплення фасаду шухляди (праве)" });
                proj.Panels.Add(pFacade);

                // Drawer Box height
                double boxH = Math.Max(80.0, Math.Min(180.0, fH - 55.0));
                double boxInnerW = innerW - 42.0 - 2 * T; // Blum Tandem clearance
                double boxZ = currentZ + 25.0;

                // Drawer Left & Right Box Sides
                var pBoxL = CreatePanel(ref seq, m, "Боковина шухляди #" + dNum + " ліва", "drawer_side",
                    boxDepth, boxH, T, ox + T + 21.0, oy + 5.0, boxZ, T, boxDepth, boxH, matBody, eBody, eBody, eBody, eBody);
                var pBoxR = CreatePanel(ref seq, m, "Боковина шухляди #" + dNum + " права", "drawer_side",
                    boxDepth, boxH, T, ox + W - T - 21.0 - T, oy + 5.0, boxZ, T, boxDepth, boxH, matBody, eBody, eBody, eBody, eBody);
                proj.Panels.Add(pBoxL);
                proj.Panels.Add(pBoxR);

                // Drawer Box Front & Back
                var pBoxF = CreatePanel(ref seq, m, "Передня стінка короба шухляди #" + dNum, "drawer_box",
                    boxInnerW, boxH, T, ox + 2 * T + 21.0, oy + 5.0, boxZ, boxInnerW, T, boxH, matBody, eBody, eBody, "", "");
                var pBoxB = CreatePanel(ref seq, m, "Задня стінка короба шухляди #" + dNum, "drawer_box",
                    boxInnerW, boxH, T, ox + 2 * T + 21.0, oy + 5.0 + boxDepth - T, boxZ, boxInnerW, T, boxH, matBody, eBody, eBody, "", "");
                proj.Panels.Add(pBoxF);
                proj.Panels.Add(pBoxB);

                // Drawer Bottom HDF
                var pBoxBot = CreatePanel(ref seq, m, "Дно шухляди #" + dNum + " ХДФ", "hdf_drawer_bottom",
                    boxInnerW + 2 * T, boxDepth, matHdf.ThicknessMm,
                    ox + T + 21.0, oy + 5.0, boxZ - matHdf.ThicknessMm,
                    boxInnerW + 2 * T, boxDepth, matHdf.ThicknessMm, matHdf, "", "", "", "");
                proj.Panels.Add(pBoxBot);

                currentZ += fH + 3.0;
                AddHardware(hw, "560H4500B", "Напрямні прихованого монтажу Blum TANDEM BLUMOTION L=450 мм", "компл", 1, 980.0);
                AddHardware(hw, "10412", "Конфірмат 5х50 мм цинк", "шт", 8, 0.85);
            }

            AddHardware(hw, "10412", "Конфірмат 5х50 мм цинк", "шт", 8, 0.85);
            AddHardware(hw, "14010", "Ніжка кухонна регульована H=100 мм з кліпсою", "шт", 4, 18.0);
        }

        // =====================================================================
        // MODULE 3: KITCHEN UPPER WALL CABINET (ШАФА КУХОННА ВЕРХНЯ НАВІСНА)
        // =====================================================================
        private static void BuildKitchenUpperCabinet(FurnitureProject proj, ModuleInput m, ref int seq, Dictionary<string, HardwareLine> hw)
        {
            var matBody = ViyarCatalog.GetMaterial(m.BodyMaterialId);
            var matFacade = ViyarCatalog.GetMaterial(m.FacadeMaterialId);
            var matHdf = ViyarCatalog.GetMaterial(m.BackMaterialId);

            double T = matBody.ThicknessMm;
            double W = m.WidthMm;
            double H = m.HeightMm;
            double D = m.DepthMm > 400 ? 320.0 : m.DepthMm;
            double ox = m.OriginXMm;
            double oy = m.OriginYMm;
            double oz = m.OriginZMm;

            string eBody = ViyarCatalog.GetEdgeCode(matBody.DefaultEdgeBody);
            string eFront = ViyarCatalog.GetEdgeCode(matBody.DefaultEdgeFront);
            string eFacade = ViyarCatalog.GetEdgeCode(matFacade.DefaultEdgeFront);

            double innerW = W - 2 * T;

            // 1 & 2. Left & Right Sides
            for (int sideIdx = 0; sideIdx < 2; sideIdx++)
            {
                bool isLeft = (sideIdx == 0);
                var pSide = CreatePanel(ref seq, m, isLeft ? "Боковина верхньої шафи ліва" : "Боковина верхньої шафи права", "body",
                    H, D, T,
                    isLeft ? ox : (ox + W - T), oy, oz,
                    T, D, H,
                    matBody, eFront, eBody, eBody, eBody);
                pSide.Grooves.Add(new GrooveSlot
                {
                    Side = "face_top", Orientation = "along_length",
                    OffsetMm = 16.0, WidthMm = 4.0, DepthMm = 8.0, StartMm = 0, EndMm = H,
                    Purpose = "Паз під ХДФ (відступ 16мм під навіси Camar 806)"
                });
                AddHorizontalJointFaceHoles(pSide, 9.0, D, true);
                AddHorizontalJointFaceHoles(pSide, H - 9.0, D, true);
                proj.Panels.Add(pSide);
            }

            // 3. Bottom Panel (Дно вкладне — опційно з пазом під LED профіль 16х7 мм)
            var pBottom = CreatePanel(ref seq, m, "Дно верхньої шафи", "body",
                innerW, D, T, ox + T, oy, oz, innerW, D, T, matBody, eFront, eBody, "", "");
            pBottom.Grooves.Add(new GrooveSlot { Side = "face_top", Orientation = "along_length", OffsetMm = 16.0, WidthMm = 4.0, DepthMm = 8.0, StartMm = 0, EndMm = innerW, Purpose = "Паз під ХДФ 4х8мм" });
            if (m.LedGroove)
            {
                pBottom.Grooves.Add(new GrooveSlot
                {
                    Side = "face_bottom", Orientation = "along_length",
                    OffsetMm = 50.0, WidthMm = 16.0, DepthMm = 7.0, StartMm = 0, EndMm = innerW,
                    Purpose = "Фрезерування паза під врізний LED-профіль Lumines (16х7 мм)"
                });
            }
            AddEdgeConfirmatsAndDowels(pBottom, D);
            proj.Panels.Add(pBottom);

            // 4. Top Panel (Дах вкладний)
            var pTop = CreatePanel(ref seq, m, "Дах верхньої шафи", "body",
                innerW, D, T, ox + T, oy, oz + H - T, innerW, D, T, matBody, eFront, eBody, "", "");
            pTop.Grooves.Add(new GrooveSlot { Side = "face_bottom", Orientation = "along_length", OffsetMm = 16.0, WidthMm = 4.0, DepthMm = 8.0, StartMm = 0, EndMm = innerW, Purpose = "Паз під ХДФ 4х8мм" });
            AddEdgeConfirmatsAndDowels(pTop, D);
            proj.Panels.Add(pTop);

            // 5. Shelves
            int shelves = m.ShelvesCount <= 0 ? 2 : m.ShelvesCount;
            double shelfW = innerW - 2.0;
            double shelfD = D - 35.0;
            for (int s = 1; s <= shelves; s++)
            {
                double sz = Math.Round((H / (shelves + 1)) * s, 1);
                var pShelf = CreatePanel(ref seq, m, "Полиця верхньої шафи #" + s, "shelf",
                    shelfW, shelfD, T, ox + T + 1.0, oy + 10.0, oz + sz, shelfW, shelfD, T, matBody, eFront, eBody, eBody, eBody);
                proj.Panels.Add(pShelf);
                AddHardware(hw, "10820", "Полицетримач 5 мм з кільцем нікель", "шт", 4, 1.2);
            }

            // 6. HDF Back Panel
            var pHdf = CreatePanel(ref seq, m, "Задня стінка верхньої шафи ХДФ", "hdf_back",
                H - 2 * T + 14.0, innerW + 14.0, matHdf.ThicknessMm,
                ox + T - 7.0, oy + D - 19.0, oz + T - 7.0,
                innerW + 14.0, matHdf.ThicknessMm, H - 2 * T + 14.0, matHdf, "", "", "", "");
            proj.Panels.Add(pHdf);

            // 7. Facades (with optional handleless overhang below bottom panel)
            int doors = m.DoorsCount <= 0 ? (W >= 550 ? 2 : 1) : m.DoorsCount;
            double overhang = m.HandlelessOverhangMm;
            double fH = H - 4.0 + overhang;
            double fW = doors == 1 ? (W - 4.0) : Math.Round((W - 7.0) / 2.0, 1);
            for (int d = 0; d < doors; d++)
            {
                double fx = ox + 2.0 + d * (fW + 3.0);
                var pDoor = CreatePanel(ref seq, m, doors == 1 ? "Фасад верхньої шафи" : (d == 0 ? "Фасад верхній лівий" : "Фасад верхній правий"), "facade_door",
                    fH, fW, matFacade.ThicknessMm,
                    fx, oy - matFacade.ThicknessMm - 1.0, oz + 2.0 - overhang,
                    fW, matFacade.ThicknessMm, fH,
                    matFacade, eFacade, eFacade, eFacade, eFacade);

                double cupY = (d == 0) ? 22.5 : (fW - 22.5);
                pDoor.Drilling.Add(new DrillHole { Side = "face_bottom", XMm = 90.0, YMm = cupY, DiameterMm = 35.0, DepthMm = 13.0, Purpose = "Чашка петлі Blum D35x13 (верхня)" });
                pDoor.Drilling.Add(new DrillHole { Side = "face_bottom", XMm = fH - 90.0 - overhang, YMm = cupY, DiameterMm = 35.0, DepthMm = 13.0, Purpose = "Чашка петлі Blum D35x13 (нижня)" });
                proj.Panels.Add(pDoor);
                AddHardware(hw, "71B3550", "Петля накладна Blum CLIP top BLUMOTION 110° + планка", "компл", 2, 165.0);
            }

            AddHardware(hw, "806.19", "Навіс регульований Camar 806 (лівий/правий) для верхніх секцій", "пара", 1, 115.0);
            AddHardware(hw, "10412", "Конфірмат 5х50 мм цинк", "шт", 8, 0.85);
            AddHardware(hw, "10501", "Шкант буковий 8х30 мм", "шт", 8, 0.45);
        }

        // =====================================================================
        // MODULE 4: WARDROBE / TALL CABINET (ШАФА РОЗПАШНА / ПЕНАЛ)
        // =====================================================================
        private static void BuildWardrobe(FurnitureProject proj, ModuleInput m, ref int seq, Dictionary<string, HardwareLine> hw)
        {
            var matBody = ViyarCatalog.GetMaterial(m.BodyMaterialId);
            var matFacade = ViyarCatalog.GetMaterial(m.FacadeMaterialId);
            var matHdf = ViyarCatalog.GetMaterial(m.BackMaterialId);

            double T = matBody.ThicknessMm;
            double W = m.WidthMm;
            double H = m.HeightMm;
            double D = m.DepthMm;
            double plinthH = m.LegHeightMm > 0 ? m.LegHeightMm : 80.0;
            double ox = m.OriginXMm;
            double oy = m.OriginYMm;
            double oz = m.OriginZMm;

            string eBody = ViyarCatalog.GetEdgeCode(matBody.DefaultEdgeBody);
            string eFront = ViyarCatalog.GetEdgeCode(matBody.DefaultEdgeFront);
            string eFacade = ViyarCatalog.GetEdgeCode(matFacade.DefaultEdgeFront);

            double innerW = W - 2 * T;
            double innerH = H - plinthH - 2 * T;

            // 1 & 2. Left & Right Tall Sides
            for (int sideIdx = 0; sideIdx < 2; sideIdx++)
            {
                bool isLeft = (sideIdx == 0);
                var pSide = CreatePanel(ref seq, m, isLeft ? "Стійка шафи ліва" : "Стійка шафи права", "body",
                    H, D, T,
                    isLeft ? ox : (ox + W - T), oy, oz,
                    T, D, H,
                    matBody, eFront, eBody, eBody, eBody);
                AddHorizontalJointFaceHoles(pSide, plinthH + T / 2.0, D, true);
                AddHorizontalJointFaceHoles(pSide, H - T / 2.0, D, true);
                proj.Panels.Add(pSide);
            }

            // 3 & 4. Bottom & Top Horizon Panels
            var pBottom = CreatePanel(ref seq, m, "Дно шафи вкладне", "body",
                innerW, D, T, ox + T, oy, oz + plinthH, innerW, D, T, matBody, eFront, eBody, "", "");
            AddEdgeConfirmatsAndDowels(pBottom, D);
            proj.Panels.Add(pBottom);

            var pTop = CreatePanel(ref seq, m, "Дах шафи вкладний", "body",
                innerW, D, T, ox + T, oy, oz + H - T, innerW, D, T, matBody, eFront, eBody, "", "");
            AddEdgeConfirmatsAndDowels(pTop, D);
            proj.Panels.Add(pTop);

            // 5. Front & Rear Plinths (Цоколь передній та задній)
            var pPlinthF = CreatePanel(ref seq, m, "Цоколь передній", "stretcher",
                innerW, plinthH, T, ox + T, oy + 20.0, oz, innerW, T, plinthH, matBody, eBody, eBody, "", "");
            var pPlinthR = CreatePanel(ref seq, m, "Цоколь задній", "stretcher",
                innerW, plinthH, T, ox + T, oy + D - 40.0, oz, innerW, T, plinthH, matBody, eBody, eBody, "", "");
            proj.Panels.Add(pPlinthF);
            proj.Panels.Add(pPlinthR);

            // 6. Central Vertical Partition (if 2 sections)
            int sections = m.SectionsCount <= 1 ? (W >= 900 ? 2 : 1) : m.SectionsCount;
            if (sections >= 2)
            {
                double partX = ox + T + Math.Round((innerW - T) / 2.0, 1);
                var pDivider = CreatePanel(ref seq, m, "Перегородка вертикальна центральна", "body",
                    innerH, D - 20.0, T, partX, oy + 10.0, oz + plinthH + T, T, D - 20.0, innerH, matBody, eFront, eBody, "", "");
                proj.Panels.Add(pDivider);

                double secW = Math.Round((innerW - T) / 2.0, 1) - 2.0;
                int shelvesCount = m.ShelvesCount <= 0 ? 4 : m.ShelvesCount;
                for (int s = 1; s <= shelvesCount; s++)
                {
                    double sz = oz + plinthH + T + Math.Round((innerH / (shelvesCount + 1)) * s, 1);
                    var pShelf = CreatePanel(ref seq, m, "Полиця секції білизни #" + s, "shelf",
                        secW, D - 30.0, T, ox + T + 1.0, oy + 15.0, sz, secW, D - 30.0, T, matBody, eFront, eBody, eBody, eBody);
                    proj.Panels.Add(pShelf);
                    AddHardware(hw, "10820", "Полицетримач 5 мм з кільцем нікель", "шт", 4, 1.2);
                }

                // Top shelf in hanging section
                var pHangShelf = CreatePanel(ref seq, m, "Полиця антресольна над штангою", "shelf",
                    secW + 2.0, D - 30.0, T, partX + T, oy + 15.0, oz + H - T - 380.0, secW + 2.0, D - 30.0, T, matBody, eFront, eBody, "", "");
                proj.Panels.Add(pHangShelf);
                AddHardware(hw, "15300", "Штанга гардеробна овальна з тримачами", "компл", 1, 180.0);
            }

            // 7. HDF Back Panel
            var pHdf = CreatePanel(ref seq, m, "Задня стінка шафи ХДФ", "hdf_back",
                H - plinthH - 4.0, W - 4.0, matHdf.ThicknessMm,
                ox + 2.0, oy + D, oz + plinthH + 2.0, W - 4.0, matHdf.ThicknessMm, H - plinthH - 4.0, matHdf, "", "", "", "");
            proj.Panels.Add(pHdf);

            // 8. Tall Doors with 4 or 5 Blum Hinges per door
            int doors = m.DoorsCount <= 0 ? 2 : m.DoorsCount;
            double fH = H - plinthH - 4.0;
            double fW = Math.Round((W - 4.0 - (doors - 1) * 3.0) / doors, 1);
            int hingesPerDoor = fH > 2000 ? 4 : (fH > 1500 ? 3 : 2);

            for (int d = 0; d < doors; d++)
            {
                double fx = ox + 2.0 + d * (fW + 3.0);
                var pDoor = CreatePanel(ref seq, m, "Фасад шафи #" + (d + 1), "facade_door",
                    fH, fW, matFacade.ThicknessMm,
                    fx, oy - matFacade.ThicknessMm - 1.0, oz + plinthH + 2.0,
                    fW, matFacade.ThicknessMm, fH,
                    matFacade, eFacade, eFacade, eFacade, eFacade);

                double cupY = (d % 2 == 0) ? 22.5 : (fW - 22.5);
                for (int hIdx = 0; hIdx < hingesPerDoor; hIdx++)
                {
                    double hx = Math.Round(100.0 + hIdx * ((fH - 200.0) / Math.Max(1, hingesPerDoor - 1)), 1);
                    pDoor.Drilling.Add(new DrillHole { Side = "face_bottom", XMm = hx, YMm = cupY, DiameterMm = 35.0, DepthMm = 13.0, Purpose = "Чашка петлі Blum D35x13 #" + (hIdx + 1) });
                }
                proj.Panels.Add(pDoor);
                AddHardware(hw, "71B3550", "Петля накладна Blum CLIP top BLUMOTION 110° + планка", "компл", hingesPerDoor, 165.0);
            }

            AddHardware(hw, "10412", "Конфірмат 5х50 мм цинк", "шт", 20, 0.85);
            AddHardware(hw, "10501", "Шкант буковий 8х30 мм", "шт", 16, 0.45);
        }

        // =====================================================================
        // MODULE 5: OFFICE DESK (СТІЛ ПИСЬМОВИЙ / РОБОЧИЙ)
        // =====================================================================
        private static void BuildOfficeDesk(FurnitureProject proj, ModuleInput m, ref int seq, Dictionary<string, HardwareLine> hw)
        {
            var matBody = ViyarCatalog.GetMaterial(m.BodyMaterialId);
            var matFacade = ViyarCatalog.GetMaterial(m.FacadeMaterialId);
            double T = matBody.ThicknessMm;
            double W = m.WidthMm;
            double H = m.HeightMm > 0 ? m.HeightMm : 750.0;
            double D = m.DepthMm > 0 ? m.DepthMm : 650.0;
            double ox = m.OriginXMm;
            double oy = m.OriginYMm;
            double oz = m.OriginZMm;

            string eBody = ViyarCatalog.GetEdgeCode(matBody.DefaultEdgeBody);
            string eFront = ViyarCatalog.GetEdgeCode(matFacade.DefaultEdgeFront);

            // 1. Worktop (Стільниця)
            var pTop = CreatePanel(ref seq, m, "Стільниця робочого столу", "worktop",
                W, D, T, ox, oy, oz + H - T, W, D, T, matFacade, eFront, eFront, eFront, eFront);
            proj.Panels.Add(pTop);

            // 2 & 3. Left & Right Legs
            double legH = H - T;
            var pLegL = CreatePanel(ref seq, m, "Опора столу ліва", "body",
                legH, D - 40.0, T, ox + 20.0, oy + 20.0, oz, T, D - 40.0, legH, matBody, eFront, eBody, eBody, eBody);
            var pLegR = CreatePanel(ref seq, m, "Опора столу права", "body",
                legH, D - 40.0, T, ox + W - 20.0 - T, oy + 20.0, oz, T, D - 40.0, legH, matBody, eFront, eBody, eBody, eBody);
            proj.Panels.Add(pLegL);
            proj.Panels.Add(pLegR);

            // 4. Modesty Panel / Stretcher (Царга-екран столу)
            double modW = W - 40.0 - 2 * T;
            var pModesty = CreatePanel(ref seq, m, "Царга столу (ребро жорсткості)", "stretcher",
                modW, 300.0, T, ox + 20.0 + T, oy + D - 120.0, oz + legH - 350.0, modW, T, 300.0, matBody, eBody, eBody, "", "");
            AddStretcherEdgeHoles(pModesty);
            proj.Panels.Add(pModesty);

            AddHardware(hw, "11205", "Стяжка ексцентрикова Мініфікс Hafele (D15 + шток)", "компл", 8, 14.5);
            AddHardware(hw, "10412", "Конфірмат 5х50 мм цинк", "шт", 4, 0.85);
        }

        // =====================================================================
        // MODULE 6: TV UNIT / CONSOLE (ТУМБА ПІД ТВ З ШУХЛЯДАМИ ПО БОКАХ АБО ПІДВІСНА З ВІДКИДНИМИ ФАСАДАМИ)
        // =====================================================================
        private static void BuildTvUnit(FurnitureProject proj, ModuleInput m, ref int seq, Dictionary<string, HardwareLine> hw)
        {
            string sub = (m.SubType ?? "").ToLowerInvariant();
            if (sub.Contains("flap") || sub.Contains("floating") || m.SectionsCount >= 4)
            {
                BuildFloatingFlapTvUnit(proj, m, ref seq, hw);
                return;
            }

            var matBody = ViyarCatalog.GetMaterial(string.IsNullOrEmpty(m.BodyMaterialId) ? "egger_h1180_18" : m.BodyMaterialId);
            var matFacade = ViyarCatalog.GetMaterial(string.IsNullOrEmpty(m.FacadeMaterialId) ? "egger_h1180_18" : m.FacadeMaterialId);
            var matHdf = ViyarCatalog.GetMaterial(string.IsNullOrEmpty(m.BackMaterialId) ? "hdf_white_3" : m.BackMaterialId);

            double T = matBody.ThicknessMm; // 18mm
            double W = m.WidthMm > 0 ? m.WidthMm : 1800.0;
            double H = m.HeightMm > 0 ? m.HeightMm : 450.0;
            double D = m.DepthMm > 0 ? m.DepthMm : 400.0;
            double ox = m.OriginXMm;
            double oy = m.OriginYMm;
            double oz = m.OriginZMm;

            string eBody = ViyarCatalog.GetEdgeCode(matBody.DefaultEdgeBody);
            string eFront = ViyarCatalog.GetEdgeCode(matBody.DefaultEdgeFront);
            string eFacade = ViyarCatalog.GetEdgeCode(matFacade.DefaultEdgeFront);

            // Real engineering layout learned from hall_wall_table.skp:
            // - Top panel (__1. tv table top): full width W (1800) x full depth D (400) x 18mm at top (Z = H - T = 432).
            // - Left & Right Outer Side panels (__3, __4): sit directly under Top panel (Z = 0..432, height = 432), full depth D (400), forming a clean frame around inset facades.
            // - Bottom panel (__2. tv table down): sits BETWEEN Left & Right Outer Sides (width = W - 2*T = 1764) at Z = 0..18, recessed 19mm from front (Y = 19..400, depth = 381) so 18mm inset facades sit flush at Y = 0..18!
            // - Two Internal Vertical Partitions (__5, __6): sit between Bottom & Top (Z = 18..432, height = 414), Y = 19..400 (depth = 381), fastened via Muller countersunk Confirmats (D7-DEPTHF-CTRSINK-$44817 + Dside4.5).
            double sideH = H - T;          // 432mm
            double innerW = W - 2 * T;     // 1764mm
            double facadeRecess = T + 1.0; // 19mm front recess for inset 18mm facades
            double bodyD = D - facadeRecess; // 381mm
            double partH = H - 2 * T;      // 414mm
            double secW = Math.Round((innerW - 2 * T) / 3.0, 1); // (1764 - 36)/3 = 576mm per section

            double p1LocalX = secW + T * 0.5;           // 585mm from inner left
            double p2LocalX = 2 * secW + T + T * 0.5;   // 1179mm from inner left

            // 1. __1. tv table top (Стільниця верхня суцільна 1800х400х18)
            var pTop = CreatePanel(ref seq, m, "Стільниця верхня (tv table top)", "worktop",
                W, D, T, ox, oy, oz + sideH, W, D, T, matFacade, eFront, eBody, eFront, eFront, "tv table top");
            // 4 x Hettich Minifix (18 mm) bolts on underside at Left (X=9) & Right (X=W-9) Outer Sides
            foreach (double xSide in new[] { T * 0.5, W - T * 0.5 })
            {
                pTop.Drilling.Add(new DrillHole { Side = "face_bottom", XMm = xSide, YMm = 37.0, DiameterMm = 5.0, DepthMm = 11.5, Purpose = "Мініфікс - Hettich (18 мм) шток D5x11.5" });
                pTop.Drilling.Add(new DrillHole { Side = "face_bottom", XMm = xSide, YMm = D - 56.0, DiameterMm = 5.0, DepthMm = 11.5, Purpose = "Мініфікс - Hettich (18 мм) шток D5x11.5" });
            }
            // 4 x Confirmat countersunk through-holes (D7-DEPTHF-CTRSINK-$44817) for the 2 internal dividers
            foreach (double xDiv in new[] { T + p1LocalX, T + p2LocalX })
            {
                pTop.Drilling.Add(new DrillHole { Side = "face_top", XMm = Math.Round(xDiv, 1), YMm = facadeRecess + 50.0, DiameterMm = 7.0, DepthMm = T, Purpose = "Конфірмат зенкер Muller D7 наскрізний" });
                pTop.Drilling.Add(new DrillHole { Side = "face_top", XMm = Math.Round(xDiv, 1), YMm = D - 56.0, DiameterMm = 7.0, DepthMm = T, Purpose = "Конфірмат зенкер Muller D7 наскрізний" });
            }
            proj.Panels.Add(pTop);

            // 2. __2. tv table down (Дно тумби вкладне 1764х381х18)
            var pBottom = CreatePanel(ref seq, m, "Дно тумби вкладне (tv table down)", "body",
                innerW, bodyD, T, ox + T, oy + facadeRecess, oz, innerW, bodyD, T, matBody, eFront, eBody, "", "", "tv table down");
            // Minifix cams + side drills on Left (X=0) and Right (X=innerW) ends to join to Outer Sides
            foreach (double yPos in new[] { 37.0, bodyD - 56.0 })
            {
                pBottom.Drilling.Add(new DrillHole { Side = "face_top", XMm = 34.0, YMm = yPos, DiameterMm = 15.0, DepthMm = 13.8, Purpose = "Мініфікс - Hettich (18 мм) ексцентрик D15x13.8" });
                pBottom.Drilling.Add(new DrillHole { Side = "edge_w1", XMm = yPos, YMm = 9.0, DiameterMm = 8.0, DepthMm = 33.0, Purpose = "Мініфікс - Hettich (18 мм) торець D8x33" });

                pBottom.Drilling.Add(new DrillHole { Side = "face_top", XMm = innerW - 34.0, YMm = yPos, DiameterMm = 15.0, DepthMm = 13.8, Purpose = "Мініфікс - Hettich (18 мм) ексцентрик D15x13.8" });
                pBottom.Drilling.Add(new DrillHole { Side = "edge_w2", XMm = yPos, YMm = 9.0, DiameterMm = 8.0, DepthMm = 33.0, Purpose = "Мініфікс - Hettich (18 мм) торець D8x33" });
            }
            // 4 x Confirmat countersunk through-holes on underside for the 2 internal dividers
            foreach (double px in new[] { p1LocalX, p2LocalX })
            {
                pBottom.Drilling.Add(new DrillHole { Side = "face_bottom", XMm = Math.Round(px, 1), YMm = 50.0, DiameterMm = 7.0, DepthMm = T, Purpose = "Конфірмат зенкер Muller D7 наскрізний" });
                pBottom.Drilling.Add(new DrillHole { Side = "face_bottom", XMm = Math.Round(px, 1), YMm = bodyD - 56.0, DiameterMm = 7.0, DepthMm = T, Purpose = "Конфірмат зенкер Muller D7 наскрізний" });
            }
            proj.Panels.Add(pBottom);

            // 3 & 4. __3. tv table right side & __4. tv table left side (Боковини 432х400х18)
            for (int sideIdx = 0; sideIdx < 2; sideIdx++)
            {
                bool isRight = (sideIdx == 0); // Match hall_wall_table order: __3 right side, __4 left side
                string uaName = isRight ? "Боковина тумби права (tv table right side)" : "Боковина тумби ліва (tv table left side)";
                string abfName = isRight ? "tv table right side" : "tv table left side";
                string innerFace = isRight ? "face_bottom" : "face_top";

                var pSide = CreatePanel(ref seq, m, uaName, "body",
                    sideH, D, T,
                    isRight ? (ox + W - T) : ox, oy, oz,
                    T, D, sideH,
                    matBody, eFront, eBody, eFront, "", abfName);

                // Top Minifix cam + Dside8 bore into top edge (edge_w2) to lock into __1. tv table top
                foreach (double yPos in new[] { 37.0, D - 56.0 })
                {
                    pSide.Drilling.Add(new DrillHole { Side = innerFace, XMm = sideH - 34.0, YMm = yPos, DiameterMm = 15.0, DepthMm = 13.8, Purpose = "Мініфікс - Hettich (18 мм) ексцентрик D15x13.8" });
                    pSide.Drilling.Add(new DrillHole { Side = "edge_w2", XMm = yPos, YMm = 9.0, DiameterMm = 8.0, DepthMm = 33.0, Purpose = "Мініфікс - Hettich (18 мм) торець D8x33" });
                }
                // Bottom Minifix bolt holes (D5-DEPTH11.5-$57722) at Z = 9mm to receive __2. tv table down
                foreach (double yPos in new[] { facadeRecess + 37.0, D - 56.0 })
                {
                    pSide.Drilling.Add(new DrillHole { Side = innerFace, XMm = 9.0, YMm = yPos, DiameterMm = 5.0, DepthMm = 11.5, Purpose = "Мініфікс - Hettich (18 мм) шток дна D5x11.5" });
                }
                // Drawer slide mounting holes (D5-DEPTH8.5-$52636)
                pSide.Drilling.Add(new DrillHole { Side = innerFace, XMm = 55.0, YMm = facadeRecess + 37.0, DiameterMm = 5.0, DepthMm = 8.5, Purpose = "Єврогвинт напрямної шухляди D5x8.5" });
                pSide.Drilling.Add(new DrillHole { Side = innerFace, XMm = 55.0, YMm = facadeRecess + 261.0, DiameterMm = 5.0, DepthMm = 8.5, Purpose = "Єврогвинт напрямної шухляди D5x8.5" });

                proj.Panels.Add(pSide);
            }

            // 5 & 6. __5. tv table mid left & __6. tv table mid right (Перегородки внутрішні 414х381х18)
            double[] partWorldX = new[] { ox + T + secW, ox + T + 2 * secW + T };
            for (int pIdx = 0; pIdx < 2; pIdx++)
            {
                bool isLeftMid = (pIdx == 0);
                var pDiv = CreatePanel(ref seq, m,
                    isLeftMid ? "Перегородка внутрішня ліва (tv table mid left)" : "Перегородка внутрішня права (tv table mid right)",
                    "body",
                    partH, bodyD, T,
                    partWorldX[pIdx], oy + facadeRecess, oz + T,
                    T, bodyD, partH,
                    matBody, eFront, eBody, "", "",
                    isLeftMid ? "tv table mid left" : "tv table mid right");

                // Bottom (edge_w1) and Top (edge_w2) Confirmat end-grain bores (Dside4.5)
                foreach (double yPos in new[] { 50.0, bodyD - 56.0 })
                {
                    pDiv.Drilling.Add(new DrillHole { Side = "edge_w1", XMm = yPos, YMm = 9.0, DiameterMm = 4.5, DepthMm = 34.0, Purpose = "Конфірмат зенкер Muller торець D4.5x34 (дно)" });
                    pDiv.Drilling.Add(new DrillHole { Side = "edge_w2", XMm = yPos, YMm = 9.0, DiameterMm = 4.5, DepthMm = 34.0, Purpose = "Конфірмат зенкер Muller торець D4.5x34 (верх)" });
                }
                // Shelf pin holes (Полицетримач оцинкований 5x16) on center-facing side
                string centerFace = isLeftMid ? "face_top" : "face_bottom";
                string drawerFace = isLeftMid ? "face_bottom" : "face_top";
                double midZ = Math.Round(partH * 0.5, 1);
                pDiv.Drilling.Add(new DrillHole { Side = centerFace, XMm = midZ, YMm = 50.0, DiameterMm = 5.0, DepthMm = 11.5, Purpose = "Полицетримач оцинкований 5x16" });
                pDiv.Drilling.Add(new DrillHole { Side = centerFace, XMm = midZ, YMm = bodyD - 56.0, DiameterMm = 5.0, DepthMm = 11.5, Purpose = "Полицетримач оцинкований 5x16" });

                // Hinge mounting plates (_ABF_hingeMountingPlate, D2x3.0) on left divider for center inset door
                if (isLeftMid)
                {
                    pDiv.Drilling.Add(new DrillHole { Side = centerFace, XMm = 75.0, YMm = 37.0, DiameterMm = 2.0, DepthMm = 3.0, Purpose = "Планка завіси внутрішньої D2x3.0" });
                    pDiv.Drilling.Add(new DrillHole { Side = centerFace, XMm = partH - 75.0, YMm = 37.0, DiameterMm = 2.0, DepthMm = 3.0, Purpose = "Планка завіси внутрішньої D2x3.0" });
                }

                // Drawer slide mounting holes on drawer-facing side
                pDiv.Drilling.Add(new DrillHole { Side = drawerFace, XMm = 37.0, YMm = 37.0, DiameterMm = 5.0, DepthMm = 8.5, Purpose = "Єврогвинт напрямної шухляди D5x8.5" });
                pDiv.Drilling.Add(new DrillHole { Side = drawerFace, XMm = 37.0, YMm = 261.0, DiameterMm = 5.0, DepthMm = 8.5, Purpose = "Єврогвинт напрямної шухляди D5x8.5" });

                proj.Panels.Add(pDiv);
            }

            // 7. __7. tv table center shelf (Полиця центральної медіа-ніші 574х360х18)
            double centerShelfW = secW - 2.0; // 574mm
            double centerShelfD = bodyD - 21.0; // 360mm
            var pCenterShelf = CreatePanel(ref seq, m, "Полиця центральної секції (tv table center shelf)", "shelf",
                centerShelfW, centerShelfD, T,
                partWorldX[0] + T + 1.0, oy + facadeRecess + 10.0, oz + T + Math.Round(partH * 0.5, 1),
                centerShelfW, centerShelfD, T,
                matBody, eFront, eBody, eBody, eBody, "tv table center shelf");
            proj.Panels.Add(pCenterShelf);

            // 8. __8. tv table center door (Фасад центральної секції вкладний, шар Facade)
            // Inset under Top panel (Z = 1.5..430.5, height = 429mm), flush with Y = 0..18mm
            double facadeH = sideH - 3.0; // 429mm
            double centerDoorW = secW + T - 3.0; // 591mm
            var pCenterDoor = CreatePanel(ref seq, m, "Фасад центральної секції (tv table center door)", "facade_door",
                centerDoorW, facadeH, matFacade.ThicknessMm,
                partWorldX[0] + (T * 0.5) + 1.5, oy, oz + 1.5,
                centerDoorW, matFacade.ThicknessMm, facadeH,
                matFacade, eFacade, eFacade, eFacade, eFacade, "tv table center door");
            pCenterDoor.Drilling.Add(new DrillHole { Side = "face_top", XMm = 21.5, YMm = 91.5, DiameterMm = 35.0, DepthMm = 12.0, Purpose = "Чашка завіси внутрішньої D35x12" });
            pCenterDoor.Drilling.Add(new DrillHole { Side = "face_top", XMm = 21.5, YMm = facadeH - 91.5, DiameterMm = 35.0, DepthMm = 12.0, Purpose = "Чашка завіси внутрішньої D35x12" });
            proj.Panels.Add(pCenterDoor);
            AddHardware(hw, "97743", "Завіса внутрішня Muller / Blum CLIP top 110° з доводчиком", "компл", 2, 165.0);

            // 9..18. Two Side Pull-Out Drawers (__9..__13 Right Drawer, __14..__18 Left Drawer)
            // Exact hall_wall_table.skp (__71..__78) & Drawer with fittings.skp construction:
            // - 13.0mm slide clearance per side (drawerOuterW = secW - 2*13 = 550mm; drawerBackW = 550 - 2*18 = 514mm)
            // - Drawer Front attached to Drawer Left/Right Sides via 4 x Hettich Minifix (18 mm) (D15-DEPTH13.8-$61281 + Dside8 + D5-DEPTH11.5-$57722)
            // - Drawer Left/Right Sides attached to Drawer Back via 4 x Confirmat countersunk screws (D7-DEPTHF-CTRSINK-$44817 + Dside4.5)
            double slideClearance = 13.0;
            double drawBoxDepth = 340.0;
            double drawBoxH = 260.0;
            double drawOuterW = secW - 2 * slideClearance; // 576 - 26 = 550mm
            double drawBackW = drawOuterW - 2 * T;         // 550 - 36 = 514mm
            double drawFacadeW = secW + (T * 0.5) - 3.0;   // 582mm

            for (int dIdx = 0; dIdx < 2; dIdx++)
            {
                bool isRightDraw = (dIdx == 0); // Match hall_wall_table order: right draw first, then left draw
                string sideEn = isRightDraw ? "right" : "left";
                string sideUa = isRightDraw ? "правої" : "лівої";
                double secStartX = isRightDraw ? (partWorldX[1] + T) : (ox + T);
                double facadeX = isRightDraw ? (partWorldX[1] + (T * 0.5) + 1.5) : (ox + T + 1.5);
                double boxLeftX = secStartX + slideClearance;
                double boxY = oy + matFacade.ThicknessMm; // Directly behind the 18mm inset Drawer Front (Y = 18..358)
                double boxZ = oz + T + 22.0;              // Z = 40..300mm

                // A. Drawer Front (Facade layer)
                var pDrawFront = CreatePanel(ref seq, m,
                    "Фасад шухляди " + sideUa + " (tv table " + sideEn + " draw)",
                    "facade_drawer",
                    drawFacadeW, facadeH, matFacade.ThicknessMm,
                    facadeX, oy, oz + 1.5,
                    drawFacadeW, matFacade.ThicknessMm, facadeH,
                    matFacade, eFacade, eFacade, eFacade, eFacade,
                    "tv table " + sideEn + " draw");

                double leftSideCenterOnFacade = (boxLeftX + T * 0.5) - facadeX;
                double rightSideCenterOnFacade = (boxLeftX + drawOuterW - T * 0.5) - facadeX;
                foreach (double xOnF in new[] { leftSideCenterOnFacade, rightSideCenterOnFacade })
                {
                    pDrawFront.Drilling.Add(new DrillHole { Side = "face_top", XMm = Math.Round(xOnF, 1), YMm = 105.0, DiameterMm = 5.0, DepthMm = 11.5, Purpose = "Мініфікс - Hettich (18 мм) кріплення фасаду шухляди" });
                    pDrawFront.Drilling.Add(new DrillHole { Side = "face_top", XMm = Math.Round(xOnF, 1), YMm = 245.0, DiameterMm = 5.0, DepthMm = 11.5, Purpose = "Мініфікс - Hettich (18 мм) кріплення фасаду шухляди" });
                }
                proj.Panels.Add(pDrawFront);

                // B. Drawer Side Left (draw side left)
                var pDrawSideL = CreatePanel(ref seq, m,
                    "Боковина шухляди " + sideUa + " ліва (tv table " + sideEn + " draw side left)",
                    "drawer_side",
                    drawBoxDepth, drawBoxH, T,
                    boxLeftX, boxY, boxZ,
                    T, drawBoxDepth, drawBoxH,
                    matBody, eBody, eBody, eBody, eBody,
                    "tv table " + sideEn + " draw side left");
                foreach (double zLocal in new[] { 66.5, 206.5 })
                {
                    // Front Minifix cam + Dside8 bore into front edge (edge_l1) to attach Drawer Front
                    pDrawSideL.Drilling.Add(new DrillHole { Side = "face_top", XMm = 34.0, YMm = zLocal, DiameterMm = 15.0, DepthMm = 13.8, Purpose = "Мініфікс - Hettich (18 мм) ексцентрик фасаду шухляди" });
                    pDrawSideL.Drilling.Add(new DrillHole { Side = "edge_l1", XMm = zLocal, YMm = 9.0, DiameterMm = 8.0, DepthMm = 33.0, Purpose = "Мініфікс - Hettich (18 мм) торець фасаду шухляди" });
                    // Rear Confirmat countersunk through-hole (D7-DEPTHF-CTRSINK-$44817) into Drawer Back
                    pDrawSideL.Drilling.Add(new DrillHole { Side = "face_bottom", XMm = drawBoxDepth - 9.0, YMm = zLocal, DiameterMm = 7.0, DepthMm = T, Purpose = "Конфірмат зенкер Muller задньої стінки шухляди" });
                }
                proj.Panels.Add(pDrawSideL);

                // C. Drawer Side Right (draw side right)
                var pDrawSideR = CreatePanel(ref seq, m,
                    "Боковина шухляди " + sideUa + " права (tv table " + sideEn + " draw side right)",
                    "drawer_side",
                    drawBoxDepth, drawBoxH, T,
                    boxLeftX + drawOuterW - T, boxY, boxZ,
                    T, drawBoxDepth, drawBoxH,
                    matBody, eBody, eBody, eBody, eBody,
                    "tv table " + sideEn + " draw side right");
                foreach (double zLocal in new[] { 66.5, 206.5 })
                {
                    pDrawSideR.Drilling.Add(new DrillHole { Side = "face_bottom", XMm = 34.0, YMm = zLocal, DiameterMm = 15.0, DepthMm = 13.8, Purpose = "Мініфікс - Hettich (18 мм) ексцентрик фасаду шухляди" });
                    pDrawSideR.Drilling.Add(new DrillHole { Side = "edge_l1", XMm = zLocal, YMm = 9.0, DiameterMm = 8.0, DepthMm = 33.0, Purpose = "Мініфікс - Hettich (18 мм) торець фасаду шухляди" });
                    pDrawSideR.Drilling.Add(new DrillHole { Side = "face_top", XMm = drawBoxDepth - 9.0, YMm = zLocal, DiameterMm = 7.0, DepthMm = T, Purpose = "Конфірмат зенкер Muller задньої стінки шухляди" });
                }
                proj.Panels.Add(pDrawSideR);

                // D. Drawer Back (draw back — inset between Left & Right Drawer Sides)
                var pDrawBack = CreatePanel(ref seq, m,
                    "Задня стінка шухляди " + sideUa + " (tv table " + sideEn + " draw back)",
                    "drawer_box",
                    drawBackW, drawBoxH, T,
                    boxLeftX + T, boxY + drawBoxDepth - T, boxZ,
                    drawBackW, T, drawBoxH,
                    matBody, eBody, eBody, "", "",
                    "tv table " + sideEn + " draw back");
                foreach (double zLocal in new[] { 66.5, 206.5 })
                {
                    pDrawBack.Drilling.Add(new DrillHole { Side = "edge_w1", XMm = zLocal, YMm = 9.0, DiameterMm = 4.5, DepthMm = 34.0, Purpose = "Конфірмат зенкер Muller торець D4.5x34" });
                    pDrawBack.Drilling.Add(new DrillHole { Side = "edge_w2", XMm = zLocal, YMm = 9.0, DiameterMm = 4.5, DepthMm = 34.0, Purpose = "Конфірмат зенкер Muller торець D4.5x34" });
                }
                proj.Panels.Add(pDrawBack);

                // E. Drawer Bottom HDF 3mm (draw bottom)
                var pDrawBot = CreatePanel(ref seq, m,
                    "Дно шухляди " + sideUa + " ХДФ (tv table " + sideEn + " draw bottom)",
                    "hdf_drawer_bottom",
                    drawOuterW, drawBoxDepth, matHdf.ThicknessMm,
                    boxLeftX, boxY, boxZ - matHdf.ThicknessMm,
                    drawOuterW, drawBoxDepth, matHdf.ThicknessMm,
                    matHdf, "", "", "", "",
                    "tv table " + sideEn + " draw bottom");
                proj.Panels.Add(pDrawBot);

                AddHardware(hw, "52505", "Направляючі кулькові телескопічні повного висування L=350 мм (зазор 13 мм)", "компл", 1, 340.0);
            }

            // 19. __19. tv table back hdf (Задня стінка ХДФ 3мм)
            var pHdf = CreatePanel(ref seq, m, "Задня стінка ХДФ 3мм (tv table back hdf)", "hdf_back",
                W - 4.0, sideH - 4.0, matHdf.ThicknessMm,
                ox + 2.0, oy + D, oz + 2.0,
                W - 4.0, matHdf.ThicknessMm, sideH - 4.0,
                matHdf, "", "", "", "", "tv table back hdf");
            proj.Panels.Add(pHdf);

            AddHardware(hw, "61281", "Мініфікс ексцентрик Hettich D15 (для плити 18 мм) + шток D5 (57722)", "компл", 16, 14.5);
            AddHardware(hw, "44817", "Конфірмат зенкер Muller 7х50 мм цинк", "шт", 16, 0.95);
            AddHardware(hw, "57722", "Полицетримач оцинкований 5х16 мм", "шт", 4, 1.2);
            AddHardware(hw, "807.29", "Підвіс посилений Camar 807 для підвісних ТВ-тумб", "пара", 2, 290.0);
        }

        // =====================================================================
        // MODULE 6B: FLOATING MINIMALIST TV STAND WITH PROTRUDING PERIMETER FRAME & RECESSED INSET FLAP DOORS
        // (ПІДВІСНА МІНІМАЛІСТИЧНА ТВ-ТУМБА H=250 З ВИСТУПАЮЧОЮ РАМКОЮ ПО ПЕРИМЕТРУ ТА ВТОПЛЕНИМИ ВІДКИДНИМИ ДВЕРЦЯТАМИ)
        // =====================================================================
        private static void BuildFloatingFlapTvUnit(FurnitureProject proj, ModuleInput m, ref int seq, Dictionary<string, HardwareLine> hw)
        {
            var matBody = ViyarCatalog.GetMaterial(string.IsNullOrEmpty(m.BodyMaterialId) ? "kronospan_k009_18" : m.BodyMaterialId);
            var matFacade = ViyarCatalog.GetMaterial(string.IsNullOrEmpty(m.FacadeMaterialId) ? "kronospan_k009_18" : m.FacadeMaterialId);
            var matHdf = ViyarCatalog.GetMaterial(string.IsNullOrEmpty(m.BackMaterialId) ? "hdf_white_3" : m.BackMaterialId);

            double T = matBody.ThicknessMm; // 18.0 mm
            double W = m.WidthMm > 0 ? m.WidthMm : 2760.0;
            double H = m.HeightMm > 0 ? m.HeightMm : 250.0;
            double D = m.DepthMm > 0 ? m.DepthMm : 300.0;
            double ox = m.OriginXMm;
            double oy = m.OriginYMm;
            double oz = m.OriginZMm > 0 ? m.OriginZMm : (m.LegHeightMm > 0 ? m.LegHeightMm : 150.0); // 150mm robot vacuum clearance

            int sections = m.SectionsCount > 0 ? m.SectionsCount : 4;
            int dividersCount = sections - 1;

            string eBody = ViyarCatalog.GetEdgeCode(matBody.DefaultEdgeBody);
            string eFront = ViyarCatalog.GetEdgeCode(matBody.DefaultEdgeFront);
            string eFacade = ViyarCatalog.GetEdgeCode(matFacade.DefaultEdgeFront);

            // Architecture (matching reference photo):
            // - Outer Perimeter Frame (Top, Bottom, Left Side, Right Side) has FULL depth D (300mm, Y = oy..oy+300) so its 18mm front edge protrudes all around!
            // - 4 Inset Flap Doors sit INSIDE the perimeter window (width = W - 2*T = 2724mm, height = H - 2*T = 214mm) and are recessed by doorSetbackY = 5.0mm from the front plane (Y = oy + 5..23mm)!
            // - 3 Internal Vertical Dividers sit BEHIND the inset doors (Y = oy + 25..300mm, depth = 275mm, height = 214mm) so no vertical divider edges interrupt the 4 doors from the front!
            double sideH = H - T;                  // 250 - 18 = 232.0 mm
            double innerW = W - 2 * T;             // 2742 - 36 = 2706.0 mm
            double partH = H - 2 * T;              // 250 - 36 = 214.0 mm
            double doorSetbackY = 5.0;             // 5.0 mm perimeter frame protrusion in front of inset doors
            double hdfT = matHdf.ThicknessMm;      // 3.0 mm HDF back inset flush inside the rear of the frame (Y = 297..300mm)
            double divRecessY = doorSetbackY + T + 2.0; // 5 + 18 + 2 = 25.0 mm front recess for internal dividers
            double divD = D - divRecessY - hdfT;   // 300 - 25 - 3 = 272.0 mm (touches inset HDF back at Y = 297mm)

            // Exact door sizing inside innerW (2706mm): 1.5mm outer gaps + 3.0mm gaps between doors -> (2706 - 12) / 4 = 673.5 mm
            double outerGapX = 1.5;
            double interDoorGapX = 3.0;
            double doorW = Math.Round((innerW - 2 * outerGapX - (sections - 1) * interDoorGapX) / sections, 1); // 673.5 mm
            double doorH = Math.Round(partH - 4.0, 1); // 214 - 4 = 210.0 mm
            double stepX = doorW + interDoorGapX;      // 676.5 mm pitch inside innerW

            // 1. __1. tv table top (Стільниця верхня суцільна рамки 2742х300х18)
            var pTop = CreatePanel(ref seq, m, "Стільниця верхня рамки (tv table top)", "worktop",
                W, D, T, ox, oy, oz + sideH, W, D, T, matBody, eFront, eBody, eFront, eFront, "tv table top");

            // Blind Minifix bolts (D5-DEPTH11.5-$57722) on underside (face_bottom) for Left Side, Right Side, and all 3 Internal Dividers (ZERO holes on top face!)
            double yFrontSideMfx = 37.0;
            double yFrontDivMfx = divRecessY + 37.0; // 62.0 mm
            double yRearMfx = D - 56.0;              // 244.0 mm
            foreach (double xSide in new[] { T * 0.5, W - T * 0.5 })
            {
                pTop.Drilling.Add(new DrillHole { Side = "face_bottom", XMm = xSide, YMm = yFrontSideMfx, DiameterMm = 5.0, DepthMm = 11.5, Purpose = "Мініфікс - Hettich (18 мм) шток боковини D5x11.5" });
                pTop.Drilling.Add(new DrillHole { Side = "face_bottom", XMm = xSide, YMm = yRearMfx, DiameterMm = 5.0, DepthMm = 11.5, Purpose = "Мініфікс - Hettich (18 мм) шток боковини D5x11.5" });
            }
            for (int k = 1; k <= dividersCount; k++)
            {
                double divCenterXInTop = Math.Round(T + outerGapX + k * stepX - interDoorGapX * 0.5, 1);
                pTop.Drilling.Add(new DrillHole { Side = "face_bottom", XMm = divCenterXInTop, YMm = yFrontDivMfx, DiameterMm = 5.0, DepthMm = 11.5, Purpose = "Мініфікс - Hettich (18 мм) шток перегородки #" + k + " D5x11.5" });
                pTop.Drilling.Add(new DrillHole { Side = "face_bottom", XMm = divCenterXInTop, YMm = yRearMfx, DiameterMm = 5.0, DepthMm = 11.5, Purpose = "Мініфікс - Hettich (18 мм) шток перегородки #" + k + " D5x11.5" });
            }
            // Tip-On push-to-open adapter plate pilot holes (D2x3.0, 32mm spacing along Y at Y=42mm & 74mm) under pTop above each of the 4 flap doors
            for (int s = 0; s < sections; s++)
            {
                double tipOnXInTop = Math.Round(T + outerGapX + s * stepX + doorW * 0.5, 1);
                pTop.Drilling.Add(new DrillHole { Side = "face_bottom", XMm = tipOnXInTop, YMm = doorSetbackY + T + 19.0, DiameterMm = 2.0, DepthMm = 3.0, Purpose = "Планка-адаптер Tip-On відкидного фасаду #" + (s + 1) + " D2x3.0" });
            }
            proj.Panels.Add(pTop);

            // 2. __2. tv table down (Дно тумби рамки на всю глибину 2724х300х18)
            var pBottom = CreatePanel(ref seq, m, "Дно тумби рамки (tv table down)", "body",
                innerW, D, T, ox + T, oy, oz, innerW, D, T, matBody, eFront, eBody, "", "", "tv table down");

            // Minifix cams + side drills on Left (X=0) and Right (X=innerW) ends to join to Outer Sides
            foreach (double yPos in new[] { 37.0, D - 56.0 })
            {
                pBottom.Drilling.Add(new DrillHole { Side = "face_top", XMm = 34.0, YMm = yPos, DiameterMm = 15.0, DepthMm = 13.8, Purpose = "Мініфікс - Hettich (18 мм) ексцентрик D15x13.8" });
                pBottom.Drilling.Add(new DrillHole { Side = "edge_w1", XMm = yPos, YMm = 9.0, DiameterMm = 8.0, DepthMm = 33.0, Purpose = "Мініфікс - Hettich (18 мм) торець D8x33" });

                pBottom.Drilling.Add(new DrillHole { Side = "face_top", XMm = innerW - 34.0, YMm = yPos, DiameterMm = 15.0, DepthMm = 13.8, Purpose = "Мініфікс - Hettich (18 мм) ексцентрик D15x13.8" });
                pBottom.Drilling.Add(new DrillHole { Side = "edge_w2", XMm = yPos, YMm = 9.0, DiameterMm = 8.0, DepthMm = 33.0, Purpose = "Мініфікс - Hettich (18 мм) торець D8x33" });
            }
            // Countersunk Confirmat through-holes on underside (face_bottom) for the 3 internal dividers
            for (int k = 1; k <= dividersCount; k++)
            {
                double pxOnBot = Math.Round(outerGapX + k * stepX - interDoorGapX * 0.5, 1); // 681.0, 1362.0, 2043.0 mm
                pBottom.Drilling.Add(new DrillHole { Side = "face_bottom", XMm = pxOnBot, YMm = divRecessY + 50.0, DiameterMm = 7.0, DepthMm = T, Purpose = "Конфірмат зенкер Muller D7 наскрізний (перегородка #" + k + ")" });
                pBottom.Drilling.Add(new DrillHole { Side = "face_bottom", XMm = pxOnBot, YMm = D - 56.0, DiameterMm = 7.0, DepthMm = T, Purpose = "Конфірмат зенкер Muller D7 наскрізний (перегородка #" + k + ")" });
            }
            // Hinge mounting plates (_ABF_hingeMountingPlate, D2x3.0) on top face of bottom panel behind inset doors (Y = doorSetbackY + T + 37 = 60mm)
            double plateYOnBot = doorSetbackY + T + 37.0; // 60.0 mm
            for (int s = 0; s < sections; s++)
            {
                double doorStartOnBot = outerGapX + s * stepX;
                double h1XOnBot = Math.Round(doorStartOnBot + 90.0, 1);
                double h2XOnBot = Math.Round(doorStartOnBot + doorW - 90.0, 1);
                pBottom.Drilling.Add(new DrillHole { Side = "face_top", XMm = h1XOnBot, YMm = plateYOnBot, DiameterMm = 2.0, DepthMm = 3.0, Purpose = "Планка завіси внутрішньої відкидного фасаду #" + (s + 1) + " D2x3.0" });
                pBottom.Drilling.Add(new DrillHole { Side = "face_top", XMm = h2XOnBot, YMm = plateYOnBot, DiameterMm = 2.0, DepthMm = 3.0, Purpose = "Планка завіси внутрішньої відкидного фасаду #" + (s + 1) + " D2x3.0" });
            }
            proj.Panels.Add(pBottom);

            // 3 & 4. __3. tv table right side & __4. tv table left side (Боковини рамки на всю глибину 232х300х18)
            for (int sideIdx = 0; sideIdx < 2; sideIdx++)
            {
                bool isRight = (sideIdx == 0);
                string uaName = isRight ? "Боковина рамки права (tv table right side)" : "Боковина рамки ліва (tv table left side)";
                string abfName = isRight ? "tv table right side" : "tv table left side";
                string innerFace = isRight ? "face_bottom" : "face_top";

                var pSide = CreatePanel(ref seq, m, uaName, "body",
                    sideH, D, T,
                    isRight ? (ox + W - T) : ox, oy, oz,
                    T, D, sideH,
                    matBody, eFront, eBody, eFront, "", abfName);

                // Top Minifix cam + Dside8 bore into top edge (edge_w2) to lock into __1. tv table top
                foreach (double yPos in new[] { 37.0, D - 56.0 })
                {
                    pSide.Drilling.Add(new DrillHole { Side = innerFace, XMm = sideH - 34.0, YMm = yPos, DiameterMm = 15.0, DepthMm = 13.8, Purpose = "Мініфікс - Hettich (18 мм) ексцентрик D15x13.8" });
                    pSide.Drilling.Add(new DrillHole { Side = "edge_w2", XMm = yPos, YMm = 9.0, DiameterMm = 8.0, DepthMm = 33.0, Purpose = "Мініфікс - Hettich (18 мм) торець D8x33" });
                }
                // Bottom Minifix bolt holes (D5-DEPTH11.5-$57722) at Z = 9mm to receive __2. tv table down
                foreach (double yPos in new[] { 37.0, D - 56.0 })
                {
                    pSide.Drilling.Add(new DrillHole { Side = innerFace, XMm = 9.0, YMm = yPos, DiameterMm = 5.0, DepthMm = 11.5, Purpose = "Мініфікс - Hettich (18 мм) шток дна D5x11.5" });
                }
                // Heavy-duty concealed wall hanger Camar 807 mounting holes (2 x D5x11.5 euroscrews at top-rear Z_world=330 & 362mm, Y_world=265mm)
                pSide.Drilling.Add(new DrillHole { Side = innerFace, XMm = sideH - 52.0, YMm = D - 35.0, DiameterMm = 5.0, DepthMm = 11.5, Purpose = "Підвіс посилений Camar 807 єврогвинт D5x11.5 (нижній)" });
                pSide.Drilling.Add(new DrillHole { Side = innerFace, XMm = sideH - 20.0, YMm = D - 35.0, DiameterMm = 5.0, DepthMm = 11.5, Purpose = "Підвіс посилений Camar 807 єврогвинт D5x11.5 (верхній)" });

                // Soft-down gas strut cabinet bracket pilot hole (D2x3.0) on left outer side (for flap door #1)
                if (!isRight)
                {
                    pSide.Drilling.Add(new DrillHole { Side = innerFace, XMm = 170.0, YMm = 135.0, DiameterMm = 2.0, DepthMm = 3.0, Purpose = "Кронштейн газового амортизатора плавного опускання секції #1 D2x3.0" });
                }

                proj.Panels.Add(pSide);
            }

            // 5..7. Internal Vertical Dividers behind inset doors (__5..__7: 214х272х18)
            for (int k = 1; k <= dividersCount; k++)
            {
                double divCenterOnBot = outerGapX + k * stepX - interDoorGapX * 0.5;
                double divWorldX = Math.Round(ox + T + divCenterOnBot - T * 0.5, 1);
                string divUa = "Перегородка внутрішня #" + k + (k == 2 ? " центральна" : "") + " (tv table divider " + k + ")";
                string divAbf = "tv table divider " + k;

                var pDiv = CreatePanel(ref seq, m, divUa, "body",
                    partH, divD, T,
                    divWorldX, oy + divRecessY, oz + T,
                    T, divD, partH,
                    matBody, eFront, "", "", "", divAbf);

                // Top: Minifix cam + Dside8 bore into top edge (edge_w2) to lock into __1. tv table top
                foreach (double yPos in new[] { 37.0, divD - 53.0 })
                {
                    pDiv.Drilling.Add(new DrillHole { Side = "face_top", XMm = partH - 34.0, YMm = yPos, DiameterMm = 15.0, DepthMm = 13.8, Purpose = "Мініфікс - Hettich (18 мм) ексцентрик верху D15x13.8" });
                    pDiv.Drilling.Add(new DrillHole { Side = "edge_w2", XMm = yPos, YMm = 9.0, DiameterMm = 8.0, DepthMm = 33.0, Purpose = "Мініфікс - Hettich (18 мм) торець верху D8x33" });
                }
                // Bottom (edge_w1): Confirmat end-grain bores (Dside4.5) to fasten to __2. tv table down
                foreach (double yPos in new[] { 50.0, divD - 53.0 })
                {
                    pDiv.Drilling.Add(new DrillHole { Side = "edge_w1", XMm = yPos, YMm = 9.0, DiameterMm = 4.5, DepthMm = 34.0, Purpose = "Конфірмат зенкер Muller торець D4.5x34 (дно)" });
                }
                // Heavy-duty concealed wall hanger Camar 807 mounting holes on each internal divider (Z_world=330 & 362mm, Y_world=265mm)
                pDiv.Drilling.Add(new DrillHole { Side = "face_top", XMm = partH - 52.0, YMm = divD - 32.0, DiameterMm = 5.0, DepthMm = 11.5, Purpose = "Підвіс посилений Camar 807 перегородки #" + k + " D5x11.5 (нижній)" });
                pDiv.Drilling.Add(new DrillHole { Side = "face_top", XMm = partH - 20.0, YMm = divD - 32.0, DiameterMm = 5.0, DepthMm = 11.5, Purpose = "Підвіс посилений Camar 807 перегородки #" + k + " D5x11.5 (верхній)" });
                // Soft-down gas strut cabinet bracket pilot hole (D2x3.0) on right face of divider (for flap door #(k+1))
                pDiv.Drilling.Add(new DrillHole { Side = "face_top", XMm = 152.0, YMm = 135.0 - divRecessY, DiameterMm = 2.0, DepthMm = 3.0, Purpose = "Кронштейн газового амортизатора плавного опускання секції #" + (k + 1) + " D2x3.0" });

                proj.Panels.Add(pDiv);
            }

            // 8..11. 4 Identical Inset Drop-Down Flap Doors recessed 5mm inside the perimeter frame (__8..__11: 673.5х210х18, layer Facade)
            for (int s = 0; s < sections; s++)
            {
                double fx = Math.Round(ox + T + outerGapX + s * stepX, 1);
                var pFlap = CreatePanel(ref seq, m,
                    "Фасад вкладний втоплений #" + (s + 1) + " (tv table flap door " + (s + 1) + ")", "facade_door",
                    doorW, doorH, matFacade.ThicknessMm,
                    fx, oy + doorSetbackY, oz + T + 2.0,
                    doorW, matFacade.ThicknessMm, doorH,
                    matFacade, eFacade, eFacade, eFacade, eFacade,
                    "tv table flap door " + (s + 1));

                // 2 x Bottom Ø35x12mm Inset Hinge Cups (Y = 21.5mm from bottom edge of flap door, X = 90mm from left & right edges)
                pFlap.Drilling.Add(new DrillHole { Side = "face_top", XMm = 90.0, YMm = 21.5, DiameterMm = 35.0, DepthMm = 12.0, Purpose = "Чашка завіси внутрішньої відкидного фасаду D35x12 (ліва)" });
                pFlap.Drilling.Add(new DrillHole { Side = "face_top", XMm = doorW - 90.0, YMm = 21.5, DiameterMm = 35.0, DepthMm = 12.0, Purpose = "Чашка завіси внутрішньої відкидного фасаду D35x12 (права)" });
                // Soft-down gas strut door bracket pilot hole (D2x3.0 at X=32mm from left edge, Y=130mm from bottom edge)
                pFlap.Drilling.Add(new DrillHole { Side = "face_top", XMm = 32.0, YMm = 130.0, DiameterMm = 2.0, DepthMm = 3.0, Purpose = "Кронштейн фасадний газового амортизатора секції #" + (s + 1) + " D2x3.0" });

                proj.Panels.Add(pFlap);
                AddHardware(hw, "97743", "Завіса внутрішня (вкладна) Muller / Blum CLIP top 110° для відкидного фасаду", "компл", 2, 165.0);
                AddHardware(hw, "11361", "Механізм Tip-On (відкривання від натискання) довгий з магнітом", "компл", 1, 185.0);
                AddHardware(hw, "84412", "Газовий амортизатор зворотньої дії (плавне опускання вниз) N=60", "шт", 1, 160.0);
            }

            // 12 & 13. Split 3mm HDF Back Panels inset flush inside the rear of the perimeter frame (__12 left, __13 right: 1353х214х3 at Y = 297..300mm)
            double hdfHalfW = Math.Round(innerW * 0.5, 1); // 1353.0 mm
            double hdfH = partH;                           // 214.0 mm
            var pHdfL = CreatePanel(ref seq, m, "Задня стінка ХДФ внутрішня ліва (tv table back hdf left)", "hdf_back",
                hdfHalfW, hdfH, matHdf.ThicknessMm,
                ox + T, oy + D - hdfT, oz + T,
                hdfHalfW, matHdf.ThicknessMm, hdfH,
                matHdf, "", "", "", "", "tv table back hdf left");
            var pHdfR = CreatePanel(ref seq, m, "Задня стінка ХДФ внутрішня права (tv table back hdf right)", "hdf_back",
                hdfHalfW, hdfH, matHdf.ThicknessMm,
                ox + T + hdfHalfW, oy + D - hdfT, oz + T,
                hdfHalfW, matHdf.ThicknessMm, hdfH,
                matHdf, "", "", "", "", "tv table back hdf right");
            proj.Panels.Add(pHdfL);
            proj.Panels.Add(pHdfR);

            AddHardware(hw, "61281", "Мініфікс ексцентрик Hettich D15 (для плити 18 мм) + шток D5 (57722)", "компл", 14, 14.5);
            AddHardware(hw, "44817", "Конфірмат зенкер Muller 7х50 мм цинк", "шт", 6, 0.95);
            AddHardware(hw, "807.29", "Підвіс посилений Camar 807 з шиною для підвісних ТВ-тумб (до 240 кг)", "пара", 4, 290.0);
        }

        // =====================================================================
        // MODULE 7: RIGHT-SIDE WALL SHELVES (3 DESIGN VARIANTS: PANEL+36MM, L-CORNER 36MM, ASYMMETRIC 18MM)
        // =====================================================================
        private static void BuildWallShelves(FurnitureProject proj, ModuleInput m, ref int seq, Dictionary<string, HardwareLine> hw)
        {
            var mat18 = ViyarCatalog.GetMaterial(string.IsNullOrEmpty(m.BodyMaterialId) ? "kronospan_k009_18" : m.BodyMaterialId);
            var mat36 = ViyarCatalog.GetMaterial("kronospan_k009_36");
            string eFront = ViyarCatalog.GetEdgeCode(mat18.DefaultEdgeFront);
            string eBody = ViyarCatalog.GetEdgeCode(mat18.DefaultEdgeBody);
            string e36 = ViyarCatalog.GetEdgeCode(mat36.DefaultEdgeFront);

            string sub = (m.SubType ?? "var1_panel_36mm").ToLowerInvariant();
            double ox = m.OriginXMm > 0 ? m.OriginXMm : 2140.0; // Right zone of 2800mm wall (X = 2140..2780/2800mm)
            double oy = m.OriginYMm;                            // Y = 0..300mm
            double oz = m.OriginZMm > 0 ? m.OriginZMm : 400.0;  // Starts above TV stand top (Z = 400mm)

            if (sub.Contains("var1") || sub.Contains("panel"))
            {
                // CHOSEN FINAL DESIGN:
                // 1) Back wall panel split into 2 parts (broken by the TV stand Z = 150..400mm):
                //    - Bottom part under TV stand: Z = 0..150mm, X = 2140..2762mm (622 x 150 x 18mm), Y = 282..300mm
                //    - Top part above TV stand:    Z = 400..2400mm, X = 2140..2762mm (2000 x 622 x 18mm), Y = 282..300mm
                // 2) Neighboring right-wall full-height vertical panel:
                //    - Z = 0..2400mm, X = 2762..2780mm (thickness 18mm), Y = -300..300mm (width 600mm)
                //    - TV stand length is reduced by 18mm (2760 - 18 = 2742mm, X = 20..2762mm) to butt cleanly against this panel!
                // 3) 4 Monolithic L-Shaped Shelves (cut from a single sheet, NO seam in the corner!):
                //    - Each L-shelf spans X = 2140..2762mm (622mm) along the back panel (depth 220mm, Y = 62..282mm)
                //      and turns continuously along the right-wall panel (width 220mm, X = 2542..2762mm) forward to Y = -300mm (total Y span = 582mm)!
                string roleTag = "layer:Shelves_Var1_Panel_36mm";
                double T18 = 18.0;
                double T36 = 36.0;
                double backPanelX = ox;                  // 2140.0 mm
                double rightPanelInnerX = 2762.0;        // 2780 - 18 = 2762.0 mm (right end of 2742mm TV stand!)
                double backPanelW = rightPanelInnerX - backPanelX; // 622.0 mm
                double bottomPanelH = 150.0;             // Z = 0..150 mm (under TV stand)
                double topPanelZ = 400.0;                // Z = 400..2499 mm (solid continuous panel extending 243mm above 4th shelf; Z=2499..2800 is 301mm NEMAN slatted frieze!)
                double topPanelH = 2099.0;               // 2499 - 400 = 2099.0 mm
                double rightPanelFrontY = -158.8;        // Flush with front edge of L-shelves along right wall
                double rightPanelW = 300.0 - rightPanelFrontY; // 458.8 mm total width along Y (-158.8..+300)
                double rightPanelH = 2499.0;             // Z = 0..2499 mm (solid continuous panel up to 2499mm; Z=2499..2800 is 301mm NEMAN slatted frieze!)

                // 1. Bottom Back Panel under TV Stand (Z = 0..150mm)
                var pWallBackBot = CreatePanel(ref seq, m,
                    "Стінова панель задня нижня під тумбою 18мм (wall back panel bottom)", roleTag,
                    backPanelW, bottomPanelH, T18,
                    backPanelX, 300.0 - T18, 0.0,
                    backPanelW, T18, bottomPanelH,
                    mat18, eFront, eFront, eFront, "",
                    "wall back panel bottom");
                proj.Panels.Add(pWallBackBot);

                // 2. Top Back Panel above TV Stand (Z = 400..2400mm)
                var pWallBackTop = CreatePanel(ref seq, m,
                    "Стінова панель задня верхня над тумбою 18мм (wall back panel top)", roleTag,
                    topPanelH, backPanelW, T18,
                    backPanelX, 300.0 - T18, topPanelZ,
                    backPanelW, T18, topPanelH,
                    mat18, eFront, "", eFront, "",
                    "wall back panel top");

                double[] shelfZLevels = new[] { 840.0, 1300.0, 1760.0, 2220.0 };
                for (int i = 0; i < shelfZLevels.Length; i++)
                {
                    double sz = shelfZLevels[i];
                    double localZOnTopPanel = sz - topPanelZ + T36 * 0.5;
                    foreach (double xLocal in new[] { 80.0, backPanelW * 0.5, backPanelW - 80.0 })
                    {
                        pWallBackTop.Drilling.Add(new DrillHole { Side = "face_top", XMm = xLocal, YMm = localZOnTopPanel, DiameterMm = 7.0, DepthMm = T18, Purpose = "Стяжний конфірмат/шуруп Г-полиці #" + (i + 1) });
                    }
                }
                proj.Panels.Add(pWallBackTop);

                // 3. Neighboring Right-Wall Vertical Panel (X = 2762..2780mm, Y = -158.8..300mm, Z = 0..2400mm)
                var pRightWallPanel = CreatePanel(ref seq, m,
                    "Стінова панель бокова на правій стіні 18мм (right wall vertical panel)", roleTag,
                    rightPanelH, rightPanelW, T18,
                    rightPanelInnerX, rightPanelFrontY, 0.0,
                    T18, rightPanelW, rightPanelH,
                    mat18, eFront, eBody, eFront, eFront,
                    "right wall vertical panel");
                for (int i = 0; i < shelfZLevels.Length; i++)
                {
                    double zCenterOnRight = shelfZLevels[i] + T36 * 0.5; // 798, 1178, 1558, 1938 mm
                    foreach (double yLocalOnRight in new[] { 98.8, 278.8 }) // Y_world = -60.0 and 120.0 mm
                    {
                        pRightWallPanel.Drilling.Add(new DrillHole { Side = "face_top", XMm = zCenterOnRight, YMm = yLocalOnRight, DiameterMm = 7.0, DepthMm = T18, Purpose = "Стяжний конфірмат/менсолотримач правого крила Г-полиці #" + (i + 1) });
                    }
                }
                proj.Panels.Add(pRightWallPanel);

                // 4..7. 4 Monolithic Single-Piece L-Shaped Shelves (Y = -158.8..282.0 mm, spanY = 440.8mm, sideWingWidth = 220mm)
                double backWingDepth = 220.0;   // Y = 62..282 mm along back wall panel
                double sideWingWidth = 220.0;   // X = 2542..2762 mm
                double totalShelfSpanY = 440.8; // Y = -158.8..282.0 mm
                double shelfFrontY = (300.0 - T18) - totalShelfSpanY; // 282 - 440.8 = -158.8 mm

                for (int i = 0; i < shelfZLevels.Length; i++)
                {
                    double sz = shelfZLevels[i];
                    var pLShelf = CreatePanel(ref seq, m,
                        "Полиця Г-подібна суцільна #" + (i + 1) + " (L shelf solid " + (i + 1) + ")", roleTag,
                        backPanelW, totalShelfSpanY, T36,
                        backPanelX, shelfFrontY, sz,
                        backPanelW, totalShelfSpanY, T36,
                        mat36, e36, e36, e36, e36,
                        "L shelf solid " + (i + 1));
                    // Back-edge bores (edge_l2 at Y=282mm, Z_local=18mm) matching pWallBackTop
                    foreach (double xLocal in new[] { 80.0, backPanelW * 0.5, backPanelW - 80.0 })
                    {
                        pLShelf.Drilling.Add(new DrillHole { Side = "edge_l2", XMm = xLocal, YMm = T36 * 0.5, DiameterMm = 4.5, DepthMm = 34.0, Purpose = "Торцевий отвір кріплення заднього крила Г-полиці #" + (i + 1) });
                    }
                    // Right-edge bores (edge_w2 at X=2762mm, Z_local=18mm) matching pRightWallPanel
                    foreach (double yLocalOnRight in new[] { 98.8, 278.8 })
                    {
                        pLShelf.Drilling.Add(new DrillHole { Side = "edge_w2", XMm = yLocalOnRight, YMm = T36 * 0.5, DiameterMm = 4.5, DepthMm = 34.0, Purpose = "Торцевий отвір кріплення правого крила Г-полиці #" + (i + 1) });
                    }
                    // Mark as single-piece monolithic L-shaped polygon for 3D SketchUp solid generation
                    pLShelf.Notes = string.Format(CultureInfo.InvariantCulture, "L_SHAPE:{0},{1}", backWingDepth, sideWingWidth);
                    proj.Panels.Add(pLShelf);
                    AddHardware(hw, "TRIADE-M", "Менсолотримач прихований Italiana Ferramenta Triade Mini (для Г-полиць)", "шт", 4, 165.0);
                }
            }
            else if (sub.Contains("var2") || sub.Contains("corner"))
            {
                // VARIANT 2 (L-Shaped Corner Shelves turning onto the Right Wall at X=2800mm):
                // 3 tiers of L-shaped 36mm (18+18mm) shelves:
                // - Front arm along main grey wall: 660 x 220 x 36 mm (X = 2140..2800, Y = 80..300)
                // - Right return arm turning forward along right wall (X = 2800): 420 x 200 x 36 mm (X = 2600..2800, Y = -340..80)
                string roleTag = "layer:Shelves_Var2_L_Corner_36mm";
                double T36 = 36.0;
                double frontW = 660.0; // X = 2140..2800
                double frontD = 220.0; // Y = 80..300
                double sideL = 420.0;  // Y = -340..80 (turns forward along right wall!)
                double sideD = 200.0;  // X = 2600..2800
                double[] tierZ = new[] { 820.0, 1260.0, 1700.0 };

                for (int i = 0; i < tierZ.Length; i++)
                {
                    double sz = tierZ[i];
                    int tNum = i + 1;

                    // A. Main wall arm (660 x 220 x 36)
                    var pMainArm = CreatePanel(ref seq, m,
                        "Полиця Г-подібна фронтальна 36мм #" + tNum + " (var2 L shelf main " + tNum + ")", roleTag,
                        frontW, frontD, T36,
                        2800.0 - frontW, 300.0 - frontD, sz,
                        frontW, frontD, T36,
                        mat36, e36, "", e36, "",
                        "var2 L shelf main " + tNum);
                    // Minifix bolts on front face near right corner (X = 510, 610) to join the return arm
                    pMainArm.Drilling.Add(new DrillHole { Side = "edge_l1", XMm = frontW - 150.0, YMm = 18.0, DiameterMm = 8.0, DepthMm = 33.0, Purpose = "Стяжка кутового Г-з'єднання полиці" });
                    pMainArm.Drilling.Add(new DrillHole { Side = "edge_l1", XMm = frontW - 50.0, YMm = 18.0, DiameterMm = 8.0, DepthMm = 33.0, Purpose = "Стяжка кутового Г-з'єднання полиці" });
                    proj.Panels.Add(pMainArm);

                    // B. Right wall return arm (420 x 200 x 36, grain running along Y axis)
                    var pReturnArm = CreatePanel(ref seq, m,
                        "Полиця Г-подібна поворотна на праву стіну 36мм #" + tNum + " (var2 L shelf return " + tNum + ")", roleTag,
                        sideL, sideD, T36,
                        2800.0 - sideD, (300.0 - frontD) - sideL, sz,
                        sideD, sideL, T36,
                        mat36, e36, "", e36, "",
                        "var2 L shelf return " + tNum);
                    pReturnArm.Drilling.Add(new DrillHole { Side = "face_bottom", XMm = 50.0, YMm = sideL - 34.0, DiameterMm = 15.0, DepthMm = 13.8, Purpose = "Ексцентрик кутового Г-з'єднання D15" });
                    pReturnArm.Drilling.Add(new DrillHole { Side = "face_bottom", XMm = 150.0, YMm = sideL - 34.0, DiameterMm = 15.0, DepthMm = 13.8, Purpose = "Ексцентрик кутового Г-з'єднання D15" });
                    proj.Panels.Add(pReturnArm);

                    AddHardware(hw, "TRIADE-M", "Менсолотримач прихований Italiana Ferramenta Triade Mini (для плит 30-40 мм)", "шт", 4, 165.0);
                }
            }
            else
            {
                // VARIANT 3 (Asymmetric Cascading L-Shaped Shelves from standard 18mm LDSP with vertical uprights & right-wall turns):
                // Uses standard 18mm LDSP (Kronospan K009) — no 36mm splicing needed!
                string roleTag = "layer:Shelves_Var3_Asymmetric_18mm";
                double T = 18.0;

                // Tier 1 (Z = 760mm): Main shelf (660x220x18 at X=2140..2800) + Right-wall turn (360x180x18) + Left vertical L-upright (240x220x18 at X=2140)
                var pT1Main = CreatePanel(ref seq, m,
                    "Асиметрична полиця #1 фронтальна 18мм (var3 asym shelf 1 main)", roleTag,
                    660.0, 220.0, T, 2140.0, 80.0, 760.0, 660.0, 220.0, T, mat18, eFront, "", eFront, "", "var3 asym shelf 1 main");
                var pT1Turn = CreatePanel(ref seq, m,
                    "Асиметрична полиця #1 поворот на стіну 18мм (var3 asym shelf 1 turn)", roleTag,
                    360.0, 180.0, T, 2620.0, -280.0, 760.0, 180.0, 360.0, T, mat18, eFront, "", eFront, "", "var3 asym shelf 1 turn");
                var pT1Up = CreatePanel(ref seq, m,
                    "Вертикальна стійка Г-полиці #1 ліва 18мм (var3 asym upright 1)", roleTag,
                    240.0, 220.0, T, 2140.0, 80.0, 778.0, T, 220.0, 240.0, mat18, eFront, "", "", eFront, "var3 asym upright 1");
                proj.Panels.Add(pT1Main);
                proj.Panels.Add(pT1Turn);
                proj.Panels.Add(pT1Up);

                // Tier 2 (Z = 1180mm, shifted left X = 2100..2680 for asymmetry): Shelf (580x220x18) + Vertical divider linking to Tier 3 at X=2520 (382x220x18)
                var pT2Main = CreatePanel(ref seq, m,
                    "Асиметрична полиця #2 зміщена 18мм (var3 asym shelf 2)", roleTag,
                    580.0, 220.0, T, 2100.0, 80.0, 1180.0, 580.0, 220.0, T, mat18, eFront, "", eFront, eFront, "var3 asym shelf 2");
                var pT2Link = CreatePanel(ref seq, m,
                    "Вертикальна стійка-перегородка #2 18мм (var3 asym upright 2)", roleTag,
                    382.0, 220.0, T, 2520.0, 80.0, 1198.0, T, 220.0, 382.0, mat18, eFront, "", "", "", "var3 asym upright 2");
                proj.Panels.Add(pT2Main);
                proj.Panels.Add(pT2Link);

                // Tier 3 (Z = 1580mm): Main shelf (620x220x18 at X=2180..2800) + Longer Right-wall turn (460x180x18) + Top vertical bookend at X=2280 (220x220x18)
                var pT3Main = CreatePanel(ref seq, m,
                    "Асиметрична полиця #3 фронтальна 18мм (var3 asym shelf 3 main)", roleTag,
                    620.0, 220.0, T, 2180.0, 80.0, 1580.0, 620.0, 220.0, T, mat18, eFront, "", eFront, "", "var3 asym shelf 3 main");
                var pT3Turn = CreatePanel(ref seq, m,
                    "Асиметрична полиця #3 поворот на стіну 18мм (var3 asym shelf 3 turn)", roleTag,
                    460.0, 180.0, T, 2620.0, -380.0, 1580.0, 180.0, 460.0, T, mat18, eFront, "", eFront, "", "var3 asym shelf 3 turn");
                var pT3Up = CreatePanel(ref seq, m,
                    "Вертикальна стійка Г-полиці #3 18мм (var3 asym upright 3)", roleTag,
                    220.0, 220.0, T, 2280.0, 80.0, 1598.0, T, 220.0, 220.0, mat18, eFront, "", "", eFront, "var3 asym upright 3");
                proj.Panels.Add(pT3Main);
                proj.Panels.Add(pT3Turn);
                proj.Panels.Add(pT3Up);
            }
        }

        // =====================================================================
        // DRILLING & PANEL HELPERS
        // =====================================================================
        private static FurniturePanel CreatePanel(
            ref int seq, ModuleInput m, string name, string role,
            double lengthMm, double widthMm, double thicknessMm,
            double x, double y, double z, double dx, double dy, double dz,
            MaterialSpec mat, string el1, string el2, string ew1, string ew2,
            string abfName = null)
        {
            // Compute ViyarPro Saw Size (Пильний розмір) by subtracting PUR edge banding thickness
            double dedL = GetEdgeThicknessDeduction(ew1) + GetEdgeThicknessDeduction(ew2);
            double dedW = GetEdgeThicknessDeduction(el1) + GetEdgeThicknessDeduction(el2);
            double sawL = Math.Round(Math.Max(10.0, lengthMm - dedL), 1);
            double sawW = Math.Round(Math.Max(10.0, widthMm - dedW), 1);

            return new FurniturePanel
            {
                PartNumber = seq++,
                ModuleId = m.Id,
                ModuleName = m.Name,
                Name = name,
                AbfName = !string.IsNullOrEmpty(abfName) ? abfName : name,
                Role = role,
                LengthMm = Math.Round(lengthMm, 1),
                WidthMm = Math.Round(widthMm, 1),
                SawLengthMm = sawL,
                SawWidthMm = sawW,
                ThicknessMm = Math.Round(thicknessMm, 1),
                XMm = Math.Round(x, 1),
                YMm = Math.Round(y, 1),
                ZMm = Math.Round(z, 1),
                DXMm = Math.Round(dx, 1),
                DYMm = Math.Round(dy, 1),
                DZMm = Math.Round(dz, 1),
                MaterialId = mat.Id,
                MaterialName = mat.Name,
                MaterialArticle = mat.ViyarArticle,
                ColorHex = mat.ColorHex,
                HasGrain = mat.HasGrain,
                Edges = new PanelEdges { L1 = el1 ?? "", L2 = el2 ?? "", W1 = ew1 ?? "", W2 = ew2 ?? "" }
            };
        }

        private static double GetEdgeThicknessDeduction(string edgeCode)
        {
            if (string.IsNullOrEmpty(edgeCode)) return 0.0;
            if (edgeCode.StartsWith("2.0")) return 2.0;
            if (edgeCode.StartsWith("1.0")) return 1.0;
            if (edgeCode.StartsWith("0.8")) return 0.8;
            if (edgeCode.StartsWith("0.5") || edgeCode.StartsWith("0.4")) return 0.5;
            return 0.8;
        }

        private static void AddHorizontalJointFaceHoles(FurniturePanel sidePanel, double xAlongHeight, double depthMm, bool withDowels)
        {
            double frontY = depthMm - 50.0;
            double rearY = 50.0;
            sidePanel.Drilling.Add(new DrillHole { Side = "face_top", XMm = xAlongHeight, YMm = frontY, DiameterMm = 5.0, DepthMm = sidePanel.ThicknessMm, Purpose = "Конфірмат D5 наскрізний (передній)" });
            sidePanel.Drilling.Add(new DrillHole { Side = "face_top", XMm = xAlongHeight, YMm = rearY, DiameterMm = 5.0, DepthMm = sidePanel.ThicknessMm, Purpose = "Конфірмат D5 наскрізний (задній)" });
            if (withDowels)
            {
                sidePanel.Drilling.Add(new DrillHole { Side = "face_top", XMm = xAlongHeight, YMm = frontY - 32.0, DiameterMm = 8.0, DepthMm = 12.0, Purpose = "Шкант D8x12 в пласть (крок 32мм)" });
                sidePanel.Drilling.Add(new DrillHole { Side = "face_top", XMm = xAlongHeight, YMm = rearY + 32.0, DiameterMm = 8.0, DepthMm = 12.0, Purpose = "Шкант D8x12 в пласть (крок 32мм)" });
            }
        }

        private static void AddEdgeConfirmatsAndDowels(FurniturePanel horizPanel, double widthMm)
        {
            double frontY = widthMm - 50.0;
            double rearY = 50.0;
            foreach (string edgeSide in new[] { "edge_w1", "edge_w2" })
            {
                horizPanel.Drilling.Add(new DrillHole { Side = edgeSide, XMm = frontY, YMm = 9.0, DiameterMm = 5.0, DepthMm = 35.0, Purpose = "Торцевий отвір D5x35 під конфірмат" });
                horizPanel.Drilling.Add(new DrillHole { Side = edgeSide, XMm = rearY, YMm = 9.0, DiameterMm = 5.0, DepthMm = 35.0, Purpose = "Торцевий отвір D5x35 під конфірмат" });
                horizPanel.Drilling.Add(new DrillHole { Side = edgeSide, XMm = frontY - 32.0, YMm = 9.0, DiameterMm = 8.0, DepthMm = 22.0, Purpose = "Торцевий отвір D8x22 під шкант" });
                horizPanel.Drilling.Add(new DrillHole { Side = edgeSide, XMm = rearY + 32.0, YMm = 9.0, DiameterMm = 8.0, DepthMm = 22.0, Purpose = "Торцевий отвір D8x22 під шкант" });
            }
        }

        private static void AddStretcherEdgeHoles(FurniturePanel stretcher)
        {
            foreach (string edgeSide in new[] { "edge_w1", "edge_w2" })
            {
                stretcher.Drilling.Add(new DrillHole { Side = edgeSide, XMm = 25.0, YMm = 9.0, DiameterMm = 5.0, DepthMm = 35.0, Purpose = "Торцевий отвір D5x35 під конфірмат" });
                stretcher.Drilling.Add(new DrillHole { Side = edgeSide, XMm = stretcher.WidthMm - 25.0, YMm = 9.0, DiameterMm = 8.0, DepthMm = 22.0, Purpose = "Торцевий отвір D8x22 під шкант" });
            }
        }

        // =====================================================================
        // 2D SHEET NESTING & METRICS CALCULATOR
        // =====================================================================
        private static void ComputeProjectMetricsAndNesting(FurnitureProject proj)
        {
            double areaSqM = 0;
            double edgeM = 0;
            int holes = 0;

            foreach (var p in proj.Panels)
            {
                areaSqM += (p.LengthMm * p.WidthMm) / 1000000.0;
                if (!string.IsNullOrEmpty(p.Edges.L1)) edgeM += p.LengthMm / 1000.0;
                if (!string.IsNullOrEmpty(p.Edges.L2)) edgeM += p.LengthMm / 1000.0;
                if (!string.IsNullOrEmpty(p.Edges.W1)) edgeM += p.WidthMm / 1000.0;
                if (!string.IsNullOrEmpty(p.Edges.W2)) edgeM += p.WidthMm / 1000.0;
                holes += p.Drilling.Count;
            }

            proj.TotalAreaSqM = Math.Round(areaSqM, 2);
            proj.TotalEdgeLengthM = Math.Round(edgeM, 2);
            proj.TotalDrillingHoles = holes;

            // Shelf-based 2D Guillotine Nesting per material
            double kerf = 4.0;
            double trim = 15.0;
            int globalSheetIdx = 1;
            double totalSheetsCost = 0;

            foreach (var mat in proj.MaterialsUsed)
            {
                var matPanels = proj.Panels
                    .Where(p => p.MaterialId == mat.Id)
                    .OrderByDescending(p => Math.Max(p.LengthMm, p.WidthMm))
                    .ThenByDescending(p => p.LengthMm * p.WidthMm)
                    .ToList();

                double usableL = mat.SheetLengthMm - 2 * trim;
                double usableW = mat.SheetWidthMm - 2 * trim;

                SheetLayout currentSheet = null;
                double curX = trim;
                double curY = trim;
                double rowHeight = 0;

                foreach (var p in matPanels)
                {
                    double pL = p.LengthMm;
                    double pW = p.WidthMm;
                    bool rotated = false;

                    // If no grain and rotating makes height smaller for shelf packing, consider rotation
                    if (!mat.HasGrain && pW > pL && pW <= usableL)
                    {
                        double tmp = pL; pL = pW; pW = tmp;
                        rotated = true;
                    }

                    if (currentSheet == null || (curX + pL > mat.SheetLengthMm - trim && curY + rowHeight + kerf + pW > mat.SheetWidthMm - trim))
                    {
                        currentSheet = new SheetLayout
                        {
                            SheetIndex = globalSheetIdx++,
                            MaterialId = mat.Id,
                            MaterialName = mat.Name,
                            MaterialArticle = mat.ViyarArticle,
                            SheetLengthMm = mat.SheetLengthMm,
                            SheetWidthMm = mat.SheetWidthMm
                        };
                        proj.SheetLayouts.Add(currentSheet);
                        totalSheetsCost += mat.PriceUahPerSheet;
                        curX = trim;
                        curY = trim;
                        rowHeight = 0;
                    }

                    if (curX + pL > mat.SheetLengthMm - trim)
                    {
                        curX = trim;
                        curY += rowHeight + kerf;
                        rowHeight = 0;
                    }

                    currentSheet.Placements.Add(new NestedPartPlacement
                    {
                        PartNumber = p.PartNumber,
                        Name = p.Name,
                        XMm = Math.Round(curX, 1),
                        YMm = Math.Round(curY, 1),
                        PlacedLengthMm = pL,
                        PlacedWidthMm = pW,
                        Rotated = rotated
                    });

                    curX += pL + kerf;
                    if (pW > rowHeight) rowHeight = pW;
                }
            }

            foreach (var sh in proj.SheetLayouts)
            {
                double sheetArea = (double)sh.SheetLengthMm * sh.SheetWidthMm;
                double partsArea = sh.Placements.Sum(pl => pl.PlacedLengthMm * pl.PlacedWidthMm);
                sh.UtilizationPercent = Math.Round((partsArea / sheetArea) * 100.0, 1);
            }

            double hwCost = proj.HardwareBom.Sum(h => h.TotalPriceUah);
            double edgeCost = proj.TotalEdgeLengthM * 28.0;
            double servicesCost = proj.Panels.Count * 18.0 + proj.TotalDrillingHoles * 2.8;
            proj.EstimatedTotalCostUah = Math.Round(totalSheetsCost + hwCost + edgeCost + servicesCost, 0);
        }

        // =====================================================================
        // EXPORTERS: SEPARATED SKETCHUP 3D DESIGN VS. VIYAR CONVERSION
        // =====================================================================
        public static Dictionary<string, string> ExportSketchUpOnly(FurnitureProject proj, string outputDir)
        {
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            string rbPath   = Path.Combine(outputDir, "sketchup_builder.rb");
            string jsonPath = Path.Combine(outputDir, "project_data.json");

            WriteSketchUpRubyScript(proj, rbPath);
            WriteProjectJson(proj, jsonPath);
            string skpPath = ViyarSketchUpSkpExporter.WriteNativeSkpFile(proj, outputDir);

            return new Dictionary<string, string>
            {
                { "skp", skpPath },
                { "sketchup_rb", rbPath },
                { "json", jsonPath }
            };
        }

        public static Dictionary<string, string> ExportViyarOnly(FurnitureProject proj, string outputDir)
        {
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
            string dxfDir = Path.Combine(outputDir, "dxf");
            if (!Directory.Exists(dxfDir)) Directory.CreateDirectory(dxfDir);

            string xmlPath  = Path.Combine(outputDir, "viyarpro_project.xml");
            string csvPath  = Path.Combine(outputDir, "viyarpro_details.csv");
            string tsvPath  = Path.Combine(outputDir, "viyarpro_clipboard.tsv");
            string jsonPath = Path.Combine(outputDir, "project_data.json");

            WriteViyarXml(proj, xmlPath);
            WriteViyarCsvAndTsv(proj, csvPath, tsvPath);
            WriteDxfFiles(proj, dxfDir);
            WriteProjectJson(proj, jsonPath);
            string htmlPath = ViyarStudioGenerator.GenerateHtmlStudio(proj, outputDir);

            return new Dictionary<string, string>
            {
                { "xml", xmlPath },
                { "csv", csvPath },
                { "tsv", tsvPath },
                { "dxf_dir", dxfDir },
                { "json", jsonPath },
                { "html_studio", htmlPath }
            };
        }

        public static Dictionary<string, string> ExportAll(FurnitureProject proj, string outputDir)
        {
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
            string dxfDir = Path.Combine(outputDir, "dxf");
            if (!Directory.Exists(dxfDir)) Directory.CreateDirectory(dxfDir);

            string xmlPath = Path.Combine(outputDir, "viyarpro_project.xml");
            string csvPath = Path.Combine(outputDir, "viyarpro_details.csv");
            string tsvPath = Path.Combine(outputDir, "viyarpro_clipboard.tsv");
            string rbPath  = Path.Combine(outputDir, "sketchup_builder.rb");
            string jsonPath = Path.Combine(outputDir, "project_data.json");

            WriteViyarXml(proj, xmlPath);
            WriteViyarCsvAndTsv(proj, csvPath, tsvPath);
            WriteDxfFiles(proj, dxfDir);
            WriteSketchUpRubyScript(proj, rbPath);
            WriteProjectJson(proj, jsonPath);
            string htmlPath = ViyarStudioGenerator.GenerateHtmlStudio(proj, outputDir);
            string skpPath = ViyarSketchUpSkpExporter.WriteNativeSkpFile(proj, outputDir);

            return new Dictionary<string, string>
            {
                { "skp", skpPath },
                { "xml", xmlPath },
                { "csv", csvPath },
                { "tsv", tsvPath },
                { "dxf_dir", dxfDir },
                { "sketchup_rb", rbPath },
                { "json", jsonPath },
                { "html_studio", htmlPath }
            };
        }

        public static void WriteViyarXml(FurnitureProject proj, string path)
        {
            var inv = CultureInfo.InvariantCulture;
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendFormat(inv, "<project name=\"{0}\" client=\"{1}\" created=\"{2}\" generator=\"ViyarAIAgent\" version=\"2026.1\">\r\n",
                EscXml(proj.ProjectName), EscXml(proj.ClientName), EscXml(proj.CreatedAt));

            sb.AppendLine("  <materials>");
            foreach (var m in proj.MaterialsUsed)
            {
                sb.AppendFormat(inv, "    <material id=\"{0}\" viyar_article=\"{1}\" name=\"{2}\" thickness=\"{3}\" length=\"{4}\" width=\"{5}\" texture=\"{6}\" />\r\n",
                    EscXml(m.Id), EscXml(m.ViyarArticle), EscXml(m.Name), m.ThicknessMm, m.SheetLengthMm, m.SheetWidthMm, m.HasGrain ? 1 : 0);
            }
            sb.AppendLine("  </materials>");

            sb.AppendLine("  <details>");
            foreach (var p in proj.Panels)
            {
                sb.AppendFormat(inv, "    <detail id=\"{0}\" module=\"{1}\" name=\"{2}\" material_id=\"{3}\" viyar_article=\"{4}\" length=\"{5}\" width=\"{6}\" thickness=\"{7}\" quantity=\"1\" texture=\"{8}\">\r\n",
                    p.PartNumber, EscXml(p.ModuleName), EscXml(p.Name), EscXml(p.MaterialId), EscXml(p.MaterialArticle),
                    p.LengthMm, p.WidthMm, p.ThicknessMm, p.HasGrain ? 1 : 0);
                sb.AppendFormat(inv, "      <edges l1=\"{0}\" l2=\"{1}\" w1=\"{2}\" w2=\"{3}\" />\r\n",
                    EscXml(p.Edges.L1), EscXml(p.Edges.L2), EscXml(p.Edges.W1), EscXml(p.Edges.W2));

                if (p.Drilling.Count > 0 || p.Grooves.Count > 0)
                {
                    sb.AppendLine("      <operations>");
                    foreach (var d in p.Drilling)
                    {
                        sb.AppendFormat(inv, "        <drill side=\"{0}\" x=\"{1}\" y=\"{2}\" diameter=\"{3}\" depth=\"{4}\" note=\"{5}\" />\r\n",
                            EscXml(d.Side), d.XMm, d.YMm, d.DiameterMm, d.DepthMm, EscXml(d.Purpose));
                    }
                    foreach (var g in p.Grooves)
                    {
                        sb.AppendFormat(inv, "        <groove side=\"{0}\" orientation=\"{1}\" offset=\"{2}\" width=\"{3}\" depth=\"{4}\" start=\"{5}\" end=\"{6}\" note=\"{7}\" />\r\n",
                            EscXml(g.Side), EscXml(g.Orientation), g.OffsetMm, g.WidthMm, g.DepthMm, g.StartMm, g.EndMm, EscXml(g.Purpose));
                    }
                    sb.AppendLine("      </operations>");
                }
                sb.AppendLine("    </detail>");
            }
            sb.AppendLine("  </details>");

            sb.AppendLine("  <hardware>");
            foreach (var h in proj.HardwareBom)
            {
                sb.AppendFormat(inv, "    <item viyar_article=\"{0}\" name=\"{1}\" quantity=\"{2}\" unit=\"{3}\" price_uah=\"{4}\" />\r\n",
                    EscXml(h.ViyarArticle), EscXml(h.Name), h.Quantity, EscXml(h.Unit), h.UnitPriceUah);
            }
            sb.AppendLine("  </hardware>");
            sb.AppendLine("</project>");

            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
        }

        public static void WriteViyarCsvAndTsv(FurnitureProject proj, string csvPath, string tsvPath)
        {
            var inv = CultureInfo.InvariantCulture;
            var csv = new StringBuilder();
            var tsv = new StringBuilder();

            csv.AppendLine("№;Модуль;Назва деталі (ABF);Габаритна Довжина (мм);Габаритна Ширина (мм);Пильна Довжина (мм);Пильна Ширина (мм);Товщина (мм);Кількість;Текстура;Матеріал;Артикул ВіЯр;Крайка L1 (Верх);Крайка L2 (Низ);Крайка W1 (Ліво);Крайка W2 (Право);Отворів (шт);Пазів (шт)");
            tsv.AppendLine("Назва деталі (ABF)\tГабаритна Довжина\tГабаритна Ширина\tПильна Довжина\tПильна Ширина\tКількість\tТекстура\tКрайка Верх (L1)\tКрайка Низ (L2)\tКрайка Ліво (W1)\tКрайка Право (W2)\tМатеріал (Артикул ВіЯр)");

            foreach (var p in proj.Panels)
            {
                string grainStr = p.HasGrain ? "Так" : "Ні";
                string abfTitle = string.Format(inv, "__{0}. {1}", p.PartNumber, !string.IsNullOrEmpty(p.AbfName) ? p.AbfName : p.Name);
                double sawL = p.SawLengthMm > 0 ? p.SawLengthMm : p.LengthMm;
                double sawW = p.SawWidthMm > 0 ? p.SawWidthMm : p.WidthMm;

                csv.AppendFormat(inv, "{0};{1};{2};{3};{4};{5};{6};{7};1;{8};{9};{10};{11};{12};{13};{14};{15};{16}\r\n",
                    p.PartNumber, p.ModuleName, abfTitle, p.LengthMm, p.WidthMm, sawL, sawW, p.ThicknessMm, grainStr,
                    p.MaterialName, p.MaterialArticle, p.Edges.L1, p.Edges.L2, p.Edges.W1, p.Edges.W2,
                    p.Drilling.Count, p.Grooves.Count);

                tsv.AppendFormat(inv, "{0}\t{1}\t{2}\t{3}\t{4}\t1\t{5}\t{6}\t{7}\t{8}\t{9}\t{10} [{11}]\r\n",
                    abfTitle, p.LengthMm, p.WidthMm, sawL, sawW, grainStr,
                    p.Edges.L1, p.Edges.L2, p.Edges.W1, p.Edges.W2, p.MaterialName, p.MaterialArticle);
            }

            File.WriteAllText(csvPath, csv.ToString(), new UTF8Encoding(true));
            File.WriteAllText(tsvPath, tsv.ToString(), new UTF8Encoding(true));
        }

        public static void WriteDxfFiles(FurnitureProject proj, string dxfDir)
        {
            var inv = CultureInfo.InvariantCulture;
            foreach (var p in proj.Panels)
            {
                string safeName = string.Join("_", (!string.IsNullOrEmpty(p.AbfName) ? p.AbfName : p.Name).Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");
                string fileName = string.Format(inv, "__{0}_{1}_{2}x{3}x{4}.dxf", p.PartNumber, safeName, (int)p.LengthMm, (int)p.WidthMm, (int)p.ThicknessMm);
                string fullPath = Path.Combine(dxfDir, fileName);

                var dxf = new StringBuilder();
                dxf.AppendLine("0\r\nSECTION\r\n2\r\nHEADER\r\n9\r\n$INSUNITS\r\n70\r\n4\r\n0\r\nENDSEC");
                dxf.AppendLine("0\r\nSECTION\r\n2\r\nENTITIES");

                WritePanelToDxfEntities(dxf, p, 0.0, 0.0);

                dxf.AppendLine("0\r\nENDSEC\r\n0\r\nEOF");
                File.WriteAllText(fullPath, dxf.ToString(), new UTF8Encoding(false));
            }

            // Also write combined ABF "<project> flatten.dxf" in parent outputDir (matching hall_wall_table flatten.dxf!)
            string parentDir = Path.GetDirectoryName(dxfDir);
            string flattenPath = Path.Combine(parentDir, "viyar_model flatten.dxf");
            var flatDxf = new StringBuilder();
            flatDxf.AppendLine("0\r\nSECTION\r\n2\r\nHEADER\r\n9\r\n$INSUNITS\r\n70\r\n4\r\n0\r\nENDSEC");
            flatDxf.AppendLine("0\r\nSECTION\r\n2\r\nENTITIES");

            double sheetOffsetX = 0.0;
            foreach (var sh in proj.SheetLayouts)
            {
                AppendDxfRect(flatDxf, "ABF_SHEETBORDER", sheetOffsetX, 0.0, sheetOffsetX + sh.SheetLengthMm, sh.SheetWidthMm);
                foreach (var pl in sh.Placements)
                {
                    var panel = proj.Panels.FirstOrDefault(x => x.PartNumber == pl.PartNumber);
                    if (panel != null)
                    {
                        WritePanelToDxfEntities(flatDxf, panel, sheetOffsetX + pl.XMm, pl.YMm);
                    }
                }
                sheetOffsetX += sh.SheetLengthMm + 300.0;
            }

            flatDxf.AppendLine("0\r\nENDSEC\r\n0\r\nEOF");
            File.WriteAllText(flattenPath, flatDxf.ToString(), new UTF8Encoding(false));
        }

        private static void WritePanelToDxfEntities(StringBuilder dxf, FurniturePanel p, double ox, double oy)
        {
            var inv = CultureInfo.InvariantCulture;
            // 1. Panel Outer Contour on layer ABF_CUTTINGLINES
            AppendDxfRect(dxf, "ABF_CUTTINGLINES", ox, oy, ox + p.LengthMm, oy + p.WidthMm);

            // 2. Edge Banding indicator lines on layer ABF_EDGEBANDING
            if (!string.IsNullOrEmpty(p.Edges.L1)) AppendDxfLine(dxf, "ABF_EDGEBANDING", ox + 5, oy + 2, ox + p.LengthMm - 5, oy + 2);
            if (!string.IsNullOrEmpty(p.Edges.L2)) AppendDxfLine(dxf, "ABF_EDGEBANDING", ox + 5, oy + p.WidthMm - 2, ox + p.LengthMm - 5, oy + p.WidthMm - 2);
            if (!string.IsNullOrEmpty(p.Edges.W1)) AppendDxfLine(dxf, "ABF_EDGEBANDING", ox + 2, oy + 5, ox + 2, oy + p.WidthMm - 5);
            if (!string.IsNullOrEmpty(p.Edges.W2)) AppendDxfLine(dxf, "ABF_EDGEBANDING", ox + p.LengthMm - 2, oy + 5, ox + p.LengthMm - 2, oy + p.WidthMm - 5);

            // 3. Label arrow on layer ABF_LABEL
            double cx = ox + p.LengthMm * 0.5;
            double cy = oy + p.WidthMm * 0.5;
            AppendDxfLine(dxf, "ABF_LABEL", cx - 15, cy, cx + 15, cy);

            // 4. Drilling circles & side bores on exact Viyar-coded ABF CNC layers
            foreach (var d in p.Drilling)
            {
                bool isSide = d.Side != null && d.Side.StartsWith("edge_");
                if (!isSide)
                {
                    string layer = GetAbfDxfLayerForFaceHole(d);
                    dxf.AppendFormat(inv, "0\r\nCIRCLE\r\n8\r\n{0}\r\n10\r\n{1}\r\n20\r\n{2}\r\n30\r\n0.0\r\n40\r\n{3}\r\n",
                        layer, ox + d.XMm, oy + d.YMm, d.DiameterMm * 0.5);
                }
                else
                {
                    string sideLayer = Math.Abs(d.DiameterMm - 8.0) < 0.5 ? "DSIDE8" : "DSIDE4_5";
                    double depth = d.DepthMm > 0 ? d.DepthMm : 33.0;
                    if (d.Side == "edge_w1")
                        AppendDxfRect(dxf, sideLayer, ox, oy + d.XMm - d.DiameterMm * 0.5, ox + depth, oy + d.XMm + d.DiameterMm * 0.5);
                    else if (d.Side == "edge_w2")
                        AppendDxfRect(dxf, sideLayer, ox + p.LengthMm - depth, oy + d.XMm - d.DiameterMm * 0.5, ox + p.LengthMm, oy + d.XMm + d.DiameterMm * 0.5);
                    else if (d.Side == "edge_l1")
                        AppendDxfRect(dxf, sideLayer, ox + d.XMm - d.DiameterMm * 0.5, oy, ox + d.XMm + d.DiameterMm * 0.5, oy + depth);
                    else if (d.Side == "edge_l2")
                        AppendDxfRect(dxf, sideLayer, ox + d.XMm - d.DiameterMm * 0.5, oy + p.WidthMm - depth, ox + d.XMm + d.DiameterMm * 0.5, oy + p.WidthMm);
                }
            }
        }

        private static string GetAbfDxfLayerForFaceHole(DrillHole d)
        {
            if (Math.Abs(d.DiameterMm - 35.0) < 0.5) return "DEPTH12-ARTICLE97743";
            if (Math.Abs(d.DiameterMm - 2.0) < 0.5) return "DEPTH3_0-ARTICLE46331";
            if (Math.Abs(d.DiameterMm - 15.0) < 0.5) return "D15-DEPTH13_8-$61281";
            if (Math.Abs(d.DiameterMm - 7.0) < 0.5) return "D7-DEPTHF-CTRSINK-$44817";
            string purpose = (d.Purpose ?? "").ToLowerInvariant();
            if (purpose.Contains("напрямн") || purpose.Contains("slide")) return "D5-DEPTH8_5-$52636";
            return "D5-DEPTH11_5-$57722";
        }

        private static void AppendDxfRect(StringBuilder dxf, string layer, double x1, double y1, double x2, double y2)
        {
            var inv = CultureInfo.InvariantCulture;
            AppendDxfLine(dxf, layer, x1, y1, x2, y1);
            AppendDxfLine(dxf, layer, x2, y1, x2, y2);
            AppendDxfLine(dxf, layer, x2, y2, x1, y2);
            AppendDxfLine(dxf, layer, x1, y2, x1, y1);
        }

        private static void AppendDxfLine(StringBuilder dxf, string layer, double x1, double y1, double x2, double y2)
        {
            var inv = CultureInfo.InvariantCulture;
            dxf.AppendFormat(inv, "0\r\nLINE\r\n8\r\n{0}\r\n10\r\n{1}\r\n20\r\n{2}\r\n30\r\n0.0\r\n11\r\n{3}\r\n21\r\n{4}\r\n31\r\n0.0\r\n",
                layer, x1, y1, x2, y2);
        }

        public static void WriteProjectJson(FurnitureProject proj, string jsonPath)
        {
            var inv = CultureInfo.InvariantCulture;
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendFormat(inv, "  \"project_name\": \"{0}\",\r\n", EscJson(proj.ProjectName));
            sb.AppendFormat(inv, "  \"client_name\": \"{0}\",\r\n", EscJson(proj.ClientName));
            sb.AppendFormat(inv, "  \"created_at\": \"{0}\",\r\n", EscJson(proj.CreatedAt));
            sb.AppendFormat(inv, "  \"total_area_sqm\": {0},\r\n", proj.TotalAreaSqM);
            sb.AppendFormat(inv, "  \"total_edge_m\": {0},\r\n", proj.TotalEdgeLengthM);
            sb.AppendFormat(inv, "  \"total_drilling_holes\": {0},\r\n", proj.TotalDrillingHoles);
            sb.AppendFormat(inv, "  \"estimated_cost_uah\": {0},\r\n", proj.EstimatedTotalCostUah);

            sb.AppendLine("  \"materials_used\": [");
            for (int i = 0; i < proj.MaterialsUsed.Count; i++)
            {
                var m = proj.MaterialsUsed[i];
                sb.AppendFormat(inv, "    {{ \"id\": \"{0}\", \"viyar_article\": \"{1}\", \"name\": \"{2}\", \"thickness_mm\": {3}, \"sheet_length_mm\": {4}, \"sheet_width_mm\": {5}, \"has_grain\": {6}, \"color_hex\": \"{7}\", \"price_uah_per_sheet\": {8} }}{9}\r\n",
                    EscJson(m.Id), EscJson(m.ViyarArticle), EscJson(m.Name), m.ThicknessMm, m.SheetLengthMm, m.SheetWidthMm,
                    m.HasGrain ? "true" : "false", EscJson(m.ColorHex), m.PriceUahPerSheet, (i < proj.MaterialsUsed.Count - 1) ? "," : "");
            }
            sb.AppendLine("  ],");

            sb.AppendLine("  \"modules\": [");
            for (int i = 0; i < proj.Modules.Count; i++)
            {
                var mod = proj.Modules[i];
                sb.AppendFormat(inv, "    {{ \"id\": \"{0}\", \"name\": \"{1}\", \"type\": \"{2}\", \"width_mm\": {3}, \"height_mm\": {4}, \"depth_mm\": {5}, \"origin_x_mm\": {6}, \"origin_y_mm\": {7}, \"origin_z_mm\": {8} }}{9}\r\n",
                    EscJson(mod.Id), EscJson(mod.Name), EscJson(mod.Type), mod.WidthMm, mod.HeightMm, mod.DepthMm, mod.OriginXMm, mod.OriginYMm, mod.OriginZMm,
                    (i < proj.Modules.Count - 1) ? "," : "");
            }
            sb.AppendLine("  ],");

            sb.AppendLine("  \"panels\": [");
            for (int i = 0; i < proj.Panels.Count; i++)
            {
                var p = proj.Panels[i];
                sb.AppendLine("    {");
                sb.AppendFormat(inv, "      \"part_number\": {0}, \"module_id\": \"{1}\", \"module_name\": \"{2}\", \"name\": \"{3}\", \"role\": \"{4}\",\r\n",
                    p.PartNumber, EscJson(p.ModuleId), EscJson(p.ModuleName), EscJson(p.Name), EscJson(p.Role));
                sb.AppendFormat(inv, "      \"length_mm\": {0}, \"width_mm\": {1}, \"thickness_mm\": {2},\r\n", p.LengthMm, p.WidthMm, p.ThicknessMm);
                sb.AppendFormat(inv, "      \"x_mm\": {0}, \"y_mm\": {1}, \"z_mm\": {2}, \"dx_mm\": {3}, \"dy_mm\": {4}, \"dz_mm\": {5},\r\n",
                    p.XMm, p.YMm, p.ZMm, p.DXMm, p.DYMm, p.DZMm);
                sb.AppendFormat(inv, "      \"material_id\": \"{0}\", \"material_name\": \"{1}\", \"material_article\": \"{2}\", \"color_hex\": \"{3}\", \"has_grain\": {4},\r\n",
                    EscJson(p.MaterialId), EscJson(p.MaterialName), EscJson(p.MaterialArticle), EscJson(p.ColorHex), p.HasGrain ? "true" : "false");
                sb.AppendFormat(inv, "      \"edges\": {{ \"l1\": \"{0}\", \"l2\": \"{1}\", \"w1\": \"{2}\", \"w2\": \"{3}\" }},\r\n",
                    EscJson(p.Edges.L1), EscJson(p.Edges.L2), EscJson(p.Edges.W1), EscJson(p.Edges.W2));

                sb.AppendLine("      \"drilling\": [");
                for (int dIdx = 0; dIdx < p.Drilling.Count; dIdx++)
                {
                    var d = p.Drilling[dIdx];
                    sb.AppendFormat(inv, "        {{ \"side\": \"{0}\", \"x_mm\": {1}, \"y_mm\": {2}, \"diameter_mm\": {3}, \"depth_mm\": {4}, \"purpose\": \"{5}\" }}{6}\r\n",
                        EscJson(d.Side), d.XMm, d.YMm, d.DiameterMm, d.DepthMm, EscJson(d.Purpose), (dIdx < p.Drilling.Count - 1) ? "," : "");
                }
                sb.AppendLine("      ],");

                sb.AppendLine("      \"grooves\": [");
                for (int gIdx = 0; gIdx < p.Grooves.Count; gIdx++)
                {
                    var g = p.Grooves[gIdx];
                    sb.AppendFormat(inv, "        {{ \"side\": \"{0}\", \"orientation\": \"{1}\", \"offset_mm\": {2}, \"width_mm\": {3}, \"depth_mm\": {4}, \"start_mm\": {5}, \"end_mm\": {6}, \"purpose\": \"{7}\" }}{8}\r\n",
                        EscJson(g.Side), EscJson(g.Orientation), g.OffsetMm, g.WidthMm, g.DepthMm, g.StartMm, g.EndMm, EscJson(g.Purpose), (gIdx < p.Grooves.Count - 1) ? "," : "");
                }
                sb.AppendLine("      ]");
                sb.AppendFormat(inv, "    }}{0}\r\n", (i < proj.Panels.Count - 1) ? "," : "");
            }
            sb.AppendLine("  ],");

            sb.AppendLine("  \"hardware_bom\": [");
            for (int i = 0; i < proj.HardwareBom.Count; i++)
            {
                var h = proj.HardwareBom[i];
                sb.AppendFormat(inv, "    {{ \"viyar_article\": \"{0}\", \"name\": \"{1}\", \"quantity\": {2}, \"unit\": \"{3}\", \"unit_price_uah\": {4}, \"total_price_uah\": {5} }}{6}\r\n",
                    EscJson(h.ViyarArticle), EscJson(h.Name), h.Quantity, EscJson(h.Unit), h.UnitPriceUah, h.TotalPriceUah, (i < proj.HardwareBom.Count - 1) ? "," : "");
            }
            sb.AppendLine("  ],");

            sb.AppendLine("  \"sheet_layouts\": [");
            for (int i = 0; i < proj.SheetLayouts.Count; i++)
            {
                var sh = proj.SheetLayouts[i];
                sb.AppendLine("    {");
                sb.AppendFormat(inv, "      \"sheet_index\": {0}, \"material_id\": \"{1}\", \"material_name\": \"{2}\", \"material_article\": \"{3}\", \"sheet_length_mm\": {4}, \"sheet_width_mm\": {5}, \"utilization_percent\": {6},\r\n",
                    sh.SheetIndex, EscJson(sh.MaterialId), EscJson(sh.MaterialName), EscJson(sh.MaterialArticle), sh.SheetLengthMm, sh.SheetWidthMm, sh.UtilizationPercent);
                sb.AppendLine("      \"placements\": [");
                for (int plIdx = 0; plIdx < sh.Placements.Count; plIdx++)
                {
                    var pl = sh.Placements[plIdx];
                    sb.AppendFormat(inv, "        {{ \"part_number\": {0}, \"name\": \"{1}\", \"x_mm\": {2}, \"y_mm\": {3}, \"length_mm\": {4}, \"width_mm\": {5}, \"rotated\": {6} }}{7}\r\n",
                        pl.PartNumber, EscJson(pl.Name), pl.XMm, pl.YMm, pl.PlacedLengthMm, pl.PlacedWidthMm, pl.Rotated ? "true" : "false", (plIdx < sh.Placements.Count - 1) ? "," : "");
                }
                sb.AppendLine("      ]");
                sb.AppendFormat(inv, "    }}{0}\r\n", (i < proj.SheetLayouts.Count - 1) ? "," : "");
            }
            sb.AppendLine("  ]");
            sb.AppendLine("}");

            File.WriteAllText(jsonPath, sb.ToString(), new UTF8Encoding(false));
        }

        public static void WriteSketchUpRubyScript(FurnitureProject proj, string rbPath)
        {
            string jsonPath = Path.Combine(Path.GetDirectoryName(rbPath), "project_data.json").Replace("\\", "/");
            var sb = new StringBuilder();
            sb.AppendLine("# encoding: UTF-8");
            sb.AppendLine("# Автоматично згенерований скрипт побудови моделі для SketchUp (Viyar AI Agent)");
            sb.AppendLine("# Сумісний із плагінами ABF, Gava, OpenCutList та Dynamic Components");
            sb.AppendLine("require 'sketchup.rb'");
            sb.AppendLine("require 'json'");
            sb.AppendLine();
            sb.AppendFormat("json_file = '{0}'\r\n", jsonPath);
            sb.AppendLine("if defined?(ViyarAIBridge) && File.exist?(json_file)");
            sb.AppendLine("  data = JSON.parse(File.read(json_file, encoding: 'UTF-8'))");
            sb.AppendLine("  ViyarAIBridge.build_project_from_data(data)");
            sb.AppendLine("else");
            sb.AppendLine("  UI.messagebox('Плагін ViyarAIBridge не завантажено або файл project_data.json не знайдено.')");
            sb.AppendLine("end");
            File.WriteAllText(rbPath, sb.ToString(), new UTF8Encoding(false));
        }

        public static string SyncToSketchUpLive(string jsonPath, string liveQueuePath)
        {
            string projectJson = File.ReadAllText(jsonPath, Encoding.UTF8);

            // 1. Write to live_queue file so SketchUp's timer picks it up automatically
            if (!string.IsNullOrEmpty(liveQueuePath))
            {
                string dir = Path.GetDirectoryName(liveQueuePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(liveQueuePath, projectJson, new UTF8Encoding(false));
            }

            // 2. Try direct TCP socket connection to SketchUp 127.0.0.1:9876
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect("127.0.0.1", 9876, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(600));
                    if (success && client.Connected)
                    {
                        client.EndConnect(result);
                        using (var stream = client.GetStream())
                        using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true })
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            string oneLine = projectJson.Replace("\r", " ").Replace("\n", " ");
                            string rpc = "{\"command\":\"clear_and_build\",\"params\":" + oneLine + "}";
                            writer.WriteLine(rpc);
                            string resp = reader.ReadLine();
                            return "LIVE_TCP_SYNC_OK: " + resp;
                        }
                    }
                }
            }
            catch
            {
                // SketchUp is not currently running with TCP listener; live queue file is ready
            }
            return "QUEUED_FOR_SKETCHUP: Проект збережено у чергу живого оновлення SketchUp (" + liveQueuePath + ")";
        }

        private static string EscXml(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        private static string EscJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n");
        }
    }
}

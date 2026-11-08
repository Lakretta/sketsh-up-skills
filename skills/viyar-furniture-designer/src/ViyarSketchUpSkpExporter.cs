using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ViyarAgent
{
    public static class ViyarSketchUpSkpExporter
    {
        private const string SuApiDll = @"D:\Program Files\SketchUp\SketchUpAPI.dll";
        private const double MmToInch = 1.0 / 25.4;

        [StructLayout(LayoutKind.Sequential)]
        public struct SUPoint3D
        {
            public double x, y, z;
            public SUPoint3D(double x, double y, double z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SUVector3D
        {
            public double x, y, z;
            public SUVector3D(double x, double y, double z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SUColor
        {
            public byte red, green, blue, alpha;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SUTransformation
        {
            public double m00, m10, m20, m30;
            public double m01, m11, m21, m31;
            public double m02, m12, m22, m32;
            public double tx, ty, tz, m33;

            public static SUTransformation Translation(double x, double y, double z)
            {
                return new SUTransformation
                {
                    m00 = 1.0, m11 = 1.0, m22 = 1.0, m33 = 1.0,
                    tx = x, ty = y, tz = z
                };
            }
        }

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SUInitialize();

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SUTerminate();

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUModelCreate(out IntPtr model);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUModelRelease(ref IntPtr model);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUModelGetEntities(IntPtr model, out IntPtr entities);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUModelGetCamera(IntPtr model, out IntPtr camera);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUCameraSetOrientation(IntPtr camera, ref SUPoint3D position, ref SUPoint3D target, ref SUVector3D upVector);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUCameraSetPerspective(IntPtr camera, bool perspective);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SULayerCreate(out IntPtr layer);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SULayerSetName(IntPtr layer, byte[] nameUtf8);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUModelAddLayers(IntPtr model, UIntPtr count, IntPtr[] layers);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUDrawingElementSetLayer(IntPtr elem, IntPtr layer);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUGroupCreate(out IntPtr group);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUGroupSetName(IntPtr group, byte[] nameUtf8);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUGroupSetTransform(IntPtr group, ref SUTransformation transform);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUGroupGetEntities(IntPtr group, out IntPtr entities);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUEntitiesAddGroup(IntPtr entities, IntPtr group);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUGeometryInputCreate(out IntPtr geomInput);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUGeometryInputRelease(ref IntPtr geomInput);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUGeometryInputAddVertex(IntPtr geomInput, ref SUPoint3D point);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SULoopInputCreate(out IntPtr loopInput);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SULoopInputAddVertexIndex(IntPtr loopInput, UIntPtr vertexIndex);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUGeometryInputAddFace(IntPtr geomInput, ref IntPtr outerLoop, out UIntPtr addedFaceIndex);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUEntitiesFill(IntPtr entities, IntPtr geomInput, bool weldVertices);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUEntitiesGetNumFaces(IntPtr entities, out UIntPtr count);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUEntitiesGetFaces(IntPtr entities, UIntPtr len, IntPtr[] faces, out UIntPtr count);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUFaceGetNormal(IntPtr face, out SUVector3D normal);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUEdgeCreate(out IntPtr edge, ref SUPoint3D start, ref SUPoint3D end);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUEntitiesAddEdges(IntPtr entities, UIntPtr len, IntPtr[] edges);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUMaterialCreate(out IntPtr mat);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUMaterialSetName(IntPtr mat, byte[] nameUtf8);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUMaterialSetColor(IntPtr mat, ref SUColor color);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUModelAddMaterials(IntPtr model, UIntPtr count, IntPtr[] mats);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUDrawingElementSetMaterial(IntPtr elem, IntPtr mat);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUEntityGetAttributeDictionary(IntPtr entity, byte[] dictName, out IntPtr dict);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUTypedValueCreate(out IntPtr val);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUTypedValueRelease(ref IntPtr val);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUTypedValueSetString(IntPtr val, byte[] strUtf8);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUTypedValueSetDouble(IntPtr val, double dval);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUTypedValueSetInt32(IntPtr val, int ival);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUTypedValueSetBool(IntPtr val, bool bval);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUAttributeDictionarySetValue(IntPtr dict, byte[] keyUtf8, IntPtr valIn);

        [DllImport(SuApiDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SUModelSaveToFile(IntPtr model, byte[] pathUtf8);

        // Standard ABF + Viyar CNC layer names discovered across D:\Project\SketshUp\design
        public const string LayerFacade = "Facade";
        public const string LayerInnerFacade = "Внутрішній фасад";
        public const string LayerAbfCuttingLines = "ABF_cuttingLines";
        public const string LayerAbfEdgeBanding = "ABF_edgeBanding";
        public const string LayerAbfLabel = "ABF_Label";
        public const string LayerAbfSheetBorder = "ABF_sheetBorder";
        public const string LayerMinifixCam18 = "D15-DEPTH13.8-$61281";
        public const string LayerMinifixCam16 = "D15-DEPTH13-$59042";
        public const string LayerMinifixBolt5 = "D5-DEPTH11.5-$57722";
        public const string LayerSideDrill8 = "Dside8";
        public const string LayerConfirmatThrough = "D7-DEPTHF-CTRSINK-$44817";
        public const string LayerConfirmatPlain = "D7-DEPTHF-$44817";
        public const string LayerSideDrill45 = "Dside4.5";
        public const string LayerHingeCup35 = "DEPTH12-ARTICLE97743";
        public const string LayerHingeCup35D13 = "DEPTH13-ARTICLE96055";
        public const string LayerHingePilot2 = "DEPTH3.0-ARTICLE46331";
        public const string LayerSlideScrew5 = "D5-DEPTH8.5-$52636";
        public const string LayerTipOnPilot2 = "D2-DEPTH3-$11361";

        public static string WriteNativeSkpFile(FurnitureProject proj, string outputDir)
        {
            if (!File.Exists(SuApiDll)) return "";

            string skpPath = Path.Combine(outputDir, "viyar_model.skp");
            if (File.Exists(skpPath))
            {
                try { File.Delete(skpPath); } catch { }
            }

            SUInitialize();
            IntPtr model = IntPtr.Zero;
            try
            {
                if (SUModelCreate(out model) != 0 || model == IntPtr.Zero) return "";

                IntPtr rootEntities = IntPtr.Zero;
                SUModelGetEntities(model, out rootEntities);

                // 1. Create exact ABF + Viyar CNC layers in the model (matching hall_wall_table.skp & badroom_wardrobe_actual_fittings.skp)
                var layerMap = new Dictionary<string, IntPtr>(StringComparer.OrdinalIgnoreCase);
                string[] standardLayers = new[]
                {
                    LayerFacade,
                    LayerInnerFacade,
                    LayerAbfCuttingLines,
                    LayerAbfEdgeBanding,
                    LayerAbfLabel,
                    LayerAbfSheetBorder,
                    LayerMinifixCam18,
                    LayerMinifixCam16,
                    LayerMinifixBolt5,
                    LayerSideDrill8,
                    LayerConfirmatThrough,
                    LayerConfirmatPlain,
                    LayerSideDrill45,
                    LayerHingeCup35,
                    LayerHingeCup35D13,
                    LayerHingePilot2,
                    LayerSlideScrew5,
                    LayerTipOnPilot2,
                    "Shelves_Var1_Panel_36mm",
                    "Shelves_Var2_L_Corner_36mm",
                    "Shelves_Var3_Asymmetric_18mm"
                };
                foreach (string lyrName in standardLayers)
                {
                    layerMap[lyrName] = CreateLayer(model, lyrName);
                }

                // 2. Create SketchUp Materials using exact Viyar/ABF naming convention: <brand>_<code_name>_SU_$<ViyarArticle>
                var matPtrMap = new Dictionary<string, IntPtr>(StringComparer.OrdinalIgnoreCase);
                foreach (var m in proj.MaterialsUsed)
                {
                    IntPtr matPtr = IntPtr.Zero;
                    if (SUMaterialCreate(out matPtr) == 0 && matPtr != IntPtr.Zero)
                    {
                        string suMatName = !string.IsNullOrEmpty(m.SketchUpMaterialName)
                            ? m.SketchUpMaterialName
                            : BuildAbfMaterialName(m);
                        SUMaterialSetName(matPtr, Utf8Z(suMatName));
                        SUColor col = ParseHexColor(m.ColorHex);
                        SUMaterialSetColor(matPtr, ref col);
                        SUModelAddMaterials(model, new UIntPtr(1), new[] { matPtr });

                        // OpenCutList & ViyarPro metadata on Material
                        SetStringAttr(matPtr, "ladb_opencutlist", "thickness", m.ThicknessMm.ToString("0.#", CultureInfo.InvariantCulture) + "mm");
                        SetStringAttr(matPtr, "ladb_opencutlist", "std_dimension", m.SheetLengthMm + "mm x " + m.SheetWidthMm + "mm");
                        SetBoolAttr(matPtr, "ladb_opencutlist", "grained", m.HasGrain);
                        SetStringAttr(matPtr, "ViyarPro", "viyar_article", m.ViyarArticle);
                        SetDoubleAttr(matPtr, "ViyarPro", "thickness_mm", m.ThicknessMm);

                        matPtrMap[m.Id] = matPtr;
                    }
                }

                // Create helper materials used by ABF (ABF_EdgeBand, ABF_Label)
                CreateHelperMaterial(model, "ABF_EdgeBand", "#D23228");
                CreateHelperMaterial(model, "ABF_Label", "#6E2323");

                // 3. Add every FurniturePanel as a TOP-LEVEL SUGroup in rootEntities (exact ABF architecture!)
                double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
                double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;
                int pairSeed = 1001;

                foreach (var p in proj.Panels)
                {
                    if (p.XMm < minX) minX = p.XMm;
                    if (p.YMm < minY) minY = p.YMm;
                    if (p.ZMm < minZ) minZ = p.ZMm;
                    if (p.XMm + p.DXMm > maxX) maxX = p.XMm + p.DXMm;
                    if (p.YMm + p.DYMm > maxY) maxY = p.YMm + p.DYMm;
                    if (p.ZMm + p.DZMm > maxZ) maxZ = p.ZMm + p.DZMm;

                    AddAbfBoardGroupToSkp(rootEntities, p, matPtrMap, layerMap, ref pairSeed);
                }

                // 3B. Optional Room Context (Wall 2800mm, 65" TV at Z_center=1370mm, Robot Vacuum under 150mm clearance)
                foreach (var mod in proj.Modules)
                {
                    string mType = (mod.Type ?? "").ToLowerInvariant();
                    string mSub = (mod.SubType ?? "").ToLowerInvariant();
                    if (mType == "tv_unit" && (mSub.Contains("flap") || mSub.Contains("floating")))
                    {
                        AddFloatingTvRoomContextToSkp(model, rootEntities, mod);
                        if (0.0 < minX) minX = 0.0;
                        if (2800.0 > maxX) maxX = 2800.0;
                        if (0.0 < minZ) minZ = 0.0;
                        if (1900.0 > maxZ) maxZ = 1900.0;
                        break;
                    }
                }

                // 4. Configure SketchUp 3D Camera (Front-Left-Top 3/4 Isometric Perspective with +Z UP)
                if (minX < maxX)
                {
                    IntPtr camera;
                    if (SUModelGetCamera(model, out camera) == 0 && camera != IntPtr.Zero)
                    {
                        double cx = ((minX + maxX) * 0.5) * MmToInch;
                        double cy = ((minY + maxY) * 0.5) * MmToInch;
                        double cz = ((minZ + maxZ) * 0.5) * MmToInch;
                        double spanX = Math.Max(600.0, maxX - minX) * MmToInch;
                        double spanZ = Math.Max(400.0, maxZ - minZ) * MmToInch;

                        var target = new SUPoint3D(cx, cy, cz);
                        var pos = new SUPoint3D(cx - spanX * 0.55, (minY * MmToInch) - spanX * 1.35, cz + spanZ * 0.85);
                        var up = new SUVector3D(0.0, 0.0, 1.0); // Strictly +Z Up

                        SUCameraSetPerspective(camera, true);
                        SUCameraSetOrientation(camera, ref pos, ref target, ref up);
                    }
                }

                SUModelSaveToFile(model, Utf8Z(skpPath));
                return skpPath;
            }
            finally
            {
                if (model != IntPtr.Zero) SUModelRelease(ref model);
                SUTerminate();
            }
        }

        private static string BuildAbfMaterialName(MaterialSpec m)
        {
            string brand = (m.Brand ?? "egger").ToLowerInvariant().Replace(" ", "_");
            string idPart = (m.Id ?? "mat").ToLowerInvariant();
            string art = string.IsNullOrEmpty(m.ViyarArticle) ? "81190" : m.ViyarArticle;
            return string.Format(CultureInfo.InvariantCulture, "{0}_{1}_SU_${2}", brand, idPart, art);
        }

        private static IntPtr CreateHelperMaterial(IntPtr model, string name, string hex)
        {
            IntPtr matPtr = IntPtr.Zero;
            if (SUMaterialCreate(out matPtr) == 0 && matPtr != IntPtr.Zero)
            {
                SUMaterialSetName(matPtr, Utf8Z(name));
                SUColor col = ParseHexColor(hex);
                SUMaterialSetColor(matPtr, ref col);
                SUModelAddMaterials(model, new UIntPtr(1), new[] { matPtr });
                return matPtr;
            }
            return IntPtr.Zero;
        }

        private static IntPtr CreateLayer(IntPtr model, string name)
        {
            IntPtr layer = IntPtr.Zero;
            if (SULayerCreate(out layer) == 0 && layer != IntPtr.Zero)
            {
                SULayerSetName(layer, Utf8Z(name));
                SUModelAddLayers(model, new UIntPtr(1), new[] { layer });
                return layer;
            }
            return IntPtr.Zero;
        }

        private static void AddFloatingTvRoomContextToSkp(IntPtr model, IntPtr rootEntities, ModuleInput mod)
        {
            IntPtr lyrRoom = CreateLayer(model, "Context_Wall_TV_Robot");
            IntPtr matWall = CreateHelperMaterial(model, "Context_Wall_Grey", "#7D8085");
            IntPtr matTvScreen = CreateHelperMaterial(model, "Context_TV_OLED_65", "#191B1F");
            IntPtr matRobot = CreateHelperMaterial(model, "Context_Robot_Vacuum", "#2C2E33");

            double wallW = 2800.0;
            double wallH = 2800.0;
            double wallY = mod.OriginYMm + mod.DepthMm; // Flush at Y=300.0mm directly behind the perimeter frame, inset HDF, and 18mm wall panels!

            // 1. Reference Main Grey Wall 2800 x 2800 mm
            CreateContextSolidBox(rootEntities, "Context_Wall_Grey_2800x2800",
                0.0, wallY, 0.0, wallW, 60.0, wallH, lyrRoom, matWall);

            // 1B. Reference Right Perpendicular Grey Wall (at X = 2780mm right behind the 18mm right-wall wood panel X=2762..2780)
            CreateContextSolidBox(rootEntities, "Context_Right_Corner_Wall_Grey",
                2780.0, -500.0, 0.0, 60.0, wallY + 560.0, wallH, lyrRoom, matWall);

            // 2. Reference TV with 1300 mm diagonal (16:9 -> 1133 x 637 x 35 mm) centered on 2800mm wall (X_center=1400, Z_center=1370 -> Z=1051.5..1688.5)
            double tvW = 1133.0;
            double tvH = 637.0;
            double tvD = 35.0;
            double tvX = (wallW - tvW) * 0.5; // 833.5 mm
            double tvZ = 1370.0 - tvH * 0.5;  // 1051.5 mm
            double tvY = wallY - tvD - 20.0;  // 20mm wall mount gap (leaves 3mm clearance in front of 17mm NEMAN slatted panel!)
            CreateContextSolidBox(rootEntities, "Context_TV_Diag1300mm_1133x637_CenterZ1370",
                tvX, tvY, tvZ, tvW, tvD, tvH, lyrRoom, matTvScreen);

            // 3. Reference Robot Vacuum (D=350mm x H=103mm with LiDAR turret) under the 150mm bottom clearance (10mm clearance from wall at Y=300)
            double robX = mod.OriginXMm + 180.0;
            double robY = wallY - 360.0; // Y = -60..290 mm (10mm in front of wall Y=300)
            CreateContextSolidBox(rootEntities, "Context_Robot_Vacuum_Body_350x350x85",
                robX, robY, 0.0, 350.0, 350.0, 85.0, lyrRoom, matRobot);
            CreateContextSolidBox(rootEntities, "Context_Robot_Vacuum_LiDAR_H103",
                robX + 127.0, robY + 127.0, 85.0, 96.0, 96.0, 18.0, lyrRoom, matRobot);

            // 4. Decorative Slatted Wall Panels NEMAN "Ялина карпатська" (301 x 2780 x 17 mm: 5mm black backing + 6 x 34x12mm vertical wood slats per 301mm module)
            AddNemanDecorativeSlattedPanels(model, rootEntities, wallY);
        }

        private static void AddNemanDecorativeSlattedPanels(IntPtr model, IntPtr rootEntities, double wallY)
        {
            IntPtr lyrNeman = CreateLayer(model, "Decorative_Panels_Neman_301x2780x17");
            IntPtr matBlackBase = CreateHelperMaterial(model, "Neman_Black_Backing_5mm", "#18191C");
            IntPtr matSlatWood = CreateHelperMaterial(model, "Neman_Yalyna_Karpatska_Slat_12mm", "#845E3E");

            double baseT = 5.0;       // 5mm black MDF/felt backing
            double slatT = 12.0;      // 12mm wood slat (5 + 12 = 17.0 mm total thickness!)
            double slatW = 34.0;      // 34mm wood slat width
            double pitch = 301.0 / 6.0; // 50.1667mm modular pitch (6 slats per 301mm panel)

            // ZONE A (Left of shelving unit): 1 panel width 301mm (X = 1839.0..2140.0mm), height 2400mm (Z = 400.0..2800.0mm), thickness 17mm (Y = 283.0..300.0mm)
            double leftX = 2140.0 - 301.0; // 1839.0 mm
            double leftZ = 400.0;
            double leftH = 2400.0;
            CreateSlattedPanelAssemblyBackWall(rootEntities, "Deco_Neman_Left_301x2400x17",
                leftX, wallY, leftZ, 301.0, leftH, 6, baseT, slatT, slatW, pitch, lyrNeman, matBlackBase, matSlatWood);

            // ZONE B (Top frieze above solid back panel Z=400..2499mm): width 622mm (X = 2140.0..2762.0mm), height 301mm (Z = 2499.0..2800.0mm), thickness 17mm (Y = 283.0..300.0mm)
            double topZ = 2499.0;
            double topH = 2800.0 - topZ; // 301.0 mm
            CreateSlattedPanelAssemblyBackWall(rootEntities, "Deco_Neman_Top_Back_622x301x17",
                2140.0, wallY, topZ, 622.0, topH, 12, baseT, slatT, slatW, pitch, lyrNeman, matBlackBase, matSlatWood);

            // ZONE C (Top frieze above solid right-wall panel Z=0..2499mm): width 441.8mm (Y = -158.8..283.0mm), height 301mm (Z = 2499.0..2800.0mm), thickness 17mm (X = 2763.0..2780.0mm)
            CreateSlattedPanelAssemblyRightWall(rootEntities, "Deco_Neman_Top_RightWall_442x301x17",
                2780.0, -158.8, topZ, 441.8, topH, 9, baseT, slatT, slatW, pitch, lyrNeman, matBlackBase, matSlatWood);
        }

        private static void CreateSlattedPanelAssemblyBackWall(
            IntPtr rootEntities, string name,
            double x0Mm, double wallYMm, double z0Mm,
            double widthMm, double heightMm, int slatsCount,
            double baseTMm, double slatTMm, double slatWMm, double pitchMm,
            IntPtr layer, IntPtr matBase, IntPtr matSlat)
        {
            IntPtr grp = IntPtr.Zero;
            if (SUGroupCreate(out grp) != 0 || grp == IntPtr.Zero) return;
            SUEntitiesAddGroup(rootEntities, grp);
            SUGroupSetName(grp, Utf8Z(name));
            if (layer != IntPtr.Zero) SUDrawingElementSetLayer(grp, layer);

            double totalT = baseTMm + slatTMm; // 17.0 mm
            var tr = SUTransformation.Translation(x0Mm * MmToInch, (wallYMm - totalT) * MmToInch, z0Mm * MmToInch);
            SUGroupSetTransform(grp, ref tr);

            IntPtr ents;
            SUGroupGetEntities(grp, out ents);

            // 1. Black backing plate (Y_local = slatTMm .. slatTMm + baseTMm = 12..17 mm)
            CreateContextSolidBox(ents, "Neman_Backing_5mm",
                0.0, slatTMm, 0.0, widthMm, baseTMm, heightMm, layer, matBase);

            // 2. Vertical 3D wooden slats (Y_local = 0 .. slatTMm = 0..12 mm)
            for (int i = 0; i < slatsCount; i++)
            {
                double sx = 4.0 + i * pitchMm;
                if (sx + slatWMm > widthMm) sx = widthMm - slatWMm - 2.0;
                CreateContextSolidBox(ents, "Neman_Slat_" + (i + 1),
                    sx, 0.0, 0.0, slatWMm, slatTMm, heightMm, layer, matSlat);
            }
        }

        private static void CreateSlattedPanelAssemblyRightWall(
            IntPtr rootEntities, string name,
            double rightWallXMm, double y0Mm, double z0Mm,
            double spanYMm, double heightMm, int slatsCount,
            double baseTMm, double slatTMm, double slatWMm, double pitchMm,
            IntPtr layer, IntPtr matBase, IntPtr matSlat)
        {
            IntPtr grp = IntPtr.Zero;
            if (SUGroupCreate(out grp) != 0 || grp == IntPtr.Zero) return;
            SUEntitiesAddGroup(rootEntities, grp);
            SUGroupSetName(grp, Utf8Z(name));
            if (layer != IntPtr.Zero) SUDrawingElementSetLayer(grp, layer);

            double totalT = baseTMm + slatTMm; // 17.0 mm (X = 2763..2780 mm)
            var tr = SUTransformation.Translation((rightWallXMm - totalT) * MmToInch, y0Mm * MmToInch, z0Mm * MmToInch);
            SUGroupSetTransform(grp, ref tr);

            IntPtr ents;
            SUGroupGetEntities(grp, out ents);

            // 1. Black backing plate against right wall (X_local = slatTMm .. totalT = 12..17 mm)
            CreateContextSolidBox(ents, "Neman_Backing_5mm",
                slatTMm, 0.0, 0.0, baseTMm, spanYMm, heightMm, layer, matBase);

            // 2. Vertical 3D wooden slats facing left (-X, X_local = 0 .. 12 mm)
            for (int i = 0; i < slatsCount; i++)
            {
                double sy = 4.0 + i * pitchMm;
                if (sy + slatWMm > spanYMm) sy = spanYMm - slatWMm - 2.0;
                CreateContextSolidBox(ents, "Neman_Slat_" + (i + 1),
                    0.0, sy, 0.0, slatTMm, slatWMm, heightMm, layer, matSlat);
            }
        }

        private static void CreateContextSolidBox(
            IntPtr rootEntities, string name,
            double xMm, double yMm, double zMm,
            double dxMm, double dyMm, double dzMm,
            IntPtr layer, IntPtr mat)
        {
            IntPtr grp = IntPtr.Zero;
            if (SUGroupCreate(out grp) != 0 || grp == IntPtr.Zero) return;
            SUEntitiesAddGroup(rootEntities, grp);
            SUGroupSetName(grp, Utf8Z(name));

            var tr = SUTransformation.Translation(xMm * MmToInch, yMm * MmToInch, zMm * MmToInch);
            SUGroupSetTransform(grp, ref tr);
            if (layer != IntPtr.Zero) SUDrawingElementSetLayer(grp, layer);
            if (mat != IntPtr.Zero) SUDrawingElementSetMaterial(grp, mat);

            double dx = Math.Max(1.0, dxMm) * MmToInch;
            double dy = Math.Max(1.0, dyMm) * MmToInch;
            double dz = Math.Max(1.0, dzMm) * MmToInch;

            var vertices = new[]
            {
                new SUPoint3D(0,  0,  0),
                new SUPoint3D(dx, 0,  0),
                new SUPoint3D(dx, dy, 0),
                new SUPoint3D(0,  dy, 0),
                new SUPoint3D(0,  0,  dz),
                new SUPoint3D(dx, 0,  dz),
                new SUPoint3D(dx, dy, dz),
                new SUPoint3D(0,  dy, dz)
            };
            int[][] faceIndices = new[]
            {
                new[] { 0, 3, 2, 1 },
                new[] { 4, 5, 6, 7 },
                new[] { 0, 1, 5, 4 },
                new[] { 2, 3, 7, 6 },
                new[] { 3, 0, 4, 7 },
                new[] { 1, 2, 6, 5 }
            };

            IntPtr grpEnts;
            SUGroupGetEntities(grp, out grpEnts);
            IntPtr geomInput;
            if (SUGeometryInputCreate(out geomInput) == 0 && geomInput != IntPtr.Zero)
            {
                for (int i = 0; i < 8; i++)
                {
                    var pt = vertices[i];
                    SUGeometryInputAddVertex(geomInput, ref pt);
                }
                foreach (var loopVerts in faceIndices)
                {
                    IntPtr loopInput;
                    if (SULoopInputCreate(out loopInput) == 0 && loopInput != IntPtr.Zero)
                    {
                        foreach (int vIdx in loopVerts)
                        {
                            SULoopInputAddVertexIndex(loopInput, new UIntPtr((uint)vIdx));
                        }
                        UIntPtr addedFaceIdx;
                        SUGeometryInputAddFace(geomInput, ref loopInput, out addedFaceIdx);
                    }
                }
                SUEntitiesFill(grpEnts, geomInput, true);
                SUGeometryInputRelease(ref geomInput);
            }
        }

        private static void AddAbfBoardGroupToSkp(
            IntPtr rootEntities,
            FurniturePanel p,
            Dictionary<string, IntPtr> matPtrMap,
            Dictionary<string, IntPtr> layerMap,
            ref int pairSeed)
        {
            IntPtr partGrp = IntPtr.Zero;
            SUGroupCreate(out partGrp);
            SUEntitiesAddGroup(rootEntities, partGrp);

            // Exact ABF board naming convention: "__<index>. <english part name>"
            string shortEnName = !string.IsNullOrEmpty(p.AbfName) ? p.AbfName : p.Name;
            string grpName = string.Format(CultureInfo.InvariantCulture, "__{0}. {1}", p.PartNumber, shortEnName);
            SUGroupSetName(partGrp, Utf8Z(grpName));

            // Set group transform to (XMm, YMm, ZMm) so local coordinates are (0..DX, 0..DY, 0..DZ)
            var tr = SUTransformation.Translation(p.XMm * MmToInch, p.YMm * MmToInch, p.ZMm * MmToInch);
            SUGroupSetTransform(partGrp, ref tr);

            // Assign Layer: Facade panels on 'Facade' layer, custom 'layer:<name>' on its named layer, body & drawer panels on default 'Layer0'
            string rawRole = p.Role ?? "body";
            string role = rawRole.ToLowerInvariant();
            if (rawRole.StartsWith("layer:", StringComparison.OrdinalIgnoreCase))
            {
                string customLayerName = rawRole.Substring(6).Trim();
                if (layerMap.ContainsKey(customLayerName) && layerMap[customLayerName] != IntPtr.Zero)
                {
                    SUDrawingElementSetLayer(partGrp, layerMap[customLayerName]);
                }
            }
            else if (role.Contains("facade") || role.Contains("door"))
            {
                if (layerMap.ContainsKey(LayerFacade) && layerMap[LayerFacade] != IntPtr.Zero)
                {
                    SUDrawingElementSetLayer(partGrp, layerMap[LayerFacade]);
                }
            }

            // Assign Viyar Material directly to the Board SUGroup (leaving faces inside with default material, just like hall_wall_table.skp!)
            if (!string.IsNullOrEmpty(p.MaterialId) && matPtrMap.ContainsKey(p.MaterialId))
            {
                IntPtr matPtr = matPtrMap[p.MaterialId];
                if (matPtr != IntPtr.Zero)
                {
                    SUDrawingElementSetMaterial(partGrp, matPtr);
                }
            }

            // Exact Dict[ABF] attributes on the Board SUGroup
            if (role != "slide_guide")
            {
                SetIntAttr(partGrp, "ABF", "board-index", p.PartNumber);
                SetBoolAttr(partGrp, "ABF", "is-board", true);
                SetIntAttr(partGrp, "ABF", "label-rotation", 0);
            }

            // Also keep ViyarPro attributes for direct metadata inspection
            SetBoolAttr(partGrp, "ViyarPro", "is_viyar_entity", true);
            SetStringAttr(partGrp, "ViyarPro", "name", p.Name);
            SetStringAttr(partGrp, "ViyarPro", "abf_name", grpName);
            SetStringAttr(partGrp, "ViyarPro", "material_name", p.MaterialName);
            SetStringAttr(partGrp, "ViyarPro", "material_article", p.MaterialArticle);
            SetDoubleAttr(partGrp, "ViyarPro", "length_mm", p.LengthMm);
            SetDoubleAttr(partGrp, "ViyarPro", "width_mm", p.WidthMm);
            SetDoubleAttr(partGrp, "ViyarPro", "saw_length_mm", p.SawLengthMm > 0 ? p.SawLengthMm : p.LengthMm);
            SetDoubleAttr(partGrp, "ViyarPro", "saw_width_mm", p.SawWidthMm > 0 ? p.SawWidthMm : p.WidthMm);
            SetDoubleAttr(partGrp, "ViyarPro", "thickness_mm", p.ThicknessMm);
            SetStringAttr(partGrp, "ViyarPro", "edge_l1", p.Edges.L1);
            SetStringAttr(partGrp, "ViyarPro", "edge_l2", p.Edges.L2);
            SetStringAttr(partGrp, "ViyarPro", "edge_w1", p.Edges.W1);
            SetStringAttr(partGrp, "ViyarPro", "edge_w2", p.Edges.W2);

            // Build 6-face manifold solid box OR 8-face monolithic L-shaped solid in local coordinates (0..dx, 0..dy, 0..dz)
            double dx = Math.Max(1.0, p.DXMm) * MmToInch;
            double dy = Math.Max(1.0, p.DYMm) * MmToInch;
            double dz = Math.Max(1.0, p.DZMm) * MmToInch;

            SUPoint3D[] vertices;
            int[][] faceIndices;

            if (!string.IsNullOrEmpty(p.Notes) && p.Notes.StartsWith("L_SHAPE:", StringComparison.OrdinalIgnoreCase))
            {
                // Parse L_SHAPE:<backWingDepthMm>,<sideWingWidthMm>
                double backDepthMm = 220.0;
                double sideWidthMm = 200.0;
                string[] parts = p.Notes.Substring(8).Split(',');
                if (parts.Length >= 2)
                {
                    double.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out backDepthMm);
                    double.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out sideWidthMm);
                }

                double xCut = Math.Max(10.0, p.DXMm - sideWidthMm) * MmToInch;
                double yCut = Math.Max(10.0, p.DYMm - backDepthMm) * MmToInch;

                // 12 vertices of the monolithic L-shaped shelf (0..5 bottom L-polygon, 6..11 top L-polygon)
                vertices = new[]
                {
                    new SUPoint3D(0,    yCut, 0),  // 0: front-left of back arm
                    new SUPoint3D(xCut, yCut, 0),  // 1: inner concave L-corner
                    new SUPoint3D(xCut, 0,    0),  // 2: front-left of right return arm
                    new SUPoint3D(dx,   0,    0),  // 3: front-right of right return arm
                    new SUPoint3D(dx,   dy,   0),  // 4: back-right outer corner
                    new SUPoint3D(0,    dy,   0),  // 5: back-left corner
                    new SUPoint3D(0,    yCut, dz), // 6: top front-left of back arm
                    new SUPoint3D(xCut, yCut, dz), // 7: top inner concave L-corner
                    new SUPoint3D(xCut, 0,    dz), // 8: top front-left of right return arm
                    new SUPoint3D(dx,   0,    dz), // 9: top front-right of right return arm
                    new SUPoint3D(dx,   dy,   dz), // 10: top back-right outer corner
                    new SUPoint3D(0,    dy,   dz)  // 11: top back-left corner
                };

                // 8 water-tight faces (1 bottom L-face, 1 top L-face, 6 vertical edge faces — ZERO internal split lines!)
                faceIndices = new[]
                {
                    new[] { 0, 5, 4, 3, 2, 1 }, // Bottom L-face (-Z)
                    new[] { 6, 7, 8, 9, 10, 11 }, // Top L-face (+Z)
                    new[] { 0, 1, 7, 6 },       // Front edge of back arm (-Y)
                    new[] { 1, 2, 8, 7 },       // Inner edge of return arm (-X)
                    new[] { 2, 3, 9, 8 },       // Front end of return arm (-Y)
                    new[] { 3, 4, 10, 9 },      // Right wall edge (+X)
                    new[] { 4, 5, 11, 10 },     // Back wall edge (+Y)
                    new[] { 5, 0, 6, 11 }       // Left end of back arm (-X)
                };
            }
            else
            {
                vertices = new[]
                {
                    new SUPoint3D(0,  0,  0),  // 0: bottom-front-left
                    new SUPoint3D(dx, 0,  0),  // 1: bottom-front-right
                    new SUPoint3D(dx, dy, 0),  // 2: bottom-back-right
                    new SUPoint3D(0,  dy, 0),  // 3: bottom-back-left
                    new SUPoint3D(0,  0,  dz), // 4: top-front-left
                    new SUPoint3D(dx, 0,  dz), // 5: top-front-right
                    new SUPoint3D(dx, dy, dz), // 6: top-back-right
                    new SUPoint3D(0,  dy, dz)  // 7: top-back-left
                };

                // Outward-facing vertex loops for all 6 faces of the box
                faceIndices = new[]
                {
                    new[] { 0, 3, 2, 1 }, // Bottom (-Z)
                    new[] { 4, 5, 6, 7 }, // Top (+Z)
                    new[] { 0, 1, 5, 4 }, // Front (-Y)
                    new[] { 2, 3, 7, 6 }, // Back (+Y)
                    new[] { 3, 0, 4, 7 }, // Left (-X)
                    new[] { 1, 2, 6, 5 }  // Right (+X)
                };
            }

            IntPtr grpEnts = IntPtr.Zero;
            SUGroupGetEntities(partGrp, out grpEnts);

            IntPtr geomInput = IntPtr.Zero;
            if (SUGeometryInputCreate(out geomInput) == 0 && geomInput != IntPtr.Zero)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    var pt = vertices[i];
                    SUGeometryInputAddVertex(geomInput, ref pt);
                }

                foreach (var loopVerts in faceIndices)
                {
                    IntPtr loopInput = IntPtr.Zero;
                    if (SULoopInputCreate(out loopInput) == 0 && loopInput != IntPtr.Zero)
                    {
                        foreach (int vIdx in loopVerts)
                        {
                            SULoopInputAddVertexIndex(loopInput, new UIntPtr((uint)vIdx));
                        }
                        UIntPtr addedFaceIdx;
                        SUGeometryInputAddFace(geomInput, ref loopInput, out addedFaceIdx);
                    }
                }

                SUEntitiesFill(grpEnts, geomInput, true);
                SUGeometryInputRelease(ref geomInput);
            }

            // Tag faces with Dict[ABF]: is-cnced-face, is-labeled-face, and edge-band-id=0
            if (role != "slide_guide")
            {
                ApplyAbfFaceAttributes(grpEnts, p);
                AddAbfLabelSubGroup(grpEnts, p, layerMap);
                AddAbfFastenerSubGroups(grpEnts, p, layerMap, ref pairSeed);
            }
        }

        private static void ApplyAbfFaceAttributes(IntPtr grpEnts, FurniturePanel p)
        {
            UIntPtr numFaces;
            if (SUEntitiesGetNumFaces(grpEnts, out numFaces) != 0 || (uint)numFaces == 0) return;
            var faces = new IntPtr[(int)numFaces];
            UIntPtr actualFaces;
            if (SUEntitiesGetFaces(grpEnts, numFaces, faces, out actualFaces) != 0) return;

            // Determine orientation of the board
            // Case 0: Horizontal board (DZMm == ThicknessMm) -> large faces are +Z / -Z
            // Case 1: Vertical X-thin board (DXMm == ThicknessMm) -> large faces are +X / -X
            // Case 2: Vertical Y-thin board (DYMm == ThicknessMm) -> large faces are +Y / -Y
            int orient = 0;
            if (Math.Abs(p.DZMm - p.ThicknessMm) < 1.0) orient = 0;
            else if (Math.Abs(p.DXMm - p.ThicknessMm) < 1.0) orient = 1;
            else orient = 2;

            bool hasL1 = !string.IsNullOrEmpty(p.Edges.L1);
            bool hasL2 = !string.IsNullOrEmpty(p.Edges.L2);
            bool hasW1 = !string.IsNullOrEmpty(p.Edges.W1);
            bool hasW2 = !string.IsNullOrEmpty(p.Edges.W2);

            for (int i = 0; i < (int)actualFaces; i++)
            {
                IntPtr face = faces[i];
                SUVector3D n;
                if (SUFaceGetNormal(face, out n) != 0) continue;

                if (orient == 0)
                {
                    // Horizontal board: Top (+Z) or Bottom (-Z) is labeled/CNC face
                    bool isTopPrimary = (p.Role != "worktop");
                    if ((isTopPrimary && n.z > 0.8) || (!isTopPrimary && n.z < -0.8))
                    {
                        SetBoolAttr(face, "ABF", "is-cnced-face", true);
                        SetBoolAttr(face, "ABF", "is-labeled-face", true);
                    }
                    else if (n.y < -0.8 && hasL1) SetIntAttr(face, "ABF", "edge-band-id", 0);
                    else if (n.y > 0.8 && hasL2) SetIntAttr(face, "ABF", "edge-band-id", 0);
                    else if (n.x < -0.8 && hasW1) SetIntAttr(face, "ABF", "edge-band-id", 0);
                    else if (n.x > 0.8 && hasW2) SetIntAttr(face, "ABF", "edge-band-id", 0);
                }
                else if (orient == 1)
                {
                    // Vertical Side/Divider (DXMm == ThicknessMm): inner face (+X for left side, -X for right side) is CNC/labeled face
                    bool preferPlusX = (p.XMm < 900.0);
                    if ((preferPlusX && n.x > 0.8) || (!preferPlusX && n.x < -0.8))
                    {
                        SetBoolAttr(face, "ABF", "is-cnced-face", true);
                        SetBoolAttr(face, "ABF", "is-labeled-face", true);
                    }
                    else if (n.y < -0.8 && hasL1) SetIntAttr(face, "ABF", "edge-band-id", 0);
                    else if (n.y > 0.8 && hasL2) SetIntAttr(face, "ABF", "edge-band-id", 0);
                    else if (n.z < -0.8 && hasW1) SetIntAttr(face, "ABF", "edge-band-id", 0);
                    else if (n.z > 0.8 && hasW2) SetIntAttr(face, "ABF", "edge-band-id", 0);
                }
                else
                {
                    // Vertical Frontal/Back Panel (DYMm == ThicknessMm): inner face (+Y) is CNC/labeled face
                    if (n.y > 0.8)
                    {
                        SetBoolAttr(face, "ABF", "is-cnced-face", true);
                        SetBoolAttr(face, "ABF", "is-labeled-face", true);
                    }
                    else if (n.z > 0.8 && hasL1) SetIntAttr(face, "ABF", "edge-band-id", 0);
                    else if (n.z < -0.8 && hasL2) SetIntAttr(face, "ABF", "edge-band-id", 0);
                    else if (n.x < -0.8 && hasW1) SetIntAttr(face, "ABF", "edge-band-id", 0);
                    else if (n.x > 0.8 && hasW2) SetIntAttr(face, "ABF", "edge-band-id", 0);
                }
            }
        }

        private static void AddAbfLabelSubGroup(IntPtr panelEntities, FurniturePanel p, Dictionary<string, IntPtr> layerMap)
        {
            IntPtr lblGrp = IntPtr.Zero;
            if (SUGroupCreate(out lblGrp) != 0 || lblGrp == IntPtr.Zero) return;
            SUEntitiesAddGroup(panelEntities, lblGrp);
            SUGroupSetName(lblGrp, Utf8Z("_ABF_Label"));

            IntPtr lyrLabel = layerMap.ContainsKey(LayerAbfLabel) ? layerMap[LayerAbfLabel] : IntPtr.Zero;
            if (lyrLabel != IntPtr.Zero) SUDrawingElementSetLayer(lblGrp, lyrLabel);

            // Position label at the center of the board's labeled face
            double lx = p.DXMm * 0.5;
            double ly = p.DYMm * 0.5;
            double lz = p.DZMm * 0.5;
            int plane = 0; // 0 = XY, 1 = YZ, 2 = XZ
            if (Math.Abs(p.DZMm - p.ThicknessMm) < 1.0)
            {
                plane = 0;
                lz = (p.Role == "worktop") ? 0.0 : p.DZMm;
                if (!string.IsNullOrEmpty(p.Notes) && p.Notes.StartsWith("L_SHAPE:", StringComparison.OrdinalIgnoreCase))
                {
                    ly = Math.Max(20.0, p.DYMm - 110.0);
                }
            }
            else if (Math.Abs(p.DXMm - p.ThicknessMm) < 1.0)
            {
                plane = 1;
                lx = (p.XMm < 900.0) ? p.DXMm : 0.0;
            }
            else
            {
                plane = 2;
                ly = p.DYMm;
            }

            var tr = SUTransformation.Translation(lx * MmToInch, ly * MmToInch, lz * MmToInch);
            SUGroupSetTransform(lblGrp, ref tr);

            SetBoolAttr(lblGrp, "ABF", "is-label", true);
            SetIntAttr(lblGrp, "ABF", "label-height", 2);
            SetDoubleAttr(lblGrp, "ABF", "label-scale", 1.575);
            SetDoubleAttr(lblGrp, "ABF", "label-width", 22.0);

            // Add grain direction arrow + label box edges on layer ABF_Label
            IntPtr lblEnts;
            SUGroupGetEntities(lblGrp, out lblEnts);
            double w = 16.0 * MmToInch;
            double h = 3.0 * MmToInch;
            var edges = new List<IntPtr>();
            if (plane == 0)
            {
                AddEdge3D(edges, lyrLabel, -w, 0, 0, w, 0, 0);
                AddEdge3D(edges, lyrLabel, w - h, h, 0, w, 0, 0);
                AddEdge3D(edges, lyrLabel, w - h, -h, 0, w, 0, 0);
            }
            else if (plane == 1)
            {
                AddEdge3D(edges, lyrLabel, 0, -w, 0, 0, w, 0);
                AddEdge3D(edges, lyrLabel, 0, w - h, h, 0, w, 0);
                AddEdge3D(edges, lyrLabel, 0, w - h, -h, 0, w, 0);
            }
            else
            {
                AddEdge3D(edges, lyrLabel, -w, 0, 0, w, 0, 0);
                AddEdge3D(edges, lyrLabel, w - h, 0, h, w, 0, 0);
                AddEdge3D(edges, lyrLabel, w - h, 0, -h, w, 0, 0);
            }
            if (edges.Count > 0)
            {
                SUEntitiesAddEdges(lblEnts, new UIntPtr((uint)edges.Count), edges.ToArray());
            }
        }

        private static void AddAbfFastenerSubGroups(
            IntPtr panelEntities,
            FurniturePanel p,
            Dictionary<string, IntPtr> layerMap,
            ref int pairSeed)
        {
            if (p.Drilling == null || p.Drilling.Count == 0) return;

            int orient = 0; // 0 = Horizontal (DZ=T), 1 = Vertical Side (DX=T), 2 = Vertical Frontal (DY=T)
            if (Math.Abs(p.DZMm - p.ThicknessMm) < 1.0) orient = 0;
            else if (Math.Abs(p.DXMm - p.ThicknessMm) < 1.0) orient = 1;
            else orient = 2;

            foreach (var d in p.Drilling)
            {
                int pid = pairSeed++;
                string purpose = (d.Purpose ?? "").ToLowerInvariant();
                bool isSide = d.Side != null && d.Side.StartsWith("edge_");

                // Compute 3D local coordinates (cx, cy, cz) in mm on the panel and circle plane (0=XY, 1=YZ, 2=XZ)
                double cxMm = 0, cyMm = 0, czMm = 0;
                int circlePlane = 0;
                MapDrillToLocal3D(p, d, orient, out cxMm, out cyMm, out czMm, out circlePlane);

                if (!isSide && Math.Abs(d.DiameterMm - 35.0) < 0.5)
                {
                    // Hinge Cup (_ABF_hingeCup): 35mm circle on DEPTH12-ARTICLE97743 + two 2mm pilot holes on DEPTH3.0-ARTICLE46331
                    IntPtr grp = CreateSubGroupAt(panelEntities, "_ABF_hingeCup", cxMm, cyMm, czMm);
                    SetIntAttr(grp, "ABF", "hinge-group-a-id", pid);
                    SetBoolAttr(grp, "ABF", "is-hinge-part-b", true);
                    SetStringAttr(grp, "ABF", "setting-name", "завіса, внутрішня, 18 мм");

                    IntPtr ents;
                    SUGroupGetEntities(grp, out ents);
                    var edges = new List<IntPtr>();
                    AddCircleEdges24(edges, GetLayer(layerMap, LayerHingeCup35), 0, 0, 0, 17.5 * MmToInch, circlePlane);
                    // Two pilot holes at +/-22.5mm along door edge, +9.5mm deeper from edge
                    if (circlePlane == 2)
                    {
                        AddCircleEdges24(edges, GetLayer(layerMap, LayerHingePilot2), -22.5 * MmToInch, 0, 9.5 * MmToInch, 1.0 * MmToInch, circlePlane);
                        AddCircleEdges24(edges, GetLayer(layerMap, LayerHingePilot2), 22.5 * MmToInch, 0, 9.5 * MmToInch, 1.0 * MmToInch, circlePlane);
                    }
                    else
                    {
                        AddCircleEdges24(edges, GetLayer(layerMap, LayerHingePilot2), -22.5 * MmToInch, 9.5 * MmToInch, 0, 1.0 * MmToInch, circlePlane);
                        AddCircleEdges24(edges, GetLayer(layerMap, LayerHingePilot2), 22.5 * MmToInch, 9.5 * MmToInch, 0, 1.0 * MmToInch, circlePlane);
                    }
                    if (edges.Count > 0) SUEntitiesAddEdges(ents, new UIntPtr((uint)edges.Count), edges.ToArray());
                }
                else if (!isSide && Math.Abs(d.DiameterMm - 2.0) < 0.5)
                {
                    if (purpose.Contains("tip-on"))
                    {
                        // Tip-On Push-to-Open Adapter Plate (_ABF_tipOnAdapter): two 2mm circles spaced 32mm along Y on D2-DEPTH3-$11361
                        IntPtr grp = CreateSubGroupAt(panelEntities, "_ABF_tipOnAdapter", cxMm, cyMm, czMm);
                        SetIntAttr(grp, "ABF", "hinge-group-b-id", pid);
                        SetBoolAttr(grp, "ABF", "is-hinge-part-a", true);
                        SetStringAttr(grp, "ABF", "setting-name", "Tip-On довгий з магнітом Blum");

                        IntPtr ents;
                        SUGroupGetEntities(grp, out ents);
                        var edges = new List<IntPtr>();
                        AddCircleEdges24(edges, GetLayer(layerMap, LayerTipOnPilot2), 0, 0, 0, 1.0 * MmToInch, circlePlane);
                        AddCircleEdges24(edges, GetLayer(layerMap, LayerTipOnPilot2), 0, 32.0 * MmToInch, 0, 1.0 * MmToInch, circlePlane);
                        if (edges.Count > 0) SUEntitiesAddEdges(ents, new UIntPtr((uint)edges.Count), edges.ToArray());
                    }
                    else if (purpose.Contains("амортизатор") || purpose.Contains("газліфт"))
                    {
                        // Soft-Down Flap Stay Bracket (_ABF_flapStayBracket): two 2mm pilot holes spaced 32mm on D2-DEPTH3-$11361
                        IntPtr grp = CreateSubGroupAt(panelEntities, "_ABF_flapStayBracket", cxMm, cyMm, czMm);
                        SetIntAttr(grp, "ABF", "hinge-group-b-id", pid);
                        SetBoolAttr(grp, "ABF", "is-hinge-part-a", true);
                        SetStringAttr(grp, "ABF", "setting-name", "Газовий амортизатор зворотньої дії N=60");

                        IntPtr ents;
                        SUGroupGetEntities(grp, out ents);
                        var edges = new List<IntPtr>();
                        AddCircleEdges24(edges, GetLayer(layerMap, LayerTipOnPilot2), 0, 0, -16.0 * MmToInch, 1.0 * MmToInch, circlePlane);
                        AddCircleEdges24(edges, GetLayer(layerMap, LayerTipOnPilot2), 0, 0, 16.0 * MmToInch, 1.0 * MmToInch, circlePlane);
                        if (edges.Count > 0) SUEntitiesAddEdges(ents, new UIntPtr((uint)edges.Count), edges.ToArray());
                    }
                    else
                    {
                        // Hinge Mounting Plate (_ABF_hingeMountingPlate): two 2mm circles spaced 32mm apart on DEPTH3.0-ARTICLE46331
                        IntPtr grp = CreateSubGroupAt(panelEntities, "_ABF_hingeMountingPlate", cxMm, cyMm, czMm);
                        SetIntAttr(grp, "ABF", "hinge-group-b-id", pid);
                        SetBoolAttr(grp, "ABF", "is-hinge-part-a", true);
                        SetStringAttr(grp, "ABF", "setting-name", "завіса, внутрішня, 18 мм");

                        IntPtr ents;
                        SUGroupGetEntities(grp, out ents);
                        var edges = new List<IntPtr>();
                        if (circlePlane == 1)
                        {
                            AddCircleEdges24(edges, GetLayer(layerMap, LayerHingePilot2), 0, 0, -16.0 * MmToInch, 1.0 * MmToInch, circlePlane);
                            AddCircleEdges24(edges, GetLayer(layerMap, LayerHingePilot2), 0, 0, 16.0 * MmToInch, 1.0 * MmToInch, circlePlane);
                        }
                        else
                        {
                            AddCircleEdges24(edges, GetLayer(layerMap, LayerHingePilot2), -16.0 * MmToInch, 0, 0, 1.0 * MmToInch, circlePlane);
                            AddCircleEdges24(edges, GetLayer(layerMap, LayerHingePilot2), 16.0 * MmToInch, 0, 0, 1.0 * MmToInch, circlePlane);
                        }
                        if (edges.Count > 0) SUEntitiesAddEdges(ents, new UIntPtr((uint)edges.Count), edges.ToArray());
                    }
                }
                else if (!isSide && Math.Abs(d.DiameterMm - 15.0) < 0.5)
                {
                    // Minifix Cam Housing (_ABF_minifixBolts): 15mm circle on D15-DEPTH13.8-$61281 (18mm) or D15-DEPTH13-$59042 (16mm)
                    bool is16Mm = (p.ThicknessMm <= 16.5 || Math.Abs(d.DepthMm - 13.0) < 0.3);
                    string mfxSetting = is16Mm ? "Мініфікс - Hettich (16 мм)" : "Мініфікс - Hettich (18 мм)";
                    string mfxLayer   = is16Mm ? LayerMinifixCam16 : LayerMinifixCam18;

                    IntPtr grp = CreateSubGroupAt(panelEntities, "_ABF_minifixBolts", cxMm, cyMm, czMm);
                    SetBoolAttr(grp, "ABF", "is-minifix-part-b", true);
                    SetIntAttr(grp, "ABF", "minifix-group-a-id", pid);
                    SetStringAttr(grp, "ABF", "setting-name", mfxSetting);

                    IntPtr ents;
                    SUGroupGetEntities(grp, out ents);
                    var edges = new List<IntPtr>();
                    AddCircleEdges24(edges, GetLayer(layerMap, mfxLayer), 0, 0, 0, 7.5 * MmToInch, circlePlane);
                    if (edges.Count > 0) SUEntitiesAddEdges(ents, new UIntPtr((uint)edges.Count), edges.ToArray());
                }
                else if (!isSide && Math.Abs(d.DiameterMm - 7.0) < 0.5)
                {
                    // Confirmat Countersunk Through-Hole (_ABF_screwInside): 7mm circle on D7-DEPTHF-CTRSINK-$44817
                    IntPtr grp = CreateSubGroupAt(panelEntities, "_ABF_screwInside", cxMm, cyMm, czMm);
                    SetBoolAttr(grp, "ABF", "is-minifix-part-a", true);
                    SetBoolAttr(grp, "ABF", "is-screw", true);
                    SetIntAttr(grp, "ABF", "minifix-group-b-id", pid);
                    SetStringAttr(grp, "ABF", "setting-name", "Конфірмат зенкер Muller");

                    IntPtr ents;
                    SUGroupGetEntities(grp, out ents);
                    var edges = new List<IntPtr>();
                    AddCircleEdges24(edges, GetLayer(layerMap, LayerConfirmatThrough), 0, 0, 0, 3.5 * MmToInch, circlePlane);
                    if (edges.Count > 0) SUEntitiesAddEdges(ents, new UIntPtr((uint)edges.Count), edges.ToArray());
                }
                else if (!isSide && Math.Abs(d.DiameterMm - 5.0) < 0.5)
                {
                    bool isWallHanger = purpose.Contains("підвіс") || purpose.Contains("camar");
                    string subGrpName = isWallHanger ? "_ABF_wallHangerCamar807" : "_ABF_minifixPlasticBase";
                    IntPtr grp = CreateSubGroupAt(panelEntities, subGrpName, cxMm, cyMm, czMm);
                    SetBoolAttr(grp, "ABF", "is-minifix-part-a", true);
                    SetIntAttr(grp, "ABF", "minifix-group-b-id", pid);

                    string settingName = (p.ThicknessMm <= 16.5) ? "Мініфікс - Hettich (16 мм)" : "Мініфікс - Hettich (18 мм)";
                    string lyrName = LayerMinifixBolt5;
                    if (isWallHanger)
                    {
                        settingName = "Підвіс посилений Camar 807";
                    }
                    else if (purpose.Contains("полицетримач") || purpose.Contains("shelf"))
                    {
                        settingName = "Полицетримач оцинкований 5x16";
                    }
                    else if (purpose.Contains("напрямн") || purpose.Contains("slide"))
                    {
                        settingName = "L400 (33) 52505 - Ящ";
                        lyrName = LayerSlideScrew5;
                    }
                    SetStringAttr(grp, "ABF", "setting-name", settingName);

                    IntPtr ents;
                    SUGroupGetEntities(grp, out ents);
                    var edges = new List<IntPtr>();
                    AddCircleEdges24(edges, GetLayer(layerMap, lyrName), 0, 0, 0, 2.5 * MmToInch, circlePlane);
                    if (edges.Count > 0) SUEntitiesAddEdges(ents, new UIntPtr((uint)edges.Count), edges.ToArray());
                }
                else if (isSide && Math.Abs(d.DiameterMm - 8.0) < 0.5)
                {
                    // Minifix Side Bore (_ABF_sideDrill + _ABF_sideDrillDepth on Dside8)
                    string mfxSetting = (p.ThicknessMm <= 16.5) ? "Мініфікс - Hettich (16 мм)" : "Мініфікс - Hettich (18 мм)";
                    IntPtr grp = CreateSubGroupAt(panelEntities, "_ABF_sideDrill", cxMm, cyMm, czMm);
                    SetIntAttr(grp, "ABF", "face-drill-group-id", pid);
                    SetBoolAttr(grp, "ABF", "is-side-drill", true);
                    SetIntAttr(grp, "ABF", "minifix-group-a-id", pid);
                    SetStringAttr(grp, "ABF", "setting-name", mfxSetting);

                    IntPtr ents;
                    SUGroupGetEntities(grp, out ents);
                    var edges = new List<IntPtr>();
                    AddCircleEdges24(edges, GetLayer(layerMap, LayerSideDrill8), 0, 0, 0, 4.0 * MmToInch, circlePlane);
                    if (edges.Count > 0) SUEntitiesAddEdges(ents, new UIntPtr((uint)edges.Count), edges.ToArray());

                    // Companion _ABF_sideDrillDepth group showing 33mm bore depth
                    AddSideDrillDepthGroup(panelEntities, cxMm, cyMm, czMm, 4.0, 33.0, circlePlane, p, GetLayer(layerMap, LayerSideDrill8), pid, mfxSetting);
                }
                else if (isSide && (Math.Abs(d.DiameterMm - 4.5) < 0.6 || Math.Abs(d.DiameterMm - 5.0) < 0.6))
                {
                    // Confirmat End-Grain Bore (_ABF_sideDrill + _ABF_sideDrillDepth on Dside4.5)
                    IntPtr grp = CreateSubGroupAt(panelEntities, "_ABF_sideDrill", cxMm, cyMm, czMm);
                    SetStringAttr(grp, "ABF", "component-counter", "{\"type\":\"screw\",\"name\":\"Конфірмат зенкер Muller\"}");
                    SetBoolAttr(grp, "ABF", "is-minifix-part-b", true);
                    SetBoolAttr(grp, "ABF", "is-side-drill", true);
                    SetIntAttr(grp, "ABF", "minifix-group-a-id", pid);
                    SetStringAttr(grp, "ABF", "setting-name", "Конфірмат зенкер Muller");

                    IntPtr ents;
                    SUGroupGetEntities(grp, out ents);
                    var edges = new List<IntPtr>();
                    AddCircleEdges24(edges, GetLayer(layerMap, LayerSideDrill45), 0, 0, 0, 2.25 * MmToInch, circlePlane);
                    if (edges.Count > 0) SUEntitiesAddEdges(ents, new UIntPtr((uint)edges.Count), edges.ToArray());

                    // Companion _ABF_sideDrillDepth group showing 34mm bore depth
                    AddSideDrillDepthGroup(panelEntities, cxMm, cyMm, czMm, 2.25, 34.0, circlePlane, p, GetLayer(layerMap, LayerSideDrill45), pid, "Конфірмат зенкер Muller");
                }
            }
        }

        private static void MapDrillToLocal3D(
            FurniturePanel p,
            DrillHole d,
            int orient,
            out double cxMm,
            out double cyMm,
            out double czMm,
            out int circlePlane)
        {
            string side = (d.Side ?? "face_top").ToLowerInvariant();
            if (orient == 0)
            {
                // Horizontal panel: DX = Length, DY = Width, DZ = Thickness
                if (side == "edge_w1") // Left edge (-X, plane YZ = 1)
                {
                    cxMm = 0.0; cyMm = d.XMm; czMm = d.YMm; circlePlane = 1;
                }
                else if (side == "edge_w2") // Right edge (+X, plane YZ = 1)
                {
                    cxMm = p.DXMm; cyMm = d.XMm; czMm = d.YMm; circlePlane = 1;
                }
                else if (side == "edge_l1") // Front edge (-Y, plane XZ = 2)
                {
                    cxMm = d.XMm; cyMm = 0.0; czMm = d.YMm; circlePlane = 2;
                }
                else if (side == "edge_l2") // Back edge (+Y, plane XZ = 2)
                {
                    cxMm = d.XMm; cyMm = p.DYMm; czMm = d.YMm; circlePlane = 2;
                }
                else
                {
                    cxMm = d.XMm; cyMm = d.YMm;
                    czMm = (side == "face_bottom") ? 0.0 : p.DZMm;
                    circlePlane = 0;
                }
            }
            else if (orient == 1)
            {
                // Vertical Side/Divider: DX = Thickness, DY = Depth, DZ = Height
                bool lengthIsZ = Math.Abs(p.DZMm - p.LengthMm) < 1.0;
                if (side == "edge_w1") // Bottom edge (-Z, plane XY = 0)
                {
                    cxMm = d.YMm; cyMm = d.XMm; czMm = 0.0; circlePlane = 0;
                }
                else if (side == "edge_w2") // Top edge (+Z, plane XY = 0)
                {
                    cxMm = d.YMm; cyMm = d.XMm; czMm = p.DZMm; circlePlane = 0;
                }
                else if (side == "edge_l1") // Front edge (-Y, plane XZ = 2)
                {
                    cxMm = d.YMm; cyMm = 0.0; czMm = d.XMm; circlePlane = 2;
                }
                else if (side == "edge_l2") // Back edge (+Y, plane XZ = 2)
                {
                    cxMm = d.YMm; cyMm = p.DYMm; czMm = d.XMm; circlePlane = 2;
                }
                else
                {
                    cxMm = (side == "face_bottom") ? 0.0 : p.DXMm;
                    cyMm = lengthIsZ ? d.YMm : d.XMm;
                    czMm = lengthIsZ ? d.XMm : d.YMm;
                    circlePlane = 1;
                }
            }
            else
            {
                // Vertical Frontal/Back Panel: DX = Length, DY = Thickness, DZ = Width/Height
                if (side == "edge_w1") // Left edge (-X, plane YZ = 1)
                {
                    cxMm = 0.0; cyMm = d.YMm; czMm = d.XMm; circlePlane = 1;
                }
                else if (side == "edge_w2") // Right edge (+X, plane YZ = 1)
                {
                    cxMm = p.DXMm; cyMm = d.YMm; czMm = d.XMm; circlePlane = 1;
                }
                else
                {
                    cxMm = d.XMm;
                    cyMm = (side == "face_bottom") ? 0.0 : p.DYMm;
                    czMm = d.YMm;
                    circlePlane = 2;
                }
            }
        }

        private static void AddSideDrillDepthGroup(
            IntPtr panelEntities,
            double cxMm,
            double cyMm,
            double czMm,
            double radiusMm,
            double depthMm,
            int edgeCirclePlane,
            FurniturePanel p,
            IntPtr layer,
            int pid,
            string settingName)
        {
            IntPtr grp = CreateSubGroupAt(panelEntities, "_ABF_sideDrillDepth", cxMm, cyMm, czMm);
            SetBoolAttr(grp, "ABF", "is-side-drill-depth", true);
            SetIntAttr(grp, "ABF", "minifix-group-a-id", pid);
            SetStringAttr(grp, "ABF", "setting-name", settingName);

            IntPtr ents;
            SUGroupGetEntities(grp, out ents);
            var edges = new List<IntPtr>();
            double r = radiusMm * MmToInch;
            double d = depthMm * MmToInch;

            if (edgeCirclePlane == 1)
            {
                // Edge is at X=0 or X=DXMm -> depth extends along X
                double dirX = (cxMm < p.DXMm * 0.5) ? d : -d;
                AddEdge3D(edges, layer, 0, -r, 0, dirX, -r, 0);
                AddEdge3D(edges, layer, dirX, -r, 0, dirX, r, 0);
                AddEdge3D(edges, layer, dirX, r, 0, 0, r, 0);
                AddEdge3D(edges, layer, 0, r, 0, 0, -r, 0);
            }
            else if (edgeCirclePlane == 0)
            {
                // Edge is at Z=0 or Z=DZMm -> depth extends along Z
                double dirZ = (czMm < p.DZMm * 0.5) ? d : -d;
                AddEdge3D(edges, layer, 0, -r, 0, 0, -r, dirZ);
                AddEdge3D(edges, layer, 0, -r, dirZ, 0, r, dirZ);
                AddEdge3D(edges, layer, 0, r, dirZ, 0, r, 0);
                AddEdge3D(edges, layer, 0, r, 0, 0, -r, 0);
            }
            else
            {
                // Edge is at Y=0 or Y=DYMm -> depth extends along Y
                double dirY = (cyMm < p.DYMm * 0.5) ? d : -d;
                AddEdge3D(edges, layer, -r, 0, 0, -r, dirY, 0);
                AddEdge3D(edges, layer, -r, dirY, 0, r, dirY, 0);
                AddEdge3D(edges, layer, r, dirY, 0, r, 0, 0);
                AddEdge3D(edges, layer, r, 0, 0, -r, 0, 0);
            }

            if (edges.Count > 0) SUEntitiesAddEdges(ents, new UIntPtr((uint)edges.Count), edges.ToArray());
        }

        private static IntPtr CreateSubGroupAt(IntPtr parentEntities, string name, double xMm, double yMm, double zMm)
        {
            IntPtr grp = IntPtr.Zero;
            SUGroupCreate(out grp);
            SUEntitiesAddGroup(parentEntities, grp);
            SUGroupSetName(grp, Utf8Z(name));
            var tr = SUTransformation.Translation(xMm * MmToInch, yMm * MmToInch, zMm * MmToInch);
            SUGroupSetTransform(grp, ref tr);
            return grp;
        }

        private static IntPtr GetLayer(Dictionary<string, IntPtr> layerMap, string name)
        {
            if (layerMap != null && layerMap.ContainsKey(name)) return layerMap[name];
            return IntPtr.Zero;
        }

        private static void AddCircleEdges24(List<IntPtr> edges, IntPtr layer, double cx, double cy, double cz, double rInch, int plane)
        {
            int segs = 24;
            for (int s = 0; s < segs; s++)
            {
                double a0 = (2.0 * Math.PI * s) / segs;
                double a1 = (2.0 * Math.PI * (s + 1)) / segs;
                if (plane == 0) // XY plane
                {
                    AddEdge3D(edges, layer,
                        cx + rInch * Math.Cos(a0), cy + rInch * Math.Sin(a0), cz,
                        cx + rInch * Math.Cos(a1), cy + rInch * Math.Sin(a1), cz);
                }
                else if (plane == 1) // YZ plane
                {
                    AddEdge3D(edges, layer,
                        cx, cy + rInch * Math.Cos(a0), cz + rInch * Math.Sin(a0),
                        cx, cy + rInch * Math.Cos(a1), cz + rInch * Math.Sin(a1));
                }
                else // XZ plane
                {
                    AddEdge3D(edges, layer,
                        cx + rInch * Math.Cos(a0), cy, cz + rInch * Math.Sin(a0),
                        cx + rInch * Math.Cos(a1), cy, cz + rInch * Math.Sin(a1));
                }
            }
        }

        private static void AddEdge3D(List<IntPtr> edges, IntPtr layer, double x0, double y0, double z0, double x1, double y1, double z1)
        {
            var p0 = new SUPoint3D(x0, y0, z0);
            var p1 = new SUPoint3D(x1, y1, z1);
            IntPtr edge = IntPtr.Zero;
            if (SUEdgeCreate(out edge, ref p0, ref p1) == 0 && edge != IntPtr.Zero)
            {
                if (layer != IntPtr.Zero) SUDrawingElementSetLayer(edge, layer);
                edges.Add(edge);
            }
        }

        private static void SetStringAttr(IntPtr entity, string dictName, string key, string val)
        {
            IntPtr dict = IntPtr.Zero;
            if (SUEntityGetAttributeDictionary(entity, Utf8Z(dictName), out dict) != 0) return;
            IntPtr tv = IntPtr.Zero;
            if (SUTypedValueCreate(out tv) == 0)
            {
                SUTypedValueSetString(tv, Utf8Z(val ?? ""));
                SUAttributeDictionarySetValue(dict, Utf8Z(key), tv);
                SUTypedValueRelease(ref tv);
            }
        }

        private static void SetDoubleAttr(IntPtr entity, string dictName, string key, double val)
        {
            IntPtr dict = IntPtr.Zero;
            if (SUEntityGetAttributeDictionary(entity, Utf8Z(dictName), out dict) != 0) return;
            IntPtr tv = IntPtr.Zero;
            if (SUTypedValueCreate(out tv) == 0)
            {
                SUTypedValueSetDouble(tv, val);
                SUAttributeDictionarySetValue(dict, Utf8Z(key), tv);
                SUTypedValueRelease(ref tv);
            }
        }

        private static void SetIntAttr(IntPtr entity, string dictName, string key, int val)
        {
            IntPtr dict = IntPtr.Zero;
            if (SUEntityGetAttributeDictionary(entity, Utf8Z(dictName), out dict) != 0) return;
            IntPtr tv = IntPtr.Zero;
            if (SUTypedValueCreate(out tv) == 0)
            {
                SUTypedValueSetInt32(tv, val);
                SUAttributeDictionarySetValue(dict, Utf8Z(key), tv);
                SUTypedValueRelease(ref tv);
            }
        }

        private static void SetBoolAttr(IntPtr entity, string dictName, string key, bool val)
        {
            IntPtr dict = IntPtr.Zero;
            if (SUEntityGetAttributeDictionary(entity, Utf8Z(dictName), out dict) != 0) return;
            IntPtr tv = IntPtr.Zero;
            if (SUTypedValueCreate(out tv) == 0)
            {
                SUTypedValueSetBool(tv, val);
                SUAttributeDictionarySetValue(dict, Utf8Z(key), tv);
                SUTypedValueRelease(ref tv);
            }
        }

        private static SUColor ParseHexColor(string hex)
        {
            string h = (hex ?? "#D9BA92").Replace("#", "").Trim();
            byte r = 217, g = 186, b = 146;
            if (h.Length == 6)
            {
                byte.TryParse(h.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out r);
                byte.TryParse(h.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out g);
                byte.TryParse(h.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out b);
            }
            return new SUColor { red = r, green = g, blue = b, alpha = 255 };
        }

        private static byte[] Utf8Z(string s)
        {
            byte[] b = Encoding.UTF8.GetBytes(s ?? "");
            byte[] z = new byte[b.Length + 1];
            Buffer.BlockCopy(b, 0, z, 0, b.Length);
            return z;
        }
    }
}

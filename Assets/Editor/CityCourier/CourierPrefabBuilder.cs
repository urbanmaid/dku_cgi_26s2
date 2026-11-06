using System.IO;
using UnityEditor;
using UnityEngine;

namespace CityCourier.Editor
{
    public static class CourierPrefabBuilder
    {
        private const string ModelsDir = "Assets/Models/CityCourier";
        private const string MaterialsDir = "Assets/Materials/CityCourier";
        private const string PrefabsDir = "Assets/Prefabs/CityCourier";
        private const string TexturesDir = "Assets/Resources/Art/CityCourier";

        [MenuItem("Tools/City Courier/Build City Prefabs")]
        public static void BuildPrefabs()
        {
            EnsureFolder("Assets/Models");
            EnsureFolder(ModelsDir);
            EnsureFolder("Assets/Materials");
            EnsureFolder(MaterialsDir);
            EnsureFolder("Assets/Prefabs");
            EnsureFolder(PrefabsDir);

            AssetDatabase.Refresh();

            // Configure textures so they look crisp and repeat properly where needed
            ConfigureTexture($"{TexturesDir}/AsphaltRoad.png", TextureWrapMode.Repeat);
            ConfigureTexture($"{TexturesDir}/BuildingWindows.png", TextureWrapMode.Repeat);
            ConfigureTexture($"{TexturesDir}/NeonBillboard_Runner.png", TextureWrapMode.Clamp);
            ConfigureTexture($"{TexturesDir}/NeonBillboard_Vertical.png", TextureWrapMode.Clamp);
            ConfigureTexture($"{TexturesDir}/Sign_SlideGate.png", TextureWrapMode.Clamp);
            ConfigureTexture($"{TexturesDir}/Sign_ChevronCyan.png", TextureWrapMode.Clamp);
            ConfigureTexture($"{TexturesDir}/Sign_HazardStripe.png", TextureWrapMode.Clamp);

            AssetDatabase.Refresh();

            // 1. Create persistent Material assets
            var darkHullMat = CreateOrUpdateLitMaterial(
                $"{MaterialsDir}/MAT_Cyber_DarkHull.mat",
                new Color(0.05f, 0.07f, 0.11f),
                new Color(0.01f, 0.02f, 0.04f),
                0.78f,
                0.65f);

            var chromeMat = CreateOrUpdateLitMaterial(
                $"{MaterialsDir}/MAT_Cyber_DarkChrome.mat",
                new Color(0.12f, 0.16f, 0.23f),
                new Color(0.02f, 0.05f, 0.09f),
                0.90f,
                0.85f);

            var wetAsphaltMat = CreateOrUpdateTexturedMaterial(
                $"{MaterialsDir}/MAT_Cyber_WetAsphalt.mat",
                $"{TexturesDir}/AsphaltRoad.png",
                new Color(0.65f, 0.78f, 0.92f),
                new Color(0.06f, 0.14f, 0.24f),
                0.88f,
                0.35f,
                new Vector2(1f, 18f));

            var windowMat = CreateOrUpdateTexturedMaterial(
                $"{MaterialsDir}/MAT_Cyber_Windows.mat",
                $"{TexturesDir}/BuildingWindows.png",
                new Color(0.88f, 0.96f, 1.0f),
                new Color(1.35f, 1.65f, 2.15f),
                0.82f,
                0.45f,
                new Vector2(2f, 6f));

            var neonCyanMat = CreateOrUpdateNeonMaterial(
                $"{MaterialsDir}/MAT_Cyber_NeonCyan.mat",
                new Color(0.15f, 0.95f, 1.0f),
                3.4f);

            var neonMagentaMat = CreateOrUpdateNeonMaterial(
                $"{MaterialsDir}/MAT_Cyber_NeonMagenta.mat",
                new Color(1.0f, 0.16f, 0.72f),
                3.3f);

            var neonGoldMat = CreateOrUpdateNeonMaterial(
                $"{MaterialsDir}/MAT_Cyber_NeonGold.mat",
                new Color(1.0f, 0.76f, 0.14f),
                3.1f);

            var neonRedMat = CreateOrUpdateNeonMaterial(
                $"{MaterialsDir}/MAT_Cyber_NeonRed.mat",
                new Color(1.0f, 0.18f, 0.22f),
                3.4f);

            var neonGreenMat = CreateOrUpdateNeonMaterial(
                $"{MaterialsDir}/MAT_Cyber_NeonGreen.mat",
                new Color(0.18f, 1.0f, 0.56f),
                3.2f);

            var signRunnerMat = CreateOrUpdateTexturedMaterial(
                $"{MaterialsDir}/MAT_Sign_RunnerCity.mat",
                $"{TexturesDir}/NeonBillboard_Runner.png",
                Color.white,
                new Color(1.5f, 1.5f, 1.7f),
                0.78f,
                0.2f,
                Vector2.one);

            var signVerticalMat = CreateOrUpdateTexturedMaterial(
                $"{MaterialsDir}/MAT_Sign_VerticalNeon.mat",
                $"{TexturesDir}/NeonBillboard_Vertical.png",
                Color.white,
                new Color(1.55f, 1.45f, 1.75f),
                0.78f,
                0.2f,
                Vector2.one);

            var signSlideMat = CreateOrUpdateTexturedMaterial(
                $"{MaterialsDir}/MAT_Sign_SlideGate.mat",
                $"{TexturesDir}/Sign_SlideGate.png",
                Color.white,
                new Color(1.6f, 1.45f, 1.25f),
                0.78f,
                0.2f,
                Vector2.one);

            var signChevronMat = CreateOrUpdateTexturedMaterial(
                $"{MaterialsDir}/MAT_Sign_ChevronCyan.mat",
                $"{TexturesDir}/Sign_ChevronCyan.png",
                Color.white,
                new Color(1.3f, 1.7f, 1.9f),
                0.78f,
                0.2f,
                Vector2.one);

            var signDangerMat = CreateOrUpdateTexturedMaterial(
                $"{MaterialsDir}/MAT_Sign_DangerDive.mat",
                $"{TexturesDir}/Sign_HazardStripe.png",
                Color.white,
                new Color(1.8f, 1.35f, 1.35f),
                0.78f,
                0.2f,
                Vector2.one);

            // 2. Load 3D Meshes from .obj files
            var skyscraperMesh = LoadMeshFromObj($"{ModelsDir}/SM_Cyber_Skyscraper.obj");
            var pyramidMesh = LoadMeshFromObj($"{ModelsDir}/SM_Cyber_PyramidArcology.obj");
            var chevronMesh = LoadMeshFromObj($"{ModelsDir}/SM_Cyber_ChevronArrow3D.obj");
            var spinnerMesh = LoadMeshFromObj($"{ModelsDir}/SM_Cyber_SpinnerCar.obj");

            // 3. Build Modular Prefabs
            BuildSkyscraperPrefab(
                $"{PrefabsDir}/PF_Cyber_Skyscraper_Cyan.prefab",
                "PF_Cyber_Skyscraper_Cyan",
                skyscraperMesh,
                windowMat,
                darkHullMat,
                neonCyanMat,
                signVerticalMat,
                true);

            BuildSkyscraperPrefab(
                $"{PrefabsDir}/PF_Cyber_Skyscraper_Magenta.prefab",
                "PF_Cyber_Skyscraper_Magenta",
                skyscraperMesh,
                windowMat,
                darkHullMat,
                neonMagentaMat,
                signRunnerMat,
                false);

            BuildSkyscraperPrefab(
                $"{PrefabsDir}/PF_Cyber_Skyscraper_Gold.prefab",
                "PF_Cyber_Skyscraper_Gold",
                skyscraperMesh,
                windowMat,
                darkHullMat,
                neonGoldMat,
                signVerticalMat,
                true);

            BuildPyramidArcologyPrefab(
                $"{PrefabsDir}/PF_Cyber_PyramidArcology.prefab",
                pyramidMesh,
                windowMat,
                darkHullMat,
                neonMagentaMat,
                neonCyanMat);

            BuildFramedSignPrefab(
                $"{PrefabsDir}/PF_Sign_RunnerCity.prefab",
                "PF_Sign_RunnerCity",
                new Vector2(8.8f, 3.8f),
                signRunnerMat,
                darkHullMat,
                neonCyanMat);

            BuildFramedSignPrefab(
                $"{PrefabsDir}/PF_Sign_VerticalNeon.prefab",
                "PF_Sign_VerticalNeon",
                new Vector2(3.6f, 13.5f),
                signVerticalMat,
                darkHullMat,
                neonMagentaMat);

            BuildSlideGatePortalPrefab(
                $"{PrefabsDir}/PF_Prop_SlideGatePortal.prefab",
                chevronMesh,
                darkHullMat,
                neonGoldMat,
                signSlideMat);

            BuildDangerDivePortalPrefab(
                $"{PrefabsDir}/PF_Prop_DangerDivePortal.prefab",
                chevronMesh,
                darkHullMat,
                neonRedMat,
                signDangerMat);

            BuildWallRunGuidePrefab(
                $"{PrefabsDir}/PF_Prop_WallRunGuide_Right.prefab",
                "PF_Prop_WallRunGuide_Right",
                chevronMesh,
                darkHullMat,
                neonCyanMat,
                signChevronMat,
                1f);

            BuildWallRunGuidePrefab(
                $"{PrefabsDir}/PF_Prop_WallRunGuide_Left.prefab",
                "PF_Prop_WallRunGuide_Left",
                chevronMesh,
                darkHullMat,
                neonCyanMat,
                signChevronMat,
                -1f);

            BuildChevronArrowPrefab(
                $"{PrefabsDir}/PF_Guide_ChevronArrow3D_Cyan.prefab",
                "PF_Guide_ChevronArrow3D_Cyan",
                chevronMesh,
                neonCyanMat);

            BuildChevronArrowPrefab(
                $"{PrefabsDir}/PF_Guide_ChevronArrow3D_Gold.prefab",
                "PF_Guide_ChevronArrow3D_Gold",
                chevronMesh,
                neonGoldMat);

            BuildStreetlampPrefab(
                $"{PrefabsDir}/PF_Highway_Streetlamp.prefab",
                darkHullMat,
                neonCyanMat);

            BuildGuardrailPrefab(
                $"{PrefabsDir}/PF_Highway_Guardrail_12m.prefab",
                darkHullMat,
                neonCyanMat,
                neonMagentaMat);

            BuildSpinnerCarPrefab(
                $"{PrefabsDir}/PF_Cyber_SpinnerCar.prefab",
                spinnerMesh,
                chromeMat,
                neonCyanMat,
                neonRedMat);

            BuildCheckpointArchPrefab(
                $"{PrefabsDir}/PF_Prop_CheckpointArch.prefab",
                darkHullMat,
                neonCyanMat,
                neonMagentaMat);

            BuildFinishArchPrefab(
                $"{PrefabsDir}/PF_Prop_FinishGoalArch.prefab",
                chevronMesh,
                darkHullMat,
                neonGreenMat,
                neonGoldMat,
                signRunnerMat);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CourierPrefabBuilder] Successfully built all CityCourier 3D Models, Materials, and Prefabs!");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var folderName = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folderName))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        private static void ConfigureTexture(string assetPath, TextureWrapMode wrapMode)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Default;
            importer.wrapMode = wrapMode;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 2048;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();
        }

        private static Mesh LoadMeshFromObj(string assetPath)
        {
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (mesh != null)
            {
                return mesh;
            }

            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (var asset in assets)
            {
                if (asset is Mesh foundMesh)
                {
                    return foundMesh;
                }
            }

            Debug.LogWarning($"[CourierPrefabBuilder] Could not find mesh at {assetPath}");
            return null;
        }

        private static Shader FindLitShader()
        {
            return Shader.Find("Universal Render Pipeline/Lit")
                   ?? Shader.Find("Standard")
                   ?? Shader.Find("Sprites/Default");
        }

        private static Material CreateOrUpdateLitMaterial(
            string assetPath,
            Color baseColor,
            Color emissionColor,
            float smoothness,
            float metallic)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (material == null)
            {
                material = new Material(FindLitShader());
                AssetDatabase.CreateAsset(material, assetPath);
            }

            ApplyMaterialSettings(material, null, baseColor, emissionColor, smoothness, metallic, Vector2.one);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CreateOrUpdateNeonMaterial(
            string assetPath,
            Color neonColor,
            float intensity)
        {
            var hdrEmission = new Color(neonColor.r * intensity, neonColor.g * intensity, neonColor.b * intensity, 1f);
            return CreateOrUpdateLitMaterial(assetPath, neonColor, hdrEmission, 0.9f, 0.15f);
        }

        private static Material CreateOrUpdateTexturedMaterial(
            string assetPath,
            string texturePath,
            Color baseTint,
            Color emissionTint,
            float smoothness,
            float metallic,
            Vector2 tiling)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (material == null)
            {
                material = new Material(FindLitShader());
                AssetDatabase.CreateAsset(material, assetPath);
            }

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            ApplyMaterialSettings(material, texture, baseTint, emissionTint, smoothness, metallic, tiling);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ApplyMaterialSettings(
            Material material,
            Texture2D texture,
            Color baseColor,
            Color emissionColor,
            float smoothness,
            float metallic,
            Vector2 tiling)
        {
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", baseColor);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", baseColor);
            }

            if (texture != null)
            {
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", texture);
                    material.SetTextureScale("_BaseMap", tiling);
                }

                if (material.HasProperty("_MainTex"))
                {
                    material.SetTexture("_MainTex", texture);
                    material.SetTextureScale("_MainTex", tiling);
                }
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", smoothness);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }

            material.EnableKeyword("_EMISSION");
            if (material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", emissionColor);
            }

            if (texture != null && material.HasProperty("_EmissionMap"))
            {
                material.SetTexture("_EmissionMap", texture);
            }
        }

        private static void SavePrefab(GameObject root, string prefabPath)
        {
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
        }

        private static GameObject AddMeshChild(
            Transform parent,
            string name,
            Mesh mesh,
            Vector3 localPos,
            Vector3 localScale,
            Quaternion localRot,
            Material material)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
            go.transform.localScale = localScale;

            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        private static GameObject AddPrimitiveChild(
            Transform parent,
            string name,
            PrimitiveType primitiveType,
            Vector3 localPos,
            Vector3 localScale,
            Quaternion localRot,
            Material material)
        {
            var go = GameObject.CreatePrimitive(primitiveType);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
            go.transform.localScale = localScale;

            var collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            return go;
        }

        private static void AddFramedSignChild(
            Transform parent,
            string name,
            Vector3 localPos,
            Quaternion localRot,
            Vector2 size,
            Material displayMat,
            Material hullMat,
            Material trimMat)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.localPosition = localPos;
            root.localRotation = localRot;

            AddPrimitiveChild(root, "BackingFrame", PrimitiveType.Cube, new Vector3(0f, 0f, 0.14f), new Vector3(size.x + 0.36f, size.y + 0.36f, 0.22f), Quaternion.identity, hullMat);
            AddPrimitiveChild(root, "TopNeonTrim", PrimitiveType.Cube, new Vector3(0f, size.y * 0.5f + 0.18f, 0.08f), new Vector3(size.x + 0.45f, 0.12f, 0.28f), Quaternion.identity, trimMat);
            AddPrimitiveChild(root, "BottomNeonTrim", PrimitiveType.Cube, new Vector3(0f, -size.y * 0.5f - 0.18f, 0.08f), new Vector3(size.x + 0.45f, 0.12f, 0.28f), Quaternion.identity, trimMat);
            AddPrimitiveChild(root, "FrontFaceQuad", PrimitiveType.Quad, new Vector3(0f, 0f, -0.01f), new Vector3(size.x, size.y, 1f), Quaternion.identity, displayMat);
            AddPrimitiveChild(root, "BackFaceQuad", PrimitiveType.Quad, new Vector3(0f, 0f, 0.27f), new Vector3(size.x, size.y, 1f), Quaternion.Euler(0f, 180f, 0f), displayMat);
        }

        private static void BuildSkyscraperPrefab(
            string prefabPath,
            string name,
            Mesh skyscraperMesh,
            Material windowMat,
            Material hullMat,
            Material accentMat,
            Material signMat,
            bool verticalSign)
        {
            var root = new GameObject(name);
            var t = root.transform;

            const float width = 14f;
            const float height = 68f;
            const float depth = 14f;

            // Main chamfered 3D tower mesh
            AddMeshChild(
                t,
                "TowerHull3D",
                skyscraperMesh,
                new Vector3(0f, height * 0.5f, 0f),
                new Vector3(width, height, depth),
                Quaternion.identity,
                windowMat);

            // Corner structural fins
            for (var sx = -1; sx <= 1; sx += 2)
            {
                for (var sz = -1; sz <= 1; sz += 2)
                {
                    AddPrimitiveChild(
                        t,
                        "CornerFin",
                        PrimitiveType.Cube,
                        new Vector3(sx * (width * 0.47f), height * 0.5f, sz * (depth * 0.47f)),
                        new Vector3(0.75f, height * 1.01f, 0.75f),
                        Quaternion.identity,
                        hullMat);
                }
            }

            // Horizontal neon trim rings
            float[] ringRatios = { 0.32f, 0.64f, 0.88f, 0.99f };
            foreach (var r in ringRatios)
            {
                var scaleFactor = r > 0.78f ? 0.73f : (r > 0.48f ? 0.87f : 1.02f);
                AddPrimitiveChild(
                    t,
                    "NeonBelt",
                    PrimitiveType.Cube,
                    new Vector3(0f, height * r, 0f),
                    new Vector3(width * scaleFactor, 0.42f, depth * scaleFactor),
                    Quaternion.identity,
                    accentMat);
            }

            // Rooftop communication spire & glowing beacon
            var spireHeight = 14f;
            AddPrimitiveChild(
                t,
                "RoofSpire",
                PrimitiveType.Cylinder,
                new Vector3(0f, height + spireHeight * 0.5f, 0f),
                new Vector3(0.28f, spireHeight * 0.5f, 0.28f),
                Quaternion.identity,
                hullMat);
            AddPrimitiveChild(
                t,
                "RoofBeacon",
                PrimitiveType.Sphere,
                new Vector3(0f, height + spireHeight, 0f),
                new Vector3(1.1f, 1.1f, 1.1f),
                Quaternion.identity,
                accentMat);

            // Upright neon billboard on front facade (-Z)
            if (verticalSign)
            {
                AddFramedSignChild(
                    t,
                    "VerticalNeonSign",
                    new Vector3(0f, height * 0.52f, -depth * 0.5f - 0.35f),
                    Quaternion.identity,
                    new Vector2(3.6f, 15.5f),
                    signMat,
                    hullMat,
                    accentMat);
            }
            else
            {
                AddFramedSignChild(
                    t,
                    "HorizontalBillboard",
                    new Vector3(0f, height * 0.62f, -depth * 0.5f - 0.35f),
                    Quaternion.identity,
                    new Vector2(width * 0.88f, 4.4f),
                    signMat,
                    hullMat,
                    accentMat);
            }

            SavePrefab(root, prefabPath);
        }

        private static void BuildPyramidArcologyPrefab(
            string prefabPath,
            Mesh pyramidMesh,
            Material windowMat,
            Material hullMat,
            Material neonMagentaMat,
            Material neonCyanMat)
        {
            var root = new GameObject("PF_Cyber_PyramidArcology");
            var t = root.transform;

            AddMeshChild(
                t,
                "PyramidHull3D",
                pyramidMesh,
                new Vector3(0f, 38f, 0f),
                new Vector3(36f, 76f, 32f),
                Quaternion.identity,
                windowMat);

            float[] beltHeights = { 18f, 38f, 56f, 72f };
            float[] beltWidths = { 32f, 24f, 16f, 9f };
            for (var i = 0; i < beltHeights.Length; i++)
            {
                AddPrimitiveChild(
                    t,
                    $"NeonBelt_{i}",
                    PrimitiveType.Cube,
                    new Vector3(0f, beltHeights[i], 0f),
                    new Vector3(beltWidths[i], 0.55f, beltWidths[i] * 0.9f),
                    Quaternion.identity,
                    i % 2 == 0 ? neonMagentaMat : neonCyanMat);
            }

            AddPrimitiveChild(
                t,
                "ApexCore",
                PrimitiveType.Sphere,
                new Vector3(0f, 78f, 0f),
                new Vector3(3.6f, 3.6f, 3.6f),
                Quaternion.identity,
                neonMagentaMat);

            SavePrefab(root, prefabPath);
        }

        private static void BuildFramedSignPrefab(
            string prefabPath,
            string name,
            Vector2 size,
            Material displayMat,
            Material hullMat,
            Material trimMat)
        {
            var root = new GameObject(name);
            AddFramedSignChild(root.transform, "SignBoard", Vector3.zero, Quaternion.identity, size, displayMat, hullMat, trimMat);
            SavePrefab(root, prefabPath);
        }

        private static void BuildSlideGatePortalPrefab(
            string prefabPath,
            Mesh chevronMesh,
            Material hullMat,
            Material neonGoldMat,
            Material signSlideMat)
        {
            var root = new GameObject("PF_Prop_SlideGatePortal");
            var t = root.transform;

            AddFramedSignChild(
                t,
                "SlideSignBoard",
                new Vector3(0f, 3.35f, 0f),
                Quaternion.identity,
                new Vector2(6.6f, 1.35f),
                signSlideMat,
                hullMat,
                neonGoldMat);

            AddPrimitiveChild(t, "LowClearanceLaserBar", PrimitiveType.Cube, new Vector3(0f, 1.22f, 0f), new Vector3(7.6f, 0.14f, 0.24f), Quaternion.identity, neonGoldMat);

            for (var i = -1; i <= 1; i++)
            {
                AddMeshChild(
                    t,
                    $"DownArrow3D_{i + 1}",
                    chevronMesh,
                    new Vector3(i * 2.1f, 2.35f, -0.25f),
                    new Vector3(0.55f, 0.55f, 0.55f),
                    Quaternion.Euler(0f, 0f, -90f),
                    neonGoldMat);
            }

            SavePrefab(root, prefabPath);
        }

        private static void BuildDangerDivePortalPrefab(
            string prefabPath,
            Mesh chevronMesh,
            Material hullMat,
            Material neonRedMat,
            Material signDangerMat)
        {
            var root = new GameObject("PF_Prop_DangerDivePortal");
            var t = root.transform;

            AddFramedSignChild(
                t,
                "DangerDiveBoard",
                new Vector3(0f, 4.9f, 0f),
                Quaternion.identity,
                new Vector2(7.0f, 1.45f),
                signDangerMat,
                hullMat,
                neonRedMat);

            for (var s = -1; s <= 1; s += 2)
            {
                AddPrimitiveChild(t, "LaserEmitterPost", PrimitiveType.Cube, new Vector3(s * 3.7f, 2.5f, 0f), new Vector3(0.32f, 5.0f, 0.32f), Quaternion.identity, hullMat);
                AddPrimitiveChild(t, "LaserEmitterStrip", PrimitiveType.Cube, new Vector3(s * 3.55f, 2.5f, 0f), new Vector3(0.10f, 4.6f, 0.14f), Quaternion.identity, neonRedMat);
            }

            for (var i = -1; i <= 1; i += 2)
            {
                AddMeshChild(
                    t,
                    "DiveChevron3D",
                    chevronMesh,
                    new Vector3(i * 2.2f, 3.6f, -0.35f),
                    new Vector3(0.6f, 0.6f, 0.6f),
                    Quaternion.Euler(0f, 0f, -90f),
                    neonRedMat);
            }

            SavePrefab(root, prefabPath);
        }

        private static void BuildWallRunGuidePrefab(
            string prefabPath,
            string name,
            Mesh chevronMesh,
            Material hullMat,
            Material neonCyanMat,
            Material signChevronMat,
            float inwardNormalX)
        {
            var root = new GameObject(name);
            var t = root.transform;

            var yRot = inwardNormalX < 0f ? 90f : -90f;
            AddFramedSignChild(
                t,
                "WallRunSignBoard",
                new Vector3(0f, 2.6f, -3.2f),
                Quaternion.Euler(0f, yRot, 0f),
                new Vector2(3.6f, 1.35f),
                signChevronMat,
                hullMat,
                neonCyanMat);

            for (var i = 0; i < 4; i++)
            {
                var zOffset = -1.2f + i * 2.1f;
                AddMeshChild(
                    t,
                    $"WallChevron3D_{i}",
                    chevronMesh,
                    new Vector3(0f, 2.35f + i * 0.12f, zOffset),
                    new Vector3(0.75f, 0.75f, 0.75f),
                    Quaternion.Euler(0f, yRot, 0f),
                    neonCyanMat);
            }

            AddPrimitiveChild(t, "TopNeonRail", PrimitiveType.Cube, new Vector3(0f, 4.4f, 0f), new Vector3(0.14f, 0.12f, 9.6f), Quaternion.identity, neonCyanMat);
            AddPrimitiveChild(t, "BottomNeonRail", PrimitiveType.Cube, new Vector3(0f, 0.7f, 0f), new Vector3(0.14f, 0.12f, 9.6f), Quaternion.identity, neonCyanMat);

            SavePrefab(root, prefabPath);
        }

        private static void BuildChevronArrowPrefab(
            string prefabPath,
            string name,
            Mesh chevronMesh,
            Material neonMat)
        {
            var root = new GameObject(name);
            AddMeshChild(root.transform, "Chevron3D", chevronMesh, Vector3.zero, Vector3.one, Quaternion.identity, neonMat);
            SavePrefab(root, prefabPath);
        }

        private static void BuildStreetlampPrefab(
            string prefabPath,
            Material hullMat,
            Material neonCyanMat)
        {
            var root = new GameObject("PF_Highway_Streetlamp");
            var t = root.transform;

            AddPrimitiveChild(t, "LampPole", PrimitiveType.Cylinder, new Vector3(0f, 3.1f, 0f), new Vector3(0.14f, 3.1f, 0.14f), Quaternion.identity, hullMat);
            AddPrimitiveChild(t, "CantileverArm", PrimitiveType.Cube, new Vector3(-0.75f, 6.05f, 0f), new Vector3(1.65f, 0.12f, 0.16f), Quaternion.Euler(0f, 0f, 8f), hullMat);
            AddPrimitiveChild(t, "NeonHead", PrimitiveType.Cube, new Vector3(-1.35f, 5.95f, 0f), new Vector3(0.95f, 0.09f, 0.30f), Quaternion.identity, neonCyanMat);

            var lightObj = new GameObject("StreetPointLight");
            lightObj.transform.SetParent(t, false);
            lightObj.transform.localPosition = new Vector3(-1.35f, 5.6f, 0f);
            var pointLight = lightObj.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.color = new Color(0.25f, 0.90f, 1.0f);
            pointLight.intensity = 1.6f;
            pointLight.range = 14f;
            pointLight.shadows = LightShadows.None;

            SavePrefab(root, prefabPath);
        }

        private static void BuildGuardrailPrefab(
            string prefabPath,
            Material hullMat,
            Material neonCyanMat,
            Material neonMagentaMat)
        {
            var root = new GameObject("PF_Highway_Guardrail_12m");
            var t = root.transform;

            AddPrimitiveChild(t, "CurbBase", PrimitiveType.Cube, new Vector3(0f, 0.18f, 0f), new Vector3(0.32f, 0.36f, 12f), Quaternion.identity, hullMat);
            AddPrimitiveChild(t, "UpperLaserRail", PrimitiveType.Cube, new Vector3(0f, 0.72f, 0f), new Vector3(0.08f, 0.07f, 12f), Quaternion.identity, neonCyanMat);
            AddPrimitiveChild(t, "LowerLaserRail", PrimitiveType.Cube, new Vector3(0f, 0.42f, 0f), new Vector3(0.06f, 0.05f, 12f), Quaternion.identity, neonMagentaMat);

            for (var z = -6f; z <= 6f; z += 4f)
            {
                AddPrimitiveChild(t, "BollardPost", PrimitiveType.Cube, new Vector3(0f, 0.50f, z), new Vector3(0.20f, 1.0f, 0.22f), Quaternion.identity, hullMat);
                AddPrimitiveChild(t, "BollardCap", PrimitiveType.Cube, new Vector3(0f, 1.03f, z), new Vector3(0.22f, 0.08f, 0.24f), Quaternion.identity, neonCyanMat);
            }

            SavePrefab(root, prefabPath);
        }

        private static void BuildSpinnerCarPrefab(
            string prefabPath,
            Mesh spinnerMesh,
            Material chromeMat,
            Material neonCyanMat,
            Material neonRedMat)
        {
            var root = new GameObject("PF_Cyber_SpinnerCar");
            var t = root.transform;

            AddMeshChild(t, "SpinnerBody3D", spinnerMesh, Vector3.zero, new Vector3(1.35f, 1.15f, 1.35f), Quaternion.identity, chromeMat);
            AddPrimitiveChild(t, "HeadlightBar", PrimitiveType.Cube, new Vector3(0f, 0.0f, 1.55f), new Vector3(1.1f, 0.12f, 0.15f), Quaternion.identity, neonCyanMat);
            AddPrimitiveChild(t, "TaillightStreak", PrimitiveType.Cube, new Vector3(0f, 0.05f, -2.4f), new Vector3(1.15f, 0.10f, 2.8f), Quaternion.identity, neonRedMat);

            SavePrefab(root, prefabPath);
        }

        private static void BuildCheckpointArchPrefab(
            string prefabPath,
            Material hullMat,
            Material neonCyanMat,
            Material neonMagentaMat)
        {
            var root = new GameObject("PF_Prop_CheckpointArch");
            var t = root.transform;

            for (var s = -1; s <= 1; s += 2)
            {
                AddPrimitiveChild(t, "PylonBase", PrimitiveType.Cube, new Vector3(s * 3.85f, 2.6f, 0f), new Vector3(0.35f, 5.2f, 0.35f), Quaternion.identity, hullMat);
                AddPrimitiveChild(t, "PylonNeonCore", PrimitiveType.Cube, new Vector3(s * 3.68f, 2.6f, 0f), new Vector3(0.10f, 4.9f, 0.14f), Quaternion.identity, neonCyanMat);
            }

            AddPrimitiveChild(t, "TopArchBridge", PrimitiveType.Cube, new Vector3(0f, 5.15f, 0f), new Vector3(8.1f, 0.28f, 0.35f), Quaternion.identity, hullMat);
            AddPrimitiveChild(t, "TopArchNeon", PrimitiveType.Cube, new Vector3(0f, 4.98f, 0f), new Vector3(7.6f, 0.10f, 0.18f), Quaternion.identity, neonMagentaMat);

            SavePrefab(root, prefabPath);
        }

        private static void BuildFinishArchPrefab(
            string prefabPath,
            Mesh chevronMesh,
            Material hullMat,
            Material neonGreenMat,
            Material neonGoldMat,
            Material signRunnerMat)
        {
            var root = new GameObject("PF_Prop_FinishGoalArch");
            var t = root.transform;

            for (var s = -1; s <= 1; s += 2)
            {
                AddPrimitiveChild(t, "GoalPillar", PrimitiveType.Cube, new Vector3(s * 3.85f, 3.8f, 0f), new Vector3(0.55f, 7.6f, 0.55f), Quaternion.identity, hullMat);
                AddPrimitiveChild(t, "GoalPillarNeon", PrimitiveType.Cube, new Vector3(s * 3.55f, 3.8f, 0f), new Vector3(0.14f, 7.2f, 0.22f), Quaternion.identity, neonGreenMat);
            }

            AddFramedSignChild(
                t,
                "FinishBillboard",
                new Vector3(0f, 6.8f, 0f),
                Quaternion.identity,
                new Vector2(8.2f, 2.1f),
                signRunnerMat,
                hullMat,
                neonGreenMat);

            for (var i = -1; i <= 1; i++)
            {
                AddMeshChild(
                    t,
                    $"FinishChevron3D_{i + 1}",
                    chevronMesh,
                    new Vector3(i * 2.2f, 4.8f, -0.25f),
                    new Vector3(0.65f, 0.65f, 0.65f),
                    Quaternion.Euler(0f, 0f, 90f),
                    neonGoldMat);
            }

            SavePrefab(root, prefabPath);
        }
    }
}

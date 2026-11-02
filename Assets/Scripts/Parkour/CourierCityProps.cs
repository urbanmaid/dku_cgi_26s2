using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CityCourier
{
    public sealed class CourierCityProps : MonoBehaviour
    {
        const string ContainerName = "Cyberpunk City Props";

        struct TrafficSpinner
        {
            public Transform Transform;
            public float Speed;
            public float MinZ;
            public float MaxZ;
            public float BaseY;
            public float BobPhase;
        }

        readonly List<TrafficSpinner> spinners = new List<TrafficSpinner>();
        readonly List<Transform> rotatingRings = new List<Transform>();

        Material darkHullMat;
        Material wetAsphaltMat;
        Material windowTowerMat;
        Material billboardRunnerMat;
        Material billboardVertMat;
        Material slideGateSignMat;
        Material chevronCyanMat;
        Material hazardDiveSignMat;
        Material neonCyanMat;
        Material neonMagentaMat;
        Material neonGoldMat;
        Material neonRedMat;
        Material neonGreenMat;
        Material holoBeamCyanMat;
        Material holoBeamMagentaMat;

        Mesh chamferTowerMesh;
        Mesh pyramidArcologyMesh;
        Mesh chevron3DMesh;
        Mesh spinnerBodyMesh;

        void Awake()
        {
            BuildCityIfNeeded();
        }

        void Update()
        {
            float dt = Time.deltaTime;
            float t = Time.time;

            // Animate flying cyberpunk hover-traffic along sky-lanes
            for (int i = 0; i < spinners.Count; i++)
            {
                TrafficSpinner s = spinners[i];
                if (s.Transform == null) continue;
                Vector3 pos = s.Transform.localPosition;
                pos.z += s.Speed * dt;
                if (s.Speed > 0f && pos.z > s.MaxZ) pos.z = s.MinZ;
                else if (s.Speed < 0f && pos.z < s.MinZ) pos.z = s.MaxZ;
                pos.y = s.BaseY + Mathf.Sin(t * 2.2f + s.BobPhase) * 0.25f;
                s.Transform.localPosition = pos;
            }

            // Animate rotating holographic rings at portals & goal
            for (int i = 0; i < rotatingRings.Count; i++)
            {
                Transform r = rotatingRings[i];
                if (r == null) continue;
                r.Rotate(0f, (i % 2 == 0 ? 38f : -32f) * dt, 0f, Space.Self);
            }
        }

        public void BuildCityIfNeeded()
        {
            Transform existing = transform.Find(ContainerName);
            if (existing != null) return;
            RebuildCity();
        }

        public void RebuildCity()
        {
            spinners.Clear();
            rotatingRings.Clear();

            Transform existing = transform.Find(ContainerName);
            if (existing != null)
            {
                if (Application.isPlaying) Destroy(existing.gameObject);
                else DestroyImmediate(existing.gameObject);
            }

            InitProceduralMeshes();
            InitMaterials();
            UpgradeExistingStageVisuals();

            Transform root = new GameObject(ContainerName).transform;
            root.SetParent(transform, false);

            BuildOpenHighwayGuardrailsAndLights(root);
            Build3DSkyscraperCanyon(root);
            BuildDistantMountainsAndArcologies(root);
            BuildFlyingSpinnerTraffic(root);
            BuildParkourGuidanceProps(root);
            ConfigureCyberpunkAtmosphere();
        }

        void UpgradeExistingStageVisuals()
        {
            // 1. Hide the 5m-tall opaque black corridor walls that blocked 85% of the view!
            // Keep their BoxColliders 100% enabled so physics and tests are completely unchanged.
            string[] invisibleWalls = { "Left Boundary", "Right Boundary", "Lobby End", "Route End" };
            foreach (string wallName in invisibleWalls)
            {
                Transform w = transform.Find(wallName);
                if (w != null)
                {
                    var r = w.GetComponent<Renderer>();
                    if (r != null) r.enabled = false;
                }
            }

            // 2. Hide legacy plain black "City Tower" boxes
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.name == "City Tower")
                    child.gameObject.SetActive(false);
                else if (child.name.StartsWith("00 ") || child.name.StartsWith("02 ") ||
                         child.name.StartsWith("04 ") || child.name.StartsWith("05 ") ||
                         child.name.StartsWith("06 ") || child.name.StartsWith("07 "))
                {
                    // Apply wet reflective cyberpunk asphalt to road platforms
                    var r = child.GetComponent<Renderer>();
                    if (r != null && wetAsphaltMat != null && !child.name.Contains("Cyan"))
                        r.sharedMaterial = wetAsphaltMat;
                }
                else if (child.name.StartsWith("03 Basic Slide") || child.name.StartsWith("04 Slide Then Jump"))
                {
                    var r = child.GetComponent<Renderer>();
                    if (r != null && darkHullMat != null && !child.name.Contains("Lip"))
                        r.sharedMaterial = darkHullMat;
                }
            }
        }

        void InitProceduralMeshes()
        {
            chamferTowerMesh = CreateChamferedTowerMesh();
            pyramidArcologyMesh = CreatePyramidArcologyMesh();
            chevron3DMesh = CreateChevronArrow3DMesh();
            spinnerBodyMesh = CreateHoverSpinnerMesh();
        }

        void InitMaterials()
        {
            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null) lit = Shader.Find("Standard");
            Shader unlit = Shader.Find("Universal Render Pipeline/Unlit");
            if (unlit == null) unlit = lit;

            darkHullMat = CreateColorMaterial(lit, "Cyber_DarkHull", new Color(0.05f, 0.07f, 0.13f), Color.black, 0.65f, 0.4f);
            neonCyanMat = CreateColorMaterial(lit, "Cyber_NeonCyan", new Color(0.1f, 0.95f, 1f), new Color(0.0f, 2.2f, 2.6f), 0.85f, 0.1f);
            neonMagentaMat = CreateColorMaterial(lit, "Cyber_NeonMagenta", new Color(1f, 0.15f, 0.75f), new Color(2.4f, 0.15f, 1.6f), 0.85f, 0.1f);
            neonGoldMat = CreateColorMaterial(lit, "Cyber_NeonGold", new Color(1f, 0.82f, 0.12f), new Color(2.2f, 1.5f, 0.1f), 0.85f, 0.1f);
            neonRedMat = CreateColorMaterial(lit, "Cyber_NeonRed", new Color(1f, 0.12f, 0.22f), new Color(2.5f, 0.15f, 0.3f), 0.85f, 0.1f);
            neonGreenMat = CreateColorMaterial(lit, "Cyber_NeonGreen", new Color(0.15f, 1f, 0.48f), new Color(0.2f, 2.4f, 0.8f), 0.85f, 0.1f);

            holoBeamCyanMat = CreateColorMaterial(unlit, "Cyber_HoloCyan", new Color(0.05f, 0.88f, 1f), new Color(0f, 1.4f, 1.8f), 0.5f, 0f);
            holoBeamMagentaMat = CreateColorMaterial(unlit, "Cyber_HoloMagenta", new Color(1f, 0.2f, 0.78f), new Color(1.6f, 0.2f, 1.1f), 0.5f, 0f);

            Texture2D roadTex = Resources.Load<Texture2D>("Art/CityCourier/AsphaltRoad");
            wetAsphaltMat = CreateTexturedMaterial(lit, "Cyber_WetAsphalt", roadTex, new Color(0.75f, 0.82f, 0.95f), new Color(0.05f, 0.18f, 0.25f), new Vector2(1f, 6f), 0.78f, 0.35f);

            Texture2D winTex = Resources.Load<Texture2D>("Art/CityCourier/BuildingWindows");
            windowTowerMat = CreateTexturedMaterial(lit, "Cyber_Windows", winTex, Color.white, new Color(0.95f, 1.05f, 1.25f), new Vector2(2f, 3f), 0.7f, 0.25f);

            Texture2D runnerTex = Resources.Load<Texture2D>("Art/CityCourier/NeonBillboard_Runner");
            billboardRunnerMat = CreateTexturedMaterial(unlit, "Cyber_SignRunner", runnerTex, Color.white, new Color(1.3f, 1.3f, 1.3f), Vector2.one, 0.5f, 0f);

            Texture2D vertTex = Resources.Load<Texture2D>("Art/CityCourier/NeonBillboard_Vertical");
            billboardVertMat = CreateTexturedMaterial(unlit, "Cyber_SignVert", vertTex, Color.white, new Color(1.3f, 1.3f, 1.3f), Vector2.one, 0.5f, 0f);

            Texture2D slideTex = Resources.Load<Texture2D>("Art/CityCourier/Sign_SlideGate");
            slideGateSignMat = CreateTexturedMaterial(unlit, "Cyber_SignSlide", slideTex, Color.white, new Color(1.3f, 1.3f, 1.3f), Vector2.one, 0.5f, 0f);

            Texture2D chevTex = Resources.Load<Texture2D>("Art/CityCourier/Sign_ChevronCyan");
            chevronCyanMat = CreateTexturedMaterial(unlit, "Cyber_SignChevron", chevTex, Color.white, new Color(1.3f, 1.3f, 1.3f), Vector2.one, 0.5f, 0f);

            Texture2D hazTex = Resources.Load<Texture2D>("Art/CityCourier/Sign_HazardStripe");
            hazardDiveSignMat = CreateTexturedMaterial(unlit, "Cyber_SignDive", hazTex, Color.white, new Color(1.3f, 1.3f, 1.3f), Vector2.one, 0.5f, 0f);
        }

        static Material CreateColorMaterial(Shader shader, string name, Color baseCol, Color emissionCol, float smoothness, float metallic)
        {
            var mat = new Material(shader) { name = name };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseCol);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", baseCol);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (emissionCol.maxColorComponent > 0.01f)
            {
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emissionCol);
            }
            return mat;
        }

        static Material CreateTexturedMaterial(Shader shader, string name, Texture2D tex, Color baseCol, Color emissionCol, Vector2 tiling, float smoothness, float metallic)
        {
            var mat = CreateColorMaterial(shader, name, baseCol, emissionCol, smoothness, metallic);
            if (tex != null)
            {
                tex.wrapMode = TextureWrapMode.Repeat;
                tex.filterMode = FilterMode.Bilinear;
                if (mat.HasProperty("_BaseMap"))
                {
                    mat.SetTexture("_BaseMap", tex);
                    mat.SetTextureScale("_BaseMap", tiling);
                }
                if (mat.HasProperty("_MainTex"))
                {
                    mat.SetTexture("_MainTex", tex);
                    mat.SetTextureScale("_MainTex", tiling);
                }
                if (mat.HasProperty("_EmissionMap"))
                {
                    mat.SetTexture("_EmissionMap", tex);
                }
            }
            return mat;
        }

        // =====================================================================
        // 1. OPEN HIGHWAY GUARDRAILS, CANTILEVER STREETLIGHTS & UNDER-BRIDGES
        // =====================================================================
        void BuildOpenHighwayGuardrailsAndLights(Transform root)
        {
            Transform highway = new GameObject("01_Open_Highway_Deck").transform;
            highway.SetParent(root, false);

            // Waist-high sleek guardrails along each platform (instead of 5m tall black walls!)
            Vector2[] platformSpans = {
                new Vector2(-12f, 24f),
                new Vector2(45f, 79f),
                new Vector2(84f, 98f),
                new Vector2(103f, 128f),
                new Vector2(139f, 156f),
            };

            foreach (Vector2 span in platformSpans)
            {
                float len = span.y - span.x;
                float midZ = (span.x + span.y) * 0.5f;
                foreach (float side in new[] { -1f, 1f })
                {
                    float rx = side * 3.92f;
                    // Lower metal curb
                    PropPrimitive(highway, PrimitiveType.Cube, "Guardrail_Curb",
                        new Vector3(rx, 0.18f, midZ), Quaternion.identity, new Vector3(0.22f, 0.36f, len), darkHullMat);
                    // Glowing cyan & magenta laser safety rails at waist height (y = 0.55m and 0.82m)
                    PropPrimitive(highway, PrimitiveType.Cube, "Guardrail_LaserLower",
                        new Vector3(rx, 0.54f, midZ), Quaternion.identity, new Vector3(0.06f, 0.06f, len), neonCyanMat);
                    PropPrimitive(highway, PrimitiveType.Cube, "Guardrail_LaserUpper",
                        new Vector3(rx, 0.82f, midZ), Quaternion.identity, new Vector3(0.06f, 0.06f, len), neonMagentaMat);

                    // Angled bollard posts every 4m
                    for (float z = span.x + 1f; z <= span.y - 1f; z += 4f)
                    {
                        PropPrimitive(highway, PrimitiveType.Cube, "Guardrail_Bollard",
                            new Vector3(rx, 0.48f, z), Quaternion.Euler(0f, 0f, -side * 8f), new Vector3(0.14f, 0.95f, 0.18f), darkHullMat);
                    }
                }

                // Under-deck steel truss beams & deep canyon support pylons
                for (float z = span.x + 4f; z <= span.y - 2f; z += 12f)
                {
                    PropPrimitive(highway, PrimitiveType.Cube, "Deck_CrossBeam",
                        new Vector3(0f, -1.05f, z), Quaternion.identity, new Vector3(8.6f, 0.55f, 1.2f), darkHullMat);
                    foreach (float side in new[] { -3.3f, 3.3f })
                    {
                        PropPrimitive(highway, PrimitiveType.Cylinder, "Canyon_SupportPylon",
                            new Vector3(side, -16f, z), Quaternion.identity, new Vector3(0.95f, 15f, 0.95f), darkHullMat);
                        PropPrimitive(highway, PrimitiveType.Cube, "Pylon_NeonVein",
                            new Vector3(side, -12f, z), Quaternion.identity, new Vector3(1.05f, 16f, 0.16f), neonCyanMat);
                    }
                }
            }

            // Futuristic Cantilever Streetlamps curving over the road + Real Neon Point Lights
            float[] lampZ = { -6f, 8f, 20f, 48f, 64f, 77f, 88f, 104f, 120f, 142f, 153f };
            for (int i = 0; i < lampZ.Length; i++)
            {
                float z = lampZ[i];
                Material lampNeon = (i % 2 == 0) ? neonCyanMat : neonMagentaMat;
                Color lightCol = (i % 2 == 0) ? new Color(0.1f, 0.9f, 1f) : new Color(1f, 0.25f, 0.8f);

                foreach (float side in new[] { -1f, 1f })
                {
                    float lx = side * 4.05f;
                    // Vertical mast
                    PropPrimitive(highway, PrimitiveType.Cylinder, "Streetlamp_Mast",
                        new Vector3(lx, 2.6f, z), Quaternion.identity, new Vector3(0.14f, 2.6f, 0.14f), darkHullMat);
                    // Angled cantilever arm reaching inward at y = 5.4m
                    PropPrimitive(highway, PrimitiveType.Cube, "Streetlamp_Arm",
                        new Vector3(side * 3.1f, 5.45f, z), Quaternion.Euler(0f, 0f, side * 18f), new Vector3(2.2f, 0.14f, 0.16f), darkHullMat);
                    // Glowing LED luminaire head
                    PropPrimitive(highway, PrimitiveType.Cube, "Streetlamp_Head",
                        new Vector3(side * 2.3f, 5.65f, z), Quaternion.identity, new Vector3(0.9f, 0.1f, 0.28f), lampNeon);
                }

                // Real point light to cast neon reflections on wet asphalt
                var lightGo = new GameObject("Cyber_StreetLight_" + z);
                lightGo.transform.SetParent(highway, false);
                lightGo.transform.localPosition = new Vector3(0f, 5.2f, z);
                var ptLight = lightGo.AddComponent<Light>();
                ptLight.type = LightType.Point;
                ptLight.color = lightCol;
                ptLight.range = 18f;
                ptLight.intensity = 2.2f;
                ptLight.shadows = LightShadows.None;
            }
        }

        // =====================================================================
        // 2. 3D CHAMFERED SKYSCRAPERS, HOLO-TOWERS & UPRIGHT BILLBOARDS
        // =====================================================================
        void Build3DSkyscraperCanyon(Transform root)
        {
            Transform canyon = new GameObject("02_Cyberpunk_3D_Skyscrapers").transform;
            canyon.SetParent(root, false);

            Material[] neonTrims = { neonCyanMat, neonMagentaMat, neonGoldMat };

            for (int i = 0; i < 18; i++)
            {
                float z = -14f + i * 10.5f;
                for (int s = 0; s < 2; s++)
                {
                    float side = (s == 0) ? -1f : 1f;
                    int hash = i * 11 + s * 17;
                    float bx = side * (11.5f + (hash % 3) * 3.2f);
                    float width = 6.8f + (hash % 2) * 1.8f;
                    float depth = 7.2f + ((hash + 1) % 3) * 1.6f;
                    float height = 22f + (hash * 7 % 26);
                    float baseY = -18f;
                    Material trimMat = neonTrims[hash % neonTrims.Length];

                    // Main 3D Chamfered Skyscraper Mesh (octagonal beveled tower with sloped cyber roof!)
                    PropCustomMesh(canyon, "Chamfered_Skyscraper", chamferTowerMesh,
                        new Vector3(bx, baseY, z), Quaternion.Euler(0f, (s == 0 ? 0f : 180f), 0f),
                        new Vector3(width, height, depth), windowTowerMat);

                    // Glowing horizontal neon rings around tower
                    float roofY = baseY + height;
                    PropPrimitive(canyon, PrimitiveType.Cube, "Tower_NeonBelt_Upper",
                        new Vector3(bx, roofY * 0.72f, z), Quaternion.identity,
                        new Vector3(width * 1.02f, 0.28f, depth * 1.02f), trimMat);
                    PropPrimitive(canyon, PrimitiveType.Cube, "Tower_NeonBelt_Mid",
                        new Vector3(bx, roofY * 0.35f, z), Quaternion.identity,
                        new Vector3(width * 1.02f, 0.22f, depth * 1.02f), neonCyanMat);

                    // Twin Rooftop Communication Spires & Rotating Holo-Ring on every 3rd building
                    PropPrimitive(canyon, PrimitiveType.Cylinder, "Rooftop_Spire_A",
                        new Vector3(bx - 1.2f, roofY + 3.5f, z - 1.0f), Quaternion.identity,
                        new Vector3(0.16f, 3.5f, 0.16f), darkHullMat);
                    PropPrimitive(canyon, PrimitiveType.Sphere, "Spire_Beacon",
                        new Vector3(bx - 1.2f, roofY + 7.1f, z - 1.0f), Quaternion.identity,
                        new Vector3(0.45f, 0.45f, 0.45f), trimMat);

                    if (i % 3 == 0)
                    {
                        // Vertical Sky-Searchlight beam rising into the night sky
                        PropPrimitive(canyon, PrimitiveType.Cylinder, "Sky_SearchlightBeam",
                            new Vector3(bx, roofY + 18f, z), Quaternion.identity,
                            new Vector3(0.35f, 18f, 0.35f), s == 0 ? holoBeamCyanMat : holoBeamMagentaMat);
                    }

                    // UPRIGHT Neon Billboards mounted on the inward face of the buildings (using Quad so UVs are 100% upright!)
                    float inwardX = bx - side * (width * 0.52f + 0.15f);
                    Quaternion faceRoadRot = Quaternion.Euler(0f, side < 0 ? -90f : 90f, 0f);

                    if (i % 2 == 0)
                    {
                        // Upright Vertical "NEON RUNNER" Sign
                        CreateFramedSignBoard(canyon, "Vertical_NeonSign",
                            new Vector3(inwardX, Mathf.Clamp(roofY - 6f, 4f, 12f), z),
                            faceRoadRot, new Vector2(2.4f, 8.5f), billboardVertMat, trimMat);
                    }
                    else if (i % 3 == 1)
                    {
                        // Upright Horizontal "RUNNER : CITY" Billboard
                        CreateFramedSignBoard(canyon, "Horizontal_NeonBillboard",
                            new Vector3(inwardX, Mathf.Clamp(roofY - 4.5f, 4.5f, 10f), z),
                            faceRoadRot, new Vector2(6.4f, 2.4f), billboardRunnerMat, neonCyanMat);
                    }
                }
            }
        }

        // =====================================================================
        // 3. DISTANT PYRAMID ARCOLOGIES & 3D MOUNTAIN HORIZON
        // =====================================================================
        void BuildDistantMountainsAndArcologies(Transform root)
        {
            Transform horizon = new GameObject("03_Horizon_Arcologies_And_Mountains").transform;
            horizon.SetParent(root, false);

            // Blade-Runner style tapered Pyramid Mega-Arcologies in the mid-ground (x = ±28..±38)
            for (int i = 0; i < 8; i++)
            {
                float z = -5f + i * 24f;
                for (int s = 0; s < 2; s++)
                {
                    float side = (s == 0) ? -1f : 1f;
                    float ax = side * (29f + (i % 2) * 5f);
                    float h = 36f + (i * 7 % 18);
                    PropCustomMesh(horizon, "Pyramid_MegaArcology", pyramidArcologyMesh,
                        new Vector3(ax, -18f, z), Quaternion.Euler(0f, s * 45f, 0f),
                        new Vector3(16f, h, 16f), windowTowerMat);
                    // Glowing apex crown on top of the pyramid arcology
                    PropPrimitive(horizon, PrimitiveType.Sphere, "Arcology_ApexCore",
                        new Vector3(ax, -18f + h + 0.8f, z), Quaternion.identity,
                        new Vector3(1.8f, 1.8f, 1.8f), (i + s) % 2 == 0 ? neonMagentaMat : neonCyanMat);
                }
            }

            // Instantiate the actual 3D Mountain_Alpine.obj model on the distant horizon if available
            GameObject mountainPrefab = Resources.Load<GameObject>("Models/Mountain_Alpine");
            if (mountainPrefab != null)
            {
                Vector3[] mountainPositions = {
                    new Vector3(0f, -14f, 215f),
                    new Vector3(-65f, -16f, 185f),
                    new Vector3(65f, -16f, 185f),
                };
                Vector3[] mountainScales = {
                    new Vector3(6.5f, 5.5f, 5.5f),
                    new Vector3(5.0f, 4.2f, 4.5f),
                    new Vector3(5.0f, 4.2f, 4.5f),
                };
                for (int i = 0; i < mountainPositions.Length; i++)
                {
                    GameObject m = Instantiate(mountainPrefab, horizon);
                    m.name = "Cyber_Horizon_Mountain_" + i;
                    m.transform.localPosition = mountainPositions[i];
                    m.transform.localRotation = Quaternion.Euler(0f, i * 35f, 0f);
                    m.transform.localScale = mountainScales[i];
                    foreach (var col in m.GetComponentsInChildren<Collider>(true))
                    {
                        if (Application.isPlaying) Destroy(col);
                        else DestroyImmediate(col);
                    }
                    foreach (var r in m.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        string gName = r.gameObject.name.ToLowerInvariant();
                        if (gName.Contains("snow")) r.sharedMaterial = neonCyanMat;
                        else if (gName.Contains("grass")) r.sharedMaterial = neonMagentaMat;
                        else r.sharedMaterial = darkHullMat;
                    }
                }
            }
        }

        // =====================================================================
        // 4. ANIMATED FLYING CYBER-SPINNER HOVER TRAFFIC
        // =====================================================================
        void BuildFlyingSpinnerTraffic(Transform root)
        {
            Transform traffic = new GameObject("04_Flying_Spinner_Traffic").transform;
            traffic.SetParent(root, false);

            float[] laneX = { -8.2f, -14.5f, 8.2f, 14.5f };
            float[] laneY = { 2.8f, 9.5f, 3.2f, 11.0f };
            float[] laneDir = { 1f, -1f, -1f, 1f };

            for (int lane = 0; lane < laneX.Length; lane++)
            {
                for (int k = 0; k < 6; k++)
                {
                    float z = -15f + k * 32f + (lane * 7f);
                    float dir = laneDir[lane];
                    var vehicle = new GameObject($"CyberSpinner_L{lane}_{k}").transform;
                    vehicle.SetParent(traffic, false);
                    vehicle.localPosition = new Vector3(laneX[lane], laneY[lane], z);
                    vehicle.localRotation = Quaternion.Euler(0f, dir > 0f ? 0f : 180f, 0f);

                    // Sleek 3D aerodynamic spinner hull
                    PropCustomMesh(vehicle, "Spinner_Hull", spinnerBodyMesh,
                        Vector3.zero, Quaternion.identity, new Vector3(1.1f, 0.55f, 2.6f), darkHullMat);

                    // Glowing cyan headlights (front +Z of vehicle)
                    PropPrimitive(vehicle, PrimitiveType.Cube, "Spinner_Headlights",
                        new Vector3(0f, 0.05f, 1.25f), Quaternion.identity, new Vector3(0.85f, 0.12f, 0.15f), neonCyanMat);

                    // Glowing red/magenta thruster tail-trail (back -Z of vehicle)
                    PropPrimitive(vehicle, PrimitiveType.Cube, "Spinner_TailTrail",
                        new Vector3(0f, 0.08f, -2.1f), Quaternion.identity, new Vector3(0.75f, 0.10f, 1.8f),
                        lane % 2 == 0 ? neonRedMat : neonMagentaMat);

                    spinners.Add(new TrafficSpinner
                    {
                        Transform = vehicle,
                        Speed = dir * (22f + (k % 3) * 6f),
                        MinZ = -25f,
                        MaxZ = 175f,
                        BaseY = laneY[lane],
                        BobPhase = k * 1.3f + lane
                    });
                }
            }
        }

        // =====================================================================
        // 5. PARKOUR ROUTE GUIDANCE PROPS (3D CHEVRONS, UPRIGHT GATES & RINGS)
        // =====================================================================
        void BuildParkourGuidanceProps(Transform root)
        {
            Transform guide = new GameObject("05_Parkour_Route_Guidance").transform;
            guide.SetParent(root, false);

            // [00] START PORTAL ARCH & UPRIGHT "RUNNER : CITY" SIGN (z = -2.8)
            // Facing -Z (rotation = identity) so the player at z = -8 looking +Z reads it 100% upright!
            CreateFramedSignBoard(guide, "StartPortal_RunnerBanner",
                new Vector3(0f, 4.35f, -2.85f), Quaternion.identity,
                new Vector2(6.8f, 2.4f), billboardRunnerMat, neonCyanMat);

            foreach (float sx in new[] { -3.45f, 3.45f })
            {
                PropPrimitive(guide, PrimitiveType.Cylinder, "StartPortal_Pylon",
                    new Vector3(sx, 2.2f, -2.8f), Quaternion.identity, new Vector3(0.38f, 2.2f, 0.38f), darkHullMat);
                PropPrimitive(guide, PrimitiveType.Cube, "StartPortal_NeonStrip",
                    new Vector3(sx, 2.2f, -3.02f), Quaternion.identity, new Vector3(0.14f, 4.2f, 0.08f), neonCyanMat);
            }

            // [01] JUMP HURDLE 3D CHEVRON ARROWS (z = 9..12)
            for (int i = 0; i < 3; i++)
            {
                // 3D extruded arrows on the road pointing forward (+Z) toward the jump hurdle
                PropCustomMesh(guide, "Jump_FloorArrow3D_" + i, chevron3DMesh,
                    new Vector3(0f, 0.03f, 8.2f + i * 1.1f), Quaternion.identity,
                    new Vector3(1.8f, 0.06f, 0.75f), i == 2 ? neonGoldMat : neonCyanMat);
            }
            // Side upward-pointing 3D chevrons flanking the Jump Hurdle at z = 12
            foreach (float sx in new[] { -3.65f, 3.65f })
            {
                for (int h = 0; h < 2; h++)
                {
                    PropCustomMesh(guide, "Jump_UpArrow3D", chevron3DMesh,
                        new Vector3(sx, 1.1f + h * 0.75f, 11.8f), Quaternion.Euler(-90f, 0f, 0f),
                        new Vector3(0.75f, 0.12f, 0.55f), neonGoldMat);
                }
            }

            // [02] WALL-RUN CANYON 3D CHEVRONS & NEON RAILS (z = 21..47, x = ±2.8)
            foreach (float wx in new[] { -2.8f, 2.8f })
            {
                float signSide = Mathf.Sign(wx);
                float inwardX = wx - signSide * 0.28f;
                // Sleek top & bottom glowing trim rails on the cyan wall-run panels
                PropPrimitive(guide, PrimitiveType.Cube, "WallRun_TopTrim",
                    new Vector3(wx, 5.06f, 34f), Quaternion.identity, new Vector3(0.56f, 0.14f, 26.2f), neonGoldMat);
                PropPrimitive(guide, PrimitiveType.Cube, "WallRun_BottomTrim",
                    new Vector3(wx, -1.04f, 34f), Quaternion.identity, new Vector3(0.56f, 0.14f, 26.2f), neonCyanMat);

                // Upright Chevron Banner along the upper wall face (using properly oriented Quad!)
                Quaternion wallFaceRot = Quaternion.Euler(0f, signSide > 0 ? 90f : -90f, 0f);
                CreateFramedSignBoard(guide, "WallRun_DirectionSign",
                    new Vector3(inwardX, 3.65f, 34f), wallFaceRot,
                    new Vector2(18f, 1.1f), chevronCyanMat, neonCyanMat);

                // Series of 3D Forward Chevron Arrows along the wall at z = 24, 30, 36, 42
                for (float z = 24f; z <= 42f; z += 6f)
                {
                    PropCustomMesh(guide, "WallRun_3DArrow", chevron3DMesh,
                        new Vector3(inwardX, 2.2f, z), Quaternion.Euler(0f, 0f, signSide * 90f),
                        new Vector3(0.9f, 0.08f, 0.8f), neonGoldMat);
                }
            }

            // [03 & 04] SLIDE TUNNEL OVERPASS GATES ("SLIDE [SHIFT] LOW GATE" upright LED board!)
            foreach (float tunnelFrontZ in new[] { 53.92f, 70.92f })
            {
                CreateFramedSignBoard(guide, "SlideGate_LED_Display",
                    new Vector3(0f, 2.15f, tunnelFrontZ), Quaternion.identity,
                    new Vector2(7.6f, 1.55f), slideGateSignMat, neonGoldMat);

                // Downward-pointing 3D chevrons on left and right gate pillars
                foreach (float sx in new[] { -3.5f, 3.5f })
                {
                    PropCustomMesh(guide, "Slide_DownArrow3D", chevron3DMesh,
                        new Vector3(sx, 0.85f, tunnelFrontZ - 0.1f), Quaternion.Euler(90f, 0f, 0f),
                        new Vector3(0.7f, 0.12f, 0.55f), neonCyanMat);
                }
            }

            // Slide-Jump Launch Ramp Indicator at z = 78.5
            for (int i = 0; i < 2; i++)
            {
                PropCustomMesh(guide, "SlideJump_LaunchArrow3D", chevron3DMesh,
                    new Vector3(0f, 0.03f, 77.4f + i * 0.9f), Quaternion.identity,
                    new Vector3(2.2f, 0.06f, 0.75f), neonCyanMat);
            }

            // [05] JUMP -> AIR-DIVE OVERHEAD LASER GATE (z = 97.5 takeoff + z = 104.8 warning sign)
            PropCustomMesh(guide, "DiveGap_TakeoffArrow3D", chevron3DMesh,
                new Vector3(0f, 0.03f, 97.2f), Quaternion.identity,
                new Vector3(2.4f, 0.06f, 0.85f), neonGoldMat);

            CreateFramedSignBoard(guide, "LaserGrid_DangerSign",
                new Vector3(0f, 2.35f, 104.75f), Quaternion.identity,
                new Vector2(7.6f, 1.5f), hazardDiveSignMat, neonRedMat);

            for (float z = 105f; z <= 113f; z += 1f)
            {
                foreach (float sx in new[] { -3.88f, 3.88f })
                {
                    PropPrimitive(guide, PrimitiveType.Cylinder, "Laser_EmitterHousing",
                        new Vector3(sx, 1.14f, z), Quaternion.Euler(0f, 0f, 90f),
                        new Vector3(0.24f, 0.18f, 0.24f), darkHullMat);
                    PropPrimitive(guide, PrimitiveType.Sphere, "Laser_EmitterLens",
                        new Vector3(sx, 1.14f, z), Quaternion.identity,
                        new Vector3(0.22f, 0.22f, 0.22f), neonRedMat);
                }
            }

            // [06] INTERACTIVE BRIDGE HOLOGRAM GANTRY (z = 124..128)
            Transform consoleRing = PropPrimitive(guide, PrimitiveType.Cylinder, "Console_HoloRing",
                new Vector3(-1.8f, 0.12f, 124f), Quaternion.identity, new Vector3(1.9f, 0.03f, 1.9f), neonGoldMat).transform;
            rotatingRings.Add(consoleRing);

            // [07] FINAL DELIVERY GOAL HUB & ROTATING HOLOGRAM RINGS (z = 150..153)
            CreateFramedSignBoard(guide, "DeliveryGoal_HeaderBanner",
                new Vector3(0f, 5.2f, 151.8f), Quaternion.identity,
                new Vector2(7.4f, 2.2f), billboardRunnerMat, neonGreenMat);

            for (int r = 0; r < 3; r++)
            {
                float ry = 1.6f + r * 1.4f;
                float scale = 3.6f - r * 0.55f;
                Transform ring = PropPrimitive(guide, PrimitiveType.Cube, "Goal_RotatingHoloFrame_" + r,
                    new Vector3(0f, ry, 152f), Quaternion.Euler(0f, r * 30f, 0f),
                    new Vector3(scale, 0.08f, scale), r % 2 == 0 ? neonGreenMat : neonCyanMat).transform;
                rotatingRings.Add(ring);
            }

            // Towering Sky-Beam column marking the finish line from the start of the course
            PropPrimitive(guide, PrimitiveType.Cylinder, "Goal_SkyBeamColumn",
                new Vector3(0f, 24f, 152f), Quaternion.identity,
                new Vector3(0.55f, 24f, 0.55f), holoBeamCyanMat);
        }

        // Helper: Creates an upright Quad sign mounted slightly in front of a dark 3D metal backing frame
        void CreateFramedSignBoard(Transform parent, string name, Vector3 position, Quaternion rotation, Vector2 size, Material signMat, Material borderMat)
        {
            Transform holder = new GameObject(name).transform;
            holder.SetParent(parent, false);
            holder.localPosition = position;
            holder.localRotation = rotation;

            // 3D Dark Metal Backing Frame
            PropPrimitive(holder, PrimitiveType.Cube, "Frame_Backing",
                new Vector3(0f, 0f, 0.08f), Quaternion.identity, new Vector3(size.x + 0.25f, size.y + 0.25f, 0.14f), darkHullMat);
            // Glowing Neon Top & Bottom Frame Bars
            PropPrimitive(holder, PrimitiveType.Cube, "Frame_TopNeon",
                new Vector3(0f, size.y * 0.5f + 0.08f, 0.04f), Quaternion.identity, new Vector3(size.x + 0.3f, 0.09f, 0.18f), borderMat);
            PropPrimitive(holder, PrimitiveType.Cube, "Frame_BottomNeon",
                new Vector3(0f, -size.y * 0.5f - 0.08f, 0.04f), Quaternion.identity, new Vector3(size.x + 0.3f, 0.09f, 0.18f), borderMat);

            // Front Display Quad (Quad normal points -Z, UVs are (0,0) bottom-left to (1,1) top-right -> 100% upright!)
            PropPrimitive(holder, PrimitiveType.Quad, "Display_FaceFront",
                new Vector3(0f, 0f, -0.01f), Quaternion.identity, new Vector3(size.x, size.y, 1f), signMat);
            // Back Display Quad (rotated 180 around Y so back side is also upright)
            PropPrimitive(holder, PrimitiveType.Quad, "Display_FaceBack",
                new Vector3(0f, 0f, 0.17f), Quaternion.Euler(0f, 180f, 0f), new Vector3(size.x, size.y, 1f), signMat);
        }

        static void ConfigureCyberpunkAtmosphere()
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.06f, 0.09f, 0.18f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 52f;
            RenderSettings.fogEndDistance = 230f;
            RenderSettings.ambientLight = new Color(0.18f, 0.22f, 0.36f);

            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.clearFlags = CameraClearFlags.SolidColor;
                mainCam.backgroundColor = new Color(0.04f, 0.06f, 0.14f);
                mainCam.allowHDR = true;

                // Enable URP Post-Processing on Main Camera via reflection so no extra asmdef dependency is needed
                foreach (var comp in mainCam.GetComponents<Component>())
                {
                    if (comp != null && comp.GetType().Name == "UniversalAdditionalCameraData")
                    {
                        var prop = comp.GetType().GetProperty("renderPostProcessing");
                        if (prop != null) prop.SetValue(comp, true);
                    }
                }
            }
        }

        // =====================================================================
        // PROCEDURAL 3D MESH GENERATORS (CHAMFERED TOWERS, PYRAMIDS, 3D ARROWS)
        // =====================================================================
        static Mesh CreateChamferedTowerMesh()
        {
            // 8-sided chamfered skyscraper with a sloped 45-degree cyber-roof top (unit size 1x1x1, pivot at bottom y=0)
            Mesh mesh = new Mesh { name = "Procedural_ChamferTower" };
            float c = 0.36f;
            float e = 0.5f;
            Vector2[] ring = {
                new Vector2(-c, -e), new Vector2(c, -e),
                new Vector2(e, -c),  new Vector2(e, c),
                new Vector2(c, e),   new Vector2(-c, e),
                new Vector2(-e, c),  new Vector2(-e, -c)
            };

            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            float roofSplit = 0.88f;
            for (int i = 0; i < 8; i++)
            {
                int next = (i + 1) % 8;
                Vector3 b0 = new Vector3(ring[i].x, 0f, ring[i].y);
                Vector3 b1 = new Vector3(ring[next].x, 0f, ring[next].y);
                Vector3 m0 = new Vector3(ring[i].x, roofSplit, ring[i].y);
                Vector3 m1 = new Vector3(ring[next].x, roofSplit, ring[next].y);
                Vector3 t0 = new Vector3(ring[i].x * 0.55f, 1f, ring[i].y * 0.55f);
                Vector3 t1 = new Vector3(ring[next].x * 0.55f, 1f, ring[next].y * 0.55f);

                int vBase = verts.Count;
                verts.Add(b0); verts.Add(b1); verts.Add(m1); verts.Add(m0);
                uvs.Add(new Vector2(0f, 0f)); uvs.Add(new Vector2(1f, 0f));
                uvs.Add(new Vector2(1f, 1f)); uvs.Add(new Vector2(0f, 1f));
                tris.Add(vBase); tris.Add(vBase + 2); tris.Add(vBase + 1);
                tris.Add(vBase); tris.Add(vBase + 3); tris.Add(vBase + 2);

                int vRoof = verts.Count;
                verts.Add(m0); verts.Add(m1); verts.Add(t1); verts.Add(t0);
                uvs.Add(new Vector2(0f, 0f)); uvs.Add(new Vector2(1f, 0f));
                uvs.Add(new Vector2(1f, 0.25f)); uvs.Add(new Vector2(0f, 0.25f));
                tris.Add(vRoof); tris.Add(vRoof + 2); tris.Add(vRoof + 1);
                tris.Add(vRoof); tris.Add(vRoof + 3); tris.Add(vRoof + 2);
            }

            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        static Mesh CreatePyramidArcologyMesh()
        {
            // Tapered Blade-Runner style ziggurat/pyramid arcology (unit size 1x1x1, pivot at bottom y=0)
            Mesh mesh = new Mesh { name = "Procedural_PyramidArcology" };
            Vector3[] b = {
                new Vector3(-0.5f, 0f, -0.5f), new Vector3(0.5f, 0f, -0.5f),
                new Vector3(0.5f, 0f, 0.5f),   new Vector3(-0.5f, 0f, 0.5f)
            };
            Vector3[] t = {
                new Vector3(-0.14f, 1f, -0.14f), new Vector3(0.14f, 1f, -0.14f),
                new Vector3(0.14f, 1f, 0.14f),   new Vector3(-0.14f, 1f, 0.14f)
            };

            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            for (int i = 0; i < 4; i++)
            {
                int next = (i + 1) % 4;
                int idx = verts.Count;
                verts.Add(b[i]); verts.Add(b[next]); verts.Add(t[next]); verts.Add(t[i]);
                uvs.Add(new Vector2(0f, 0f)); uvs.Add(new Vector2(1f, 0f));
                uvs.Add(new Vector2(0.8f, 1f)); uvs.Add(new Vector2(0.2f, 1f));
                tris.Add(idx); tris.Add(idx + 2); tris.Add(idx + 1);
                tris.Add(idx); tris.Add(idx + 3); tris.Add(idx + 2);
            }
            int topIdx = verts.Count;
            verts.Add(t[0]); verts.Add(t[1]); verts.Add(t[2]); verts.Add(t[3]);
            uvs.Add(Vector2.zero); uvs.Add(Vector2.right); uvs.Add(Vector2.one); uvs.Add(Vector2.up);
            tris.Add(topIdx); tris.Add(topIdx + 2); tris.Add(topIdx + 1);
            tris.Add(topIdx); tris.Add(topIdx + 3); tris.Add(topIdx + 2);

            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        static Mesh CreateChevronArrow3DMesh()
        {
            // 3D Extruded V-Chevron Arrow pointing toward +Z
            Mesh mesh = new Mesh { name = "Procedural_Chevron3D" };
            Vector2[] poly = {
                new Vector2(-0.5f, -0.5f),
                new Vector2(0f, 0.15f),
                new Vector2(0.5f, -0.5f),
                new Vector2(0.5f, -0.05f),
                new Vector2(0f, 0.5f),
                new Vector2(-0.5f, -0.05f)
            };

            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();

            // Top face (y = +0.5)
            for (int i = 0; i < 6; i++)
                verts.Add(new Vector3(poly[i].x, 0.5f, poly[i].y));
            // Left wing & Right wing triangles
            tris.AddRange(new[] { 0, 5, 1, 1, 5, 4, 1, 4, 2, 2, 4, 3 });

            // Bottom face (y = -0.5)
            for (int i = 0; i < 6; i++)
                verts.Add(new Vector3(poly[i].x, -0.5f, poly[i].y));
            tris.AddRange(new[] { 6, 7, 11, 7, 10, 11, 7, 8, 10, 8, 9, 10 });

            // Side walls
            for (int i = 0; i < 6; i++)
            {
                int next = (i + 1) % 6;
                int idx = verts.Count;
                verts.Add(new Vector3(poly[i].x, -0.5f, poly[i].y));
                verts.Add(new Vector3(poly[next].x, -0.5f, poly[next].y));
                verts.Add(new Vector3(poly[next].x, 0.5f, poly[next].y));
                verts.Add(new Vector3(poly[i].x, 0.5f, poly[i].y));
                tris.Add(idx); tris.Add(idx + 2); tris.Add(idx + 1);
                tris.Add(idx); tris.Add(idx + 3); tris.Add(idx + 2);
            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        static Mesh CreateHoverSpinnerMesh()
        {
            // Sleek wedge-shaped flying cyberpunk vehicle pointing +Z
            Mesh mesh = new Mesh { name = "Procedural_HoverSpinner" };
            Vector3[] v = {
                new Vector3(-0.35f, 0f, 0.5f),   new Vector3(0.35f, 0f, 0.5f),   // Front nose bottom
                new Vector3(0.5f, 0f, -0.5f),    new Vector3(-0.5f, 0f, -0.5f),  // Rear tail bottom
                new Vector3(-0.22f, 0.35f, 0.35f), new Vector3(0.22f, 0.35f, 0.35f), // Front windshield top
                new Vector3(0.38f, 0.45f, -0.45f), new Vector3(-0.38f, 0.45f, -0.45f) // Rear canopy top
            };
            int[] t = {
                4,5,6, 4,6,7, // Roof
                0,1,5, 0,5,4, // Front nose
                1,2,6, 1,6,5, // Right side
                2,3,7, 2,7,6, // Rear
                3,0,4, 3,4,7, // Left side
                3,2,1, 3,1,0  // Bottom
            };
            mesh.vertices = v;
            mesh.triangles = t;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        static GameObject PropPrimitive(Transform parent, PrimitiveType type, string name, Vector3 localPos, Quaternion localRot, Vector3 localScale, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            var col = go.GetComponent<Collider>();
            if (col != null)
            {
                if (Application.isPlaying) Destroy(col);
                else DestroyImmediate(col);
            }
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
            go.transform.localScale = localScale;
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null && mat != null)
            {
                renderer.sharedMaterial = mat;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
            }
            return go;
        }

        static GameObject PropCustomMesh(Transform parent, string name, Mesh mesh, Vector3 localPos, Quaternion localRot, Vector3 localScale, Material mat)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
            go.transform.localScale = localScale;
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            if (mat != null) mr.sharedMaterial = mat;
            mr.shadowCastingMode = ShadowCastingMode.Off;
            return go;
        }
    }
}

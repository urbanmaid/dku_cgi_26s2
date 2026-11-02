using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CityCourier.Editor
{
    public static class CourierSceneBuilder
    {
        const string ScenePath = "Assets/Scenes/SampleScene.unity";
        static Transform root;
        static CourierStage stage;
        static Material floor, dark, cyan, yellow, red, green, blue, white;

        [MenuItem("Tools/City Courier/Rebuild SampleScene POC")]
        public static void Build()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var old = GameObject.Find("City Courier POC");
            if (old != null) Object.DestroyImmediate(old);
            var demo = GameObject.Find("GLPrimitivesWithSpaceKeyNewInputSystem");
            if (demo != null) Object.DestroyImmediate(demo);
            Directory.CreateDirectory("Assets/Art/CityCourier");
            AssetDatabase.Refresh();
            floor = Material("Asphalt", new Color(0.15f, 0.2f, 0.27f));
            dark = Material("Structure", new Color(0.07f, 0.1f, 0.16f));
            cyan = Material("Wall Run Cyan", new Color(0.03f, 0.85f, 0.9f));
            yellow = Material("Letter Gold", new Color(1f, 0.72f, 0.06f));
            red = Material("Hazard Red", new Color(1f, 0.12f, 0.12f));
            green = Material("Goal Green", new Color(0.2f, 1f, 0.4f));
            blue = Material("Portal Blue", new Color(0.15f, 0.4f, 1f));
            white = Material("Player White", new Color(0.9f, 0.94f, 1));
            root = new GameObject("City Courier POC").transform;
            stage = root.gameObject.AddComponent<CourierStage>();

            Platform("00 Lobby + Move + Jump", -12, 24);
            Platform("02 Wall Run Landing + Slide", 45, 79);
            Platform("04 Slide Jump Landing", 84, 98);
            Platform("05 Jump Dive Landing", 103, 128);
            Platform("07 Delivery", 139, 156);
            stage.Road = Platform("06 Interactive Bridge", 128, 139);
            stage.Road.SetActive(false);

            // Tall side boundaries make the training route linear and prevent walking around obstacles.
            Box("Left Boundary", new Vector3(-4.3f, 2.5f, 72), new Vector3(0.5f, 5, 170), dark);
            Box("Right Boundary", new Vector3(4.3f, 2.5f, 72), new Vector3(0.5f, 5, 170), dark);
            Box("Lobby End", new Vector3(0, 2.5f, -12), new Vector3(9, 5, 0.5f), dark);
            Box("Route End", new Vector3(0, 2.5f, 156), new Vector3(9, 5, 0.5f), dark);
            stage.EntryBarrier = Box("Portal Entry Barrier", new Vector3(0, 2.5f, -0.7f), new Vector3(8, 5, 0.4f), blue);
            Box("01 Jump Hurdle", new Vector3(0, 0.65f, 12), new Vector3(8, 1.3f, 1), yellow);

            foreach (float x in new[] { -2.8f, 2.8f })
            {
                var wall = Box("02 Cyan Wall Run Surface", new Vector3(x, 2, 34), new Vector3(0.5f, 6, 26), cyan);
                wall.AddComponent<WallRunSurface>();
            }
            Tunnel("03 Basic Slide", 57, 6);
            Tunnel("04 Slide Then Jump", 74.5f, 7);
            // Landing starts at 103. The overhead hazard requires diving and remaining low.
            var hazard = Trigger("05 Jump Then Dive Hazard", CourierTrigger.TriggerKind.Hazard,
                new Vector3(0, 3.1f, 109), new Vector3(8, 4, 8));
            hazard.Message = "상단 레이저 접촉 — 공중 Shift / B로 급강하한 뒤 낮게 통과하세요";
            for (float z = 105; z <= 113; z += 1)
                Visual("Overhead Laser", new Vector3(0, 1.14f, z), new Vector3(8, 0.08f, 0.1f), red);
            stage.RoadBarrier = Box("06 Closed Road", new Vector3(0, 2.5f, 127), new Vector3(8, 5, 0.4f), red);

            Letter(0, 18);
            Letter(1, 49);
            Letter(2, 66);
            Letter(3, 89);
            Letter(4, 117);
            Checkpoint(19);
            Checkpoint(50);
            Checkpoint(67);
            Checkpoint(90);
            Checkpoint(118);

            Hint(5, "이동: WASD / LS. Ctrl: 천천히 걷기. 노란 장애물 앞에서 Space / A로 점프하세요.");
            Hint(20, "긴 틈: 청록색 벽 옆으로 붙어 점프한 뒤 전진하면 벽타기. 벽 옆에서 Space / A를 다시 누르면 벽 반대쪽으로 도약합니다.");
            Hint(52, "낮은 통로: 지상에서 Shift / B로 슬라이드하세요. 누르고 있으면 슬라이드 유지, 놓으면 천장이 없는 곳에서 일어납니다.");
            Hint(69, "슬라이드 → 점프: 통로 출구에서 Space / A로 점프하여 틈을 건너세요. 슬라이드의 속도가 점프에 이어집니다.");
            Hint(93, "점프 → 슬라이드: 다음 틈을 점프로 건너고 Shift / B로 급강하하세요. 빨간 상단 레이저 아래에서는 슬라이드를 유지하세요.");
            Hint(120, "주황색 도로 제어기 옆에서 E / X를 눌러 다리를 연결하세요.");

            var guide = Interactable("Guide - E or X", new Vector3(-2, 0.9f, -7), cyan,
                CourierInteractable.InteractionKind.Guide);
            guide.Dialogue = new[] {
                "도시 배달원 훈련에 오신 것을 환영합니다. 파란 포탈 가까이에서 E / X를 누르면 같은 씬의 코스로 진입합니다. 다시 E / X를 눌러 다음 안내를 들으세요.",
                "WASD / LS 이동, Space / A 점프와 벽점프, Shift / B 슬라이드와 공중 급강하. 청록색 벽 가까이에서 전진하면 벽을 탑니다.",
                "마우스 / RS로 시점을 조절하고 휠 버튼 / Y로 이동 방향에 맞춥니다. Ctrl은 천천히 걷기입니다.",
                "노란 편지 다섯 개를 모두 모으고 제어기를 작동시켜 초록 목적지로 향하세요. 추락하거나 레이저에 닿으면 최근 체크포인트로 복귀하며 편지는 유지됩니다."
            };
            var portal = Interactable("Start Portal - E or X", new Vector3(0, 1, -2.8f), blue,
                CourierInteractable.InteractionKind.Portal);
            var roadSwitch = Interactable("Road Console - E or X", new Vector3(-1.8f, 0.8f, 124), yellow,
                CourierInteractable.InteractionKind.RoadSwitch);
            stage.Interactables = new[] { guide, portal, roadSwitch };

            var goal = Box("Delivery Beacon", new Vector3(0, 2, 152), new Vector3(2, 4, 1), green);
            stage.GoalBeacon = goal.GetComponent<Renderer>();
            stage.CompletedMaterial = yellow;
            Trigger("Delivery Arrival", CourierTrigger.TriggerKind.Goal, new Vector3(0, 1.5f, 150), new Vector3(8, 3, 2));

            var player = new GameObject("Courier Player");
            player.transform.SetParent(root);
            player.layer = 2; // Ignore Raycast: player is excluded from world and camera clearance checks.
            player.transform.position = new Vector3(0, 0.1f, -8);
            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = Vector3.up * 0.9f;
            controller.stepOffset = 0.2f;
            controller.skinWidth = 0.025f;
            controller.minMoveDistance = 0;
            var motor = player.AddComponent<CourierMotor>();
            stage.Player = motor;
            motor.Stage = stage;
            motor.SlideSpeed = 17f;
            motor.SlideEndSpeed = 7f;
            motor.SlideDuration = 0.85f;
            motor.SlideEasePower = 2f;
            motor.TurnSharpness = 12f;
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Courier Body";
            Object.DestroyImmediate(body.GetComponent<Collider>());
            body.transform.SetParent(player.transform, false);
            body.transform.localPosition = Vector3.up * 0.9f;
            body.transform.localScale = new Vector3(0.65f, 0.9f, 0.65f);
            body.GetComponent<Renderer>().sharedMaterial = white;
            motor.Body = body.transform;
            var pack = Visual("Courier Bag", new Vector3(0, 1, -0.32f), new Vector3(0.5f, 0.5f, 0.25f), yellow);
            pack.transform.SetParent(body.transform, false);
            pack.transform.localPosition = new Vector3(0, 0, -0.5f);
            pack.transform.localScale = new Vector3(0.75f, 0.55f, 0.4f);

            Camera camera = Camera.main;
            if (camera == null) camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 400;
            camera.fieldOfView = 72;
            var follow = camera.GetComponent<CourierCamera>();
            if (follow == null) follow = camera.gameObject.AddComponent<CourierCamera>();
            follow.Target = player.transform;
            follow.Motor = motor;
            follow.RecenterTime = 0.3f;
            motor.View = follow;
            camera.transform.position = new Vector3(0, 3.5f, -13);
            camera.transform.rotation = Quaternion.Euler(18, 0, 0);

            var audio = root.gameObject.AddComponent<CourierAudio>();
            audio.Stage = stage;
            audio.Motor = motor;
            audio.BgmClip = LoadClip("Cyberpunk_BGM");
            audio.FootstepClip = LoadClip("SFX_Footstep");
            audio.JumpClip = LoadClip("SFX_Jump");
            audio.WallJumpClip = LoadClip("SFX_WallJump");
            audio.SlideClip = LoadClip("SFX_Slide");
            audio.DiveClip = LoadClip("SFX_Dive");
            audio.LandClip = LoadClip("SFX_Land");
            audio.WallRunLoopClip = LoadClip("SFX_WallRunLoop");
            audio.LetterPickupClip = LoadClip("SFX_LetterPickup");
            audio.CheckpointClip = LoadClip("SFX_Checkpoint");
            audio.DamageClip = LoadClip("SFX_Damage");
            audio.StageClearClip = LoadClip("SFX_StageClear");

            // Build Cyberpunk City Skyline, Highway Infrastructure, and Parkour Route Guidance Props
            var cityProps = root.gameObject.AddComponent<CourierCityProps>();
            cityProps.RebuildCity();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[POC BUILD OK] SampleScene saved with Cyberpunk City props, audio, and five letters.");
        }

        static AudioClip LoadClip(string name)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/CityCourier/" + name + ".wav");
        }

        static Material Material(string name, Color color)
        {
            string path = "Assets/Art/CityCourier/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", 0.25f);
            EditorUtility.SetDirty(material);
            return material;
        }

        static GameObject Box(string name, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(root);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }
        static GameObject Visual(string name, Vector3 position, Vector3 scale, Material material)
        {
            var go = Box(name, position, scale, material);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }
        static GameObject Platform(string name, float from, float to)
        {
            var go = Box(name, new Vector3(0, -0.4f, (from + to) / 2), new Vector3(8, 0.8f, to - from), floor);
            for (float z = from + 1; z < to; z += 3)
            {
                var mark = Visual("Route Stripe", new Vector3(0, 0.015f, z), new Vector3(0.12f, 0.02f, 1), cyan);
                mark.transform.SetParent(go.transform, true);
            }
            return go;
        }
        static void Tunnel(string name, float z, float length)
        {
            Box(name, new Vector3(0, 3.05f, z), new Vector3(8, 3.9f, length), dark);
            Visual(name + " Yellow Lip", new Vector3(0, 1.15f, z - length / 2 - 0.03f), new Vector3(8, 0.12f, 0.08f), yellow);
        }
        static CourierTrigger Trigger(string name, CourierTrigger.TriggerKind kind, Vector3 position, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root);
            go.transform.position = position;
            var collider = go.AddComponent<BoxCollider>();
            collider.size = size;
            collider.isTrigger = true;
            var trigger = go.AddComponent<CourierTrigger>();
            trigger.Kind = kind;
            trigger.Stage = stage;
            return trigger;
        }
        static void Letter(int index, float z)
        {
            var trigger = Trigger("Letter " + (index + 1), CourierTrigger.TriggerKind.Letter,
                new Vector3(0, 1, z), new Vector3(7.9f, 2, 1.2f));
            trigger.LetterIndex = index;
            var visual = Visual("Gold Envelope", new Vector3(0, 1, z), new Vector3(0.9f, 0.6f, 0.15f), yellow);
            visual.transform.SetParent(trigger.transform, true);
            visual.AddComponent<CourierLetter>();
        }
        static void Checkpoint(float z)
        {
            var trigger = Trigger("Checkpoint " + z, CourierTrigger.TriggerKind.Checkpoint,
                new Vector3(0, 1, z), new Vector3(8, 2, 0.5f));
            trigger.SpawnPoint = new Vector3(0, 0.1f, z);
            Visual("Checkpoint Mark", new Vector3(0, 0.02f, z), new Vector3(8, 0.02f, 0.25f), green);
        }
        static void Hint(float z, string text)
        {
            Trigger("Hint " + z, CourierTrigger.TriggerKind.Hint, new Vector3(0, 1, z), new Vector3(8, 3, 1)).Message = text;
        }
        static CourierInteractable Interactable(string name, Vector3 position, Material color, CourierInteractable.InteractionKind kind)
        {
            var go = Box(name, position, new Vector3(0.8f, 1.5f, 0.8f), color);
            var item = go.AddComponent<CourierInteractable>();
            item.Kind = kind;
            item.Indicator = go.GetComponent<Renderer>();
            item.ActivatedMaterial = green;
            return item;
        }

        public static void CapturePreview()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var camera = Camera.main;
            camera.transform.position = new Vector3(0, 3.2f, -11);
            camera.transform.LookAt(new Vector3(0, 1, 4));
            Capture(camera, "courier-start.png");
            camera.transform.position = new Vector3(20, 43, 51);
            camera.transform.LookAt(new Vector3(0, 0, 76));
            Capture(camera, "courier-course.png");
            // The first render can show placeholder shaders during editor shader warm-up.
            camera.transform.position = new Vector3(0, 3.2f, -11);
            camera.transform.LookAt(new Vector3(0, 1, 4));
            Capture(camera, "courier-start.png");
        }

        public static void BuildAndCapture()
        {
            Build();
            CapturePreview();
        }

        static void Capture(Camera camera, string name)
        {
            var target = new RenderTexture(1280, 800, 24);
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            var texture = new Texture2D(1280, 800, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, 1280, 800), 0, 0);
            texture.Apply();
            Directory.CreateDirectory("Logs");
            File.WriteAllBytes("Logs/" + name, texture.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(texture);
            Object.DestroyImmediate(target);
        }
    }
}

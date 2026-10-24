#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Generates a separate preview scene for the custom URP Lina Style shader.
/// The imported OBJ is a real 3D model, and material slots are assigned explicitly.
/// The user's current scene is not overwritten.
/// </summary>
public static class LinaMountainDemoCreator
{
    private const string Root = "Assets/LinaStylePack/";
    private const string DemoFolder = Root + "Demo/";
    private const string MountainModel = Root + "Models/Mountain_Alpine.obj";

    [MenuItem("Tools/Lina Style/2 - Create and Open Mountain Demo")]
    public static void CreateMountainDemo()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        GameObject sourceModel = AssetDatabase.LoadAssetAtPath<GameObject>(MountainModel);
        Material grass = GetMaterial("MAT_05_Mint_Terrain");
        Material stone = GetMaterial("MAT_04_Lavender_Stone");
        Material snow = GetMaterial("MAT_09_Snow_Cream");
        Material cyan = GetMaterial("MAT_06_Cyan_Crystal");
        Material pink = GetMaterial("MAT_07_Pink_Neon");
        if (sourceModel == null || grass == null || stone == null || snow == null)
        {
            EditorUtility.DisplayDialog("Lina Mountain", "Missing assets! Copy the ENTIRE LinaStylePack folder into Assets, then try again.\n\nMissing model or one of: MAT_05_Mint_Terrain, MAT_04_Lavender_Stone, MAT_09_Snow_Cream.", "OK");
            return;
        }

        // This menu only works with the custom shader from the original pack.
        if (stone.shader == null || stone.shader.name != "LinaStyle/StylizedSoftLit_URP")
        {
            EditorUtility.DisplayDialog("Lina Mountain", "The existing LinaStyle URP shader is missing or not assigned. Check the shader file and URP project.", "OK");
            return;
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // A broad practice plinth below the mountain (the OBJ itself is movable).
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Mint Plateau (example floor)";
        floor.transform.position = new Vector3(0f, -0.55f, 0f);
        floor.transform.localScale = new Vector3(16.6f, 0.65f, 16.6f);
        floor.GetComponent<MeshRenderer>().sharedMaterial = grass;

        // Instantiate the model and connect actual Lina custom-shader materials.
        GameObject mountain = (GameObject)PrefabUtility.InstantiatePrefab(sourceModel);
        mountain.name = "Mountain - Lina Custom Shader";
        mountain.transform.position = Vector3.zero;
        mountain.transform.rotation = Quaternion.identity;
        mountain.transform.localScale = Vector3.one;

        // OBJ groups are written in the order Grass / LavenderRock / SnowCap.
        // Unity typically imports these as 3 submesh slots. The editor assigns all 3 explicitly.
        MeshRenderer[] meshRenderers = mountain.GetComponentsInChildren<MeshRenderer>(true);
        if (meshRenderers.Length == 0)
        {
            EditorUtility.DisplayDialog("Lina Mountain", "The imported mountain OBJ has no MeshRenderer. Check Model Import Settings.", "OK");
            Object.DestroyImmediate(mountain);
            return;
        }
        foreach (MeshRenderer renderer in meshRenderers)
        {
            if (renderer.sharedMaterials.Length >= 3)
            {
                renderer.sharedMaterials = new Material[] { grass, stone, snow };
            }
            else
            {
                // Some model-import configurations turn OBJ groups into children.
                // Detect those group names so grass and summit remain correctly colored.
                string groupName = renderer.gameObject.name.ToLowerInvariant();
                if (groupName.Contains("grass")) renderer.sharedMaterial = grass;
                else if (groupName.Contains("snow")) renderer.sharedMaterial = snow;
                else renderer.sharedMaterial = stone;
            }
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }

        // Make a prefab so newcomers can drag the complete mountain into another Scene.
        // Unpack the imported model first so the result is a regular editable Unity prefab.
        if (PrefabUtility.IsPartOfPrefabInstance(mountain))
            PrefabUtility.UnpackPrefabInstance(mountain, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        PrefabUtility.SaveAsPrefabAsset(mountain, DemoFolder + "PF_Lina_Mountain.prefab");

        if (cyan != null) CreateGem("Cyan crystal accent", new Vector3(-5.2f, 0.55f, -4.6f), new Vector3(.65f,1.45f,.65f), cyan);
        if (pink != null) CreateGem("Pink crystal accent", new Vector3(5.15f,0.5f,-3.8f), new Vector3(.48f,1.15f,.48f), pink);

        // Soft, slightly warm sunlight helps show stylized shade transitions.
        GameObject sunlightObject = new GameObject("Directional Light - Warm Sunlight");
        Light sunlight = sunlightObject.AddComponent<Light>();
        sunlight.type = LightType.Directional;
        sunlight.color = new Color(1.0f, 0.94f, 0.89f);
        sunlight.intensity = 1.3f;
        sunlight.shadows = LightShadows.Soft;
        sunlightObject.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.53f, 0.57f, 0.69f);
        RenderSettings.skybox = null;
        RenderSettings.fog = false;

        GameObject camObject = new GameObject("Main Camera");
        camObject.tag = "MainCamera";
        Camera camera = camObject.AddComponent<Camera>();
        camObject.AddComponent<AudioListener>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.44f, .56f, .72f, 1f);
        camera.allowHDR = true;
        camera.orthographic = true;
        camera.orthographicSize = 9.6f;
        camObject.transform.position = new Vector3(15.3f, 10.4f, 16.8f);
        camObject.transform.LookAt(new Vector3(0, 2.1f, 0));
        UniversalAdditionalCameraData cameraData = camObject.GetComponent<UniversalAdditionalCameraData>();
        if (cameraData == null) cameraData = camObject.AddComponent<UniversalAdditionalCameraData>();
        cameraData.renderPostProcessing = true;

        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(DemoFolder + "Lina_Mountain_Volume.asset");
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, DemoFolder + "Lina_Mountain_Volume.asset");
        }
        if (!profile.TryGet(out ColorAdjustments color)) color = profile.Add<ColorAdjustments>(true);
        color.saturation.Override(12f);
        color.contrast.Override(7f);
        if (!profile.TryGet(out Tonemapping tone)) tone = profile.Add<Tonemapping>(true);
        tone.mode.Override(TonemappingMode.Neutral);
        if (!profile.TryGet(out Bloom bloom)) bloom = profile.Add<Bloom>(true);
        bloom.threshold.Override(1.1f);
        bloom.intensity.Override(0.2f);
        bloom.scatter.Override(0.65f);
        foreach (VolumeComponent component in profile.components)
        {
            if (component != null && !AssetDatabase.Contains(component))
                AssetDatabase.AddObjectToAsset(component, profile);
        }
        EditorUtility.SetDirty(profile);
        GameObject volumeObject = new GameObject("Global Volume - Stylized Colors");
        Volume volume = volumeObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.sharedProfile = profile;

        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(scene, DemoFolder + "Lina_Mountain_Demo.unity");
        Selection.activeGameObject = mountain;
        EditorGUIUtility.PingObject(mountain);
        EditorUtility.DisplayDialog("Mountain Demo Created", "Done! Open the Game tab to see your mountain.\n\nPrefab: Assets/LinaStylePack/Demo/PF_Lina_Mountain.prefab\nScene: Assets/LinaStylePack/Demo/Lina_Mountain_Demo.unity\n\nClick a mountain mesh to inspect and change the custom shader materials.", "OK");
    }

    private static Material GetMaterial(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Material>(Root + "Materials/" + name + ".mat");
    }

    private static GameObject CreateGem(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject gem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        gem.name = name;
        gem.transform.position = position;
        gem.transform.localScale = scale;
        gem.GetComponent<MeshRenderer>().sharedMaterial = material;
        return gem;
    }
}
#endif

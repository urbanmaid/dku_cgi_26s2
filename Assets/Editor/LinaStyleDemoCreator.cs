#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// Beginner demo generator. No third-party dependencies besides URP.
public static class LinaStyleDemoCreator
{
    private const string Root = "Assets/LinaStylePack/";

    [MenuItem("Tools/Lina Style/1 - Create and Open Demo Scene")]
    public static void BuildDemoScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        // Start a separate scene: this does not overwrite the user's existing scenes.
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Material pink = Mat("MAT_01_Pink_Hair");
        Material violet = Mat("MAT_02_Violet_Outfit");
        Material peach = Mat("MAT_03_Skin_Peach");
        Material stone = Mat("MAT_04_Lavender_Stone");
        Material mint = Mat("MAT_05_Mint_Terrain");
        Material cyan = Mat("MAT_06_Cyan_Crystal");
        Material neon = Mat("MAT_07_Pink_Neon");
        Material sandstone = Mat("MAT_08_Warm_Sandstone");
        if (pink == null || violet == null || peach == null || stone == null || mint == null || cyan == null || neon == null || sandstone == null)
        {
            EditorUtility.DisplayDialog("Lina Style", "Materials are missing. Make sure the entire LinaStylePack folder is inside Assets.", "OK");
            return;
        }

        GameObject stage = new GameObject("Lina Style - Practice Display");
        Place(PrimitiveType.Cube, "Lavender stone platform", new Vector3(0,-.34f,0), new Vector3(10,.65f,6), stone,stage.transform);
        Place(PrimitiveType.Cube, "Mint grass floor", new Vector3(-3,0.01f,0), new Vector3(3,.12f,4.8f), mint,stage.transform);
        Place(PrimitiveType.Cylinder, "Left pedestal", new Vector3(-2,0.30f,0), new Vector3(1.45f,.3f,1.45f), sandstone,stage.transform);
        Place(PrimitiveType.Cylinder, "Middle pedestal", new Vector3(0,.30f,0), new Vector3(1.45f,.3f,1.45f), stone,stage.transform);
        Place(PrimitiveType.Cylinder, "Right pedestal", new Vector3(2,.30f,0), new Vector3(1.45f,.3f,1.45f), sandstone,stage.transform);

        // Simple stylized avatar: geometric display pieces, not copyrighted 3D models.
        Place(PrimitiveType.Capsule, "Violet outfit", new Vector3(-2,1.25f,0), new Vector3(0.68f,.65f,.60f),violet,stage.transform);
        Place(PrimitiveType.Sphere, "Peach face", new Vector3(-2,2.2f,0), new Vector3(0.95f,0.95f,0.95f),peach,stage.transform);
        Place(PrimitiveType.Sphere, "Pink hair top", new Vector3(-2,2.7f,0), new Vector3(1.03f,0.45f,1.0f),pink,stage.transform);
        Place(PrimitiveType.Sphere, "Cyan accessory", new Vector3(-1.55f,2.23f,-.39f), new Vector3(0.26f,0.26f,0.26f),cyan,stage.transform);
        Place(PrimitiveType.Cube, "Glowing pink belt", new Vector3(-2,1.25f,-.35f), new Vector3(0.72f,0.11f,0.15f),neon,stage.transform);

        Place(PrimitiveType.Sphere, "Pink hair sample", new Vector3(0,1.39f,0), new Vector3(1.18f,1.18f,1.18f),pink,stage.transform);
        Place(PrimitiveType.Sphere, "Cyan crystal sample", new Vector3(2,1.55f,0), new Vector3(0.85f,1.35f,0.85f),cyan,stage.transform);
        Place(PrimitiveType.Cube, "Neon stripe 1", new Vector3(2,.7f,-.55f),new Vector3(1.4f,0.11f,0.16f),neon,stage.transform);

        GameObject sun = new GameObject("Soft sunlight");
        Light sunlight = sun.AddComponent<Light>();
        sunlight.type = LightType.Directional;
        sunlight.color = new Color(1.0f,.94f,.88f);
        sunlight.intensity = 1.2f;
        sunlight.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(43,-33,0);

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera cam = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(.18f,.24f,.35f);
        cam.allowHDR = true;
        cameraObject.transform.position = new Vector3(0,3.3f,-9.8f);
        cameraObject.transform.LookAt(new Vector3(0,1.1f,0));
        UniversalAdditionalCameraData urpCamera = cameraObject.GetComponent<UniversalAdditionalCameraData>();
        if (urpCamera == null) urpCamera = cameraObject.AddComponent<UniversalAdditionalCameraData>();
        urpCamera.renderPostProcessing = true;

        string demoFolder = Root + "Demo";
        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(demoFolder + "/Lina_Demo_Volume.asset");
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, demoFolder + "/Lina_Demo_Volume.asset");
        }
        if (!profile.TryGet(out ColorAdjustments adj)) adj = profile.Add<ColorAdjustments>(true);
        adj.saturation.Override(14f);
        adj.contrast.Override(7f);
        adj.postExposure.Override(0f);
        if (!profile.TryGet(out Bloom bloom)) bloom = profile.Add<Bloom>(true);
        bloom.threshold.Override(1.1f);
        bloom.intensity.Override(.3f);
        bloom.scatter.Override(.65f);
        if (!profile.TryGet(out Tonemapping tone)) tone = profile.Add<Tonemapping>(true);
        tone.mode.Override(TonemappingMode.Neutral);
        // A newly generated VolumeProfile needs its components saved as subassets.
        foreach (VolumeComponent component in profile.components)
        {
            if (component != null && !AssetDatabase.Contains(component))
                AssetDatabase.AddObjectToAsset(component, profile);
        }
        EditorUtility.SetDirty(profile);

        GameObject volumeObject = new GameObject("Global Volume - colors and bloom");
        Volume volume = volumeObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.sharedProfile = profile;

        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(scene, demoFolder + "/LinaStyle_Demo.unity");
        Selection.activeObject = stage;
        EditorUtility.DisplayDialog("Lina Style Demo Ready", "Demo scene created. Switch to Game tab or press Play to see the sample materials. It is saved to Assets/LinaStylePack/Demo.", "OK");
    }

    private static Material Mat(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Material>(Root + "Materials/" + name + ".mat");
    }
    private static GameObject Place(PrimitiveType type,string name,Vector3 pos,Vector3 scale,Material mat,Transform parent)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        obj.transform.localScale = scale;
        MeshRenderer r = obj.GetComponent<MeshRenderer>();
        if (r != null) r.sharedMaterial = mat;
        return obj;
    }
}
#endif

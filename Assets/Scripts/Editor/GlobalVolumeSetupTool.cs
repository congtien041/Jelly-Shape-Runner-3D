using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Công cụ Editor tự động tạo và cấu hình Global Volume phát sáng neon rực rỡ,
/// gán sẵn vào cả 2 Scene (MainMenu và SampleScene), bật Post Processing cho Camera.
/// </summary>
public static class GlobalVolumeSetupTool
{
    private const string ProfilePath = "Assets/Settings/JellyRunner_GlobalVolumeProfile.asset";
    private const string MenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string GameScenePath = "Assets/Scenes/SampleScene.unity";

    [MenuItem("Jelly Runner/✨ Cấu Hình Global Volume Phát Sáng (Cả 2 Scene)", false, 3)]
    public static void SetupGlobalVolumeAllScenes()
    {
        Debug.Log("<color=#00E5FF><b>==== [Volume Setup] BẮT ĐẦU CẤU HÌNH GLOBAL VOLUME ====</b></color>");

        VolumeProfile profile = CreateOrGetNeonProfile();

        SetupVolumeInScene(MenuScenePath, profile);
        SetupVolumeInScene(GameScenePath, profile);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("<color=#00FF66><b>==== [Volume Setup] 🎉 HOÀN TẤT! Đồ họa Neon Bloom đã được thêm vào cả 2 Scene! ====</b></color>");
    }

    [MenuItem("Jelly Runner/✨ Cấu Hình Global Volume Cho Scene Đang Mở", false, 4)]
    public static void SetupGlobalVolumeInActiveScene()
    {
        VolumeProfile profile = CreateOrGetNeonProfile();
        SetupVolumeInCurrentScene(profile);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("<color=#00FF66><b>[Volume Setup] ✅ Đã cấu hình Global Volume thành công cho Scene đang mở!</b></color>");
    }

    public static VolumeProfile CreateOrGetNeonProfile()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Settings"))
        {
            AssetDatabase.CreateFolder("Assets", "Settings");
        }

        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "JellyRunner_GlobalVolumeProfile";
            AssetDatabase.CreateAsset(profile, ProfilePath);
        }

        // 1. Bloom
        if (!profile.TryGet(out Bloom bloom))
        {
            bloom = profile.Add<Bloom>(true);
        }
        bloom.threshold.Override(0.85f);
        bloom.intensity.Override(1.35f);
        bloom.scatter.Override(0.7f);
        bloom.highQualityFiltering.Override(true);

        // 2. Color Adjustments
        if (!profile.TryGet(out ColorAdjustments colorAdj))
        {
            colorAdj = profile.Add<ColorAdjustments>(true);
        }
        colorAdj.postExposure.Override(0.2f);
        colorAdj.contrast.Override(16f);
        colorAdj.saturation.Override(22f);

        // 3. Vignette
        if (!profile.TryGet(out Vignette vignette))
        {
            vignette = profile.Add<Vignette>(true);
        }
        vignette.intensity.Override(0.28f);
        vignette.smoothness.Override(0.45f);

        // 4. Chromatic Aberration
        if (!profile.TryGet(out ChromaticAberration chromatic))
        {
            chromatic = profile.Add<ChromaticAberration>(true);
        }
        chromatic.intensity.Override(0.12f);

        // 5. Tonemapping
        if (!profile.TryGet(out Tonemapping tonemapping))
        {
            tonemapping = profile.Add<Tonemapping>(true);
        }
        tonemapping.mode.Override(TonemappingMode.Neutral);

        EditorUtility.SetDirty(profile);
        return profile;
    }

    public static void SetupVolumeInScene(string scenePath, VolumeProfile profile)
    {
        if (!System.IO.File.Exists(scenePath)) return;

        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 1. Tìm hoặc tạo GameObject "Global Volume"
        GameObject volumeObj = GameObject.Find("Global Volume");
        if (volumeObj == null)
        {
            volumeObj = new GameObject("Global Volume");
        }

        Volume volume = volumeObj.GetComponent<Volume>();
        if (volume == null) volume = volumeObj.AddComponent<Volume>();

        volume.isGlobal = true;
        volume.priority = 1f;
        volume.weight = 1f;
        volume.sharedProfile = profile;

        GlobalVolumeManager mgr = volumeObj.GetComponent<GlobalVolumeManager>();
        if (mgr == null) mgr = volumeObj.AddComponent<GlobalVolumeManager>();

        // 2. Bật Post Processing cho Camera chính
        Camera cam = Camera.main;
        if (cam != null)
        {
            UniversalAdditionalCameraData camData = cam.GetUniversalAdditionalCameraData();
            if (camData != null)
            {
                camData.renderPostProcessing = true;
                EditorUtility.SetDirty(cam);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[Volume Setup] ✅ Đã cấu hình Global Volume thành công trong: {scenePath}");
    }

    public static void SetupVolumeInCurrentScene(VolumeProfile profile)
    {
        // 1. Tìm hoặc tạo GameObject "Global Volume"
        GameObject volumeObj = GameObject.Find("Global Volume");
        if (volumeObj == null)
        {
            volumeObj = new GameObject("Global Volume");
        }

        Volume volume = volumeObj.GetComponent<Volume>();
        if (volume == null) volume = volumeObj.AddComponent<Volume>();

        volume.isGlobal = true;
        volume.priority = 1f;
        volume.weight = 1f;
        volume.sharedProfile = profile;

        GlobalVolumeManager mgr = volumeObj.GetComponent<GlobalVolumeManager>();
        if (mgr == null) mgr = volumeObj.AddComponent<GlobalVolumeManager>();

        // 2. Bật Post Processing cho Camera chính
        Camera cam = Camera.main;
        if (cam != null)
        {
            UniversalAdditionalCameraData camData = cam.GetUniversalAdditionalCameraData();
            if (camData != null)
            {
                camData.renderPostProcessing = true;
                EditorUtility.SetDirty(cam);
            }
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }
}

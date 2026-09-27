using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Công cụ Editor quản lý AssetBundle và tự động tạo Material, Hiệu ứng (Trail, Particles) đẹp mắt cho Shop.
/// Cung cấp 2 Menu Item:
/// 1. Tạo toàn bộ tài nguyên Shop mẫu (Materials phát sáng, Trails, Particles).
/// 2. Đóng gói AssetBundles vào StreamingAssets.
/// </summary>
public static class ShopAssetBundleBuilder
{
    public const string BundleName = "shop_assets";
    public const string ShopMaterialsFolder = "Assets/Materials/Shop";
    public const string ShopEffectsFolder = "Assets/Prefabs/Effects";
    public const string StreamingBundlesFolder = "Assets/StreamingAssets/AssetBundles";

    [MenuItem("Jelly Runner/📦 1. Tạo Vật Phẩm Shop (Materials & Effects)", false, 10)]
    public static void GenerateShopAssets()
    {
        EnsureFolderExists("Assets/Materials");
        EnsureFolderExists(ShopMaterialsFolder);
        EnsureFolderExists("Assets/Prefabs");
        EnsureFolderExists(ShopEffectsFolder);

        Debug.Log("<color=#00E5FF><b>[Shop Builder] Đang tạo các Skin Material và Hiệu Ứng chất lượng cao...</b></color>");

        // 1. TẠO PLAYER SKINS (Materials)
        CreateMaterial("Mat_Player_CyberCyan", new Color(0.0f, 0.9f, 1.0f), new Color(0.0f, 0.4f, 0.5f), 0.3f, 0.95f);
        CreateMaterial("Mat_Player_GoldRoyale", new Color(1.0f, 0.85f, 0.15f), new Color(0.6f, 0.45f, 0.05f), 0.9f, 0.9f);
        CreateMaterial("Mat_Player_GalaxyPurple", new Color(0.65f, 0.15f, 1.0f), new Color(0.35f, 0.05f, 0.6f), 0.6f, 0.85f);
        CreateMaterial("Mat_Player_MagmaLava", new Color(1.0f, 0.28f, 0.05f), new Color(0.8f, 0.15f, 0.0f), 0.4f, 0.8f);
        CreateMaterial("Mat_Player_EmeraldJade", new Color(0.05f, 0.95f, 0.45f), new Color(0.02f, 0.5f, 0.2f), 0.5f, 0.95f);
        CreateMaterial("Mat_Player_VoidShadow", new Color(0.12f, 0.12f, 0.18f), new Color(0.3f, 0.1f, 0.5f), 0.7f, 0.9f);

        // 2. TẠO WALL SKINS (Materials)
        CreateMaterial("Mat_Wall_Crimson", new Color(1.0f, 0.32f, 0.35f), Color.black, 0.2f, 0.5f);
        CreateMaterial("Mat_Wall_Synthwave", new Color(0.95f, 0.2f, 0.75f), new Color(0.3f, 0.05f, 0.25f), 0.5f, 0.8f);
        CreateMaterial("Mat_Wall_GoldenPalace", new Color(0.98f, 0.8f, 0.2f), new Color(0.4f, 0.3f, 0.05f), 0.85f, 0.85f);
        CreateMaterial("Mat_Wall_ObsidianGlass", new Color(0.1f, 0.15f, 0.22f), new Color(0.05f, 0.1f, 0.2f), 0.6f, 0.95f);
        CreateMaterial("Mat_Wall_ToxicSlime", new Color(0.45f, 1.0f, 0.15f), new Color(0.2f, 0.5f, 0.05f), 0.4f, 0.85f);

        // 3. TẠO EFFECT PREFABS
        CreateRainbowTrailPrefab();
        CreateSparkleParticlesPrefab();
        CreateFireAuraPrefab();
        CreateElectricAuraPrefab();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("<color=#00FF66><b>[Shop Builder] ✅ Đã tạo toàn bộ Material & Effect Prefabs và gán nhãn AssetBundle 'shop_assets' thành công!</b></color>");
    }

    [MenuItem("Jelly Runner/📦 2. Build Shop AssetBundles", false, 11)]
    public static void BuildAllAssetBundles()
    {
        GenerateShopAssets();

        EnsureFolderExists("Assets/StreamingAssets");
        EnsureFolderExists(StreamingBundlesFolder);

        Debug.Log("<color=#00E5FF><b>[Shop Builder] Bắt đầu đóng gói AssetBundles vào: " + StreamingBundlesFolder + "...</b></color>");

        BuildPipeline.BuildAssetBundles(
            StreamingBundlesFolder,
            BuildAssetBundleOptions.None,
            EditorUserBuildSettings.activeBuildTarget
        );

        AssetDatabase.Refresh();
        Debug.Log("<color=#00FF66><b>[Shop Builder] 🎉 ĐÓNG GÓI ASSETBUNDLE THÀNH CÔNG! Sẵn sàng tải runtime.</b></color>");
    }

    private static void EnsureFolderExists(string folderPath)
    {
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            string parent = Path.GetDirectoryName(folderPath).Replace('\\', '/');
            string child = Path.GetFileName(folderPath);
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolderExists(parent);
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    private static Material CreateMaterial(string name, Color mainColor, Color emissionColor, float metallic, float smoothness)
    {
        string path = $"{ShopMaterialsFolder}/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

        if (mat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                         ?? Shader.Find("Standard");
            mat = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(mat, path);
        }

        mat.color = mainColor;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", mainColor);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

        if (emissionColor != Color.black)
        {
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emissionColor * 1.5f);
        }

        EditorUtility.SetDirty(mat);

        // Gán AssetBundle
        AssetImporter importer = AssetImporter.GetAtPath(path);
        if (importer != null)
        {
            importer.assetBundleName = BundleName;
        }

        return mat;
    }

    private static void CreateRainbowTrailPrefab()
    {
        string path = $"{ShopEffectsFolder}/FX_RainbowTrail.prefab";
        GameObject go = new GameObject("FX_RainbowTrail");
        TrailRenderer tr = go.AddComponent<TrailRenderer>();
        tr.time = 0.6f;
        tr.startWidth = 0.8f;
        tr.endWidth = 0.05f;
        tr.minVertexDistance = 0.1f;

        // Gradient cầu vồng neon
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0f, 0.9f, 1f), 0.0f),
                new GradientColorKey(new Color(1f, 0.3f, 0.9f), 0.5f),
                new GradientColorKey(new Color(1f, 0.85f, 0.1f), 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.85f, 0.0f),
                new GradientAlphaKey(0.4f, 0.7f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        tr.colorGradient = grad;

        Material trailMat = CreateOrGetDefaultParticleMat();
        if (trailMat != null) tr.sharedMaterial = trailMat;

        SavePrefabAndAssignBundle(go, path);
    }

    private static void CreateSparkleParticlesPrefab()
    {
        string path = $"{ShopEffectsFolder}/FX_GoldSparkles.prefab";
        GameObject go = new GameObject("FX_GoldSparkles");
        ParticleSystem ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime = 1.0f;
        main.startSpeed = 1.2f;
        main.startSize = 0.25f;
        main.startColor = new Color(1.0f, 0.9f, 0.2f, 0.9f);
        main.maxParticles = 80;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 25f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(1f, 1f, 0.2f);

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0f, 1f);
        curve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = CreateOrGetDefaultParticleMat();

        SavePrefabAndAssignBundle(go, path);
    }

    private static void CreateFireAuraPrefab()
    {
        string path = $"{ShopEffectsFolder}/FX_FireAura.prefab";
        GameObject go = new GameObject("FX_FireAura");
        ParticleSystem ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime = 0.8f;
        main.startSpeed = 1.8f;
        main.startSize = 0.4f;
        main.startColor = new Color(1.0f, 0.35f, 0.05f, 0.9f);
        main.maxParticles = 90;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 35f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15f;
        shape.radius = 0.4f;

        var colOverLifetime = ps.colorOverLifetime;
        colOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.8f, 0.1f), 0.0f),
                new GradientColorKey(new Color(1f, 0.1f, 0.0f), 0.7f),
                new GradientColorKey(new Color(0.2f, 0.2f, 0.2f), 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(0.6f, 0.6f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        colOverLifetime.color = grad;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = CreateOrGetDefaultParticleMat();

        SavePrefabAndAssignBundle(go, path);
    }

    private static void CreateElectricAuraPrefab()
    {
        string path = $"{ShopEffectsFolder}/FX_CosmicElectric.prefab";
        GameObject go = new GameObject("FX_CosmicElectric");
        ParticleSystem ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 2.5f;
        main.startSize = 0.2f;
        main.startColor = new Color(0.2f, 0.8f, 1.0f, 0.95f);
        main.maxParticles = 60;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = ps.emission;
        emission.rateOverTime = 20f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.8f;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = CreateOrGetDefaultParticleMat();

        SavePrefabAndAssignBundle(go, path);
    }

    private static Material CreateOrGetDefaultParticleMat()
    {
        string path = $"{ShopMaterialsFolder}/Mat_ParticleDefault.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader sh = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                     ?? Shader.Find("Particles/Standard Unlit")
                     ?? Shader.Find("Sprites/Default");
            mat = new Material(sh) { name = "Mat_ParticleDefault" };
            mat.color = Color.white;
            AssetDatabase.CreateAsset(mat, path);
        }

        AssetImporter importer = AssetImporter.GetAtPath(path);
        if (importer != null) importer.assetBundleName = BundleName;
        return mat;
    }

    private static void SavePrefabAndAssignBundle(GameObject go, string prefabPath)
    {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);

        AssetImporter importer = AssetImporter.GetAtPath(prefabPath);
        if (importer != null)
        {
            importer.assetBundleName = BundleName;
        }
    }
}

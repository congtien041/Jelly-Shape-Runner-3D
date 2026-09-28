using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Tự động thiết lập 100% toàn bộ Game: 2 Scene (MainMenu + SampleScene), 
/// Prefabs, Materials, UI, Camera, Player, RoadSpawner, WallSpawner, GameManager.
/// </summary>
[InitializeOnLoad]
public static class AutoGameSetup
{
    private const string MaterialsFolder = "Assets/Materials";
    private const string PrefabsFolder = "Assets/Prefabs";
    private const string ScenesFolder = "Assets/Scenes";
    private const string GameScenePath = "Assets/Scenes/SampleScene.unity";
    private const string MenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string SessionKey = "JellyRunner_SetupV3";

    static AutoGameSetup()
    {
        EditorApplication.delayCall += CheckAndAutoSetup;
    }

    private static void CheckAndAutoSetup()
    {
        if (!SessionState.GetBool(SessionKey, false))
        {
            SessionState.SetBool(SessionKey, true);
            if (Object.FindAnyObjectByType<JellyPlayer>() == null)
            {
                SetupCompleteGame();
            }
        }
    }

    [MenuItem("Jelly Runner/🚀 Tự Động Thiết Lập Toàn Bộ Game", false, 0)]
    public static void SetupCompleteGame()
    {
        Debug.Log("<color=#00D2FF><b>==== [Jelly Runner] BẮT ĐẦU THIẾT LẬP ====</b></color>");

        CleanupOldScripts();
        EnsureDirectories();

        Material matJelly = CreateOrGetMaterial("Mat_JellyPlayer", new Color(0.0f, 0.85f, 1.0f), 0.9f, new Color(0.0f, 0.45f, 0.7f));
        Material matTrack = CreateOrGetMaterial("Mat_Track", new Color(0.12f, 0.15f, 0.20f), 0.2f);
        Material matRail = CreateOrGetMaterial("Mat_Rail", new Color(1.0f, 0.8f, 0.2f), 0.5f, new Color(0.6f, 0.45f, 0.05f));
        Material matWall = CreateOrGetMaterial("Mat_Wall", new Color(1.0f, 0.35f, 0.35f), 0.3f, new Color(0.5f, 0.15f, 0.15f));

        GameObject wallPrefab = CreateWallPrefab(matWall);

        // Tạo toàn bộ vật phẩm Shop (Materials & Effects) và gán AssetBundle
        ShopAssetBundleBuilder.GenerateShopAssets();

        SetupMainMenuScene();
        SetupGameScene(matJelly, matTrack, matRail, wallPrefab);

        // Áp dụng 100% Giao diện Layer Lab GUI Pro cao cấp vào cả 2 Scene và xuất Prefab
        LayerLabAutoSetup.SetupAll();

        // Tối ưu hóa toàn diện Model Cute Magic (Fox & T-Rex): Scale x2, URP Lit, Animation chạy, offset bám sàn
        CubeAnimalsOptimizer.OptimizeAllCharacters();

        // Tự động thêm và cấu hình Global Volume phát sáng Neon rực rỡ vào cả 2 Scene
        GlobalVolumeSetupTool.SetupGlobalVolumeAllScenes();

        SetupBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("<color=#00FF66><b>==== [Jelly Runner] THIẾT LẬP HOÀN TẤT! BẤM PLAY! ====</b></color>");
    }

    private static void CleanupOldScripts()
    {
        // Xóa các file script cũ ở thư mục gốc Scripts/ (đã chuyển vào subfolder)
        string[] oldFiles = new string[]
        {
            "Assets/Scripts/JellyPlayer.cs",
            "Assets/Scripts/PlayerCollision.cs",
            "Assets/Scripts/Wall.cs",
            "Assets/Scripts/WallSpawner.cs",
            "Assets/Scripts/GameManager.cs",
            "Assets/Scripts/CameraFollow.cs",
            "Assets/Scripts/CameraController.cs",
        };

        foreach (string path in oldFiles)
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
                // Xóa .meta tương ứng
                string metaPath = path + ".meta";
                if (System.IO.File.Exists(metaPath))
                    System.IO.File.Delete(metaPath);
            }
        }
        AssetDatabase.Refresh();
    }

    private static void EnsureDirectories()
    {
        EnsureFolder("Assets", "Materials");
        EnsureFolder("Assets", "Prefabs");
        EnsureFolder("Assets", "Scenes");
    }

    private static void EnsureFolder(string parent, string child)
    {
        string full = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(full))
            AssetDatabase.CreateFolder(parent, child);
    }

    private static Material CreateOrGetMaterial(string name, Color color, float smoothness, Color? emission = null)
    {
        string path = $"{MaterialsFolder}/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

        if (mat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                         ?? Shader.Find("Standard");
            mat = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(mat, path);
        }

        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

        if (emission.HasValue && emission.Value != Color.black)
        {
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emission.Value * 1.5f);
        }

        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static GameObject CreateWallPrefab(Material matWall)
    {
        string prefabPath = $"{PrefabsFolder}/WallPrefab.prefab";

        GameObject wallObj = new GameObject("WallPrefab");
        Wall wall = wallObj.AddComponent<Wall>();

        GameObject leftBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftBlock.name = "LeftBlock";
        leftBlock.tag = "Obstacle";
        leftBlock.transform.SetParent(wallObj.transform);
        leftBlock.GetComponent<MeshRenderer>().sharedMaterial = matWall;

        GameObject rightBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightBlock.name = "RightBlock";
        rightBlock.tag = "Obstacle";
        rightBlock.transform.SetParent(wallObj.transform);
        rightBlock.GetComponent<MeshRenderer>().sharedMaterial = matWall;

        GameObject topBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        topBlock.name = "TopBlock";
        topBlock.tag = "Obstacle";
        topBlock.transform.SetParent(wallObj.transform);
        topBlock.GetComponent<MeshRenderer>().sharedMaterial = matWall;

        GameObject triggerObj = new GameObject("PassTrigger");
        triggerObj.tag = "ScoreZone";
        triggerObj.transform.SetParent(wallObj.transform);
        BoxCollider triggerCol = triggerObj.AddComponent<BoxCollider>();
        triggerCol.isTrigger = true;

        wall.ConfigureBlocks(topBlock.transform, leftBlock.transform, rightBlock.transform, triggerCol);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(wallObj, prefabPath);
        Object.DestroyImmediate(wallObj);
        return prefab;
    }

    // ========== MAIN MENU SCENE ==========
    private static void SetupMainMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Background Camera
        Camera cam = Camera.main;
        if (cam != null)
            cam.backgroundColor = new Color(0.05f, 0.07f, 0.12f);

        // Core Managers
        GameObject coreObj = new GameObject("CoreManagers");
        coreObj.AddComponent<LocalizationManager>();
        coreObj.AddComponent<AudioManager>();
        coreObj.AddComponent<CurrencyManager>();
        coreObj.AddComponent<LeaderboardManager>();
        coreObj.AddComponent<ShopManager>();

        // MainMenu Manager & ShopUI
        GameObject menuObj = new GameObject("MainMenuManager");
        menuObj.AddComponent<MainMenuUI>();
        menuObj.AddComponent<ShopUI>();

        // EventSystem (đảm bảo nút bấm hoạt động)
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        EditorSceneManager.SaveScene(scene, MenuScenePath);
    }

    // ========== GAME SCENE ==========
    private static void SetupGameScene(Material matJelly, Material matTrack, Material matRail, GameObject wallPrefab)
    {
        Scene scene;

        // Tạo SampleScene mới thay vì mở cái cũ (đảm bảo sạch sẽ)
        if (System.IO.File.Exists(GameScenePath))
        {
            scene = EditorSceneManager.OpenScene(GameScenePath);
            // Xóa hết object cũ
            foreach (GameObject obj in scene.GetRootGameObjects())
                Object.DestroyImmediate(obj);
        }
        else
        {
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        // 1. Directional Light
        GameObject lightObj = new GameObject("Directional Light");
        Light dirLight = lightObj.AddComponent<Light>();
        dirLight.type = LightType.Directional;
        dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        dirLight.color = new Color(1.0f, 0.96f, 0.90f);
        dirLight.intensity = 1.2f;

        // 2. Player
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.name = "JellyPlayer";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 0.5f, 0f);
        player.transform.localScale = Vector3.one;
        player.GetComponent<MeshRenderer>().sharedMaterial = matJelly;
        JellyPlayer jp = player.AddComponent<JellyPlayer>();
        player.AddComponent<PlayerCollision>();

        GameObject foxPf = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Model_Fox.prefab");
        GameObject trexPf = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Model_TRex.prefab");
        SerializedObject jpSo = new SerializedObject(jp);
        if (foxPf != null) jpSo.FindProperty("foxModelPrefab").objectReferenceValue = foxPf;
        if (trexPf != null) jpSo.FindProperty("trexModelPrefab").objectReferenceValue = trexPf;
        jpSo.ApplyModifiedProperties();

        // 3. Camera
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        camObj.AddComponent<AudioListener>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.15f, 0.17f, 0.25f);
        camObj.transform.position = new Vector3(0f, 4.5f, -7.5f);
        camObj.transform.rotation = Quaternion.Euler(20f, 0f, 0f);
        camObj.AddComponent<CameraFollow>();

        // 4. RoadSpawner (đường chạy vô tận lặp lại)
        GameObject roadObj = new GameObject("RoadSpawner");
        RoadSpawner road = roadObj.AddComponent<RoadSpawner>();
        road.Configure(player.transform);

        // Lan can 2 bên (dài 2000m)
        CreateRail("Rail_Left", new Vector3(-4.2f, 0.25f, 500f), matRail);
        CreateRail("Rail_Right", new Vector3(4.2f, 0.25f, 500f), matRail);

        // 5. WallSpawner
        GameObject spawnerObj = new GameObject("WallSpawner");
        WallSpawner spawner = spawnerObj.AddComponent<WallSpawner>();
        spawner.Configure(wallPrefab, player.transform);

        // 6. Core Managers & GameManager
        GameObject coreObj = new GameObject("CoreManagers");
        coreObj.AddComponent<LocalizationManager>();
        coreObj.AddComponent<AudioManager>();
        coreObj.AddComponent<CurrencyManager>();
        coreObj.AddComponent<LeaderboardManager>();
        coreObj.AddComponent<ShopManager>();

        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();

        // 7. UIManager (HUD + GameOver + Pause)
        GameObject uiObj = new GameObject("UIManager");
        uiObj.AddComponent<UIManager>();

        // 8. EventSystem
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, GameScenePath);
    }

    private static void CreateRail(string name, Vector3 pos, Material mat)
    {
        GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rail.name = name;
        rail.transform.position = pos;
        rail.transform.localScale = new Vector3(0.4f, 0.5f, 2000f);
        rail.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    // ========== BUILD SETTINGS ==========
    private static void SetupBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(MenuScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true),
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("[Jelly Runner] Build Settings cập nhật: MainMenu (0), SampleScene (1)");
    }
}

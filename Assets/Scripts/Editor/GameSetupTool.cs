using UnityEngine;
using UnityEditor;

/// <summary>
/// Công cụ tự động thiết lập Scene cho game.
/// Tạo menu "Jelly Runner" trên thanh công cụ của Unity.
/// </summary>
public class GameSetupTool
{
    [MenuItem("Jelly Runner/1. Auto Setup Game Scene (In-Game)")]
    public static void SetupGameScene()
    {
        // --- 1. Tạo nhóm Core Managers ---
        GameObject managersObj = GameObject.Find("CoreManagers");
        if (managersObj == null)
            managersObj = new GameObject("CoreManagers");

        GetOrAddComponent<GameManager>(managersObj);
        GetOrAddComponent<LocalizationManager>(managersObj);
        GetOrAddComponent<AudioManager>(managersObj);
        GetOrAddComponent<CurrencyManager>(managersObj);
        GetOrAddComponent<LeaderboardManager>(managersObj);
        GetOrAddComponent<ShopManager>(managersObj);

        // --- 2. Tạo UIManager ---
        GameObject uiManagerObj = GameObject.Find("UIManager");
        if (uiManagerObj == null)
            uiManagerObj = new GameObject("UIManager");
        
        GetOrAddComponent<UIManager>(uiManagerObj);

        // --- 3. Tạo Spawners ---
        GameObject spawnersObj = GameObject.Find("Environment_Spawners");
        if (spawnersObj == null)
            spawnersObj = new GameObject("Environment_Spawners");

        GetOrAddComponent<WallSpawner>(spawnersObj);
        // Nếu có RoadSpawner, bạn có thể thêm: GetOrAddComponent<RoadSpawner>(spawnersObj);

        // --- 4. Tạo Player (nếu chưa có) ---
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            playerObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            playerObj.name = "JellyPlayer";
            playerObj.tag = "Player";
            playerObj.transform.position = new Vector3(0, 0.5f, 0);
            
            // Đổi màu hồng cho dễ nhìn
            MeshRenderer mr = playerObj.GetComponent<MeshRenderer>();
            if (mr != null && mr.sharedMaterial != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(1f, 0.4f, 0.7f);
                mr.material = mat;
            }
        }
        
        GetOrAddComponent<JellyPlayer>(playerObj);
        GetOrAddComponent<PlayerCollision>(playerObj);

        // Sinh trực tiếp toàn bộ cây UI vào Scene để chỉnh sửa trong Editor
        SceneUIHierarchyGenerator.GenerateGameSceneUI();

        // Cấu hình Global Volume phát sáng neon cho Game Scene
        GlobalVolumeSetupTool.SetupGlobalVolumeInActiveScene();

        // Đánh dấu Scene đã thay đổi để có thể lưu
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("✅ [Jelly Runner] Đã thiết lập xong GAME SCENE! Hãy kiểm tra Hierarchy.");
    }

    [MenuItem("Jelly Runner/2. Auto Setup Main Menu Scene")]
    public static void SetupMainMenu()
    {
        // --- 1. Tạo nhóm Core Managers ---
        GameObject managersObj = GameObject.Find("CoreManagers");
        if (managersObj == null)
            managersObj = new GameObject("CoreManagers");

        GetOrAddComponent<LocalizationManager>(managersObj);
        GetOrAddComponent<AudioManager>(managersObj);
        GetOrAddComponent<LeaderboardManager>(managersObj);
        GetOrAddComponent<CurrencyManager>(managersObj);
        GetOrAddComponent<ShopManager>(managersObj);

        // --- 2. Tạo Main Menu UI & Shop UI ---
        GameObject menuUIObj = GameObject.Find("MainMenuUI");
        if (menuUIObj == null)
            menuUIObj = new GameObject("MainMenuUI");
        
        GetOrAddComponent<MainMenuUI>(menuUIObj);
        GetOrAddComponent<ShopUI>(menuUIObj);

        // Sinh trực tiếp toàn bộ cây UI vào Scene để chỉnh sửa trong Editor
        SceneUIHierarchyGenerator.GenerateMainMenuSceneUI();

        // Cấu hình Global Volume phát sáng neon cho Main Menu Scene
        GlobalVolumeSetupTool.SetupGlobalVolumeInActiveScene();

        // Đánh dấu Scene đã thay đổi
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("✅ [Jelly Runner] Đã thiết lập xong MAIN MENU SCENE! Hãy kiểm tra Hierarchy.");
    }

    /// <summary>
    /// Hàm tiện ích: Lấy component, nếu chưa có thì tự add.
    /// </summary>
    private static T GetOrAddComponent<T>(GameObject obj) where T : Component
    {
        T comp = obj.GetComponent<T>();
        if (comp == null)
        {
            comp = obj.AddComponent<T>();
        }
        return comp;
    }
}

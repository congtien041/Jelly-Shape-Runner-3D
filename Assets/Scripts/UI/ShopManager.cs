using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Quản lý Cửa Hàng (Shop System) hỗ trợ AssetBundle:
/// Hỗ trợ 3 danh mục:
/// 1. Skin Nhân Vật (Player Skins)
/// 2. Skin Tường Chướng Ngại Vật (Wall Skins)
/// 3. Hiệu Ứng & Kỹ Năng (Effects & Skills: Trails, Particles)
/// Nạp tự động từ AssetBundle trong StreamingAssets, tương thích 100% mọi phiên bản Unity (kể cả khi chưa bật AssetBundleModule).
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Jelly Runner/Shop Manager")]
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    public enum ItemType
    {
        PlayerSkin,
        WallSkin,
        Effect
    }

    [Serializable]
    public class ShopItemData
    {
        public string id;
        public string displayName;
        public ItemType type;
        public int price;
        public string assetName; // Tên asset trong bundle (ví dụ: Mat_Player_GoldRoyale)
        public Color previewColor = Color.cyan;
        public Material materialAsset;
        public GameObject effectPrefab;
        public JellyPlayer.CharacterType characterType = JellyPlayer.CharacterType.ClassicJelly;
        public bool isUnlocked;
    }

    [Header("--- AssetBundle Settings ---")]
    [SerializeField] private string bundleFileName = "shop_assets";
    private object loadedBundleObject = null; // object đại diện AssetBundle để an toàn tuyệt đối với mọi phiên bản Unity
    private bool isBundleLoading = false;

    [Header("--- Danh Sách Vật Phẩm ---")]
    [SerializeField] private List<ShopItemData> playerSkins = new List<ShopItemData>();
    [SerializeField] private List<ShopItemData> wallSkins = new List<ShopItemData>();
    [SerializeField] private List<ShopItemData> effectSkills = new List<ShopItemData>();

    public event Action OnShopDataUpdated;

    private const string PREF_EQUIPPED_PLAYER = "Shop_Equipped_PlayerSkin";
    private const string PREF_EQUIPPED_WALL = "Shop_Equipped_WallSkin";
    private const string PREF_EQUIPPED_EFFECT = "Shop_Equipped_Effect";
    private const string PREF_UNLOCKED_PREFIX = "Shop_Unlocked_";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeDefaultItems();
        LoadUnlockedAndEquippedState();
        StartCoroutine(LoadAssetBundleRoutine());
    }

    /// <summary>
    /// Khởi tạo danh sách vật phẩm ban đầu
    /// </summary>
    private void InitializeDefaultItems()
    {
        playerSkins.Clear();
        wallSkins.Clear();
        effectSkills.Clear();

        // 1. PLAYER SKINS & CHARACTERS (Có sẵn Nhân Vật 3D Fox & T-Rex)
        playerSkins.Add(new ShopItemData { id = "p_cyan", displayName = "Neon Cyan (Cube)", type = ItemType.PlayerSkin, price = 0, assetName = "Mat_Player_CyberCyan", previewColor = new Color(0f, 0.9f, 1f), isUnlocked = true, characterType = JellyPlayer.CharacterType.ClassicJelly });
        playerSkins.Add(new ShopItemData { id = "p_fox", displayName = "Cáo Voxel Cute", type = ItemType.PlayerSkin, price = 100, assetName = "", previewColor = new Color(1f, 0.55f, 0.1f), isUnlocked = true, characterType = JellyPlayer.CharacterType.Fox });
        playerSkins.Add(new ShopItemData { id = "p_trex", displayName = "Khủng Long T-Rex", type = ItemType.PlayerSkin, price = 200, assetName = "", previewColor = new Color(0.2f, 0.8f, 0.3f), characterType = JellyPlayer.CharacterType.TRex });
        playerSkins.Add(new ShopItemData { id = "p_gold", displayName = "Gold Royale", type = ItemType.PlayerSkin, price = 150, assetName = "Mat_Player_GoldRoyale", previewColor = new Color(1f, 0.85f, 0.15f), characterType = JellyPlayer.CharacterType.ClassicJelly });
        playerSkins.Add(new ShopItemData { id = "p_galaxy", displayName = "Galaxy Violet", type = ItemType.PlayerSkin, price = 250, assetName = "Mat_Player_GalaxyPurple", previewColor = new Color(0.65f, 0.15f, 1f), characterType = JellyPlayer.CharacterType.ClassicJelly });
        playerSkins.Add(new ShopItemData { id = "p_magma", displayName = "Magma Fire", type = ItemType.PlayerSkin, price = 350, assetName = "Mat_Player_MagmaLava", previewColor = new Color(1f, 0.3f, 0.05f), characterType = JellyPlayer.CharacterType.ClassicJelly });
        playerSkins.Add(new ShopItemData { id = "p_emerald", displayName = "Emerald Jade", type = ItemType.PlayerSkin, price = 500, assetName = "Mat_Player_EmeraldJade", previewColor = new Color(0.05f, 0.95f, 0.45f), characterType = JellyPlayer.CharacterType.ClassicJelly });
        playerSkins.Add(new ShopItemData { id = "p_void", displayName = "Void Shadow", type = ItemType.PlayerSkin, price = 750, assetName = "Mat_Player_VoidShadow", previewColor = new Color(0.2f, 0.15f, 0.35f), characterType = JellyPlayer.CharacterType.ClassicJelly });

        // 2. WALL SKINS
        wallSkins.Add(new ShopItemData { id = "w_crimson", displayName = "Classic Crimson", type = ItemType.WallSkin, price = 0, assetName = "Mat_Wall_Crimson", previewColor = new Color(1f, 0.32f, 0.35f), isUnlocked = true });
        wallSkins.Add(new ShopItemData { id = "w_synth", displayName = "Synthwave Neon", type = ItemType.WallSkin, price = 150, assetName = "Mat_Wall_Synthwave", previewColor = new Color(0.95f, 0.2f, 0.75f) });
        wallSkins.Add(new ShopItemData { id = "w_gold", displayName = "Golden Palace", type = ItemType.WallSkin, price = 250, assetName = "Mat_Wall_GoldenPalace", previewColor = new Color(0.98f, 0.8f, 0.2f) });
        wallSkins.Add(new ShopItemData { id = "w_obsidian", displayName = "Dark Obsidian", type = ItemType.WallSkin, price = 350, assetName = "Mat_Wall_ObsidianGlass", previewColor = new Color(0.1f, 0.15f, 0.22f) });
        wallSkins.Add(new ShopItemData { id = "w_toxic", displayName = "Toxic Acid", type = ItemType.WallSkin, price = 500, assetName = "Mat_Wall_ToxicSlime", previewColor = new Color(0.45f, 1f, 0.15f) });

        // 3. EFFECT SKILLS
        effectSkills.Add(new ShopItemData { id = "e_none", displayName = "Mặc Định", type = ItemType.Effect, price = 0, assetName = "", previewColor = Color.gray, isUnlocked = true });
        effectSkills.Add(new ShopItemData { id = "e_rainbow", displayName = "Rainbow Trail", type = ItemType.Effect, price = 200, assetName = "FX_RainbowTrail", previewColor = new Color(0f, 0.9f, 1f) });
        effectSkills.Add(new ShopItemData { id = "e_sparkle", displayName = "Gold Stars", type = ItemType.Effect, price = 300, assetName = "FX_GoldSparkles", previewColor = new Color(1f, 0.9f, 0.2f) });
        effectSkills.Add(new ShopItemData { id = "e_fire", displayName = "Blazing Fire", type = ItemType.Effect, price = 450, assetName = "FX_FireAura", previewColor = new Color(1f, 0.35f, 0.05f) });
        effectSkills.Add(new ShopItemData { id = "e_electric", displayName = "Cosmic Lightning", type = ItemType.Effect, price = 600, assetName = "FX_CosmicElectric", previewColor = new Color(0.2f, 0.8f, 1f) });
    }

    /// <summary>
    /// Nạp AssetBundle từ StreamingAssets (an toàn tuyệt đối cho mọi phiên bản Unity)
    /// </summary>
    private IEnumerator LoadAssetBundleRoutine()
    {
        yield return null; // đợi 1 frame

        string bundlePath = Path.Combine(Application.streamingAssetsPath, "AssetBundles", bundleFileName);

        if (File.Exists(bundlePath))
        {
            isBundleLoading = true;
            loadedBundleObject = AssetBundleSafeBridge.LoadFromFile(bundlePath);
            isBundleLoading = false;

            if (loadedBundleObject != null)
            {
                Debug.Log($"<color=#00FF66>[ShopManager] Đã tải thành công AssetBundle: {bundleFileName}</color>");
                BindAssetsFromBundle(loadedBundleObject);
            }
            else
            {
                Debug.LogWarning("[ShopManager] Không thể giải mã AssetBundle. Sử dụng chế độ Fallback.");
                FallbackLoadAssets();
            }
        }
        else
        {
            // Nếu chưa có file bundle -> Fallback thông minh
            FallbackLoadAssets();
        }

        OnShopDataUpdated?.Invoke();
    }

    private void BindAssetsFromBundle(object bundle)
    {
        foreach (var item in playerSkins)
        {
            if (!string.IsNullOrEmpty(item.assetName))
                item.materialAsset = AssetBundleSafeBridge.LoadAsset<Material>(bundle, item.assetName);
        }

        foreach (var item in wallSkins)
        {
            if (!string.IsNullOrEmpty(item.assetName))
                item.materialAsset = AssetBundleSafeBridge.LoadAsset<Material>(bundle, item.assetName);
        }

        foreach (var item in effectSkills)
        {
            if (!string.IsNullOrEmpty(item.assetName))
                item.effectPrefab = AssetBundleSafeBridge.LoadAsset<GameObject>(bundle, item.assetName);
        }
    }

    private void FallbackLoadAssets()
    {
#if UNITY_EDITOR
        // Trong Unity Editor, nạp trực tiếp qua AssetDatabase để không bao giờ bị gián đoạn phát triển
        foreach (var item in playerSkins)
        {
            if (item.materialAsset == null && !string.IsNullOrEmpty(item.assetName))
                item.materialAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>($"Assets/Materials/Shop/{item.assetName}.mat");
        }
        foreach (var item in wallSkins)
        {
            if (item.materialAsset == null && !string.IsNullOrEmpty(item.assetName))
                item.materialAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>($"Assets/Materials/Shop/{item.assetName}.mat");
        }
        foreach (var item in effectSkills)
        {
            if (item.effectPrefab == null && !string.IsNullOrEmpty(item.assetName))
                item.effectPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Effects/{item.assetName}.prefab");
        }
#endif

        // Runtime fallback (tạo dynamic material nếu cần)
        Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                     ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                     ?? Shader.Find("Standard");

        foreach (var item in playerSkins)
        {
            if (item.materialAsset == null && shader != null)
            {
                Material m = new Material(shader) { name = item.assetName };
                m.color = item.previewColor;
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", item.previewColor);
                item.materialAsset = m;
            }
        }

        foreach (var item in wallSkins)
        {
            if (item.materialAsset == null && shader != null)
            {
                Material m = new Material(shader) { name = item.assetName };
                m.color = item.previewColor;
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", item.previewColor);
                item.materialAsset = m;
            }
        }
    }

    // === QUẢN LÝ MUA & TRANG BỊ ===

    public List<ShopItemData> GetItems(ItemType type)
    {
        switch (type)
        {
            case ItemType.PlayerSkin: return playerSkins;
            case ItemType.WallSkin: return wallSkins;
            case ItemType.Effect: return effectSkills;
            default: return playerSkins;
        }
    }

    public int GetEquippedIndex(ItemType type)
    {
        switch (type)
        {
            case ItemType.PlayerSkin: return PlayerPrefs.GetInt(PREF_EQUIPPED_PLAYER, 0);
            case ItemType.WallSkin: return PlayerPrefs.GetInt(PREF_EQUIPPED_WALL, 0);
            case ItemType.Effect: return PlayerPrefs.GetInt(PREF_EQUIPPED_EFFECT, 0);
            default: return 0;
        }
    }

    public bool BuyItem(ItemType type, int index)
    {
        List<ShopItemData> list = GetItems(type);
        if (index < 0 || index >= list.Count) return false;

        ShopItemData item = list[index];
        if (item.isUnlocked)
        {
            EquipItem(type, index);
            return true;
        }

        // Kiểm tra và trừ tiền (vàng)
        if (item.price > 0)
        {
            if (CurrencyManager.Instance != null)
            {
                if (!CurrencyManager.Instance.SpendCoins(item.price))
                {
                    Debug.LogWarning($"[ShopManager] Không đủ vàng để mua {item.displayName}! Cần: {item.price}, Hiện có: {CurrencyManager.Instance.TotalCoins}");
                    return false;
                }
            }
            else
            {
                int currentCoins = PlayerPrefs.GetInt("TotalCoins", 0);
                if (currentCoins < item.price)
                {
                    Debug.LogWarning($"[ShopManager] Không đủ vàng (PlayerPrefs) để mua {item.displayName}! Cần: {item.price}, Hiện có: {currentCoins}");
                    return false;
                }
                currentCoins -= item.price;
                PlayerPrefs.SetInt("TotalCoins", currentCoins);
                PlayerPrefs.Save();
            }
        }

        // Mở khóa & Trang bị
        item.isUnlocked = true;
        PlayerPrefs.SetInt(PREF_UNLOCKED_PREFIX + item.id, 1);
        PlayerPrefs.Save();
        EquipItem(type, index);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCoinCollectSound();

        OnShopDataUpdated?.Invoke();
        return true;
    }

    public void EquipItem(ItemType type, int index)
    {
        List<ShopItemData> list = GetItems(type);
        if (index < 0 || index >= list.Count) return;
        if (!list[index].isUnlocked) return;

        switch (type)
        {
            case ItemType.PlayerSkin:
                PlayerPrefs.SetInt(PREF_EQUIPPED_PLAYER, index);
                break;
            case ItemType.WallSkin:
                PlayerPrefs.SetInt(PREF_EQUIPPED_WALL, index);
                break;
            case ItemType.Effect:
                PlayerPrefs.SetInt(PREF_EQUIPPED_EFFECT, index);
                break;
        }
        PlayerPrefs.Save();
        OnShopDataUpdated?.Invoke();

        // Áp dụng ngay nếu đang trong game
        JellyPlayer player = FindAnyObjectByType<JellyPlayer>();
        if (player != null) ApplyEquippedItemsToPlayer(player);

        WallSpawner spawner = FindAnyObjectByType<WallSpawner>();
        if (spawner != null) spawner.SetWallMaterial(GetEquippedWallMaterial());
    }

    public Material GetEquippedPlayerMaterial()
    {
        int idx = GetEquippedIndex(ItemType.PlayerSkin);
        if (idx >= 0 && idx < playerSkins.Count)
            return playerSkins[idx].materialAsset;
        return null;
    }

    public Material GetEquippedWallMaterial()
    {
        int idx = GetEquippedIndex(ItemType.WallSkin);
        if (idx >= 0 && idx < wallSkins.Count)
            return wallSkins[idx].materialAsset;
        return null;
    }

    public GameObject GetEquippedEffectPrefab()
    {
        int idx = GetEquippedIndex(ItemType.Effect);
        if (idx >= 0 && idx < effectSkills.Count)
            return effectSkills[idx].effectPrefab;
        return null;
    }

    public void ApplyEquippedItemsToPlayer(JellyPlayer player)
    {
        if (player == null) return;

        int playerIdx = GetEquippedIndex(ItemType.PlayerSkin);
        if (playerIdx >= 0 && playerIdx < playerSkins.Count)
        {
            var item = playerSkins[playerIdx];
            player.SetCharacter(item.characterType);

            if (item.characterType == JellyPlayer.CharacterType.ClassicJelly && item.materialAsset != null)
            {
                player.ApplySkin(item.materialAsset);
            }
        }

        GameObject effectPrefab = GetEquippedEffectPrefab();
        player.ApplyEffect(effectPrefab);
    }

    private void LoadUnlockedAndEquippedState()
    {
        // Mở khóa item mặc định đầu tiên
        if (playerSkins.Count > 0) playerSkins[0].isUnlocked = true;
        if (wallSkins.Count > 0) wallSkins[0].isUnlocked = true;
        if (effectSkills.Count > 0) effectSkills[0].isUnlocked = true;

        foreach (var item in playerSkins)
            if (PlayerPrefs.GetInt(PREF_UNLOCKED_PREFIX + item.id, 0) == 1) item.isUnlocked = true;

        foreach (var item in wallSkins)
            if (PlayerPrefs.GetInt(PREF_UNLOCKED_PREFIX + item.id, 0) == 1) item.isUnlocked = true;

        foreach (var item in effectSkills)
            if (PlayerPrefs.GetInt(PREF_UNLOCKED_PREFIX + item.id, 0) == 1) item.isUnlocked = true;
    }

    private void OnDestroy()
    {
        if (loadedBundleObject != null)
        {
            AssetBundleSafeBridge.Unload(loadedBundleObject, false);
            loadedBundleObject = null;
        }
    }
}

/// <summary>
/// Cầu nối an toàn nạp AssetBundle:
/// Hoạt động trơn tru bất kể project có bật package AssetBundle hay chưa.
/// </summary>
public static class AssetBundleSafeBridge
{
    private static Type s_AssetBundleType;
    private static bool s_Checked = false;

    public static Type AssetBundleType
    {
        get
        {
            if (!s_Checked)
            {
                s_AssetBundleType = Type.GetType("UnityEngine.AssetBundle, UnityEngine.AssetBundleModule")
                                 ?? Type.GetType("UnityEngine.AssetBundle, UnityEngine");
                s_Checked = true;
            }
            return s_AssetBundleType;
        }
    }

    public static object LoadFromFile(string path)
    {
        if (AssetBundleType == null || !File.Exists(path)) return null;
        try
        {
            MethodInfo m = AssetBundleType.GetMethod("LoadFromFile", new Type[] { typeof(string) });
            return m != null ? m.Invoke(null, new object[] { path }) : null;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[AssetBundleSafeBridge] LoadFromFile exception: {ex.Message}");
            return null;
        }
    }

    public static T LoadAsset<T>(object bundle, string assetName) where T : UnityEngine.Object
    {
        if (bundle == null || AssetBundleType == null || string.IsNullOrEmpty(assetName)) return null;
        try
        {
            MethodInfo m = AssetBundleType.GetMethod("LoadAsset", new Type[] { typeof(string), typeof(Type) });
            return m != null ? m.Invoke(bundle, new object[] { assetName, typeof(T) }) as T : null;
        }
        catch
        {
            return null;
        }
    }

    public static void Unload(object bundle, bool unloadAll)
    {
        if (bundle == null || AssetBundleType == null) return;
        try
        {
            MethodInfo m = AssetBundleType.GetMethod("Unload", new Type[] { typeof(bool) });
            m?.Invoke(bundle, new object[] { unloadAll });
        }
        catch
        {
        }
    }
}

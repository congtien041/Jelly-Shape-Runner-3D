using UnityEngine;
using UnityEditor;

/// <summary>
/// CÔNG CỤ QUẢN LÝ VÀ CHỈNH SỬA VÀNG (COINS TOOL)
/// Cung cấp giao diện trực quan trong Unity Editor và các Menu Item nhanh
/// để thêm, bớt, đặt số vàng bất kỳ phục vụ việc test game và mua sắm Shop.
/// Tự động cập nhật thời gian thực cả khi đang chơi (Play Mode) lẫn khi chỉnh sửa (Edit Mode).
/// </summary>
public class CoinsCheatTool : EditorWindow
{
    private const string COINS_KEY = "TotalCoins";
    private int customAmount = 10000;
    private Vector2 scrollPos;

    [MenuItem("Jelly Runner/🪙 Công Cụ Chỉnh Vàng (Coins Tool)...", false, 50)]
    [MenuItem("Tools/Jelly Runner/🪙 Chỉnh Vàng (Coins Tool)...", false, 50)]
    public static void ShowWindow()
    {
        CoinsCheatTool window = GetWindow<CoinsCheatTool>("🪙 Quản Lý Vàng");
        window.minSize = new Vector2(340, 420);
        window.Show();
    }

    // =========================================================================
    // MENU ITEMS TRỰC TIẾP TRÊN THANH CÔNG CỤ (ONE-CLICK SHORTCUTS)
    // =========================================================================
    [MenuItem("Jelly Runner/🪙 Nhanh: ➕ Thêm 1,000 Vàng", false, 51)]
    public static void QuickAdd1000()
    {
        AddCoins(1000);
    }

    [MenuItem("Jelly Runner/🪙 Nhanh: ➕ Thêm 10,000 Vàng", false, 52)]
    public static void QuickAdd10000()
    {
        AddCoins(10000);
    }

    [MenuItem("Jelly Runner/🪙 Nhanh: 👑 Đặt 999,999 Vàng (Đại Gia)", false, 53)]
    public static void QuickSetRich()
    {
        SetCoinsDirectly(999999);
    }

    [MenuItem("Jelly Runner/🪙 Nhanh: 🔄 Reset Vàng Về 0", false, 54)]
    public static void QuickResetZero()
    {
        SetCoinsDirectly(0);
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        // Header
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(1f, 0.85f, 0.2f) }
        };

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("🪙 CÔNG CỤ CHỈNH VÀNG (COINS TOOL)", titleStyle);
        EditorGUILayout.Space(6);

        int currentCoins = GetCurrentCoins();

        // Khung hiển thị vàng hiện tại
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUIStyle statLabelStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter
        };
        GUIStyle coinValueStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 22,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(1f, 0.8f, 0f) }
        };

        string statusText = Application.isPlaying ? "🎮 ĐANG CHƠI (Play Mode)" : "🛠️ EDIT MODE";
        EditorGUILayout.LabelField(statusText, statLabelStyle);
        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField($"{currentCoins:N0} VÀNG", coinValueStyle);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // Phần đặt số vàng tùy ý
        EditorGUILayout.LabelField("✏️ Đặt Số Vàng Tùy Ý:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        customAmount = EditorGUILayout.IntField(customAmount, GUILayout.Height(24));
        if (customAmount < 0) customAmount = 0;

        GUI.backgroundColor = new Color(0.2f, 0.85f, 0.4f);
        if (GUILayout.Button("Áp Dụng", GUILayout.Width(90), GUILayout.Height(24)))
        {
            SetCoinsDirectly(customAmount);
            ShowNotification(new GUIContent($"Đã đặt: {customAmount:N0} vàng"));
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(12);

        // Các nút bấm cộng nhanh
        EditorGUILayout.LabelField("➕ Cộng Nhanh Vàng:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+500", GUILayout.Height(28))) AddCoins(500);
        if (GUILayout.Button("+1,000", GUILayout.Height(28))) AddCoins(1000);
        if (GUILayout.Button("+5,000", GUILayout.Height(28))) AddCoins(5000);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+10,000", GUILayout.Height(28))) AddCoins(10000);
        if (GUILayout.Button("+50,000", GUILayout.Height(28))) AddCoins(50000);
        if (GUILayout.Button("+100,000", GUILayout.Height(28))) AddCoins(100000);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);

        // Các nút bấm trừ bớt
        EditorGUILayout.LabelField("➖ Trừ Bớt Vàng:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("-500", GUILayout.Height(26))) AddCoins(-500);
        if (GUILayout.Button("-1,000", GUILayout.Height(26))) AddCoins(-1000);
        if (GUILayout.Button("-5,000", GUILayout.Height(26))) AddCoins(-5000);
        if (GUILayout.Button("-10,000", GUILayout.Height(26))) AddCoins(-10000);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(12);

        // Nút bấm đặt nhanh đặc biệt
        EditorGUILayout.LabelField("⚡ Phím Tắt Tiện Ích:", EditorStyles.boldLabel);
        GUI.backgroundColor = new Color(1f, 0.75f, 0.1f);
        if (GUILayout.Button("👑 Đặt 999,999 Vàng (Đại Gia Mua Hết Shop)", GUILayout.Height(32)))
        {
            SetCoinsDirectly(999999);
            ShowNotification(new GUIContent("👑 Đã đặt 999,999 Vàng!"));
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(4);

        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("🔄 Đặt Về 0 Vàng (Reset Hoàn Toàn)", GUILayout.Height(28)))
        {
            if (EditorUtility.DisplayDialog("Xác nhận", "Bạn có chắc chắn muốn reset số vàng về 0 không?", "Đồng ý", "Hủy"))
            {
                SetCoinsDirectly(0);
                ShowNotification(new GUIContent("Đã reset về 0 Vàng!"));
            }
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(10);

        // Debug nhanh thời gian chơi để test Hướng Dẫn Viền Xanh (1 phút) và Lỗ 5 Tầng (2 phút)
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("⏱️ Test Thời Gian Chơi & Tính Năng:", EditorStyles.boldLabel);
            WallSpawner spawner = FindAnyObjectByType<WallSpawner>();
            if (spawner != null)
            {
                float duration = spawner.GameplayDuration;
                bool isTutorialActive = duration < 60f;
                bool isFiveTierUnlocked = duration >= 120f;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"⏱️ Thời gian chơi: <b>{duration:F1}s</b>", new GUIStyle(EditorStyles.label) { richText = true });
                
                string tutorialStatus = isTutorialActive 
                    ? "<color=#00FF66><b>ĐANG BẬT</b></color> (Đổi xanh lá khi vuốt đúng)" 
                    : "<color=#FFAA00><b>ĐÃ TẮT</b></color> (Giữ màu theme, tự phán đoán)";
                EditorGUILayout.LabelField($"💡 Hướng dẫn 1 phút: {tutorialStatus}", new GUIStyle(EditorStyles.label) { richText = true });

                string tier5Status = isFiveTierUnlocked 
                    ? "<color=#00FF66><b>ĐÃ MỞ KHÓA</b></color> (Sinh lỗ 5 tầng)" 
                    : "<color=#AAAAAA><b>ĐANG KHÓA</b></color> (Chỉ mở sau 120s)";
                EditorGUILayout.LabelField($"🧱 Lỗ 5 Tầng (Chia 5 ô): {tier5Status}", new GUIStyle(EditorStyles.label) { richText = true });
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = isTutorialActive ? new Color(1f, 0.6f, 0.2f) : new Color(0.7f, 0.7f, 0.7f);
                if (GUILayout.Button("⏩ Nhảy Tới 65s\n(Tắt Viền Xanh)", GUILayout.Height(36)))
                {
                    spawner.SetGameplayDuration(65f);
                    ShowNotification(new GUIContent("⏩ Đã chuyển sang 65s (Tắt hướng dẫn viền xanh)!"));
                }

                GUI.backgroundColor = !isFiveTierUnlocked ? new Color(0.3f, 0.8f, 1f) : new Color(0.7f, 0.7f, 0.7f);
                if (GUILayout.Button("⏩ Nhảy Tới 125s\n(Mở Lỗ 5 Tầng)", GUILayout.Height(36)))
                {
                    spawner.SetGameplayDuration(125f);
                    ShowNotification(new GUIContent("⏩ Đã mở khóa Lỗ 5 Tầng (125s)!"));
                }

                GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                if (GUILayout.Button("⏪ Reset Về 0s\n(Bật Lại Hướng Dẫn)", GUILayout.Width(130), GUILayout.Height(36)))
                {
                    spawner.SetGameplayDuration(0f);
                    ShowNotification(new GUIContent("⏪ Đã reset thời gian về 0s (Hướng dẫn hiện lại)!"));
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space(6);
        }

        EditorGUILayout.HelpBox("💡 Mẹo: Khi đang trong Play Mode, việc chỉnh vàng sẽ lập tức cập nhật lên UI góc màn hình và Shop mà không cần load lại game!", MessageType.Info);

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// Lấy số vàng hiện tại (ưu tiên CurrencyManager lúc runtime, fallback về PlayerPrefs).
    /// </summary>
    public static int GetCurrentCoins()
    {
        if (Application.isPlaying && CurrencyManager.Instance != null)
        {
            return CurrencyManager.Instance.TotalCoins;
        }
        return PlayerPrefs.GetInt(COINS_KEY, 0);
    }

    /// <summary>
    /// Thêm hoặc bớt số vàng chỉ định.
    /// </summary>
    public static void AddCoins(int delta)
    {
        int current = GetCurrentCoins();
        int newAmount = Mathf.Max(0, current + delta);
        SetCoinsDirectly(newAmount);
    }

    /// <summary>
    /// Đặt trực tiếp số vàng và đồng bộ cả PlayerPrefs lẫn CurrencyManager trong Play Mode.
    /// </summary>
    public static void SetCoinsDirectly(int amount)
    {
        int clamped = Mathf.Max(0, amount);
        PlayerPrefs.SetInt(COINS_KEY, clamped);
        PlayerPrefs.Save();

        if (Application.isPlaying && CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.SetCoins(clamped);
        }

        Debug.Log($"<color=#FFD700><b>[CoinsTool]</b></color> Đã cập nhật số vàng thành: <b>{clamped:N0}</b> coins.");
    }
}

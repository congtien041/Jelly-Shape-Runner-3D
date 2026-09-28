using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Công cụ Editor tạo toàn bộ cây UI thật 100% vào Scene (Hierarchy & Scene View).
/// Cho phép lập trình viên / thiết kế có thể nhìn thấy, click chọn, kéo thả và chỉnh sửa UI trực quan
/// ngay trong Unity Editor mà không cần phải bấm Play!
/// 
/// ★ PHIÊN BẢN NÂNG CẤP — Giao diện chuyên nghiệp hơn:
///   - Bo góc mềm mại (sprite 9-slice khi có sẵn hoặc color mềm)
///   - Gradient-like backgrounds qua overlay layers
///   - Consistent spacing và sizing system
///   - Decorative separator lines
///   - Modern color palette (dark navy theme)
/// </summary>
public static class SceneUIHierarchyGenerator
{
    private const string MenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string GameScenePath = "Assets/Scenes/SampleScene.unity";

    // ── Design Token Colors ──
    private static readonly Color COL_BG_DARK = new Color(0.04f, 0.05f, 0.10f, 1f);
    private static readonly Color COL_BG_CARD = new Color(0.07f, 0.09f, 0.16f, 0.96f);
    private static readonly Color COL_BG_CARD_INNER = new Color(0.09f, 0.12f, 0.20f, 0.94f);
    private static readonly Color COL_ACCENT_CYAN = new Color(0f, 0.88f, 1f);
    private static readonly Color COL_ACCENT_GOLD = new Color(1f, 0.85f, 0.15f);
    private static readonly Color COL_ACCENT_GREEN = new Color(0.18f, 0.88f, 0.52f);
    private static readonly Color COL_ACCENT_ORANGE = new Color(0.98f, 0.55f, 0.15f);
    private static readonly Color COL_ACCENT_RED = new Color(0.92f, 0.28f, 0.30f);
    private static readonly Color COL_ACCENT_PURPLE = new Color(0.58f, 0.35f, 0.90f);
    private static readonly Color COL_TEXT_SUB = new Color(0.60f, 0.70f, 0.85f);
    private static readonly Color COL_BTN_INACTIVE = new Color(0.15f, 0.18f, 0.28f);
    private static readonly Color COL_SEPARATOR = new Color(0.20f, 0.25f, 0.40f, 0.5f);
    private static readonly Color COL_INPUT_BG = new Color(0.10f, 0.13f, 0.22f, 1f);
    private static readonly Color COL_SLIDER_BG = new Color(0.14f, 0.18f, 0.28f);

    [MenuItem("Jelly Runner/🎨 Tạo Toàn Bộ UI Trực Quan Vào Scene (Để chỉnh sửa)", false, 2)]
    public static void GenerateAllSceneUIs()
    {
        Debug.Log("<color=#00E5FF><b>==== [UI Generator] BẮT ĐẦU TẠO CÂY UI LAYER LAB GUI PRO CAO CẤP ====</b></color>");

        // Sử dụng công cụ Layer Lab Auto Setup để áp dụng bộ GUI Pro cao cấp
        LayerLabAutoSetup.SetupAll();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("<color=#00FF66><b>==== [UI Generator] 🎉 HOÀN TẤT! Giao diện Layer Lab GUI Pro đã hiện 100% trong Hierarchy và Scene View! ====</b></color>");
    }

    // =========================================================================
    // 1. XÂY DỰNG UI CHO SCENE MAIN MENU
    // =========================================================================

    public static void GenerateMainMenuSceneUI()
    {
        Scene scene = EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);

        // Đảm bảo có EventSystem
        EnsureEventSystem();

        // Tìm hoặc tạo Canvas
        GameObject canvasObj = GameObject.Find("MenuCanvas");
        if (canvasObj != null)
            Object.DestroyImmediate(canvasObj);

        canvasObj = new GameObject("MenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // 1. Background — Deep dark gradient-like
        CreatePanel(canvasObj.transform, "Background", COL_BG_DARK);

        // 2. Main Menu Panel
        GameObject menuPanel = CreatePanel(canvasObj.transform, "MainMenuPanel", Color.clear);

        // ── Title Area ──
        CreateText(menuPanel.transform, "TitleText", "JELLY SHAPE\nRUNNER 3D",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -240), new Vector2(800, 180), 70, COL_ACCENT_CYAN, TextAlignmentOptions.Center);

        // Decorative line under title
        CreateSeparator(menuPanel.transform, "TitleSep", new Vector2(0, -370), 550);

        // Best Score — Badge style
        GameObject bestBadge = CreateRoundedPanel(menuPanel.transform, "BestScoreBadge",
            COL_BG_CARD, new Vector2(0.5f, 1f), new Vector2(0, -415), new Vector2(500, 60));
        CreateText(menuPanel.transform, "BestScoreText", "🏆 KỶ LỤC: 0 ĐIỂM",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -415), new Vector2(480, 55), 30, COL_ACCENT_GOLD, TextAlignmentOptions.Center);

        // ── Buttons Group — Centered, uniform spacing ──
        GameObject btnsGroup = CreatePanel(menuPanel.transform, "ButtonsGroup", Color.clear);

        CreateStyledButton(btnsGroup.transform, "PlayButton", "▶  CHƠI NGAY",
            new Vector2(0, 100), new Vector2(440, 115), 44, COL_ACCENT_GREEN);

        CreateStyledButton(btnsGroup.transform, "ShopButton", "🛍  CỬA HÀNG",
            new Vector2(0, -30), new Vector2(400, 100), 36, COL_ACCENT_ORANGE);

        CreateStyledButton(btnsGroup.transform, "LeaderboardButton", "🏆  XẾP HẠNG",
            new Vector2(0, -145), new Vector2(400, 95), 34, new Color(0.25f, 0.55f, 0.90f));

        CreateStyledButton(btnsGroup.transform, "SettingsButton", "⚙  CÀI ĐẶT",
            new Vector2(0, -255), new Vector2(380, 90), 32, COL_ACCENT_PURPLE);

        CreateStyledButton(btnsGroup.transform, "QuitButton", "THOÁT",
            new Vector2(0, -365), new Vector2(300, 78), 28, COL_ACCENT_RED);

        // 3. Profile Card Top-Right
        BuildProfileWidget(canvasObj.transform);

        // 4. Onboarding Panel
        BuildOnboardingPanel(canvasObj.transform);

        // 5. Settings Panel
        BuildSettingsPanel(canvasObj.transform);

        // 6. Leaderboard Panel
        BuildLeaderboardPanel(canvasObj.transform);

        // 7. Shop Modal Panel
        BuildShopModalPanel(canvasObj.transform);

        if (!System.IO.Directory.Exists("Assets/Prefabs/UI"))
            System.IO.Directory.CreateDirectory("Assets/Prefabs/UI");
        PrefabUtility.SaveAsPrefabAsset(canvasObj, "Assets/Prefabs/UI/MenuCanvas.prefab");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, MenuScenePath);
        Debug.Log("[UI Generator] ✅ Đã lưu cấu trúc UI đầy đủ vào MainMenu.unity!");
    }

    private static void BuildProfileWidget(Transform parent)
    {
        // Profile Card — Rounded dark card
        GameObject profileCard = CreateRoundedPanel(null, "ProfileCard_TopRight",
            COL_BG_CARD, Vector2.zero, Vector2.zero, Vector2.zero);
        profileCard.transform.SetParent(parent, false);
        RectTransform rt = profileCard.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-20, -25);
        rt.sizeDelta = new Vector2(360, 110);
        profileCard.AddComponent<Button>();

        // Avatar Frame
        GameObject avatarFrame = new GameObject("AvatarFrame");
        avatarFrame.transform.SetParent(profileCard.transform, false);
        RectTransform avRt = avatarFrame.AddComponent<RectTransform>();
        avRt.anchorMin = new Vector2(0f, 0.5f);
        avRt.anchorMax = new Vector2(0f, 0.5f);
        avRt.pivot = new Vector2(0f, 0.5f);
        avRt.anchoredPosition = new Vector2(18, 0);
        avRt.sizeDelta = new Vector2(76, 76);
        Image avImg = avatarFrame.AddComponent<Image>();
        avImg.color = new Color(0.12f, 0.28f, 0.55f);

        CreateText(avatarFrame.transform, "Icon", "💎",
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, 42, Color.white, TextAlignmentOptions.Center);

        CreateText(profileCard.transform, "PlayerName", "Người Chơi",
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(110, 18), new Vector2(230, 34), 26, Color.white, TextAlignmentOptions.Left);

        CreateText(profileCard.transform, "SubText", "18 tuổi • 🇻🇳",
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(110, -14), new Vector2(230, 28), 20, COL_TEXT_SUB, TextAlignmentOptions.Left);

        // Coin Display — Góc trái trên
        GameObject coinBadge = CreateRoundedPanel(parent, "CoinBadge",
            COL_BG_CARD, new Vector2(0f, 1f), new Vector2(25, -30), new Vector2(220, 52));
        RectTransform cbRt = coinBadge.GetComponent<RectTransform>();
        cbRt.pivot = new Vector2(0f, 1f);

        CreateText(parent, "CoinDisplay", "🪙 0 XU",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(135, -56), new Vector2(200, 48), 28, COL_ACCENT_GOLD, TextAlignmentOptions.Center);
    }

    private static void BuildOnboardingPanel(Transform parent)
    {
        GameObject onb = CreatePanel(parent, "OnboardingPanel", new Color(0.01f, 0.02f, 0.06f, 0.88f));

        // Card body
        CreateRoundedPanel(onb.transform, "Card", COL_BG_CARD,
            new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(880, 1300));

        // Title
        CreateText(onb.transform, "Title", "🎉 HỒ SƠ NGƯỜI CHƠI",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 575), new Vector2(750, 60), 44, COL_ACCENT_CYAN, TextAlignmentOptions.Center);

        CreateText(onb.transform, "Desc", "Hãy chọn tên, tuổi và avatar đại diện",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 510), new Vector2(720, 36), 24, COL_TEXT_SUB, TextAlignmentOptions.Center);

        CreateSeparator(onb.transform, "Sep1", new Vector2(0, 475), 720);

        // ── Name ──
        CreateText(onb.transform, "NameLabel", "TÊN CỦA BẠN",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 435), new Vector2(720, 36), 28, COL_TEXT_SUB, TextAlignmentOptions.Left);

        CreateInputField(onb.transform, "OnbNameInput", "Jelly Runner", "Nhập tên...",
            new Vector2(0, 375), new Vector2(720, 75));

        CreateSeparator(onb.transform, "Sep2", new Vector2(0, 325), 720);

        // ── Age ──
        CreateText(onb.transform, "AgeLabel", "ĐỘ TUỔI",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-130, 285), new Vector2(400, 36), 28, COL_TEXT_SUB, TextAlignmentOptions.Left);

        CreateText(onb.transform, "AgeVal", "18 tuổi",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(250, 285), new Vector2(200, 36), 30, COL_ACCENT_GOLD, TextAlignmentOptions.Right);

        CreateSlider(onb.transform, "OnbAgeSlider", new Vector2(0, 225), new Vector2(720, 45), 0.2f);

        CreateSeparator(onb.transform, "Sep3", new Vector2(0, 185), 720);

        // ── Language ──
        CreateText(onb.transform, "LangLabel", "NGÔN NGỮ",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 145), new Vector2(720, 36), 28, COL_TEXT_SUB, TextAlignmentOptions.Left);

        GameObject langGroup = new GameObject("LangGroup");
        langGroup.transform.SetParent(onb.transform, false);
        RectTransform lgRt = langGroup.AddComponent<RectTransform>();
        lgRt.anchoredPosition = new Vector2(0, 85);
        lgRt.sizeDelta = new Vector2(720, 75);

        CreateStyledButton(langGroup.transform, "LangVN", "🇻🇳  Tiếng Việt",
            new Vector2(-185, 0), new Vector2(340, 72), 26, COL_ACCENT_GREEN);

        CreateStyledButton(langGroup.transform, "LangEN", "🇬🇧  English",
            new Vector2(185, 0), new Vector2(340, 72), 26, COL_BTN_INACTIVE);

        CreateSeparator(onb.transform, "Sep4", new Vector2(0, 30), 720);

        // ── Avatar ──
        CreateText(onb.transform, "AvatarLabel", "BIỂU TƯỢNG ĐẠI DIỆN",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -10), new Vector2(720, 36), 28, COL_TEXT_SUB, TextAlignmentOptions.Left);

        // Avatar Grid
        GameObject grid = new GameObject("AvatarGrid");
        grid.transform.SetParent(onb.transform, false);
        RectTransform gRt = grid.AddComponent<RectTransform>();
        gRt.anchoredPosition = new Vector2(0, -110);
        gRt.sizeDelta = new Vector2(720, 110);

        for (int i = 0; i < MainMenuUI.AvatarList.Length; i++)
        {
            var av = MainMenuUI.AvatarList[i];
            float xPos = -300 + (i * 120);
            
            // Border/selection indicator
            GameObject item = new GameObject("AvatarItem_" + i);
            item.transform.SetParent(grid.transform, false);
            RectTransform iRt = item.AddComponent<RectTransform>();
            iRt.anchoredPosition = new Vector2(xPos, 0);
            iRt.sizeDelta = new Vector2(105, 105);

            Image borderImg = item.AddComponent<Image>();
            borderImg.color = (i == 0) ? COL_ACCENT_CYAN : COL_BTN_INACTIVE;
            item.AddComponent<Button>();

            // Inner colored circle
            GameObject inner = new GameObject("Inner");
            inner.transform.SetParent(item.transform, false);
            RectTransform inRt = inner.AddComponent<RectTransform>();
            inRt.anchorMin = new Vector2(0.08f, 0.08f);
            inRt.anchorMax = new Vector2(0.92f, 0.92f);
            inRt.offsetMin = Vector2.zero;
            inRt.offsetMax = Vector2.zero;
            Image inImg = inner.AddComponent<Image>();
            inImg.color = av.color;

            CreateText(inner.transform, "Icon", av.icon,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, 46, Color.white, TextAlignmentOptions.Center);
        }

        CreateSeparator(onb.transform, "Sep5", new Vector2(0, -180), 720);

        // ── Done Button ──
        CreateStyledButton(onb.transform, "DoneBtn", "✓  XÁC NHẬN & VÀO MENU",
            new Vector2(0, -280), new Vector2(500, 110), 38, COL_ACCENT_GREEN);

        onb.SetActive(false);
    }

    private static void BuildSettingsPanel(Transform parent)
    {
        GameObject set = CreatePanel(parent, "SettingsPanel", new Color(0.01f, 0.02f, 0.06f, 0.88f));

        // Card
        CreateRoundedPanel(set.transform, "Card", COL_BG_CARD,
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(850, 1100));

        CreateText(set.transform, "SettingsTitle", "⚙  CÀI ĐẶT",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 440), new Vector2(500, 60), 46, Color.white, TextAlignmentOptions.Center);

        CreateSeparator(set.transform, "S1", new Vector2(0, 395), 680);

        // Volume
        CreateText(set.transform, "VolLabel", "🔊 ÂM LƯỢNG",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 350), new Vector2(680, 36), 28, COL_TEXT_SUB, TextAlignmentOptions.Left);
        CreateSlider(set.transform, "VolumeSlider", new Vector2(0, 295), new Vector2(680, 45), 1f);

        CreateSeparator(set.transform, "S2", new Vector2(0, 255), 680);

        // Sensitivity
        CreateText(set.transform, "SensLabel", "👆 ĐỘ NHẠY VUỐT",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 210), new Vector2(680, 36), 28, COL_TEXT_SUB, TextAlignmentOptions.Left);
        CreateSlider(set.transform, "SensSlider", new Vector2(0, 155), new Vector2(680, 45), 0.5f);

        CreateSeparator(set.transform, "S3", new Vector2(0, 115), 680);

        // Bloom
        CreateText(set.transform, "BloomLabel", "✨ ĐỘ PHÁT SÁNG NEON",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 70), new Vector2(680, 36), 28, new Color(0.4f, 0.95f, 1f), TextAlignmentOptions.Left);
        CreateSlider(set.transform, "BloomSlider", new Vector2(0, 15), new Vector2(680, 45), 0.54f);

        CreateSeparator(set.transform, "S4", new Vector2(0, -25), 680);

        // Language Toggle
        CreateStyledButton(set.transform, "LangToggleBtn", "🌐 ĐỔI NGÔN NGỮ",
            new Vector2(0, -90), new Vector2(420, 85), 28, COL_ACCENT_GREEN);

        // Reset Best
        CreateStyledButton(set.transform, "ResetBestBtn", "🗑  XÓA KỶ LỤC",
            new Vector2(0, -200), new Vector2(400, 80), 28, COL_ACCENT_RED);

        // Close — Circle-ish button
        CreateStyledButton(set.transform, "CloseSettingsBtn", "✕",
            new Vector2(360, 450), new Vector2(70, 70), 32, new Color(0.25f, 0.30f, 0.42f));

        set.SetActive(false);
    }

    private static void BuildLeaderboardPanel(Transform parent)
    {
        GameObject lb = CreatePanel(parent, "LeaderboardPanel", new Color(0.01f, 0.02f, 0.06f, 0.88f));

        // Card
        CreateRoundedPanel(lb.transform, "Card", COL_BG_CARD,
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860, 1200));

        CreateText(lb.transform, "LBTitle", "🏆 BẢNG XẾP HẠNG",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 490), new Vector2(700, 60), 46, COL_ACCENT_GOLD, TextAlignmentOptions.Center);

        CreateSeparator(lb.transform, "LbSep", new Vector2(0, 445), 680);

        // Entries frame
        CreateRoundedPanel(lb.transform, "EntriesFrame", COL_BG_CARD_INNER,
            new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(740, 780));

        CreateText(lb.transform, "LBEntries", "1. JellyPro — 500\n2. Player — 320\n3. Runner — 210",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 30), new Vector2(680, 720), 32, Color.white, TextAlignmentOptions.TopLeft);

        // Close
        CreateStyledButton(lb.transform, "CloseLBBtn", "✕",
            new Vector2(370, 510), new Vector2(70, 70), 32, new Color(0.25f, 0.30f, 0.42f));

        lb.SetActive(false);
    }

    private static void BuildShopModalPanel(Transform parent)
    {
        GameObject shop = CreatePanel(parent, "ShopModalPanel", new Color(0.01f, 0.02f, 0.06f, 0.92f));

        // Card
        CreateRoundedPanel(shop.transform, "Card", COL_BG_CARD,
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000, 1550));

        CreateText(shop.transform, "Title", "🛍  CỬA HÀNG",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -80), new Vector2(600, 60), 46, COL_ACCENT_CYAN, TextAlignmentOptions.Center);

        CreateText(shop.transform, "ShopCoins", "🪙 0 XU",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-40, -85), new Vector2(260, 50), 34, COL_ACCENT_GOLD, TextAlignmentOptions.Right);

        // Tab Group
        GameObject tabGroup = new GameObject("TabGroup");
        tabGroup.transform.SetParent(shop.transform, false);
        RectTransform tabRt = tabGroup.AddComponent<RectTransform>();
        tabRt.anchorMin = new Vector2(0.5f, 1f);
        tabRt.anchorMax = new Vector2(0.5f, 1f);
        tabRt.pivot = new Vector2(0.5f, 1f);
        tabRt.anchoredPosition = new Vector2(0, -160);
        tabRt.sizeDelta = new Vector2(940, 82);

        CreateStyledButton(tabGroup.transform, "Tab_Player", "👤 NHÂN VẬT",
            new Vector2(-315, 0), new Vector2(290, 78), 28, new Color(0f, 0.6f, 0.85f));

        CreateStyledButton(tabGroup.transform, "Tab_Wall", "🧱 TƯỜNG",
            new Vector2(0, 0), new Vector2(290, 78), 28, COL_BTN_INACTIVE);

        CreateStyledButton(tabGroup.transform, "Tab_Effect", "✨ KỸ NĂNG",
            new Vector2(315, 0), new Vector2(290, 78), 28, COL_BTN_INACTIVE);

        // Scroll View
        GameObject scrollObj = new GameObject("ScrollView");
        scrollObj.transform.SetParent(shop.transform, false);
        RectTransform scrollRt = scrollObj.AddComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0.5f, 0.5f);
        scrollRt.anchorMax = new Vector2(0.5f, 0.5f);
        scrollRt.pivot = new Vector2(0.5f, 0.5f);
        scrollRt.anchoredPosition = new Vector2(0, 10);
        scrollRt.sizeDelta = new Vector2(940, 1000);

        ScrollRect scrollRect = scrollObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform viewRt = viewport.AddComponent<RectTransform>();
        viewRt.anchorMin = Vector2.zero;
        viewRt.anchorMax = Vector2.one;
        viewRt.offsetMin = Vector2.zero;
        viewRt.offsetMax = Vector2.zero;
        viewport.AddComponent<RectMask2D>();
        scrollRect.viewport = viewRt;

        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRt = content.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = new Vector2(0, 1000);

        VerticalLayoutGroup vLayout = content.AddComponent<VerticalLayoutGroup>();
        vLayout.spacing = 18;
        vLayout.padding = new RectOffset(15, 15, 15, 15);
        vLayout.childAlignment = TextAnchor.UpperCenter;
        vLayout.childControlHeight = false;
        vLayout.childControlWidth = true;

        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = contentRt;

        // Close Button
        CreateStyledButton(shop.transform, "CloseBtn", "← QUAY LẠI",
            new Vector2(0, -660), new Vector2(380, 90), 34, COL_ACCENT_RED);

        shop.SetActive(false);
    }

    // =========================================================================
    // 2. XÂY DỰNG UI CHO SCENE IN-GAME (SAMPLESCENE)
    // =========================================================================

    public static void GenerateGameSceneUI()
    {
        Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        EnsureEventSystem();

        GameObject canvasObj = GameObject.Find("GameCanvas");
        if (canvasObj != null)
            Object.DestroyImmediate(canvasObj);

        canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // 1. HUD Panel
        GameObject hudPanel = CreatePanel(canvasObj.transform, "HUDPanel", Color.clear);

        // Top HUD bar background
        CreateRoundedPanel(hudPanel.transform, "TopBar", new Color(0.05f, 0.07f, 0.14f, 0.85f),
            new Vector2(0.5f, 1f), new Vector2(0, -50), new Vector2(1040, 100));

        CreateText(hudPanel.transform, "ScoreText", "0",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -50), new Vector2(300, 70), 52, Color.white, TextAlignmentOptions.Center);

        // Coin badge
        CreateRoundedPanel(hudPanel.transform, "CoinBadge", new Color(0.08f, 0.10f, 0.18f, 0.9f),
            new Vector2(0f, 1f), new Vector2(30, -125), new Vector2(180, 48));
        CreateText(hudPanel.transform, "CoinHUDText", "🪙 0",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(120, -149), new Vector2(160, 42), 28, COL_ACCENT_GOLD, TextAlignmentOptions.Center);

        CreateText(hudPanel.transform, "DistanceText", "0m",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-30, -149), new Vector2(180, 42), 26, new Color(0.6f, 0.9f, 1f), TextAlignmentOptions.Right);

        CreateStyledButton(hudPanel.transform, "PauseButton", "⏸",
            new Vector2(-505, -50), new Vector2(80, 80), 40, new Color(1, 1, 1, 0.15f));
        // Adjust pause button to top-left
        RectTransform pauseRt = hudPanel.transform.Find("PauseButton").GetComponent<RectTransform>();
        pauseRt.anchorMin = new Vector2(0f, 1f);
        pauseRt.anchorMax = new Vector2(0f, 1f);
        pauseRt.pivot = new Vector2(0f, 1f);
        pauseRt.anchoredPosition = new Vector2(30, -30);

        // 2. GameOver Panel
        GameObject gameOverPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.01f, 0.02f, 0.06f, 0.90f));

        CreateRoundedPanel(gameOverPanel.transform, "Card", COL_BG_CARD,
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(880, 1200));

        CreateText(gameOverPanel.transform, "GameOverTitle", "KẾT THÚC!",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 420), new Vector2(650, 90), 64, COL_ACCENT_RED, TextAlignmentOptions.Center);

        CreateSeparator(gameOverPanel.transform, "GOSep1", new Vector2(0, 365), 600);

        CreateText(gameOverPanel.transform, "GameOverScore", "ĐIỂM: 0",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 300), new Vector2(550, 65), 48, COL_ACCENT_GOLD, TextAlignmentOptions.Center);

        CreateText(gameOverPanel.transform, "BestScore", "KỶ LỤC: 0",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 230), new Vector2(500, 45), 34, Color.white, TextAlignmentOptions.Center);

        CreateSeparator(gameOverPanel.transform, "GOSep2", new Vector2(0, 195), 600);

        // Leaderboard mini frame
        CreateRoundedPanel(gameOverPanel.transform, "LBMiniFrame", COL_BG_CARD_INNER,
            new Vector2(0.5f, 0.5f), new Vector2(0, 50), new Vector2(700, 240));
        CreateText(gameOverPanel.transform, "LeaderboardMini", "",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 50), new Vector2(660, 210), 26, new Color(0.82f, 0.88f, 1f), TextAlignmentOptions.Center);

        CreateStyledButton(gameOverPanel.transform, "RestartButton", "🔄  CHƠI LẠI",
            new Vector2(0, -160), new Vector2(420, 105), 38, COL_ACCENT_GREEN);

        CreateStyledButton(gameOverPanel.transform, "MenuButton", "🏠  MENU",
            new Vector2(0, -280), new Vector2(340, 85), 30, new Color(0.30f, 0.35f, 0.48f));

        gameOverPanel.SetActive(false);

        // 3. Pause Panel
        GameObject pausePanel = CreatePanel(canvasObj.transform, "PausePanel", new Color(0.01f, 0.02f, 0.06f, 0.88f));

        CreateRoundedPanel(pausePanel.transform, "Card", COL_BG_CARD,
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(850, 980));

        CreateText(pausePanel.transform, "PauseTitle", "⏸  TẠM DỪNG",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 370), new Vector2(550, 70), 52, Color.white, TextAlignmentOptions.Center);

        CreateSeparator(pausePanel.transform, "PSep1", new Vector2(0, 325), 650);

        CreateStyledButton(pausePanel.transform, "ResumeButton", "▶  TIẾP TỤC",
            new Vector2(0, 230), new Vector2(400, 105), 38, COL_ACCENT_CYAN);

        CreateStyledButton(pausePanel.transform, "PauseRestartBtn", "🔄  CHƠI LẠI",
            new Vector2(0, 110), new Vector2(380, 95), 34, COL_ACCENT_GREEN);

        CreateSeparator(pausePanel.transform, "PSep2", new Vector2(0, 45), 650);

        // Bloom controls
        CreateText(pausePanel.transform, "PauseBloomLabel", "✨ Độ phát sáng Neon",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -5), new Vector2(650, 36), 26, new Color(0.4f, 0.95f, 1f), TextAlignmentOptions.Left);
        CreateSlider(pausePanel.transform, "PauseBloomSlider", new Vector2(0, -55), new Vector2(650, 42), 0.54f);

        CreateSeparator(pausePanel.transform, "PSep3", new Vector2(0, -100), 650);

        CreateStyledButton(pausePanel.transform, "PauseMenuBtn", "🏠  MENU",
            new Vector2(0, -170), new Vector2(340, 85), 30, new Color(0.30f, 0.35f, 0.48f));

        pausePanel.SetActive(false);

        if (!System.IO.Directory.Exists("Assets/Prefabs/UI"))
            System.IO.Directory.CreateDirectory("Assets/Prefabs/UI");
        PrefabUtility.SaveAsPrefabAsset(canvasObj, "Assets/Prefabs/UI/GameCanvas.prefab");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, GameScenePath);
        Debug.Log("[UI Generator] ✅ Đã lưu cấu trúc UI đầy đủ vào SampleScene.unity!");
    }

    // =========================================================================
    // UI BUILDER HELPERS CHO EDITOR
    // =========================================================================

    private static void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = obj.AddComponent<Image>();
        img.color = color;
        return obj;
    }

    /// <summary>Panel bo góc giả lập bằng Image color + positioning</summary>
    private static GameObject CreateRoundedPanel(Transform parent, string name, Color color,
        Vector2 anchor, Vector2 anchoredPos, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        if (parent != null) obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.color = color;
        return obj;
    }

    /// <summary>Thanh phân cách trang trí</summary>
    private static void CreateSeparator(Transform parent, string name, Vector2 pos, float width)
    {
        GameObject sep = new GameObject(name);
        sep.transform.SetParent(parent, false);
        RectTransform rt = sep.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(width, 2);
        Image img = sep.AddComponent<Image>();
        img.color = COL_SEPARATOR;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 size, int fontSize, Color color, TextAlignmentOptions align)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = align;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        return tmp;
    }

    /// <summary>Button có viền phát sáng nhẹ, chuyên nghiệp hơn</summary>
    private static Button CreateStyledButton(Transform parent, string name, string label,
        Vector2 anchoredPos, Vector2 size, int fontSize, Color bgColor)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.color = bgColor;

        Button btn = obj.AddComponent<Button>();

        // Label — centered
        CreateText(obj.transform, name + "_Label", label,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, fontSize, Color.white, TextAlignmentOptions.Center);

        return btn;
    }

    private static TMP_InputField CreateInputField(Transform parent, string name,
        string defaultText, string placeholder, Vector2 anchoredPos, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image bgImg = obj.AddComponent<Image>();
        bgImg.color = COL_INPUT_BG;

        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(obj.transform, false);
        RectTransform textAreaRt = textArea.AddComponent<RectTransform>();
        textAreaRt.anchorMin = Vector2.zero;
        textAreaRt.anchorMax = Vector2.one;
        textAreaRt.offsetMin = new Vector2(20, 5);
        textAreaRt.offsetMax = new Vector2(-20, -5);
        textArea.AddComponent<RectMask2D>();

        GameObject placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(textArea.transform, false);
        RectTransform phRt = placeholderObj.AddComponent<RectTransform>();
        phRt.anchorMin = Vector2.zero;
        phRt.anchorMax = Vector2.one;
        phRt.offsetMin = Vector2.zero;
        phRt.offsetMax = Vector2.zero;
        TextMeshProUGUI phText = placeholderObj.AddComponent<TextMeshProUGUI>();
        phText.text = placeholder;
        phText.fontSize = 28;
        phText.color = new Color(0.45f, 0.50f, 0.62f, 0.65f);
        phText.alignment = TextAlignmentOptions.MidlineLeft;

        GameObject inputTextObj = new GameObject("Text");
        inputTextObj.transform.SetParent(textArea.transform, false);
        RectTransform itRt = inputTextObj.AddComponent<RectTransform>();
        itRt.anchorMin = Vector2.zero;
        itRt.anchorMax = Vector2.one;
        itRt.offsetMin = Vector2.zero;
        itRt.offsetMax = Vector2.zero;
        TextMeshProUGUI inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
        inputText.text = defaultText;
        inputText.fontSize = 30;
        inputText.color = Color.white;
        inputText.alignment = TextAlignmentOptions.MidlineLeft;

        TMP_InputField inputField = obj.AddComponent<TMP_InputField>();
        inputField.textViewport = textAreaRt;
        inputField.textComponent = inputText;
        inputField.placeholder = phText;
        inputField.text = defaultText;

        return inputField;
    }

    private static Slider CreateSlider(Transform parent, string name, Vector2 anchoredPos, Vector2 size, float value)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Slider slider = obj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = value;

        // Background — Dark track
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(obj.transform, false);
        RectTransform bgRt = bgObj.AddComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0f, 0.3f);
        bgRt.anchorMax = new Vector2(1f, 0.7f);
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = COL_SLIDER_BG;

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(obj.transform, false);
        RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0f, 0.3f);
        fillAreaRt.anchorMax = new Vector2(1f, 0.7f);
        fillAreaRt.offsetMin = new Vector2(5, 0);
        fillAreaRt.offsetMax = new Vector2(-5, 0);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRt = fill.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = COL_ACCENT_CYAN;

        slider.fillRect = fillRt;

        // Handle Slide Area
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(obj.transform, false);
        RectTransform handleAreaRt = handleArea.AddComponent<RectTransform>();
        handleAreaRt.anchorMin = Vector2.zero;
        handleAreaRt.anchorMax = Vector2.one;
        handleAreaRt.offsetMin = new Vector2(10, 0);
        handleAreaRt.offsetMax = new Vector2(-10, 0);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRt = handle.AddComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(32, 0);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

        slider.handleRect = handleRt;
        slider.targetGraphic = handleImg;

        return slider;
    }
}

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
/// </summary>
public static class SceneUIHierarchyGenerator
{
    private const string MenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string GameScenePath = "Assets/Scenes/SampleScene.unity";

    [MenuItem("Jelly Runner/🎨 Tạo Toàn Bộ UI Trực Quan Vào Scene (Để chỉnh sửa)", false, 2)]
    public static void GenerateAllSceneUIs()
    {
        Debug.Log("<color=#00E5FF><b>==== [UI Generator] BẮT ĐẦU TẠO CÂY UI TRỰC QUAN VÀO SCENES ====</b></color>");

        // 1. Tạo UI cho MainMenu Scene
        GenerateMainMenuSceneUI();

        // 2. Tạo UI cho Game Scene (SampleScene)
        GenerateGameSceneUI();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("<color=#00FF66><b>==== [UI Generator] 🎉 HOÀN TẤT! Toàn bộ UI đã hiện trong Hierarchy và Scene View! ====</b></color>");
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

        // 1. Background
        CreatePanel(canvasObj.transform, "Background", new Color(0.05f, 0.07f, 0.12f, 1f));

        // 2. Main Menu Panel (Chứa Title, Kỷ lục, và các Nút chính)
        GameObject menuPanel = CreatePanel(canvasObj.transform, "MainMenuPanel", Color.clear);

        CreateText(menuPanel.transform, "TitleText", "JELLY SHAPE\nRUNNER 3D",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 480), new Vector2(850, 240), 78, new Color(0f, 0.88f, 1f), TextAlignmentOptions.Center);

        CreateText(menuPanel.transform, "BestScoreText", "🏆 KỶ LỤC: 0 ĐIỂM",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 330), new Vector2(650, 50), 36, new Color(1f, 0.85f, 0.15f), TextAlignmentOptions.Center);

        // Buttons Group
        GameObject btnsGroup = CreatePanel(menuPanel.transform, "ButtonsGroup", Color.clear);
        CreateButton(btnsGroup.transform, "PlayButton", "▶  BẮT ĐẦU CHƠI",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 160), new Vector2(560, 110), 46, new Color(0f, 0.85f, 0.45f));

        CreateButton(btnsGroup.transform, "ShopButton", "🛍️  CỬA HÀNG VẬT PHẨM",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 30), new Vector2(520, 95), 36, new Color(0.95f, 0.45f, 0.1f));

        CreateButton(btnsGroup.transform, "LeaderboardButton", "🏆  BẢNG XẾP HẠNG",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -90), new Vector2(480, 85), 34, new Color(0.9f, 0.7f, 0.05f));

        CreateButton(btnsGroup.transform, "SettingsButton", "⚙️  CÀI ĐẶT",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -200), new Vector2(440, 80), 32, new Color(0.25f, 0.5f, 0.85f));

        CreateButton(btnsGroup.transform, "QuitButton", "THOÁT",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -310), new Vector2(320, 65), 28, new Color(0.55f, 0.2f, 0.2f));

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

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, MenuScenePath);
        Debug.Log("[UI Generator] ✅ Đã lưu cấu trúc UI đầy đủ vào MainMenu.unity!");
    }

    private static void BuildProfileWidget(Transform parent)
    {
        GameObject profileCard = new GameObject("ProfileCard_TopRight");
        profileCard.transform.SetParent(parent, false);
        RectTransform rt = profileCard.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-30, -35);
        rt.sizeDelta = new Vector2(460, 120);

        Image bg = profileCard.AddComponent<Image>();
        bg.color = new Color(0.12f, 0.16f, 0.25f, 0.92f);
        profileCard.AddComponent<Button>();

        // Avatar Frame
        GameObject avatarFrame = new GameObject("AvatarFrame");
        avatarFrame.transform.SetParent(profileCard.transform, false);
        RectTransform avRt = avatarFrame.AddComponent<RectTransform>();
        avRt.anchorMin = new Vector2(1f, 0.5f);
        avRt.anchorMax = new Vector2(1f, 0.5f);
        avRt.pivot = new Vector2(1f, 0.5f);
        avRt.anchoredPosition = new Vector2(-15, 0);
        avRt.sizeDelta = new Vector2(90, 90);
        Image avImg = avatarFrame.AddComponent<Image>();
        avImg.color = new Color(0f, 0.85f, 1f);

        CreateText(avatarFrame.transform, "Icon", "💎",
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, 50, Color.white, TextAlignmentOptions.Center);

        CreateText(profileCard.transform, "PlayerName", "Người Chơi",
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(25, 20), new Vector2(310, 45), 32, Color.white, TextAlignmentOptions.Left);

        CreateText(profileCard.transform, "SubText", "18 tuổi • 🇻🇳 VN",
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(25, -20), new Vector2(310, 35), 24, new Color(0.7f, 0.8f, 0.95f), TextAlignmentOptions.Left);

        CreateText(parent, "CoinDisplay", "🪙 0 XU",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-35, -170), new Vector2(400, 50), 32, new Color(1f, 0.85f, 0.1f), TextAlignmentOptions.Right);
    }

    private static void BuildOnboardingPanel(Transform parent)
    {
        GameObject onb = CreatePanel(parent, "OnboardingPanel", new Color(0.03f, 0.04f, 0.07f, 0.98f));

        CreateText(onb.transform, "Title", "🎉 CHÀO MỪNG BẠN!",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 580), new Vector2(800, 80), 56, new Color(0f, 0.9f, 1f), TextAlignmentOptions.Center);

        CreateText(onb.transform, "Desc", "Hãy thiết lập hồ sơ người chơi của bạn",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 500), new Vector2(800, 50), 30, Color.gray, TextAlignmentOptions.Center);

        CreateText(onb.transform, "NameLabel", "1. NHẬP TÊN CỦA BẠN:",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 410), new Vector2(700, 40), 32, Color.white, TextAlignmentOptions.Left);

        CreateInputField(onb.transform, "OnbNameInput", "Jelly Runner", "Nhập tên người chơi...",
            new Vector2(0, 335), new Vector2(700, 80));

        CreateText(onb.transform, "AgeLabel", "2. KÉO CHỌN TUỔI CỦA BẠN:",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 230), new Vector2(700, 40), 32, Color.white, TextAlignmentOptions.Left);

        CreateText(onb.transform, "AgeVal", "👉 Tuổi: 18",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 175), new Vector2(700, 45), 36, new Color(1f, 0.85f, 0.2f), TextAlignmentOptions.Center);

        CreateSlider(onb.transform, "OnbAgeSlider", new Vector2(0, 110), new Vector2(700, 45), 0.2f);

        CreateText(onb.transform, "LangLabel", "3. CHỌN NGÔN NGỮ (MẶC ĐỊNH: TIẾNG VIỆT):",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 20), new Vector2(700, 40), 30, Color.white, TextAlignmentOptions.Left);

        GameObject langGroup = new GameObject("LangGroup");
        langGroup.transform.SetParent(onb.transform, false);
        RectTransform lgRt = langGroup.AddComponent<RectTransform>();
        lgRt.anchoredPosition = new Vector2(0, -45);
        lgRt.sizeDelta = new Vector2(700, 75);

        CreateButton(langGroup.transform, "LangVN", "🇻🇳  Tiếng Việt (Mặc định)",
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(0, 0), new Vector2(335, 75), 26, new Color(0f, 0.65f, 0.4f));

        CreateButton(langGroup.transform, "LangEN", "🇬🇧  English",
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0, 0), new Vector2(335, 75), 26, new Color(0.2f, 0.25f, 0.35f));

        CreateText(onb.transform, "AvatarLabel", "4. CHỌN AVATAR CỦA BẠN:",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -135), new Vector2(700, 40), 30, Color.white, TextAlignmentOptions.Left);

        // Avatar Grid
        GameObject grid = new GameObject("AvatarGrid");
        grid.transform.SetParent(onb.transform, false);
        RectTransform gRt = grid.AddComponent<RectTransform>();
        gRt.anchoredPosition = new Vector2(0, -250);
        gRt.sizeDelta = new Vector2(720, 160);

        for (int i = 0; i < MainMenuUI.AvatarList.Length; i++)
        {
            var av = MainMenuUI.AvatarList[i];
            GameObject item = new GameObject("AvatarItem_" + i);
            item.transform.SetParent(grid.transform, false);
            RectTransform iRt = item.AddComponent<RectTransform>();
            float xPos = -300 + (i * 120);
            iRt.anchoredPosition = new Vector2(xPos, 0);
            iRt.sizeDelta = new Vector2(105, 105);

            Image borderImg = item.AddComponent<Image>();
            borderImg.color = (i == 0) ? Color.white : new Color(0.2f, 0.25f, 0.35f);
            item.AddComponent<Button>();

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
                Vector2.zero, Vector2.zero, 48, Color.white, TextAlignmentOptions.Center);
        }

        CreateButton(onb.transform, "DoneBtn", "✓  HOÀN TẤT & VÀO MENU",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -480), new Vector2(620, 105), 40, new Color(0f, 0.85f, 0.45f));

        onb.SetActive(false); // Ẩn mặc định để Designer nhìn thấy Menu chính trước
    }

    private static void BuildSettingsPanel(Transform parent)
    {
        GameObject set = CreatePanel(parent, "SettingsPanel", new Color(0, 0, 0, 0.94f));

        CreateText(set.transform, "SettingsTitle", "⚙️ CÀI ĐẶT",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 380), new Vector2(500, 80), 56, Color.white, TextAlignmentOptions.Center);

        // Âm lượng
        CreateText(set.transform, "VolLabel", "ÂM LƯỢNG",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 270), new Vector2(400, 40), 28, Color.gray, TextAlignmentOptions.Center);
        CreateSlider(set.transform, "VolumeSlider", new Vector2(0, 220), new Vector2(550, 45), 1f);

        // Độ nhạy
        CreateText(set.transform, "SensLabel", "ĐỘ NHẠY VUỐT",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 150), new Vector2(400, 40), 28, Color.gray, TextAlignmentOptions.Center);
        CreateSlider(set.transform, "SensSlider", new Vector2(0, 100), new Vector2(550, 45), 0.5f);

        // Độ phát sáng (Bloom)
        CreateText(set.transform, "BloomLabel", "✨ ĐỘ PHÁT SÁNG (BLOOM)",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 30), new Vector2(500, 40), 28, new Color(0f, 0.88f, 1f), TextAlignmentOptions.Center);
        CreateSlider(set.transform, "BloomSlider", new Vector2(0, -20), new Vector2(550, 45), 0.54f);

        // Đổi ngôn ngữ
        CreateButton(set.transform, "LangToggleBtn", "🌐 NGÔN NGỮ: TIẾNG VIỆT 🇻🇳",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -110), new Vector2(480, 70), 28, new Color(0.15f, 0.6f, 0.45f));

        // Xóa kỷ lục
        CreateButton(set.transform, "ResetBestBtn", "XÓA KỶ LỤC",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -200), new Vector2(440, 70), 30, new Color(0.85f, 0.2f, 0.2f));

        // Đóng
        CreateButton(set.transform, "CloseSettingsBtn", "ĐÓNG",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -310), new Vector2(340, 75), 34, new Color(0.4f, 0.4f, 0.5f));

        set.SetActive(false);
    }

    private static void BuildLeaderboardPanel(Transform parent)
    {
        GameObject lb = CreatePanel(parent, "LeaderboardPanel", new Color(0, 0, 0, 0.94f));

        CreateText(lb.transform, "LBTitle", "🏆 BẢNG XẾP HẠNG TOP 5",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 350), new Vector2(700, 80), 54, new Color(1f, 0.85f, 0.1f), TextAlignmentOptions.Center);

        CreateText(lb.transform, "LBEntries", "1. JellyPro — 500\n2. Player — 320\n3. Runner — 210",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 60), new Vector2(750, 480), 38, Color.white, TextAlignmentOptions.Center);

        CreateButton(lb.transform, "CloseLBBtn", "ĐÓNG",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -300), new Vector2(340, 75), 34, new Color(0.4f, 0.4f, 0.5f));

        lb.SetActive(false);
    }

    private static void BuildShopModalPanel(Transform parent)
    {
        GameObject shop = CreatePanel(parent, "ShopModalPanel", new Color(0.04f, 0.05f, 0.09f, 0.96f));

        CreateText(shop.transform, "Title", "🛍️ CỬA HÀNG VẬT PHẨM",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -90), new Vector2(700, 80), 54, new Color(0f, 0.9f, 1f), TextAlignmentOptions.Center);

        CreateText(shop.transform, "ShopCoins", "🪙 0 XU",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-40, -90), new Vector2(300, 60), 38, new Color(1f, 0.85f, 0.1f), TextAlignmentOptions.Right);

        // Tab Group
        GameObject tabGroup = new GameObject("TabGroup");
        tabGroup.transform.SetParent(shop.transform, false);
        RectTransform tabRt = tabGroup.AddComponent<RectTransform>();
        tabRt.anchorMin = new Vector2(0.5f, 1f);
        tabRt.anchorMax = new Vector2(0.5f, 1f);
        tabRt.pivot = new Vector2(0.5f, 1f);
        tabRt.anchoredPosition = new Vector2(0, -180);
        tabRt.sizeDelta = new Vector2(980, 80);

        CreateButton(tabGroup.transform, "Tab_Player", "👤 SKIN JELLY",
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(0, 0), new Vector2(310, 75), 30, new Color(0f, 0.6f, 0.8f));

        CreateButton(tabGroup.transform, "Tab_Wall", "🧱 SKIN TƯỜNG",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 0), new Vector2(310, 75), 30, new Color(0.2f, 0.25f, 0.35f));

        CreateButton(tabGroup.transform, "Tab_Effect", "✨ KỸ NĂNG/HIỆU ỨNG",
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0, 0), new Vector2(310, 75), 28, new Color(0.2f, 0.25f, 0.35f));

        // Scroll View
        GameObject scrollObj = new GameObject("ScrollView");
        scrollObj.transform.SetParent(shop.transform, false);
        RectTransform scrollRt = scrollObj.AddComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0.5f, 0.5f);
        scrollRt.anchorMax = new Vector2(0.5f, 0.5f);
        scrollRt.pivot = new Vector2(0.5f, 0.5f);
        scrollRt.anchoredPosition = new Vector2(0, -40);
        scrollRt.sizeDelta = new Vector2(980, 1250);

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
        vLayout.spacing = 25;
        vLayout.padding = new RectOffset(10, 10, 20, 20);
        vLayout.childAlignment = TextAnchor.UpperCenter;
        vLayout.childControlHeight = false;
        vLayout.childControlWidth = true;

        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = contentRt;

        CreateButton(shop.transform, "CloseBtn", "✕  QUAY LẠI MENU",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 90), new Vector2(500, 85), 36, new Color(0.85f, 0.25f, 0.25f));

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

        CreateText(hudPanel.transform, "ScoreText", "0",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -50), new Vector2(300, 80), 68, Color.white, TextAlignmentOptions.Center);

        CreateText(hudPanel.transform, "CoinHUDText", "🪙 0",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-25, -45), new Vector2(250, 50), 34, new Color(1f, 0.85f, 0f), TextAlignmentOptions.Right);

        CreateText(hudPanel.transform, "DistanceText", "0m",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-25, -100), new Vector2(250, 50), 28, new Color(0.7f, 0.85f, 1f), TextAlignmentOptions.Right);

        CreateButton(hudPanel.transform, "PauseButton", "⏸",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(45, -45), new Vector2(90, 90), 48, new Color(1, 1, 1, 0.6f));

        // 2. GameOver Panel
        GameObject gameOverPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.04f, 0.06f, 0.1f, 0.95f));

        CreateText(gameOverPanel.transform, "GameOverTitle", "THUA CUỘC!",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 340), new Vector2(650, 100), 72, new Color(1f, 0.3f, 0.35f), TextAlignmentOptions.Center);

        CreateText(gameOverPanel.transform, "GameOverScore", "ĐIỂM CỦA BẠN: 0",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 220), new Vector2(550, 60), 48, Color.white, TextAlignmentOptions.Center);

        CreateText(gameOverPanel.transform, "BestScore", "KỶ LỤC: 0",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 150), new Vector2(550, 50), 36, new Color(1f, 0.85f, 0f), TextAlignmentOptions.Center);

        CreateText(gameOverPanel.transform, "LeaderboardMini", "",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 10), new Vector2(700, 220), 28, new Color(0.85f, 0.9f, 1f), TextAlignmentOptions.Center);

        CreateButton(gameOverPanel.transform, "RestartButton", "🔄  CHƠI LẠI",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -160), new Vector2(480, 90), 40, new Color(0f, 0.85f, 0.45f));

        CreateButton(gameOverPanel.transform, "MenuButton", "🏠  VỀ MENU",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -270), new Vector2(360, 75), 32, new Color(0.4f, 0.45f, 0.55f));

        gameOverPanel.SetActive(false);

        // 3. Pause Panel
        GameObject pausePanel = CreatePanel(canvasObj.transform, "PausePanel", new Color(0.04f, 0.06f, 0.1f, 0.92f));

        CreateText(pausePanel.transform, "PauseTitle", "⏸️  TẠM DỪNG",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 240), new Vector2(550, 100), 72, Color.white, TextAlignmentOptions.Center);

        CreateButton(pausePanel.transform, "ResumeButton", "▶  TIẾP TỤC",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 110), new Vector2(460, 85), 40, new Color(0f, 0.85f, 1f));

        CreateButton(pausePanel.transform, "PauseRestartBtn", "🔄  CHƠI LẠI",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 10), new Vector2(460, 85), 40, new Color(0f, 0.85f, 0.45f));

        // Thanh trượt phát sáng Neon trực tiếp khi Pause
        CreateText(pausePanel.transform, "PauseBloomLabel", "✨ ĐỘ PHÁT SÁNG (BLOOM)",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -80), new Vector2(460, 35), 26, new Color(0f, 0.88f, 1f), TextAlignmentOptions.Center);
        CreateSlider(pausePanel.transform, "PauseBloomSlider", new Vector2(0, -125), new Vector2(480, 42), 0.54f);

        CreateButton(pausePanel.transform, "PauseMenuBtn", "🏠  VỀ MENU",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -220), new Vector2(360, 75), 32, new Color(0.4f, 0.45f, 0.55f));

        pausePanel.SetActive(false);

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
        return tmp;
    }

    private static Button CreateButton(Transform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 size, int fontSize, Color bgColor)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.color = bgColor;

        Button btn = obj.AddComponent<Button>();

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
        bgImg.color = new Color(0.12f, 0.16f, 0.24f, 1f);

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
        phText.fontSize = 32;
        phText.color = new Color(0.5f, 0.55f, 0.65f, 0.7f);
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
        inputText.fontSize = 34;
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

        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(obj.transform, false);
        RectTransform bgRt = bgObj.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.25f, 0.35f);

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(obj.transform, false);
        RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero;
        fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.offsetMin = new Vector2(5, 5);
        fillAreaRt.offsetMax = new Vector2(-5, -5);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRt = fill.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0f, 0.85f, 1f);

        slider.fillRect = fillRt;

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
        handleRt.sizeDelta = new Vector2(35, 0);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

        slider.handleRect = handleRt;
        slider.targetGraphic = handleImg;

        return slider;
    }
}

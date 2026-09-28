using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Tool tự động thiết lập toàn bộ Scene MainMenu và SampleScene với Layer Lab GUI Pro.
/// Tự động sinh/thay thế các Prefab Layer Lab, canh chỉnh RectTransform, gán Font LilitaOne,
/// kết nối Component và Particle FX Manager.
/// 
/// ★ PHIÊN BẢN NÂNG CẤP — Giao diện cao cấp, chuyên nghiệp hơn:
///   - Sử dụng Gradient Frames, Border Frames, Popup Divided
///   - Layout VerticalLayoutGroup tự động cho button groups
///   - Decorative dividers, title ribbons cải tiến
///   - Profile Card dùng UserInfo prefab
///   - Spacing & sizing nhất quán, cân đối
/// </summary>
[InitializeOnLoad]
public static class LayerLabAutoSetup
{
    private const string PREF_KEY = "LayerLab_AutoSetup_v4";

    private const string PATH_FONT_OUTLINE = "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Fonts/LilitaOne-Regular Outline 72 SDF.asset";
    private const string PATH_FONT_REGULAR = "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Fonts/LilitaOne-Regular SDF.asset";

    // ===== BUTTON PREFABS =====
    private const string BTN_GREEN_225 = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_225_BtnText_Green.prefab";
    private const string BTN_GREEN_195 = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_195_BtnText_Green.prefab";
    private const string BTN_GREEN_175 = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_175_BtnText_Green.prefab";
    private const string BTN_YELLOW_195 = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_195_BtnText_Yellow.prefab";
    private const string BTN_ORANGE_195 = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_195_BtnText_Orange.prefab";
    private const string BTN_BLUE_195 = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_195_BtnText_Blue.prefab";
    private const string BTN_BLUE_175 = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_175_BtnText_Blue.prefab";
    private const string BTN_BLUE_225 = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_225_BtnText_Blue.prefab";
    private const string BTN_GRAY_195 = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_195_BtnText_Gray.prefab";
    private const string BTN_GRAY_175 = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_175_BtnText_Gray.prefab";
    private const string BTN_RED_145 = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_145_BtnText_Red.prefab";
    private const string BTN_RED_175 = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_175_BtnText_Red.prefab";
    private const string BTN_SKY_225 = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_225_BtnText_Sky.prefab";
    private const string BTN_PURPLE_195 = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_195_BtnText_Purple.prefab";
    private const string BTN_CIRCLE_DARK_128 = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_Circle128_Dark.prefab";
    private const string BTN_CIRCLE_WHITE_128 = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_Circle128_White.prefab";
    private const string BTN_SQUARE_SKY_01 = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_Square01_Sky.prefab";
    private const string BTN_SQUARE_NAVY_03 = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_Square03_Navy.prefab";

    // ===== FRAME PREFABS =====
    private const string FRAME_ROUND12_NAVY = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BasicFrame_Round12_Navy.prefab";
    private const string FRAME_ROUND12_GRADIENT_NAVY = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BasicFrame_Round12_Gradient_Navy.prefab";
    private const string FRAME_ROUND20_TRANSPARENT_NAVY = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BasicFrame_Round20_Transparent_Navy_1.prefab";
    private const string BORDER_ROUND01_BLUE = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BorderFrame_Round01_Blue.prefab";
    private const string BORDER_ROUND03_NAVY = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BorderFrame_Round03_Navy.prefab";
    private const string BORDER_CIRCLE_NAVY = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BorderFrame_Circle81_Navy.prefab";
    private const string PANEL_FRAME_ROUND_NAVY = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/PanelFrame01_Round_Navy.prefab";
    private const string PROFILE_FRAME_BLUE = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/ProfileFrame01_Blue.prefab";

    // ===== POPUP PREFABS =====
    private const string POPUP01_NAVY = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/Popup01_Single_Navy.prefab";
    private const string POPUP02_DIVIDED_NAVY = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/Popup02_Divided_Single_Navy.prefab";
    private const string POPUP_FULLWIDTH_NAVY = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/Popup_FullWidth01_Navy.prefab";
    private const string POPUP_FULLWIDTH03_NAVY = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/Popup_FullWidth03_Single_Navy.prefab";

    // ===== SLIDER PREFABS =====
    private const string SLIDER_BASIC02_YELLOW = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Basic02_DemoYellow.prefab";
    private const string SLIDER_HANDLE_WHITE = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Handle_White.prefab";
    private const string SLIDER_HANDLE_YELLOW = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Handle_Yellow.prefab";

    // ===== LABEL / TITLE PREFABS =====
    private const string TITLE_RIBBON_BLUE = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Title_Ribbon_Blue.prefab";
    private const string TITLE_RIBBON_GREEN = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Title_Ribbon_Green.prefab";
    private const string TITLE_RIBBON_YELLOW = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Title_Ribbon_Yellow.prefab";
    private const string TITLE_FLAG_RED = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Title_Flag01_Red.prefab";
    private const string TITLE_FLAG_BLUE = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Title_Flag01_Blue.prefab";
    private const string TITLE_FLAG_PURPLE = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Title_Flag01_Purple.prefab";
    private const string TITLE_OVAL_BLUE = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Title_Oval_Blue.prefab";
    private const string LABEL_ROUND_GREEN = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Label_Round02_Green.prefab";
    private const string TITLE_DIVIDER = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Title_Line_Divider_TransparentWhite.prefab";

    // ===== INPUT FIELD =====
    private const string INPUTFIELD_NAVY = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/InputField02_Navy.prefab";

    // ===== PARTICLE FX =====
    private const string FX_SPREAD_STAR = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Spread_Star03.prefab";
    private const string FX_SPREAD_CIRCLE = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Spread_Circle01.prefab";
    private const string FX_SHINES_GLOW = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Shines_Glow01.prefab";
    private const string FX_SPARKLE_WHITE = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_Star01_CustomColor_White.prefab";
    private const string FX_ROTATE_LIGHT = "Assets/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Rotate_Light01.prefab";

    static LayerLabAutoSetup()
    {
        EditorApplication.delayCall += RunAutoSetupOnce;
    }

    private static void RunAutoSetupOnce()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        if (!EditorPrefs.GetBool(PREF_KEY, false))
        {
            EditorPrefs.SetBool(PREF_KEY, true);
            Debug.Log("[LayerLabAutoSetup] Tự động thực hiện cài đặt Layer Lab UI...");
            SetupAll(isAuto: true);
        }
    }

    [MenuItem("Tools/Setup Layer Lab UI")]
    public static void ManualSetup()
    {
        SetupAll(isAuto: false);
    }

    public static void SetupAll(bool isAuto = false)
    {
        Debug.Log("[LayerLabAutoSetup] BẮT ĐẦU CÀI ĐẶT TOÀN BỘ GIAO DIỆN LAYER LAB...");
        EditorPrefs.SetBool(PREF_KEY, true);

        SetupMainMenuScene();
        SetupSampleScene();

        // Mở lại MainMenu làm Scene mặc định
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

        Debug.Log("[LayerLabAutoSetup] HOÀN TẤT THIẾT LẬP TẤT CẢ SCENE THÀNH CÔNG!");
        if (!isAuto && !Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("Layer Lab UI", "Đã tự động cài đặt toàn bộ giao diện Layer Lab GUI Pro thành công cho cả MainMenu và SampleScene!", "Tuyệt vời");
        }
    }

    // =========================================================================
    // MAIN MENU SCENE
    // =========================================================================

    public static void SetupMainMenuScene()
    {
        Debug.Log("[LayerLabAutoSetup] Đang thiết lập MainMenu.unity...");
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

        // 1. Canvas
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGo = new GameObject("MenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGo.GetComponent<Canvas>();
        }
        else
        {
            canvas.gameObject.name = "MenuCanvas";
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        // EventSystem
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }

        Transform cTrans = canvas.transform;

        // Xóa UI cũ nếu có để làm mới hoàn toàn
        string[] oldNames = { "MainMenuPanel", "ProfileCard_TopRight", "OnboardingPanel", "SettingsPanel", "LeaderboardPanel", "ShopModalPanel", "CoinDisplay", "Background" };
        foreach (var n in oldNames)
        {
            Transform t = cTrans.Find(n);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }

        // ===== BACKGROUND — Gradient overlay mờ ảo tinh tế =====
        GameObject bgFrame = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgFrame.transform.SetParent(cTrans, false);
        StretchFull(bgFrame);
        bgFrame.GetComponent<Image>().color = new Color(0.04f, 0.06f, 0.12f, 0.65f);

        // ===== 1. MainMenuPanel =====
        GameObject mmPanel = new GameObject("MainMenuPanel", typeof(RectTransform));
        mmPanel.transform.SetParent(cTrans, false);
        StretchFull(mmPanel);

        // ── Title Ribbon Layer Lab ──
        GameObject titleRibbon = InstantiatePrefab(TITLE_RIBBON_BLUE, mmPanel.transform, "Title_Ribbon");
        RectTransform ribbonRt = titleRibbon.GetComponent<RectTransform>();
        ribbonRt.anchorMin = new Vector2(0.5f, 1f);
        ribbonRt.anchorMax = new Vector2(0.5f, 1f);
        ribbonRt.pivot = new Vector2(0.5f, 1f);
        ribbonRt.anchoredPosition = new Vector2(0, -140);
        ribbonRt.sizeDelta = new Vector2(740, 160);

        // TitleText — Nằm chính giữa ribbon
        TextMeshProUGUI titleTmp = CreateTMPText(mmPanel.transform, "TitleText", "JELLY RUNNER 3D", 48f, Color.white, TextAlignmentOptions.Center, new Vector2(680, 80), new Vector2(0, -210));
        titleTmp.fontStyle = FontStyles.Bold;

        // ── Decorative divider dưới title ──
        GameObject divider1 = InstantiatePrefab(TITLE_DIVIDER, mmPanel.transform, "Divider_Title");
        SetAnchored(divider1, new Vector2(0.5f, 1f), new Vector2(0, -295), new Vector2(520, 25));

        // BestScoreText — Khung bo tròn nhỏ xinh (lồng Text vào trong Frame để căn chuẩn)
        GameObject bestScoreFrame = InstantiatePrefab(BORDER_ROUND03_NAVY, mmPanel.transform, "BestScoreFrame");
        SetAnchored(bestScoreFrame, new Vector2(0.5f, 1f), new Vector2(0, -345), new Vector2(440, 55));
        CreateTMPText(bestScoreFrame.transform, "BestScoreText", "🏆 KỶ LỤC: 0 ĐIỂM", 28f, new Color(1f, 0.88f, 0.3f), TextAlignmentOptions.Center, new Vector2(420, 50), Vector2.zero);

        // ── PlayButton — Nút chính CHƠI NGAY nổi bật ở tâm dưới màn hình ──
        GameObject playBtn = InstantiatePrefab(BTN_GREEN_225, mmPanel.transform, "PlayButton");
        RectTransform playRt = playBtn.GetComponent<RectTransform>();
        playRt.anchorMin = new Vector2(0.5f, 0f);
        playRt.anchorMax = new Vector2(0.5f, 0f);
        playRt.pivot = new Vector2(0.5f, 0f);
        playRt.anchoredPosition = new Vector2(0, 210);
        playRt.sizeDelta = new Vector2(460, 125);
        SetButtonText(playBtn, "CHƠI NGAY", 44f);

        // ── ButtonsGroup — Hàng nút phụ nằm ngang gọn gàng ở sát đáy màn hình ──
        GameObject btnGroup = new GameObject("ButtonsGroup", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        btnGroup.transform.SetParent(mmPanel.transform, false);
        RectTransform bgBtnRt = btnGroup.GetComponent<RectTransform>();
        bgBtnRt.anchorMin = new Vector2(0.5f, 0f);
        bgBtnRt.anchorMax = new Vector2(0.5f, 0f);
        bgBtnRt.pivot = new Vector2(0.5f, 0f);
        bgBtnRt.anchoredPosition = new Vector2(0, 65);
        bgBtnRt.sizeDelta = new Vector2(860, 110);
        HorizontalLayoutGroup btnHlg = btnGroup.GetComponent<HorizontalLayoutGroup>();
        btnHlg.childAlignment = TextAnchor.MiddleCenter;
        btnHlg.spacing = 16f;
        btnHlg.childControlWidth = false;
        btnHlg.childControlHeight = false;
        btnHlg.childForceExpandWidth = false;
        btnHlg.childForceExpandHeight = false;

        // ShopButton — Cam/Vàng
        GameObject shopBtn = InstantiatePrefab(BTN_ORANGE_195, btnGroup.transform, "ShopButton");
        shopBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 95);
        SetButtonText(shopBtn, "🛒 SHOP", 26f);

        // LeaderboardButton — Xanh dương
        GameObject lbBtn = InstantiatePrefab(BTN_BLUE_195, btnGroup.transform, "LeaderboardButton");
        lbBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 95);
        SetButtonText(lbBtn, "🏆 HẠNG", 26f);

        // SettingsButton — Tím
        GameObject setBtn = InstantiatePrefab(BTN_PURPLE_195, btnGroup.transform, "SettingsButton");
        setBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 95);
        SetButtonText(setBtn, "⚙️ CÀI ĐẶT", 26f);

        // QuitButton — Đỏ
        GameObject quitBtn = InstantiatePrefab(BTN_RED_145, btnGroup.transform, "QuitButton");
        quitBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(170, 95);
        SetButtonText(quitBtn, "🚪 THOÁT", 26f);

        // ===== 2. ProfileCard_TopRight =====
        GameObject pCard = new GameObject("ProfileCard_TopRight", typeof(RectTransform), typeof(Button));
        pCard.transform.SetParent(cTrans, false);
        RectTransform pcRt = pCard.GetComponent<RectTransform>();
        pcRt.anchorMin = new Vector2(1f, 1f);
        pcRt.anchorMax = new Vector2(1f, 1f);
        pcRt.pivot = new Vector2(1f, 1f);
        pcRt.anchoredPosition = new Vector2(-25, -30);
        pcRt.sizeDelta = new Vector2(340, 85);

        // Profile Frame visual — Gradient frame nhỏ
        GameObject frameObj = InstantiatePrefab(PROFILE_FRAME_BLUE, pCard.transform, "FrameVisual");
        RectTransform fRt = frameObj.GetComponent<RectTransform>();
        fRt.anchorMin = Vector2.zero;
        fRt.anchorMax = Vector2.one;
        fRt.sizeDelta = Vector2.zero;
        fRt.anchoredPosition = Vector2.zero;

        // AvatarFrame — Khung viền đẹp
        GameObject avFrame = new GameObject("AvatarFrame", typeof(RectTransform), typeof(Image));
        avFrame.transform.SetParent(pCard.transform, false);
        RectTransform avRt = avFrame.GetComponent<RectTransform>();
        avRt.anchorMin = new Vector2(0f, 0.5f);
        avRt.anchorMax = new Vector2(0f, 0.5f);
        avRt.pivot = new Vector2(0f, 0.5f);
        avRt.anchoredPosition = new Vector2(12, 0);
        avRt.sizeDelta = new Vector2(64, 64);
        avFrame.GetComponent<Image>().color = new Color(0.12f, 0.18f, 0.32f);

        // AvatarImage con — Hiển thị ẢNH THẬT (Sprite) không bị méo
        GameObject avImgObj = new GameObject("AvatarImage", typeof(RectTransform), typeof(Image));
        avImgObj.transform.SetParent(avFrame.transform, false);
        RectTransform avImgRt = avImgObj.GetComponent<RectTransform>();
        avImgRt.anchorMin = Vector2.zero;
        avImgRt.anchorMax = Vector2.one;
        avImgRt.sizeDelta = new Vector2(-6, -6);
        avImgRt.anchoredPosition = Vector2.zero;
        Image avImg = avImgObj.GetComponent<Image>();
        avImg.preserveAspect = true;
        Sprite defAvSp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Avatars/Avatar_0_KingJelly.png")
                      ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/Icon_ItemIcons/128/Icon_Crown.png");
        if (defAvSp != null) avImg.sprite = defAvSp;

        CreateTMPText(pCard.transform, "PlayerName", "Người Chơi", 24f, Color.white, TextAlignmentOptions.Left, new Vector2(210, 30), new Vector2(25, 14));
        CreateTMPText(pCard.transform, "SubText", "18 tuổi • 🇻🇳", 18f, new Color(0.7f, 0.85f, 1f), TextAlignmentOptions.Left, new Vector2(210, 24), new Vector2(25, -14));

        // CoinDisplay — Góc trên trái, khung coin đẹp
        GameObject coinFrame = InstantiatePrefab(BORDER_ROUND03_NAVY, cTrans, "CoinFrame");
        SetAnchored(coinFrame, new Vector2(0f, 1f), new Vector2(25, -30), new Vector2(230, 65));
        RectTransform cfRt = coinFrame.GetComponent<RectTransform>();
        cfRt.pivot = new Vector2(0f, 1f);
        CreateTMPText(coinFrame.transform, "CoinDisplay", "🪙 0 XU", 28f, new Color(1f, 0.88f, 0.25f), TextAlignmentOptions.Center, new Vector2(210, 55), Vector2.zero);

        // ===== 3. OnboardingPanel =====
        BuildOnboardingPanel(cTrans);

        // ===== 4. SettingsPanel =====
        BuildSettingsPanel(cTrans);

        // ===== 5. LeaderboardPanel =====
        BuildLeaderboardPanel(cTrans);

        // ===== 6. ShopModalPanel =====
        BuildShopModalPanel(cTrans);

        // ===== 7. MainMenuManager & ShopUI =====
        GameObject mmMgr = GameObject.Find("MainMenuManager");
        if (mmMgr == null) mmMgr = new GameObject("MainMenuManager");
        MainMenuUI mmUI = mmMgr.GetComponent<MainMenuUI>();
        if (mmUI == null) mmUI = mmMgr.AddComponent<MainMenuUI>();

        ShopUI shopUI = mmMgr.GetComponent<ShopUI>();
        if (shopUI == null) shopUI = mmMgr.AddComponent<ShopUI>();

        SerializedObject mmSo = new SerializedObject(mmUI);
        SerializedProperty shopProp = mmSo.FindProperty("shopUI");
        if (shopProp != null) shopProp.objectReferenceValue = shopUI;

        // Tự động gán 6 Avatar Sprites vào mảng customAvatarSprites trên Inspector của MainMenuUI
        string[] autoAvPaths = {
            "Assets/Resources/Avatars/Avatar_0_KingJelly.png",
            "Assets/Resources/Avatars/Avatar_1_CyberZap.png",
            "Assets/Resources/Avatars/Avatar_2_FireBlaze.png",
            "Assets/Resources/Avatars/Avatar_3_DiamondElite.png",
            "Assets/Resources/Avatars/Avatar_4_DevRocket.png",
            "Assets/Resources/Avatars/Avatar_5_PixelHacker.png"
        };
        string[] autoLlFallbacks = {
            "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/Icon_ItemIcons/128/Icon_Crown.png",
            "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/Icon_ItemIcons/128/Icon_Bolt.png",
            "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/Icon_ItemIcons/128/Icon_Heart.png",
            "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/Icon_ItemIcons/128/Icon_Gem03_Diamond_Blue.png",
            "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/Icon_ItemIcons/128/Icon_Missile.png",
            "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Demo/Demo_Character/Character_Sample01_m.png"
        };
        SerializedProperty avSpritesProp = mmSo.FindProperty("customAvatarSprites");
        if (avSpritesProp != null && avSpritesProp.isArray)
        {
            avSpritesProp.arraySize = 6;
            for (int i = 0; i < 6; i++)
            {
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(autoAvPaths[i])
                         ?? AssetDatabase.LoadAssetAtPath<Sprite>(autoLlFallbacks[i]);
                if (sp != null)
                {
                    avSpritesProp.GetArrayElementAtIndex(i).objectReferenceValue = sp;
                }
            }
        }
        mmSo.ApplyModifiedProperties();

        // ===== 8. UIParticleFXManager =====
        SetupParticleFXManager(mmMgr);

        if (!System.IO.Directory.Exists("Assets/Prefabs/UI"))
            System.IO.Directory.CreateDirectory("Assets/Prefabs/UI");
        PrefabUtility.SaveAsPrefabAsset(canvas.gameObject, "Assets/Prefabs/UI/MenuCanvas.prefab");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[LayerLabAutoSetup] MainMenu.unity hoàn tất thiết lập thành công!");
    }

    // ── ONBOARDING PANEL ──
    private static void BuildOnboardingPanel(Transform cTrans)
    {
        GameObject onbPanel = new GameObject("OnboardingPanel", typeof(RectTransform));
        onbPanel.transform.SetParent(cTrans, false);
        StretchFull(onbPanel);

        CreateDarkOverlay(onbPanel.transform, "DarkBackdrop");

        // Popup card chính
        GameObject onbCard = InstantiatePrefab(POPUP01_NAVY, onbPanel.transform, "Card");
        SetAnchored(onbCard, new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(880, 1300));

        // Title ribbon
        GameObject titleRib = InstantiatePrefab(TITLE_RIBBON_GREEN, onbPanel.transform, "TitleRibbon");
        SetAnchored(titleRib, new Vector2(0.5f, 0.5f), new Vector2(0, 580), new Vector2(650, 130));

        CreateTMPText(onbPanel.transform, "Title", "HỒ SƠ NGƯỜI CHƠI", 44f, Color.white, TextAlignmentOptions.Center, new Vector2(600, 55), new Vector2(0, 575));
        CreateTMPText(onbPanel.transform, "Desc", "Hãy chọn tên, tuổi và avatar đại diện", 24f, new Color(0.72f, 0.82f, 0.96f), TextAlignmentOptions.Center, new Vector2(720, 36), new Vector2(0, 500));

        // ── Divider ──
        GameObject div1 = InstantiatePrefab(TITLE_DIVIDER, onbPanel.transform, "Div1");
        SetAnchored(div1, new Vector2(0.5f, 0.5f), new Vector2(0, 465), new Vector2(700, 20));

        // Name Section
        CreateTMPText(onbPanel.transform, "NameLabel", "TÊN CỦA BẠN", 28f, new Color(0.6f, 0.8f, 1f), TextAlignmentOptions.Left, new Vector2(700, 36), new Vector2(0, 420));
        GameObject nameInputObj = InstantiatePrefab(INPUTFIELD_NAVY, onbPanel.transform, "OnbNameInput");
        SetAnchored(nameInputObj, new Vector2(0.5f, 0.5f), new Vector2(0, 365), new Vector2(720, 75));
        var tmpInput = nameInputObj.GetComponent<TMP_InputField>();
        if (tmpInput == null) tmpInput = nameInputObj.AddComponent<TMP_InputField>();
        var childTmp = nameInputObj.GetComponentInChildren<TextMeshProUGUI>();
        if (childTmp != null) tmpInput.textComponent = childTmp;

        // ── Divider ──
        GameObject div2 = InstantiatePrefab(TITLE_DIVIDER, onbPanel.transform, "Div2");
        SetAnchored(div2, new Vector2(0.5f, 0.5f), new Vector2(0, 310), new Vector2(700, 20));

        // Age Section — Label + Value ngang hàng
        CreateTMPText(onbPanel.transform, "AgeLabel", "ĐỘ TUỔI", 28f, new Color(0.6f, 0.8f, 1f), TextAlignmentOptions.Left, new Vector2(400, 36), new Vector2(-100, 270));
        CreateTMPText(onbPanel.transform, "AgeVal", "18 tuổi", 30f, new Color(1f, 0.88f, 0.25f), TextAlignmentOptions.Right, new Vector2(200, 36), new Vector2(270, 270));
        GameObject ageSliderObj = InstantiatePrefab(SLIDER_HANDLE_YELLOW, onbPanel.transform, "OnbAgeSlider");
        SetAnchored(ageSliderObj, new Vector2(0.5f, 0.5f), new Vector2(0, 210), new Vector2(720, 50));

        // ── Divider ──
        GameObject div3 = InstantiatePrefab(TITLE_DIVIDER, onbPanel.transform, "Div3");
        SetAnchored(div3, new Vector2(0.5f, 0.5f), new Vector2(0, 160), new Vector2(700, 20));

        // Language Section
        CreateTMPText(onbPanel.transform, "LangLabel", "NGÔN NGỮ", 28f, new Color(0.6f, 0.8f, 1f), TextAlignmentOptions.Left, new Vector2(700, 36), new Vector2(0, 120));
        GameObject langGroup = new GameObject("LangGroup", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        langGroup.transform.SetParent(onbPanel.transform, false);
        SetAnchored(langGroup, new Vector2(0.5f, 0.5f), new Vector2(0, 55), new Vector2(720, 80));
        HorizontalLayoutGroup hlg = langGroup.GetComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = 25f;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        GameObject langVn = InstantiatePrefab(BTN_GREEN_175, langGroup.transform, "LangVN");
        langVn.GetComponent<RectTransform>().sizeDelta = new Vector2(335, 75);
        SetButtonText(langVn, "🇻🇳 Tiếng Việt", 26f);

        GameObject langEn = InstantiatePrefab(BTN_GRAY_175, langGroup.transform, "LangEN");
        langEn.GetComponent<RectTransform>().sizeDelta = new Vector2(335, 75);
        SetButtonText(langEn, "🇬🇧 English", 26f);

        // ── Divider ──
        GameObject div4 = InstantiatePrefab(TITLE_DIVIDER, onbPanel.transform, "Div4");
        SetAnchored(div4, new Vector2(0.5f, 0.5f), new Vector2(0, -5), new Vector2(700, 20));

        // Avatar Section
        CreateTMPText(onbPanel.transform, "AvatarLabel", "BIỂU TƯỢNG ĐẠI DIỆN", 28f, new Color(0.6f, 0.8f, 1f), TextAlignmentOptions.Left, new Vector2(700, 36), new Vector2(0, -40));
        GameObject avGrid = new GameObject("AvatarGrid", typeof(RectTransform), typeof(GridLayoutGroup));
        avGrid.transform.SetParent(onbPanel.transform, false);
        SetAnchored(avGrid, new Vector2(0.5f, 0.5f), new Vector2(0, -140), new Vector2(720, 110));
        GridLayoutGroup glg = avGrid.GetComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(105, 105);
        glg.spacing = new Vector2(15, 15);
        glg.childAlignment = TextAnchor.MiddleCenter;

        string[] avGridPaths = {
            "Assets/Resources/Avatars/Avatar_0_KingJelly.png",
            "Assets/Resources/Avatars/Avatar_1_CyberZap.png",
            "Assets/Resources/Avatars/Avatar_2_FireBlaze.png",
            "Assets/Resources/Avatars/Avatar_3_DiamondElite.png",
            "Assets/Resources/Avatars/Avatar_4_DevRocket.png",
            "Assets/Resources/Avatars/Avatar_5_PixelHacker.png"
        };
        string[] avGridFallbacks = {
            "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/Icon_ItemIcons/128/Icon_Crown.png",
            "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/Icon_ItemIcons/128/Icon_Bolt.png",
            "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/Icon_ItemIcons/128/Icon_Heart.png",
            "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/Icon_ItemIcons/128/Icon_Gem03_Diamond_Blue.png",
            "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/Icon_ItemIcons/128/Icon_Missile.png",
            "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Demo/Demo_Character/Character_Sample01_m.png"
        };

        for (int i = 0; i < 6; i++)
        {
            GameObject avItem = InstantiatePrefab(BTN_SQUARE_NAVY_03, avGrid.transform, "AvatarItem_" + i);
            avItem.GetComponent<RectTransform>().sizeDelta = new Vector2(105, 105);
            Button itemBtn = avItem.GetComponent<Button>();
            if (itemBtn == null) avItem.AddComponent<Button>();

            // AvatarImage con hiển thị ẢNH THẬT (Sprite)
            GameObject imgObj = new GameObject("AvatarImage", typeof(RectTransform), typeof(Image));
            imgObj.transform.SetParent(avItem.transform, false);
            RectTransform imgRt = imgObj.GetComponent<RectTransform>();
            imgRt.anchorMin = new Vector2(0.12f, 0.12f);
            imgRt.anchorMax = new Vector2(0.88f, 0.88f);
            imgRt.sizeDelta = Vector2.zero;
            imgRt.anchoredPosition = Vector2.zero;

            Image imgComp = imgObj.GetComponent<Image>();
            imgComp.preserveAspect = true;
            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(avGridPaths[i])
                     ?? AssetDatabase.LoadAssetAtPath<Sprite>(avGridFallbacks[i]);
            if (sp != null) imgComp.sprite = sp;

            // Viền vàng nổi bật khi được chọn
            GameObject border = new GameObject("SelectBorder", typeof(RectTransform), typeof(Image));
            border.transform.SetParent(avItem.transform, false);
            RectTransform bRt = border.GetComponent<RectTransform>();
            bRt.anchorMin = Vector2.zero;
            bRt.anchorMax = Vector2.one;
            bRt.sizeDelta = new Vector2(10, 10);
            bRt.anchoredPosition = Vector2.zero;
            Image bImg = border.GetComponent<Image>();
            bImg.color = new Color(1f, 0.88f, 0.15f, 0.95f);
            bImg.raycastTarget = false;
            border.SetActive(i == 0);
        }

        // DoneBtn — Lớn, nổi bật
        GameObject doneBtn = InstantiatePrefab(BTN_GREEN_225, onbPanel.transform, "DoneBtn");
        SetAnchored(doneBtn, new Vector2(0.5f, 0.5f), new Vector2(0, -330), new Vector2(400, 110));
        SetButtonText(doneBtn, "✓ XÁC NHẬN", 38f);

        onbPanel.SetActive(false);
    }

    // ── SETTINGS PANEL ──
    private static void BuildSettingsPanel(Transform cTrans)
    {
        GameObject setPanel = new GameObject("SettingsPanel", typeof(RectTransform));
        setPanel.transform.SetParent(cTrans, false);
        StretchFull(setPanel);

        CreateDarkOverlay(setPanel.transform, "DarkBackdrop");

        // Popup card — Divided style đẹp hơn
        GameObject setCard = InstantiatePrefab(POPUP02_DIVIDED_NAVY, setPanel.transform, "Card");
        SetAnchored(setCard, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(850, 1100));

        // Title
        GameObject setTitleRib = InstantiatePrefab(TITLE_OVAL_BLUE, setPanel.transform, "TitleOval");
        SetAnchored(setTitleRib, new Vector2(0.5f, 0.5f), new Vector2(0, 470), new Vector2(420, 90));
        CreateTMPText(setPanel.transform, "SettingsTitle", "CÀI ĐẶT", 42f, Color.white, TextAlignmentOptions.Center, new Vector2(400, 55), new Vector2(0, 468));

        // Volume
        CreateTMPText(setPanel.transform, "VolLabel", "🔊 ÂM LƯỢNG", 28f, new Color(0.7f, 0.9f, 1f), TextAlignmentOptions.Left, new Vector2(700, 36), new Vector2(0, 350));
        GameObject volSlider = InstantiatePrefab(SLIDER_HANDLE_WHITE, setPanel.transform, "VolumeSlider");
        SetAnchored(volSlider, new Vector2(0.5f, 0.5f), new Vector2(0, 295), new Vector2(680, 48));

        // ── Divider ──
        GameObject ds1 = InstantiatePrefab(TITLE_DIVIDER, setPanel.transform, "DS1");
        SetAnchored(ds1, new Vector2(0.5f, 0.5f), new Vector2(0, 250), new Vector2(680, 20));

        // Sensitivity
        CreateTMPText(setPanel.transform, "SensLabel", "👆 ĐỘ NHẠY VUỐT", 28f, new Color(0.7f, 0.9f, 1f), TextAlignmentOptions.Left, new Vector2(700, 36), new Vector2(0, 210));
        GameObject sensSlider = InstantiatePrefab(SLIDER_HANDLE_WHITE, setPanel.transform, "SensSlider");
        SetAnchored(sensSlider, new Vector2(0.5f, 0.5f), new Vector2(0, 155), new Vector2(680, 48));

        // ── Divider ──
        GameObject ds2 = InstantiatePrefab(TITLE_DIVIDER, setPanel.transform, "DS2");
        SetAnchored(ds2, new Vector2(0.5f, 0.5f), new Vector2(0, 110), new Vector2(680, 20));

        // Bloom
        CreateTMPText(setPanel.transform, "BloomLabel", "✨ ĐỘ PHÁT SÁNG NEON", 28f, new Color(0.5f, 0.95f, 1f), TextAlignmentOptions.Left, new Vector2(700, 36), new Vector2(0, 70));
        GameObject bloomSlider = InstantiatePrefab(SLIDER_HANDLE_YELLOW, setPanel.transform, "BloomSlider");
        SetAnchored(bloomSlider, new Vector2(0.5f, 0.5f), new Vector2(0, 15), new Vector2(680, 48));

        // ── Divider ──
        GameObject ds3 = InstantiatePrefab(TITLE_DIVIDER, setPanel.transform, "DS3");
        SetAnchored(ds3, new Vector2(0.5f, 0.5f), new Vector2(0, -30), new Vector2(680, 20));

        // Language Toggle
        GameObject langToggleBtn = InstantiatePrefab(BTN_GREEN_175, setPanel.transform, "LangToggleBtn");
        SetAnchored(langToggleBtn, new Vector2(0.5f, 0.5f), new Vector2(0, -100), new Vector2(420, 90));
        SetButtonText(langToggleBtn, "🌐 ĐỔI NGÔN NGỮ", 28f);

        // Reset Best
        GameObject resetBestBtn = InstantiatePrefab(BTN_RED_175, setPanel.transform, "ResetBestBtn");
        SetAnchored(resetBestBtn, new Vector2(0.5f, 0.5f), new Vector2(0, -220), new Vector2(400, 85));
        SetButtonText(resetBestBtn, "🗑 XÓA KỶ LỤC", 28f);

        // Close — Circle button góc phải trên
        GameObject closeSetBtn = InstantiatePrefab(BTN_CIRCLE_DARK_128, setPanel.transform, "CloseSettingsBtn");
        SetAnchored(closeSetBtn, new Vector2(0.5f, 0.5f), new Vector2(370, 480), new Vector2(75, 75));

        setPanel.SetActive(false);
    }

    // ── LEADERBOARD PANEL ──
    private static void BuildLeaderboardPanel(Transform cTrans)
    {
        GameObject lbPanel = new GameObject("LeaderboardPanel", typeof(RectTransform));
        lbPanel.transform.SetParent(cTrans, false);
        StretchFull(lbPanel);

        CreateDarkOverlay(lbPanel.transform, "DarkBackdrop");

        // Popup card
        GameObject lbCard = InstantiatePrefab(POPUP01_NAVY, lbPanel.transform, "Card");
        SetAnchored(lbCard, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860, 1200));

        // Title Flag — Vàng vinh quang
        GameObject titleFlag = InstantiatePrefab(TITLE_RIBBON_YELLOW, lbPanel.transform, "TitleFlag");
        SetAnchored(titleFlag, new Vector2(0.5f, 0.5f), new Vector2(0, 510), new Vector2(620, 130));

        CreateTMPText(lbPanel.transform, "LBTitle", "🏆 BẢNG XẾP HẠNG", 40f, Color.white, TextAlignmentOptions.Center, new Vector2(580, 50), new Vector2(0, 508));

        // Entries Frame — Khung riêng cho danh sách
        GameObject entriesFrame = InstantiatePrefab(FRAME_ROUND20_TRANSPARENT_NAVY, lbPanel.transform, "EntriesFrame");
        SetAnchored(entriesFrame, new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(740, 780));
        CreateTMPText(lbPanel.transform, "LBEntries", "Đang tải...", 30f, new Color(0.9f, 0.95f, 1f), TextAlignmentOptions.TopLeft, new Vector2(680, 720), new Vector2(0, 30));

        // Close
        GameObject closeLbBtn = InstantiatePrefab(BTN_CIRCLE_DARK_128, lbPanel.transform, "CloseLBBtn");
        SetAnchored(closeLbBtn, new Vector2(0.5f, 0.5f), new Vector2(370, 530), new Vector2(75, 75));

        lbPanel.SetActive(false);
    }

    // ── SHOP MODAL PANEL ──
    private static void BuildShopModalPanel(Transform cTrans)
    {
        GameObject shopPanel = new GameObject("ShopModalPanel", typeof(RectTransform));
        shopPanel.transform.SetParent(cTrans, false);
        StretchFull(shopPanel);

        CreateDarkOverlay(shopPanel.transform, "DarkBackdrop");

        // Card chính — Gradient đẹp
        GameObject shopCard = InstantiatePrefab(FRAME_ROUND12_GRADIENT_NAVY, shopPanel.transform, "Card");
        SetAnchored(shopCard, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000, 1550));

        // Title
        GameObject shopTitleRib = InstantiatePrefab(TITLE_RIBBON_YELLOW, shopPanel.transform, "TitleRibbon");
        SetAnchored(shopTitleRib, new Vector2(0.5f, 0.5f), new Vector2(0, 670), new Vector2(560, 120));
        CreateTMPText(shopPanel.transform, "Title", "CỬA HÀNG", 44f, Color.white, TextAlignmentOptions.Center, new Vector2(520, 50), new Vector2(0, 668));

        // Coin display
        CreateTMPText(shopPanel.transform, "ShopCoins", "🪙 0 XU", 34f, new Color(1f, 0.88f, 0.25f), TextAlignmentOptions.Right, new Vector2(280, 50), new Vector2(310, 670));

        // TabGroup — Horizontal Layout
        GameObject tabGroup = new GameObject("TabGroup", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        tabGroup.transform.SetParent(shopPanel.transform, false);
        SetAnchored(tabGroup, new Vector2(0.5f, 0.5f), new Vector2(0, 575), new Vector2(940, 85));
        HorizontalLayoutGroup thlg = tabGroup.GetComponent<HorizontalLayoutGroup>();
        thlg.childAlignment = TextAnchor.MiddleCenter;
        thlg.spacing = 15f;
        thlg.childControlWidth = false;
        thlg.childControlHeight = false;

        GameObject tabPlayer = InstantiatePrefab(BTN_BLUE_175, tabGroup.transform, "Tab_Player");
        tabPlayer.GetComponent<RectTransform>().sizeDelta = new Vector2(290, 78);
        SetButtonText(tabPlayer, "👤 NHÂN VẬT", 28f);

        GameObject tabWall = InstantiatePrefab(BTN_GRAY_175, tabGroup.transform, "Tab_Wall");
        tabWall.GetComponent<RectTransform>().sizeDelta = new Vector2(290, 78);
        SetButtonText(tabWall, "🧱 TƯỜNG", 28f);

        GameObject tabEffect = InstantiatePrefab(BTN_GRAY_175, tabGroup.transform, "Tab_Effect");
        tabEffect.GetComponent<RectTransform>().sizeDelta = new Vector2(290, 78);
        SetButtonText(tabEffect, "✨ KỸ NĂNG", 28f);

        // ScrollView — Khu vực cuộn danh sách vật phẩm (dùng RectMask2D để không bị lỗi stencil/mask)
        GameObject scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
        scrollObj.transform.SetParent(shopPanel.transform, false);
        SetAnchored(scrollObj, new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(940, 1000));

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        viewport.transform.SetParent(scrollObj.transform, false);
        StretchFull(viewport);

        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        RectTransform contRt = content.GetComponent<RectTransform>();
        contRt.anchorMin = new Vector2(0f, 1f);
        contRt.anchorMax = new Vector2(1f, 1f);
        contRt.pivot = new Vector2(0.5f, 1f);
        contRt.sizeDelta = new Vector2(0, 0);
        VerticalLayoutGroup vlg = content.GetComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.spacing = 18f;
        vlg.padding = new RectOffset(15, 15, 15, 15);
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect sr = scrollObj.GetComponent<ScrollRect>();
        sr.content = contRt;
        sr.viewport = viewport.GetComponent<RectTransform>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped;

        // Sinh sẵn các thẻ card mẫu trực quan trong Scene Editor
        BuildDefaultShopCardsInEditor(content.transform);

        // Close Button — Đỏ phía dưới
        GameObject closeShopBtn = InstantiatePrefab(BTN_RED_175, shopPanel.transform, "CloseBtn");
        SetAnchored(closeShopBtn, new Vector2(0.5f, 0.5f), new Vector2(0, -660), new Vector2(380, 95));
        SetButtonText(closeShopBtn, "← QUAY LẠI", 32f);

        shopPanel.SetActive(false);
    }

    private static void BuildDefaultShopCardsInEditor(Transform parent)
    {
        var sampleItems = new (string name, string status, Color statusCol, Color previewCol, string btnText, string btnPrefab)[]
        {
            ("Neon Cyan", "★ ĐANG TRANG BỊ", new Color(0.25f, 0.95f, 0.55f), new Color(0f, 0.9f, 1f), "ĐANG DÙNG", BTN_GREEN_175),
            ("Gold Royale", "🪙 150 XU", new Color(1f, 0.85f, 0.2f), new Color(1f, 0.85f, 0.15f), "MUA", BTN_ORANGE_195),
            ("Galaxy Violet", "🪙 250 XU", new Color(1f, 0.85f, 0.2f), new Color(0.65f, 0.15f, 1f), "MUA", BTN_ORANGE_195),
            ("Magma Fire", "🪙 350 XU", new Color(1f, 0.85f, 0.2f), new Color(1f, 0.3f, 0.05f), "MUA", BTN_ORANGE_195),
            ("Emerald Jade", "🪙 500 XU", new Color(1f, 0.85f, 0.2f), new Color(0.05f, 0.95f, 0.45f), "MUA", BTN_ORANGE_195),
            ("Void Shadow", "🪙 750 XU", new Color(1f, 0.85f, 0.2f), new Color(0.2f, 0.15f, 0.35f), "MUA", BTN_ORANGE_195)
        };

        for (int i = 0; i < sampleItems.Length; i++)
        {
            var item = sampleItems[i];
            bool isEquipped = (i == 0);

            GameObject card = new GameObject("Card_" + item.name, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            card.transform.SetParent(parent, false);
            RectTransform cardRt = card.GetComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(880, 150);

            Image cardBg = card.GetComponent<Image>();
            cardBg.color = isEquipped
                ? new Color(0.06f, 0.24f, 0.18f, 0.95f)
                : new Color(0.08f, 0.11f, 0.18f, 0.92f);

            LayoutElement le = card.GetComponent<LayoutElement>();
            le.minHeight = 150f;
            le.preferredHeight = 150f;
            le.minWidth = 880f;
            le.preferredWidth = 880f;
            le.flexibleWidth = 1f;

            if (isEquipped)
            {
                GameObject border = new GameObject("EquippedBorder", typeof(RectTransform), typeof(Image));
                border.transform.SetParent(card.transform, false);
                RectTransform bRt = border.GetComponent<RectTransform>();
                bRt.anchorMin = Vector2.zero;
                bRt.anchorMax = Vector2.one;
                bRt.offsetMin = new Vector2(-3, -3);
                bRt.offsetMax = new Vector2(3, 3);
                bRt.SetAsFirstSibling();
                Image bImg = border.GetComponent<Image>();
                bImg.color = new Color(0.18f, 0.95f, 0.55f, 0.7f);
            }

            // Preview Box Frame
            GameObject previewFrame = new GameObject("PreviewFrame", typeof(RectTransform), typeof(Image));
            previewFrame.transform.SetParent(card.transform, false);
            RectTransform pfRt = previewFrame.GetComponent<RectTransform>();
            pfRt.anchorMin = new Vector2(0f, 0.5f);
            pfRt.anchorMax = new Vector2(0f, 0.5f);
            pfRt.pivot = new Vector2(0f, 0.5f);
            pfRt.anchoredPosition = new Vector2(20, 0);
            pfRt.sizeDelta = new Vector2(110, 110);
            Image pfImg = previewFrame.GetComponent<Image>();
            pfImg.color = new Color(0.15f, 0.18f, 0.28f, 0.95f);

            GameObject prevColor = new GameObject("PreviewColor", typeof(RectTransform), typeof(Image));
            prevColor.transform.SetParent(previewFrame.transform, false);
            RectTransform pcRt = prevColor.GetComponent<RectTransform>();
            pcRt.anchorMin = Vector2.zero;
            pcRt.anchorMax = Vector2.one;
            pcRt.offsetMin = new Vector2(8, 8);
            pcRt.offsetMax = new Vector2(-8, -8);
            Image pcImg = prevColor.GetComponent<Image>();
            pcImg.color = item.previewCol;

            // Name
            CreateTMPText(card.transform, "ItemName", item.name, 34f, Color.white, TextAlignmentOptions.Left,
                new Vector2(400, 46), new Vector2(0, 0));
            RectTransform nameRt = card.transform.Find("ItemName").GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0f, 0.5f);
            nameRt.anchorMax = new Vector2(0f, 0.5f);
            nameRt.pivot = new Vector2(0f, 0.5f);
            nameRt.anchoredPosition = new Vector2(150, 22);

            // Status
            CreateTMPText(card.transform, "ItemStatus", item.status, 26f, item.statusCol, TextAlignmentOptions.Left,
                new Vector2(400, 36), new Vector2(0, 0));
            RectTransform statRt = card.transform.Find("ItemStatus").GetComponent<RectTransform>();
            statRt.anchorMin = new Vector2(0f, 0.5f);
            statRt.anchorMax = new Vector2(0f, 0.5f);
            statRt.pivot = new Vector2(0f, 0.5f);
            statRt.anchoredPosition = new Vector2(150, -22);

            // Button
            GameObject actionBtn = InstantiatePrefab(item.btnPrefab, card.transform, "ActionBtn");
            SetAnchored(actionBtn, new Vector2(1f, 0.5f), new Vector2(-120, 0), new Vector2(210, 82));
            SetButtonText(actionBtn, item.btnText, 28f);
        }
    }

    // =========================================================================
    // SAMPLE SCENE (IN-GAME)
    // =========================================================================

    public static void SetupSampleScene()
    {
        Debug.Log("[LayerLabAutoSetup] Đang thiết lập SampleScene.unity...");
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");

        // 1. Canvas
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGo = new GameObject("GameCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGo.GetComponent<Canvas>();
        }
        else
        {
            canvas.gameObject.name = "GameCanvas";
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        Transform cTrans = canvas.transform;

        // Xóa các panel cũ
        string[] oldPanels = { "HUDPanel", "GameOverPanel", "PausePanel" };
        foreach (var p in oldPanels)
        {
            Transform t = cTrans.Find(p);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }

        // ===== 1. HUDPanel =====
        GameObject hudPanel = new GameObject("HUDPanel", typeof(RectTransform));
        hudPanel.transform.SetParent(cTrans, false);
        StretchFull(hudPanel);

        // Top HUD Bar — Thanh gradient navy cao cấp ôm trọn top màn hình
        GameObject topBar = InstantiatePrefab(PANEL_FRAME_ROUND_NAVY, hudPanel.transform, "TopBar");
        RectTransform tbRt = topBar.GetComponent<RectTransform>();
        tbRt.anchorMin = new Vector2(0.5f, 1f);
        tbRt.anchorMax = new Vector2(0.5f, 1f);
        tbRt.pivot = new Vector2(0.5f, 1f);
        tbRt.anchoredPosition = new Vector2(0, -20);
        tbRt.sizeDelta = new Vector2(1020, 100);

        // ScoreText — Nổi bật chính giữa TopBar
        TextMeshProUGUI scoreTmp = CreateTMPText(hudPanel.transform, "ScoreText", "0", 54f, Color.white, TextAlignmentOptions.Center, new Vector2(300, 70), new Vector2(0, -65));
        scoreTmp.fontStyle = FontStyles.Bold;

        // CoinBadge — Khung coin bo tròn góc trái trong TopBar
        GameObject coinBadge = InstantiatePrefab(BORDER_ROUND03_NAVY, hudPanel.transform, "CoinBadge");
        SetAnchored(coinBadge, new Vector2(0f, 1f), new Vector2(35, -35), new Vector2(190, 55));
        RectTransform cbRt = coinBadge.GetComponent<RectTransform>();
        cbRt.pivot = new Vector2(0f, 1f);
        TextMeshProUGUI coinTmp = CreateTMPText(coinBadge.transform, "CoinHUDText", "🪙 0", 28f, new Color(1f, 0.88f, 0.25f), TextAlignmentOptions.Center, new Vector2(170, 45), Vector2.zero);

        // DistanceText — Phía dưới thanh TopBar nhẹ nhàng
        TextMeshProUGUI distTmp = CreateTMPText(hudPanel.transform, "DistanceText", "0m", 28f, new Color(0.6f, 0.92f, 1f), TextAlignmentOptions.Center, new Vector2(250, 40), new Vector2(0, -135));

        // PauseButton — Nút tròn nhỏ gọn góc phải TopBar
        GameObject pauseBtn = InstantiatePrefab(BTN_CIRCLE_WHITE_128, hudPanel.transform, "PauseButton");
        RectTransform pbRt = pauseBtn.GetComponent<RectTransform>();
        pbRt.anchorMin = new Vector2(1f, 1f);
        pbRt.anchorMax = new Vector2(1f, 1f);
        pbRt.pivot = new Vector2(1f, 1f);
        pbRt.anchoredPosition = new Vector2(-35, -30);
        pbRt.sizeDelta = new Vector2(75, 75);

        // ===== 2. GameOverPanel =====
        GameObject goPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        goPanel.transform.SetParent(cTrans, false);
        StretchFull(goPanel);

        CreateDarkOverlay(goPanel.transform, "DarkBackdrop");

        GameObject goCard = InstantiatePrefab(POPUP01_NAVY, goPanel.transform, "Card");
        SetAnchored(goCard, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(880, 1200));

        // GameOverTitle — Flag đỏ ấn tượng
        GameObject titleFlag = InstantiatePrefab(TITLE_FLAG_RED, goPanel.transform, "GameOverTitle");
        SetAnchored(titleFlag, new Vector2(0.5f, 0.5f), new Vector2(0, 480), new Vector2(600, 140));
        TextMeshProUGUI goTitleTmp = titleFlag.GetComponentInChildren<TextMeshProUGUI>();
        if (goTitleTmp != null)
        {
            goTitleTmp.text = "KẾT THÚC";
            goTitleTmp.fontSize = 44f;
            TMP_FontAsset font = GetFont();
            if (font != null) goTitleTmp.font = font;
        }

        // Score — Lớn, vàng, nổi bật
        TextMeshProUGUI goScore = CreateTMPText(goPanel.transform, "GameOverScore", "ĐIỂM: 0", 52f, new Color(1f, 0.88f, 0.3f), TextAlignmentOptions.Center, new Vector2(700, 70), new Vector2(0, 340));
        goScore.fontStyle = FontStyles.Bold;

        // BestScore
        CreateTMPText(goPanel.transform, "BestScore", "KỶ LỤC: 0", 36f, Color.white, TextAlignmentOptions.Center, new Vector2(600, 48), new Vector2(0, 260));

        // Divider
        GameObject dgo = InstantiatePrefab(TITLE_DIVIDER, goPanel.transform, "DivGO");
        SetAnchored(dgo, new Vector2(0.5f, 0.5f), new Vector2(0, 220), new Vector2(650, 20));

        // LeaderboardMini — Khung riêng cho top scores
        GameObject lbMiniFrame = InstantiatePrefab(FRAME_ROUND20_TRANSPARENT_NAVY, goPanel.transform, "LBMiniFrame");
        SetAnchored(lbMiniFrame, new Vector2(0.5f, 0.5f), new Vector2(0, 70), new Vector2(700, 260));
        TextMeshProUGUI lbMini = CreateTMPText(goPanel.transform, "LeaderboardMini", "TOP ĐIỂM CAO", 28f, new Color(0.85f, 0.92f, 1f), TextAlignmentOptions.Center, new Vector2(640, 220), new Vector2(0, 70));

        // Buttons — VerticalLayout
        GameObject goBtns = new GameObject("GameOverBtns", typeof(RectTransform), typeof(VerticalLayoutGroup));
        goBtns.transform.SetParent(goPanel.transform, false);
        SetAnchored(goBtns, new Vector2(0.5f, 0.5f), new Vector2(0, -210), new Vector2(450, 260));
        VerticalLayoutGroup goVlg = goBtns.GetComponent<VerticalLayoutGroup>();
        goVlg.childAlignment = TextAnchor.UpperCenter;
        goVlg.spacing = 22f;
        goVlg.childControlWidth = false;
        goVlg.childControlHeight = false;

        // RestartButton
        GameObject restartBtn = InstantiatePrefab(BTN_GREEN_225, goBtns.transform, "RestartButton");
        restartBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 110);
        SetButtonText(restartBtn, "🔄 CHƠI LẠI", 38f);

        // MenuButton
        GameObject menuBtn = InstantiatePrefab(BTN_GRAY_195, goBtns.transform, "MenuButton");
        menuBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(340, 90);
        SetButtonText(menuBtn, "🏠 MENU", 32f);

        // Particle FX
        GameObject partFx = InstantiatePrefab(FX_SPREAD_STAR, goPanel.transform, "ParticleFX");
        if (partFx.GetComponent<RectTransform>() != null)
            partFx.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 340);

        goPanel.SetActive(false);

        // ===== 3. PausePanel =====
        GameObject pausePanel = new GameObject("PausePanel", typeof(RectTransform));
        pausePanel.transform.SetParent(cTrans, false);
        StretchFull(pausePanel);

        CreateDarkOverlay(pausePanel.transform, "DarkBackdrop");

        GameObject pauseCard = InstantiatePrefab(POPUP_FULLWIDTH_NAVY, pausePanel.transform, "Card");
        SetAnchored(pauseCard, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(850, 1000));

        // Title — Flag xanh dương
        GameObject pauseTitleFlag = InstantiatePrefab(TITLE_FLAG_BLUE, pausePanel.transform, "TitleFlag");
        SetAnchored(pauseTitleFlag, new Vector2(0.5f, 0.5f), new Vector2(0, 400), new Vector2(480, 120));
        TextMeshProUGUI pauseTitle = CreateTMPText(pausePanel.transform, "PauseTitle", "TẠM DỪNG", 44f, Color.white, TextAlignmentOptions.Center, new Vector2(440, 50), new Vector2(0, 398));

        // Buttons
        GameObject pBtns = new GameObject("PauseBtns", typeof(RectTransform), typeof(VerticalLayoutGroup));
        pBtns.transform.SetParent(pausePanel.transform, false);
        SetAnchored(pBtns, new Vector2(0.5f, 0.5f), new Vector2(0, 180), new Vector2(420, 260));
        VerticalLayoutGroup pVlg = pBtns.GetComponent<VerticalLayoutGroup>();
        pVlg.childAlignment = TextAnchor.UpperCenter;
        pVlg.spacing = 20f;
        pVlg.childControlWidth = false;
        pVlg.childControlHeight = false;

        // ResumeButton
        GameObject resumeBtn = InstantiatePrefab(BTN_SKY_225, pBtns.transform, "ResumeButton");
        resumeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 110);
        SetButtonText(resumeBtn, "▶ TIẾP TỤC", 38f);

        // PauseRestartBtn
        GameObject pRstBtn = InstantiatePrefab(BTN_GREEN_195, pBtns.transform, "PauseRestartBtn");
        pRstBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(360, 95);
        SetButtonText(pRstBtn, "🔄 CHƠI LẠI", 34f);

        // ── Bloom slider ──
        GameObject bloomDiv = InstantiatePrefab(TITLE_DIVIDER, pausePanel.transform, "BloomDiv");
        SetAnchored(bloomDiv, new Vector2(0.5f, 0.5f), new Vector2(0, -15), new Vector2(650, 20));

        TextMeshProUGUI pauseBloomLabel = CreateTMPText(pausePanel.transform, "PauseBloomLabel", "✨ Độ phát sáng Neon", 26f, new Color(0.5f, 0.95f, 1f), TextAlignmentOptions.Left, new Vector2(650, 36), new Vector2(0, -55));
        GameObject pBloomSlider = InstantiatePrefab(SLIDER_HANDLE_YELLOW, pausePanel.transform, "PauseBloomSlider");
        SetAnchored(pBloomSlider, new Vector2(0.5f, 0.5f), new Vector2(0, -110), new Vector2(650, 48));

        // ── Divider ──
        GameObject pd2 = InstantiatePrefab(TITLE_DIVIDER, pausePanel.transform, "PauseDiv2");
        SetAnchored(pd2, new Vector2(0.5f, 0.5f), new Vector2(0, -160), new Vector2(650, 20));

        // PauseMenuBtn
        GameObject pMenuBtn = InstantiatePrefab(BTN_GRAY_195, pausePanel.transform, "PauseMenuBtn");
        SetAnchored(pMenuBtn, new Vector2(0.5f, 0.5f), new Vector2(0, -230), new Vector2(340, 90));
        SetButtonText(pMenuBtn, "🏠 MENU", 32f);

        pausePanel.SetActive(false);

        // ===== 4. UIManager & UIParticleFXManager =====
        GameObject uiMgrGo = GameObject.Find("UIManager");
        if (uiMgrGo == null) uiMgrGo = new GameObject("UIManager");
        UIManager uiMgr = uiMgrGo.GetComponent<UIManager>();
        if (uiMgr == null) uiMgr = uiMgrGo.AddComponent<UIManager>();

        // Wire serialized references
        SerializedObject uiSo = new SerializedObject(uiMgr);
        SetField(uiSo, "scoreText", scoreTmp);
        SetField(uiSo, "distanceText", distTmp);
        SetField(uiSo, "coinHUDText", coinTmp);
        SetField(uiSo, "pauseButton", pauseBtn.GetComponent<Button>());

        SetField(uiSo, "gameOverPanel", goPanel);
        SetField(uiSo, "gameOverTitleText", goTitleTmp);
        SetField(uiSo, "gameOverScoreText", goScore);
        SetField(uiSo, "bestScoreText", lbMini);  // Temporary — BestScore text in game over
        SetField(uiSo, "leaderboardText", lbMini);
        SetField(uiSo, "restartButton", restartBtn.GetComponent<Button>());
        SetField(uiSo, "menuButton", menuBtn.GetComponent<Button>());
        SetField(uiSo, "gameOverParticleFX", partFx);

        SetField(uiSo, "pausePanel", pausePanel);
        SetField(uiSo, "pauseTitleText", pauseTitle);
        SetField(uiSo, "resumeButton", resumeBtn.GetComponent<Button>());
        SetField(uiSo, "pauseRestartButton", pRstBtn.GetComponent<Button>());
        SetField(uiSo, "pauseMenuButton", pMenuBtn.GetComponent<Button>());
        SetField(uiSo, "pauseBloomLabelText", pauseBloomLabel);
        SetField(uiSo, "pauseBloomSlider", pBloomSlider.GetComponent<Slider>());

        // Fix: Also find BestScore text separately
        TextMeshProUGUI bestScoreTmp = goPanel.transform.Find("BestScore")?.GetComponent<TextMeshProUGUI>();
        if (bestScoreTmp == null)
        {
            // Create it since we have it in hierarchy
            bestScoreTmp = CreateTMPText(goPanel.transform, "BestScoreFixed", "KỶ LỤC: 0", 36f, Color.white, TextAlignmentOptions.Center, new Vector2(600, 48), new Vector2(0, 260));
        }
        SetField(uiSo, "bestScoreText", bestScoreTmp);

        uiSo.ApplyModifiedProperties();

        SetupParticleFXManager(uiMgrGo);

        if (!System.IO.Directory.Exists("Assets/Prefabs/UI"))
            System.IO.Directory.CreateDirectory("Assets/Prefabs/UI");
        PrefabUtility.SaveAsPrefabAsset(canvas.gameObject, "Assets/Prefabs/UI/GameCanvas.prefab");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[LayerLabAutoSetup] SampleScene.unity hoàn tất thiết lập thành công!");
    }

    // =========================================================================
    // UTILITY HELPERS
    // =========================================================================

    private static void SetField(SerializedObject so, string propertyName, Object obj)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null) prop.objectReferenceValue = obj;
    }

    private static void SetupParticleFXManager(GameObject targetGo)
    {
        UIParticleFXManager pfx = targetGo.GetComponent<UIParticleFXManager>();
        if (pfx == null) pfx = targetGo.AddComponent<UIParticleFXManager>();

        SerializedObject so = new SerializedObject(pfx);
        SetProperty(so, "fxSpreadStar", FX_SPREAD_STAR);
        SetProperty(so, "fxRotateLight", FX_ROTATE_LIGHT);
        SetProperty(so, "fxShinesGlow", FX_SHINES_GLOW);
        SetProperty(so, "fxSparkleStarWhite", FX_SPARKLE_WHITE);
        SetProperty(so, "fxSpreadCircle", FX_SPREAD_CIRCLE);
        so.ApplyModifiedProperties();
    }

    private static void SetProperty(SerializedObject so, string propertyName, string assetPath)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null)
        {
            prop.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        }
    }

    private static GameObject InstantiatePrefab(string path, Transform parent, string newName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"[LayerLabAutoSetup] Prefab không tìm thấy tại đường dẫn: {path}");
            GameObject fallback = new GameObject(newName, typeof(RectTransform));
            fallback.transform.SetParent(parent, false);
            return fallback;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        if (instance != null)
        {
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            instance.name = newName;
            return instance;
        }

        GameObject go = Object.Instantiate(prefab, parent);
        go.name = newName;
        return go;
    }

    private static TMP_FontAsset GetFont()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PATH_FONT_OUTLINE);
        if (font == null) font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PATH_FONT_REGULAR);
        if (font == null) font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        return font;
    }

    private static TextMeshProUGUI CreateTMPText(Transform parent, string name, string text, float fontSize, Color color, TextAlignmentOptions alignment, Vector2 size, Vector2 anchoredPos)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;

        TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.raycastTarget = false;
        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        TMP_FontAsset font = GetFont();
        if (font != null) tmp.font = font;
        return tmp;
    }

    private static void SetButtonText(GameObject btnObj, string text, float fontSize = 32f)
    {
        TextMeshProUGUI tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.raycastTarget = false;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            TMP_FontAsset font = GetFont();
            if (font != null) tmp.font = font;
        }
    }

    /// <summary>Tạo dark overlay mờ phía sau popup</summary>
    private static GameObject CreateDarkOverlay(Transform parent, string name)
    {
        GameObject overlay = new GameObject(name, typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(parent, false);
        StretchFull(overlay);
        Image img = overlay.GetComponent<Image>();
        img.color = new Color(0.01f, 0.02f, 0.06f, 0.82f);
        img.raycastTarget = true;
        return overlay;
    }

    /// <summary>Stretch RectTransform kín toàn bộ parent</summary>
    private static void StretchFull(GameObject obj)
    {
        RectTransform rt = obj.GetComponent<RectTransform>();
        if (rt == null) rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    /// <summary>Set anchor trung tâm + vị trí + kích thước cho RectTransform</summary>
    private static void SetAnchored(GameObject obj, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        RectTransform rt = obj.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }
}

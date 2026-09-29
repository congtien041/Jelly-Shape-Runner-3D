using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// MainMenuUI v3 — Thiết kế lại cho Portrait Mode (9:16).
/// Loại bỏ emoji Unicode, dùng màu sắc thay cho icon avatar.
/// Giao diện đơn giản, sạch sẽ, responsive.
///
/// KHÔNG CÒN TỰ SINH UI BẰNG CODE — toàn bộ UI phải được thiết kế sẵn trên Scene
/// bằng các Prefab của Layer Lab. Script chỉ tìm và kết nối.
/// Hỗ trợ animation mở/đóng panel (UIAnimator).
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Jelly Runner/Main Menu UI")]
public class MainMenuUI : MonoBehaviour
{
    [Header("--- Shop UI Reference ---")]
    [SerializeField] private ShopUI shopUI;

    [Header("--- Custom Avatars (Dev Có Thể Thêm/Thay Ảnh Khác Tùy Ý) ---")]
    [Tooltip("Dev có thể kéo thả 6 ảnh đại diện tùy ý vào đây! Nếu để trống, game tự động nạp ảnh từ Assets/Resources/Avatars/")]
    [SerializeField] public Sprite[] customAvatarSprites = new Sprite[6];

    // Profile Top-Right
    private GameObject profileCard;
    private TextMeshProUGUI profileNameText;
    private TextMeshProUGUI profileSubText;
    private Image profileAvatarImage;
    private TextMeshProUGUI profileAvatarIconText;
    private TextMeshProUGUI coinDisplayText;

    // Onboarding Panel References
    private GameObject onboardingPanel;
    private TextMeshProUGUI onbTitleText;
    private TextMeshProUGUI onbDescText;
    private TextMeshProUGUI onbNameLabelText;
    private TextMeshProUGUI onbAgeLabelText;
    private TextMeshProUGUI onbLangLabelText;
    private TextMeshProUGUI onbAvatarLabelText;
    private TextMeshProUGUI onbDoneBtnText;
    private TMP_InputField onbNameInput;
    private Slider onbAgeSlider;
    private TextMeshProUGUI onbAgeValueText;
    private int selectedAvatarIndex = 0;
    private Image[] avatarSelectImages;
    private Button onbVnBtn;
    private Button onbEnBtn;

    // Main Menu Text References
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI bestScoreText;
    private TextMeshProUGUI playBtnText;
    private TextMeshProUGUI shopBtnText;
    private TextMeshProUGUI leaderboardBtnText;
    private TextMeshProUGUI settingsBtnText;
    private TextMeshProUGUI quitBtnText;

    // Settings & Leaderboard Panels
    private GameObject settingsPanel;
    private TextMeshProUGUI settingsTitleText;
    private TextMeshProUGUI volLabelText;
    private TextMeshProUGUI sensLabelText;
    private TextMeshProUGUI bloomLabelText;
    private TextMeshProUGUI soundBtnText;
    private TextMeshProUGUI resetBtnText;
    private TextMeshProUGUI langBtnText;
    private TextMeshProUGUI closeSettingsBtnText;

    private GameObject leaderboardPanel;
    private TextMeshProUGUI lbTitleText;
    private TextMeshProUGUI leaderboardDisplayText;
    private TextMeshProUGUI closeLBBtnText;

    // Avatar Presets — Định nghĩa 6 biểu tượng đại diện phong cách Dev/Gamer
    [System.Serializable]
    public struct AvatarData
    {
        public string id;
        public string name;
        public string resourcePath;
        public Color themeColor;

        // Tương thích ngược với các công cụ SceneUIHierarchyGenerator
        public Color color => themeColor;
        public string icon => name;
    }

    public static readonly AvatarData[] AvatarList = new AvatarData[]
    {
        new AvatarData { id = "king_jelly", name = "King Jelly", resourcePath = "Avatars/Avatar_0_KingJelly", themeColor = new Color(1.0f, 0.82f, 0.10f) },
        new AvatarData { id = "cyber_zap", name = "Cyber Zap", resourcePath = "Avatars/Avatar_1_CyberZap", themeColor = new Color(0.0f, 0.90f, 1.00f) },
        new AvatarData { id = "fire_blaze", name = "Fire Blaze", resourcePath = "Avatars/Avatar_2_FireBlaze", themeColor = new Color(1.0f, 0.35f, 0.10f) },
        new AvatarData { id = "diamond_elite", name = "Diamond Elite", resourcePath = "Avatars/Avatar_3_DiamondElite", themeColor = new Color(0.15f, 0.95f, 0.85f) },
        new AvatarData { id = "dev_rocket", name = "Dev Rocket", resourcePath = "Avatars/Avatar_4_DevRocket", themeColor = new Color(0.70f, 0.25f, 1.00f) },
        new AvatarData { id = "pixel_hacker", name = "Pixel Hacker", resourcePath = "Avatars/Avatar_5_PixelHacker", themeColor = new Color(0.20f, 0.95f, 0.45f) }
    };

    /// <summary>
    /// Lấy Sprite cho Avatar: Ưu tiên ảnh Dev gán trong Inspector -> sau đó tới Resources -> fallback Sprite Editor
    /// </summary>
    public Sprite GetAvatarSprite(int index)
    {
        if (index < 0 || index >= AvatarList.Length) index = 0;

        // 1. Kiểm tra ảnh do Dev kéo thả trực tiếp vào Inspector
        if (customAvatarSprites != null && index < customAvatarSprites.Length && customAvatarSprites[index] != null)
        {
            return customAvatarSprites[index];
        }

        // 2. Nạp từ Assets/Resources/Avatars/
        Sprite loaded = Resources.Load<Sprite>(AvatarList[index].resourcePath);
        if (loaded != null) return loaded;

        // 3. Fallback trong Unity Editor
#if UNITY_EDITOR
        string[] editorPaths = {
            "Assets/Resources/Avatars/Avatar_0_KingJelly.png",
            "Assets/Resources/Avatars/Avatar_1_CyberZap.png",
            "Assets/Resources/Avatars/Avatar_2_FireBlaze.png",
            "Assets/Resources/Avatars/Avatar_3_DiamondElite.png",
            "Assets/Resources/Avatars/Avatar_4_DevRocket.png",
            "Assets/Resources/Avatars/Avatar_5_PixelHacker.png"
        };
        string[] llFallback = {
            "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/Icon_ItemIcons/128/Icon_Crown.png",
            "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/Icon_ItemIcons/128/Icon_Bolt.png",
            "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/Icon_ItemIcons/128/Icon_Heart.png",
            "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/Icon_ItemIcons/128/Icon_Gem03_Diamond_Blue.png",
            "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/Icon_ItemIcons/128/Icon_Missile.png",
            "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Demo/Demo_Character/Character_Sample01_m.png"
        };
        if (index < editorPaths.Length)
        {
            Sprite edSp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(editorPaths[index])
                       ?? UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(llFallback[index]);
            if (edSp != null) return edSp;
        }
#endif
        return null;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        // Đảm bảo có LocalizationManager
        if (LocalizationManager.Instance == null)
        {
            GameObject locObj = new GameObject("LocalizationManager");
            locObj.AddComponent<LocalizationManager>();
        }

        // Đảm bảo có CurrencyManager
        if (CurrencyManager.Instance == null)
        {
            GameObject curObj = new GameObject("CurrencyManager");
            curObj.AddComponent<CurrencyManager>();
        }

        // Lắng nghe sự kiện tiền thay đổi
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCoinsChanged.RemoveListener(OnCoinsChangedCallback);
            CurrencyManager.Instance.OnCoinsChanged.AddListener(OnCoinsChangedCallback);
        }

        // Tặng 500 vàng khởi đầu để người chơi trải nghiệm mua Shop ngay
        if (!PlayerPrefs.HasKey("TotalCoins"))
        {
            PlayerPrefs.SetInt("TotalCoins", 500);
            PlayerPrefs.Save();
        }

        // Đảm bảo có ShopUI
        if (shopUI == null)
            shopUI = GetComponent<ShopUI>() ?? gameObject.AddComponent<ShopUI>();

        // Đảm bảo có ShopManager
        if (ShopManager.Instance == null)
        {
            GameObject shopMgrObj = new GameObject("ShopManager");
            shopMgrObj.AddComponent<ShopManager>();
        }

        // Tìm UI đã được thiết kế sẵn trên Scene (Layer Lab Prefab)
        FindUIReferences();

        // Tự động gán âm thanh click nảy cho 100% các nút trên MainMenu (Profile, Play, Shop, Settings, Leaderboard, Avatar items, Close...)
        GameObject canvasObj = GameObject.Find("MenuCanvas");
        if (canvasObj != null)
        {
            AudioManager.AutoHookAllButtons(canvasObj);
        }

        // Lắng nghe sự kiện đổi ngôn ngữ để dịch lại toàn bộ UI tức thì
        LocalizationManager.OnLanguageChanged += RefreshAllLocalizedTexts;

        // Cập nhật text ban đầu
        RefreshAllLocalizedTexts();

        // Kiểm tra Onboarding lần đầu
        if (PlayerPrefs.GetInt("HasCompletedOnboarding", 0) == 0)
        {
            ShowOnboardingPanel(true);
        }
        else
        {
            UpdateProfileDisplay();
        }
    }

    private void OnEnable()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCoinsChanged.RemoveListener(OnCoinsChangedCallback);
            CurrencyManager.Instance.OnCoinsChanged.AddListener(OnCoinsChangedCallback);
        }
        UpdateProfileDisplay();
    }

    private void OnDisable()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCoinsChanged.RemoveListener(OnCoinsChangedCallback);
        }
    }

    private void OnCoinsChangedCallback(int newCoins)
    {
        UpdateCoinDisplay();
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCoinsChanged.RemoveListener(OnCoinsChangedCallback);
        }
        LocalizationManager.OnLanguageChanged -= RefreshAllLocalizedTexts;
    }

    // =========================================================================
    // TÌM UI TRÊN SCENE (Layer Lab Prefab đã kéo thả)
    // =========================================================================

    private void FindUIReferences()
    {
        GameObject canvasObj = GameObject.Find("MenuCanvas");
        if (canvasObj == null)
        {
            Debug.LogWarning("[MainMenuUI] Khong tim thay 'MenuCanvas' tren Scene! " +
                "Hay keo tha Prefab Layer Lab vao Scene theo huong dan.");
            return;
        }

        Transform cTrans = canvasObj.transform;

        // ===== MAIN MENU PANEL =====
        Transform mPanel = cTrans.Find("MainMenuPanel");
        if (mPanel != null)
        {
            titleText = mPanel.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
            bestScoreText = mPanel.Find("BestScoreText")?.GetComponent<TextMeshProUGUI>()
                         ?? mPanel.Find("BestScoreFrame/BestScoreText")?.GetComponent<TextMeshProUGUI>();

            Transform bGroup = mPanel.Find("ButtonsGroup") ?? mPanel;

            Button playBtn = mPanel.Find("PlayButton")?.GetComponent<Button>() ?? bGroup.Find("PlayButton")?.GetComponent<Button>();
            if (playBtn != null)
            {
                playBtnText = playBtn.GetComponentInChildren<TextMeshProUGUI>();
                playBtn.onClick.RemoveAllListeners();
                playBtn.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClickSound(); PlayGame(); });
            }

            Button shopBtn = bGroup.Find("ShopButton")?.GetComponent<Button>();
            if (shopBtn != null)
            {
                shopBtnText = shopBtn.GetComponentInChildren<TextMeshProUGUI>();
                shopBtn.onClick.RemoveAllListeners();
                shopBtn.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClickSound(); if (shopUI != null) shopUI.ShowShop(); });
            }

            Button lbBtn = bGroup.Find("LeaderboardButton")?.GetComponent<Button>();
            if (lbBtn != null)
            {
                leaderboardBtnText = lbBtn.GetComponentInChildren<TextMeshProUGUI>();
                lbBtn.onClick.RemoveAllListeners();
                lbBtn.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClickSound(); ToggleLeaderboard(); });
            }

            Button setBtn = bGroup.Find("SettingsButton")?.GetComponent<Button>();
            if (setBtn != null)
            {
                settingsBtnText = setBtn.GetComponentInChildren<TextMeshProUGUI>();
                setBtn.onClick.RemoveAllListeners();
                setBtn.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClickSound(); ToggleSettings(); });
            }

            Button quitBtn = bGroup.Find("QuitButton")?.GetComponent<Button>();
            if (quitBtn != null)
            {
                quitBtnText = quitBtn.GetComponentInChildren<TextMeshProUGUI>();
                quitBtn.onClick.RemoveAllListeners();
                quitBtn.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClickSound(); QuitGame(); });
            }

            // ===== NHÂN VẬT 3D CLONE PREVIEW TẠI MENU =====
            Transform previewTrans = mPanel.Find("CharacterPreviewArea");
            RectTransform pRt = null;
            if (previewTrans != null)
            {
                pRt = previewTrans as RectTransform;
                if (pRt == null)
                {
                    Destroy(previewTrans.gameObject);
                    previewTrans = null;
                }
            }

            if (previewTrans == null)
            {
                GameObject previewObj = new GameObject("CharacterPreviewArea", typeof(RectTransform));
                previewObj.transform.SetParent(mPanel, false);
                pRt = previewObj.GetComponent<RectTransform>();
                pRt.anchorMin = new Vector2(0.5f, 0.5f);
                pRt.anchorMax = new Vector2(0.5f, 0.5f);
                pRt.pivot = new Vector2(0.5f, 0.5f);
                pRt.anchoredPosition = new Vector2(0f, 45f);
                pRt.sizeDelta = new Vector2(560f, 560f);

                RawImage rawImg = previewObj.AddComponent<RawImage>();
                rawImg.color = Color.white;
                rawImg.raycastTarget = true;

                previewObj.AddComponent<MenuCharacterPreview>();

                // Gợi ý vuốt xoay 360 độ
                GameObject hintObj = new GameObject("SwipeHintText", typeof(RectTransform));
                hintObj.transform.SetParent(previewObj.transform, false);
                RectTransform hintRt = hintObj.GetComponent<RectTransform>();
                hintRt.anchorMin = new Vector2(0.5f, 0f);
                hintRt.anchorMax = new Vector2(0.5f, 0f);
                hintRt.pivot = new Vector2(0.5f, 0.5f);
                hintRt.anchoredPosition = new Vector2(0f, 20f);
                hintRt.sizeDelta = new Vector2(400f, 36f);

                TextMeshProUGUI hintText = hintObj.AddComponent<TextMeshProUGUI>();
                if (TMP_Settings.defaultFontAsset != null) hintText.font = TMP_Settings.defaultFontAsset;
                hintText.text = "◄ Vuốt để xoay 360° ►";
                hintText.fontSize = 20;
                hintText.color = new Color(0.15f, 0.85f, 1f, 0.65f);
                hintText.alignment = TextAlignmentOptions.Center;
                hintText.fontStyle = FontStyles.Bold;
                hintText.raycastTarget = false;

                previewObj.transform.SetSiblingIndex(2);
            }
        }

        // ===== PROFILE CARD TOP-RIGHT =====
        Transform pCard = cTrans.Find("ProfileCard_TopRight");
        if (pCard != null)
        {
            profileCard = pCard.gameObject;
            profileNameText = pCard.Find("PlayerName")?.GetComponent<TextMeshProUGUI>();
            profileSubText = pCard.Find("SubText")?.GetComponent<TextMeshProUGUI>();
            profileAvatarImage = pCard.Find("AvatarFrame/AvatarImage")?.GetComponent<Image>()
                              ?? pCard.Find("AvatarFrame")?.GetComponent<Image>();
            profileAvatarIconText = pCard.Find("AvatarFrame/Icon")?.GetComponent<TextMeshProUGUI>();
            coinDisplayText = cTrans.Find("CoinFrame/CoinDisplay")?.GetComponent<TextMeshProUGUI>()
                           ?? cTrans.Find("CoinDisplay")?.GetComponent<TextMeshProUGUI>()
                           ?? pCard.Find("CoinDisplay")?.GetComponent<TextMeshProUGUI>();

            Button pBtn = pCard.GetComponent<Button>();
            if (pBtn != null)
            {
                pBtn.onClick.RemoveAllListeners();
                pBtn.onClick.AddListener(() => ShowOnboardingPanel(false));
            }
        }

        // ===== ONBOARDING PANEL =====
        Transform onb = cTrans.Find("OnboardingPanel");
        if (onb != null)
        {
            onboardingPanel = onb.gameObject;
            onbTitleText = onb.Find("Title")?.GetComponent<TextMeshProUGUI>();
            onbDescText = onb.Find("Desc")?.GetComponent<TextMeshProUGUI>();
            onbNameLabelText = onb.Find("NameLabel")?.GetComponent<TextMeshProUGUI>();
            onbNameInput = onb.Find("OnbNameInput")?.GetComponent<TMP_InputField>();
            onbAgeLabelText = onb.Find("AgeLabel")?.GetComponent<TextMeshProUGUI>();
            onbAgeValueText = onb.Find("AgeVal")?.GetComponent<TextMeshProUGUI>();
            onbAgeSlider = onb.Find("OnbAgeSlider")?.GetComponent<Slider>();
            onbLangLabelText = onb.Find("LangLabel")?.GetComponent<TextMeshProUGUI>();
            onbAvatarLabelText = onb.Find("AvatarLabel")?.GetComponent<TextMeshProUGUI>();

            // Language Buttons
            onbVnBtn = onb.Find("LangGroup/LangVN")?.GetComponent<Button>();
            if (onbVnBtn != null)
            {
                onbVnBtn.onClick.RemoveAllListeners();
                onbVnBtn.onClick.AddListener(() =>
                {
                    if (LocalizationManager.Instance != null)
                        LocalizationManager.Instance.SetLanguage(LocalizationManager.Language.Vietnamese);
                    UpdateLangButtonColors();
                });
            }

            onbEnBtn = onb.Find("LangGroup/LangEN")?.GetComponent<Button>();
            if (onbEnBtn != null)
            {
                onbEnBtn.onClick.RemoveAllListeners();
                onbEnBtn.onClick.AddListener(() =>
                {
                    if (LocalizationManager.Instance != null)
                        LocalizationManager.Instance.SetLanguage(LocalizationManager.Language.English);
                    UpdateLangButtonColors();
                });
            }

            // Avatar Grid — Hỗ trợ ẢNH THẬT (Sprite) và Click đổi Avatar mượt mà
            Transform grid = onb.Find("AvatarGrid");
            if (grid != null)
            {
                avatarSelectImages = new Image[AvatarList.Length];
                selectedAvatarIndex = Mathf.Clamp(PlayerPrefs.GetInt("PlayerAvatarIndex", 0), 0, AvatarList.Length - 1);
                for (int i = 0; i < AvatarList.Length; i++)
                {
                    int idx = i;
                    Transform item = grid.Find("AvatarItem_" + i);
                    if (item != null)
                    {
                        // 1. Image hiển thị Sprite Avatar
                        Image avImg = item.Find("AvatarImage")?.GetComponent<Image>() ?? item.GetComponent<Image>();
                        Sprite sp = GetAvatarSprite(idx);
                        if (sp != null && avImg != null)
                        {
                            avImg.sprite = sp;
                            avImg.color = Color.white;
                        }
                        avatarSelectImages[i] = avImg;

                        // Ẩn Text Emoji cũ nếu còn sót lại
                        Transform emojiChild = item.Find("Emoji");
                        if (emojiChild != null) emojiChild.gameObject.SetActive(false);

                        // 2. Button Click Listener
                        Button btn = item.GetComponent<Button>();
                        if (btn == null) btn = item.gameObject.AddComponent<Button>();
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() =>
                        {
                            selectedAvatarIndex = idx;
                            AudioManager.Instance?.PlayButtonClickSound();
                            UpdateAvatarSelectionUI();
                        });
                    }
                }
            }

            // Done Button
            Button doneBtn = onb.Find("DoneBtn")?.GetComponent<Button>();
            if (doneBtn != null)
            {
                onbDoneBtnText = doneBtn.GetComponentInChildren<TextMeshProUGUI>();
                doneBtn.onClick.RemoveAllListeners();
                doneBtn.onClick.AddListener(SaveAndCloseOnboarding);
            }

            // Close Button (✕)
            Button closeOnbBtn = onb.Find("CloseBtn")?.GetComponent<Button>() ?? onb.Find("Card/CloseBtn")?.GetComponent<Button>();
            if (closeOnbBtn != null)
            {
                closeOnbBtn.onClick.RemoveAllListeners();
                closeOnbBtn.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayButtonClickSound();
                    if (onboardingPanel != null) UIAnimator.PopOutAndDisable(onboardingPanel);
                });
            }

            // Age Slider — Chuẩn hóa min 0 max 1, tính tuổi chính xác và cập nhật text tức thì
            if (onbAgeSlider != null)
            {
                onbAgeSlider.minValue = 0f;
                onbAgeSlider.maxValue = 1f;
                onbAgeSlider.onValueChanged.RemoveAllListeners();
                onbAgeSlider.onValueChanged.AddListener(v =>
                {
                    int age = Mathf.RoundToInt(Mathf.Lerp(5f, 70f, Mathf.Clamp01(v)));
                    if (onbAgeValueText != null)
                    {
                        onbAgeValueText.text = LocalizationManager.Get("onb_age_val", age);
                        onbAgeValueText.enableWordWrapping = false;
                        onbAgeValueText.overflowMode = TextOverflowModes.Overflow;
                    }
                });
            }

            onboardingPanel.SetActive(false);
        }

        // ===== SETTINGS PANEL =====
        Transform set = cTrans.Find("SettingsPanel");
        if (set != null)
        {
            settingsPanel = set.gameObject;
            settingsTitleText = set.Find("SettingsTitle")?.GetComponent<TextMeshProUGUI>();
            volLabelText = set.Find("VolLabel")?.GetComponent<TextMeshProUGUI>();
            sensLabelText = set.Find("SensLabel")?.GetComponent<TextMeshProUGUI>();

            Slider volSlider = set.Find("VolumeSlider")?.GetComponent<Slider>();
            if (volSlider != null)
            {
                volSlider.value = PlayerPrefs.GetFloat("Volume", 1f);
                volSlider.onValueChanged.RemoveAllListeners();
                volSlider.onValueChanged.AddListener(v =>
                {
                    PlayerPrefs.SetFloat("Volume", v);
                    AudioListener.volume = v;
                    if (AudioManager.Instance != null) AudioManager.Instance.SetBGMVolume(v);
                });
            }

            Slider sensSlider = set.Find("SensSlider")?.GetComponent<Slider>();
            if (sensSlider != null)
            {
                sensSlider.value = PlayerPrefs.GetFloat("DragSensitivity", 3.5f) / 7f;
                sensSlider.onValueChanged.RemoveAllListeners();
                sensSlider.onValueChanged.AddListener(v => PlayerPrefs.SetFloat("DragSensitivity", v * 7f));
            }

            // Độ phát sáng Neon (Bloom)
            bloomLabelText = set.Find("BloomLabel")?.GetComponent<TextMeshProUGUI>();
            Slider bloomSlider = set.Find("BloomSlider")?.GetComponent<Slider>();
            if (bloomSlider != null)
            {
                float currentBloom = PlayerPrefs.GetFloat("PP_BloomIntensity", 1.35f);
                bloomSlider.value = currentBloom / 2.5f;
                bloomSlider.onValueChanged.RemoveAllListeners();
                bloomSlider.onValueChanged.AddListener(v =>
                {
                    float intensity = v * 2.5f;
                    if (GlobalVolumeManager.Instance != null)
                        GlobalVolumeManager.Instance.SetBloomIntensity(intensity);
                });
            }

            Button langToggleBtn = set.Find("LangToggleBtn")?.GetComponent<Button>();
            if (langToggleBtn != null)
            {
                langBtnText = langToggleBtn.GetComponentInChildren<TextMeshProUGUI>();
                langToggleBtn.onClick.RemoveAllListeners();
                langToggleBtn.onClick.AddListener(() =>
                {
                    if (LocalizationManager.Instance != null) LocalizationManager.Instance.ToggleLanguage();
                });
            }

            Button resetBtn = set.Find("ResetBestBtn")?.GetComponent<Button>();
            if (resetBtn != null)
            {
                resetBtnText = resetBtn.GetComponentInChildren<TextMeshProUGUI>();
                resetBtn.onClick.RemoveAllListeners();
                resetBtn.onClick.AddListener(() =>
                {
                    PlayerPrefs.SetInt("BestScore", 0);
                    if (LeaderboardManager.Instance != null) LeaderboardManager.Instance.ClearLeaderboard();
                    PlayerPrefs.Save();
                    if (bestScoreText != null) bestScoreText.text = LocalizationManager.Get("menu_best_score", 0);
                });
            }

            Button closeSetBtn = set.Find("CloseSettingsBtn")?.GetComponent<Button>();
            if (closeSetBtn != null)
            {
                closeSettingsBtnText = closeSetBtn.GetComponentInChildren<TextMeshProUGUI>();
                closeSetBtn.onClick.RemoveAllListeners();
                closeSetBtn.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayPopupCloseSound();
                    UIAnimator.PopOutAndDisable(settingsPanel);
                });
            }

            settingsPanel.SetActive(false);
        }

        // ===== LEADERBOARD PANEL =====
        Transform lb = cTrans.Find("LeaderboardPanel");
        if (lb != null)
        {
            leaderboardPanel = lb.gameObject;
            lbTitleText = lb.Find("LBTitle")?.GetComponent<TextMeshProUGUI>();
            leaderboardDisplayText = lb.Find("LBEntries")?.GetComponent<TextMeshProUGUI>();
            Button closeLB = lb.Find("CloseLBBtn")?.GetComponent<Button>();
            if (closeLB != null)
            {
                closeLBBtnText = closeLB.GetComponentInChildren<TextMeshProUGUI>();
                closeLB.onClick.RemoveAllListeners();
                closeLB.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayPopupCloseSound();
                    UIAnimator.PopOutAndDisable(leaderboardPanel);
                });
            }

            leaderboardPanel.SetActive(false);
        }
    }

    // =========================================================================
    // ĐIỀU HƯỚNG
    // =========================================================================

    public void PlayGame()
    {
        AudioManager.Instance?.PlayGameStartSound();
        SceneManager.LoadScene("SampleScene");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // =========================================================================
    // PROFILE CARD
    // =========================================================================

    private void UpdateProfileDisplay()
    {
        string pName = PlayerPrefs.GetString("PlayerName", "Jelly Runner");
        int age = PlayerPrefs.GetInt("PlayerAge", 18);
        int avIdx = Mathf.Clamp(PlayerPrefs.GetInt("PlayerAvatarIndex", 0), 0, AvatarList.Length - 1);

        if (profileNameText != null)
        {
            profileNameText.text = pName;
            profileNameText.enableWordWrapping = false;
            profileNameText.overflowMode = TextOverflowModes.Ellipsis;
        }
        if (profileSubText != null)
        {
            profileSubText.text = LocalizationManager.Get("profile_age_format", age);
            profileSubText.enableWordWrapping = false;
            profileSubText.overflowMode = TextOverflowModes.Overflow;
        }

        Sprite currentSp = GetAvatarSprite(avIdx);
        if (profileAvatarImage != null)
        {
            if (currentSp != null)
            {
                profileAvatarImage.sprite = currentSp;
                profileAvatarImage.color = Color.white;
            }
            else
            {
                profileAvatarImage.color = AvatarList[avIdx].themeColor;
            }
        }
        if (profileAvatarIconText != null)
        {
            profileAvatarIconText.gameObject.SetActive(currentSp == null);
        }

        // Cập nhật Vàng hiển thị
        UpdateCoinDisplay();

        // Cập nhật Điểm Kỷ Lục
        if (bestScoreText != null)
        {
            bestScoreText.text = LocalizationManager.Get("menu_best_score", PlayerPrefs.GetInt("BestScore", 0));
        }
    }

    public void UpdateCoinDisplay()
    {
        int totalCoins = CurrencyManager.Instance != null ? CurrencyManager.Instance.TotalCoins : PlayerPrefs.GetInt("TotalCoins", 0);
        if (coinDisplayText != null)
        {
            coinDisplayText.text = LocalizationManager.Get("menu_coin_format", totalCoins);
        }
    }

    private void UpdateAvatarSelectionUI()
    {
        if (onboardingPanel == null) return;
        Transform grid = onboardingPanel.transform.Find("AvatarGrid");
        if (grid == null) return;

        for (int i = 0; i < AvatarList.Length; i++)
        {
            Transform item = grid.Find("AvatarItem_" + i);
            if (item == null) continue;

            bool isSelected = (i == selectedAvatarIndex);
            Transform border = item.Find("SelectBorder");
            if (border != null)
            {
                border.gameObject.SetActive(isSelected);
            }

            item.localScale = isSelected ? Vector3.one * 1.15f : Vector3.one;

            // Đảm bảo Sprite Avatar hiển thị rõ ràng
            Image avImg = item.Find("AvatarImage")?.GetComponent<Image>() ?? item.GetComponent<Image>();
            Sprite sp = GetAvatarSprite(i);
            if (sp != null && avImg != null)
            {
                avImg.sprite = sp;
                avImg.color = isSelected ? Color.white : new Color(0.85f, 0.85f, 0.9f, 0.85f);
            }
        }
    }

    // =========================================================================
    // ONBOARDING / PROFILE EDIT
    // =========================================================================

    public void ShowOnboardingPanel(bool isFirstTime)
    {
        if (onboardingPanel != null)
        {
            onboardingPanel.SetActive(true);
            UIAnimator.PopIn(onboardingPanel);
            AudioManager.Instance?.PlayPopupOpenSound();

            selectedAvatarIndex = Mathf.Clamp(PlayerPrefs.GetInt("PlayerAvatarIndex", 0), 0, AvatarList.Length - 1);
            UpdateAvatarSelectionUI();

            // Cập nhật giá trị đang lưu
            if (onbNameInput != null)
                onbNameInput.text = PlayerPrefs.GetString("PlayerName", "Jelly Runner");

            int age = PlayerPrefs.GetInt("PlayerAge", 18);
            if (onbAgeSlider != null)
            {
                onbAgeSlider.minValue = 0f;
                onbAgeSlider.maxValue = 1f;
                onbAgeSlider.value = Mathf.Clamp01((age - 5f) / (70f - 5f));
            }
            if (onbAgeValueText != null)
            {
                onbAgeValueText.text = LocalizationManager.Get("onb_age_val", age);
                onbAgeValueText.enableWordWrapping = false;
                onbAgeValueText.overflowMode = TextOverflowModes.Overflow;
            }

            UpdateLangButtonColors();
        }
    }

    private void SaveAndCloseOnboarding()
    {
        // 1. Tên
        string pName = onbNameInput != null ? onbNameInput.text.Trim() : "Jelly Runner";
        if (string.IsNullOrWhiteSpace(pName)) pName = "Jelly Runner";
        PlayerPrefs.SetString("PlayerName", pName);

        // 2. Tuổi
        int age = 18;
        if (onbAgeSlider != null)
            age = Mathf.RoundToInt(Mathf.Lerp(5f, 70f, onbAgeSlider.value));
        PlayerPrefs.SetInt("PlayerAge", age);

        // 3. Avatar
        PlayerPrefs.SetInt("PlayerAvatarIndex", selectedAvatarIndex);

        // Đánh dấu đã xong Onboarding
        PlayerPrefs.SetInt("HasCompletedOnboarding", 1);
        PlayerPrefs.Save();

        if (onboardingPanel != null)
            UIAnimator.PopOutAndDisable(onboardingPanel);

        AudioManager.Instance?.PlayPopupCloseSound();
        UpdateProfileDisplay();
    }

    // =========================================================================
    // SETTINGS & LEADERBOARD
    // =========================================================================

    private void ToggleSettings()
    {
        if (settingsPanel == null) return;

        if (!settingsPanel.activeSelf)
        {
            settingsPanel.SetActive(true);
            UIAnimator.SlideInFromRight(settingsPanel);
            AudioManager.Instance?.PlayPopupOpenSound();
        }
        else
        {
            UIAnimator.PopOutAndDisable(settingsPanel);
            AudioManager.Instance?.PlayPopupCloseSound();
        }
    }

    private void ToggleLeaderboard()
    {
        if (leaderboardPanel == null) return;

        bool show = !leaderboardPanel.activeSelf;
        if (show)
        {
            leaderboardPanel.SetActive(true);
            UIAnimator.PopIn(leaderboardPanel);
            AudioManager.Instance?.PlayPopupOpenSound();

            if (LeaderboardManager.Instance != null)
                LeaderboardManager.Instance.DisplayLeaderboard(leaderboardDisplayText);
        }
        else
        {
            UIAnimator.PopOutAndDisable(leaderboardPanel);
            AudioManager.Instance?.PlayPopupCloseSound();
        }
    }

    private void UpdateLangButtonColors()
    {
        bool isVn = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;
        if (onbVnBtn != null)
        {
            Image btnImg = onbVnBtn.GetComponent<Image>();
            if (btnImg != null) btnImg.color = isVn ? new Color(0f, 0.65f, 0.4f) : new Color(0.2f, 0.25f, 0.35f);
        }
        if (onbEnBtn != null)
        {
            Image btnImg = onbEnBtn.GetComponent<Image>();
            if (btnImg != null) btnImg.color = !isVn ? new Color(0f, 0.65f, 0.4f) : new Color(0.2f, 0.25f, 0.35f);
        }
    }

    // =========================================================================
    // DỊCH LẠI TOÀN BỘ TEXT TRÊN MENU KHI ĐỔI NGÔN NGỮ
    // =========================================================================

    public void RefreshAllLocalizedTexts()
    {
        // Menu chính
        if (titleText != null) titleText.text = LocalizationManager.Get("menu_title");
        if (bestScoreText != null) bestScoreText.text = LocalizationManager.Get("menu_best_score", PlayerPrefs.GetInt("BestScore", 0));
        if (playBtnText != null) playBtnText.text = LocalizationManager.Get("menu_play");
        if (shopBtnText != null) shopBtnText.text = LocalizationManager.Get("menu_shop");
        if (leaderboardBtnText != null) leaderboardBtnText.text = LocalizationManager.Get("menu_leaderboard");
        if (settingsBtnText != null) settingsBtnText.text = LocalizationManager.Get("menu_settings");
        if (quitBtnText != null) quitBtnText.text = LocalizationManager.Get("menu_quit");

        // Profile Card
        UpdateProfileDisplay();

        // Onboarding
        if (onbTitleText != null) onbTitleText.text = LocalizationManager.Get("onb_title");
        if (onbDescText != null) onbDescText.text = LocalizationManager.Get("onb_desc");
        if (onbNameLabelText != null) onbNameLabelText.text = LocalizationManager.Get("onb_name_label");
        if (onbAgeLabelText != null) onbAgeLabelText.text = LocalizationManager.Get("onb_age_label");
        int currentAge = PlayerPrefs.GetInt("PlayerAge", 18);
        if (onbAgeSlider != null)
            currentAge = Mathf.RoundToInt(Mathf.Lerp(5f, 70f, Mathf.Clamp01(onbAgeSlider.value)));
        if (onbAgeValueText != null)
        {
            onbAgeValueText.text = LocalizationManager.Get("onb_age_val", currentAge);
            onbAgeValueText.enableWordWrapping = false;
            onbAgeValueText.overflowMode = TextOverflowModes.Overflow;
        }
        if (onbLangLabelText != null) onbLangLabelText.text = LocalizationManager.Get("onb_lang_label");
        if (onbAvatarLabelText != null) onbAvatarLabelText.text = LocalizationManager.Get("onb_avatar_label");
        if (onbDoneBtnText != null) onbDoneBtnText.text = LocalizationManager.Get("onb_done_btn");
        UpdateLangButtonColors();

        // Settings
        if (settingsTitleText != null) settingsTitleText.text = LocalizationManager.Get("settings_title");
        if (volLabelText != null) volLabelText.text = LocalizationManager.Get("settings_volume");
        if (sensLabelText != null) sensLabelText.text = LocalizationManager.Get("settings_sensitivity");
        if (bloomLabelText != null) bloomLabelText.text = LocalizationManager.Get("settings_bloom");
        if (resetBtnText != null) resetBtnText.text = LocalizationManager.Get("settings_reset_best");
        if (closeSettingsBtnText != null) closeSettingsBtnText.text = LocalizationManager.Get("btn_close");
        if (langBtnText != null)
        {
            string langName = LocalizationManager.Instance != null && LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese ? "TIẾNG VIỆT" : "ENGLISH";
            langBtnText.text = LocalizationManager.Get("settings_language", langName);
        }

        // Leaderboard
        if (lbTitleText != null) lbTitleText.text = LocalizationManager.Get("lb_title");
        if (closeLBBtnText != null) closeLBBtnText.text = LocalizationManager.Get("btn_close");
        if (leaderboardPanel != null && leaderboardPanel.activeSelf && LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.DisplayLeaderboard(leaderboardDisplayText);
        }
    }
}

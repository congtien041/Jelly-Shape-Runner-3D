using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Giao diện Main Menu cao cấp tích hợp Hệ Thống Đa Ngôn Ngữ (Localization):
/// 1. Màn hình Onboarding nhập thông tin người chơi lần đầu (Tên, Tuổi kéo slider, Ngôn ngữ mặc định Tiếng Việt, Chọn Avatar).
/// 2. Profile Card hiển thị Tên & Avatar góc trên bên phải (Top-Right). Click vào để xem/sửa profile.
/// 3. Cập nhật đa ngôn ngữ theo thời gian thực (Realtime Localization) trên toàn bộ Menu & Cài Đặt.
/// 4. Nút mở Cửa Hàng (Shop UI) với các Skin & Hiệu ứng AssetBundle.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Jelly Runner/Main Menu UI")]
public class MainMenuUI : MonoBehaviour
{
    [Header("--- Shop UI Reference ---")]
    [SerializeField] private ShopUI shopUI;

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

    // Avatar Presets (Màu sắc và Icon biểu trưng)
    public struct AvatarData
    {
        public string icon;
        public Color color;
        public string name;
    }

    public static readonly AvatarData[] AvatarList = new AvatarData[]
    {
        new AvatarData { icon = "💎", color = new Color(0.0f, 0.85f, 1.0f), name = "Jelly Kim Cương" },
        new AvatarData { icon = "👑", color = new Color(1.0f, 0.85f, 0.15f), name = "Jelly Hoàng Gia" },
        new AvatarData { icon = "🔮", color = new Color(0.7f, 0.2f, 1.0f), name = "Jelly Ngân Hà" },
        new AvatarData { icon = "🔥", color = new Color(1.0f, 0.35f, 0.1f), name = "Jelly Bão Lửa" },
        new AvatarData { icon = "🍀", color = new Color(0.1f, 0.9f, 0.45f), name = "Jelly Ngọc Bích" },
        new AvatarData { icon = "⚡", color = new Color(0.95f, 0.95f, 0.2f), name = "Jelly Tia Chớp" }
    };

    private void Start()
    {
        Time.timeScale = 1f;

        // Đảm bảo có LocalizationManager
        if (LocalizationManager.Instance == null)
        {
            GameObject locObj = new GameObject("LocalizationManager");
            locObj.AddComponent<LocalizationManager>();
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

        // Ưu tiên tìm UI đã được thiết kế sẵn trên Scene, nếu chưa có thì mới tự động sinh
        FindOrBuildUI();

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

    private void FindOrBuildUI()
    {
        GameObject canvasObj = GameObject.Find("MenuCanvas");
        if (canvasObj != null)
        {
            Transform cTrans = canvasObj.transform;

            Transform mPanel = cTrans.Find("MainMenuPanel");
            if (mPanel != null)
            {
                titleText = mPanel.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
                bestScoreText = mPanel.Find("BestScoreText")?.GetComponent<TextMeshProUGUI>();

                Transform bGroup = mPanel.Find("ButtonsGroup") ?? mPanel;
                Button playBtn = bGroup.Find("PlayButton")?.GetComponent<Button>();
                if (playBtn != null)
                {
                    playBtnText = playBtn.GetComponentInChildren<TextMeshProUGUI>();
                    playBtn.onClick.RemoveAllListeners();
                    playBtn.onClick.AddListener(PlayGame);
                }

                Button shopBtn = bGroup.Find("ShopButton")?.GetComponent<Button>();
                if (shopBtn != null)
                {
                    shopBtnText = shopBtn.GetComponentInChildren<TextMeshProUGUI>();
                    shopBtn.onClick.RemoveAllListeners();
                    shopBtn.onClick.AddListener(() => { if (shopUI != null) shopUI.ShowShop(); });
                }

                Button lbBtn = bGroup.Find("LeaderboardButton")?.GetComponent<Button>();
                if (lbBtn != null)
                {
                    leaderboardBtnText = lbBtn.GetComponentInChildren<TextMeshProUGUI>();
                    lbBtn.onClick.RemoveAllListeners();
                    lbBtn.onClick.AddListener(ToggleLeaderboard);
                }

                Button setBtn = bGroup.Find("SettingsButton")?.GetComponent<Button>();
                if (setBtn != null)
                {
                    settingsBtnText = setBtn.GetComponentInChildren<TextMeshProUGUI>();
                    setBtn.onClick.RemoveAllListeners();
                    setBtn.onClick.AddListener(ToggleSettings);
                }

                Button quitBtn = bGroup.Find("QuitButton")?.GetComponent<Button>();
                if (quitBtn != null)
                {
                    quitBtnText = quitBtn.GetComponentInChildren<TextMeshProUGUI>();
                    quitBtn.onClick.RemoveAllListeners();
                    quitBtn.onClick.AddListener(QuitGame);
                }
            }

            // Profile Card Top-Right
            Transform pCard = cTrans.Find("ProfileCard_TopRight");
            if (pCard != null)
            {
                profileCard = pCard.gameObject;
                profileNameText = pCard.Find("PlayerName")?.GetComponent<TextMeshProUGUI>();
                profileSubText = pCard.Find("SubText")?.GetComponent<TextMeshProUGUI>();
                profileAvatarImage = pCard.Find("AvatarFrame")?.GetComponent<Image>();
                profileAvatarIconText = pCard.Find("AvatarFrame/Icon")?.GetComponent<TextMeshProUGUI>();
                coinDisplayText = cTrans.Find("CoinDisplay")?.GetComponent<TextMeshProUGUI>() ?? pCard.Find("CoinDisplay")?.GetComponent<TextMeshProUGUI>();

                Button pBtn = pCard.GetComponent<Button>();
                if (pBtn != null)
                {
                    pBtn.onClick.RemoveAllListeners();
                    pBtn.onClick.AddListener(() => ShowOnboardingPanel(false));
                }
            }

            // Onboarding Panel
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

                // Avatar Grid
                Transform grid = onb.Find("AvatarGrid");
                if (grid != null)
                {
                    avatarSelectImages = new Image[AvatarList.Length];
                    for (int i = 0; i < AvatarList.Length; i++)
                    {
                        int idx = i;
                        Transform item = grid.Find("AvatarItem_" + i);
                        if (item != null)
                        {
                            avatarSelectImages[i] = item.GetComponent<Image>();
                            Button btn = item.GetComponent<Button>();
                            if (btn != null)
                            {
                                btn.onClick.RemoveAllListeners();
                                btn.onClick.AddListener(() =>
                                {
                                    selectedAvatarIndex = idx;
                                    for (int j = 0; j < avatarSelectImages.Length; j++)
                                        if (avatarSelectImages[j] != null)
                                            avatarSelectImages[j].color = (j == selectedAvatarIndex) ? Color.white : new Color(0.2f, 0.25f, 0.35f);
                                });
                            }
                        }
                    }
                }

                Button doneBtn = onb.Find("DoneBtn")?.GetComponent<Button>();
                if (doneBtn != null)
                {
                    onbDoneBtnText = doneBtn.GetComponentInChildren<TextMeshProUGUI>();
                    doneBtn.onClick.RemoveAllListeners();
                    doneBtn.onClick.AddListener(SaveAndCloseOnboarding);
                }

                if (onbAgeSlider != null)
                {
                    onbAgeSlider.onValueChanged.RemoveAllListeners();
                    onbAgeSlider.onValueChanged.AddListener(v =>
                    {
                        int age = Mathf.RoundToInt(Mathf.Lerp(5f, 70f, v));
                        if (onbAgeValueText != null) onbAgeValueText.text = LocalizationManager.Get("onb_age_val", age);
                    });
                }
            }

            // Settings Panel
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
                    closeSetBtn.onClick.AddListener(() => settingsPanel?.SetActive(false));
                }
            }

            // Leaderboard Panel
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
                    closeLB.onClick.AddListener(() => leaderboardPanel?.SetActive(false));
                }
            }
        }

        // Nếu Scene hoàn toàn chưa có Canvas thì mới sinh động
        if (titleText == null)
        {
            BuildMenuUI();
        }
    }

    private void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= RefreshAllLocalizedTexts;
    }

    public void PlayGame()
    {
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
    // XÂY DỰNG GIAO DIỆN CHÍNH (MAIN MENU)
    // =========================================================================

    private void BuildMenuUI()
    {
        GameObject canvasObj = new GameObject("MenuCanvas");
        canvasObj.transform.SetParent(transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // Nền tối hiện đại sang trọng
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(canvasObj.transform, false);
        RectTransform bgRt = bg.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.05f, 0.07f, 0.12f, 1f);

        // Tiêu đề Game
        titleText = CreateText(canvasObj.transform, "TitleText", LocalizationManager.Get("menu_title"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 480), new Vector2(850, 240), 78, new Color(0f, 0.88f, 1f), TextAlignmentOptions.Center);

        // Kỷ Lục
        int best = PlayerPrefs.GetInt("BestScore", 0);
        bestScoreText = CreateText(canvasObj.transform, "BestScoreText", LocalizationManager.Get("menu_best_score", best),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 330), new Vector2(650, 50), 36, new Color(1f, 0.85f, 0.15f), TextAlignmentOptions.Center);

        // --- PROFILE CARD Ở GÓC TRÊN BÊN PHẢI (TOP-RIGHT) ---
        BuildProfileWidget(canvasObj.transform);

        // --- CÁC NÚT ĐIỀU HƯỚNG CHÍNH ---
        // 1. Nút Bắt đầu chơi
        Button playBtn = CreateButton(canvasObj.transform, "PlayButton", LocalizationManager.Get("menu_play"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 160), new Vector2(560, 110), 46, new Color(0f, 0.85f, 0.45f), out playBtnText);
        playBtn.onClick.AddListener(PlayGame);

        // 2. Nút Cửa Hàng (Shop)
        Button shopBtn = CreateButton(canvasObj.transform, "ShopButton", LocalizationManager.Get("menu_shop"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 30), new Vector2(520, 95), 36, new Color(0.95f, 0.45f, 0.1f), out shopBtnText);
        shopBtn.onClick.AddListener(() =>
        {
            if (shopUI != null) shopUI.ShowShop();
        });

        // 3. Nút Bảng Xếp Hạng
        Button leaderboardBtn = CreateButton(canvasObj.transform, "LeaderboardButton", LocalizationManager.Get("menu_leaderboard"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -90), new Vector2(480, 85), 34, new Color(0.9f, 0.7f, 0.05f), out leaderboardBtnText);
        leaderboardBtn.onClick.AddListener(ToggleLeaderboard);

        // 4. Nút Cài Đặt
        Button settingsBtn = CreateButton(canvasObj.transform, "SettingsButton", LocalizationManager.Get("menu_settings"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -200), new Vector2(440, 80), 32, new Color(0.25f, 0.5f, 0.85f), out settingsBtnText);
        settingsBtn.onClick.AddListener(ToggleSettings);

        // 5. Nút Thoát
        Button quitBtn = CreateButton(canvasObj.transform, "QuitButton", LocalizationManager.Get("menu_quit"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -310), new Vector2(320, 65), 28, new Color(0.55f, 0.2f, 0.2f), out quitBtnText);
        quitBtn.onClick.AddListener(QuitGame);

        // Xây dựng các Panel phụ
        BuildOnboardingPanel(canvasObj.transform);
        BuildSettingsPanel(canvasObj.transform);
        BuildLeaderboardPanel(canvasObj.transform);
    }

    // =========================================================================
    // PROFILE CARD (TOP-RIGHT)
    // =========================================================================

    private void BuildProfileWidget(Transform parent)
    {
        profileCard = new GameObject("ProfileCard_TopRight");
        profileCard.transform.SetParent(parent, false);

        RectTransform rt = profileCard.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-30, -35);
        rt.sizeDelta = new Vector2(460, 120);

        Image bg = profileCard.AddComponent<Image>();
        bg.color = new Color(0.12f, 0.16f, 0.25f, 0.92f);

        Button profileBtn = profileCard.AddComponent<Button>();
        profileBtn.onClick.AddListener(() => ShowOnboardingPanel(false)); // Mở xem/sửa profile

        // Khung Avatar tròn/bo góc
        GameObject avatarFrame = new GameObject("AvatarFrame");
        avatarFrame.transform.SetParent(profileCard.transform, false);
        RectTransform avRt = avatarFrame.AddComponent<RectTransform>();
        avRt.anchorMin = new Vector2(1f, 0.5f);
        avRt.anchorMax = new Vector2(1f, 0.5f);
        avRt.pivot = new Vector2(1f, 0.5f);
        avRt.anchoredPosition = new Vector2(-15, 0);
        avRt.sizeDelta = new Vector2(90, 90);

        profileAvatarImage = avatarFrame.AddComponent<Image>();
        profileAvatarImage.color = new Color(0f, 0.85f, 1f);

        profileAvatarIconText = CreateText(avatarFrame.transform, "Icon", "💎",
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, 50, Color.white, TextAlignmentOptions.Center);

        // Tên Người Chơi
        profileNameText = CreateText(profileCard.transform, "PlayerName", "Người Chơi",
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(25, 20), new Vector2(310, 45), 32, Color.white, TextAlignmentOptions.Left);

        // Tuổi / Ngôn ngữ
        profileSubText = CreateText(profileCard.transform, "SubText", "18 tuổi • 🇻🇳 VN",
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(25, -20), new Vector2(310, 35), 24, new Color(0.7f, 0.8f, 0.95f), TextAlignmentOptions.Left);

        // Coin Display phía dưới Profile Card
        int coins = PlayerPrefs.GetInt("TotalCoins", 0);
        coinDisplayText = CreateText(parent, "CoinDisplay", LocalizationManager.Get("menu_coin_format", coins),
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-35, -170), new Vector2(400, 50), 32, new Color(1f, 0.85f, 0.1f), TextAlignmentOptions.Right);
    }

    private void UpdateProfileDisplay()
    {
        string pName = PlayerPrefs.GetString("PlayerName", "Player");
        int age = PlayerPrefs.GetInt("PlayerAge", 18);
        bool isVn = LocalizationManager.Instance != null && LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;
        string langTag = isVn ? "🇻🇳 VN" : "🇬🇧 EN";
        int avIdx = Mathf.Clamp(PlayerPrefs.GetInt("PlayerAvatarIndex", 0), 0, AvatarList.Length - 1);

        if (profileNameText != null) profileNameText.text = pName;
        if (profileSubText != null) profileSubText.text = LocalizationManager.Get("profile_age_format", age, langTag);

        if (profileAvatarImage != null && profileAvatarIconText != null)
        {
            profileAvatarImage.color = AvatarList[avIdx].color;
            profileAvatarIconText.text = AvatarList[avIdx].icon;
        }

        if (coinDisplayText != null)
        {
            int total = CurrencyManager.Instance != null ? CurrencyManager.Instance.TotalCoins : PlayerPrefs.GetInt("TotalCoins", 0);
            coinDisplayText.text = LocalizationManager.Get("menu_coin_format", total);
        }
    }

    // =========================================================================
    // MÀN HÌNH NHẬP THÔNG TIN (ONBOARDING SCREEN / PROFILE EDIT)
    // =========================================================================

    private void BuildOnboardingPanel(Transform parent)
    {
        onboardingPanel = CreatePanel(parent, "OnboardingPanel", new Color(0.03f, 0.04f, 0.07f, 0.98f));
        onboardingPanel.SetActive(false);

        // Header
        onbTitleText = CreateText(onboardingPanel.transform, "Title", LocalizationManager.Get("onb_title"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 580), new Vector2(800, 80), 56, new Color(0f, 0.9f, 1f), TextAlignmentOptions.Center);

        onbDescText = CreateText(onboardingPanel.transform, "Desc", LocalizationManager.Get("onb_desc"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 500), new Vector2(800, 50), 30, Color.gray, TextAlignmentOptions.Center);

        // 1. NHẬP TÊN
        onbNameLabelText = CreateText(onboardingPanel.transform, "NameLabel", LocalizationManager.Get("onb_name_label"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 410), new Vector2(700, 40), 32, Color.white, TextAlignmentOptions.Left);

        onbNameInput = CreateInputField(onboardingPanel.transform, "OnbNameInput",
            PlayerPrefs.GetString("PlayerName", "Jelly Runner"),
            LocalizationManager.Get("onb_name_placeholder"),
            new Vector2(0, 335), new Vector2(700, 80));

        // 2. KÉO THANH TRƯỢT CHỌN TUỔI
        onbAgeLabelText = CreateText(onboardingPanel.transform, "AgeLabel", LocalizationManager.Get("onb_age_label"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 230), new Vector2(700, 40), 32, Color.white, TextAlignmentOptions.Left);

        int currentAge = PlayerPrefs.GetInt("PlayerAge", 18);
        onbAgeValueText = CreateText(onboardingPanel.transform, "AgeVal", LocalizationManager.Get("onb_age_val", currentAge),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 175), new Vector2(700, 45), 36, new Color(1f, 0.85f, 0.2f), TextAlignmentOptions.Center);

        onbAgeSlider = CreateSlider(onboardingPanel.transform, "OnbAgeSlider",
            new Vector2(0, 110), new Vector2(700, 45),
            (currentAge - 5f) / (70f - 5f));

        onbAgeSlider.onValueChanged.AddListener(v =>
        {
            int age = Mathf.RoundToInt(Mathf.Lerp(5f, 70f, v));
            onbAgeValueText.text = LocalizationManager.Get("onb_age_val", age);
        });

        // 3. CHỌN NGÔN NGỮ (MẶC ĐỊNH: TIẾNG VIỆT)
        onbLangLabelText = CreateText(onboardingPanel.transform, "LangLabel", LocalizationManager.Get("onb_lang_label"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 20), new Vector2(700, 40), 30, Color.white, TextAlignmentOptions.Left);

        GameObject langGroup = new GameObject("LangGroup");
        langGroup.transform.SetParent(onboardingPanel.transform, false);
        RectTransform lgRt = langGroup.AddComponent<RectTransform>();
        lgRt.anchoredPosition = new Vector2(0, -45);
        lgRt.sizeDelta = new Vector2(700, 75);

        onbVnBtn = CreateButton(langGroup.transform, "LangVN", LocalizationManager.Get("lang_vietnamese"),
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(0, 0), new Vector2(335, 75), 26, new Color(0f, 0.65f, 0.4f), out _);

        onbEnBtn = CreateButton(langGroup.transform, "LangEN", LocalizationManager.Get("lang_english"),
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0, 0), new Vector2(335, 75), 26, new Color(0.2f, 0.25f, 0.35f), out _);

        onbVnBtn.onClick.AddListener(() =>
        {
            if (LocalizationManager.Instance != null)
                LocalizationManager.Instance.SetLanguage(LocalizationManager.Language.Vietnamese);
            UpdateLangButtonColors();
        });

        onbEnBtn.onClick.AddListener(() =>
        {
            if (LocalizationManager.Instance != null)
                LocalizationManager.Instance.SetLanguage(LocalizationManager.Language.English);
            UpdateLangButtonColors();
        });

        UpdateLangButtonColors();

        // 4. CHỌN AVATAR ĐẠI DIỆN
        onbAvatarLabelText = CreateText(onboardingPanel.transform, "AvatarLabel", LocalizationManager.Get("onb_avatar_label"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -135), new Vector2(700, 40), 30, Color.white, TextAlignmentOptions.Left);

        BuildAvatarGrid(onboardingPanel.transform);

        // NÚT HOÀN TẤT
        Button doneBtn = CreateButton(onboardingPanel.transform, "DoneBtn", LocalizationManager.Get("onb_done_btn"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -480), new Vector2(620, 105), 40, new Color(0f, 0.85f, 0.45f), out onbDoneBtnText);
        doneBtn.onClick.AddListener(SaveAndCloseOnboarding);
    }

    private void UpdateLangButtonColors()
    {
        bool isVn = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;
        if (onbVnBtn != null) onbVnBtn.GetComponent<Image>().color = isVn ? new Color(0f, 0.65f, 0.4f) : new Color(0.2f, 0.25f, 0.35f);
        if (onbEnBtn != null) onbEnBtn.GetComponent<Image>().color = !isVn ? new Color(0f, 0.65f, 0.4f) : new Color(0.2f, 0.25f, 0.35f);
    }

    private void BuildAvatarGrid(Transform parent)
    {
        GameObject grid = new GameObject("AvatarGrid");
        grid.transform.SetParent(parent, false);
        RectTransform gRt = grid.AddComponent<RectTransform>();
        gRt.anchoredPosition = new Vector2(0, -250);
        gRt.sizeDelta = new Vector2(720, 160);

        avatarSelectImages = new Image[AvatarList.Length];
        selectedAvatarIndex = PlayerPrefs.GetInt("PlayerAvatarIndex", 0);

        for (int i = 0; i < AvatarList.Length; i++)
        {
            int index = i;
            AvatarData data = AvatarList[i];

            GameObject item = new GameObject("AvatarItem_" + i);
            item.transform.SetParent(grid.transform, false);
            RectTransform iRt = item.AddComponent<RectTransform>();
            float xPos = -300 + (i * 120);
            iRt.anchoredPosition = new Vector2(xPos, 0);
            iRt.sizeDelta = new Vector2(105, 105);

            Image borderImg = item.AddComponent<Image>();
            avatarSelectImages[i] = borderImg;
            borderImg.color = (i == selectedAvatarIndex) ? Color.white : new Color(0.2f, 0.25f, 0.35f);

            // Icon nền
            GameObject inner = new GameObject("Inner");
            inner.transform.SetParent(item.transform, false);
            RectTransform inRt = inner.AddComponent<RectTransform>();
            inRt.anchorMin = new Vector2(0.08f, 0.08f);
            inRt.anchorMax = new Vector2(0.92f, 0.92f);
            inRt.offsetMin = Vector2.zero;
            inRt.offsetMax = Vector2.zero;
            Image inImg = inner.AddComponent<Image>();
            inImg.color = data.color;

            CreateText(inner.transform, "Icon", data.icon,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, 48, Color.white, TextAlignmentOptions.Center);

            Button btn = item.AddComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                selectedAvatarIndex = index;
                for (int j = 0; j < avatarSelectImages.Length; j++)
                    avatarSelectImages[j].color = (j == selectedAvatarIndex) ? Color.white : new Color(0.2f, 0.25f, 0.35f);
            });
        }
    }

    public void ShowOnboardingPanel(bool isFirstTime)
    {
        if (onboardingPanel != null)
        {
            onboardingPanel.SetActive(true);

            // Cập nhật giá trị đang lưu
            if (onbNameInput != null)
                onbNameInput.text = PlayerPrefs.GetString("PlayerName", "Jelly Runner");

            int age = PlayerPrefs.GetInt("PlayerAge", 18);
            if (onbAgeSlider != null)
                onbAgeSlider.value = (age - 5f) / (70f - 5f);
            if (onbAgeValueText != null)
                onbAgeValueText.text = LocalizationManager.Get("onb_age_val", age);

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
            onboardingPanel.SetActive(false);

        UpdateProfileDisplay();
    }

    // =========================================================================
    // CÀI ĐẶT & BẢNG XẾP HẠNG
    // =========================================================================

    private void BuildSettingsPanel(Transform parent)
    {
        settingsPanel = CreatePanel(parent, "SettingsPanel", new Color(0, 0, 0, 0.94f));
        settingsPanel.SetActive(false);

        settingsTitleText = CreateText(settingsPanel.transform, "SettingsTitle", LocalizationManager.Get("settings_title"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 380), new Vector2(500, 80), 56, Color.white, TextAlignmentOptions.Center);

        // Âm lượng
        volLabelText = CreateText(settingsPanel.transform, "VolLabel", LocalizationManager.Get("settings_volume"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 280), new Vector2(400, 40), 28, Color.gray, TextAlignmentOptions.Center);

        Slider volumeSlider = CreateSlider(settingsPanel.transform, "VolumeSlider",
            new Vector2(0, 230), new Vector2(550, 45),
            PlayerPrefs.GetFloat("Volume", 1f));
        volumeSlider.onValueChanged.AddListener(v =>
        {
            PlayerPrefs.SetFloat("Volume", v);
            AudioListener.volume = v;
            if (AudioManager.Instance != null) AudioManager.Instance.SetBGMVolume(v);
        });

        // Độ nhạy
        sensLabelText = CreateText(settingsPanel.transform, "SensLabel", LocalizationManager.Get("settings_sensitivity"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 160), new Vector2(400, 40), 28, Color.gray, TextAlignmentOptions.Center);

        Slider sensitivitySlider = CreateSlider(settingsPanel.transform, "SensSlider",
            new Vector2(0, 110), new Vector2(550, 45),
            PlayerPrefs.GetFloat("DragSensitivity", 3.5f) / 7f);
        sensitivitySlider.onValueChanged.AddListener(v =>
        {
            PlayerPrefs.SetFloat("DragSensitivity", v * 7f);
        });

        // Độ phát sáng (Bloom)
        bloomLabelText = CreateText(settingsPanel.transform, "BloomLabel", LocalizationManager.Get("settings_bloom"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 40), new Vector2(500, 40), 28, new Color(0f, 0.88f, 1f), TextAlignmentOptions.Center);

        float currentBloom = PlayerPrefs.GetFloat("PP_BloomIntensity", 1.35f);
        Slider bloomSlider = CreateSlider(settingsPanel.transform, "BloomSlider",
            new Vector2(0, -10), new Vector2(550, 45),
            currentBloom / 2.5f);
        bloomSlider.onValueChanged.AddListener(v =>
        {
            float intensity = v * 2.5f;
            if (GlobalVolumeManager.Instance != null)
                GlobalVolumeManager.Instance.SetBloomIntensity(intensity);
        });

        // Nút Đổi Ngôn Ngữ trong Settings
        string langName = LocalizationManager.Instance != null && LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese ? "TIẾNG VIỆT 🇻🇳" : "ENGLISH 🇬🇧";
        Button langToggleBtn = CreateButton(settingsPanel.transform, "LangToggleBtn", LocalizationManager.Get("settings_language", langName),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -100), new Vector2(480, 70), 28, new Color(0.15f, 0.6f, 0.45f), out langBtnText);
        langToggleBtn.onClick.AddListener(() =>
        {
            if (LocalizationManager.Instance != null)
                LocalizationManager.Instance.ToggleLanguage();
        });

        // Xóa Kỷ Lục
        Button resetBtn = CreateButton(settingsPanel.transform, "ResetBestBtn", LocalizationManager.Get("settings_reset_best"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -190), new Vector2(440, 70), 30, new Color(0.85f, 0.2f, 0.2f), out resetBtnText);
        resetBtn.onClick.AddListener(() =>
        {
            PlayerPrefs.SetInt("BestScore", 0);
            if (LeaderboardManager.Instance != null) LeaderboardManager.Instance.ClearLeaderboard();
            PlayerPrefs.Save();
            if (bestScoreText != null) bestScoreText.text = LocalizationManager.Get("menu_best_score", 0);
        });

        // Đóng Cài Đặt
        Button closeBtn = CreateButton(settingsPanel.transform, "CloseSettingsBtn", LocalizationManager.Get("btn_close"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -300), new Vector2(340, 75), 34, new Color(0.4f, 0.4f, 0.5f), out closeSettingsBtnText);
        closeBtn.onClick.AddListener(() => settingsPanel.SetActive(false));
    }

    private void ToggleSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    private void BuildLeaderboardPanel(Transform parent)
    {
        leaderboardPanel = CreatePanel(parent, "LeaderboardPanel", new Color(0, 0, 0, 0.94f));
        leaderboardPanel.SetActive(false);

        lbTitleText = CreateText(leaderboardPanel.transform, "LBTitle", LocalizationManager.Get("lb_title"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 350), new Vector2(700, 80), 54, new Color(1f, 0.85f, 0.1f), TextAlignmentOptions.Center);

        leaderboardDisplayText = CreateText(leaderboardPanel.transform, "LBEntries", "",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 60), new Vector2(750, 480), 38, Color.white, TextAlignmentOptions.Center);

        Button closeLB = CreateButton(leaderboardPanel.transform, "CloseLBBtn", LocalizationManager.Get("btn_close"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -300), new Vector2(340, 75), 34, new Color(0.4f, 0.4f, 0.5f), out closeLBBtnText);
        closeLB.onClick.AddListener(() => leaderboardPanel.SetActive(false));
    }

    private void ToggleLeaderboard()
    {
        if (leaderboardPanel == null) return;
        bool show = !leaderboardPanel.activeSelf;
        leaderboardPanel.SetActive(show);

        if (show && LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.DisplayLeaderboard(leaderboardDisplayText);
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
        if (onbAgeValueText != null) onbAgeValueText.text = LocalizationManager.Get("onb_age_val", PlayerPrefs.GetInt("PlayerAge", 18));
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
            string langName = LocalizationManager.Instance != null && LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese ? "TIẾNG VIỆT 🇻🇳" : "ENGLISH 🇬🇧";
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

    // =========================================================================
    // UI BUILDER HELPERS
    // =========================================================================

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
        Vector2 anchoredPos, Vector2 size, int fontSize, Color bgColor, out TextMeshProUGUI labelTmp)
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

        labelTmp = CreateText(obj.transform, name + "_Label", label,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, fontSize, Color.white, TextAlignmentOptions.Center);
        return btn;
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
        phText.fontStyle = FontStyles.Italic;

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
        inputField.characterLimit = 16;
        inputField.contentType = TMP_InputField.ContentType.Standard;

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

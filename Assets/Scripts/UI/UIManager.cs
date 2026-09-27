using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý toàn bộ giao diện HUD, Game Over, Pause trong game.
/// Hỗ trợ cả 2 chế độ:
/// 1. Sử dụng UI có sẵn trên Scene (được thiết kế trực tiếp trong Unity Editor).
/// 2. Tự động sinh UI dự phòng nếu Scene chưa có.
/// Tự động cập nhật Đa Ngôn Ngữ khi Chơi Lại (Restart) và khi Đổi Ngôn Ngữ.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Jelly Runner/UI Manager")]
public class UIManager : MonoBehaviour
{
    [Header("--- HUD (In-Game) ---")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI coinHUDText;
    [SerializeField] private Button pauseButton;

    [Header("--- Game Over Panel ---")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverTitleText;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI leaderboardText;
    [SerializeField] private Button restartButton;
    [SerializeField] private TextMeshProUGUI restartBtnText;
    [SerializeField] private Button menuButton;
    [SerializeField] private TextMeshProUGUI menuBtnText;

    [Header("--- Pause Panel ---")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private TextMeshProUGUI pauseTitleText;
    [SerializeField] private Button resumeButton;
    [SerializeField] private TextMeshProUGUI resumeBtnText;
    [SerializeField] private Button pauseRestartButton;
    [SerializeField] private TextMeshProUGUI pauseRestartBtnText;
    [SerializeField] private Button pauseMenuButton;
    [SerializeField] private TextMeshProUGUI pauseMenuBtnText;
    [SerializeField] private TextMeshProUGUI pauseBloomLabelText;
    [SerializeField] private Slider pauseBloomSlider;

    [Header("--- Tham Chiếu ---")]
    [SerializeField] private JellyPlayer player;

    private Canvas canvas;

    private void Start()
    {
        // Nếu chưa kéo thả UI từ Scene, tự động tìm hoặc sinh mới
        if (scoreText == null)
            FindOrBuildUI();

        // Ẩn các panel khi bắt đầu game
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        // Tìm Player nếu chưa gán
        if (player == null)
            player = FindAnyObjectByType<JellyPlayer>();

        // Đăng ký sự kiện với GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged.AddListener(UpdateScore);
            GameManager.Instance.OnGameOver.AddListener(ShowGameOver);
            GameManager.Instance.OnGamePaused.AddListener(ShowPause);
            GameManager.Instance.OnGameResumed.AddListener(HidePause);
        }

        // Đăng ký sự kiện coin
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCoinsChanged.AddListener(UpdateCoinHUD);
            CurrencyManager.Instance.SetCoinText(coinHUDText);
        }

        // Gắn nút (xóa listener cũ để tránh trùng lặp khi restart)
        SetupButtonListeners();

        // Lắng nghe sự kiện đổi ngôn ngữ
        LocalizationManager.OnLanguageChanged += RefreshLocalizedTexts;

        // Cập nhật text đa ngôn ngữ ngay khi vào game / chơi lại
        RefreshLocalizedTexts();

        UpdateScore(GameManager.Instance != null ? GameManager.Instance.CurrentScore : 0);
        UpdateCoinHUD(CurrencyManager.Instance != null ? CurrencyManager.Instance.TotalCoins : 0);
    }

    private void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= RefreshLocalizedTexts;
    }

    private void SetupButtonListeners()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(() => GameManager.Instance?.PauseGame());
        }
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(() => GameManager.Instance?.RestartGame());
        }
        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(() => GameManager.Instance?.GoToMainMenu());
        }
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(() => GameManager.Instance?.ResumeGame());
        }
        if (pauseRestartButton != null)
        {
            pauseRestartButton.onClick.RemoveAllListeners();
            pauseRestartButton.onClick.AddListener(() => GameManager.Instance?.RestartGame());
        }
        if (pauseMenuButton != null)
        {
            pauseMenuButton.onClick.RemoveAllListeners();
            pauseMenuButton.onClick.AddListener(() => GameManager.Instance?.GoToMainMenu());
        }
    }

    private void Update()
    {
        if (player != null && distanceText != null)
        {
            distanceText.text = LocalizationManager.Get("hud_distance", player.DistanceTraveled.ToString("F0"));
        }
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = LocalizationManager.Get("hud_score", score);
    }

    private void UpdateCoinHUD(int coins)
    {
        if (coinHUDText != null)
            coinHUDText.text = $"🪙 {coins}";
    }

    public void RefreshLocalizedTexts()
    {
        // Game Over Panel Texts
        if (gameOverTitleText != null) gameOverTitleText.text = LocalizationManager.Get("gameover_title");
        if (restartBtnText != null) restartBtnText.text = LocalizationManager.Get("btn_restart");
        if (menuBtnText != null) menuBtnText.text = LocalizationManager.Get("btn_menu");

        // Pause Panel Texts
        if (pauseTitleText != null) pauseTitleText.text = LocalizationManager.Get("pause_title");
        if (resumeBtnText != null) resumeBtnText.text = LocalizationManager.Get("btn_resume");
        if (pauseRestartBtnText != null) pauseRestartBtnText.text = LocalizationManager.Get("btn_restart");
        if (pauseMenuBtnText != null) pauseMenuBtnText.text = LocalizationManager.Get("btn_menu");
        if (pauseBloomLabelText != null) pauseBloomLabelText.text = LocalizationManager.Get("settings_bloom");

        if (gameOverPanel != null && gameOverPanel.activeSelf)
        {
            int score = GameManager.Instance != null ? GameManager.Instance.CurrentScore : 0;
            int best = GameManager.Instance != null ? GameManager.Instance.BestScore : 0;
            if (gameOverScoreText != null) gameOverScoreText.text = LocalizationManager.Get("gameover_score", score);
            if (bestScoreText != null) bestScoreText.text = LocalizationManager.Get("gameover_best", best);
        }
    }

    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            int score = GameManager.Instance != null ? GameManager.Instance.CurrentScore : 0;
            int best = GameManager.Instance != null ? GameManager.Instance.BestScore : 0;

            if (gameOverScoreText != null)
                gameOverScoreText.text = LocalizationManager.Get("gameover_score", score);
            if (bestScoreText != null)
                bestScoreText.text = LocalizationManager.Get("gameover_best", best);

            RefreshLocalizedTexts();

            // Hiển thị leaderboard
            if (leaderboardText != null && LeaderboardManager.Instance != null)
                LeaderboardManager.Instance.DisplayLeaderboard(leaderboardText);
        }
    }

    private void ShowPause()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            RefreshLocalizedTexts();
        }
    }

    private void HidePause()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    // =========================================================================
    // TỰ ĐỘNG TÌM HOẶC SINH DỰ PHÒNG (FALLBACK)
    // =========================================================================

    private void FindOrBuildUI()
    {
        // 1. Thử tìm trên Scene trước
        Canvas existingCanvas = FindAnyObjectByType<Canvas>();
        if (existingCanvas != null)
        {
            scoreText = existingCanvas.transform.Find("HUDPanel/ScoreText")?.GetComponent<TextMeshProUGUI>();
            distanceText = existingCanvas.transform.Find("HUDPanel/DistanceText")?.GetComponent<TextMeshProUGUI>();
            coinHUDText = existingCanvas.transform.Find("HUDPanel/CoinHUDText")?.GetComponent<TextMeshProUGUI>();
            pauseButton = existingCanvas.transform.Find("HUDPanel/PauseButton")?.GetComponent<Button>();

            Transform goPanel = existingCanvas.transform.Find("GameOverPanel");
            if (goPanel != null)
            {
                gameOverPanel = goPanel.gameObject;
                gameOverTitleText = goPanel.Find("GameOverTitle")?.GetComponent<TextMeshProUGUI>();
                gameOverScoreText = goPanel.Find("GameOverScore")?.GetComponent<TextMeshProUGUI>();
                bestScoreText = goPanel.Find("BestScore")?.GetComponent<TextMeshProUGUI>();
                leaderboardText = goPanel.Find("LeaderboardMini")?.GetComponent<TextMeshProUGUI>();
                restartButton = goPanel.Find("RestartButton")?.GetComponent<Button>();
                if (restartButton != null) restartBtnText = restartButton.GetComponentInChildren<TextMeshProUGUI>();
                menuButton = goPanel.Find("MenuButton")?.GetComponent<Button>();
                if (menuButton != null) menuBtnText = menuButton.GetComponentInChildren<TextMeshProUGUI>();
            }

            Transform pPanel = existingCanvas.transform.Find("PausePanel");
            if (pPanel != null)
            {
                pausePanel = pPanel.gameObject;
                pauseTitleText = pPanel.Find("PauseTitle")?.GetComponent<TextMeshProUGUI>();
                resumeButton = pPanel.Find("ResumeButton")?.GetComponent<Button>();
                if (resumeButton != null) resumeBtnText = resumeButton.GetComponentInChildren<TextMeshProUGUI>();
                pauseRestartButton = pPanel.Find("PauseRestartBtn")?.GetComponent<Button>();
                if (pauseRestartButton != null) pauseRestartBtnText = pauseRestartButton.GetComponentInChildren<TextMeshProUGUI>();
                pauseMenuButton = pPanel.Find("PauseMenuBtn")?.GetComponent<Button>();
                if (pauseMenuButton != null) pauseMenuBtnText = pauseMenuButton.GetComponentInChildren<TextMeshProUGUI>();

                pauseBloomLabelText = pPanel.Find("PauseBloomLabel")?.GetComponent<TextMeshProUGUI>();
                pauseBloomSlider = pPanel.Find("PauseBloomSlider")?.GetComponent<Slider>();
                if (pauseBloomSlider != null)
                {
                    float currentBloom = PlayerPrefs.GetFloat("PP_BloomIntensity", 1.35f);
                    pauseBloomSlider.value = currentBloom / 2.5f;
                    pauseBloomSlider.onValueChanged.RemoveAllListeners();
                    pauseBloomSlider.onValueChanged.AddListener(v =>
                    {
                        float intensity = v * 2.5f;
                        if (GlobalVolumeManager.Instance != null)
                            GlobalVolumeManager.Instance.SetBloomIntensity(intensity);
                    });
                }
            }
        }

        // 2. Nếu vẫn chưa có gì thì mới tự động sinh
        if (scoreText == null)
            BuildUI();
    }

    private void BuildUI()
    {
        GameObject canvasObj = new GameObject("GameCanvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // ===== HUD =====
        GameObject hudPanel = new GameObject("HUDPanel");
        hudPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform hudRt = hudPanel.AddComponent<RectTransform>();
        hudRt.anchorMin = Vector2.zero;
        hudRt.anchorMax = Vector2.one;
        hudRt.offsetMin = Vector2.zero;
        hudRt.offsetMax = Vector2.zero;

        scoreText = CreateText(hudPanel.transform, "ScoreText", "0",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -50), new Vector2(300, 80), 68, Color.white, TextAlignmentOptions.Center);

        coinHUDText = CreateText(hudPanel.transform, "CoinHUDText", "🪙 0",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-25, -45), new Vector2(250, 50), 34, new Color(1f, 0.85f, 0f), TextAlignmentOptions.Right);

        distanceText = CreateText(hudPanel.transform, "DistanceText", "0m",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-25, -100), new Vector2(250, 50), 28, new Color(0.7f, 0.85f, 1f), TextAlignmentOptions.Right);

        pauseButton = CreateButton(hudPanel.transform, "PauseButton", "⏸",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(45, -45), new Vector2(90, 90), 48, new Color(1, 1, 1, 0.6f), out _);

        // ===== GAME OVER PANEL =====
        gameOverPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.04f, 0.06f, 0.1f, 0.95f));

        gameOverTitleText = CreateText(gameOverPanel.transform, "GameOverTitle", LocalizationManager.Get("gameover_title"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 340), new Vector2(650, 100), 72, new Color(1f, 0.3f, 0.35f), TextAlignmentOptions.Center);

        gameOverScoreText = CreateText(gameOverPanel.transform, "GameOverScore", LocalizationManager.Get("gameover_score", 0),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 220), new Vector2(550, 60), 48, Color.white, TextAlignmentOptions.Center);

        bestScoreText = CreateText(gameOverPanel.transform, "BestScore", LocalizationManager.Get("gameover_best", 0),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 150), new Vector2(550, 50), 36, new Color(1f, 0.85f, 0f), TextAlignmentOptions.Center);

        leaderboardText = CreateText(gameOverPanel.transform, "LeaderboardMini", "",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 10), new Vector2(700, 220), 28, new Color(0.85f, 0.9f, 1f), TextAlignmentOptions.Center);

        restartButton = CreateButton(gameOverPanel.transform, "RestartButton", LocalizationManager.Get("btn_restart"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -160), new Vector2(480, 90), 40, new Color(0f, 0.85f, 0.45f), out restartBtnText);

        menuButton = CreateButton(gameOverPanel.transform, "MenuButton", LocalizationManager.Get("btn_menu"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -270), new Vector2(360, 75), 32, new Color(0.4f, 0.45f, 0.55f), out menuBtnText);

        // ===== PAUSE PANEL =====
        pausePanel = CreatePanel(canvasObj.transform, "PausePanel", new Color(0.04f, 0.06f, 0.1f, 0.92f));

        pauseTitleText = CreateText(pausePanel.transform, "PauseTitle", LocalizationManager.Get("pause_title"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 240), new Vector2(550, 100), 72, Color.white, TextAlignmentOptions.Center);

        resumeButton = CreateButton(pausePanel.transform, "ResumeButton", LocalizationManager.Get("btn_resume"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 110), new Vector2(460, 85), 40, new Color(0f, 0.85f, 1f), out resumeBtnText);

        pauseRestartButton = CreateButton(pausePanel.transform, "PauseRestartBtn", LocalizationManager.Get("btn_restart"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 10), new Vector2(460, 85), 40, new Color(0f, 0.85f, 0.45f), out pauseRestartBtnText);

        pauseBloomLabelText = CreateText(pausePanel.transform, "PauseBloomLabel", LocalizationManager.Get("settings_bloom"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -80), new Vector2(460, 35), 26, new Color(0f, 0.88f, 1f), TextAlignmentOptions.Center);

        float curBloom = PlayerPrefs.GetFloat("PP_BloomIntensity", 1.35f);
        pauseBloomSlider = CreateSlider(pausePanel.transform, "PauseBloomSlider", new Vector2(0, -125), new Vector2(480, 42), curBloom / 2.5f);
        pauseBloomSlider.onValueChanged.AddListener(v =>
        {
            float intensity = v * 2.5f;
            if (GlobalVolumeManager.Instance != null)
                GlobalVolumeManager.Instance.SetBloomIntensity(intensity);
        });

        pauseMenuButton = CreateButton(pausePanel.transform, "PauseMenuBtn", LocalizationManager.Get("btn_menu"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -220), new Vector2(360, 75), 32, new Color(0.4f, 0.45f, 0.55f), out pauseMenuBtnText);
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
        handleRt.sizeDelta = new Vector2(30, 0);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

        slider.handleRect = handleRt;
        slider.targetGraphic = handleImg;

        return slider;
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UIManager v2 — Tích hợp Layer Lab GUI Pro-CasualGame.
/// Quản lý toàn bộ giao diện HUD, Game Over, Pause trong game.
/// Tìm và kết nối các Prefab Layer Lab đã được kéo thả vào Scene.
/// Hỗ trợ animation mở/đóng panel (UIAnimator) và Particle FX (UIParticleFXManager).
/// Tự động cập nhật Đa Ngôn Ngữ khi Chơi Lại (Restart) và khi Đổi Ngôn Ngữ.
///
/// KHÔNG CÒN TỰ SINH UI BẰNG CODE — toàn bộ UI phải được thiết kế sẵn trên Scene
/// bằng các Prefab của Layer Lab.
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

    [Header("--- Particle FX (Layer Lab) ---")]
    [Tooltip("Kéo thả Fx_Spread_Star hoặc Fx_Shines_Glow vào đây (con của GameOverPanel)")]
    [SerializeField] private GameObject gameOverParticleFX;

    [Header("--- Tham Chiếu ---")]
    [SerializeField] private JellyPlayer player;

    private void Start()
    {
        // Nếu chưa kéo thả UI trong Inspector, tự động tìm trên Scene
        if (scoreText == null)
            FindUIReferences();

        // Ẩn các panel khi bắt đầu game
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverParticleFX != null) gameOverParticleFX.SetActive(false);

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
            coinHUDText.text = $"$ {coins}";
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

            // ★ Animation Layer Lab: PopIn bounce effect
            UIAnimator.PopIn(gameOverPanel);

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

            // ★ Bật Particle FX Layer Lab (Fx_Spread_Star, Fx_Shines_Glow...)
            if (gameOverParticleFX != null)
            {
                gameOverParticleFX.SetActive(true);
                var ps = gameOverParticleFX.GetComponent<ParticleSystem>();
                if (ps != null) ps.Play();
            }

            // ★ Hoặc dùng UIParticleFXManager nếu đã cài đặt
            if (UIParticleFXManager.Instance != null)
            {
                UIParticleFXManager.Instance.PlaySpreadStar(gameOverPanel.GetComponent<RectTransform>());
            }
        }
    }

    private void ShowPause()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);

            // ★ Animation Layer Lab: Trượt vào từ trên
            UIAnimator.SlideInFromTop(pausePanel);

            RefreshLocalizedTexts();
        }
    }

    private void HidePause()
    {
        if (pausePanel != null)
        {
            // ★ Animation Layer Lab: Mờ dần rồi ẩn
            UIAnimator.FadeOutAndDisable(pausePanel);
        }
    }

    // =========================================================================
    // TÌM UI TRÊN SCENE (Layer Lab Prefab đã kéo thả)
    // =========================================================================

    private void FindUIReferences()
    {
        Canvas existingCanvas = FindAnyObjectByType<Canvas>();
        if (existingCanvas == null)
        {
            Debug.LogWarning("[UIManager] Không tìm thấy Canvas nào trên Scene! " +
                "Hãy kéo thả Prefab Layer Lab vào Scene theo hướng dẫn.");
            return;
        }

        Transform c = existingCanvas.transform;

        // --- HUD ---
        scoreText = FindTMP(c, "HUDPanel/ScoreText");
        distanceText = FindTMP(c, "HUDPanel/DistanceText");
        coinHUDText = FindTMP(c, "HUDPanel/CoinBadge/CoinHUDText") ?? FindTMP(c, "HUDPanel/CoinHUDText");
        pauseButton = FindButton(c, "HUDPanel/PauseButton");

        // --- Game Over Panel ---
        Transform goPanel = c.Find("GameOverPanel");
        if (goPanel != null)
        {
            gameOverPanel = goPanel.gameObject;
            gameOverTitleText = FindTMP(goPanel, "GameOverTitle");
            gameOverScoreText = FindTMP(goPanel, "GameOverScore");
            bestScoreText = FindTMP(goPanel, "BestScore");
            leaderboardText = FindTMP(goPanel, "LeaderboardMini");

            restartButton = FindButton(goPanel, "RestartButton");
            if (restartButton != null) restartBtnText = restartButton.GetComponentInChildren<TextMeshProUGUI>();
            menuButton = FindButton(goPanel, "MenuButton");
            if (menuButton != null) menuBtnText = menuButton.GetComponentInChildren<TextMeshProUGUI>();

            // Particle FX (Kéo thả Fx_Spread_Star vào trong GameOverPanel, đặt tên "ParticleFX")
            Transform fxT = goPanel.Find("ParticleFX");
            if (fxT != null) gameOverParticleFX = fxT.gameObject;
        }

        // --- Pause Panel ---
        Transform pPanel = c.Find("PausePanel");
        if (pPanel != null)
        {
            pausePanel = pPanel.gameObject;
            pauseTitleText = FindTMP(pPanel, "PauseTitle");

            resumeButton = FindButton(pPanel, "ResumeButton");
            if (resumeButton != null) resumeBtnText = resumeButton.GetComponentInChildren<TextMeshProUGUI>();
            pauseRestartButton = FindButton(pPanel, "PauseRestartBtn");
            if (pauseRestartButton != null) pauseRestartBtnText = pauseRestartButton.GetComponentInChildren<TextMeshProUGUI>();
            pauseMenuButton = FindButton(pPanel, "PauseMenuBtn");
            if (pauseMenuButton != null) pauseMenuBtnText = pauseMenuButton.GetComponentInChildren<TextMeshProUGUI>();

            pauseBloomLabelText = FindTMP(pPanel, "PauseBloomLabel");
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

    // =========================================================================
    // HELPERS
    // =========================================================================

    private static TextMeshProUGUI FindTMP(Transform parent, string path)
    {
        return parent.Find(path)?.GetComponent<TextMeshProUGUI>();
    }

    private static Button FindButton(Transform parent, string path)
    {
        return parent.Find(path)?.GetComponent<Button>();
    }
}

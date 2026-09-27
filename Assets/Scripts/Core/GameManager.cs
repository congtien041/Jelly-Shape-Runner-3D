using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý toàn bộ vòng đời game: Score, Game Over, Restart, Pause, Best Score.
/// Tích hợp LeaderboardManager khi game over.
/// Singleton pattern — tồn tại duy nhất trong Scene.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Jelly Runner/Game Manager")]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("--- Trạng Thái ---")]
    [SerializeField] private int currentScore = 0;

    public int CurrentScore => currentScore;
    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }
    public int BestScore => PlayerPrefs.GetInt("BestScore", 0);

    [Header("--- Sự Kiện ---")]
    public UnityEvent OnGameOver = new UnityEvent();
    public UnityEvent<int> OnScoreChanged = new UnityEvent<int>();
    public UnityEvent OnGamePaused = new UnityEvent();
    public UnityEvent OnGameResumed = new UnityEvent();
    public UnityEvent OnGameRestarted = new UnityEvent();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        IsGameOver = false;
        IsPaused = false;
        currentScore = 0;
        OnScoreChanged?.Invoke(currentScore);
    }

    private void Update()
    {
        // Phím Escape / nút Back -> toggle Pause
        if (Input.GetKeyDown(KeyCode.Escape) && !IsGameOver)
        {
            if (IsPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void AddScore(int amount = 1)
    {
        if (IsGameOver) return;

        currentScore += amount;
        OnScoreChanged?.Invoke(currentScore);
    }

    public void TriggerGameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;

        // Lưu Best Score
        if (currentScore > BestScore)
            PlayerPrefs.SetInt("BestScore", currentScore);

        // Cập nhật Leaderboard
        if (LeaderboardManager.Instance != null)
            LeaderboardManager.Instance.AddScore(currentScore);

        PlayerPrefs.Save();

        OnGameOver?.Invoke();
    }

    public void PauseGame()
    {
        if (IsGameOver || IsPaused) return;
        IsPaused = true;
        Time.timeScale = 0f;
        OnGamePaused?.Invoke();
    }

    public void ResumeGame()
    {
        if (!IsPaused) return;
        IsPaused = false;
        Time.timeScale = 1f;
        OnGameResumed?.Invoke();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        OnGameRestarted?.Invoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}

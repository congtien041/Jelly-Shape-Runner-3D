using UnityEngine;
using System;

/// <summary>
/// Hệ thống Quản Lý Lưu Trữ Dữ Liệu Tập Trung (Save & Data Manager):
/// Quản lý đồng bộ và bền vững toàn bộ dữ liệu game:
/// 1. Thông tin người chơi (Tên, Tuổi, Avatar).
/// 2. Tài chính (Tiền vàng / TotalCoins).
/// 3. Kỷ lục & Điểm cao (BestScore, Top 5 Leaderboard).
/// 4. Cài đặt hệ thống (Ngôn ngữ, Âm thanh, Độ nhạy, Đồ họa Bloom).
/// 5. Tiến trình Cửa Hàng (Các Skin/Hiệu Ứng đã sở hữu và đang trang bị).
/// 
/// Tự động Save khi Pause hoặc Quit game.
/// Cung cấp tính năng Reset dữ liệu và Test coins cho lập trình viên/người chơi.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Jelly Runner/Save Manager")]
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public const string KEY_PLAYER_NAME = "PlayerName";
    public const string KEY_PLAYER_AGE = "PlayerAge";
    public const string KEY_PLAYER_AVATAR = "PlayerAvatarIndex";
    public const string KEY_HAS_COMPLETED_ONBOARDING = "HasCompletedOnboarding";
    public const string KEY_TOTAL_COINS = "TotalCoins";
    public const string KEY_BEST_SCORE = "BestScore";
    public const string KEY_SELECTED_LANGUAGE = "SelectedLanguageCode";

    public static event Action OnDataSaved;
    public static event Action OnDataReset;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveAll();
        }
    }

    private void OnApplicationQuit()
    {
        SaveAll();
    }

    /// <summary>
    /// Lưu cưỡng chế tất cả dữ liệu xuống ổ đĩa
    /// </summary>
    public static void SaveAll()
    {
        PlayerPrefs.Save();
        OnDataSaved?.Invoke();
        Debug.Log("<color=#00FF66>[SaveManager] Toàn bộ dữ liệu game đã được lưu trữ an toàn xuống đĩa!</color>");
    }

    /// <summary>
    /// Lưu thông tin hồ sơ người chơi
    /// </summary>
    public static void SaveProfile(string name, int age, int avatarIndex)
    {
        if (string.IsNullOrWhiteSpace(name)) name = "Jelly Runner";
        PlayerPrefs.SetString(KEY_PLAYER_NAME, name);
        PlayerPrefs.SetInt(KEY_PLAYER_AGE, age);
        PlayerPrefs.SetInt(KEY_PLAYER_AVATAR, avatarIndex);
        PlayerPrefs.SetInt(KEY_HAS_COMPLETED_ONBOARDING, 1);
        SaveAll();
    }

    /// <summary>
    /// Lấy thông tin hồ sơ hiện tại
    /// </summary>
    public static (string name, int age, int avatarIndex, bool hasCompletedOnboarding) GetProfile()
    {
        string name = PlayerPrefs.GetString(KEY_PLAYER_NAME, "Jelly Runner");
        int age = PlayerPrefs.GetInt(KEY_PLAYER_AGE, 18);
        int avatar = PlayerPrefs.GetInt(KEY_PLAYER_AVATAR, 0);
        bool completed = PlayerPrefs.GetInt(KEY_HAS_COMPLETED_ONBOARDING, 0) == 1;
        return (name, age, avatar, completed);
    }

    /// <summary>
    /// Cộng tiền vàng test hoặc phần thưởng
    /// </summary>
    public static void AddTestCoins(int amount)
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddCoins(amount);
        }
        else
        {
            int current = PlayerPrefs.GetInt(KEY_TOTAL_COINS, 0);
            PlayerPrefs.SetInt(KEY_TOTAL_COINS, current + amount);
            SaveAll();
        }
    }

    /// <summary>
    /// Xóa toàn bộ dữ liệu để test game từ trạng thái mới tinh (First-Time User)
    /// </summary>
    public static void ResetAllGameData()
    {
        PlayerPrefs.DeleteAll();
        SaveAll();

        // Khôi phục lại Leaderboard mặc định
        if (LeaderboardManager.Instance != null)
            LeaderboardManager.Instance.ResetToDefault();

        OnDataReset?.Invoke();
        Debug.LogWarning("[SaveManager] ĐÃ XÓA TOÀN BỘ DỮ LIỆU GAME (RESET TO DEFAULT)!");
    }
}

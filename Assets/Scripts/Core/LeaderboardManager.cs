using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

/// <summary>
/// Quản lý bảng xếp hạng Top 5 điểm cao nhất.
/// Lưu trữ bền vững bằng PlayerPrefs + JSON.
/// Lưu cả Tên Người Chơi, Avatar và Điểm Số.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Jelly Runner/Leaderboard Manager")]
public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    [Header("--- UI Hiển Thị ---")]
    [SerializeField] private TextMeshProUGUI leaderboardText;

    private const string LEADERBOARD_KEY = "Leaderboard_v2";
    private const int MAX_ENTRIES = 5;

    private LeaderboardData data;

    // Danh sách avatar đại diện (không dùng emoji để tránh lỗi font)
    private static readonly string[] AvatarIcons = new string[] { "", "", "", "", "", "" };

    [Serializable]
    public class LeaderboardEntry
    {
        public string playerName;
        public int score;
        public int avatarIndex;

        public LeaderboardEntry(string name, int score, int avatarIndex = 0)
        {
            this.playerName = name;
            this.score = score;
            this.avatarIndex = avatarIndex;
        }
    }

    [Serializable]
    public class LeaderboardData
    {
        public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadLeaderboard();
    }

    /// <summary>
    /// Kiểm tra và thêm điểm vào bảng xếp hạng nếu đủ điều kiện Top 5.
    /// Lưu kèm Avatar và Tên của người chơi.
    /// </summary>
    public void AddScore(int score)
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "Jelly Runner");
        int avatarIndex = Mathf.Clamp(PlayerPrefs.GetInt("PlayerAvatarIndex", 0), 0, AvatarIcons.Length - 1);

        // Kiểm tra có đủ điều kiện Top 5 không
        if (data.entries.Count < MAX_ENTRIES || score > data.entries[data.entries.Count - 1].score)
        {
            data.entries.Add(new LeaderboardEntry(playerName, score, avatarIndex));

            // Sắp xếp giảm dần theo điểm
            data.entries.Sort((a, b) => b.score.CompareTo(a.score));

            // Giữ tối đa MAX_ENTRIES
            if (data.entries.Count > MAX_ENTRIES)
                data.entries.RemoveRange(MAX_ENTRIES, data.entries.Count - MAX_ENTRIES);

            SaveLeaderboard();
            Debug.Log($"<color=#00FF66>[LeaderboardManager] Đã ghi danh kỷ lục mới: {playerName} ({score} điểm) vào Top 5!</color>");
        }
    }

    /// <summary>
    /// Kiểm tra xem điểm này có đủ để vào Top 5 không.
    /// </summary>
    public bool IsHighScore(int score)
    {
        if (data.entries.Count < MAX_ENTRIES) return true;
        return score > data.entries[data.entries.Count - 1].score;
    }

    /// <summary>
    /// Hiển thị bảng xếp hạng lên UI Text.
    /// </summary>
    public void DisplayLeaderboard()
    {
        if (leaderboardText == null) return;

        bool isVn = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;
        string header = isVn ? "BẢNG XẾP HẠNG TOP 5\n\n" : "TOP 5 LEADERBOARD\n\n";
        string display = header;

        for (int i = 0; i < data.entries.Count; i++)
        {
            var entry = data.entries[i];
            string medal = i switch
            {
                0 => "TOP 1.",
                1 => "TOP 2.",
                2 => "TOP 3.",
                _ => $"TOP {i + 1}."
            };

            string scoreUnit = isVn ? "điểm" : "pts";
            display += $"{medal} {entry.playerName}  —  <color=#FFDE43><b>{entry.score}</b></color> {scoreUnit}\n\n";
        }

        if (data.entries.Count == 0)
        {
            display += isVn ? "Chưa có kỷ lục nào!\nHãy chơi để ghi danh!" : "No records yet!\nPlay now to make history!";
        }

        leaderboardText.text = display;
    }

    /// <summary>
    /// Hiển thị bảng xếp hạng lên một TextMeshProUGUI tùy ý.
    /// </summary>
    public void DisplayLeaderboard(TextMeshProUGUI targetText)
    {
        if (targetText == null) return;

        TextMeshProUGUI original = leaderboardText;
        leaderboardText = targetText;
        DisplayLeaderboard();
        leaderboardText = original;
    }

    public List<LeaderboardEntry> GetEntries()
    {
        return new List<LeaderboardEntry>(data.entries);
    }

    /// <summary>
    /// Xóa toàn bộ kỷ lục và đưa về trạng thái mặc định ban đầu
    /// </summary>
    public void ClearLeaderboard()
    {
        ResetToDefault();
    }

    /// <summary>
    /// Xóa và tái lập bảng xếp hạng mẫu ban đầu
    /// </summary>
    public void ResetToDefault()
    {
        data.entries.Clear();
        InitializeDefaultEntries();
        SaveLeaderboard();
    }

    private void LoadLeaderboard()
    {
        string json = PlayerPrefs.GetString(LEADERBOARD_KEY, "");
        if (!string.IsNullOrEmpty(json))
        {
            data = JsonUtility.FromJson<LeaderboardData>(json);
        }

        if (data == null || data.entries == null || data.entries.Count == 0)
        {
            data = new LeaderboardData();
            InitializeDefaultEntries();
            SaveLeaderboard();
        }
    }

    /// <summary>
    /// Khởi tạo sẵn các kỷ lục mẫu ban đầu để bảng xếp hạng luôn sinh động và có mục tiêu thi đấu
    /// </summary>
    private void InitializeDefaultEntries()
    {
        data.entries.Add(new LeaderboardEntry("Hoàng Gia", 1500, 1));
        data.entries.Add(new LeaderboardEntry("Kim Cương", 1200, 0));
        data.entries.Add(new LeaderboardEntry("Bão Lửa", 950, 3));
        data.entries.Add(new LeaderboardEntry("Ngân Hà", 700, 2));
        data.entries.Add(new LeaderboardEntry("Tia Chớp", 500, 5));
    }

    private void SaveLeaderboard()
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(LEADERBOARD_KEY, json);
        PlayerPrefs.Save();
    }
}

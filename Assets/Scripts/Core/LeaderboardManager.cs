using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

/// <summary>
/// Quản lý bảng xếp hạng Top 5 điểm cao nhất.
/// Lưu trữ bằng PlayerPrefs + JSON. Singleton pattern.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Jelly Runner/Leaderboard Manager")]
public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    [Header("--- UI Hiển Thị ---")]
    [SerializeField] private TextMeshProUGUI leaderboardText;

    private const string LEADERBOARD_KEY = "Leaderboard";
    private const int MAX_ENTRIES = 5;

    private LeaderboardData data;

    [Serializable]
    public class LeaderboardEntry
    {
        public string playerName;
        public int score;

        public LeaderboardEntry(string name, int score)
        {
            this.playerName = name;
            this.score = score;
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
    /// </summary>
    public void AddScore(int score)
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "Player");

        // Kiểm tra có đủ điều kiện Top 5 không
        if (data.entries.Count < MAX_ENTRIES || score > data.entries[data.entries.Count - 1].score)
        {
            data.entries.Add(new LeaderboardEntry(playerName, score));

            // Sắp xếp giảm dần theo điểm
            data.entries.Sort((a, b) => b.score.CompareTo(a.score));

            // Giữ tối đa MAX_ENTRIES
            if (data.entries.Count > MAX_ENTRIES)
                data.entries.RemoveRange(MAX_ENTRIES, data.entries.Count - MAX_ENTRIES);

            SaveLeaderboard();
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

        string display = "🏆 BẢNG XẾP HẠNG 🏆\n\n";

        for (int i = 0; i < data.entries.Count; i++)
        {
            string medal = i switch
            {
                0 => "🥇",
                1 => "🥈",
                2 => "🥉",
                _ => $" {i + 1}."
            };

            display += $"{medal} {data.entries[i].playerName} — {data.entries[i].score}\n";
        }

        // Nếu chưa có ai
        if (data.entries.Count == 0)
            display += "Chưa có kỷ lục nào!\n";

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

    /// <summary>
    /// Lấy danh sách entries (read-only).
    /// </summary>
    public List<LeaderboardEntry> GetEntries()
    {
        return new List<LeaderboardEntry>(data.entries);
    }

    /// <summary>
    /// Xóa toàn bộ bảng xếp hạng.
    /// </summary>
    public void ClearLeaderboard()
    {
        data.entries.Clear();
        SaveLeaderboard();
    }

    private void LoadLeaderboard()
    {
        string json = PlayerPrefs.GetString(LEADERBOARD_KEY, "");
        if (!string.IsNullOrEmpty(json))
        {
            data = JsonUtility.FromJson<LeaderboardData>(json);
        }

        if (data == null)
            data = new LeaderboardData();
    }

    private void SaveLeaderboard()
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(LEADERBOARD_KEY, json);
        PlayerPrefs.Save();
    }
}

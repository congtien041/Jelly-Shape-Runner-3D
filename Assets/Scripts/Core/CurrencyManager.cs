using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Quản lý tiền tệ (coins) trong game. Singleton pattern.
/// Load/Save qua PlayerPrefs, cập nhật UI.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Jelly Runner/Currency Manager")]
public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("--- UI ---")]
    [SerializeField] private TextMeshProUGUI coinText;

    [Header("--- Sự Kiện ---")]
    public UnityEvent<int> OnCoinsChanged = new UnityEvent<int>();

    private const string COINS_KEY = "TotalCoins";
    private int totalCoins;

    public int TotalCoins => totalCoins;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadCoins();
    }

    private void Start()
    {
        UpdateUI();
    }

    /// <summary>
    /// Thêm coins.
    /// </summary>
    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        totalCoins += amount;
        SaveCoins();
        UpdateUI();
        OnCoinsChanged?.Invoke(totalCoins);
    }

    /// <summary>
    /// Chi tiêu coins. Trả về true nếu đủ tiền.
    /// </summary>
    public bool SpendCoins(int amount)
    {
        if (amount <= 0 || totalCoins < amount) return false;

        totalCoins -= amount;
        SaveCoins();
        UpdateUI();
        OnCoinsChanged?.Invoke(totalCoins);
        return true;
    }

    /// <summary>
    /// Kiểm tra có đủ coins không.
    /// </summary>
    public bool HasEnoughCoins(int amount)
    {
        return totalCoins >= amount;
    }

    /// <summary>
    /// Gán UI text hiển thị coin (dùng khi UI được tạo runtime).
    /// </summary>
    public void SetCoinText(TextMeshProUGUI text)
    {
        coinText = text;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (coinText != null)
            coinText.text = totalCoins.ToString();
    }

    private void LoadCoins()
    {
        totalCoins = PlayerPrefs.GetInt(COINS_KEY, 0);
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt(COINS_KEY, totalCoins);
        PlayerPrefs.Save();
    }
}

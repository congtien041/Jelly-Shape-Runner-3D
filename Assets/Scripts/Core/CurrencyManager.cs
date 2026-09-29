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

    private int secretTapCount = 0;
    private float lastTapTime = 0f;

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        // === PHÍM TẮT CHỈNH VÀNG CHO BẢN BUILD (PC / TEST) ===
        // F8: Thêm +5,000 vàng ngay lập tức
        if (Input.GetKeyDown(KeyCode.F8))
        {
            AddCoins(5000);
            AudioManager.Instance?.PlayShopBuySound();
            Debug.Log($"<color=#FFD700><b>[Cheat Vàng] Đã cộng +5,000 vàng! Tổng: {totalCoins}</b></color>");
        }
        // F9: Đặt luôn 999,999 vàng (Full Vàng)
        else if (Input.GetKeyDown(KeyCode.F9))
        {
            SetCoins(999999);
            AudioManager.Instance?.PlayHighScoreSound();
            Debug.Log($"<color=#FFD700><b>[Cheat Vàng] Đã đặt 999,999 vàng!</b></color>");
        }
        // F7: Reset vàng về 0
        else if (Input.GetKeyDown(KeyCode.F7))
        {
            SetCoins(0);
            Debug.Log("<color=#FF6666><b>[Cheat Vàng] Đã đưa vàng về 0!</b></color>");
        }
    }

    /// <summary>
    /// Chạm/click liên tục 5 lần vào số vàng để nhận +10,000 vàng (hỗ trợ cả Mobile khi build APK).
    /// </summary>
    public void SecretTapAddCoins()
    {
        if (Time.time - lastTapTime > 2.0f)
        {
            secretTapCount = 0;
        }

        secretTapCount++;
        lastTapTime = Time.time;

        if (secretTapCount >= 5)
        {
            secretTapCount = 0;
            AddCoins(10000);
            AudioManager.Instance?.PlayHighScoreSound();
            Debug.Log($"<color=#FFD700><b>[Secret Tap] Đã mở khóa +10,000 vàng!</b></color>");
        }
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
    /// Đặt trực tiếp số coins (dành cho Tool chỉnh vàng hoặc debug).
    /// </summary>
    public void SetCoins(int amount)
    {
        totalCoins = Mathf.Max(0, amount);
        SaveCoins();
        UpdateUI();
        OnCoinsChanged?.Invoke(totalCoins);
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

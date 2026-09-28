using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// ShopUI v2 — Tích hợp Layer Lab GUI Pro-CasualGame.
/// Giao diện Cửa Hàng (Shop UI) hiện đại hỗ trợ Đa Ngôn Ngữ (Localization):
/// 3 Tabs: Skin Nhân Vật, Skin Tường, Hiệu Ứng (Skills).
/// Tự động cập nhật coin và ngôn ngữ thời gian thực.
///
/// KHÔNG CÒN TỰ SINH UI BẰNG CODE — dùng Prefab Layer Lab kéo thả.
/// Hỗ trợ animation mở/đóng panel (UIAnimator).
/// Hỗ trợ sử dụng Layer Lab CardFrame/ItemFrame prefab cho item card.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Jelly Runner/Shop UI")]
public class ShopUI : MonoBehaviour
{
    [Header("--- Layer Lab Item Card Prefab ---")]
    [Tooltip("Kéo thả CardFrame05 hoặc ItemFrame02 từ Layer Lab vào đây. " +
             "Nếu để trống, sẽ tự tạo card đơn giản.")]
    [SerializeField] private GameObject itemCardPrefab;

    private GameObject shopPanel;
    private Transform contentContainer;
    private TextMeshProUGUI coinText;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI closeBtnText;

    private ShopManager.ItemType currentTab = ShopManager.ItemType.PlayerSkin;
    private Button tabPlayerBtn;
    private Button tabWallBtn;
    private Button tabEffectBtn;
    private TextMeshProUGUI tabPlayerBtnText;
    private TextMeshProUGUI tabWallBtnText;
    private TextMeshProUGUI tabEffectBtnText;

    private void Start()
    {
        FindShopUI();

        if (ShopManager.Instance != null)
            ShopManager.Instance.OnShopDataUpdated += RefreshShopItems;

        LocalizationManager.OnLanguageChanged += RefreshShopTexts;

        // Tự động nạp danh sách vật phẩm ban đầu
        RefreshShopItems();
    }

    /// <summary>
    /// Tìm ShopModalPanel kể cả khi đang inactive.
    /// GameObject.Find() KHÔNG tìm thấy inactive objects — đây là bug phổ biến.
    /// Ta duyệt qua Canvas để tìm child theo tên.
    /// </summary>
    private void FindShopUI()
    {
        // Bước 1: thử GameObject.Find (nhanh, cho trường hợp panel đang active)
        GameObject panelObj = GameObject.Find("ShopModalPanel");

        // Bước 2: Nếu không tìm thấy (panel inactive), duyệt qua tất cả Canvas
        if (panelObj == null)
        {
            Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
            foreach (Canvas c in allCanvases)
            {
                if (c == null || c.gameObject.scene.name == null) continue; // skip prefab assets
                Transform found = FindChildRecursive(c.transform, "ShopModalPanel");
                if (found != null)
                {
                    panelObj = found.gameObject;
                    break;
                }
            }
        }

        if (panelObj != null)
        {
            shopPanel = panelObj;
            titleText = panelObj.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            coinText = panelObj.transform.Find("ShopCoins")?.GetComponent<TextMeshProUGUI>();

            tabPlayerBtn = panelObj.transform.Find("TabGroup/Tab_Player")?.GetComponent<Button>();
            if (tabPlayerBtn != null)
            {
                tabPlayerBtnText = tabPlayerBtn.GetComponentInChildren<TextMeshProUGUI>();
                tabPlayerBtn.onClick.RemoveAllListeners();
                tabPlayerBtn.onClick.AddListener(() => SwitchTab(ShopManager.ItemType.PlayerSkin));
            }

            tabWallBtn = panelObj.transform.Find("TabGroup/Tab_Wall")?.GetComponent<Button>();
            if (tabWallBtn != null)
            {
                tabWallBtnText = tabWallBtn.GetComponentInChildren<TextMeshProUGUI>();
                tabWallBtn.onClick.RemoveAllListeners();
                tabWallBtn.onClick.AddListener(() => SwitchTab(ShopManager.ItemType.WallSkin));
            }

            tabEffectBtn = panelObj.transform.Find("TabGroup/Tab_Effect")?.GetComponent<Button>();
            if (tabEffectBtn != null)
            {
                tabEffectBtnText = tabEffectBtn.GetComponentInChildren<TextMeshProUGUI>();
                tabEffectBtn.onClick.RemoveAllListeners();
                tabEffectBtn.onClick.AddListener(() => SwitchTab(ShopManager.ItemType.Effect));
            }

            contentContainer = panelObj.transform.Find("ScrollView/Viewport/Content");

            Button closeBtn = panelObj.transform.Find("CloseBtn")?.GetComponent<Button>();
            if (closeBtn != null)
            {
                closeBtnText = closeBtn.GetComponentInChildren<TextMeshProUGUI>();
                closeBtn.onClick.RemoveAllListeners();
                closeBtn.onClick.AddListener(HideShop);
            }

            shopPanel.SetActive(false);

            Debug.Log($"[ShopUI] Đã tìm thấy ShopModalPanel! ContentContainer: {(contentContainer != null ? "OK" : "NULL")}");
        }
        else
        {
            Debug.LogWarning("[ShopUI] Không tìm thấy 'ShopModalPanel' trên Scene! " +
                "Hãy kéo thả Prefab Shop (Layer Lab) hoặc tự dựng ShopModalPanel theo hướng dẫn.");
        }
    }

    /// <summary>
    /// Tìm child theo tên (đệ quy), hoạt động kể cả khi child đang inactive.
    /// </summary>
    private static Transform FindChildRecursive(Transform parent, string childName)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == childName) return child;
            Transform found = FindChildRecursive(child, childName);
            if (found != null) return found;
        }
        return null;
    }

    private void OnDestroy()
    {
        if (ShopManager.Instance != null)
            ShopManager.Instance.OnShopDataUpdated -= RefreshShopItems;

        LocalizationManager.OnLanguageChanged -= RefreshShopTexts;
    }

    public void ShowShop()
    {
        if (shopPanel == null)
            FindShopUI();

        if (shopPanel != null)
        {
            shopPanel.SetActive(true);

            // ★ Animation Layer Lab: Trượt vào từ dưới
            UIAnimator.SlideInFromBottom(shopPanel);

            UpdateCoinDisplay();
            RefreshShopTexts();
            RefreshShopItems();
        }
    }

    public void HideShop()
    {
        if (shopPanel != null)
        {
            // ★ Animation Layer Lab: Trượt xuống và ẩn
            UIAnimator.SlideOutToBottom(shopPanel);
        }
    }

    private void SwitchTab(ShopManager.ItemType type)
    {
        currentTab = type;
        Color activeCol = new Color(0.05f, 0.55f, 0.88f);
        Color inactiveCol = new Color(0.12f, 0.15f, 0.24f);

        if (tabPlayerBtn != null)
            tabPlayerBtn.GetComponent<Image>().color = (type == ShopManager.ItemType.PlayerSkin) ? activeCol : inactiveCol;
        if (tabWallBtn != null)
            tabWallBtn.GetComponent<Image>().color = (type == ShopManager.ItemType.WallSkin) ? activeCol : inactiveCol;
        if (tabEffectBtn != null)
            tabEffectBtn.GetComponent<Image>().color = (type == ShopManager.ItemType.Effect) ? activeCol : inactiveCol;

        RefreshShopItems();
    }

    public void RefreshShopTexts()
    {
        if (titleText != null) titleText.text = LocalizationManager.Get("shop_title");
        if (tabPlayerBtnText != null) tabPlayerBtnText.text = LocalizationManager.Get("shop_tab_player");
        if (tabWallBtnText != null) tabWallBtnText.text = LocalizationManager.Get("shop_tab_wall");
        if (tabEffectBtnText != null) tabEffectBtnText.text = LocalizationManager.Get("shop_tab_effect");
        if (closeBtnText != null) closeBtnText.text = LocalizationManager.Get("shop_back_menu");

        RefreshShopItems();
    }

    public void RefreshShopItems()
    {
        if (shopPanel == null) FindShopUI();
        if (contentContainer == null) return;

        if (ShopManager.Instance == null)
        {
            var existingMgr = FindAnyObjectByType<ShopManager>();
            if (existingMgr == null)
            {
                var mgrObj = new GameObject("ShopManager");
                mgrObj.AddComponent<ShopManager>();
            }
        }
        if (ShopManager.Instance == null) return;

        UpdateCoinDisplay();

        // Xóa các card cũ
        for (int i = contentContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(contentContainer.GetChild(i).gameObject);
        }

        List<ShopManager.ShopItemData> items = ShopManager.Instance.GetItems(currentTab);
        int equippedIndex = ShopManager.Instance.GetEquippedIndex(currentTab);

        for (int i = 0; i < items.Count; i++)
        {
            int index = i;
            ShopManager.ShopItemData item = items[i];
            bool isEquipped = (equippedIndex == i);

            CreateItemCard(contentContainer, item, index, isEquipped);
        }
    }

    private void CreateItemCard(Transform parent, ShopManager.ShopItemData item, int index, bool isEquipped)
    {
        // Tạo card — dùng Layer Lab prefab nếu có, ngược lại tạo card bo góc cao cấp
        GameObject card;
        if (itemCardPrefab != null)
        {
            card = Instantiate(itemCardPrefab, parent);
        }
        else
        {
            card = new GameObject("Card_" + item.displayName);
            card.transform.SetParent(parent, false);
            RectTransform cardRt = card.AddComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(880, 150);

            // Card background — Equipped items get a subtle glowing border & green tint
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = isEquipped
                ? new Color(0.06f, 0.24f, 0.18f, 0.95f)
                : new Color(0.08f, 0.11f, 0.18f, 0.92f);

            // Viền highlight cho item đang trang bị
            if (isEquipped)
            {
                GameObject border = new GameObject("EquippedBorder");
                border.transform.SetParent(card.transform, false);
                RectTransform bRt = border.AddComponent<RectTransform>();
                bRt.anchorMin = Vector2.zero;
                bRt.anchorMax = Vector2.one;
                bRt.offsetMin = new Vector2(-3, -3);
                bRt.offsetMax = new Vector2(3, 3);
                bRt.SetAsFirstSibling();
                Image bImg = border.AddComponent<Image>();
                bImg.color = new Color(0.18f, 0.95f, 0.55f, 0.7f);
            }
        }
        card.name = "Card_" + item.displayName;

        // BẮT BUỘC có LayoutElement để VerticalLayoutGroup + ContentSizeFitter hoạt động chính xác
        LayoutElement le = card.GetComponent<LayoutElement>() ?? card.AddComponent<LayoutElement>();
        le.minHeight = 150f;
        le.preferredHeight = 150f;
        le.minWidth = 880f;
        le.preferredWidth = 880f;
        le.flexibleWidth = 1f;

        // Preview Box — Khung hiển thị màu/vật liệu phát sáng
        GameObject previewFrame = new GameObject("PreviewFrame");
        previewFrame.transform.SetParent(card.transform, false);
        RectTransform pfRt = previewFrame.AddComponent<RectTransform>();
        pfRt.anchorMin = new Vector2(0f, 0.5f);
        pfRt.anchorMax = new Vector2(0f, 0.5f);
        pfRt.pivot = new Vector2(0f, 0.5f);
        pfRt.anchoredPosition = new Vector2(20, 0);
        pfRt.sizeDelta = new Vector2(110, 110);
        Image pfImg = previewFrame.AddComponent<Image>();
        pfImg.color = new Color(0.15f, 0.18f, 0.28f, 0.95f);

        GameObject previewObj = new GameObject("PreviewColor");
        previewObj.transform.SetParent(previewFrame.transform, false);
        RectTransform prevRt = previewObj.AddComponent<RectTransform>();
        prevRt.anchorMin = Vector2.zero;
        prevRt.anchorMax = Vector2.one;
        prevRt.offsetMin = new Vector2(8, 8);
        prevRt.offsetMax = new Vector2(-8, -8);
        Image prevImg = previewObj.AddComponent<Image>();
        prevImg.color = item.previewColor;

        // Display Name — Tên sản phẩm
        CreateCardText(card.transform, "ItemName", item.displayName,
            new Vector2(0f, 0.5f), new Vector2(150, 22), new Vector2(400, 46),
            34, Color.white, TextAlignmentOptions.Left);

        // Price / Status text
        string statusStr = item.isUnlocked
            ? (isEquipped ? LocalizationManager.Get("shop_status_equipped") : LocalizationManager.Get("shop_status_owned"))
            : LocalizationManager.Get("shop_price_format", item.price);

        Color statusColor = item.isUnlocked
            ? (isEquipped ? new Color(0.25f, 0.95f, 0.55f) : new Color(0.55f, 0.65f, 0.8f))
            : new Color(1f, 0.85f, 0.2f);

        CreateCardText(card.transform, "ItemStatus", statusStr,
            new Vector2(0f, 0.5f), new Vector2(150, -22), new Vector2(400, 36),
            26, statusColor, TextAlignmentOptions.Left);

        // Action Button — Nút hành động bên phải
        string btnLabel;
        Color btnColor;
        if (isEquipped)
        {
            btnLabel = LocalizationManager.Get("shop_btn_equipped");
            btnColor = new Color(0.12f, 0.58f, 0.35f);
        }
        else if (item.isUnlocked)
        {
            btnLabel = LocalizationManager.Get("shop_btn_equip");
            btnColor = new Color(0.05f, 0.55f, 0.88f);
        }
        else
        {
            btnLabel = LocalizationManager.Get("shop_btn_buy");
            btnColor = new Color(0.95f, 0.55f, 0.05f);
        }

        Button actionBtn = CreateCardButton(card.transform, "ActionBtn", btnLabel,
            new Vector2(1f, 0.5f), new Vector2(-22, 0), new Vector2(220, 82),
            28, btnColor);

        actionBtn.onClick.AddListener(() =>
        {
            if (ShopManager.Instance == null) return;

            if (item.isUnlocked)
            {
                ShopManager.Instance.EquipItem(currentTab, index);
                if (UIParticleFXManager.Instance != null)
                    UIParticleFXManager.Instance.PlaySparkle(actionBtn.GetComponent<RectTransform>());
            }
            else
            {
                bool success = ShopManager.Instance.BuyItem(currentTab, index);
                if (success)
                {
                    if (UIParticleFXManager.Instance != null)
                        UIParticleFXManager.Instance.PlaySpreadCircle(actionBtn.GetComponent<RectTransform>());
                }
                else
                {
                    Debug.LogWarning("[ShopUI] Không đủ vàng để mua vật phẩm này!");
                    AudioManager.Instance?.PlayObstacleHitSound();
                }
            }

            RefreshShopItems();
        });
    }

    private void UpdateCoinDisplay()
    {
        if (coinText != null)
        {
            int total = CurrencyManager.Instance != null ? CurrencyManager.Instance.TotalCoins : PlayerPrefs.GetInt("TotalCoins", 0);
            coinText.text = LocalizationManager.Get("menu_coin_format", total);
        }
    }

    // =========================================================================
    // HELPERS — Chỉ dùng cho sinh item card động (không dùng cho panel tĩnh)
    // =========================================================================

    private static TextMeshProUGUI CreateCardText(Transform parent, string name, string text,
        Vector2 anchor, Vector2 anchoredPos, Vector2 size,
        int fontSize, Color color, TextAlignmentOptions align)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = align;
        tmp.fontStyle = FontStyles.Bold;
        return tmp;
    }

    private static Button CreateCardButton(Transform parent, string name, string label,
        Vector2 anchor, Vector2 anchoredPos, Vector2 size,
        int fontSize, Color bgColor)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.color = bgColor;

        Button btn = obj.AddComponent<Button>();

        CreateCardText(obj.transform, name + "_Label", label,
            Vector2.zero, Vector2.zero, Vector2.zero,
            fontSize, Color.white, TextAlignmentOptions.Center);

        // Stretch label trong button
        RectTransform labelRt = obj.transform.Find(name + "_Label")?.GetComponent<RectTransform>();
        if (labelRt != null)
        {
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.pivot = new Vector2(0.5f, 0.5f);
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;
        }

        return btn;
    }
}

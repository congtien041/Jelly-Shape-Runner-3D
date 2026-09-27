using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Giao diện Cửa Hàng (Shop UI) hiện đại hỗ trợ Đa Ngôn Ngữ (Localization):
/// 3 Tabs: Skin Nhân Vật, Skin Tường, Hiệu Ứng (Skills).
/// Tự động cập nhật coin và ngôn ngữ thời gian thực.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Jelly Runner/Shop UI")]
public class ShopUI : MonoBehaviour
{
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
        FindOrBuildShopUI();

        if (ShopManager.Instance != null)
            ShopManager.Instance.OnShopDataUpdated += RefreshShopItems;

        LocalizationManager.OnLanguageChanged += RefreshShopTexts;
    }

    private void FindOrBuildShopUI()
    {
        GameObject panelObj = GameObject.Find("ShopModalPanel");
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
        }
        else
        {
            BuildShopUI();
        }
    }

    private void OnDestroy()
    {
        if (ShopManager.Instance != null)
            ShopManager.Instance.OnShopDataUpdated -= RefreshShopItems;

        LocalizationManager.OnLanguageChanged -= RefreshShopTexts;
    }

    public void ShowShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            UpdateCoinDisplay();
            RefreshShopTexts();
        }
    }

    public void HideShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    private void BuildShopUI()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindAnyObjectByType<Canvas>();

        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("ShopCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Panel nền
        shopPanel = new GameObject("ShopModalPanel");
        shopPanel.transform.SetParent(canvas.transform, false);
        RectTransform rt = shopPanel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image bg = shopPanel.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.05f, 0.09f, 0.96f);

        // Header Title
        titleText = CreateText(shopPanel.transform, "Title", LocalizationManager.Get("shop_title"),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -90), new Vector2(700, 80), 54, new Color(0f, 0.9f, 1f), TextAlignmentOptions.Center);

        // Coin Display
        coinText = CreateText(shopPanel.transform, "ShopCoins", "🪙 0",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-40, -90), new Vector2(300, 60), 38, new Color(1f, 0.85f, 0.1f), TextAlignmentOptions.Right);

        // --- TAB BUTTONS ---
        GameObject tabGroup = new GameObject("TabGroup");
        tabGroup.transform.SetParent(shopPanel.transform, false);
        RectTransform tabRt = tabGroup.AddComponent<RectTransform>();
        tabRt.anchorMin = new Vector2(0.5f, 1f);
        tabRt.anchorMax = new Vector2(0.5f, 1f);
        tabRt.pivot = new Vector2(0.5f, 1f);
        tabRt.anchoredPosition = new Vector2(0, -180);
        tabRt.sizeDelta = new Vector2(980, 80);

        tabPlayerBtn = CreateButton(tabGroup.transform, "Tab_Player", LocalizationManager.Get("shop_tab_player"),
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(0, 0), new Vector2(310, 75), 30, new Color(0f, 0.6f, 0.8f), out tabPlayerBtnText);
        tabPlayerBtn.onClick.AddListener(() => SwitchTab(ShopManager.ItemType.PlayerSkin));

        tabWallBtn = CreateButton(tabGroup.transform, "Tab_Wall", LocalizationManager.Get("shop_tab_wall"),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 0), new Vector2(310, 75), 30, new Color(0.2f, 0.25f, 0.35f), out tabWallBtnText);
        tabWallBtn.onClick.AddListener(() => SwitchTab(ShopManager.ItemType.WallSkin));

        tabEffectBtn = CreateButton(tabGroup.transform, "Tab_Effect", LocalizationManager.Get("shop_tab_effect"),
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0, 0), new Vector2(310, 75), 28, new Color(0.2f, 0.25f, 0.35f), out tabEffectBtnText);
        tabEffectBtn.onClick.AddListener(() => SwitchTab(ShopManager.ItemType.Effect));

        // --- SCROLL VIEW FOR ITEMS ---
        GameObject scrollObj = new GameObject("ScrollView");
        scrollObj.transform.SetParent(shopPanel.transform, false);
        RectTransform scrollRt = scrollObj.AddComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0.5f, 0.5f);
        scrollRt.anchorMax = new Vector2(0.5f, 0.5f);
        scrollRt.pivot = new Vector2(0.5f, 0.5f);
        scrollRt.anchoredPosition = new Vector2(0, -40);
        scrollRt.sizeDelta = new Vector2(980, 1250);

        ScrollRect scrollRect = scrollObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform viewRt = viewport.AddComponent<RectTransform>();
        viewRt.anchorMin = Vector2.zero;
        viewRt.anchorMax = Vector2.one;
        viewRt.offsetMin = Vector2.zero;
        viewRt.offsetMax = Vector2.zero;
        viewport.AddComponent<RectMask2D>();
        scrollRect.viewport = viewRt;

        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRt = content.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = new Vector2(0, 1000);

        VerticalLayoutGroup vLayout = content.AddComponent<VerticalLayoutGroup>();
        vLayout.spacing = 25;
        vLayout.padding = new RectOffset(10, 10, 20, 20);
        vLayout.childAlignment = TextAnchor.UpperCenter;
        vLayout.childControlHeight = false;
        vLayout.childControlWidth = true;
        vLayout.childForceExpandHeight = false;
        vLayout.childForceExpandWidth = true;

        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRt;
        contentContainer = content.transform;

        // Nút Đóng
        Button closeBtn = CreateButton(shopPanel.transform, "CloseBtn", LocalizationManager.Get("shop_back_menu"),
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 90), new Vector2(500, 85), 36, new Color(0.85f, 0.25f, 0.25f), out closeBtnText);
        closeBtn.onClick.AddListener(HideShop);

        shopPanel.SetActive(false);
    }

    private void SwitchTab(ShopManager.ItemType type)
    {
        currentTab = type;
        Color activeCol = new Color(0f, 0.75f, 0.95f);
        Color inactiveCol = new Color(0.18f, 0.22f, 0.32f);

        tabPlayerBtn.GetComponent<Image>().color = (type == ShopManager.ItemType.PlayerSkin) ? activeCol : inactiveCol;
        tabWallBtn.GetComponent<Image>().color = (type == ShopManager.ItemType.WallSkin) ? activeCol : inactiveCol;
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
        if (contentContainer == null || ShopManager.Instance == null) return;

        UpdateCoinDisplay();

        // Xóa các card cũ
        foreach (Transform child in contentContainer)
            Destroy(child.gameObject);

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
        GameObject card = new GameObject("Card_" + item.displayName);
        card.transform.SetParent(parent, false);

        RectTransform cardRt = card.AddComponent<RectTransform>();
        cardRt.sizeDelta = new Vector2(940, 160);

        Image cardBg = card.AddComponent<Image>();
        cardBg.color = isEquipped ? new Color(0.08f, 0.25f, 0.22f, 0.9f) : new Color(0.12f, 0.15f, 0.22f, 0.9f);

        // Preview Box / Icon
        GameObject previewObj = new GameObject("PreviewColor");
        previewObj.transform.SetParent(card.transform, false);
        RectTransform prevRt = previewObj.AddComponent<RectTransform>();
        prevRt.anchorMin = new Vector2(0f, 0.5f);
        prevRt.anchorMax = new Vector2(0f, 0.5f);
        prevRt.pivot = new Vector2(0f, 0.5f);
        prevRt.anchoredPosition = new Vector2(25, 0);
        prevRt.sizeDelta = new Vector2(110, 110);
        Image prevImg = previewObj.AddComponent<Image>();
        prevImg.color = item.previewColor;

        // Display Name
        CreateText(card.transform, "ItemName", item.displayName,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(165, 25), new Vector2(400, 50), 38, Color.white, TextAlignmentOptions.Left);

        // Price / Status text
        string statusStr = item.isUnlocked
            ? (isEquipped ? LocalizationManager.Get("shop_status_equipped") : LocalizationManager.Get("shop_status_owned"))
            : LocalizationManager.Get("shop_price_format", item.price);

        Color statusColor = item.isUnlocked ? (isEquipped ? new Color(0.2f, 1f, 0.5f) : Color.gray) : new Color(1f, 0.85f, 0.2f);
        CreateText(card.transform, "ItemStatus", statusStr,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(165, -25), new Vector2(400, 40), 28, statusColor, TextAlignmentOptions.Left);

        // Action Button
        string btnLabel;
        Color btnColor;
        if (isEquipped)
        {
            btnLabel = LocalizationManager.Get("shop_btn_equipped");
            btnColor = new Color(0.15f, 0.6f, 0.35f);
        }
        else if (item.isUnlocked)
        {
            btnLabel = LocalizationManager.Get("shop_btn_equip");
            btnColor = new Color(0f, 0.7f, 0.95f);
        }
        else
        {
            btnLabel = LocalizationManager.Get("shop_btn_buy");
            btnColor = new Color(1f, 0.6f, 0f);
        }

        Button actionBtn = CreateButton(card.transform, "ActionBtn", btnLabel,
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(-30, 0), new Vector2(240, 90), 30, btnColor, out _);

        actionBtn.onClick.AddListener(() =>
        {
            if (ShopManager.Instance == null) return;

            if (item.isUnlocked)
            {
                ShopManager.Instance.EquipItem(currentTab, index);
            }
            else
            {
                ShopManager.Instance.BuyItem(currentTab, index);
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
}

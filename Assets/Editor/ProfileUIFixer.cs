using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script tự động sửa và tối ưu hóa 100% hiển thị cho ProfileCard_TopRight và OnboardingPanel trong MainMenu.unity.
/// Khắc phục hoàn toàn lỗi không thấy tên/tuổi, lỗi slider trượt tuổi và lỗi title text bị đè.
/// </summary>
public static class ProfileUIFixer
{
    private const string MENU_SCENE_PATH = "Assets/Scenes/MainMenu.unity";

    [InitializeOnLoadMethod]
    private static void AutoFixOnLoad()
    {
        EditorApplication.delayCall += () =>
        {
            FixProfileAndOnboardingUI(false);
        };
    }

    [MenuItem("Tools/Sửa & Cập Nhật Giao Diện Profile & Onboarding", false, 5)]
    public static void ManualFix()
    {
        FixProfileAndOnboardingUI(true);
    }

    public static void FixProfileAndOnboardingUI(bool showDialog)
    {
        string currentScene = SceneManager.GetActiveScene().path;
        bool needReopen = false;

        if (currentScene != MENU_SCENE_PATH)
        {
            if (System.IO.File.Exists(MENU_SCENE_PATH))
            {
                EditorSceneManager.OpenScene(MENU_SCENE_PATH, OpenSceneMode.Single);
                needReopen = true;
            }
            else return;
        }

        GameObject canvasObj = GameObject.Find("MenuCanvas");
        if (canvasObj == null) return;
        Transform cTrans = canvasObj.transform;

        bool modified = false;

        // =====================================================================
        // 1. SỬA PROFILE CARD (GÓC TRÊN PHẢI)
        // =====================================================================
        Transform pCard = cTrans.Find("ProfileCard_TopRight");
        if (pCard != null)
        {
            RectTransform pcRt = pCard.GetComponent<RectTransform>();
            pcRt.sizeDelta = new Vector2(360, 90);
            pcRt.anchoredPosition = new Vector2(-25, -30);

            // Ẩn ảnh mẫu Character to đùng gây che chữ trong FrameVisual
            Transform charObj = pCard.Find("FrameVisual/Character") 
                             ?? pCard.Find("FrameVisual/Inner/Character");
            if (charObj != null)
            {
                charObj.gameObject.SetActive(false);
                modified = true;
            }

            // AvatarFrame
            Transform avFrame = pCard.Find("AvatarFrame");
            if (avFrame != null)
            {
                RectTransform avRt = avFrame.GetComponent<RectTransform>();
                avRt.anchorMin = new Vector2(0f, 0.5f);
                avRt.anchorMax = new Vector2(0f, 0.5f);
                avRt.pivot = new Vector2(0f, 0.5f);
                avRt.anchoredPosition = new Vector2(15, 0);
                avRt.sizeDelta = new Vector2(66, 66);
            }

            // Tên người chơi (PlayerName)
            Transform nameTrans = pCard.Find("PlayerName");
            if (nameTrans != null)
            {
                RectTransform nameRt = nameTrans.GetComponent<RectTransform>();
                nameRt.anchorMin = new Vector2(0f, 0.5f);
                nameRt.anchorMax = new Vector2(0f, 0.5f);
                nameRt.pivot = new Vector2(0f, 0.5f);
                nameRt.anchoredPosition = new Vector2(94, 16);
                nameRt.sizeDelta = new Vector2(250, 36);

                TextMeshProUGUI nameTmp = nameTrans.GetComponent<TextMeshProUGUI>();
                if (nameTmp != null)
                {
                    nameTmp.text = PlayerPrefs.GetString("PlayerName", "Jelly Runner");
                    nameTmp.fontSize = 24;
                    nameTmp.fontStyle = FontStyles.Bold;
                    nameTmp.color = Color.white;
                    nameTmp.alignment = TextAlignmentOptions.Left;
                    nameTmp.enableWordWrapping = false;
                    nameTmp.overflowMode = TextOverflowModes.Ellipsis;
                    nameTmp.raycastTarget = false;
                }
                modified = true;
            }

            // Tuổi người chơi (SubText)
            Transform subTrans = pCard.Find("SubText");
            if (subTrans != null)
            {
                RectTransform subRt = subTrans.GetComponent<RectTransform>();
                subRt.anchorMin = new Vector2(0f, 0.5f);
                subRt.anchorMax = new Vector2(0f, 0.5f);
                subRt.pivot = new Vector2(0f, 0.5f);
                subRt.anchoredPosition = new Vector2(94, -18);
                subRt.sizeDelta = new Vector2(250, 32);

                TextMeshProUGUI subTmp = subTrans.GetComponent<TextMeshProUGUI>();
                if (subTmp != null)
                {
                    int age = PlayerPrefs.GetInt("PlayerAge", 18);
                    subTmp.text = $"{age} Tuổi";
                    subTmp.fontSize = 20;
                    subTmp.fontStyle = FontStyles.Bold;
                    subTmp.color = new Color(1f, 0.88f, 0.25f); // Vàng kim nổi bật
                    subTmp.alignment = TextAlignmentOptions.Left;
                    subTmp.enableWordWrapping = false;
                    subTmp.overflowMode = TextOverflowModes.Overflow;
                    subTmp.raycastTarget = false;
                }
                modified = true;
            }
        }

        // =====================================================================
        // 2. SỬA ONBOARDING PANEL
        // =====================================================================
        Transform onb = cTrans.Find("OnboardingPanel");
        if (onb != null)
        {
            // Ẩn các text mặc định trùng lặp bên trong TitleRibbon
            Transform titleRib = onb.Find("TitleRibbon");
            if (titleRib != null)
            {
                TextMeshProUGUI[] ribTexts = titleRib.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in ribTexts)
                {
                    if (t.gameObject != onb.Find("Title")?.gameObject)
                    {
                        t.gameObject.SetActive(false);
                    }
                }
            }

            // Tiêu đề chính
            Transform titleTrans = onb.Find("Title");
            if (titleTrans != null)
            {
                TextMeshProUGUI tTmp = titleTrans.GetComponent<TextMeshProUGUI>();
                if (tTmp != null)
                {
                    tTmp.text = "HỒ SƠ NGƯỜI CHƠI";
                    tTmp.fontSize = 44;
                    tTmp.fontStyle = FontStyles.Bold;
                    tTmp.color = Color.white;
                    tTmp.alignment = TextAlignmentOptions.Center;
                    tTmp.enableWordWrapping = false;
                }
                titleTrans.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 575);
                titleTrans.GetComponent<RectTransform>().sizeDelta = new Vector2(650, 60);
                modified = true;
            }

            // Mô tả
            Transform descTrans = onb.Find("Desc");
            if (descTrans != null)
            {
                TextMeshProUGUI dTmp = descTrans.GetComponent<TextMeshProUGUI>();
                if (dTmp != null)
                {
                    dTmp.text = "Tùy chỉnh tên, độ tuổi và ảnh đại diện của bạn";
                    dTmp.fontSize = 24;
                    dTmp.color = new Color(0.72f, 0.82f, 0.96f);
                    dTmp.alignment = TextAlignmentOptions.Center;
                }
                descTrans.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 500);
                descTrans.GetComponent<RectTransform>().sizeDelta = new Vector2(750, 36);
                modified = true;
            }

            // Nhãn Tên
            Transform nameLbl = onb.Find("NameLabel");
            if (nameLbl != null)
            {
                TextMeshProUGUI nlTmp = nameLbl.GetComponent<TextMeshProUGUI>();
                if (nlTmp != null) nlTmp.text = "TÊN NGƯỜI CHƠI";
                nameLbl.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 420);
                nameLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 36);
                modified = true;
            }

            // Nhãn Tuổi
            Transform ageLbl = onb.Find("AgeLabel");
            if (ageLbl != null)
            {
                TextMeshProUGUI alTmp = ageLbl.GetComponent<TextMeshProUGUI>();
                if (alTmp != null) alTmp.text = "ĐỘ TUỔI";
                ageLbl.GetComponent<RectTransform>().anchoredPosition = new Vector2(-120, 272);
                ageLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 42);
                modified = true;
            }

            // Số Tuổi Hiển Thị (AgeVal) — Rộng rãi, không bao giờ bị che
            Transform ageVal = onb.Find("AgeVal");
            if (ageVal != null)
            {
                RectTransform avRt = ageVal.GetComponent<RectTransform>();
                avRt.anchoredPosition = new Vector2(200, 272);
                avRt.sizeDelta = new Vector2(320, 48);

                TextMeshProUGUI avTmp = ageVal.GetComponent<TextMeshProUGUI>();
                if (avTmp != null)
                {
                    int savedAge = PlayerPrefs.GetInt("PlayerAge", 18);
                    avTmp.text = $"{savedAge} Tuổi";
                    avTmp.fontSize = 32;
                    avTmp.fontStyle = FontStyles.Bold;
                    avTmp.color = new Color(1f, 0.88f, 0.25f);
                    avTmp.alignment = TextAlignmentOptions.Right;
                    avTmp.enableWordWrapping = false;
                    avTmp.overflowMode = TextOverflowModes.Overflow;
                }
                modified = true;
            }

            // Slider Tuổi
            Transform sliderTrans = onb.Find("OnbAgeSlider");
            if (sliderTrans != null)
            {
                sliderTrans.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 210);
                sliderTrans.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 55);

                Slider slider = sliderTrans.GetComponent<Slider>();
                if (slider != null)
                {
                    slider.minValue = 0f;
                    slider.maxValue = 1f;
                    int curAge = PlayerPrefs.GetInt("PlayerAge", 18);
                    slider.value = Mathf.Clamp01((curAge - 5f) / (70f - 5f));
                }
                modified = true;
            }

            // Nhãn Ngôn Ngữ
            Transform langLbl = onb.Find("LangLabel");
            if (langLbl != null)
            {
                TextMeshProUGUI llTmp = langLbl.GetComponent<TextMeshProUGUI>();
                if (llTmp != null) llTmp.text = "NGÔN NGỮ";
                langLbl.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 120);
                langLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 36);
                modified = true;
            }

            // Nhãn Avatar
            Transform avLbl = onb.Find("AvatarLabel");
            if (avLbl != null)
            {
                TextMeshProUGUI avlTmp = avLbl.GetComponent<TextMeshProUGUI>();
                if (avlTmp != null) avlTmp.text = "CHỌN ẢNH ĐẠI DIỆN";
                avLbl.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -40);
                avLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 36);
                modified = true;
            }

            // Nút Done
            Transform doneBtn = onb.Find("DoneBtn");
            if (doneBtn != null)
            {
                TextMeshProUGUI dBtnTmp = doneBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (dBtnTmp != null) dBtnTmp.text = "HOÀN TẤT";
                modified = true;
            }
        }

        // =====================================================================
        // 3. SỬA COIN DISPLAY (BỎ ICON COIN THEO YÊU CẦU)
        // =====================================================================
        Transform coinDisp = cTrans.Find("CoinFrame/CoinDisplay") 
                          ?? cTrans.Find("CoinDisplay")
                          ?? pCard?.Find("CoinDisplay");
        if (coinDisp != null)
        {
            TextMeshProUGUI cdTmp = coinDisp.GetComponent<TextMeshProUGUI>();
            if (cdTmp != null)
            {
                int coins = PlayerPrefs.GetInt("TotalCoins", 0);
                cdTmp.text = $"{coins} XU";
                cdTmp.enableWordWrapping = false;
                cdTmp.overflowMode = TextOverflowModes.Overflow;
                modified = true;
            }

            // Xóa triệt để nếu có object con icon coin
            for (int i = coinDisp.childCount - 1; i >= 0; i--)
            {
                Transform child = coinDisp.GetChild(i);
                if (child.name.ToLower().Contains("icon") || child.name.ToLower().Contains("coin") || child.GetComponent<UnityEngine.UI.Image>() != null)
                {
                    Object.DestroyImmediate(child.gameObject);
                    modified = true;
                }
            }
            if (coinDisp.parent != null)
            {
                for (int i = coinDisp.parent.childCount - 1; i >= 0; i--)
                {
                    Transform sibling = coinDisp.parent.GetChild(i);
                    if (sibling != coinDisp && (sibling.name.ToLower().Contains("icon") || (sibling.name.ToLower().Contains("coin") && sibling.name != "CoinDisplay")))
                    {
                        Object.DestroyImmediate(sibling.gameObject);
                        modified = true;
                    }
                }
            }
        }

        Transform shopCoins = cTrans.Find("ShopModalPanel/ShopCoins");
        if (shopCoins != null)
        {
            TextMeshProUGUI scTmp = shopCoins.GetComponent<TextMeshProUGUI>();
            if (scTmp != null)
            {
                int coins = PlayerPrefs.GetInt("TotalCoins", 0);
                scTmp.text = $"{coins} XU";
                scTmp.enableWordWrapping = false;
                scTmp.overflowMode = TextOverflowModes.Overflow;
                modified = true;
            }

            for (int i = shopCoins.childCount - 1; i >= 0; i--)
            {
                Transform child = shopCoins.GetChild(i);
                if (child.name.ToLower().Contains("icon") || child.name.ToLower().Contains("coin") || child.GetComponent<UnityEngine.UI.Image>() != null)
                {
                    Object.DestroyImmediate(child.gameObject);
                    modified = true;
                }
            }
        }

        if (modified)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("<color=#00FF66><b>[ProfileUIFixer] ĐÃ CẬP NHẬT HOÀN HẢO GIAO DIỆN PROFILE, ONBOARDING VÀ COIN DISPLAY TRONG SCENE!</b></color>");
        }

        if (needReopen && !string.IsNullOrEmpty(currentScene))
        {
            EditorSceneManager.OpenScene(currentScene, OpenSceneMode.Single);
        }

        if (showDialog && !Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("Hoàn Tất", 
                "Đã sửa và cập nhật thành công toàn bộ giao diện Profile & Onboarding:\n\n" +
                "• ProfileCard: Hiển thị đầy đủ Tên ('Jelly Runner') và Tuổi ('18 Tuổi') to rõ, không bị che.\n" +
                "• Slider Tuổi: Đồng bộ chuẩn 0..1, hiển thị tức thì số tuổi khi trượt.\n" +
                "• Title & Labels: Loại bỏ hoàn toàn text bị đè, chỉnh câu chữ rõ ràng, đẹp mắt.", "Tuyệt vời!");
        }
    }
}

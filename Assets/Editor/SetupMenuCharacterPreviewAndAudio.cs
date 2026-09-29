using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.IO;

/// <summary>
/// Script tự động cài đặt Nhân Vật Clone 3D tại Main Menu và gán toàn bộ 15 âm thanh SFX & BGM.
/// </summary>
public static class SetupMenuCharacterPreviewAndAudio
{
    private const string MAIN_MENU_SCENE = "Assets/Scenes/MainMenu.unity";
    private const string SAMPLE_SCENE = "Assets/Scenes/SampleScene.unity";
    private const string MENU_CANVAS_PREFAB = "Assets/Prefabs/UI/MenuCanvas.prefab";

    [InitializeOnLoadMethod]
    private static void AutoRunOnCompile()
    {
        EditorApplication.delayCall += () =>
        {
            string flagFile = "Assets/Audio/SFX_TabSwitch.wav";
            if (!File.Exists(flagFile))
            {
                RunFullSetup();
            }
        };
    }

    [MenuItem("Tools/🚀 Cài Đặt Nhân Vật Clone Menu & Toàn Bộ Âm Thanh", false, 1)]
    public static void RunFullSetup()
    {
        Debug.Log("<color=#00E5FF><b>[Setup] Bắt đầu cấu hình Nhân Vật Clone và Toàn Bộ Âm Thanh...</b></color>");

        // 1. Tạo toàn bộ 15 file WAV nếu chưa có
        AudioGenerator.GenerateAllAudioFiles(false);

        // 2. Cài đặt vào Scene MainMenu.unity
        SetupMainMenuScene();

        // 3. Cài đặt vào Scene SampleScene.unity (Gameplay)
        SetupGameplayScene();

        // 4. Cập nhật MenuCanvas Prefab
        SetupMenuCanvasPrefab();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("<color=#00FF66><b>[Setup] Hoàn tất 100%! Đã tạo Nhân Vật Clone 3D xoay tại Menu và kết nối đầy đủ toàn bộ âm thanh!</b></color>");
    }

    private static void SetupMainMenuScene()
    {
        if (!File.Exists(MAIN_MENU_SCENE)) return;

        var scene = EditorSceneManager.OpenScene(MAIN_MENU_SCENE, OpenSceneMode.Single);

        // 1. Tìm MenuCanvas và MainMenuPanel
        GameObject canvasObj = GameObject.Find("MenuCanvas");
        if (canvasObj == null)
        {
            Debug.LogWarning("[Setup] Không tìm thấy MenuCanvas trong MainMenu.unity");
            return;
        }

        Transform mainMenuPanel = canvasObj.transform.Find("MainMenuPanel");
        if (mainMenuPanel == null)
        {
            Debug.LogWarning("[Setup] Không tìm thấy MainMenuPanel trong MenuCanvas");
            return;
        }

        // 2. Tìm hoặc tạo CharacterPreviewArea với chuẩn RectTransform
        Transform previewTrans = mainMenuPanel.Find("CharacterPreviewArea");
        RectTransform rt;
        if (previewTrans != null)
        {
            rt = previewTrans as RectTransform;
            if (rt == null)
            {
                Object.DestroyImmediate(previewTrans.gameObject);
                previewTrans = null;
            }
        }

        GameObject previewObj;
        if (previewTrans == null)
        {
            previewObj = new GameObject("CharacterPreviewArea", typeof(RectTransform));
            previewObj.transform.SetParent(mainMenuPanel, false);
            rt = previewObj.GetComponent<RectTransform>();
        }
        else
        {
            previewObj = previewTrans.gameObject;
            rt = previewObj.GetComponent<RectTransform>();
        }

        // Cấu hình RectTransform
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, 45f); // Đặt ở vị trí trung tâm, thoáng đãng
        rt.sizeDelta = new Vector2(560f, 560f);

        // RawImage hiển thị RenderTexture trong suốt
        RawImage rawImg = previewObj.GetComponent<RawImage>() ?? previewObj.AddComponent<RawImage>();
        rawImg.color = Color.white;
        rawImg.raycastTarget = true; // Cho phép vuốt xoay 360 độ

        // MenuCharacterPreview script
        MenuCharacterPreview previewScript = previewObj.GetComponent<MenuCharacterPreview>() ?? previewObj.AddComponent<MenuCharacterPreview>();

        // Thêm nhãn hướng dẫn vuốt tinh tế
        Transform hintTrans = previewObj.transform.Find("SwipeHintText");
        RectTransform hintRt;
        if (hintTrans != null && !(hintTrans is RectTransform))
        {
            Object.DestroyImmediate(hintTrans.gameObject);
            hintTrans = null;
        }

        GameObject hintObj;
        if (hintTrans == null)
        {
            hintObj = new GameObject("SwipeHintText", typeof(RectTransform));
            hintObj.transform.SetParent(previewObj.transform, false);
            hintRt = hintObj.GetComponent<RectTransform>();
        }
        else
        {
            hintObj = hintTrans.gameObject;
            hintRt = hintObj.GetComponent<RectTransform>();
        }

        hintRt.anchorMin = new Vector2(0.5f, 0f);
        hintRt.anchorMax = new Vector2(0.5f, 0f);
        hintRt.pivot = new Vector2(0.5f, 0.5f);
        hintRt.anchoredPosition = new Vector2(0f, 20f);
        hintRt.sizeDelta = new Vector2(400f, 36f);

        TextMeshProUGUI hintText = hintObj.GetComponent<TextMeshProUGUI>() ?? hintObj.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) hintText.font = TMP_Settings.defaultFontAsset;
        hintText.text = "◄ Vuốt để xoay 360° ►";
        hintText.fontSize = 20;
        hintText.color = new Color(0.15f, 0.85f, 1f, 0.65f);
        hintText.alignment = TextAlignmentOptions.Center;
        hintText.fontStyle = FontStyles.Bold;
        hintText.raycastTarget = false;

        // Đặt SiblingIndex cho CharacterPreviewArea để nó nằm ngay sau TitleText nhưng trước PlayButton
        previewObj.transform.SetSiblingIndex(2);

        // 3. Gán toàn bộ âm thanh vào AudioManager trên scene MainMenu
        AssignAllAudioClipsInScene();

        EditorUtility.SetDirty(previewObj);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("<color=#00FF66>[Setup] Đã cấu hình thành công Nhân Vật Clone trên Scene MainMenu.unity!</color>");
    }

    private static void SetupGameplayScene()
    {
        if (!File.Exists(SAMPLE_SCENE)) return;

        var scene = EditorSceneManager.OpenScene(SAMPLE_SCENE, OpenSceneMode.Single);
        AssignAllAudioClipsInScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("<color=#00FF66>[Setup] Đã kết nối âm thanh cho Scene Gameplay (SampleScene.unity)!</color>");
    }

    private static void SetupMenuCanvasPrefab()
    {
        if (!File.Exists(MENU_CANVAS_PREFAB)) return;

        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(MENU_CANVAS_PREFAB);
        if (prefabRoot == null) return;

        Transform mainMenuPanel = prefabRoot.transform.Find("MainMenuPanel");
        if (mainMenuPanel != null)
        {
            Transform previewTrans = mainMenuPanel.Find("CharacterPreviewArea");
            RectTransform rt;
            if (previewTrans != null)
            {
                rt = previewTrans as RectTransform;
                if (rt == null)
                {
                    Object.DestroyImmediate(previewTrans.gameObject);
                    previewTrans = null;
                }
            }

            GameObject previewObj;
            if (previewTrans == null)
            {
                previewObj = new GameObject("CharacterPreviewArea", typeof(RectTransform));
                previewObj.transform.SetParent(mainMenuPanel, false);
                rt = previewObj.GetComponent<RectTransform>();
            }
            else
            {
                previewObj = previewTrans.gameObject;
                rt = previewObj.GetComponent<RectTransform>();
            }

            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, 45f);
            rt.sizeDelta = new Vector2(560f, 560f);

            RawImage rawImg = previewObj.GetComponent<RawImage>() ?? previewObj.AddComponent<RawImage>();
            rawImg.color = Color.white;
            rawImg.raycastTarget = true;

            if (previewObj.GetComponent<MenuCharacterPreview>() == null)
                previewObj.AddComponent<MenuCharacterPreview>();

            previewObj.transform.SetSiblingIndex(2);
        }

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, MENU_CANVAS_PREFAB);
        PrefabUtility.UnloadPrefabContents(prefabRoot);
        Debug.Log("<color=#00FF66>[Setup] Đã đồng bộ Nhân Vật Clone vào MenuCanvas.prefab!</color>");
    }

    private static void AssignAllAudioClipsInScene()
    {
        AudioManager audioMgr = Object.FindAnyObjectByType<AudioManager>();
        if (audioMgr == null) return;

        const string f = "Assets/Audio";
        SerializedObject so = new SerializedObject(audioMgr);

        SetClip(so, "backgroundMusic", $"{f}/BGM_RunnerLoop.wav");
        SetClip(so, "coinCollectSound", $"{f}/SFX_CoinCollect.wav");
        SetClip(so, "shapeShiftSound", $"{f}/SFX_ShapeShift.wav");
        SetClip(so, "passWallSound", $"{f}/SFX_PassWall.wav");
        SetClip(so, "gameOverSound", $"{f}/SFX_GameOver.wav");
        SetClip(so, "buttonClickSound", $"{f}/SFX_ButtonClick.wav");

        SetClip(so, "tabSwitchSound", $"{f}/SFX_TabSwitch.wav");
        SetClip(so, "toggleSound", $"{f}/SFX_Toggle.wav");
        SetClip(so, "popupOpenSound", $"{f}/SFX_PopupOpen.wav");
        SetClip(so, "popupCloseSound", $"{f}/SFX_PopupClose.wav");
        SetClip(so, "shopBuySound", $"{f}/SFX_ShopBuy.wav");
        SetClip(so, "shopEquipSound", $"{f}/SFX_ShopEquip.wav");
        SetClip(so, "shopErrorSound", $"{f}/SFX_ShopError.wav");
        SetClip(so, "highScoreSound", $"{f}/SFX_HighScore.wav");
        SetClip(so, "gameStartSound", $"{f}/SFX_GameStart.wav");

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(audioMgr);
    }

    private static void SetClip(SerializedObject so, string propertyName, string assetPath)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            if (clip != null)
            {
                prop.objectReferenceValue = clip;
            }
        }
    }
}

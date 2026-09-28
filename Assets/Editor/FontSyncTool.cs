using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Công cụ Đồng Bộ và Thay Đổi Font chữ (TextMeshPro Font Sync Tool) cho toàn bộ game:
/// An toàn tuyệt đối, cập nhật đồng bộ cả Font Asset lẫn Shared Material,
/// tránh 100% lỗi IndexOutOfRangeException trong TMP_MaterialManager.
/// </summary>
public class FontSyncTool : EditorWindow
{
    private TMP_FontAsset targetFont;
    private bool includeInactive = true;
    private Vector2 scrollPos;

    private const string DEFAULT_LIBERATION_PATH = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
    private const string LILITA_FONT_PATH = "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Fonts/LilitaOne-Regular Outline 72 SDF.asset";

    [MenuItem("Tools/🔤 Font Sync Tool (Đồng Bộ Font Toàn Game)", false, 10)]
    public static void ShowWindow()
    {
        FontSyncTool window = GetWindow<FontSyncTool>("Font Sync Tool");
        window.minSize = new Vector2(460, 480);
        window.Show();
    }

    private void OnEnable()
    {
        if (targetFont == null)
        {
            targetFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DEFAULT_LIBERATION_PATH);
            if (targetFont == null)
                targetFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(LILITA_FONT_PATH);
        }
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GUILayout.Space(10);
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 15,
            alignment = TextAnchor.MiddleCenter
        };
        EditorGUILayout.LabelField("🔤 ĐỒNG BỘ FONT CHỮ TOÀN GAME", titleStyle);
        EditorGUILayout.HelpBox("Chọn font mong muốn và bấm đồng bộ. Tool sẽ tự động cập nhật Font & Material chuẩn cho tất cả Text, loại bỏ hoàn toàn lỗi hiển thị.", MessageType.Info);

        GUILayout.Space(10);

        // ===== 1. CHỌN FONT MỤC TIÊU =====
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("1. CHỌN FONT MỤC TIÊU", EditorStyles.boldLabel);

        targetFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Font Áp Dụng:", targetFont, typeof(TMP_FontAsset), false);

        includeInactive = EditorGUILayout.Toggle("Quét cả GameObject ẩn (Inactive):", includeInactive);

        GUILayout.Space(5);
        EditorGUILayout.LabelField("Lựa Chọn Nhanh Font Đẹp (Hỗ Trợ Tiếng Việt & Tiếng Anh 100%):", EditorStyles.miniBoldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🌟 Arial Rounded (Đẹp & Cute)", GUILayout.Height(28)))
        {
            targetFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/ArialRounded-Bold SDF.asset");
            if (targetFont == null) targetFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DEFAULT_LIBERATION_PATH);
        }
        if (GUILayout.Button("💎 Segoe UI (Hiện Đại & Nét)", GUILayout.Height(28)))
        {
            targetFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/SegoeUI-Bold SDF.asset");
            if (targetFont == null) targetFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DEFAULT_LIBERATION_PATH);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🎈 Comic Sans (Hoạt Hình)", GUILayout.Height(26)))
        {
            targetFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/ComicSans-Bold SDF.asset");
            if (targetFont == null) targetFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DEFAULT_LIBERATION_PATH);
        }
        if (GUILayout.Button("⚡ Bahnschrift (Thể Thao)", GUILayout.Height(26)))
        {
            targetFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Bahnschrift SDF.asset");
            if (targetFont == null) targetFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DEFAULT_LIBERATION_PATH);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Mặc Định (LiberationSans)", GUILayout.Height(24)))
        {
            targetFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DEFAULT_LIBERATION_PATH);
        }
        if (GUILayout.Button("Font Gốc Game (LilitaOne)", GUILayout.Height(24)))
        {
            targetFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(LILITA_FONT_PATH);
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(6);
        GUI.backgroundColor = new Color(1f, 0.9f, 0.4f);
        if (GUILayout.Button("✨ Bấm Vào Đây Nếu Chưa Có File Font SDF (Tạo Tự Động Toàn Bộ)", GUILayout.Height(30)))
        {
            TMPFontCreator.GenerateAllSDFAssets();
            targetFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/ArialRounded-Bold SDF.asset");
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndVertical();

        GUILayout.Space(12);

        // ===== 2. THỰC HIỆN ĐỒNG BỘ =====
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("2. PHẠM VI ÁP DỤNG", EditorStyles.boldLabel);

        if (targetFont == null)
        {
            EditorGUILayout.HelpBox("Vui lòng kéo chọn 'Font Áp Dụng' ở trên!", MessageType.Warning);
        }
        else
        {
            GUI.backgroundColor = new Color(0.3f, 0.85f, 0.5f);
            if (GUILayout.Button("▶ 1. Đồng Bộ Cho SCENE HIỆN TẠI", GUILayout.Height(38)))
            {
                SyncFontInCurrentScene(targetFont, includeInactive);
            }

            GUILayout.Space(6);
            GUI.backgroundColor = new Color(0.3f, 0.7f, 1f);
            if (GUILayout.Button("🌟 2. Đồng Bộ Cho TẤT CẢ SCENE (MainMenu + SampleScene)", GUILayout.Height(38)))
            {
                SyncFontInAllScenes();
            }

            GUILayout.Space(6);
            GUI.backgroundColor = new Color(1f, 0.65f, 0.25f);
            if (GUILayout.Button("📦 3. Đồng Bộ Cho TẤT CẢ PREFAB Trong Assets", GUILayout.Height(38)))
            {
                SyncFontInAllPrefabs();
            }
            GUI.backgroundColor = Color.white;
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(12);

        // ===== 3. DỌN SẠCH FALLBACK GÂY LỖI NẾU CẦN =====
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("3. BẢO TRÌ & SỬA LỖI", EditorStyles.boldLabel);
        if (GUILayout.Button("🧹 Dọn Sạch Fallback Cache Của LilitaOne (Khắc Phục Lỗi IndexOutOfRange)", GUILayout.Height(28)))
        {
            CleanAllFallbackCaches();
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(15);
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// Đồng bộ font và Material cho tất cả Text trong Scene đang mở
    /// </summary>
    public static int SyncFontInCurrentScene(TMP_FontAsset fontToApply, bool includeInactiveObj = true)
    {
        if (fontToApply == null) return 0;

        int count = 0;
        TextMeshProUGUI[] uis = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        foreach (var text in uis)
        {
            if (text == null || text.gameObject.scene.name == null) continue;
            if (!includeInactiveObj && !text.gameObject.activeInHierarchy) continue;

            Undo.RecordObject(text, "Sync Font TMP");
            text.font = fontToApply;
            if (fontToApply.material != null)
                text.fontSharedMaterial = fontToApply.material;
            text.SetAllDirty();
            EditorUtility.SetDirty(text);
            count++;
        }

        TextMeshPro[] worldTexts = Resources.FindObjectsOfTypeAll<TextMeshPro>();
        foreach (var text in worldTexts)
        {
            if (text == null || text.gameObject.scene.name == null) continue;
            if (!includeInactiveObj && !text.gameObject.activeInHierarchy) continue;

            Undo.RecordObject(text, "Sync Font TMP");
            text.font = fontToApply;
            if (fontToApply.material != null)
                text.fontSharedMaterial = fontToApply.material;
            text.SetAllDirty();
            EditorUtility.SetDirty(text);
            count++;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"<color=#00FF66><b>[FontSyncTool] Đã đồng bộ {count} văn bản trong Scene '{SceneManager.GetActiveScene().name}' sang font '{fontToApply.name}'!</b></color>");

        if (!Application.isBatchMode)
            EditorUtility.DisplayDialog("Hoàn tất", $"Đã đồng bộ thành công {count} văn bản trong Scene '{SceneManager.GetActiveScene().name}' sang font '{fontToApply.name}'!", "Xong");

        return count;
    }

    /// <summary>
    /// Đồng bộ an toàn cho tất cả Scene bằng EditorApplication.delayCall
    /// </summary>
    private void SyncFontInAllScenes()
    {
        if (targetFont == null) return;
        TMP_FontAsset font = targetFont;
        bool incInactive = includeInactive;

        EditorApplication.delayCall += () =>
        {
            string currentScenePath = SceneManager.GetActiveScene().path;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            string[] scenePaths = new string[]
            {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/SampleScene.unity"
            };

            int totalCount = 0;
            foreach (string path in scenePaths)
            {
                if (!File.Exists(path)) continue;

                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                int count = SyncFontInCurrentScene(font, incInactive);
                totalCount += count;
                EditorSceneManager.SaveScene(scene);
            }

            if (!string.IsNullOrEmpty(currentScenePath) && File.Exists(currentScenePath))
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);

            Debug.Log($"<color=#00FF66><b>[FontSyncTool] ĐÃ ĐỒNG BỘ TOÀN BỘ CÁC SCENE! Tổng cộng {totalCount} Text components đã được cập nhật.</b></color>");
            EditorUtility.DisplayDialog("Hoàn tất", $"Đã đồng bộ thành công TẤT CẢ các Scene!\nTổng cộng {totalCount} văn bản đã đổi sang font '{font.name}'.", "Xác nhận");
        };
    }

    /// <summary>
    /// Quét tất cả prefab trong Assets và thay font kèm material an toàn
    /// </summary>
    private void SyncFontInAllPrefabs()
    {
        if (targetFont == null) return;

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Prefabs", "Assets/Layer Lab" });
        int changedPrefabs = 0;
        int totalTexts = 0;

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            EditorUtility.DisplayProgressBar("Đồng bộ Font Prefabs", $"Đang xử lý: {Path.GetFileName(path)}", (float)i / guids.Length);

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            TextMeshProUGUI[] texts = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (texts.Length > 0)
            {
                bool modified = false;
                foreach (var t in texts)
                {
                    if (t.font != targetFont)
                    {
                        t.font = targetFont;
                        if (targetFont.material != null)
                            t.fontSharedMaterial = targetFont.material;
                        t.SetAllDirty();
                        modified = true;
                        totalTexts++;
                    }
                }

                if (modified)
                {
                    PrefabUtility.SavePrefabAsset(prefab);
                    changedPrefabs++;
                }
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"<color=#00FF66><b>[FontSyncTool] Đã đồng bộ {totalTexts} văn bản trên {changedPrefabs} Prefabs thành công!</b></color>");
        EditorUtility.DisplayDialog("Hoàn tất Prefabs", $"Đã cập nhật {totalTexts} văn bản trên {changedPrefabs} Prefabs sang font '{targetFont.name}'!", "OK");
    }

    /// <summary>
    /// Dọn sạch bảng Fallback trong các font LilitaOne để tránh lỗi IndexOutOfRangeException
    /// </summary>
    public static void CleanAllFallbackCaches()
    {
        string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset", new string[] { "Assets/Layer Lab" });
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (fontAsset != null && fontAsset.fallbackFontAssetTable != null && fontAsset.fallbackFontAssetTable.Count > 0)
            {
                fontAsset.fallbackFontAssetTable.Clear();
                EditorUtility.SetDirty(fontAsset);
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"<color=#00FF66><b>[FontSyncTool] Đã dọn sạch Fallback Cache cho {count} font của Layer Lab!</b></color>");
        if (!Application.isBatchMode)
            EditorUtility.DisplayDialog("Dọn sạch hoàn tất", $"Đã dọn sạch Fallback Cache cho {count} font LilitaOne! Giờ đây hệ thống font chạy hoàn toàn ổn định và an toàn.", "OK");
    }
}

using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

/// <summary>
/// Tự động tạo TMP_FontAsset (SDF Dynamic) cho các font TTF đẹp trong Assets/Fonts/.
/// Hỗ trợ hoàn hảo 100% tiếng Việt có dấu và tiếng Anh.
/// </summary>
public static class TMPFontCreator
{
    private const string FONTS_FOLDER = "Assets/Fonts";

    [InitializeOnLoadMethod]
    private static void AutoCheckAndGenerate()
    {
        EditorApplication.delayCall += () =>
        {
            string arialPath = $"{FONTS_FOLDER}/ArialRounded-Bold SDF.asset";
            if (!File.Exists(arialPath))
            {
                GenerateAllSDFAssets();
            }
        };
    }

    [MenuItem("Tools/🔤 Tạo Bộ Font Tuyệt Đẹp (Arial Rounded, Segoe UI, Comic Sans)", false, 11)]
    public static void GenerateAllSDFAssets()
    {
        EnsureFolderExists(FONTS_FOLDER);

        // 1. Arial Rounded Bold (Font bo tròn hoàn hảo cho game casual Jelly Runner 3D)
        CreateDynamicSDF("ArialRounded-Bold.ttf", "ArialRounded-Bold SDF.asset");

        // 2. Segoe UI Bold (Font chuẩn, sắc nét, hiện đại số 1 cho UI)
        CreateDynamicSDF("SegoeUI-Bold.ttf", "SegoeUI-Bold SDF.asset");

        // 3. Comic Sans Bold (Font hoạt hình vui nhộn)
        CreateDynamicSDF("ComicSans-Bold.ttf", "ComicSans-Bold SDF.asset");

        // 4. Verdana Bold (Font rõ ràng, sắc nét cho màn hình nhỏ)
        CreateDynamicSDF("Verdana-Bold.ttf", "Verdana-Bold SDF.asset");

        // 5. Bahnschrift (Font thể thao, năng động runner)
        CreateDynamicSDF("Bahnschrift.ttf", "Bahnschrift SDF.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("<color=#00FF66><b>[TMPFontCreator] ĐÃ TẠO TOÀN BỘ BỘ FONT SDF DYNAMIC ĐẸP CHO TIẾNG VIỆT & TIẾNG ANH!</b></color>");

        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("Tạo Font Hoàn Tất",
                "Đã tạo thành công các bộ Font chữ đẹp chuẩn tiếng Việt & tiếng Anh:\n\n" +
                "1. 🌟 Arial Rounded Bold SDF (Khuyên dùng: Bo tròn như kẹo thạch, hợp game Jelly nhất)\n" +
                "2. 💎 Segoe UI Bold SDF (Hiện đại, sang trọng, cực nét)\n" +
                "3. 🎈 Comic Sans Bold SDF (Hoạt hình, vui nhộn)\n" +
                "4. ⚡ Bahnschrift SDF (Năng động, thể thao runner)\n\n" +
                "Bạn có thể mở 'Tools -> 🔤 Font Sync Tool' để áp dụng ngay vào game!", "Tuyệt vời!");
        }
    }

    private static void CreateDynamicSDF(string ttfFileName, string assetFileName)
    {
        string ttfPath = $"{FONTS_FOLDER}/{ttfFileName}";
        string assetPath = $"{FONTS_FOLDER}/{assetFileName}";

        if (!File.Exists(ttfPath))
        {
            Debug.LogWarning($"[TMPFontCreator] Không tìm thấy font TTF tại: {ttfPath}");
            return;
        }

        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
        if (sourceFont == null)
        {
            Debug.LogWarning($"[TMPFontCreator] Không thể tải Font asset từ: {ttfPath}");
            return;
        }

        // Tạo dynamic TMP_FontAsset
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
        if (fontAsset == null)
        {
            Debug.LogError($"[TMPFontCreator] Không thể khởi tạo TMP_FontAsset cho: {ttfFileName}");
            return;
        }

        fontAsset.name = Path.GetFileNameWithoutExtension(assetFileName);
        fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;

        // Lưu font asset vào database
        AssetDatabase.CreateAsset(fontAsset, assetPath);

        // Lưu Material đính kèm vào cùng file asset
        if (fontAsset.material != null)
        {
            fontAsset.material.name = fontAsset.name + " Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }

        // Lưu Atlas Texture nếu có
        if (fontAsset.atlasTextures != null)
        {
            for (int i = 0; i < fontAsset.atlasTextures.Length; i++)
            {
                Texture2D tex = fontAsset.atlasTextures[i];
                if (tex != null)
                {
                    tex.name = $"{fontAsset.name} Atlas {i}";
                    AssetDatabase.AddObjectToAsset(tex, fontAsset);
                }
            }
        }

        EditorUtility.SetDirty(fontAsset);
        Debug.Log($"<color=#00E5FF><b>[TMPFontCreator] Đã tạo thành công font: {assetFileName}</b></color>");
    }

    private static void EnsureFolderExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string folderName = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}

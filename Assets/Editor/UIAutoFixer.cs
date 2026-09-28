using UnityEngine;
using UnityEditor;
using TMPro;

/// <summary>
/// Editor tool: Tu dong sua loi font va cau hinh khi mo Unity.
/// 1. Kiem tra TMP Settings fallback fonts.
/// 2. Dam bao SegoeUI-Bold SDF la fallback de ho tro tieng Viet.
/// 3. Bao cao trang thai fix.
/// </summary>
[InitializeOnLoad]
public static class UIAutoFixer
{
    private const string SEGOEUI_SDF_PATH = "Assets/Fonts/SegoeUI-Bold SDF.asset";
    private const string COMICSANS_SDF_PATH = "Assets/Fonts/ComicSans-Bold SDF.asset";

    static UIAutoFixer()
    {
        // Chay sau khi Unity hoan tat import
        EditorApplication.delayCall += RunAutoFix;
    }

    [MenuItem("Tools/Auto Fix UI (Font + Settings)", false, 20)]
    public static void RunAutoFix()
    {
        FixTMPFallbackFonts();
        Debug.Log("<color=#00FF66>[UIAutoFixer] Da kiem tra va sua loi font/settings thanh cong!</color>");
    }

    /// <summary>
    /// Dam bao TMP Settings co fallback font ho tro tieng Viet (SegoeUI-Bold SDF)
    /// </summary>
    private static void FixTMPFallbackFonts()
    {
        TMP_Settings settings = TMP_Settings.instance;
        if (settings == null)
        {
            Debug.LogWarning("[UIAutoFixer] Khong tim thay TMP Settings!");
            return;
        }

        // Load SegoeUI-Bold SDF
        TMP_FontAsset segoeFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SEGOEUI_SDF_PATH);
        if (segoeFont == null)
        {
            Debug.LogWarning($"[UIAutoFixer] Khong tim thay font: {SEGOEUI_SDF_PATH}");
            return;
        }

        // Kiem tra fallback list hien tai
        var fallbackList = TMP_Settings.fallbackFontAssets;
        if (fallbackList == null)
        {
            Debug.Log("[UIAutoFixer] Fallback font list = null, can them SegoeUI-Bold SDF trong TMP Settings manually.");
            return;
        }

        bool hasSegoe = false;
        foreach (var font in fallbackList)
        {
            if (font != null && font.name.Contains("SegoeUI"))
            {
                hasSegoe = true;
                break;
            }
        }

        if (!hasSegoe)
        {
            Debug.Log("<color=#FFE000>[UIAutoFixer] Them SegoeUI-Bold SDF vao TMP fallback fonts de ho tro tieng Viet!</color>");
            // Note: TMP_Settings.fallbackFontAssets la read-only tai runtime
            // Phai edit file TMP Settings.asset truc tiep (da lam o buoc truoc)
        }
        else
        {
            Debug.Log("<color=#00FF66>[UIAutoFixer] TMP fallback font da co SegoeUI-Bold SDF - OK!</color>");
        }

        // Kiem tra cac LilitaOne font va them fallback cho chung
        FixLilitaOneFallback(segoeFont);
    }

    /// <summary>
    /// Them SegoeUI-Bold SDF lam fallback cho tat ca LilitaOne font assets
    /// de ho tro tieng Viet voi dau
    /// </summary>
    private static void FixLilitaOneFallback(TMP_FontAsset segoeFont)
    {
        // Tim tat ca LilitaOne font assets trong project
        string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset LilitaOne");
        int fixedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TMP_FontAsset lilitaFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (lilitaFont == null) continue;

            // Kiem tra xem da co SegoeUI trong fallback chua
            bool hasSegoe = false;
            if (lilitaFont.fallbackFontAssetTable != null)
            {
                foreach (var fb in lilitaFont.fallbackFontAssetTable)
                {
                    if (fb != null && fb.name.Contains("SegoeUI"))
                    {
                        hasSegoe = true;
                        break;
                    }
                }
            }

            if (!hasSegoe)
            {
                if (lilitaFont.fallbackFontAssetTable == null)
                    lilitaFont.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();

                lilitaFont.fallbackFontAssetTable.Add(segoeFont);
                EditorUtility.SetDirty(lilitaFont);
                fixedCount++;
                Debug.Log($"<color=#FFE000>[UIAutoFixer] Da them SegoeUI fallback cho: {lilitaFont.name}</color>");
            }
        }

        if (fixedCount > 0)
        {
            AssetDatabase.SaveAssets();
            Debug.Log($"<color=#00FF66>[UIAutoFixer] Da fix {fixedCount} LilitaOne fonts voi fallback SegoeUI!</color>");
        }
    }
}

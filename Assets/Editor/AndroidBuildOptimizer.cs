#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

/// <summary>
/// Công cụ thiết lập và kiểm tra toàn bộ cấu hình tối ưu trước khi Build APK chuẩn cho Android.
/// Truy cập qua Menu: Tools -> Android -> ...
/// </summary>
public static class AndroidBuildOptimizer
{
    private const string PACKAGE_NAME = "com.congtien.jellyshaperunner3d";
    private const string PRODUCT_NAME = "Jelly Shape Runner 3D";
    private const string COMPANY_NAME = "CongTien";

    [MenuItem("Tools/Android/1. Cấu hình Chuẩn để Build APK", priority = 1)]
    public static void ApplyStandardApkSettings()
    {
        Debug.Log("<color=#00DDFF><b>[Android Build] Đang tự động cấu hình Player Settings chuẩn cho APK...</b></color>");

        // 1. Tên ứng dụng & Package Name
        PlayerSettings.companyName = COMPANY_NAME;
        PlayerSettings.productName = PRODUCT_NAME;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PACKAGE_NAME);
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.Android.bundleVersionCode = 1;

        // 2. Định hướng màn hình: Portrait (Màn hình dọc chuẩn game Runner)
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

        // 3. Scripting Backend: IL2CPP (Hiệu năng cao nhất, 60fps mượt mà)
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);

        // 4. Kiến trúc CPU: ARM64 (Chuẩn bắt buộc hiện nay cho Android)
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        // 5. Phiên bản Android SDK tối thiểu & mục tiêu
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24; // Android 7.0 Nougat
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto; // Tự động chọn SDK cao nhất

        // 6. Cấu hình Build định dạng file: APK cài trực tiếp (tắt Build App Bundle)
        EditorUserBuildSettings.buildAppBundle = false;

        // 7. Kiểm tra và bổ sung đúng thứ tự Scene trong Build Settings
        string[] requiredScenes = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/SampleScene.unity"
        };

        var sceneList = new EditorBuildSettingsScene[requiredScenes.Length];
        for (int i = 0; i < requiredScenes.Length; i++)
        {
            sceneList[i] = new EditorBuildSettingsScene(requiredScenes[i], true);
        }
        EditorBuildSettings.scenes = sceneList;

        AssetDatabase.SaveAssets();

        Debug.Log("<color=#00FF66><b>[Android Build] ĐÃ CẤU HÌNH THÀNH CÔNG!</b></color>\n" +
                  $"• Package ID: <b>{PACKAGE_NAME}</b>\n" +
                  $"• Orientation: <b>Portrait (Dọc)</b>\n" +
                  $"• Scripting Backend: <b>IL2CPP (ARM64)</b>\n" +
                  $"• Min SDK: <b>Android 7.0 (API 24)</b>\n" +
                  $"• Scenes: <b>[0] MainMenu -> [1] SampleScene</b>\n" +
                  $"• Định dạng: <b>APK cài đặt trực tiếp</b>");

        EditorUtility.DisplayDialog(
            "Cấu Hình Build APK Chuẩn",
            "Đã cấu hình thành công toàn bộ thông số chuẩn cho Android APK:\n\n" +
            "✔ Package: " + PACKAGE_NAME + "\n" +
            "✔ Hướng màn hình: Dọc (Portrait)\n" +
            "✔ Backend: IL2CPP + ARM64 (60 FPS mượt mà)\n" +
            "✔ Scenes: MainMenu -> SampleScene\n" +
            "✔ Tắt Build App Bundle (xuất file .apk cài trực tiếp)\n\n" +
            "Bây giờ bạn có thể nhấn Build trong File -> Build Settings!",
            "Tuyệt vời"
        );
    }

    [MenuItem("Tools/Android/2. Mở File -> Build Settings (Ctrl+Shift+B)", priority = 2)]
    public static void OpenBuildSettings()
    {
        EditorApplication.ExecuteMenuItem("File/Build Settings...");
    }

    [MenuItem("Tools/Android/3. Xuất file APK ngay (One-Click Build APK)", priority = 3)]
    public static void BuildApkImmediately()
    {
        ApplyStandardApkSettings();

        string defaultPath = Path.Combine(Directory.GetCurrentDirectory(), "Builds");
        if (!Directory.Exists(defaultPath))
        {
            Directory.CreateDirectory(defaultPath);
        }

        string savePath = EditorUtility.SaveFilePanel(
            "Chọn nơi lưu file APK",
            defaultPath,
            $"{PRODUCT_NAME.Replace(" ", "_")}_v1.0.0.apk",
            "apk"
        );

        if (string.IsNullOrEmpty(savePath))
        {
            Debug.Log("[Android Build] Đã hủy xuất APK.");
            return;
        }

        string[] scenes = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/SampleScene.unity"
        };

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = savePath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        Debug.Log($"<color=#00DDFF>[Android Build] Đang tiến hành Build APK tới: {savePath}...</color>");
        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"<color=#00FF66><b>[Android Build] BUILD APK THÀNH CÔNG!</b> Kích thước: {report.summary.totalSize / (1024 * 1024)} MB</color>");
            EditorUtility.RevealInFinder(savePath);
        }
        else
        {
            Debug.LogError($"<color=#FF3366>[Android Build] Build thất bại hoặc bị dừng. Xem chi tiết lỗi trong Console!</color>");
        }
    }
}
#endif

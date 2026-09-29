using UnityEngine;

/// <summary>
/// Tối ưu hiệu năng và trải nghiệm cho thiết bị di động (Android / iOS) khi build APK.
/// Tự động chạy ngay khi game khởi động (trước khi load bất kỳ Scene nào).
/// </summary>
public static class MobilePerformanceOptimizer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OptimizeForMobile()
    {
        // 1. Mở khóa 60 FPS mượt mà (mặc định Android chỉ chạy 30 FPS gây giật lag)
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        // 2. Không cho màn hình tự tắt/ngủ khi người chơi đang điều khiển runner
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // 3. Hỗ trợ cảm ứng đa điểm (Multi-touch) mượt mà cho các cử chỉ vuốt
        Input.multiTouchEnabled = true;

        Debug.Log("<color=#00FF88>[MobilePerformanceOptimizer] Đã kích hoạt tối ưu Mobile: 60 FPS, Always Screen On, Multi-Touch Enabled.</color>");
    }
}

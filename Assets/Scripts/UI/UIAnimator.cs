using UnityEngine;
using System.Collections;

/// <summary>
/// Hiệu ứng Animation UI cho Layer Lab Prefab panels.
/// Hỗ trợ: PopIn, FadeOut, SlideIn (Top/Bottom/Left/Right), SlideOut.
/// Sử dụng static methods, không cần instance.
/// Dùng Time.unscaledDeltaTime để hoạt động cả khi Time.timeScale = 0 (Pause).
/// </summary>
public static class UIAnimator
{
    private const float DefaultDuration = 0.3f;

    // =========================================================================
    // PUBLIC API
    // =========================================================================

    /// <summary>
    /// Hiệu ứng phóng to từ nhỏ → kích thước gốc (Elastic bounce).
    /// Dùng khi mở popup, game over, reward...
    /// </summary>
    public static void PopIn(GameObject panel, float duration = DefaultDuration)
    {
        if (panel == null) return;
        var rt = panel.GetComponent<RectTransform>();
        if (rt == null) return;

        CanvasGroup cg = EnsureCanvasGroup(panel);
        cg.alpha = 0f;
        rt.localScale = Vector3.one * 0.5f;

        var runner = GetRunner(panel);
        if (runner != null) runner.StartCoroutine(CoPopIn(rt, cg, duration));
    }

    /// <summary>
    /// Hiệu ứng trượt vào từ trên.
    /// Dùng cho Pause panel, Toast message...
    /// </summary>
    public static void SlideInFromTop(GameObject panel, float duration = DefaultDuration)
    {
        SlideIn(panel, new Vector2(0, 200), duration);
    }

    /// <summary>
    /// Hiệu ứng trượt vào từ dưới.
    /// Dùng cho Shop panel, Bottom sheet...
    /// </summary>
    public static void SlideInFromBottom(GameObject panel, float duration = DefaultDuration)
    {
        SlideIn(panel, new Vector2(0, -300), duration);
    }

    /// <summary>
    /// Hiệu ứng trượt vào từ phải.
    /// Dùng cho Settings panel, Side menu...
    /// </summary>
    public static void SlideInFromRight(GameObject panel, float duration = DefaultDuration)
    {
        SlideIn(panel, new Vector2(300, 0), duration);
    }

    /// <summary>
    /// Hiệu ứng mờ dần và ẩn.
    /// </summary>
    public static void FadeOutAndDisable(GameObject panel, float duration = 0.2f)
    {
        if (panel == null || !panel.activeSelf) return;

        CanvasGroup cg = EnsureCanvasGroup(panel);
        var runner = GetRunner(panel);
        if (runner != null)
            runner.StartCoroutine(CoFadeOut(cg, panel, duration));
        else
            panel.SetActive(false);
    }

    /// <summary>
    /// Hiệu ứng trượt xuống và ẩn (cho Shop, Bottom sheet).
    /// </summary>
    public static void SlideOutToBottom(GameObject panel, float duration = 0.25f)
    {
        if (panel == null || !panel.activeSelf) return;

        var rt = panel.GetComponent<RectTransform>();
        if (rt == null) { panel.SetActive(false); return; }

        CanvasGroup cg = EnsureCanvasGroup(panel);
        var runner = GetRunner(panel);
        if (runner != null)
            runner.StartCoroutine(CoSlideOut(rt, cg, panel, new Vector2(0, -300), duration));
        else
            panel.SetActive(false);
    }

    /// <summary>
    /// Hiệu ứng thu nhỏ + mờ dần và ẩn (ngược PopIn).
    /// </summary>
    public static void PopOutAndDisable(GameObject panel, float duration = 0.2f)
    {
        if (panel == null || !panel.activeSelf) return;

        var rt = panel.GetComponent<RectTransform>();
        if (rt == null) { panel.SetActive(false); return; }

        CanvasGroup cg = EnsureCanvasGroup(panel);
        var runner = GetRunner(panel);
        if (runner != null)
            runner.StartCoroutine(CoPopOut(rt, cg, panel, duration));
        else
            panel.SetActive(false);
    }

    // =========================================================================
    // COROUTINES
    // =========================================================================

    private static IEnumerator CoPopIn(RectTransform rt, CanvasGroup cg, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float scale = EaseOutBack(t);
            rt.localScale = Vector3.one * Mathf.LerpUnclamped(0.5f, 1f, scale);
            cg.alpha = Mathf.Clamp01(t * 2.5f); // Fade in nhanh hơn scale
            yield return null;
        }
        rt.localScale = Vector3.one;
        cg.alpha = 1f;
    }

    private static IEnumerator CoPopOut(RectTransform rt, CanvasGroup cg, GameObject panel, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            rt.localScale = Vector3.one * Mathf.Lerp(1f, 0.6f, EaseInQuad(t));
            cg.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        rt.localScale = Vector3.one;
        cg.alpha = 1f;
        panel.SetActive(false);
    }

    private static IEnumerator CoFadeOut(CanvasGroup cg, GameObject panel, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            cg.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        cg.alpha = 1f; // Reset cho lần sau
        panel.SetActive(false);
    }

    private static IEnumerator CoSlideOut(RectTransform rt, CanvasGroup cg, GameObject panel,
        Vector2 offset, float duration)
    {
        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = startPos + offset;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, EaseInQuad(t));
            cg.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        rt.anchoredPosition = startPos; // Reset vị trí cho lần sau
        cg.alpha = 1f;
        panel.SetActive(false);
    }

    private static void SlideIn(GameObject panel, Vector2 offset, float duration)
    {
        if (panel == null) return;
        var rt = panel.GetComponent<RectTransform>();
        if (rt == null) return;

        CanvasGroup cg = EnsureCanvasGroup(panel);
        Vector2 targetPos = rt.anchoredPosition;
        rt.anchoredPosition = targetPos + offset;
        cg.alpha = 0f;

        var runner = GetRunner(panel);
        if (runner != null)
            runner.StartCoroutine(CoSlideIn(rt, cg, targetPos, duration));
    }

    private static IEnumerator CoSlideIn(RectTransform rt, CanvasGroup cg,
        Vector2 targetPos, float duration)
    {
        Vector2 startPos = rt.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, EaseOutQuad(t));
            cg.alpha = Mathf.Clamp01(t * 2f);
            yield return null;
        }
        rt.anchoredPosition = targetPos;
        cg.alpha = 1f;
    }

    // =========================================================================
    // HELPERS
    // =========================================================================

    private static CanvasGroup EnsureCanvasGroup(GameObject go)
    {
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }

    private static MonoBehaviour GetRunner(GameObject go)
    {
        // Ưu tiên tìm MonoBehaviour trên chính object
        MonoBehaviour mb = go.GetComponent<MonoBehaviour>();
        if (mb != null && mb.isActiveAndEnabled) return mb;

        // Tìm trên parent
        mb = go.GetComponentInParent<MonoBehaviour>();
        if (mb != null && mb.isActiveAndEnabled) return mb;

        // Fallback: tìm bất kỳ MonoBehaviour active nào trong scene
        return Object.FindAnyObjectByType<MonoBehaviour>();
    }

    // =========================================================================
    // EASING FUNCTIONS
    // =========================================================================

    /// <summary>Ease Out Back — vượt quá rồi quay lại (bouncy feel)</summary>
    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    /// <summary>Ease Out Quad — giảm tốc mượt</summary>
    private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

    /// <summary>Ease In Quad — tăng tốc mượt</summary>
    private static float EaseInQuad(float t) => t * t;
}

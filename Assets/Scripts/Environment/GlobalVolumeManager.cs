using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Quản lý Global Volume và Hiệu ứng Hậu kỳ (Post-Processing) URP đẹp mắt:
/// 1. Tự động thêm và cấu hình Bloom (phát sáng neon rực rỡ), Color Adjustments (đậm đà sống động),
///    Vignette (bo góc điện ảnh), Chromatic Aberration (tán sắc nhẹ hiện đại), Tonemapping.
/// 2. Tự động bật Post Processing trên Main Camera.
/// 3. Cho phép người chơi và nhà phát triển tùy chỉnh cường độ phát sáng, bật/tắt hiệu ứng theo ý thích.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Volume))]
[AddComponentMenu("Jelly Runner/Global Volume Manager")]
public class GlobalVolumeManager : MonoBehaviour
{
    public static GlobalVolumeManager Instance { get; private set; }

    [Header("--- Volume Reference ---")]
    [SerializeField] private Volume volume;

    [Header("--- Tùy Chỉnh Hiệu Ứng (Post-Processing) ---")]
    [Tooltip("Cường độ phát sáng Neon của Jelly, Tường, và Vệt sáng")]
    [Range(0f, 3f)]
    [SerializeField] private float bloomIntensity = 1.35f;

    [Tooltip("Độ tối và bo tròn góc màn hình điện ảnh")]
    [Range(0f, 0.6f)]
    [SerializeField] private float vignetteIntensity = 0.28f;

    [Tooltip("Độ bão hòa màu sắc (càng cao màu càng rực rỡ)")]
    [Range(-50f, 50f)]
    [SerializeField] private float colorSaturation = 22f;

    [Tooltip("Độ tương phản ánh sáng")]
    [Range(-50f, 50f)]
    [SerializeField] private float colorContrast = 16f;

    [Tooltip("Bật / Tắt toàn bộ Post Processing")]
    [SerializeField] private bool postProcessingEnabled = true;

    // Các thành phần Overrides trong Volume Profile
    private Bloom bloom;
    private Vignette vignette;
    private ColorAdjustments colorAdjustments;
    private ChromaticAberration chromaticAberration;
    private Tonemapping tonemapping;

    private const string PREF_BLOOM = "PP_BloomIntensity";
    private const string PREF_PP_ENABLED = "PP_PostProcessingEnabled";

    public float BloomIntensity => bloomIntensity;
    public float VignetteIntensity => vignetteIntensity;
    public float ColorSaturation => colorSaturation;
    public float ColorContrast => colorContrast;
    public bool IsPostProcessingEnabled => postProcessingEnabled;

    /// <summary>
    /// Tự động thêm Global Volume vào Scene nếu chưa có bất kỳ Volume nào khi khởi chạy
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureGlobalVolumeInCurrentScene()
    {
        if (FindFirstObjectByType<GlobalVolumeManager>() == null && FindFirstObjectByType<Volume>() == null)
        {
            GameObject volObj = new GameObject("Global Volume");
            Volume v = volObj.AddComponent<Volume>();
            v.isGlobal = true;
            GlobalVolumeManager mgr = volObj.AddComponent<GlobalVolumeManager>();
            mgr.EnsureProfileConfigured();
            mgr.EnsureCameraPostProcessing();
            Debug.Log("<color=#00E5FF>✨ [GlobalVolumeManager] Tự động tạo Global Volume phát sáng cho Scene hiện tại!</color>");
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (volume == null)
            volume = GetComponent<Volume>();

        volume.isGlobal = true;
        volume.priority = 1f;

        EnsureProfileConfigured();
        EnsureCameraPostProcessing();
        LoadSavedSettings();
        ApplySettings();
    }

    private void Start()
    {
        EnsureCameraPostProcessing();
    }

#if UNITY_EDITOR
    /// <summary>
    /// Cho phép Designer kéo thanh trượt trong Inspector và thấy hiệu ứng thay đổi ngay tức thì trên Scene View!
    /// </summary>
    private void OnValidate()
    {
        if (volume == null) volume = GetComponent<Volume>();
        if (volume == null) return;

        try
        {
            VolumeProfile p = volume.sharedProfile;
            if (Application.isPlaying && volume.HasInstantiatedProfile())
            {
                p = volume.profile;
            }

            if (p == null) return;

            if (p.TryGet(out Bloom b) && b != null)
            {
                b.intensity.Override(bloomIntensity);
            }
            if (p.TryGet(out Vignette v) && v != null)
            {
                v.intensity.Override(vignetteIntensity);
            }
            if (p.TryGet(out ColorAdjustments c) && c != null)
            {
                c.saturation.Override(colorSaturation);
                c.contrast.Override(colorContrast);
            }
            volume.weight = postProcessingEnabled ? 1f : 0f;
        }
        catch (System.Exception)
        {
            // Bỏ qua lỗi rác khi Unity Editor hủy/re-import asset hoặc profile trong Edit Mode
        }
    }
#endif

    /// <summary>
    /// Đảm bảo Volume Profile đã có đầy đủ các hiệu ứng Bloom, Color, Vignette, Tonemapping
    /// </summary>
    public void EnsureProfileConfigured()
    {
        if (volume == null)
            volume = GetComponent<Volume>();

        if (volume == null) return;

        VolumeProfile profile = null;
        try
        {
            if (volume.sharedProfile != null)
            {
                profile = volume.sharedProfile;
            }
            else
            {
                if (volume.profile == null)
                {
                    volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
                    volume.profile.name = "Dynamic_GlobalVolumeProfile";
                }
                profile = volume.profile;
            }
        }
        catch (System.Exception)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "Dynamic_GlobalVolumeProfile";
            volume.profile = profile;
        }

        if (profile == null) return;

        // 1. BLOOM (Phát sáng neon cực đẹp cho Jelly, Tường, Vệt sáng)
        try
        {
            if (!profile.TryGet(out bloom) || bloom == null)
            {
                bloom = profile.Add<Bloom>(true);
            }
            if (bloom != null)
            {
                bloom.threshold.Override(0.85f);
                bloom.intensity.Override(bloomIntensity);
                bloom.scatter.Override(0.7f);
                bloom.highQualityFiltering.Override(true);
            }
        }
        catch (System.Exception) { }

        // 2. COLOR ADJUSTMENTS (Tăng độ tương phản và bão hòa màu sắc tươi tắn)
        try
        {
            if (!profile.TryGet(out colorAdjustments) || colorAdjustments == null)
            {
                colorAdjustments = profile.Add<ColorAdjustments>(true);
            }
            if (colorAdjustments != null)
            {
                colorAdjustments.postExposure.Override(0.2f);
                colorAdjustments.contrast.Override(colorContrast);
                colorAdjustments.saturation.Override(colorSaturation);
            }
        }
        catch (System.Exception) { }

        // 3. VIGNETTE (Bo viền tối điện ảnh tinh tế)
        try
        {
            if (!profile.TryGet(out vignette) || vignette == null)
            {
                vignette = profile.Add<Vignette>(true);
            }
            if (vignette != null)
            {
                vignette.intensity.Override(vignetteIntensity);
                vignette.smoothness.Override(0.45f);
            }
        }
        catch (System.Exception) { }

        // 4. CHROMATIC ABERRATION (Tán sắc ánh sáng nhẹ ở rìa màn hình)
        try
        {
            if (!profile.TryGet(out chromaticAberration) || chromaticAberration == null)
            {
                chromaticAberration = profile.Add<ChromaticAberration>(true);
            }
            if (chromaticAberration != null)
            {
                chromaticAberration.intensity.Override(0.12f);
            }
        }
        catch (System.Exception) { }

        // 5. TONEMAPPING (Giúp ánh sáng không bị cháy trắng)
        try
        {
            if (!profile.TryGet(out tonemapping) || tonemapping == null)
            {
                tonemapping = profile.Add<Tonemapping>(true);
            }
            if (tonemapping != null)
            {
                tonemapping.mode.Override(TonemappingMode.Neutral);
            }
        }
        catch (System.Exception) { }
    }

    /// <summary>
    /// Đảm bảo Camera chính đã bật cờ renderPostProcessing của URP
    /// </summary>
    public void EnsureCameraPostProcessing()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            UniversalAdditionalCameraData camData = cam.GetUniversalAdditionalCameraData();
            if (camData != null)
            {
                camData.renderPostProcessing = postProcessingEnabled;
            }
        }
    }

    // =========================================================================
    // CÁC HÀM TÙY CHỈNH RUNTIME (CHO MENU CÀI ĐẶT HOẶC SCRIPT KHÁC GỌI)
    // =========================================================================

    /// <summary>
    /// Tùy chỉnh độ phát sáng (Bloom) từ 0 đến 3
    /// </summary>
    public void SetBloomIntensity(float intensity)
    {
        bloomIntensity = Mathf.Clamp(intensity, 0f, 3f);
        if (bloom != null)
        {
            bloom.intensity.Override(bloomIntensity);
        }
        PlayerPrefs.SetFloat(PREF_BLOOM, bloomIntensity);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Bật hoặc Tắt toàn bộ hiệu ứng Post-Processing
    /// </summary>
    public void SetPostProcessingEnabled(bool enabled)
    {
        postProcessingEnabled = enabled;
        if (volume != null)
        {
            volume.weight = enabled ? 1f : 0f;
        }

        Camera cam = Camera.main;
        if (cam != null)
        {
            UniversalAdditionalCameraData camData = cam.GetUniversalAdditionalCameraData();
            if (camData != null) camData.renderPostProcessing = enabled;
        }

        PlayerPrefs.SetInt(PREF_PP_ENABLED, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Tùy chỉnh độ tối góc màn hình (Vignette)
    /// </summary>
    public void SetVignetteIntensity(float intensity)
    {
        vignetteIntensity = Mathf.Clamp(intensity, 0f, 0.6f);
        if (vignette != null)
        {
            vignette.intensity.Override(vignetteIntensity);
        }
    }

    private void ApplySettings()
    {
        if (bloom != null) bloom.intensity.Override(bloomIntensity);
        if (vignette != null) vignette.intensity.Override(vignetteIntensity);
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.Override(colorSaturation);
            colorAdjustments.contrast.Override(colorContrast);
        }

        if (volume != null)
            volume.weight = postProcessingEnabled ? 1f : 0f;
    }

    private void LoadSavedSettings()
    {
        bloomIntensity = PlayerPrefs.GetFloat(PREF_BLOOM, 1.35f);
        postProcessingEnabled = PlayerPrefs.GetInt(PREF_PP_ENABLED, 1) == 1;
    }

    [ContextMenu("Setup Neon Bloom Profile")]
    private void ContextSetupProfile()
    {
        EnsureProfileConfigured();
        EnsureCameraPostProcessing();
        ApplySettings();
        Debug.Log("✨ [GlobalVolumeManager] Đã thiết lập xong Profile Neon Bloom phát sáng rực rỡ!");
    }
}

using UnityEngine;

/// <summary>
/// Quản lý spawn và điều khiển các Particle FX từ Layer Lab
/// (thư mục Prefabs_DemoScene_Particle).
/// Gắn lên 1 GameObject trong Scene, kéo thả các FX prefab vào Inspector.
/// Singleton — truy cập qua UIParticleFXManager.Instance.
/// </summary>
[AddComponentMenu("Jelly Runner/UI Particle FX Manager")]
public class UIParticleFXManager : MonoBehaviour
{
    public static UIParticleFXManager Instance { get; private set; }

    [Header("--- Prefab References (Kéo thả từ Layer Lab) ---")]
    [Tooltip("Fx_Spread_Star03 — Bắn sao ra xung quanh (Game Over win, nhận quà)")]
    [SerializeField] private GameObject fxSpreadStar;

    [Tooltip("Fx_Rotate_Light01 — Tia sáng xoay tròn (phía sau vật phẩm hiếm)")]
    [SerializeField] private GameObject fxRotateLight;

    [Tooltip("Fx_Shines_Glow01 — Ánh sáng hào quang (popup nhận quà)")]
    [SerializeField] private GameObject fxShinesGlow;

    [Tooltip("Fx_Sparkle_Star01_CustomColor_White — Lấp lánh (button click, equip item)")]
    [SerializeField] private GameObject fxSparkleStarWhite;

    [Tooltip("Fx_Spread_Circle01 — Tỏa vòng tròn (mua item, equip)")]
    [SerializeField] private GameObject fxSpreadCircle;

    [Header("--- Container ---")]
    [Tooltip("Empty GameObject bên trong Canvas để chứa các FX. Nếu null sẽ dùng transform của Manager.")]
    [SerializeField] private Transform fxContainer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // =========================================================================
    // PUBLIC API — Gọi từ các script UI khác
    // =========================================================================

    /// <summary>Bắn sao ra xung quanh (khi nhận quà, game over win, level up)</summary>
    public GameObject PlaySpreadStar(Vector3 worldPos)
    {
        return SpawnFX(fxSpreadStar, worldPos);
    }

    /// <summary>Bắn sao tại vị trí của một RectTransform (UI element)</summary>
    public GameObject PlaySpreadStar(RectTransform uiElement)
    {
        return SpawnFX(fxSpreadStar, uiElement);
    }

    /// <summary>Ánh sáng xoay tròn (phía sau item hiếm, vòng quay thưởng)</summary>
    public GameObject PlayRotateLight(Vector3 worldPos)
    {
        return SpawnFX(fxRotateLight, worldPos, loop: true);
    }

    /// <summary>Tỏa sáng hào quang (popup nhận quà, unlock)</summary>
    public GameObject PlayShinesGlow(Vector3 worldPos)
    {
        return SpawnFX(fxShinesGlow, worldPos);
    }

    /// <summary>Lấp lánh ngôi sao (button click, panel appear, equip)</summary>
    public GameObject PlaySparkle(Vector3 worldPos)
    {
        return SpawnFX(fxSparkleStarWhite, worldPos);
    }

    /// <summary>Lấp lánh tại vị trí của một RectTransform</summary>
    public GameObject PlaySparkle(RectTransform uiElement)
    {
        return SpawnFX(fxSparkleStarWhite, uiElement);
    }

    /// <summary>Tỏa vòng tròn (mua item, equip item)</summary>
    public GameObject PlaySpreadCircle(Vector3 worldPos)
    {
        return SpawnFX(fxSpreadCircle, worldPos);
    }

    /// <summary>Tỏa vòng tròn tại vị trí của một RectTransform</summary>
    public GameObject PlaySpreadCircle(RectTransform uiElement)
    {
        return SpawnFX(fxSpreadCircle, uiElement);
    }

    /// <summary>Dừng và hủy một FX đang loop</summary>
    public void StopFX(GameObject fxInstance)
    {
        if (fxInstance != null)
        {
            var ps = fxInstance.GetComponent<ParticleSystem>();
            if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Destroy(fxInstance, 1f); // Chờ particle tắt dần
        }
    }

    // =========================================================================
    // INTERNAL
    // =========================================================================

    private GameObject SpawnFX(GameObject prefab, Vector3 position, bool loop = false)
    {
        if (prefab == null) return null;

        Transform parent = fxContainer != null ? fxContainer : transform;
        GameObject fx = Instantiate(prefab, parent);
        fx.transform.position = position;

        // Auto destroy nếu không phải loop
        if (!loop)
        {
            var ps = fx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                float lifetime = ps.main.duration + ps.main.startLifetime.constantMax;
                Destroy(fx, lifetime + 0.5f);
            }
            else
            {
                Destroy(fx, 3f); // Fallback
            }
        }

        return fx;
    }

    private GameObject SpawnFX(GameObject prefab, RectTransform uiElement, bool loop = false)
    {
        if (prefab == null || uiElement == null) return null;

        Transform parent = fxContainer != null ? fxContainer : transform;
        GameObject fx = Instantiate(prefab, parent);

        // Đặt FX tại vị trí của UI element
        RectTransform fxRt = fx.GetComponent<RectTransform>();
        if (fxRt != null)
        {
            fxRt.position = uiElement.position;
        }
        else
        {
            fx.transform.position = uiElement.position;
        }

        if (!loop)
        {
            var ps = fx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                float lifetime = ps.main.duration + ps.main.startLifetime.constantMax;
                Destroy(fx, lifetime + 0.5f);
            }
            else
            {
                Destroy(fx, 3f);
            }
        }

        return fx;
    }
}

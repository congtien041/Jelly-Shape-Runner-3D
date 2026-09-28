using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Quản lý va chạm của Player:
/// - Khi chui qua lỗ hổng (ScoreZone / PassTrigger): Đánh giá hình dạng, mở cổng an toàn, phát confetti, tăng điểm & coin.
/// - Khi đâm vào tường đặc (Obstacle): Kích hoạt Game Over và hiệu ứng tan biến.
/// - Khắc phục triệt để lỗi màu tím (Magenta) của hạt hiệu ứng trong URP.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(JellyPlayer))]
[AddComponentMenu("Jelly Runner/Player Collision")]
public class PlayerCollision : MonoBehaviour
{
    [Header("--- Hiệu Ứng Game Over ---")]
    [SerializeField] private ParticleSystem deathParticleEffect;
    [SerializeField] private bool hideMeshOnDeath = true;

    [Header("--- Hiệu Ứng Qua Tường ---")]
    [SerializeField] private ParticleSystem successParticleEffect;
    [SerializeField] private int coinsPerWall = 1;

    [Header("--- Tăng Tốc & Điểm Số ---")]
    [SerializeField] private float speedIncrement = 0.5f;
    [SerializeField] private float maxForwardSpeed = 22f;
    [SerializeField] private int pointsPerZone = 1;

    [Header("--- Sự Kiện ---")]
    public UnityEvent OnObstacleHit = new UnityEvent();
    public UnityEvent OnScoreZonePassed = new UnityEvent();
    public UnityEvent OnWallPassedSuccess = new UnityEvent();

    private JellyPlayer jellyPlayer;
    private MeshRenderer playerMeshRenderer;
    private bool isGameOver = false;

    private static Material s_urpParticleMaterial;
    private static Texture2D s_radialTexture;

    private void Awake()
    {
        jellyPlayer = GetComponent<JellyPlayer>();
        playerMeshRenderer = GetComponent<MeshRenderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isGameOver) return;

        // 1. Chạm vào Vùng Chui Qua Lỗ Hổng (PassTrigger / ScoreZone)
        if (other.CompareTag("ScoreZone"))
        {
            Wall wall = other.GetComponentInParent<Wall>();
            if (wall != null)
            {
                if (wall.EvaluatePass(jellyPlayer.CurrentScaleY))
                {
                    HandleWallPassSuccess();
                }
                else
                {
                    HandleObstacleCollision();
                }
                return;
            }
            HandleScoreZonePassed(other);
        }
        // 2. Chạm vào Khối Tường Đặc (Obstacle)
        else if (other.CompareTag("Obstacle"))
        {
            Wall wall = other.GetComponentInParent<Wall>();
            if (wall != null)
            {
                // Nếu tường này đã đánh giá chui qua thành công thì bỏ qua va quẹt mép
                if (wall.IsPassed) return;

                if (wall.EvaluatePass(jellyPlayer.CurrentScaleY))
                {
                    HandleWallPassSuccess();
                    return;
                }
            }

            HandleObstacleCollision();
        }
    }

    private void HandleWallPassSuccess()
    {
        // Hiệu ứng confetti/sparkle lấp lánh rực rỡ
        PlaySuccessEffect();

        // Phát âm thanh qua tường
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPassWallSound();

        // Thêm coin
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddCoins(coinsPerWall);

        // Tăng điểm
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(pointsPerZone);

        // Tăng tốc độ chạy nhẹ nhàng
        if (jellyPlayer != null)
        {
            float newSpeed = Mathf.Min(jellyPlayer.ForwardSpeed + speedIncrement, maxForwardSpeed);
            jellyPlayer.ForwardSpeed = newSpeed;
        }

        OnWallPassedSuccess?.Invoke();
        OnScoreZonePassed?.Invoke();
    }

    private void HandleObstacleCollision()
    {
        isGameOver = true;

        if (jellyPlayer != null)
        {
            jellyPlayer.CanMove = false;
            jellyPlayer.ForwardSpeed = 0f;
        }

        PlayDeathEffect();

        // Phát âm thanh Game Over
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameOverSound();

        if (hideMeshOnDeath && playerMeshRenderer != null)
            playerMeshRenderer.enabled = false;

        OnObstacleHit?.Invoke();

        if (GameManager.Instance != null)
            GameManager.Instance.TriggerGameOver();
    }

    private void HandleScoreZonePassed(Collider scoreZone)
    {
        scoreZone.enabled = false;

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(pointsPerZone);

        if (jellyPlayer != null)
        {
            float newSpeed = Mathf.Min(jellyPlayer.ForwardSpeed + speedIncrement, maxForwardSpeed);
            jellyPlayer.ForwardSpeed = newSpeed;
        }

        OnScoreZonePassed?.Invoke();
    }

    // =========================================================================
    // HỆ THỐNG HIỆU ỨNG PARTICLE CHUẨN URP (CHỐNG MÀU TÍM 100%)
    // =========================================================================

    private static Texture2D GetRadialTexture()
    {
        if (s_radialTexture == null)
        {
            int res = 64;
            s_radialTexture = new Texture2D(res, res, TextureFormat.RGBA32, false);
            s_radialTexture.wrapMode = TextureWrapMode.Clamp;
            s_radialTexture.filterMode = FilterMode.Bilinear;

            float center = (res - 1) * 0.5f;
            float radius = center;

            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = Mathf.Clamp01(1f - (dist / radius));
                    alpha = Mathf.SmoothStep(0f, 1f, alpha);
                    // Hạt phát sáng màu trắng để ParticleSystem tự do nhuộm màu rực rỡ
                    s_radialTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            s_radialTexture.Apply();
        }
        return s_radialTexture;
    }

    private static Material GetParticleMaterial()
    {
        if (s_urpParticleMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                         ?? Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Sprites/Default");

            s_urpParticleMaterial = new Material(shader) { name = "URP_Particle_Material_Runtime" };

            Texture2D tex = GetRadialTexture();
            if (s_urpParticleMaterial.HasProperty("_BaseMap"))
                s_urpParticleMaterial.SetTexture("_BaseMap", tex);
            if (s_urpParticleMaterial.HasProperty("_MainTex"))
                s_urpParticleMaterial.SetTexture("_MainTex", tex);

            if (s_urpParticleMaterial.HasProperty("_Surface"))
                s_urpParticleMaterial.SetFloat("_Surface", 1); // Transparent
            if (s_urpParticleMaterial.HasProperty("_Blend"))
                s_urpParticleMaterial.SetFloat("_Blend", 0);   // Alpha
            s_urpParticleMaterial.renderQueue = 3000;
        }
        return s_urpParticleMaterial;
    }

    private void PlaySuccessEffect()
    {
        if (successParticleEffect != null)
        {
            Instantiate(successParticleEffect, transform.position, Quaternion.identity);
        }
        else
        {
            CreateDefaultSuccessEffect();
        }
    }

    private void CreateDefaultSuccessEffect()
    {
        GameObject vfxObj = new GameObject("Wall_Pass_Confetti");
        vfxObj.transform.position = transform.position + new Vector3(0f, 0.5f, 0f);

        ParticleSystem ps = vfxObj.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psRenderer = vfxObj.GetComponent<ParticleSystemRenderer>();
        if (psRenderer != null)
        {
            psRenderer.material = GetParticleMaterial();
        }

        var main = ps.main;
        main.startLifetime = 1.2f;
        main.startSpeed = 7f;
        main.startSize = 0.28f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;
        main.maxParticles = 40;

        // Màu sắc Confetti rực rỡ: Vàng kim tuyến và Xanh lá Neon
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.1f, 1.0f, 0.4f),   // Xanh lá neon
            new Color(1.0f, 0.85f, 0.15f)  // Vàng kim tuyến
        );

        var emission = ps.emission;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 40) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 50f;
        shape.radius = 0.4f;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        ps.Play();
    }

    private void PlayDeathEffect()
    {
        if (deathParticleEffect != null)
        {
            Instantiate(deathParticleEffect, transform.position, Quaternion.identity);
        }
        else
        {
            CreateDefaultSplatterEffect();
        }
    }

    private void CreateDefaultSplatterEffect()
    {
        GameObject vfxObj = new GameObject("Jelly_Death_Splatter");
        vfxObj.transform.position = transform.position + new Vector3(0f, 0.5f, 0f);

        ParticleSystem ps = vfxObj.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psRenderer = vfxObj.GetComponent<ParticleSystemRenderer>();
        if (psRenderer != null)
        {
            psRenderer.material = GetParticleMaterial();
        }

        var main = ps.main;
        main.startLifetime = 1.0f;
        main.startSpeed = 8f;
        main.startSize = 0.35f;

        // Lấy màu sắc tươi sáng của nhân vật hoặc màu cam rực rỡ
        Color splatterColor = new Color(1.0f, 0.55f, 0.1f);
        if (playerMeshRenderer != null && playerMeshRenderer.sharedMaterial != null)
        {
            splatterColor = playerMeshRenderer.sharedMaterial.color;
        }
        main.startColor = splatterColor;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 45) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        ps.Play();
    }
}

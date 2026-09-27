using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Quản lý va chạm của Player: Obstacle -> Game Over (nếu sai hình), ScoreZone -> tăng điểm & tốc độ.
/// Khi qua tường thành công: hiệu ứng confetti, âm thanh, + coin.
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

    private void Awake()
    {
        jellyPlayer = GetComponent<JellyPlayer>();
        playerMeshRenderer = GetComponent<MeshRenderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isGameOver) return;

        if (other.CompareTag("Obstacle"))
        {
            Wall wall = other.GetComponentInParent<Wall>();
            if (wall != null && wall.CheckPassSuccess(jellyPlayer.CurrentScaleY))
            {
                // Hình dáng hợp lệ → qua tường thành công!
                HandleWallPassSuccess();
                return;
            }

            HandleObstacleCollision();
        }
        else if (other.CompareTag("ScoreZone"))
        {
            HandleScoreZonePassed(other);
        }
    }

    private void HandleWallPassSuccess()
    {
        // Hiệu ứng confetti/sparkle
        PlaySuccessEffect();

        // Phát âm thanh qua tường
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPassWallSound();

        // Thêm coin
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddCoins(coinsPerWall);

        OnWallPassedSuccess?.Invoke();
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

    // === Hiệu ứng thành công ===

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
        vfxObj.transform.position = transform.position;

        ParticleSystem ps = vfxObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 1.2f;
        main.startSpeed = 6f;
        main.startSize = 0.2f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;
        main.maxParticles = 30;

        // Nhiều màu sắc (confetti)
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0f, 0.9f, 0.4f),   // Xanh lá
            new Color(1f, 0.85f, 0f)      // Vàng
        );

        var emission = ps.emission;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 30) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 45f;
        shape.radius = 0.3f;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        ps.Play();
    }

    // === Hiệu ứng chết ===

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
        vfxObj.transform.position = transform.position;

        ParticleSystem ps = vfxObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 1.0f;
        main.startSpeed = 8f;
        main.startSize = 0.35f;
        main.startColor = playerMeshRenderer != null ? playerMeshRenderer.sharedMaterial.color : new Color(0f, 0.85f, 1f);
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 40) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;

        ps.Play();
    }
}

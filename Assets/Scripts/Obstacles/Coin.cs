using UnityEngine;

/// <summary>
/// Gắn vào prefab coin. Khi Player chạm trigger -> thu thập coin.
/// Tạo hiệu ứng particle, phát âm thanh, thêm tiền vào CurrencyManager.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
[AddComponentMenu("Jelly Runner/Coin")]
public class Coin : MonoBehaviour
{
    [Header("--- Hiệu Ứng ---")]
    [SerializeField] private ParticleSystem collectParticleEffect;
    [SerializeField] private int coinValue = 1;

    [Header("--- Xoay Tròn ---")]
    [SerializeField] private float rotateSpeed = 180f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;

    private Vector3 startPosition;
    private bool isCollected = false;

    private void Start()
    {
        startPosition = transform.position;

        // Đảm bảo collider là trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void Update()
    {
        if (isCollected) return;

        // Xoay tròn
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);

        // Nhấp nhô lên xuống
        Vector3 pos = startPosition;
        pos.y += Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = pos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        // Chỉ phản ứng với Player
        if (!other.CompareTag("Player") && other.GetComponent<JellyPlayer>() == null)
            return;

        isCollected = true;

        // Thêm coin vào CurrencyManager
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddCoins(coinValue);

        // Phát âm thanh
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCoinCollectSound();

        // Tạo hiệu ứng particle
        PlayCollectEffect();

        // Hủy/tắt coin
        gameObject.SetActive(false);
    }

    private void PlayCollectEffect()
    {
        if (collectParticleEffect != null)
        {
            ParticleSystem vfx = Instantiate(collectParticleEffect, transform.position, Quaternion.identity);
            var main = vfx.main;
            main.stopAction = ParticleSystemStopAction.Destroy;
            vfx.Play();
        }
        else
        {
            CreateDefaultCollectEffect();
        }
    }

    private void CreateDefaultCollectEffect()
    {
        GameObject vfxObj = new GameObject("Coin_Collect_VFX");
        vfxObj.transform.position = transform.position;

        ParticleSystem ps = vfxObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.6f;
        main.startSpeed = 4f;
        main.startSize = 0.2f;
        main.startColor = new Color(1f, 0.85f, 0f); // Vàng
        main.stopAction = ParticleSystemStopAction.Destroy;
        main.maxParticles = 20;

        var emission = ps.emission;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 20) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        ps.Play();
    }
}

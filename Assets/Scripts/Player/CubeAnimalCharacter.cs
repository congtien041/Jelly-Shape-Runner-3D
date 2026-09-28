using UnityEngine;

/// <summary>
/// Quản lý Model thú 3D CuteMagic (Fox / T-Rex) khi được gắn vào JellyPlayer:
/// - Tự động scale x2 chuẩn bằng kích thước khối cube Unity (1x1x1).
/// - Căn chỉnh offset (0, -0.5, 0) để chân luôn bám sát mặt đường ở mọi hình dạng Jelly.
/// - Điều khiển Animator chạy thoăn thoắt nhún nhảy theo tốc độ chạy của Player.
/// - Thêm hiệu ứng nhún nhảy dẻo (Jelly Squash & Stretch) cực kỳ đáng yêu khi di chuyển.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Jelly Runner/Cube Animal Character")]
public class CubeAnimalCharacter : MonoBehaviour
{
    [Header("--- Hoạt Ảnh & Tốc Độ ---")]
    [Tooltip("Tốc độ animation bước chạy khớp với tốc độ di chuyển của JellyPlayer")]
    [SerializeField] private float walkAnimSpeed = 1.85f;

    [Header("--- Hiệu Ứng Nhún Nhảy Vui Nhộn ---")]
    [Tooltip("Độ nhún nảy nhẹ theo bước chạy tạo cảm giác dẻo hoạt hình")]
    [SerializeField] private float bounceFrequency = 14f;
    [SerializeField] private float bounceAmplitude = 0.04f;

    private Animator animator;
    private JellyPlayer parentPlayer;
    private Vector3 baseScale = new Vector3(2f, 2f, 2f);
    private Vector3 basePosition = new Vector3(0f, -0.5f, 0f);
    private float bounceTimer = 0f;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        parentPlayer = GetComponentInParent<JellyPlayer>();

        // Thiết lập chuẩn kích thước và vị trí chân chạm sàn
        transform.localPosition = basePosition;
        transform.localScale = baseScale;
        transform.localRotation = Quaternion.identity;

        if (animator != null)
        {
            animator.speed = walkAnimSpeed;
            animator.applyRootMotion = false;
        }
    }

    private void OnEnable()
    {
        // Đảm bảo chân luôn chạm sàn và scale x2 chuẩn xác
        transform.localPosition = basePosition;
        transform.localScale = baseScale;
        transform.localRotation = Quaternion.identity;

        if (animator != null)
        {
            animator.speed = walkAnimSpeed;
            // Kích hoạt state đi bộ nếu có
            if (animator.HasState(0, Animator.StringToHash("root_Walk")))
            {
                animator.Play("root_Walk", 0, 0f);
            }
        }
    }

    private void Update()
    {
        bool isMoving = parentPlayer != null && parentPlayer.CanMove && parentPlayer.ForwardSpeed > 0f;

        if (animator != null)
        {
            animator.speed = isMoving ? walkAnimSpeed : 0.6f;
        }

        // Thêm hiệu ứng nhún nhảy nhẹ theo nhịp chạy
        if (isMoving)
        {
            bounceTimer += Time.deltaTime * bounceFrequency;
            float bounceOffset = Mathf.Sin(bounceTimer) * bounceAmplitude;
            transform.localPosition = new Vector3(basePosition.x, basePosition.y + bounceOffset, basePosition.z);
        }
        else
        {
            transform.localPosition = basePosition;
        }
    }
}

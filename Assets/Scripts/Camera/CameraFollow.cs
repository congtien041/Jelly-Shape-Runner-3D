using UnityEngine;

/// <summary>
/// Camera bám theo Player mượt mà chỉ theo trục Z bằng Vector3.Lerp.
/// Giữ offset cố định X/Y để không giật khi Player biến dạng.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Jelly Runner/Camera Follow")]
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 4.5f, -7.5f);
    [SerializeField] private float lookAnglePitch = 20f;
    [SerializeField] private float followSpeed = 10f;

    private void Start()
    {
        InitializeTarget();
        transform.rotation = Quaternion.Euler(lookAnglePitch, 0f, 0f);
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            InitializeTarget();
            if (target == null) return;
        }

        Vector3 targetPosition = new Vector3(offset.x, offset.y, target.position.z + offset.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
    }

    private void InitializeTarget()
    {
        if (target != null) return;
        JellyPlayer player = FindAnyObjectByType<JellyPlayer>();
        if (player != null) target = player.transform;
    }
}

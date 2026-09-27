using UnityEngine;

/// <summary>
/// Controller cho Player khối Jelly trong game Hyper-casual 3D Jelly Shape Runner.
/// Di chuyển liên tục theo trục Z, vuốt kéo Y để biến dạng hình học (bảo toàn thể tích).
/// </summary>
[SelectionBase]
[DisallowMultipleComponent]
[AddComponentMenu("Jelly Runner/Jelly Player")]
public class JellyPlayer : MonoBehaviour
{
    [Header("--- Di Chuyển ---")]
    [SerializeField] private float forwardSpeed = 8f;
    [SerializeField] private bool canMove = true;

    [Header("--- Giới Hạn Biến Dạng ---")]
    [Range(0.1f, 1.0f)]
    [SerializeField] private float minY = 0.5f;
    [Range(1.5f, 5.0f)]
    [SerializeField] private float maxY = 3.0f;

    [Header("--- Bảo Toàn Thể Tích ---")]
    [SerializeField] private float baseVolume = 1.0f;
    [SerializeField] private float scaleZ = 1.0f;

    [Header("--- Điều Khiển ---")]
    [SerializeField] private float dragSensitivity = 3.5f;
    [SerializeField] private float lerpSpeed = 15f;

    [Header("--- Mặt Đất ---")]
    [SerializeField] private float floorY = 0.0f;

    private float currentScaleY = 1.0f;
    private float targetScaleY = 1.0f;
    private float currentScaleX = 1.0f;
    private float lastPointerY;
    private bool isDragging;
    private float distanceTraveled = 0f;

    #region Public Properties
    public float ForwardSpeed
    {
        get => forwardSpeed;
        set => forwardSpeed = Mathf.Max(0f, value);
    }

    public bool CanMove
    {
        get => canMove;
        set => canMove = value;
    }

    public float CurrentScaleY => currentScaleY;
    public float CurrentScaleX => currentScaleX;
    public float TargetScaleY => targetScaleY;
    public float DistanceTraveled => distanceTraveled;
    #endregion

    private void Awake()
    {
        // ĐẢM BẢO CÓ RIGIDBODY KINEMATIC — bắt buộc để OnTriggerEnter hoạt động
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        currentScaleY = Mathf.Clamp(transform.localScale.y, minY, maxY);
        targetScaleY = currentScaleY;

        if (baseVolume <= 0f)
            baseVolume = transform.localScale.x * transform.localScale.y;

        currentScaleX = baseVolume / currentScaleY;

        if (scaleZ <= 0f)
            scaleZ = transform.localScale.z > 0 ? transform.localScale.z : 1.0f;

        ApplyTransform(currentScaleX, currentScaleY, false);
        LoadAndApplySavedSkinAndEffect();
    }

    private GameObject activeEffectInstance;

    /// <summary>
    /// Áp dụng Material mới cho khối Jelly.
    /// </summary>
    public void ApplySkin(Material mat)
    {
        if (mat == null) return;
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null) mr.material = mat;
    }

    /// <summary>
    /// Gắn hiệu ứng (Trail hoặc Particle Aura) theo sau khối Jelly.
    /// </summary>
    public void ApplyEffect(GameObject effectPrefab)
    {
        if (activeEffectInstance != null)
        {
            Destroy(activeEffectInstance);
            activeEffectInstance = null;
        }

        if (effectPrefab == null) return;

        activeEffectInstance = Instantiate(effectPrefab, transform);
        activeEffectInstance.transform.localPosition = Vector3.zero;
        activeEffectInstance.transform.localRotation = Quaternion.identity;
    }

    private void LoadAndApplySavedSkinAndEffect()
    {
        // Nếu ShopManager đã tồn tại
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.ApplyEquippedItemsToPlayer(this);
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

        HandleInput();
        UpdateShapeAndMovement();
    }

    private void HandleInput()
    {
        Vector2 pointerPosition = Vector2.zero;
        bool pointerDown = false;
        bool pointerHeld = false;
        bool pointerUp = false;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            pointerPosition = touch.position;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    pointerDown = true;
                    pointerHeld = true;
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    pointerHeld = true;
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    pointerUp = true;
                    break;
            }
        }
        else
        {
            pointerPosition = Input.mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                pointerDown = true;
                pointerHeld = true;
            }
            else if (Input.GetMouseButton(0))
            {
                pointerHeld = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                pointerUp = true;
            }
        }

        if (pointerDown)
        {
            isDragging = true;
            lastPointerY = pointerPosition.y;
        }
        else if (isDragging && pointerHeld)
        {
            float deltaY = pointerPosition.y - lastPointerY;
            lastPointerY = pointerPosition.y;

            float normalizedDeltaY = deltaY / Screen.height;
            targetScaleY += normalizedDeltaY * dragSensitivity;
            targetScaleY = Mathf.Clamp(targetScaleY, minY, maxY);
        }

        if (pointerUp)
        {
            isDragging = false;
        }
    }

    private void UpdateShapeAndMovement()
    {
        currentScaleY = Mathf.Lerp(currentScaleY, targetScaleY, Time.deltaTime * lerpSpeed);
        currentScaleX = baseVolume / currentScaleY;
        ApplyTransform(currentScaleX, currentScaleY, canMove);
    }

    private void ApplyTransform(float scaleX, float scaleY, bool moveForward)
    {
        transform.localScale = new Vector3(scaleX, scaleY, scaleZ);

        Vector3 pos = transform.position;
        pos.y = floorY + (scaleY * 0.5f);

        if (moveForward)
        {
            float delta = forwardSpeed * Time.deltaTime;
            pos.z += delta;
            distanceTraveled += delta;
        }

        transform.position = pos;
    }

    public void ResetShape(bool instant = false)
    {
        targetScaleY = 1.0f;
        if (instant)
        {
            currentScaleY = 1.0f;
            currentScaleX = baseVolume / 1.0f;
            ApplyTransform(currentScaleX, currentScaleY, false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 groundCenter = new Vector3(transform.position.x, floorY, transform.position.z);
        Gizmos.DrawWireCube(groundCenter, new Vector3(5f, 0.02f, 5f));

        Gizmos.color = new Color(0f, 0.8f, 1f, 0.3f);
        float minWidth = baseVolume / minY;
        Gizmos.DrawWireCube(new Vector3(transform.position.x, floorY + (minY * 0.5f), transform.position.z), new Vector3(minWidth, minY, scaleZ));

        Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);
        float maxWidth = baseVolume / maxY;
        Gizmos.DrawWireCube(new Vector3(transform.position.x, floorY + (maxY * 0.5f), transform.position.z), new Vector3(maxWidth, maxY, scaleZ));
    }
}

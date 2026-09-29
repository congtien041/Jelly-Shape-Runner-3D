using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// MenuCharacterPreview — Hiển thị nhân vật 3D Clone ở Màn Hình Chính (Main Menu).
/// Tính năng:
/// 1. Render bằng Camera riêng biệt chiếu lên RenderTexture trong suốt (không bị UI che khuất hay đè lẫn).
/// 2. Tự động đồng bộ 100% trang bị (Nhân vật Fox/TRex/Jelly, Skin Vật Liệu, Hiệu Ứng Particle/Trail Aura).
/// 3. Cơ chế "Quay ra quay lại" tự nhiên (Auto-Oscillate Yaw) và cho phép vuốt tay/chuột để xoay 360 độ.
/// 4. Hiệu ứng di chuyển (Movement Bouncing / Locomotion):
///    - Khối Jelly: Nhún nhảy đàn hồi bảo toàn thể tích (Squash & Stretch) cực mượt.
///    - Cube Animals (Fox, T-Rex): Chạy animation bước đi liên tục.
///    - Bục Neon xoay phát sáng công nghệ cao (Cyberpunk/Arcade Podium).
/// 5. Cập nhật tức thì (Real-time) ngay khi mua hoặc đổi đồ trong Shop.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RawImage))]
[AddComponentMenu("Jelly Runner/Menu Character Preview")]
public class MenuCharacterPreview : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public static MenuCharacterPreview Instance { get; private set; }

    [Header("--- Cấu Hình Studio 3D ---")]
    [SerializeField] private Vector3 studioOffset = new Vector3(0f, -80f, 0f);
    [SerializeField] private int renderTextureResolution = 512;

    [Header("--- Hiệu Ứng Xoay (Quay Ra Quay Lại) ---")]
    [SerializeField] private float autoOscillateSpeed = 1.75f;
    [SerializeField] private float autoOscillateAmplitude = 32f;
    [SerializeField] private float dragRotateSensitivity = 0.55f;
    [SerializeField] private float returnToCenterDelay = 1.6f;

    [Header("--- Hiệu Ứng Nhún Nhảy Jelly (Squash & Stretch) ---")]
    [SerializeField] private float jellyBounceSpeed = 4.8f;
    [SerializeField] private float jellyBounceAmount = 0.18f;

    // Thành phần Studio 3D
    private GameObject studioRoot;
    private Camera previewCamera;
    private RenderTexture previewRT;
    private RawImage targetRawImage;

    // Điểm treo nhân vật & Bục
    private Transform characterPivot;
    private Transform podiumTransform;
    private Transform glowingRingTransform;

    // Instance nhân vật & hiệu ứng hiện tại
    private GameObject currentModelInstance;
    private GameObject currentEffectInstance;
    private JellyPlayer.CharacterType currentLoadedType = JellyPlayer.CharacterType.ClassicJelly;
    private Material currentLoadedMaterial = null;

    // Trạng thái xoay
    private float currentYaw = 0f;
    private float targetYaw = 0f;
    private bool isDragging = false;
    private float lastDragTime = -10f;

    private void Awake()
    {
        Instance = this;
        targetRawImage = GetComponent<RawImage>();
    }

    private void Start()
    {
        SetupStudio();
        SetupRawImage();

        // Đăng ký lắng nghe sự kiện thay đổi trang bị từ ShopManager
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.OnShopDataUpdated += RefreshEquippedCharacter;
        }

        RefreshEquippedCharacter();
    }

    private void OnDestroy()
    {
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.OnShopDataUpdated -= RefreshEquippedCharacter;
        }

        if (previewRT != null)
        {
            if (previewCamera != null) previewCamera.targetTexture = null;
            previewRT.Release();
            Destroy(previewRT);
            previewRT = null;
        }

        if (studioRoot != null)
        {
            Destroy(studioRoot);
            studioRoot = null;
        }
    }

    /// <summary>
    /// Xây dựng Studio 3D ngoài tầm nhìn của Camera chính (tại y = -80)
    /// </summary>
    private void SetupStudio()
    {
        if (studioRoot != null) return;

        studioRoot = new GameObject("[MenuCharacter_Studio]");
        studioRoot.transform.position = studioOffset;
        DontDestroyOnLoad(studioRoot);

        // 1. Tạo RenderTexture trong suốt
        previewRT = new RenderTexture(renderTextureResolution, renderTextureResolution, 24, RenderTextureFormat.ARGB32)
        {
            name = "RT_MenuCharacterPreview",
            antiAliasing = 4,
            filterMode = FilterMode.Bilinear,
            useMipMap = false
        };
        previewRT.Create();

        // 2. Tạo Camera Studio riêng biệt
        GameObject camObj = new GameObject("Studio_Camera");
        camObj.transform.SetParent(studioRoot.transform, false);
        camObj.transform.localPosition = new Vector3(0f, 1.35f, -3.8f);
        camObj.transform.localRotation = Quaternion.Euler(11f, 0f, 0f);

        previewCamera = camObj.AddComponent<Camera>();
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = new Color(0f, 0f, 0f, 0f); // Hoàn toàn trong suốt
        previewCamera.fieldOfView = 34f;
        previewCamera.nearClipPlane = 0.3f;
        previewCamera.farClipPlane = 15f;
        previewCamera.targetTexture = previewRT;
        previewCamera.depth = -5;

        // 3. Hệ thống Ánh sáng Studio chuyên nghiệp
        // Key Light (Chính diện bên phải)
        GameObject keyLightObj = new GameObject("Studio_KeyLight");
        keyLightObj.transform.SetParent(studioRoot.transform, false);
        keyLightObj.transform.localPosition = new Vector3(1.8f, 3.2f, -2.5f);
        keyLightObj.transform.localRotation = Quaternion.Euler(42f, -30f, 0f);
        Light keyLight = keyLightObj.AddComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.color = new Color(1.0f, 0.98f, 0.92f);
        keyLight.intensity = 1.3f;

        // Rim Light (Hắt sau lưng tạo viền sáng Neon sắc nét)
        GameObject rimLightObj = new GameObject("Studio_RimLight");
        rimLightObj.transform.SetParent(studioRoot.transform, false);
        rimLightObj.transform.localPosition = new Vector3(-1.8f, 2.5f, 2.2f);
        rimLightObj.transform.localRotation = Quaternion.Euler(30f, 145f, 0f);
        Light rimLight = rimLightObj.AddComponent<Light>();
        rimLight.type = LightType.Directional;
        rimLight.color = new Color(0.1f, 0.85f, 1.0f);
        rimLight.intensity = 1.1f;

        // 4. Bục đứng xoay Neon Podium (Cyberpunk / Modern Arcade)
        GameObject podiumObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        podiumObj.name = "Studio_Podium";
        podiumObj.transform.SetParent(studioRoot.transform, false);
        podiumObj.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        podiumObj.transform.localScale = new Vector3(2.4f, 0.07f, 2.4f);
        Collider podCol = podiumObj.GetComponent<Collider>();
        if (podCol != null) Destroy(podCol);

        // Tạo material bóng bẩy cho bục
        Shader urpShader = Shader.Find("Universal Render Pipeline/Lit") 
                        ?? Shader.Find("Universal Render Pipeline/Simple Lit") 
                        ?? Shader.Find("Standard");
        Material podMat = new Material(urpShader);
        podMat.color = new Color(0.08f, 0.11f, 0.18f);
        if (podMat.HasProperty("_BaseColor")) podMat.SetColor("_BaseColor", new Color(0.08f, 0.11f, 0.18f));
        if (podMat.HasProperty("_Smoothness")) podMat.SetFloat("_Smoothness", 0.85f);
        podiumObj.GetComponent<Renderer>().material = podMat;
        podiumTransform = podiumObj.transform;

        // Vòng phát sáng Neon quanh viền bục
        GameObject ringObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ringObj.name = "Podium_NeonRing";
        ringObj.transform.SetParent(podiumObj.transform, false);
        ringObj.transform.localPosition = new Vector3(0f, 0.52f, 0f);
        ringObj.transform.localScale = new Vector3(0.98f, 0.08f, 0.98f);
        Collider ringCol = ringObj.GetComponent<Collider>();
        if (ringCol != null) Destroy(ringCol);

        Material ringMat = new Material(urpShader);
        Color neonColor = new Color(0.0f, 0.9f, 1.0f);
        ringMat.color = neonColor;
        if (ringMat.HasProperty("_BaseColor")) ringMat.SetColor("_BaseColor", neonColor);
        if (ringMat.HasProperty("_EmissionColor"))
        {
            ringMat.EnableKeyword("_EMISSION");
            ringMat.SetColor("_EmissionColor", neonColor * 1.8f);
        }
        ringObj.GetComponent<Renderer>().material = ringMat;
        glowingRingTransform = ringObj.transform;

        // 5. Điểm treo nhân vật (Character Pivot)
        GameObject pivotObj = new GameObject("CharacterPivot");
        pivotObj.transform.SetParent(studioRoot.transform, false);
        pivotObj.transform.localPosition = new Vector3(0f, 0.52f, 0f);
        characterPivot = pivotObj.transform;
    }

    private void SetupRawImage()
    {
        if (targetRawImage == null) targetRawImage = GetComponent<RawImage>();
        if (targetRawImage != null && previewRT != null)
        {
            targetRawImage.texture = previewRT;
            targetRawImage.color = Color.white;
            targetRawImage.raycastTarget = true;
        }
    }

    /// <summary>
    /// Đồng bộ nhân vật clone theo đúng những gì đang trang bị trong game
    /// </summary>
    public void RefreshEquippedCharacter()
    {
        if (characterPivot == null) SetupStudio();

        // 1. Xác định nhân vật & Skin đang trang bị
        JellyPlayer.CharacterType charType = JellyPlayer.CharacterType.ClassicJelly;
        Material playerSkinMat = null;
        GameObject effectPrefab = null;

        if (ShopManager.Instance != null)
        {
            int playerSkinIdx = ShopManager.Instance.GetEquippedIndex(ShopManager.ItemType.PlayerSkin);
            var items = ShopManager.Instance.GetItems(ShopManager.ItemType.PlayerSkin);
            if (playerSkinIdx >= 0 && playerSkinIdx < items.Count)
            {
                var item = items[playerSkinIdx];
                charType = item.characterType;
                playerSkinMat = item.materialAsset;
            }

            effectPrefab = ShopManager.Instance.GetEquippedEffectPrefab();
        }
        else
        {
            // Fallback nếu chưa nạp ShopManager
            charType = (JellyPlayer.CharacterType)PlayerPrefs.GetInt("SelectedCharacter", 0);
        }

        currentLoadedType = charType;
        currentLoadedMaterial = playerSkinMat;

        // 2. Xóa model cũ nếu có
        if (currentModelInstance != null)
        {
            Destroy(currentModelInstance);
            currentModelInstance = null;
        }
        if (currentEffectInstance != null)
        {
            Destroy(currentEffectInstance);
            currentEffectInstance = null;
        }

        // 3. Khởi tạo model mới
        if (charType == JellyPlayer.CharacterType.Fox || charType == JellyPlayer.CharacterType.TRex)
        {
            SpawnCubeAnimalModel(charType);
        }
        else
        {
            SpawnClassicJellyModel(playerSkinMat);
        }

        // 4. Áp dụng hiệu ứng Particle/Trail đang trang bị
        if (effectPrefab != null)
        {
            currentEffectInstance = Instantiate(effectPrefab, characterPivot);
            currentEffectInstance.transform.localPosition = Vector3.zero;
            currentEffectInstance.transform.localRotation = Quaternion.identity;
            currentEffectInstance.transform.localScale = Vector3.one * 0.9f;

            // Kích hoạt tất cả ParticleSystem để phát hiệu ứng liên tục ở Menu
            foreach (ParticleSystem ps in currentEffectInstance.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = ps.main;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                ps.Play();
            }
        }
    }

    /// <summary>
    /// Khởi tạo khối Jelly mềm dẻo với Skin đã trang bị
    /// </summary>
    private void SpawnClassicJellyModel(Material skinMat)
    {
        currentModelInstance = GameObject.CreatePrimitive(PrimitiveType.Cube);
        currentModelInstance.name = "Preview_ClassicJelly";
        currentModelInstance.transform.SetParent(characterPivot, false);
        currentModelInstance.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        currentModelInstance.transform.localScale = Vector3.one * 1.35f;

        Collider c = currentModelInstance.GetComponent<Collider>();
        if (c != null) Destroy(c);

        Renderer r = currentModelInstance.GetComponent<Renderer>();
        if (skinMat != null)
        {
            r.material = skinMat;
        }
        else
        {
            // Fallback Neon Cyan nếu chưa có material
            Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material defaultMat = new Material(s);
            Color cyan = new Color(0f, 0.88f, 1f);
            defaultMat.color = cyan;
            if (defaultMat.HasProperty("_BaseColor")) defaultMat.SetColor("_BaseColor", cyan);
            r.material = defaultMat;
        }
    }

    /// <summary>
    /// Khởi tạo thú cưng Cube Animal (Fox hoặc T-Rex) với Animation bước đi tung tăng
    /// </summary>
    private void SpawnCubeAnimalModel(JellyPlayer.CharacterType type)
    {
        GameObject prefab = null;

        if (type == JellyPlayer.CharacterType.Fox)
        {
            prefab = Resources.Load<GameObject>("Characters/Model_Fox");
#if UNITY_EDITOR
            if (prefab == null)
                prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Model_Fox.prefab")
                      ?? UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CuteMagic_CubeAnimals_Free/CubeAnimals_Free/Prefab_1/Fox.prefab");
#endif
        }
        else if (type == JellyPlayer.CharacterType.TRex)
        {
            prefab = Resources.Load<GameObject>("Characters/Model_TRex");
#if UNITY_EDITOR
            if (prefab == null)
                prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Model_TRex.prefab")
                      ?? UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CuteMagic_CubeAnimals_T-REX_Free/CubeAnimals_T-REX_Free/Prefab/Animals/T_Rex.prefab");
#endif
        }

        if (prefab == null)
        {
            // Nếu không tìm thấy prefab, fallback về Jelly
            SpawnClassicJellyModel(null);
            return;
        }

        currentModelInstance = Instantiate(prefab, characterPivot);
        currentModelInstance.name = "Preview_" + type;
        currentModelInstance.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        currentModelInstance.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // Xoay mặt về phía trước

        float scale = (type == JellyPlayer.CharacterType.Fox) ? 1.45f : 1.3f;
        currentModelInstance.transform.localScale = Vector3.one * scale;

        // Xóa các component gameplay không cần thiết
        foreach (Collider col in currentModelInstance.GetComponentsInChildren<Collider>(true))
            Destroy(col);

        // Kích hoạt Animator Animation bước đi (Walk / root_Walk)
        Animator anim = currentModelInstance.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.applyRootMotion = false;
            anim.speed = 1.35f;

            if (anim.HasState(0, Animator.StringToHash("root_Walk")))
                anim.Play("root_Walk", 0, 0f);
            else if (anim.HasState(0, Animator.StringToHash("Walk")))
                anim.Play("Walk", 0, 0f);
            else if (anim.HasState(0, Animator.StringToHash("Run")))
                anim.Play("Run", 0, 0f);
        }
    }

    private void Update()
    {
        HandleRotation();
        HandleMovementAnimation();
    }

    /// <summary>
    /// Xử lý Xoay: "Quay ra quay lại" tự nhiên khi Idle và xoay theo ngón tay/chuột khi vuốt
    /// </summary>
    private void HandleRotation()
    {
        if (characterPivot == null) return;

        if (!isDragging)
        {
            // Nếu đã buông tay quá returnToCenterDelay -> Tự động quay ra quay lại nhịp nhàng
            if (Time.time - lastDragTime > returnToCenterDelay)
            {
                // Dao động hình sin: -32 độ tới +32 độ mượt mà
                targetYaw = Mathf.Sin(Time.time * autoOscillateSpeed) * autoOscillateAmplitude;
                currentYaw = Mathf.Lerp(currentYaw, targetYaw, Time.deltaTime * 3.2f);
            }
        }

        characterPivot.localRotation = Quaternion.Euler(0f, currentYaw, 0f);

        // Bục phát sáng xoay chậm tạo chiều sâu công nghệ
        if (podiumTransform != null)
        {
            podiumTransform.Rotate(Vector3.up, -15f * Time.deltaTime, Space.Self);
        }
    }

    /// <summary>
    /// Xử lý hiệu ứng di chuyển nhún nhảy (Squash & Stretch) cho Jelly
    /// </summary>
    private void HandleMovementAnimation()
    {
        if (currentModelInstance == null) return;

        if (currentLoadedType == JellyPlayer.CharacterType.ClassicJelly)
        {
            // Hiệu ứng nhún nhảy đàn hồi bảo toàn thể tích sinh động
            float t = Time.time * jellyBounceSpeed;
            float pulse = Mathf.Sin(t);

            // Co giãn trục Y
            float sy = 1.35f * (1.0f + pulse * jellyBounceAmount);
            // Bảo toàn thể tích: X và Z tỷ lệ nghịch với căn bậc hai của Y
            float sx = 1.35f / Mathf.Sqrt(1.0f + pulse * jellyBounceAmount);
            float sz = sx;

            currentModelInstance.transform.localScale = new Vector3(sx, sy, sz);

            // Nâng hạ vị trí theo độ nhún đàn hồi
            float baseY = 0.55f;
            float jumpY = Mathf.Max(0f, pulse) * 0.16f;
            currentModelInstance.transform.localPosition = new Vector3(0f, baseY + jumpY, 0f);
        }
        else
        {
            // Với Fox và T-Rex: Nhấp nhô nhẹ nhàng theo nhịp bước chân
            float stepBob = Mathf.Abs(Mathf.Sin(Time.time * 6.5f)) * 0.08f;
            currentModelInstance.transform.localPosition = new Vector3(0f, 0.05f + stepBob, 0f);
        }
    }

    // =========================================================================
    // TƯƠNG TÁC VUỐT / KÉO ĐỂ XOAY NHÂN VẬT 360 ĐỘ
    // =========================================================================

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;
        lastDragTime = Time.time;

        // Xoay nhân vật mượt mà theo thao tác kéo ngang
        currentYaw -= eventData.delta.x * dragRotateSensitivity;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        lastDragTime = Time.time;
    }
}

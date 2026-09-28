using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ĐẶC QUYỀN SKIN 3D (Fox & T-Rex) — CƠ CHẾ PHÂN TÁCH & HỢP NHẤT ĐỘNG (1, 2, 3, 4, 5 THÀNH VIÊN):
/// 1. BAN ĐẦU / VUÔNG CHUẨN: CHỈ 1 CON THÚ DUY NHẤT to lớn uy nghi (Scale 2.0x chuẩn Cube Unity).
/// 2. KHI VUỐT CAO: Tự động phân tách thành các con nhỏ và xếp chồng thành tháp Totem:
///    - Độ cao 1: 1 con duy nhất
///    - Độ cao 2: Phân tách thành 2 con (2 tầng tháp)
///    - Độ cao 3: Phân tách thành 3 con (3 tầng tháp)
///    - Độ cao 4: Phân tách thành 4 con (4 tầng tháp)
///    - Độ cao 5: Phân tách thành 5 con (5 tầng tháp vươn chạm đỉnh độ cao 5.0!)
/// 3. KHI VUỐT DẸT: Phân tách thành 2, 3, 4 hoặc 5 con nhỏ dàn hàng ngang sóng đôi.
/// 4. KHI THẢ TAY / TRỞ LẠI VUÔNG: Tự động thu gọn và hợp nhất trở lại thành 1 con to duy nhất!
/// 5. HIỆU ỨNG PHÂN BÀO SIÊU MƯỢT (MITOSIS): Không dùng SetActive để tránh reset Animator,
///    các con thú mọc ra và thu nhỏ bằng Lerp Scale mượt mà 60 FPS.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Jelly Runner/Cube Animal Formation")]
public class CubeAnimalFormation : MonoBehaviour
{
    public enum FormationState
    {
        SingleNormal,   // Ban đầu / Vuông: CHỈ 1 CON DUY NHẤT (To đẹp chuẩn Cube Unity)
        VerticalStack,  // Vuốt cao: Phân tách 2, 3, 4 hoặc 5 con xếp tháp đứng
        HorizontalLine  // Vuốt dẹt: Phân tách 2, 3, 4 hoặc 5 con dàn hàng ngang
    }

    [Header("--- Tham Chiếu Model ---")]
    [SerializeField] private GameObject animalModelPrefab;

    [Header("--- Cấu Hình Đội Hình ---")]
    [SerializeField] private int maxSquadCount = 5;
    [SerializeField] private float transitionSpeed = 16f;

    private readonly List<Transform> members = new List<Transform>();
    private readonly List<Animator> memberAnimators = new List<Animator>();
    private JellyPlayer parentPlayer;
    private FormationState currentState = FormationState.SingleNormal;

    // Ngưỡng chuyển đổi trạng thái
    private const float THRESHOLD_FLAT = 0.85f;  // scaleY < 0.85 => Bắt đầu phân tách dàn ngang
    private const float THRESHOLD_TALL = 1.35f;  // scaleY > 1.35 => Bắt đầu phân tách xếp tháp đứng

    public void Initialize(GameObject prefab, JellyPlayer player)
    {
        animalModelPrefab = prefab;
        parentPlayer = player;
        SpawnFormationMembers();
    }

    private void Awake()
    {
        if (parentPlayer == null)
            parentPlayer = GetComponentInParent<JellyPlayer>();

        // Xóa sạch Hologram_Bounding_Guide cũ nếu có
        Transform oldGuide = transform.Find("Hologram_Bounding_Guide");
        if (oldGuide != null) Destroy(oldGuide.gameObject);
    }

    private void Start()
    {
        if (members.Count == 0 && animalModelPrefab != null)
        {
            SpawnFormationMembers();
        }
    }

    private void SpawnFormationMembers()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        members.Clear();
        memberAnimators.Clear();

        if (animalModelPrefab == null) return;

        for (int i = 0; i < maxSquadCount; i++)
        {
            GameObject member = Instantiate(animalModelPrefab, transform);
            member.name = $"Animal_Unit_{i}";

            CubeAnimalCharacter oldHelper = member.GetComponent<CubeAnimalCharacter>();
            if (oldHelper != null) Destroy(oldHelper);

            foreach (Renderer r in member.GetComponentsInChildren<Renderer>(true))
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                r.receiveShadows = true;
            }

            Animator anim = member.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.applyRootMotion = false;
                anim.speed = 1.85f;
                if (anim.HasState(0, Animator.StringToHash("root_Walk")))
                {
                    anim.Play("root_Walk", 0, Random.Range(0f, 0.5f));
                }
                memberAnimators.Add(anim);
            }

            // Tất cả thành viên đều giữ active để Animator chạy liên tục không bị giật khựng
            member.SetActive(true);
            member.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            member.transform.localScale = (i == 0) ? new Vector3(2f, 2f, 2f) : Vector3.zero;

            members.Add(member.transform);
        }
    }

    private void LateUpdate()
    {
        if (parentPlayer == null || members.Count < maxSquadCount) return;

        float scaleX = parentPlayer.transform.localScale.x;
        float scaleY = parentPlayer.transform.localScale.y;
        bool isMoving = parentPlayer.CanMove && parentPlayer.ForwardSpeed > 0f;

        // Đồng bộ tốc độ hoạt ảnh chạy
        foreach (var anim in memberAnimators)
        {
            if (anim != null)
                anim.speed = isMoving ? 1.85f : 0.6f;
        }

        // Xác định trạng thái đội hình
        if (scaleY < THRESHOLD_FLAT)
        {
            currentState = FormationState.HorizontalLine;
        }
        else if (scaleY > THRESHOLD_TALL)
        {
            currentState = FormationState.VerticalStack;
        }
        else
        {
            currentState = FormationState.SingleNormal;
        }

        UpdateMembersTransform(scaleX, scaleY);
    }

    /// <summary>
    /// Tính toán vị trí và scale mượt mà cho 5 thành viên.
    /// Không bao giờ bị méo mó (Counter-Scaling 1:1:1), tự động phân tách và hợp nhất như phép thuật.
    /// </summary>
    private void UpdateMembersTransform(float parentScaleX, float parentScaleY)
    {
        float dt = Time.deltaTime * transitionSpeed;

        float invX = 1f / Mathf.Max(0.01f, parentScaleX);
        float invY = 1f / Mathf.Max(0.01f, parentScaleY);

        switch (currentState)
        {
            // =========================================================================
            // 1. BAN ĐẦU / VUÔNG CHUẨN: CHỈ 1 CON TO DUY NHẤT (SCALE 2.0x CHUẨN CUBE)
            // =========================================================================
            case FormationState.SingleNormal:
            default:
            {
                float mainScale = 2.0f;
                Vector3 targetScale0 = new Vector3(mainScale * invX, mainScale * invY, mainScale);
                Vector3 targetPos0 = new Vector3(0f, -0.5f, 0f);

                Transform lead = members[0];
                lead.localPosition = Vector3.Lerp(lead.localPosition, targetPos0, dt);
                lead.localScale = Vector3.Lerp(lead.localScale, targetScale0, dt);
                lead.localRotation = Quaternion.Slerp(lead.localRotation, Quaternion.identity, dt);

                // Các con phụ 1, 2, 3, 4 thu nhỏ về 0 và nhập vào con 0
                for (int i = 1; i < maxSquadCount; i++)
                {
                    Transform sub = members[i];
                    sub.localPosition = Vector3.Lerp(sub.localPosition, lead.localPosition, dt);
                    sub.localScale = Vector3.Lerp(sub.localScale, Vector3.zero, dt);
                }
                break;
            }

            // =========================================================================
            // 2. VUỐT CAO: PHÂN TÁCH CHUẨN XÁC THEO ĐỘ CAO 2, 3, 4 VÀ 5!
            // =========================================================================
            case FormationState.VerticalStack:
            {
                // Xác định số tầng cần phân tách theo độ cao thực tế:
                // - scaleY từ 1.35 -> 2.35: 2 con (2 tầng tháp)
                // - scaleY từ 2.35 -> 3.35: 3 con (3 tầng tháp)
                // - scaleY từ 3.35 -> 4.35: 4 con (4 tầng tháp)
                // - scaleY >= 4.35 (lên tới 5.0): 5 con (5 tầng tháp chạm đỉnh 5.0!)
                int targetCount;
                if (parentScaleY >= 4.35f)
                    targetCount = 5;
                else if (parentScaleY >= 3.35f)
                    targetCount = 4;
                else if (parentScaleY >= 2.35f)
                    targetCount = 3;
                else
                    targetCount = 2;

                // Chiều cao mỗi tầng trong local space (phân bố đều từ đáy -0.5 lên đỉnh +0.5)
                float stepLocalY = 1.0f / targetCount;

                // Kích thước của mỗi con thú vuông vắn 1:1:1 không méo
                float uniformScale = Mathf.Clamp((parentScaleY / targetCount) * 1.55f, 0.72f, 1.25f);
                Vector3 activeScale = new Vector3(uniformScale * invX, uniformScale * invY, uniformScale);

                // Cập nhật các con đang hoạt động (từ 0 đến targetCount - 1)
                for (int i = 0; i < targetCount; i++)
                {
                    Transform t = members[i];
                    Vector3 targetPos = new Vector3(0f, -0.5f + (stepLocalY * i), 0f);

                    t.localPosition = Vector3.Lerp(t.localPosition, targetPos, dt);
                    t.localScale = Vector3.Lerp(t.localScale, activeScale, dt);
                    t.localRotation = Quaternion.Slerp(t.localRotation, Quaternion.identity, dt);
                }

                // Các con chưa dùng đến (từ targetCount đến 4) thu nhỏ mượt mà vào con đỉnh tháp
                Transform topMember = members[targetCount - 1];
                for (int i = targetCount; i < maxSquadCount; i++)
                {
                    Transform unused = members[i];
                    unused.localPosition = Vector3.Lerp(unused.localPosition, topMember.localPosition, dt);
                    unused.localScale = Vector3.Lerp(unused.localScale, Vector3.zero, dt);
                }
                break;
            }

            // =========================================================================
            // 3. VUỐT DẸT: PHÂN TÁCH CHUẨN XÁC DÀN HÀNG NGANG 2, 3, 4 HOẶC 5 CON!
            // =========================================================================
            case FormationState.HorizontalLine:
            {
                // Xác định số con dàn ngang theo độ dẹt:
                // - scaleY từ 0.65 -> 0.85: 2 con dàn ngang
                // - scaleY từ 0.48 -> 0.65: 3 con dàn ngang
                // - scaleY từ 0.36 -> 0.48: 4 con dàn ngang
                // - scaleY < 0.36: 5 con dàn ngang
                int targetCount;
                if (parentScaleY < 0.36f)
                    targetCount = 5;
                else if (parentScaleY < 0.48f)
                    targetCount = 4;
                else if (parentScaleY < 0.65f)
                    targetCount = 3;
                else
                    targetCount = 2;

                // Kích thước vừa vặn khe dẹt
                float uniformScale = Mathf.Clamp(parentScaleY * 1.5f, 0.60f, 1.15f);
                Vector3 activeScale = new Vector3(uniformScale * invX, uniformScale * invY, uniformScale);

                float stepWorldX = uniformScale * 0.60f;
                float stepLocalX = stepWorldX * invX;

                // Cập nhật các con đang dàn hàng ngang
                for (int i = 0; i < targetCount; i++)
                {
                    Transform t = members[i];
                    float offsetMultiplier = (i - ((targetCount - 1) * 0.5f));
                    Vector3 targetPos = new Vector3(offsetMultiplier * stepLocalX, -0.5f, 0f);

                    t.localPosition = Vector3.Lerp(t.localPosition, targetPos, dt);
                    t.localScale = Vector3.Lerp(t.localScale, activeScale, dt);
                    t.localRotation = Quaternion.Slerp(t.localRotation, Quaternion.identity, dt);
                }

                // Các con chưa dùng đến thu nhỏ mượt mà vào giữa
                for (int i = targetCount; i < maxSquadCount; i++)
                {
                    Transform unused = members[i];
                    unused.localPosition = Vector3.Lerp(unused.localPosition, members[0].localPosition, dt);
                    unused.localScale = Vector3.Lerp(unused.localScale, Vector3.zero, dt);
                }
                break;
            }
        }
    }
}

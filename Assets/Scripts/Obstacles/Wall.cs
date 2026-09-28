using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Bức tường có lỗ hổng (Wall Obstacle).
/// Tích hợp:
/// 1. Khung Viền Phát Sáng Neon (Neon Glow Hole Frame) 4 cạnh rõ nét.
/// 2. Vạch Phân Tầng Phát Sáng (Tier Dividers): Chia lỗ thành 1, 2, 3 hoặc 4 ô trực quan,
///    giúp người chơi nhìn từ xa 40m là BIẾT NGAY cần biến hình mấy con thú!
/// 3. Báo Hiệu Khớp Hình Thời Gian Thực (Real-time Match Indicator): Khi người chơi vuốt đúng
///    hình dạng của lỗ, viền Neon lập tức ĐỔI SANG MÀU XANH LÁ DẠ QUANG để báo hiệu "ĐÃ KHỚP!".
/// 4. Mở Cổng An Toàn (Safe Pass): Loại bỏ hoàn toàn tình trạng quẹt mép chết oan.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Jelly Runner/Wall")]
public class Wall : MonoBehaviour
{
    [Header("--- Hình Dạng Yêu Cầu ---")]
    [SerializeField] private float requiredScaleY = 1.0f;
    [SerializeField] private float requiredScaleX = 1.0f;
    [Range(0.05f, 0.6f)]
    [SerializeField] private float tolerance = 0.38f;

    [Header("--- Khối Ghép Tạo Tường ---")]
    [SerializeField] private Transform topBlock;
    [SerializeField] private Transform leftBlock;
    [SerializeField] private Transform rightBlock;

    [Header("--- Thông Số Tường ---")]
    [SerializeField] private float totalWallWidth = 8.0f;
    [SerializeField] private float totalWallHeight = 6.5f;
    [SerializeField] private float floorY = 0.0f;

    [Header("--- Trigger Chui Qua ---")]
    [SerializeField] private BoxCollider passTrigger;

    // Trạng thái đánh giá
    private bool isResolved = false;
    private bool isPassed = false;

    // Các thành phần viền Neon phát sáng
    private GameObject neonFrameContainer;
    private Transform borderTop;
    private Transform borderBottom;
    private Transform borderLeft;
    private Transform borderRight;
    private Transform hologramVeil;
    private Material neonBorderMaterial;
    private Material hologramMaterial;

    // Danh sách các vạch phân tầng phát sáng
    private readonly List<Transform> dividerBars = new List<Transform>();

    private JellyPlayer cachedPlayer;

    private static readonly Color ColorFlat = new Color(1.0f, 0.75f, 0.0f);   // Dẹt: Vàng Hổ Phách Neon
    private static readonly Color ColorSquare = new Color(0.1f, 1.0f, 0.4f);  // Vuông: Xanh Ngọc Neon
    private static readonly Color ColorTall = new Color(0.0f, 0.85f, 1.0f);   // Cao: Xanh Cyan Neon
    private static readonly Color ColorSuccess = new Color(0.0f, 1.0f, 0.3f); // Thành công/Đang khớp: Xanh Lá Neon
    private static readonly Color ColorFail = new Color(1.0f, 0.2f, 0.2f);    // Thất bại: Đỏ Neon

    public float RequiredScaleY => requiredScaleY;
    public float RequiredScaleX => requiredScaleX;
    public bool IsPassed => isPassed;
    public bool IsResolved => isResolved;

    private void Awake()
    {
        EnsureNeonFrameCreated();
    }

    private void Start()
    {
        EnsureCollidersConfigured();
        if (cachedPlayer == null)
            cachedPlayer = FindAnyObjectByType<JellyPlayer>();
    }

    private void Update()
    {
        // Báo hiệu khớp hình thời gian thực khi người chơi tiếp cận bức tường
        if (isResolved) return;

        if (cachedPlayer == null)
            cachedPlayer = FindAnyObjectByType<JellyPlayer>();

        if (cachedPlayer == null) return;

        float distZ = transform.position.z - cachedPlayer.transform.position.z;
        // Trong cự ly tiếp cận từ 0.5m đến 32m
        if (distZ > 0.5f && distZ < 32f)
        {
            // TÍNH NĂNG HƯỚNG DẪN 1 PHÚT ĐẦU:
            // Chỉ trong 1 phút đầu tiên (thời gian chơi < 60s), khi vuốt đúng thì viền tường mới sáng Xanh Lá để hướng dẫn.
            // Sau 1 phút (>= 60s), tắt tính năng này, viền giữ màu gốc để người chơi tự phán đoán bằng kỹ năng!
            // Khi chơi lại (gameplayDuration reset về 0), 1 phút hướng dẫn này sẽ tự động hiện lại.
            bool isTutorialActive = WallSpawner.CurrentGameplayDuration < 60f;

            if (isTutorialActive)
            {
                bool isMatching = CheckPassSuccess(cachedPlayer.CurrentScaleY);
                if (isMatching)
                {
                    // ĐANG KHỚP HÌNH: Sáng bừng Xanh Lá Dạ Quang hướng dẫn người chơi
                    SetNeonColor(ColorSuccess);
                }
                else
                {
                    SetNeonColor(GetThemeColorForShape(requiredScaleY));
                }
            }
            else
            {
                // Sau 1 phút: Giữ nguyên màu theme gốc của lỗ tường
                SetNeonColor(GetThemeColorForShape(requiredScaleY));
            }
        }
    }

    private void EnsureCollidersConfigured()
    {
        if (passTrigger != null)
        {
            passTrigger.isTrigger = true;
            passTrigger.tag = "ScoreZone";
        }
    }

    public void ConfigureBlocks(Transform top, Transform left, Transform right, BoxCollider trigger = null)
    {
        topBlock = top;
        leftBlock = left;
        rightBlock = right;
        passTrigger = trigger;
        EnsureNeonFrameCreated();
        UpdateHoleGeometry();
    }

    public void Setup(float targetY, float baseVolume = 1.0f)
    {
        requiredScaleY = targetY;
        requiredScaleX = baseVolume / targetY;
        isResolved = false;
        isPassed = false;

        SetObstacleCollidersEnabled(true);

        EnsureNeonFrameCreated();
        UpdateHoleGeometry();

        Color themeColor = GetThemeColorForShape(targetY);
        SetNeonColor(themeColor);
    }

    private Color GetThemeColorForShape(float targetY)
    {
        if (targetY < 0.8f) return ColorFlat;
        if (targetY > 1.8f) return ColorTall;
        return ColorSquare;
    }

    public void SetWallMaterial(Material mat)
    {
        if (mat == null) return;
        if (topBlock != null)
        {
            MeshRenderer r = topBlock.GetComponent<MeshRenderer>();
            if (r != null) r.sharedMaterial = mat;
        }
        if (leftBlock != null)
        {
            MeshRenderer r = leftBlock.GetComponent<MeshRenderer>();
            if (r != null) r.sharedMaterial = mat;
        }
        if (rightBlock != null)
        {
            MeshRenderer r = rightBlock.GetComponent<MeshRenderer>();
            if (r != null) r.sharedMaterial = mat;
        }
    }

    public void UpdateHoleGeometry()
    {
        float holeW = requiredScaleX;
        float holeH = requiredScaleY;

        // 1. Khối Top
        if (topBlock != null)
        {
            float topHeight = Mathf.Max(0.05f, totalWallHeight - holeH);
            topBlock.localScale = new Vector3(holeW, topHeight, 1.0f);
            topBlock.localPosition = new Vector3(0f, floorY + holeH + (topHeight * 0.5f), 0f);
        }

        // 2. Khối Left
        if (leftBlock != null)
        {
            float sideWidth = Mathf.Max(0.05f, (totalWallWidth - holeW) * 0.5f);
            leftBlock.localScale = new Vector3(sideWidth, totalWallHeight, 1.0f);
            leftBlock.localPosition = new Vector3(-(holeW * 0.5f) - (sideWidth * 0.5f), floorY + (totalWallHeight * 0.5f), 0f);
        }

        // 3. Khối Right
        if (rightBlock != null)
        {
            float sideWidth = Mathf.Max(0.05f, (totalWallWidth - holeW) * 0.5f);
            rightBlock.localScale = new Vector3(sideWidth, totalWallHeight, 1.0f);
            rightBlock.localPosition = new Vector3((holeW * 0.5f) + (sideWidth * 0.5f), floorY + (totalWallHeight * 0.5f), 0f);
        }

        // 4. Trigger chui qua
        if (passTrigger != null)
        {
            passTrigger.size = new Vector3(holeW + 0.1f, holeH + 0.1f, 1.2f);
            passTrigger.center = new Vector3(0f, floorY + (holeH * 0.5f), 0f);
        }

        // 5. Khung Viền Phát Sáng Neon & Vạch Phân Tầng
        UpdateNeonFrameTransforms(holeW, holeH);
    }

    private void EnsureNeonFrameCreated()
    {
        if (neonFrameContainer != null) return;

        neonFrameContainer = new GameObject("NeonHoleFrame");
        neonFrameContainer.transform.SetParent(transform, false);
        neonFrameContainer.transform.localPosition = Vector3.zero;

        Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit")
                          ?? Shader.Find("Sprites/Default");

        neonBorderMaterial = new Material(unlitShader) { name = "Mat_NeonBorder_Runtime" };
        hologramMaterial = new Material(unlitShader) { name = "Mat_HologramVeil_Runtime" };

        borderTop = CreateBorderBar("Border_Top", neonBorderMaterial);
        borderBottom = CreateBorderBar("Border_Bottom", neonBorderMaterial);
        borderLeft = CreateBorderBar("Border_Left", neonBorderMaterial);
        borderRight = CreateBorderBar("Border_Right", neonBorderMaterial);

        // Màn quét Hologram mờ ảo ở giữa lỗ
        GameObject veilObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        veilObj.name = "Hologram_Laser_Veil";
        veilObj.transform.SetParent(neonFrameContainer.transform, false);
        Destroy(veilObj.GetComponent<Collider>());
        hologramVeil = veilObj.transform;
        MeshRenderer veilRend = veilObj.GetComponent<MeshRenderer>();
        if (veilRend != null) veilRend.sharedMaterial = hologramMaterial;
    }

    private Transform CreateBorderBar(string barName, Material mat)
    {
        GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bar.name = barName;
        bar.transform.SetParent(neonFrameContainer.transform, false);
        Destroy(bar.GetComponent<Collider>());

        MeshRenderer mr = bar.GetComponent<MeshRenderer>();
        if (mr != null) mr.sharedMaterial = mat;
        return bar.transform;
    }

    private void UpdateNeonFrameTransforms(float holeW, float holeH)
    {
        if (neonFrameContainer == null) return;

        float thickness = 0.08f;
        float frontZ = -0.52f;

        if (borderTop != null)
        {
            borderTop.localScale = new Vector3(holeW + (thickness * 2f), thickness, thickness);
            borderTop.localPosition = new Vector3(0f, floorY + holeH + (thickness * 0.5f), frontZ);
        }

        if (borderBottom != null)
        {
            borderBottom.localScale = new Vector3(holeW + (thickness * 2f), thickness, thickness);
            borderBottom.localPosition = new Vector3(0f, floorY + (thickness * 0.5f), frontZ);
        }

        if (borderLeft != null)
        {
            borderLeft.localScale = new Vector3(thickness, holeH, thickness);
            borderLeft.localPosition = new Vector3(-(holeW * 0.5f) - (thickness * 0.5f), floorY + (holeH * 0.5f), frontZ);
        }

        if (borderRight != null)
        {
            borderRight.localScale = new Vector3(thickness, holeH, thickness);
            borderRight.localPosition = new Vector3((holeW * 0.5f) + (thickness * 0.5f), floorY + (holeH * 0.5f), frontZ);
        }

        if (hologramVeil != null)
        {
            hologramVeil.localScale = new Vector3(holeW, holeH, 0.04f);
            hologramVeil.localPosition = new Vector3(0f, floorY + (holeH * 0.5f), 0f);
        }

        // =========================================================================
        // TẠO CÁC VẠCH NẤC PHÂN TẦNG (TIER DIVIDER NOTCHES):
        // Giúp người chơi từ xa đếm được ngay: 1 ô, 2 ô, 3 ô hay 4 ô!
        // =========================================================================
        UpdateDividerBars(holeW, holeH, thickness, frontZ);
    }

    private void UpdateDividerBars(float holeW, float holeH, float thickness, float frontZ)
    {
        // Ẩn tất cả vạch cũ
        foreach (var bar in dividerBars)
        {
            if (bar != null) bar.gameObject.SetActive(false);
        }

        // Nếu là lỗ cao (requiredScaleY > 1.6f): Tạo vạch chia tầng ngang
        if (requiredScaleY > 1.6f)
        {
            // Số tầng tương ứng: 2, 3, 4 hoặc 5 tầng
            int tiers;
            if (requiredScaleY >= 4.4f)
                tiers = 5;
            else if (requiredScaleY >= 3.4f)
                tiers = 4;
            else if (requiredScaleY >= 2.4f)
                tiers = 3;
            else
                tiers = 2;

            int neededDividers = tiers - 1; // 2 tầng -> 1 vạch, 3 tầng -> 2 vạch, 4 tầng -> 3 vạch, 5 tầng -> 4 vạch

            while (dividerBars.Count < neededDividers)
            {
                Transform newBar = CreateBorderBar($"Divider_Bar_{dividerBars.Count}", neonBorderMaterial);
                dividerBars.Add(newBar);
            }

            float tierHeight = holeH / tiers;
            for (int i = 0; i < neededDividers; i++)
            {
                Transform bar = dividerBars[i];
                bar.gameObject.SetActive(true);
                float yPos = floorY + (tierHeight * (i + 1));
                bar.localScale = new Vector3(holeW * 0.9f, thickness * 0.7f, thickness * 0.7f);
                bar.localPosition = new Vector3(0f, yPos, frontZ);
            }
        }
        // Nếu là lỗ dẹt (requiredScaleY < 0.7f): Tạo vạch chia ô dọc dàn ngang
        else if (requiredScaleY < 0.7f)
        {
            int cols = 3; // Chia thành 3 ô ngang
            int neededDividers = cols - 1;

            while (dividerBars.Count < neededDividers)
            {
                Transform newBar = CreateBorderBar($"Divider_Bar_{dividerBars.Count}", neonBorderMaterial);
                dividerBars.Add(newBar);
            }

            float colWidth = holeW / cols;
            for (int i = 0; i < neededDividers; i++)
            {
                Transform bar = dividerBars[i];
                bar.gameObject.SetActive(true);
                float xPos = -(holeW * 0.5f) + (colWidth * (i + 1));
                bar.localScale = new Vector3(thickness * 0.7f, holeH * 0.9f, thickness * 0.7f);
                bar.localPosition = new Vector3(xPos, floorY + (holeH * 0.5f), frontZ);
            }
        }
    }

    private void SetNeonColor(Color color)
    {
        if (neonBorderMaterial != null)
        {
            neonBorderMaterial.color = color;
            if (neonBorderMaterial.HasProperty("_BaseColor"))
                neonBorderMaterial.SetColor("_BaseColor", color);
        }

        if (hologramMaterial != null)
        {
            Color transparentColor = new Color(color.r, color.g, color.b, 0.18f);
            hologramMaterial.color = transparentColor;
            if (hologramMaterial.HasProperty("_BaseColor"))
                hologramMaterial.SetColor("_BaseColor", transparentColor);
        }
    }

    public bool EvaluatePass(float playerScaleY)
    {
        if (isResolved) return isPassed;
        isResolved = true;

        bool success = Mathf.Abs(playerScaleY - requiredScaleY) <= tolerance;
        isPassed = success;

        if (success)
        {
            SetNeonColor(ColorSuccess);
            SetObstacleCollidersEnabled(false);
            if (hologramVeil != null) hologramVeil.gameObject.SetActive(false);
        }
        else
        {
            SetNeonColor(ColorFail);
        }

        return success;
    }

    public bool CheckPassSuccess(float playerScaleY)
    {
        return Mathf.Abs(playerScaleY - requiredScaleY) <= tolerance;
    }

    private void SetObstacleCollidersEnabled(bool isEnabled)
    {
        if (topBlock != null)
        {
            Collider col = topBlock.GetComponent<Collider>();
            if (col != null) col.enabled = isEnabled;
        }
        if (leftBlock != null)
        {
            Collider col = leftBlock.GetComponent<Collider>();
            if (col != null) col.enabled = isEnabled;
        }
        if (rightBlock != null)
        {
            Collider col = rightBlock.GetComponent<Collider>();
            if (col != null) col.enabled = isEnabled;
        }
        if (hologramVeil != null)
        {
            hologramVeil.gameObject.SetActive(isEnabled);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 holeCenter = transform.position + new Vector3(0f, floorY + (requiredScaleY * 0.5f), 0f);
        Gizmos.DrawWireCube(holeCenter, new Vector3(requiredScaleX, requiredScaleY, 0.3f));
    }
}

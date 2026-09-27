using UnityEngine;

/// <summary>
/// Bức tường có lỗ hổng. Chứa requiredScaleY/X để Player cần khớp hình dạng mới chui qua.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Jelly Runner/Wall")]
public class Wall : MonoBehaviour
{
    [Header("--- Hình Dạng Yêu Cầu ---")]
    [SerializeField] private float requiredScaleY = 1.0f;
    [SerializeField] private float requiredScaleX = 1.0f;
    [Range(0.05f, 0.5f)]
    [SerializeField] private float tolerance = 0.25f;

    [Header("--- Khối Ghép Tạo Tường ---")]
    [SerializeField] private Transform topBlock;
    [SerializeField] private Transform leftBlock;
    [SerializeField] private Transform rightBlock;

    [Header("--- Thông Số Tường ---")]
    [SerializeField] private float totalWallWidth = 8.0f;
    [SerializeField] private float totalWallHeight = 5.0f;
    [SerializeField] private float floorY = 0.0f;

    [Header("--- Trigger Chui Qua ---")]
    [SerializeField] private BoxCollider passTrigger;

    private bool hasEvaluated = false;

    private void Start()
    {
        Collider[] cols = GetComponentsInChildren<Collider>();
        foreach (var c in cols)
        {
            if (c.CompareTag("Obstacle"))
            {
                c.isTrigger = true;
            }
        }
    }

    public float RequiredScaleY => requiredScaleY;
    public float RequiredScaleX => requiredScaleX;

    public void ConfigureBlocks(Transform top, Transform left, Transform right, BoxCollider trigger = null)
    {
        topBlock = top;
        leftBlock = left;
        rightBlock = right;
        passTrigger = trigger;
        UpdateHoleGeometry();
    }

    public void Setup(float targetY, float baseVolume = 1.0f)
    {
        requiredScaleY = targetY;
        requiredScaleX = baseVolume / targetY;
        hasEvaluated = false;
        UpdateHoleGeometry();
    }

    /// <summary>
    /// Áp dụng Material mới cho tất cả các khối của bức tường (Wall Skin).
    /// </summary>
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

        if (topBlock != null)
        {
            float topHeight = Mathf.Max(0.05f, totalWallHeight - holeH);
            topBlock.localScale = new Vector3(holeW, topHeight, topBlock.localScale.z);
            topBlock.localPosition = new Vector3(0f, floorY + holeH + (topHeight * 0.5f), 0f);
        }

        if (leftBlock != null)
        {
            float sideWidth = Mathf.Max(0.05f, (totalWallWidth - holeW) * 0.5f);
            leftBlock.localScale = new Vector3(sideWidth, totalWallHeight, leftBlock.localScale.z);
            leftBlock.localPosition = new Vector3(-(holeW * 0.5f) - (sideWidth * 0.5f), floorY + (totalWallHeight * 0.5f), 0f);
        }

        if (rightBlock != null)
        {
            float sideWidth = Mathf.Max(0.05f, (totalWallWidth - holeW) * 0.5f);
            rightBlock.localScale = new Vector3(sideWidth, totalWallHeight, rightBlock.localScale.z);
            rightBlock.localPosition = new Vector3((holeW * 0.5f) + (sideWidth * 0.5f), floorY + (totalWallHeight * 0.5f), 0f);
        }

        if (passTrigger != null)
        {
            passTrigger.size = new Vector3(holeW, holeH, 0.4f);
            passTrigger.center = new Vector3(0f, floorY + (holeH * 0.5f), 0f);
        }
    }

    public bool CheckPassSuccess(float playerScaleY)
    {
        return Mathf.Abs(playerScaleY - requiredScaleY) <= tolerance;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 holeCenter = transform.position + new Vector3(0f, floorY + (requiredScaleY * 0.5f), 0f);
        Gizmos.DrawWireCube(holeCenter, new Vector3(requiredScaleX, requiredScaleY, 0.3f));
    }
}

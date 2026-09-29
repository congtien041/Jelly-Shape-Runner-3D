using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tự động lặp lại đường chạy vô tận: tái sử dụng các đoạn đường phía sau Player 
/// và đặt lại trước mặt.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Jelly Runner/Road Spawner")]
public class RoadSpawner : MonoBehaviour
{
    [Header("--- Cấu Hình Đường ---")]
    [Tooltip("Prefab 1 đoạn đường (Cube dẹp). Nếu để trống, tự tạo.")]
    [SerializeField] private GameObject roadSegmentPrefab;

    [Tooltip("Chiều dài mỗi đoạn đường theo Z.")]
    [SerializeField] private float segmentLength = 100f;

    [Tooltip("Material cho đường chạy. Nếu để trống, tự nạp Mat_Track từ Resources.")]
    [SerializeField] private Material roadMaterial;

    [Tooltip("Số đoạn đường luôn hiện trước mặt Player.")]
    [SerializeField] private int segmentsAhead = 5;

    [Tooltip("Số đoạn đường còn giữ phía sau Player.")]
    [SerializeField] private int segmentsBehind = 2;

    [SerializeField] private Transform playerTransform;

    private readonly List<Transform> segments = new List<Transform>();
    private float nextSpawnZ = 0f;

    public void Configure(Transform player, GameObject prefab = null)
    {
        playerTransform = player;
        if (prefab != null) roadSegmentPrefab = prefab;
    }

    private void Start()
    {
        if (playerTransform == null)
        {
            JellyPlayer jelly = FindAnyObjectByType<JellyPlayer>();
            if (jelly != null) playerTransform = jelly.transform;
        }

        int totalSegments = segmentsAhead + segmentsBehind + 1;
        nextSpawnZ = -segmentsBehind * segmentLength;

        for (int i = 0; i < totalSegments; i++)
        {
            SpawnSegment(nextSpawnZ);
            nextSpawnZ += segmentLength;
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // Nếu Player sắp hết đường trước mặt -> sinh thêm
        float targetZ = playerTransform.position.z + segmentsAhead * segmentLength;
        while (nextSpawnZ < targetZ)
        {
            SpawnSegment(nextSpawnZ);
            nextSpawnZ += segmentLength;
        }

        // Thu hồi đoạn đường quá xa phía sau
        float behindLimit = playerTransform.position.z - (segmentsBehind + 1) * segmentLength;
        for (int i = segments.Count - 1; i >= 0; i--)
        {
            if (segments[i].position.z + segmentLength < behindLimit)
            {
                // Tái sử dụng: đẩy ra trước
                segments[i].position = new Vector3(
                    segments[i].position.x,
                    segments[i].position.y,
                    nextSpawnZ + segmentLength * 0.5f
                );
                nextSpawnZ += segmentLength;

                // Đưa lên cuối list
                Transform seg = segments[i];
                segments.RemoveAt(i);
                segments.Add(seg);
            }
        }
    }

    private void SpawnSegment(float zStart)
    {
        GameObject seg;
        if (roadSegmentPrefab != null)
        {
            seg = Instantiate(roadSegmentPrefab, transform);
        }
        else
        {
            seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seg.name = "RoadSegment";
            seg.transform.SetParent(transform);

            // Gán Material chuẩn URP để không bao giờ bị màu tím trong bản build
            Renderer rend = seg.GetComponent<Renderer>();
            if (rend != null)
            {
                if (roadMaterial == null)
                {
                    roadMaterial = Resources.Load<Material>("Mat_Track");
                    if (roadMaterial == null)
                    {
                        Shader urpShader = Shader.Find("Universal Render Pipeline/Lit")
                                        ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                                        ?? Shader.Find("Standard");
                        if (urpShader != null)
                        {
                            roadMaterial = new Material(urpShader);
                            roadMaterial.color = new Color(0.12f, 0.15f, 0.22f);
                            if (roadMaterial.HasProperty("_BaseColor")) roadMaterial.SetColor("_BaseColor", new Color(0.12f, 0.15f, 0.22f));
                        }
                    }
                }

                if (roadMaterial != null)
                {
                    rend.sharedMaterial = roadMaterial;
                }
            }
        }

        seg.transform.position = new Vector3(0f, -0.5f, zStart + segmentLength * 0.5f);
        seg.transform.localScale = new Vector3(8f, 1f, segmentLength);
        segments.Add(seg.transform);
    }
}

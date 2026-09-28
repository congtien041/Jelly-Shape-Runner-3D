using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sinh tường vô tận phía trước Player với Object Pooling.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Jelly Runner/Wall Spawner")]
public class WallSpawner : MonoBehaviour
{
    [Header("--- Tham Chiếu ---")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private Transform playerTransform;

    [Header("--- Khoảng Cách ---")]
    [SerializeField] private float initialSpawnDistance = 28f;

    [Tooltip("Khoảng cách giữa các bức tường — tăng khoảng cách để người chơi thoải mái quan sát")]
    [Range(20f, 80f)]
    [SerializeField] private float wallDistance = 40f;

    [SerializeField] private int minVisibleWalls = 6;
    [SerializeField] private float despawnDistanceBehind = 15f;
    [SerializeField] private float floorY = 0f;

    // Giai đoạn đầu (< 2 phút): Dẹt ngang (0.4f), Vuông 1 con (1.0f), Cao 2 tầng (2.0f), Cao 3 tầng (3.0f), Cao 4 tầng (4.0f)
    private static readonly float[] EarlyHoleArchetypes = new float[] { 0.4f, 1.0f, 2.0f, 3.0f, 4.0f };

    // Sau khi chơi >= 2 phút (120s): Mở khóa thêm Siêu Lỗ 5 tầng (5.0f) chia 5 ô!
    private static readonly float[] LateHoleArchetypes = new float[] { 0.4f, 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

    [Header("--- Kích Thước Lỗ ---")]
    [SerializeField] private float minHoleSize = 0.4f;
    [SerializeField] private float maxHoleSize = 5.0f;

    [Header("--- Bảo Toàn Thể Tích ---")]
    [SerializeField] private float baseVolume = 1.0f;

    private readonly List<Wall> wallPool = new List<Wall>();
    private float nextSpawnZ;
    private Material currentWallMaterial;
    private float gameplayDuration = 0f;

    public float GameplayDuration => gameplayDuration;

    public void SetGameplayDuration(float duration)
    {
        gameplayDuration = Mathf.Max(0f, duration);
    }

    public float WallDistance
    {
        get => wallDistance;
        set => wallDistance = Mathf.Clamp(value, 20f, 80f);
    }

    public float MinHoleSize
    {
        get => minHoleSize;
        set => minHoleSize = Mathf.Clamp(value, 0.3f, 2.0f);
    }

    public float MaxHoleSize
    {
        get => maxHoleSize;
        set => maxHoleSize = Mathf.Clamp(value, 1.0f, 4.0f);
    }

    public void Configure(GameObject prefab, Transform targetPlayer)
    {
        wallPrefab = prefab;
        playerTransform = targetPlayer;
    }

    public void SetWallMaterial(Material mat)
    {
        currentWallMaterial = mat;
        foreach (Wall w in wallPool)
        {
            if (w != null) w.SetWallMaterial(mat);
        }
    }

    public static WallSpawner Instance { get; private set; }
    public static float CurrentGameplayDuration => Instance != null ? Instance.gameplayDuration : 0f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (ShopManager.Instance != null)
        {
            currentWallMaterial = ShopManager.Instance.GetEquippedWallMaterial();
        }

        InitializePlayer();
        InitializePool();
    }

    private void Update()
    {
        if (playerTransform == null) return;
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

        gameplayDuration += Time.deltaTime;

        RecyclePassedWalls();
        EnsureMinimumWallsAhead();
    }

    private void InitializePlayer()
    {
        if (playerTransform != null) return;

        JellyPlayer jelly = FindAnyObjectByType<JellyPlayer>();
        if (jelly != null)
            playerTransform = jelly.transform;
        else
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }
    }

    private void InitializePool()
    {
        if (wallPrefab == null) return;

        minVisibleWalls = Mathf.Max(5, minVisibleWalls);
        int poolSize = minVisibleWalls + 3;

        float playerZ = playerTransform != null ? playerTransform.position.z : 0f;
        nextSpawnZ = playerZ + initialSpawnDistance;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject wallObj = Instantiate(wallPrefab, transform);
            Wall wall = wallObj.GetComponent<Wall>() ?? wallObj.AddComponent<Wall>();
            PositionAndSetupWall(wall, nextSpawnZ);
            wallPool.Add(wall);
            nextSpawnZ += wallDistance;
        }
    }

    private void RecyclePassedWalls()
    {
        float despawnThresholdZ = playerTransform.position.z - despawnDistanceBehind;

        for (int i = 0; i < wallPool.Count; i++)
        {
            if (wallPool[i].transform.position.z < despawnThresholdZ)
            {
                wallPool[i].gameObject.SetActive(false);
                PositionAndSetupWall(wallPool[i], nextSpawnZ);
                nextSpawnZ += wallDistance;
            }
        }
    }

    private void EnsureMinimumWallsAhead()
    {
        float targetAheadZ = playerTransform.position.z + (minVisibleWalls * wallDistance);

        while (nextSpawnZ < targetAheadZ)
        {
            Wall recycled = GetFurthestWallBehindPlayer();
            if (recycled != null)
            {
                recycled.gameObject.SetActive(false);
                PositionAndSetupWall(recycled, nextSpawnZ);
            }
            else
            {
                GameObject newWallObj = Instantiate(wallPrefab, transform);
                recycled = newWallObj.GetComponent<Wall>() ?? newWallObj.AddComponent<Wall>();
                wallPool.Add(recycled);
                PositionAndSetupWall(recycled, nextSpawnZ);
            }
            nextSpawnZ += wallDistance;
        }
    }

    private Wall GetFurthestWallBehindPlayer()
    {
        Wall furthest = null;
        float lowestZ = float.MaxValue;
        foreach (Wall wall in wallPool)
        {
            float z = wall.transform.position.z;
            if (z < playerTransform.position.z && z < lowestZ)
            {
                lowestZ = z;
                furthest = wall;
            }
        }
        return furthest;
    }

    private void PositionAndSetupWall(Wall wall, float zPos)
    {
        wall.transform.position = new Vector3(0f, floorY, zPos);

        // Chỉ khi chơi được từ 2 phút (120 giây) trở lên mới mở khóa lỗ 5 tầng (5.0f)!
        float[] currentPool = (gameplayDuration >= 120f) ? LateHoleArchetypes : EarlyHoleArchetypes;
        float targetY = currentPool[Random.Range(0, currentPool.Length)];

        wall.Setup(targetY, baseVolume);
        if (currentWallMaterial != null)
        {
            wall.SetWallMaterial(currentWallMaterial);
        }
        wall.gameObject.SetActive(true);
    }

    public void ResetSpawner()
    {
        gameplayDuration = 0f;
        foreach (Wall wall in wallPool)
            if (wall != null) Destroy(wall.gameObject);
        wallPool.Clear();
        InitializePool();
    }
}

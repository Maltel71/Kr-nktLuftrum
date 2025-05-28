using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    private static BulletPool instance;
    public static BulletPool Instance => instance;

    [Header("Prefabs")]
    [SerializeField] private GameObject playerBulletPrefab;
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private int poolSizePerType = 30;

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLogs = false;

    private List<GameObject> playerBulletPool;
    private List<GameObject> enemyBulletPool;

    // Statistik för debugging
    private int totalBulletsCreated = 0;
    private int totalBulletsReused = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            ValidateSetup();
            InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void ValidateSetup()
    {
        if (playerBulletPrefab == null)
            Debug.LogWarning("Player bullet prefab not assigned!");
        if (enemyBulletPrefab == null)
            Debug.LogWarning("Enemy bullet prefab not assigned!");
    }

    private void InitializePools()
    {
        DebugLog("Initializing bullet pools...");

        playerBulletPool = new List<GameObject>();
        enemyBulletPool = new List<GameObject>();

        for (int i = 0; i < poolSizePerType; i++)
        {
            CreateBullet(true);
            CreateBullet(false);
        }

        DebugLog($"Pool initialization complete. Created {poolSizePerType * 2} bullets.");
    }

    private void CreateBullet(bool isPlayerBullet)
    {
        GameObject bullet = Instantiate(
            isPlayerBullet ? playerBulletPrefab : enemyBulletPrefab,
            transform
        );
        bullet.SetActive(false);
        totalBulletsCreated++;

        if (isPlayerBullet)
            playerBulletPool.Add(bullet);
        else
            enemyBulletPool.Add(bullet);
    }

    public GameObject GetBullet(bool isPlayerBullet)
    {
        List<GameObject> pool = isPlayerBullet ? playerBulletPool : enemyBulletPool;

        // Hitta första inaktiva skottet
        foreach (GameObject bullet in pool)
        {
            if (!bullet.activeInHierarchy)
            {
                bullet.SetActive(true);
                totalBulletsReused++;
                //DebugLog($"Reusing {(isPlayerBullet ? "player" : "enemy")} bullet. Total reused: {totalBulletsReused}");
                return bullet;
            }
        }

        // Om alla skott är aktiva, skapa ett nytt
        //DebugLog($"Pool exhausted for {(isPlayerBullet ? "player" : "enemy")} bullets, creating new bullet");
        GameObject newBullet = Instantiate(
            isPlayerBullet ? playerBulletPrefab : enemyBulletPrefab,
            transform
        );
        totalBulletsCreated++;
        pool.Add(newBullet);
        return newBullet;
    }

    public void ReturnBulletToPool(GameObject bullet)
    {
        bullet.SetActive(false);
        bullet.transform.position = transform.position;
        //DebugLog("Bullet returned to pool");
    }

    private void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            //Debug.Log($"[BulletPool] {message}");
        }
    }

    public void PrintPoolStatistics()
    {
        Debug.Log($"=== BulletPool Statistics ===\n" +
                  $"Total Bullets Created: {totalBulletsCreated}\n" +
                  $"Total Bullets Reused: {totalBulletsReused}\n" +
                  $"Player Pool Size: {playerBulletPool?.Count ?? 0}\n" +
                  $"Enemy Pool Size: {enemyBulletPool?.Count ?? 0}");
    }
}

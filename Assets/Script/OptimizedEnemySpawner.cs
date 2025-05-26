using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OptimizedEnemySpawner : MonoBehaviour
{
    [Header("Enemy Setup")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawning Settings")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private int totalEnemiesToSpawn = 10;
    [SerializeField] private float spawnInterval = 0.5f; // Tid mellan spawns
    [SerializeField] private int maxEnemiesPerFrame = 2; // Max fiender per frame

    [Header("Performance Settings")]
    [SerializeField] private float maxActiveEnemies = 8f; // Max aktiva samtidigt
    [SerializeField] private float activationDistance = 150f; // Avståndet för aktivering
    [SerializeField] private float deactivationDistance = 300f; // Avståndet för inaktivering

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private Transform playerTransform;
    private int currentSpawnIndex = 0;
    private bool isSpawning = false;

    private void Start()
    {
        // Hitta spelaren
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (playerTransform == null)
        {
            Debug.LogWarning("Kunde inte hitta spelaren!");
        }

        if (spawnOnStart)
        {
            StartSpawning();
        }
    }

    private void Update()
    {
        if (playerTransform != null && spawnedEnemies.Count > 0)
        {
            ManageEnemyActivation();
        }
    }

    public void StartSpawning()
    {
        if (isSpawning) return;

        DebugLog("Starting enemy spawning...");
        StartCoroutine(SpawnEnemiesGradually());
    }

    private IEnumerator SpawnEnemiesGradually()
    {
        isSpawning = true;
        int enemiesSpawned = 0;
        int enemiesThisFrame = 0;

        while (enemiesSpawned < totalEnemiesToSpawn)
        {
            // Spawna bara ett visst antal per frame
            if (enemiesThisFrame >= maxEnemiesPerFrame)
            {
                enemiesThisFrame = 0;
                yield return null; // Vänta till nästa frame
            }

            // Spawna fiende
            GameObject enemy = SpawnSingleEnemy();
            if (enemy != null)
            {
                spawnedEnemies.Add(enemy);
                enemiesSpawned++;
                enemiesThisFrame++;

                DebugLog($"Spawned enemy {enemiesSpawned}/{totalEnemiesToSpawn}");
            }

            // Vänta mellan spawns (optional)
            if (spawnInterval > 0 && enemiesSpawned < totalEnemiesToSpawn)
            {
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        isSpawning = false;
        DebugLog($"Spawning complete! Total enemies: {spawnedEnemies.Count}");
    }

    private GameObject SpawnSingleEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("Inga enemy prefabs definierade!");
            return null;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Inga spawn points definierade!");
            return null;
        }

        // Välj prefab och spawn point
        int enemyIndex = currentSpawnIndex % enemyPrefabs.Length;
        int spawnIndex = currentSpawnIndex % spawnPoints.Length;

        GameObject enemyPrefab = enemyPrefabs[enemyIndex];
        Transform spawnPoint = spawnPoints[spawnIndex];

        // Spawna fienden
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        // Ge den ett unikt namn
        enemy.name = $"{enemyPrefab.name}_{currentSpawnIndex}";

        // Starta som inaktiv för prestanda (optional)
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(enemy.transform.position, playerTransform.position);
            if (distance > activationDistance)
            {
                enemy.SetActive(false);
                DebugLog($"Enemy {enemy.name} spawned but deactivated (distance: {distance:F1})");
            }
        }

        // Konfigurera movement om det finns
        var movement = enemy.GetComponent<RandomEnemyMovement>();
        if (movement != null)
        {
            movement.ForcePosition(spawnPoint.position);
        }

        currentSpawnIndex++;
        return enemy;
    }

    private void ManageEnemyActivation()
    {
        int activeCount = 0;

        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy == null) continue;

            float distance = Vector3.Distance(enemy.transform.position, playerTransform.position);
            bool shouldBeActive = distance <= activationDistance && activeCount < maxActiveEnemies;

            if (shouldBeActive && !enemy.activeInHierarchy)
            {
                enemy.SetActive(true);
                DebugLog($"Activated {enemy.name} (distance: {distance:F1})");
            }
            else if (!shouldBeActive && enemy.activeInHierarchy && distance > deactivationDistance)
            {
                enemy.SetActive(false);
                DebugLog($"Deactivated {enemy.name} (distance: {distance:F1})");
            }

            if (enemy.activeInHierarchy)
            {
                activeCount++;
            }
        }
    }

    // Publika metoder för kontroll
    public void SpawnEnemyAtPoint(int enemyIndex, int spawnPointIndex)
    {
        if (enemyPrefabs == null || enemyIndex >= enemyPrefabs.Length)
        {
            Debug.LogError($"Invalid enemy index: {enemyIndex}");
            return;
        }

        if (spawnPoints == null || spawnPointIndex >= spawnPoints.Length)
        {
            Debug.LogError($"Invalid spawn point index: {spawnPointIndex}");
            return;
        }

        GameObject enemyPrefab = enemyPrefabs[enemyIndex];
        Transform spawnPoint = spawnPoints[spawnPointIndex];

        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        enemy.name = $"{enemyPrefab.name}_Manual_{spawnedEnemies.Count}";

        spawnedEnemies.Add(enemy);

        DebugLog($"Manually spawned {enemy.name} at {spawnPoint.position}");
    }

    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        spawnedEnemies.Clear();
        currentSpawnIndex = 0;
        DebugLog("All enemies cleared");
    }

    public void StopSpawning()
    {
        if (isSpawning)
        {
            StopAllCoroutines();
            isSpawning = false;
            DebugLog("Spawning stopped");
        }
    }

    // Debug helper
    private void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[OptimizedEnemySpawner] {message}");
        }
    }

    // Getters för statistik
    public int GetActiveEnemyCount()
    {
        int count = 0;
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null && enemy.activeInHierarchy)
            {
                count++;
            }
        }
        return count;
    }

    public int GetTotalEnemyCount()
    {
        return spawnedEnemies.Count;
    }

    public bool IsSpawning()
    {
        return isSpawning;
    }

    // För debugging i Editor
    private void OnDrawGizmosSelected()
    {
        if (playerTransform == null) return;

        // Rita aktiverings-radie
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(playerTransform.position, activationDistance);

        // Rita inaktiverings-radie  
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerTransform.position, deactivationDistance);

        // Rita spawn points
        if (spawnPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform spawn in spawnPoints)
            {
                if (spawn != null)
                {
                    Gizmos.DrawWireSphere(spawn.position, 2f);
                }
            }
        }
    }
}
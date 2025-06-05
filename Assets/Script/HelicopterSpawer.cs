using UnityEngine;
using System.Collections;

public class HelicopterSpawner : MonoBehaviour
{
    [Header("Helicopter Settings")]
    [SerializeField] private GameObject helicopterPrefab;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawning Settings")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private int maxHelicopters = 3;
    [SerializeField] private float spawnInterval = 8f;
    [SerializeField] private float firstSpawnDelay = 2f;

    [Header("Player Detection")]
    [SerializeField] private bool spawnBasedOnPlayerPosition = true;
    [SerializeField] private float playerDetectionRange = 50f;

    private Transform playerTransform;
    private int currentHelicopterCount = 0;
    private bool isSpawning = false;

    private void Start()
    {
        // Hitta spelaren
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (playerTransform == null)
        {
            Debug.LogWarning("Kunde inte hitta spelaren för HelicopterSpawner!");
            return;
        }

        // Kontrollera att vi har spawn points
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Inga spawn points definierade för HelicopterSpawner!");
            return;
        }

        if (spawnOnStart)
        {
            StartCoroutine(StartSpawning());
        }
    }

    private void Update()
    {
        // Räkna aktiva helikoptrar
        UpdateHelicopterCount();

        // Spawna fler om det behövs
        if (spawnBasedOnPlayerPosition && !isSpawning && currentHelicopterCount < maxHelicopters)
        {
            float distanceToPlayer = GetDistanceToNearestSpawnPoint();
            if (distanceToPlayer <= playerDetectionRange)
            {
                StartCoroutine(SpawnHelicopterDelayed());
            }
        }
    }

    private IEnumerator StartSpawning()
    {
        yield return new WaitForSeconds(firstSpawnDelay);

        while (currentHelicopterCount < maxHelicopters)
        {
            if (helicopterPrefab != null)
            {
                SpawnHelicopter();
                yield return new WaitForSeconds(spawnInterval);
            }
            else
            {
                Debug.LogError("Helicopter prefab saknas!");
                break;
            }
        }
    }

    private IEnumerator SpawnHelicopterDelayed()
    {
        isSpawning = true;
        yield return new WaitForSeconds(2f); // Kort fördröjning

        if (currentHelicopterCount < maxHelicopters)
        {
            SpawnHelicopter();
        }

        yield return new WaitForSeconds(spawnInterval);
        isSpawning = false;
    }

    private void SpawnHelicopter()
    {
        if (spawnPoints.Length == 0 || helicopterPrefab == null) return;

        // Välj en slumpmässig spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Skapa helikoptern
        GameObject helicopter = Instantiate(helicopterPrefab, spawnPoint.position, spawnPoint.rotation);

        // Lägg till EnemyHealth om det inte finns
        if (helicopter.GetComponent<EnemyHealth>() == null)
        {
            helicopter.AddComponent<EnemyHealth>();
        }

        // Sätt aggressivt läge för öken-helikoptrar
        if (helicopter.TryGetComponent<HelicopterEnemy>(out var helicopterScript))
        {
            helicopterScript.SetCombatMode(true); // Aggressiva öken-helikoptrar
        }

        currentHelicopterCount++;
        Debug.Log($"Spawnade helikopter #{currentHelicopterCount} vid {spawnPoint.position}");
    }

    private void UpdateHelicopterCount()
    {
        // Räkna alla aktiva helikoptrar med HelicopterEnemy script
        HelicopterEnemy[] helicopters = FindObjectsOfType<HelicopterEnemy>();
        currentHelicopterCount = helicopters.Length;
    }

    private float GetDistanceToNearestSpawnPoint()
    {
        if (playerTransform == null || spawnPoints.Length == 0) return float.MaxValue;

        float nearestDistance = float.MaxValue;

        foreach (Transform spawnPoint in spawnPoints)
        {
            float distance = Vector3.Distance(playerTransform.position, spawnPoint.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
            }
        }

        return nearestDistance;
    }

    // Publika metoder för kontroll
    public void SpawnImmediately()
    {
        if (currentHelicopterCount < maxHelicopters)
        {
            SpawnHelicopter();
        }
    }

    public void StopSpawning()
    {
        StopAllCoroutines();
        isSpawning = false;
    }

    public void SetMaxHelicopters(int max)
    {
        maxHelicopters = max;
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        if (spawnPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 3f);
                    Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + spawnPoint.forward * 5f);
                }
            }
        }

        if (playerTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, playerDetectionRange);
        }
    }
}
using UnityEngine;
using System.Collections.Generic;

public class ExplosionPool : MonoBehaviour
{
    private static ExplosionPool instance;
    public static ExplosionPool Instance => instance;

    [Header("Explosion Prefabs")]
    [SerializeField] private GameObject smallExplosionPrefab;
    [SerializeField] private GameObject largeExplosionPrefab;
    [SerializeField] private GameObject bossExplosionPrefab;

    [Header("Pool Settings")]
    [SerializeField] private bool usePooling = true;
    [SerializeField] private int smallExplosionPoolSize = 10;
    [SerializeField] private int largeExplosionPoolSize = 5;
    [SerializeField] private int bossExplosionPoolSize = 2;

    [Header("Prewarming")]
    [SerializeField] private bool prewarmOnStart = true;

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLogs = false;

    private List<GameObject> smallExplosionPool;
    private List<GameObject> largeExplosionPool;
    private List<GameObject> bossExplosionPool;

    private int totalExplosionsCreated = 0;
    private int totalExplosionsReused = 0;
    private int missedPoolAttempts = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            ValidateSetup();
            if (usePooling)
            {
                InitializePools();
            }
        }
        else
        {
            Debug.LogWarning("Multiple ExplosionPool instances detected! Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (prewarmOnStart)
        {
            // V�nta ett frame f�r att l�ta andra system initialisera
            StartCoroutine(PrewarmWithDelay());
        }
    }

    private System.Collections.IEnumerator PrewarmWithDelay()
    {
        yield return new WaitForEndOfFrame();
        PrewarmExplosions();
    }

    private void ValidateSetup()
    {
        if (smallExplosionPrefab == null)
            Debug.LogWarning("Small explosion prefab not assigned!");
        if (largeExplosionPrefab == null)
            Debug.LogWarning("Large explosion prefab not assigned!");
        if (bossExplosionPrefab == null)
            Debug.LogWarning("Boss explosion prefab not assigned!");

        if (!usePooling)
            Debug.Log("ExplosionPool is running in non-pooling mode. Performance might be affected.");
    }

    private void InitializePools()
    {
        DebugLog("Initializing explosion pools...");

        smallExplosionPool = new List<GameObject>();
        largeExplosionPool = new List<GameObject>();
        bossExplosionPool = new List<GameObject>();

        int successfulInits = 0;

        for (int i = 0; i < smallExplosionPoolSize; i++)
        {
            if (CreateExplosion(ExplosionType.Small))
                successfulInits++;
        }

        for (int i = 0; i < largeExplosionPoolSize; i++)
        {
            if (CreateExplosion(ExplosionType.Large))
                successfulInits++;
        }

        for (int i = 0; i < bossExplosionPoolSize; i++)
        {
            if (CreateExplosion(ExplosionType.Boss))
                successfulInits++;
        }

        DebugLog($"Pool initialization complete. Created {successfulInits} explosions.");
    }

    private bool CreateExplosion(ExplosionType type)
    {
        GameObject prefab = GetPrefabForType(type);
        if (prefab == null)
        {
            Debug.LogWarning($"Failed to create explosion of type {type} - prefab missing");
            return false;
        }

        GameObject explosion = Instantiate(prefab, transform);

        // Inaktivera och �terst�ll alla partikelsystem
        var particleSystems = explosion.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in particleSystems)
        {
            var main = ps.main;
            main.playOnAwake = false;
            main.prewarm = true;  // Aktivera prewarm
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Clear();
        }

        explosion.SetActive(false);
        totalExplosionsCreated++;

        switch (type)
        {
            case ExplosionType.Small:
                smallExplosionPool.Add(explosion);
                break;
            case ExplosionType.Large:
                largeExplosionPool.Add(explosion);
                break;
            case ExplosionType.Boss:
                bossExplosionPool.Add(explosion);
                break;
        }
        return true;
    }

    private void PrewarmExplosions()
    {
        if (!usePooling) return;

        DebugLog("Starting explosion prewarming...");

        // Aktivera och inaktivera en av varje typ f�r att ladda in shaders och texturer
        PrewarmExplosionType(ExplosionType.Small);
        PrewarmExplosionType(ExplosionType.Large);
        PrewarmExplosionType(ExplosionType.Boss);

        DebugLog("Explosion prewarming complete.");
    }

    private void PrewarmExplosionType(ExplosionType type)
    {
        GameObject explosion = GetExplosion(type);
        if (explosion != null)
        {
            // S�tt en kort livstid f�r att testa alla systems
            var particles = explosion.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particles)
            {
                ps.Play();
                ps.Stop();
            }

            // Returnera till pool snabbt
            StartCoroutine(DelayedReturn(explosion, 0.1f));
        }
    }

    private System.Collections.IEnumerator DelayedReturn(GameObject explosion, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (explosion != null)
        {
            explosion.SetActive(false);
            explosion.transform.position = transform.position;
        }
    }

    public GameObject GetExplosion(ExplosionType type)
    {
        GameObject prefab = GetPrefabForType(type);
        if (prefab == null)
        {
            Debug.LogWarning($"No explosion prefab assigned for type: {type}");
            missedPoolAttempts++;
            return null;
        }

        if (!usePooling)
        {
            DebugLog($"Creating new non-pooled explosion of type: {type}");
            GameObject explosion = Instantiate(prefab);

            // Starta alla partikelsystem explicit
            var particles = explosion.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particles)
            {
                ps.Clear();
                ps.Play();
            }

            totalExplosionsCreated++;
            Destroy(explosion, 2f);
            return explosion;
        }

        List<GameObject> pool = GetPoolForType(type);

        foreach (GameObject explosion in pool)
        {
            if (!explosion.activeInHierarchy)
            {
                explosion.SetActive(true);

                // Starta alla partikelsystem explicit
                var particles = explosion.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in particles)
                {
                    ps.Clear();
                    ps.Play();
                }

                totalExplosionsReused++;
                DebugLog($"Reusing explosion of type: {type}. Total reused: {totalExplosionsReused}");
                return explosion;
            }
        }

        DebugLog($"Pool exhausted for type: {type}, creating new explosion");
        CreateExplosion(type);
        GameObject newExplosion = GetExplosion(type);
        return newExplosion;
    }

    private GameObject GetPrefabForType(ExplosionType type)
    {
        return type switch
        {
            ExplosionType.Small => smallExplosionPrefab,
            ExplosionType.Large => largeExplosionPrefab,
            ExplosionType.Boss => bossExplosionPrefab,
            _ => null
        };
    }

    private List<GameObject> GetPoolForType(ExplosionType type)
    {
        return type switch
        {
            ExplosionType.Small => smallExplosionPool,
            ExplosionType.Large => largeExplosionPool,
            ExplosionType.Boss => bossExplosionPool,
            _ => smallExplosionPool
        };
    }

    public void ReturnExplosionToPool(GameObject explosion, float delay = 2f)
    {
        if (!usePooling)
        {
            Destroy(explosion, delay);
            return;
        }

        StartCoroutine(ReturnToPoolAfterDelay(explosion, delay));
    }

    private System.Collections.IEnumerator ReturnToPoolAfterDelay(GameObject explosion, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (explosion != null)
        {
            // Stoppa alla partikelsystem
            var particles = explosion.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particles)
            {
                ps.Stop();
                ps.Clear();
            }

            explosion.SetActive(false);
            explosion.transform.position = transform.position;
            DebugLog("Explosion returned to pool");
        }
    }

    private void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[ExplosionPool] {message}");
        }
    }

    public void PrintPoolStatistics()
    {
        Debug.Log($"=== ExplosionPool Statistics ===\n" +
                  $"Total Explosions Created: {totalExplosionsCreated}\n" +
                  $"Total Explosions Reused: {totalExplosionsReused}\n" +
                  $"Missed Pool Attempts: {missedPoolAttempts}\n" +
                  $"Small Pool Size: {smallExplosionPool?.Count ?? 0}\n" +
                  $"Large Pool Size: {largeExplosionPool?.Count ?? 0}\n" +
                  $"Boss Pool Size: {bossExplosionPool?.Count ?? 0}");
    }
}

public enum ExplosionType
{
    Small,  // F�r mindre fiender och projektiler
    Large,  // F�r st�rre fiender och bomber
    Boss    // F�r bossar
}

//using UnityEngine;
//using System.Collections.Generic;

//public class ExplosionPool : MonoBehaviour
//{
//    private static ExplosionPool instance;
//    public static ExplosionPool Instance => instance;

//    [Header("Explosion Prefabs")]
//    [SerializeField] private GameObject smallExplosionPrefab;
//    [SerializeField] private GameObject largeExplosionPrefab;
//    [SerializeField] private GameObject bossExplosionPrefab;

//    [Header("Pool Settings")]
//    [SerializeField] private bool usePooling = true;
//    [SerializeField] private int smallExplosionPoolSize = 10;
//    [SerializeField] private int largeExplosionPoolSize = 5;
//    [SerializeField] private int bossExplosionPoolSize = 2;

//    [Header("Debug Settings")]
//    [SerializeField] private bool showDebugLogs = false;

//    private List<GameObject> smallExplosionPool;
//    private List<GameObject> largeExplosionPool;
//    private List<GameObject> bossExplosionPool;

//    private int totalExplosionsCreated = 0;
//    private int totalExplosionsReused = 0;
//    private int missedPoolAttempts = 0;

//    private void Awake()
//    {
//        if (instance == null)
//        {
//            instance = this;
//            ValidateSetup();
//            if (usePooling)
//            {
//                InitializePools();
//            }
//        }
//        else
//        {
//            Debug.LogWarning("Multiple ExplosionPool instances detected! Destroying duplicate.");
//            Destroy(gameObject);
//        }
//    }

//    private void ValidateSetup()
//    {
//        if (smallExplosionPrefab == null)
//            Debug.LogWarning("Small explosion prefab not assigned!");
//        if (largeExplosionPrefab == null)
//            Debug.LogWarning("Large explosion prefab not assigned!");
//        if (bossExplosionPrefab == null)
//            Debug.LogWarning("Boss explosion prefab not assigned!");

//        if (!usePooling)
//            Debug.Log("ExplosionPool is running in non-pooling mode. Performance might be affected.");
//    }

//    private void InitializePools()
//    {
//        DebugLog("Initializing explosion pools...");

//        smallExplosionPool = new List<GameObject>();
//        largeExplosionPool = new List<GameObject>();
//        bossExplosionPool = new List<GameObject>();

//        int successfulInits = 0;

//        for (int i = 0; i < smallExplosionPoolSize; i++)
//        {
//            if (CreateExplosion(ExplosionType.Small))
//                successfulInits++;
//        }

//        for (int i = 0; i < largeExplosionPoolSize; i++)
//        {
//            if (CreateExplosion(ExplosionType.Large))
//                successfulInits++;
//        }

//        for (int i = 0; i < bossExplosionPoolSize; i++)
//        {
//            if (CreateExplosion(ExplosionType.Boss))
//                successfulInits++;
//        }

//        DebugLog($"Pool initialization complete. Created {successfulInits} explosions.");
//    }

//    private bool CreateExplosion(ExplosionType type)
//    {
//        GameObject prefab = GetPrefabForType(type);
//        if (prefab == null)
//        {
//            Debug.LogWarning($"Failed to create explosion of type {type} - prefab missing");
//            return false;
//        }

//        GameObject explosion = Instantiate(prefab, transform);

//        // Inaktivera och �terst�ll alla partikelsystem
//        var particleSystems = explosion.GetComponentsInChildren<ParticleSystem>(true);
//        foreach (var ps in particleSystems)
//        {
//            var main = ps.main;
//            main.playOnAwake = false;
//            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
//            ps.Clear();
//        }

//        explosion.SetActive(false);
//        totalExplosionsCreated++;

//        switch (type)
//        {
//            case ExplosionType.Small:
//                smallExplosionPool.Add(explosion);
//                break;
//            case ExplosionType.Large:
//                largeExplosionPool.Add(explosion);
//                break;
//            case ExplosionType.Boss:
//                bossExplosionPool.Add(explosion);
//                break;
//        }
//        return true;
//    }

//    private GameObject GetPrefabForType(ExplosionType type)
//    {
//        return type switch
//        {
//            ExplosionType.Small => smallExplosionPrefab,
//            ExplosionType.Large => largeExplosionPrefab,
//            ExplosionType.Boss => bossExplosionPrefab,
//            _ => null
//        };
//    }

//    public GameObject GetExplosion(ExplosionType type)
//    {
//        GameObject prefab = GetPrefabForType(type);
//        if (prefab == null)
//        {
//            Debug.LogWarning($"No explosion prefab assigned for type: {type}");
//            missedPoolAttempts++;
//            return null;
//        }

//        if (!usePooling)
//        {
//            DebugLog($"Creating new non-pooled explosion of type: {type}");
//            GameObject explosion = Instantiate(prefab);

//            // Starta alla partikelsystem explicit
//            var particles = explosion.GetComponentsInChildren<ParticleSystem>();
//            foreach (var ps in particles)
//            {
//                ps.Clear();
//                ps.Play();
//            }

//            totalExplosionsCreated++;
//            Destroy(explosion, 2f);
//            return explosion;
//        }

//        List<GameObject> pool = GetPoolForType(type);

//        foreach (GameObject explosion in pool)
//        {
//            if (!explosion.activeInHierarchy)
//            {
//                explosion.SetActive(true);

//                // Starta alla partikelsystem explicit
//                var particles = explosion.GetComponentsInChildren<ParticleSystem>();
//                foreach (var ps in particles)
//                {
//                    ps.Clear();
//                    ps.Play();
//                }

//                totalExplosionsReused++;
//                DebugLog($"Reusing explosion of type: {type}. Total reused: {totalExplosionsReused}");
//                return explosion;
//            }
//        }

//        DebugLog($"Pool exhausted for type: {type}, creating new explosion");
//        GameObject newExplosion = Instantiate(prefab, transform);

//        // Starta alla partikelsystem explicit
//        var newParticles = newExplosion.GetComponentsInChildren<ParticleSystem>();
//        foreach (var ps in newParticles)
//        {
//            ps.Clear();
//            ps.Play();
//        }

//        totalExplosionsCreated++;
//        pool.Add(newExplosion);
//        return newExplosion;
//    }

//    private List<GameObject> GetPoolForType(ExplosionType type)
//    {
//        return type switch
//        {
//            ExplosionType.Small => smallExplosionPool,
//            ExplosionType.Large => largeExplosionPool,
//            ExplosionType.Boss => bossExplosionPool,
//            _ => smallExplosionPool
//        };
//    }

//    public void ReturnExplosionToPool(GameObject explosion, float delay = 2f)
//    {
//        if (!usePooling)
//        {
//            Destroy(explosion, delay);
//            return;
//        }

//        StartCoroutine(ReturnToPoolAfterDelay(explosion, delay));
//    }

//    private System.Collections.IEnumerator ReturnToPoolAfterDelay(GameObject explosion, float delay)
//    {
//        yield return new WaitForSeconds(delay);
//        if (explosion != null)
//        {
//            // Stoppa alla partikelsystem
//            var particles = explosion.GetComponentsInChildren<ParticleSystem>();
//            foreach (var ps in particles)
//            {
//                ps.Stop();
//                ps.Clear();
//            }

//            explosion.SetActive(false);
//            explosion.transform.position = transform.position;
//            DebugLog("Explosion returned to pool");
//        }
//    }

//    private void DebugLog(string message)
//    {
//        if (showDebugLogs)
//        {
//            Debug.Log($"[ExplosionPool] {message}");
//        }
//    }

//    public void PrintPoolStatistics()
//    {
//        Debug.Log($"=== ExplosionPool Statistics ===\n" +
//                  $"Total Explosions Created: {totalExplosionsCreated}\n" +
//                  $"Total Explosions Reused: {totalExplosionsReused}\n" +
//                  $"Missed Pool Attempts: {missedPoolAttempts}\n" +
//                  $"Small Pool Size: {smallExplosionPool?.Count ?? 0}\n" +
//                  $"Large Pool Size: {largeExplosionPool?.Count ?? 0}\n" +
//                  $"Boss Pool Size: {bossExplosionPool?.Count ?? 0}");
//    }
//}

//public enum ExplosionType
//{
//    Small,  // F�r mindre fiender och projektiler
//    Large,  // F�r st�rre fiender och bomber
//    Boss    // F�r bossar
//}
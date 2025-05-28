using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Boss Settings")]
    [SerializeField] private bool isBoss = false;

    [Header("UI")]
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 2f, 0);
    [SerializeField] private Vector3 healthBarScale = new Vector3(0.05f, 0.05f, 0.05f);

    [Header("Crash Settings")]
    [SerializeField] private float crashSpeed = 200f;
    [SerializeField] private float crashDuration = 5f;
    [SerializeField] private float rotationSpeed = 3200f;
    [SerializeField] private bool rotateClockwise = true;
    [SerializeField] private float groundLevel = 0f;

    [Header("Explosion Settings")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionScale = 1f;
    [SerializeField] private float randomExplosionDelayMin = 1f;
    [SerializeField] private float randomExplosionDelayMax = 3f;

    [Header("Smoke Effects")]
    [SerializeField] private ParticleSystem engineExhaustSmoke;
    [SerializeField] private float smokeHealthThreshold = 0.2f;
    [SerializeField] private ParticleSystem damageSmokeEffect;
    [SerializeField] private float damageSmokefadeDuration = 2f;

    [Header("Boost Drops")]
    [SerializeField] private BoostDropSystem boostDropSystem;

    [Header("Colliders")]
    [SerializeField] public Collider mainCollider;
    [SerializeField] private Collider damageCollider;

    // Cached Components
    private Slider healthSlider;
    private GameObject healthBarInstance;
    private CameraShake cameraShake;
    private ScoreManager scoreManager;
    private ExplosionPool explosionPool;
    private AudioManager audioManager;
    private Transform cachedTransform;
    private Vector3 cachedPosition;
    private Quaternion cachedRotation;

    // State Flags
    private bool initialized = false;
    private bool isDying = false;
    private bool smokeStarted = false;
    private bool hasExploded = false;
    private float crashStartTime;

    // Static collision cache
    private static List<Collider> cachedEnemyColliders = null;
    private static float lastEnemyColliderUpdate = 0f;
    private static float enemyColliderCacheTime = 1f;

    public bool IsDying => isDying;

    private void Awake()
    {
        isDying = false;
        hasExploded = false;
        initialized = false;
        cachedTransform = transform;
    }

    private void Start()
    {
        currentHealth = maxHealth;

        CreateHealthBar();

        // Cache components
        cameraShake = CameraShake.Instance;
        scoreManager = ScoreManager.Instance;
        explosionPool = ExplosionPool.Instance;
        audioManager = AudioManager.Instance;

        if (boostDropSystem == null)
        {
            boostDropSystem = FindObjectOfType<BoostDropSystem>();
        }

        if (engineExhaustSmoke != null)
        {
            engineExhaustSmoke.Play();
        }

        if (damageSmokeEffect != null)
        {
            damageSmokeEffect.Stop();
        }

        if (mainCollider == null)
            mainCollider = GetComponent<Collider>();

        if (damageCollider == null)
        {
            var triggerObj = new GameObject("DamageCollider");
            triggerObj.transform.parent = cachedTransform;
            triggerObj.transform.localPosition = Vector3.zero;
            damageCollider = triggerObj.AddComponent<BoxCollider>();
            damageCollider.isTrigger = true;
        }

        IgnoreEnemyCollisions();
        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;

        if (healthBarInstance != null)
        {
            healthBarInstance.transform.rotation = Camera.main.transform.rotation;
        }

        if (!isDying && !smokeStarted && GetHealthPercentage() <= smokeHealthThreshold)
        {
            StartDamageSmokeEffect();
        }

        if (isDying && !hasExploded)
        {
            HandleCrashing();
        }
    }

    private void CreateHealthBar()
    {
        if (healthBarPrefab == null) return;

        healthBarInstance = Instantiate(healthBarPrefab, cachedTransform.position + healthBarOffset, Quaternion.identity, cachedTransform);
        healthSlider = healthBarInstance.GetComponentInChildren<Slider>();

        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        Canvas canvas = healthBarInstance.GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 10;
        }

        healthBarInstance.transform.localScale = healthBarScale;
    }

    private void IgnoreEnemyCollisions()
    {
        Collider[] myColliders = GetComponentsInChildren<Collider>();

        if (cachedEnemyColliders == null || Time.time - lastEnemyColliderUpdate > enemyColliderCacheTime)
        {
            UpdateEnemyColliderCache();
        }

        foreach (Collider myCol in myColliders)
        {
            foreach (Collider enemyCol in cachedEnemyColliders)
            {
                if (myCol != enemyCol && myCol.gameObject != enemyCol.gameObject)
                {
                    Physics.IgnoreCollision(myCol, enemyCol);
                }
            }
        }

        for (int i = 0; i < myColliders.Length; i++)
        {
            for (int j = i + 1; j < myColliders.Length; j++)
            {
                Physics.IgnoreCollision(myColliders[i], myColliders[j]);
            }
        }
    }

    private static void UpdateEnemyColliderCache()
    {
        cachedEnemyColliders = new List<Collider>();
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            Collider[] colliders = enemy.GetComponentsInChildren<Collider>();
            cachedEnemyColliders.AddRange(colliders);
        }

        lastEnemyColliderUpdate = Time.time;
    }

    private void StartDamageSmokeEffect()
    {
        if (damageSmokeEffect != null && !smokeStarted)
        {
            damageSmokeEffect.Play();
            smokeStarted = true;
        }
    }

    public void StartSmokeEffects()
    {
        StartDamageSmokeEffect();
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    private void HandleCrashing()
    {
        if (!isDying || hasExploded) return;

        cachedPosition = cachedTransform.position;
        cachedRotation = cachedTransform.rotation;

        float speedMultiplier = isBoss ? 0.5f : 1f;
        cachedPosition.y -= crashSpeed * speedMultiplier * Time.deltaTime;
        cachedTransform.position = cachedPosition;

        float rotationAmount = rotateClockwise ? rotationSpeed : -rotationSpeed;
        cachedTransform.Rotate(0, 0, rotationAmount * Time.deltaTime);

        bool reachedGround = cachedTransform.position.y <= groundLevel;
        bool timeIsUp = Time.time >= crashStartTime + crashDuration;

        if ((reachedGround || timeIsUp) && !hasExploded)
        {
            hasExploded = true;
            CreateExplosionEffect();
            Destroy(gameObject, 0.2f);
        }
    }

    private void CreateExplosionEffect()
    {
        if (explosionPool != null)
        {
            GameObject mainExplosion = explosionPool.GetExplosion(
                isBoss ? ExplosionType.Boss : ExplosionType.Large
            );

            if (mainExplosion != null)
            {
                mainExplosion.transform.position = cachedTransform.position;
                explosionPool.ReturnExplosionToPool(mainExplosion, 2f);

                if (!isBoss)
                {
                    int extraExplosions = Random.Range(3, 6);
                    for (int i = 0; i < extraExplosions; i++)
                    {
                        Vector3 offset = new Vector3(
                            Random.Range(-1.5f, 1.5f),
                            Random.Range(-1f, 1f),
                            Random.Range(-1.5f, 1.5f)
                        );

                        GameObject extraExplosion = explosionPool.GetExplosion(ExplosionType.Small);
                        if (extraExplosion != null)
                        {
                            extraExplosion.transform.position = cachedTransform.position + offset;
                            explosionPool.ReturnExplosionToPool(extraExplosion, 1f);
                        }
                    }
                }
            }
        }
        else if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, cachedTransform.position, Quaternion.identity);
            explosion.transform.localScale = Vector3.one * explosionScale;
            Destroy(explosion, 3f);
        }

        audioManager?.PlayBombSound(BombSoundType.Explosion);

        if (cameraShake != null)
        {
            if (isBoss)
                cameraShake.ShakaCameraVidBossDöd();
            else
                cameraShake.ShakaCameraVidBomb();
        }
    }

    public void TakeDamage(float damage)
    {
        Debug.Log($"{gameObject.name} - TakeDamage called with damage: {damage}");
        Debug.Log($"Current health before: {currentHealth}, isDying: {isDying}, initialized: {initialized}");

        if (!initialized || isDying) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            StartDying();
        }
        Debug.Log($"Current health after: {currentHealth}");
    }

    // Ersätt StartDying() metoden i EnemyHealth.cs med denna:
    public void StartDying()
    {
        if (isDying) return;

        isDying = true;
        crashStartTime = Time.time;

        if (boostDropSystem != null)
        {
            boostDropSystem.TryDropBoost(cachedTransform.position);
        }

        // NYTT: Kolla om detta är en boss och trigga completion via separat manager
        if (isBoss)
        {
            Debug.Log("Boss defeated! Triggering level completion...");
            BossCompletionManager.TriggerBossDefeated();
        }

        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
            {
                script.enabled = false;
            }
        }

        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        if (scoreManager != null)
        {
            if (isBoss)
            {
                scoreManager.AddBossPoints();
                if (cameraShake != null)
                {
                    cameraShake.ShakaCameraVidBossDöd();
                }

                if (explosionPool != null)
                {
                    GameObject bossExplosion = explosionPool.GetExplosion(ExplosionType.Boss);
                    if (bossExplosion != null)
                    {
                        bossExplosion.transform.position = cachedTransform.position;
                        explosionPool.ReturnExplosionToPool(bossExplosion, 3f);
                    }
                }
            }
            else
            {
                scoreManager.AddEnemyShipPoints();

                if (explosionPool != null)
                {
                    GameObject enemyExplosion = explosionPool.GetExplosion(ExplosionType.Small);
                    if (enemyExplosion != null)
                    {
                        enemyExplosion.transform.position = cachedTransform.position;
                        explosionPool.ReturnExplosionToPool(enemyExplosion, 2f);
                    }
                }
            }
        }

        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }
    }

    // TA BORT CompleteLevelAfterDelay() och StartFallbackLevelCompletion() metoderna från EnemyHealth.cs

    private IEnumerator CompleteLevelAfterDelay(float delay)
    {
        Debug.Log("=== BOSS DEATH SEQUENCE STARTED ===");

        GameMessageSystem messageSystem = FindObjectOfType<GameMessageSystem>();
        if (messageSystem != null)
        {
            messageSystem.ShowBoostMessage("BOSS DEFEATED!");
            Debug.Log("Boss defeated message shown");
        }
        else
        {
            Debug.LogError("GameMessageSystem not found!");
        }

        Debug.Log($"Waiting {delay} seconds before completing level...");
        yield return new WaitForSeconds(delay);

        Debug.Log("Delay completed! Now checking for LevelManager...");

        if (LevelManager.Instance != null)
        {
            Debug.Log($"LevelManager found! Current level: {LevelManager.Instance.currentLevel}");
            Debug.Log("Calling LevelManager.CompleteLevel()...");

            try
            {
                LevelManager.Instance.CompleteLevel();
                Debug.Log("LevelManager.CompleteLevel() called successfully!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error calling CompleteLevel: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("=== KRITISKT FEL: LevelManager.Instance är NULL! ===");
            Debug.LogError("Försöker hitta LevelManager i scenen...");

            LevelManager levelManager = FindObjectOfType<LevelManager>();
            if (levelManager != null)
            {
                Debug.LogWarning("Hittade LevelManager med FindObjectOfType - men Instance är null!");
                try
                {
                    levelManager.CompleteLevel();
                    Debug.Log("Manual CompleteLevel() call successful!");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error with manual CompleteLevel: {e.Message}");
                    StartFallbackLevelCompletion();
                }
            }
            else
            {
                Debug.LogError("Ingen LevelManager hittades i scenen alls!");
                StartFallbackLevelCompletion();
            }
        }

        Debug.Log("=== BOSS DEATH SEQUENCE COMPLETED ===");
    }

    private void StartFallbackLevelCompletion()
    {
        Debug.LogWarning("FALLBACK: Försöker ladda LoadingScreen direkt...");

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.StopGame();
        }

        string[] possibleLoadingScreens = {
            "LoadingScreen",
            "Loading",
            "LoadScreen"
        };

        foreach (string screenName in possibleLoadingScreens)
        {
            try
            {
                Debug.Log($"Trying to load {screenName}...");
                UnityEngine.SceneManagement.SceneManager.LoadScene(screenName);
                return;
            }
            catch (System.Exception e)
            {
                Debug.Log($"Couldn't load {screenName}: {e.Message}");
            }
        }

        Debug.LogWarning("Last resort: Loading next scene in build order...");
        int currentIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextIndex);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }
}
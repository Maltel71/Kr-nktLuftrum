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

        // Cache transform
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

        // Handle BoostDropSystem - BoostDropSystem does not use Instance pattern
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

    private void IgnoreEnemyCollisions()
    {
        Collider[] myColliders = GetComponentsInChildren<Collider>();

        // Use cached collision list for performance
        if (cachedEnemyColliders == null || Time.time - lastEnemyColliderUpdate > enemyColliderCacheTime)
        {
            UpdateEnemyColliderCache();
        }

        // Apply collision ignore
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

        // Ignore self-collisions
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

        // Cache position and rotation
        cachedPosition = cachedTransform.position;
        cachedRotation = cachedTransform.rotation;

        // Move downward
        float speedMultiplier = isBoss ? 0.5f : 1f;
        cachedPosition.y -= crashSpeed * speedMultiplier * Time.deltaTime;
        cachedTransform.position = cachedPosition;

        // Rotate
        float rotationAmount = rotateClockwise ? rotationSpeed : -rotationSpeed;
        cachedTransform.Rotate(0, 0, rotationAmount * Time.deltaTime);

        // Check if reached ground or time is up
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
            // Main explosion
            GameObject mainExplosion = explosionPool.GetExplosion(
                isBoss ? ExplosionType.Boss : ExplosionType.Large
            );

            if (mainExplosion != null)
            {
                mainExplosion.transform.position = cachedTransform.position;
                explosionPool.ReturnExplosionToPool(mainExplosion, 2f);

                // Extra explosions for non-bosses
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

        // Effects
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
    }

    public void StartDying()
    {
        if (isDying) return;

        isDying = true;
        crashStartTime = Time.time;

        if (boostDropSystem != null)
        {
            boostDropSystem.TryDropBoost(cachedTransform.position);
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

                // Boss explosion
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

                // Enemy explosion
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

        //audioManager?.PlayCombatSound(CombatSoundType.Death);
    }

    private void CreateHealthBar()
    {
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
}

//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;

//public class EnemyHealth : MonoBehaviour
//{
//    [Header("Health Settings")]
//    [SerializeField] private float maxHealth = 100f;
//    private float currentHealth;

//    [Header("Boss Settings")]
//    [SerializeField] private bool isBoss = false;

//    [Header("UI")]
//    [SerializeField] private GameObject healthBarPrefab;
//    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 2f, 0);
//    [SerializeField] private Vector3 healthBarScale = new Vector3(0.05f, 0.05f, 0.05f);

//    [Header("Crash Settings")]
//    [SerializeField] private float crashSpeed = 200f;
//    [SerializeField] private float crashDuration = 5f;
//    [SerializeField] private float rotationSpeed = 3200f;
//    [SerializeField] private bool rotateClockwise = true;
//    [SerializeField] private float groundLevel = 0f; // Y-positionen för marknivån

//    [Header("Explosion Settings")]
//    [SerializeField] private GameObject explosionPrefab;
//    [SerializeField] private float explosionScale = 1f;
//    [SerializeField] private float randomExplosionDelayMin = 1f;
//    [SerializeField] private float randomExplosionDelayMax = 3f;

//    [Header("Smoke Effects")]
//    [SerializeField] private ParticleSystem engineExhaustSmoke;
//    [SerializeField] private float smokeHealthThreshold = 0.2f;
//    [SerializeField] private ParticleSystem damageSmokeEffect;
//    [SerializeField] private float damageSmokefadeDuration = 2f;

//    [Header("Boost Drops")]
//    [SerializeField] private BoostDropSystem boostDropSystem;

//    [Header("Colliders")]
//    [SerializeField] public Collider mainCollider;        // Drag n drop huvudcollidern här i Unity
//    [SerializeField] private Collider damageCollider;      // Drag n 

//    private Slider healthSlider;
//    private GameObject healthBarInstance;
//    private CameraShake cameraShake;
//    private ScoreManager scoreManager;
//    private bool initialized = false;
//    private bool isDying = false;
//    private bool smokeStarted = false;
//    private bool hasExploded = false;
//    private float crashStartTime;

//    public bool IsDying => isDying;

//    private void Awake()
//    {
//        isDying = false;
//        hasExploded = false;
//        initialized = false;
//        // Debug.Log($"Enemy {gameObject.name} Awake completed");
//    }

//    private void Start()
//    {
//        //Debug.Log($"Enemy {gameObject.name} starting. MaxHealth: {maxHealth}");
//        currentHealth = maxHealth;
//        //Debug.Log($"Set currentHealth to {currentHealth}");

//        CreateHealthBar();
//        cameraShake = CameraShake.Instance;
//        scoreManager = ScoreManager.Instance;

//        if (boostDropSystem == null)
//        {
//            boostDropSystem = FindObjectOfType<BoostDropSystem>();
//        }

//        if (engineExhaustSmoke != null)
//        {
//            engineExhaustSmoke.Play();
//        }

//        if (damageSmokeEffect != null)
//        {
//            damageSmokeEffect.Stop();
//        }

//        if (mainCollider == null)
//            mainCollider = GetComponent<Collider>();

//        if (damageCollider == null)
//        {
//            // Skapa en trigger collider om ingen är tilldelad
//            var triggerObj = new GameObject("DamageCollider");
//            triggerObj.transform.parent = transform;
//            triggerObj.transform.localPosition = Vector3.zero;
//            damageCollider = triggerObj.AddComponent<BoxCollider>();
//            damageCollider.isTrigger = true;
//        }
//        IgnoreEnemyCollisions();

//        initialized = true;
//        //Debug.Log($"Enemy {gameObject.name} initialization complete");
//    }



//    private void Update()
//    {
//        if (!initialized) return;

//        if (healthBarInstance != null)
//        {
//            healthBarInstance.transform.rotation = Camera.main.transform.rotation;
//        }

//        if (!isDying && !smokeStarted && GetHealthPercentage() <= smokeHealthThreshold)
//        {
//            StartDamageSmokeEffect();
//        }

//        if (isDying && !hasExploded)
//        {
//            HandleCrashing();
//        }
//    }

//    private void IgnoreEnemyCollisions()
//    {
//        // Hämta alla colliders på denna fiende
//        Collider[] myColliders = GetComponentsInChildren<Collider>();

//        // Hämta alla andra fiendeobjekt
//        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

//        foreach (GameObject enemy in enemies)
//        {
//            // Ignorera kollisioner med andra fiender
//            if (enemy != gameObject)
//            {
//                Collider[] enemyColliders = enemy.GetComponentsInChildren<Collider>();

//                foreach (Collider myCol in myColliders)
//                {
//                    foreach (Collider enemyCol in enemyColliders)
//                    {
//                        Physics.IgnoreCollision(myCol, enemyCol);
//                    }
//                }
//            }
//            else
//            {
//                // Ignorera kollisioner med sig själv också (undvik "Mig_01 kolliderade med Mig_01")
//                for (int i = 0; i < myColliders.Length; i++)
//                {
//                    for (int j = i + 1; j < myColliders.Length; j++)
//                    {
//                        Physics.IgnoreCollision(myColliders[i], myColliders[j]);
//                    }
//                }
//            }
//        }
//    }

//    private void StartDamageSmokeEffect()
//    {
//        if (damageSmokeEffect != null && !smokeStarted)
//        {
//            damageSmokeEffect.Play();
//            smokeStarted = true;
//        }
//    }

//    // Lägg till denna metod för extern åtkomst
//    public void StartSmokeEffects()
//    {
//        StartDamageSmokeEffect();
//    }

//    //private void StartDamageSmokeEffect()
//    //{
//    //    if (damageSmokeEffect != null)
//    //    {
//    //        damageSmokeEffect.Play();
//    //        smokeStarted = true;
//    //    }
//    //}

//    public float GetHealthPercentage()
//    {
//        return currentHealth / maxHealth;
//    }

//    private void HandleCrashing()
//    {
//        if (!isDying || hasExploded) return;

//        // Rörelse nedåt
//        Vector3 pos = transform.position;
//        pos.y -= (isBoss ? crashSpeed * 0.5f : crashSpeed) * Time.deltaTime;
//        transform.position = pos;

//        // Rotation
//        float rotationAmount = rotateClockwise ? rotationSpeed : -rotationSpeed;
//        transform.Rotate(0, 0, rotationAmount * Time.deltaTime);

//        // Kolla om planet nått marknivån eller om tiden gått ut
//        bool reachedGround = transform.position.y <= groundLevel;
//        bool timeIsUp = Time.time >= crashStartTime + crashDuration;

//        if ((reachedGround || timeIsUp) && !hasExploded)
//        {
//            hasExploded = true; // VIKTIGT: Sätt till true, inte false!

//            Debug.Log($"Enemy {gameObject.name} nådde marken eller tidsgränsen. Skapar explosion...");

//            // Skapa FLERA explosioner för visuell effekt - en stor i mitten och några mindre runtomkring
//            if (ExplosionPool.Instance != null)
//            {
//                // Huvudexplosion
//                GameObject mainExplosion = ExplosionPool.Instance.GetExplosion(
//                    isBoss ? ExplosionType.Boss : ExplosionType.Large
//                );

//                if (mainExplosion != null)
//                {
//                    mainExplosion.transform.position = transform.position;
//                    ExplosionPool.Instance.ReturnExplosionToPool(mainExplosion, 2f);

//                    // Skapa 3-5 små explosioner runt den stora för en mer dramatisk effekt
//                    if (!isBoss) // Skippa extraexplosioner för bossar som har sina egna fina explosioner
//                    {
//                        int extraExplosions = Random.Range(3, 6);
//                        for (int i = 0; i < extraExplosions; i++)
//                        {
//                            Vector3 offset = new Vector3(
//                                Random.Range(-1.5f, 1.5f),
//                                Random.Range(-1f, 1f),
//                                Random.Range(-1.5f, 1.5f)
//                            );

//                            GameObject extraExplosion = ExplosionPool.Instance.GetExplosion(ExplosionType.Small);
//                            if (extraExplosion != null)
//                            {
//                                extraExplosion.transform.position = transform.position + offset;
//                                ExplosionPool.Instance.ReturnExplosionToPool(extraExplosion, 1f);
//                            }
//                        }
//                    }
//                }
//            }
//            // Fallback till prefab om poolen saknas
//            else if (explosionPrefab != null)
//            {
//                GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
//                explosion.transform.localScale = Vector3.one * explosionScale;
//                Destroy(explosion, 3f);
//            }

//            // VIKTIGT: Spela ljud för explosion
//            AudioManager.Instance?.PlayBombSound(BombSoundType.Explosion);

//            // VIKTIGT: Lägg till kameraskakning för bättre känsla
//            if (cameraShake != null)
//            {
//                if (isBoss)
//                    cameraShake.ShakaCameraVidBossDöd();
//                else
//                    cameraShake.ShakaCameraVidBomb();
//            }

//            // Förstör fiendeobjektet med en kort fördröjning för att låta explosionen synas
//            Destroy(gameObject, 0.2f);
//        }
//    }

//    private IEnumerator ExplodeWithRandomDelay()
//    {
//        if (hasExploded)
//        {
//            Debug.Log($"Enemy {gameObject.name} already exploded, skipping");
//            yield break;
//        }

//        hasExploded = true;
//        //Debug.Log($"Enemy {gameObject.name} starting explosion delay");

//        float randomDelay = Random.Range(randomExplosionDelayMin, randomExplosionDelayMax);
//        yield return new WaitForSeconds(randomDelay);

//        if (explosionPrefab != null)
//        {
//            Debug.Log($"Enemy {gameObject.name} creating explosion effect");
//            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
//            explosion.transform.localScale = Vector3.one * explosionScale;
//            Destroy(explosion, 3f);
//        }

//        if (damageSmokeEffect != null)
//        {
//            StartCoroutine(FadeDamageSmoke());
//        }

//        Destroy(gameObject);
//    }

//    private IEnumerator FadeDamageSmoke()
//    {
//        if (damageSmokeEffect != null)
//        {
//            var main = damageSmokeEffect.main;
//            float originalStartLifetime = main.startLifetime.constant;
//            float elapsedTime = 0f;

//            while (elapsedTime < damageSmokefadeDuration)
//            {
//                float t = elapsedTime / damageSmokefadeDuration;
//                float smoothT = -(Mathf.Cos(Mathf.PI * t) - 1) / 2;
//                main.startLifetime = Mathf.Lerp(originalStartLifetime, 0f, smoothT);

//                elapsedTime += Time.deltaTime;
//                yield return null;
//            }

//            damageSmokeEffect.Stop();
//        }
//    }

//    public void TakeDamage(float damage)
//    {
//        if (!initialized || isDying)
//        {
//            Debug.Log($"Enemy {gameObject.name} ignoring damage. Initialized: {initialized}, IsDying: {isDying}");
//            return;
//        }

//        Debug.Log($"Enemy {gameObject.name} taking {damage} damage. Current health: {currentHealth}");
//        currentHealth = Mathf.Max(0, currentHealth - damage);
//        Debug.Log($"Health after damage: {currentHealth}");

//        if (healthSlider != null)
//        {
//            healthSlider.value = currentHealth;
//        }

//        if (currentHealth <= 0)
//        {
//            //Debug.Log($"Enemy {gameObject.name} health reached 0, starting death sequence");
//            StartDying();
//        }
//    }

//    public void StartDying()
//    {
//        if (isDying) return;

//        isDying = true;
//        crashStartTime = Time.time;

//        if (boostDropSystem != null)
//        {
//            boostDropSystem.TryDropBoost(transform.position);
//        }

//        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
//        foreach (MonoBehaviour script in scripts)
//        {
//            if (script != this)
//            {
//                script.enabled = false;
//            }
//        }

//        Collider[] colliders = GetComponents<Collider>();
//        foreach (Collider col in colliders)
//        {
//            col.enabled = false;
//        }

//        if (scoreManager != null)
//        {
//            if (isBoss)
//            {
//                scoreManager.AddBossPoints();
//                if (cameraShake != null)
//                {
//                    cameraShake.ShakaCameraVidBossDöd();
//                }
//                // Använd boss-explosion för bossar
//                GameObject bossExplosion = ExplosionPool.Instance.GetExplosion(ExplosionType.Boss);
//                bossExplosion.transform.position = transform.position;
//                ExplosionPool.Instance.ReturnExplosionToPool(bossExplosion, 3f);
//            }
//            else
//            {
//                scoreManager.AddEnemyShipPoints();
//                // Använd mindre explosion för vanliga fiender
//                // Ta bort för att hantera i missile skriptet
//                GameObject enemyExplosion = ExplosionPool.Instance.GetExplosion(ExplosionType.Small);
//                enemyExplosion.transform.position = transform.position;
//                ExplosionPool.Instance.ReturnExplosionToPool(enemyExplosion, 2f);
//            }
//        }

//        if (healthBarInstance != null)
//        {
//            Destroy(healthBarInstance);
//        }

//        AudioManager.Instance?.PlayCombatSound(CombatSoundType.Death);
//    }

//    private void CreateHealthBar()
//    {
//        healthBarInstance = Instantiate(healthBarPrefab, transform.position + healthBarOffset, Quaternion.identity, transform);
//        healthSlider = healthBarInstance.GetComponentInChildren<Slider>();

//        if (healthSlider != null)
//        {
//            healthSlider.minValue = 0;
//            healthSlider.maxValue = maxHealth;
//            healthSlider.value = currentHealth;
//        }

//        Canvas canvas = healthBarInstance.GetComponentInChildren<Canvas>();
//        if (canvas != null)
//        {
//            canvas.renderMode = RenderMode.WorldSpace;
//            canvas.sortingOrder = 10;
//        }

//        healthBarInstance.transform.localScale = healthBarScale;
//    }
//}
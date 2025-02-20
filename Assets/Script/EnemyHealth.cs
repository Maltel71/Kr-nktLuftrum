using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
    [SerializeField] public Collider mainCollider;        // Drag n drop huvudcollidern här i Unity
    [SerializeField] private Collider damageCollider;      // Drag n 

    private Slider healthSlider;
    private GameObject healthBarInstance;
    private CameraShake cameraShake;
    private ScoreManager scoreManager;
    private bool initialized = false;
    private bool isDying = false;
    private bool smokeStarted = false;
    private bool hasExploded = false;
    private float crashStartTime;

    public bool IsDying => isDying;

    private void Awake()
    {
        isDying = false;
        hasExploded = false;
        initialized = false;
        // Debug.Log($"Enemy {gameObject.name} Awake completed");
    }

    private void Start()
    {
        //Debug.Log($"Enemy {gameObject.name} starting. MaxHealth: {maxHealth}");
        currentHealth = maxHealth;
        //Debug.Log($"Set currentHealth to {currentHealth}");

        CreateHealthBar();
        cameraShake = CameraShake.Instance;
        scoreManager = ScoreManager.Instance;

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
            // Skapa en trigger collider om ingen är tilldelad
            var triggerObj = new GameObject("DamageCollider");
            triggerObj.transform.parent = transform;
            triggerObj.transform.localPosition = Vector3.zero;
            damageCollider = triggerObj.AddComponent<BoxCollider>();
            damageCollider.isTrigger = true;
        }

        initialized = true;
        //Debug.Log($"Enemy {gameObject.name} initialization complete");
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

    private void StartDamageSmokeEffect()
    {
        if (damageSmokeEffect != null && !smokeStarted)
        {
            damageSmokeEffect.Play();
            smokeStarted = true;
        }
    }

    // Lägg till denna metod för extern åtkomst
    public void StartSmokeEffects()
    {
        StartDamageSmokeEffect();
    }

    //private void StartDamageSmokeEffect()
    //{
    //    if (damageSmokeEffect != null)
    //    {
    //        damageSmokeEffect.Play();
    //        smokeStarted = true;
    //    }
    //}

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    private void HandleCrashing()
    {
        if (!isDying || hasExploded) return;

        Vector3 pos = transform.position;
        pos.y -= (isBoss ? crashSpeed * 0.5f : crashSpeed) * Time.deltaTime; // Långsammare för bossar
        transform.position = pos;

        float rotationAmount = rotateClockwise ? rotationSpeed : -rotationSpeed;
        transform.Rotate(0, 0, rotationAmount * Time.deltaTime);

        if (Time.time >= crashStartTime + crashDuration && !hasExploded)
        {
            hasExploded = false;

            Debug.Log($"Enemy {gameObject.name} starting explosion sequence");
            AudioManager.Instance?.PlayBombSound(BombSoundType.Explosion);

            // Specifik explosionshantering för bossar
            if (isBoss)
            {
                GameObject bossExplosion = ExplosionPool.Instance.GetExplosion(ExplosionType.Boss);
                bossExplosion.transform.position = transform.position;
                ExplosionPool.Instance.ReturnExplosionToPool(bossExplosion, 3f);
                Destroy(gameObject);
            }
            else
            {
                StartCoroutine(ExplodeWithRandomDelay());
            }
        }
    }

    private IEnumerator ExplodeWithRandomDelay()
    {
        if (hasExploded)
        {
            Debug.Log($"Enemy {gameObject.name} already exploded, skipping");
            yield break;
        }

        hasExploded = true;
        //Debug.Log($"Enemy {gameObject.name} starting explosion delay");

        float randomDelay = Random.Range(randomExplosionDelayMin, randomExplosionDelayMax);
        yield return new WaitForSeconds(randomDelay);

        if (explosionPrefab != null)
        {
            Debug.Log($"Enemy {gameObject.name} creating explosion effect");
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            explosion.transform.localScale = Vector3.one * explosionScale;
            Destroy(explosion, 3f);
        }

        if (damageSmokeEffect != null)
        {
            StartCoroutine(FadeDamageSmoke());
        }

        Destroy(gameObject);
    }

    private IEnumerator FadeDamageSmoke()
    {
        if (damageSmokeEffect != null)
        {
            var main = damageSmokeEffect.main;
            float originalStartLifetime = main.startLifetime.constant;
            float elapsedTime = 0f;

            while (elapsedTime < damageSmokefadeDuration)
            {
                float t = elapsedTime / damageSmokefadeDuration;
                float smoothT = -(Mathf.Cos(Mathf.PI * t) - 1) / 2;
                main.startLifetime = Mathf.Lerp(originalStartLifetime, 0f, smoothT);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            damageSmokeEffect.Stop();
        }
    }

    public void TakeDamage(float damage)
    {
        if (!initialized || isDying)
        {
            Debug.Log($"Enemy {gameObject.name} ignoring damage. Initialized: {initialized}, IsDying: {isDying}");
            return;
        }

        Debug.Log($"Enemy {gameObject.name} taking {damage} damage. Current health: {currentHealth}");
        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"Health after damage: {currentHealth}");

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            //Debug.Log($"Enemy {gameObject.name} health reached 0, starting death sequence");
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
            boostDropSystem.TryDropBoost(transform.position);
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
                // Använd boss-explosion för bossar
                GameObject bossExplosion = ExplosionPool.Instance.GetExplosion(ExplosionType.Boss);
                bossExplosion.transform.position = transform.position;
                ExplosionPool.Instance.ReturnExplosionToPool(bossExplosion, 3f);
            }
            else
            {
                scoreManager.AddEnemyShipPoints();
                // Använd mindre explosion för vanliga fiender
                // Ta bort för att hantera i missile skriptet
                GameObject enemyExplosion = ExplosionPool.Instance.GetExplosion(ExplosionType.Small);
                enemyExplosion.transform.position = transform.position;
                ExplosionPool.Instance.ReturnExplosionToPool(enemyExplosion, 2f);
            }
        }

        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }

        AudioManager.Instance?.PlayCombatSound(CombatSoundType.Death);
    }

    private void CreateHealthBar()
    {
        healthBarInstance = Instantiate(healthBarPrefab, transform.position + healthBarOffset, Quaternion.identity, transform);
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
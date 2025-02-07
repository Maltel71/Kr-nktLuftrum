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

    private Slider healthSlider;
    private GameObject healthBarInstance;
    private CameraShake cameraShake;
    private ScoreManager scoreManager;
    private bool isDying = false;
    private bool smokeStarted = false;
    private float crashStartTime;
    private bool hasExploded = false;

    public bool IsDying => isDying;

    private void Start()
    {
        currentHealth = maxHealth;
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

        if (damageSmokeEffect != null) damageSmokeEffect.Stop();
    }

    private void Update()
    {
        if (healthBarInstance != null)
        {
            healthBarInstance.transform.rotation = Camera.main.transform.rotation;
        }

        if (!isDying && !smokeStarted && GetHealthPercentage() <= smokeHealthThreshold)
        {
            StartDamageSmokeEffect();
        }

        if (isDying)
        {
            HandleCrashing();
        }
    }

    private void StartDamageSmokeEffect()
    {
        if (damageSmokeEffect != null)
        {
            damageSmokeEffect.Play();
            smokeStarted = true;
        }
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    private void HandleCrashing()
    {
        Vector3 pos = transform.position;
        pos.y -= crashSpeed * Time.deltaTime;
        transform.position = pos;

        float rotationAmount = rotateClockwise ? rotationSpeed : -rotationSpeed;
        transform.Rotate(0, 0, rotationAmount * Time.deltaTime);

        if (Time.time >= crashStartTime + crashDuration)
        {
            if (!hasExploded)
            {
                AudioManager.Instance?.PlayBombSound(BombSoundType.Explosion);
                StartCoroutine(ExplodeWithRandomDelay());
            }
        }
    }

    private IEnumerator ExplodeWithRandomDelay()
    {
        hasExploded = true;
        float randomDelay = Random.Range(randomExplosionDelayMin, randomExplosionDelayMax);

        yield return new WaitForSeconds(randomDelay);

        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            explosion.transform.localScale = Vector3.one * explosionScale;
            Destroy(explosion, 3f);
        }

        Destroy(gameObject);

        if (damageSmokeEffect != null)
        {
            StartCoroutine(FadeDamageSmoke());
        }
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
        if (isDying) return;

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

    private void StartDying()
    {
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
            }
            else
            {
                scoreManager.AddEnemyShipPoints();
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
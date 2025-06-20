using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlaneHealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Shield Settings")]
    [SerializeField] private Slider shieldSlider;
    [SerializeField] private float maxShield = 100f;
    private float currentShield;

    [Header("Damage Effects")]
    [SerializeField] private ParticleSystem smokeEffect1;
    [SerializeField] private ParticleSystem smokeEffect2;
    [SerializeField] private ParticleSystem smokeEffect3;
    [SerializeField] private ParticleSystem fireEffect;
    [SerializeField] private Transform smokeSpawnPoint;
    [SerializeField] private float smoke1HPTrigger = 75f;
    [SerializeField] private float smoke2HPTrigger = 50f;
    [SerializeField] private float smoke3HPTrigger = 25f;
    [SerializeField] private float fireHPTrigger = 25f;

    [Header("References")]
    [SerializeField] private GameMessageSystem messageSystem;
    [SerializeField] private GameObject planeModel;

    [Header("Slider Animation")]
    [SerializeField] private float sliderSpeed = 5f;
    private float targetHealthValue;
    private float targetShieldValue;

    [Header("Test Settings")]
    [SerializeField] private float testDamageAmount = 10f;
    [SerializeField] private bool showDebugLogs = true;

    [Header("Animation")]
    [SerializeField] private Animator planeAnimator;
    private static readonly int PlayerDeathTrigger = Animator.StringToHash("Player_Death");
    private bool isDead = false;

    [Header("Crash Settings")]
    [SerializeField] private float crashSpeed = 200f;
    [SerializeField] private float crashRotationSpeed = 1000f;
    [SerializeField] private float groundY = 0f;
    private bool isCrashing = false;

    [Header("Invincibility Settings")]
    [SerializeField] private float invincibilityDuration = 3f;
    [SerializeField] private float blinkRate = 0.2f;
    private bool isInvincible = false;
    private Renderer[] renderers;

    private AirplaneController airplaneController;

    private void Start()
    {
        if (messageSystem == null)
        {
            messageSystem = FindObjectOfType<GameMessageSystem>();
        }

        if (planeModel == null)
        {
            planeModel = gameObject;
        }

        renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Debug.LogWarning("No renderers found on plane!");
        }

        if (planeAnimator == null)
        {
            planeAnimator = GetComponent<Animator>();
        }

        airplaneController = GetComponent<AirplaneController>();
        currentHealth = maxHealth;
        targetHealthValue = currentHealth;
        currentShield = maxShield;
        targetShieldValue = currentShield;
        UpdateSlidersImmediate();
    }

    private void Update()
    {
        if (isDead) return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(testDamageAmount);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RestoreAll();
        }

        UpdateSliders();
    }

    private void UpdateSliders()
    {
        if (healthSlider != null)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetHealthValue, Time.deltaTime * sliderSpeed);
        }

        if (shieldSlider != null)
        {
            shieldSlider.value = Mathf.Lerp(shieldSlider.value, targetShieldValue, Time.deltaTime * sliderSpeed);
        }
    }

    private void UpdateSlidersImmediate()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (shieldSlider != null)
        {
            shieldSlider.maxValue = maxShield;
            shieldSlider.value = currentShield;
        }
    }

    private void UpdateDamageEffects()
    {
        UpdateEffect(smokeEffect1, currentHealth <= smoke1HPTrigger);
        UpdateEffect(smokeEffect2, currentHealth <= smoke2HPTrigger);
        UpdateEffect(smokeEffect3, currentHealth <= smoke3HPTrigger);
        UpdateEffect(fireEffect, currentHealth <= fireHPTrigger);
    }

    private void UpdateEffect(ParticleSystem effect, bool shouldPlay)
    {
        if (effect == null) return;

        if (shouldPlay && !effect.isPlaying)
        {
            effect.transform.position = smokeSpawnPoint.position;
            effect.Play();
        }
        else if (!shouldPlay && effect.isPlaying)
        {
            effect.Stop();
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (!isInvincible)  // Om vi inte �r od�dliga
            {
                float collisionDamage = 200f;  // Eller annat l�mpligt v�rde

                if (currentShield > 0)
                {
                    // Ta bort fr�n sk�lden f�rst
                    float remainingDamage = Mathf.Max(0, collisionDamage - currentShield);
                    currentShield = Mathf.Max(0, currentShield - collisionDamage);
                    targetShieldValue = currentShield;

                    // Om det finns skada kvar efter sk�lden �r slut, ta fr�n h�lsan
                    if (remainingDamage > 0)
                    {
                        TakeDamage(remainingDamage);
                    }
                }
                else
                {
                    // Om ingen sk�ld finns, ta direkt fr�n h�lsan
                    TakeDamage(collisionDamage);
                }
            }
            OnEnemyCollision();  // Starta invincibility efter skadan
        }
    }

    private IEnumerator InvincibilityRoutine()
    {
        if (showDebugLogs) Debug.Log("Starting invincibility");
        isInvincible = true;
        SetEnemyCollision(false);
        float endTime = Time.time + invincibilityDuration;

        while (Time.time < endTime)
        {
            ToggleRenderers(!renderers[0].enabled);
            yield return new WaitForSeconds(blinkRate);
        }

        ToggleRenderers(true);
        isInvincible = false;
        SetEnemyCollision(true);
        if (showDebugLogs) Debug.Log("Ending invincibility");
    }

    private void ToggleRenderers(bool visible)
    {
        foreach (var renderer in renderers)
        {
            if (renderer != null)
                renderer.enabled = visible;
        }
    }

    public void AddHealth(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        targetHealthValue = currentHealth;
        UpdateDamageEffects();
        UpdateSlidersImmediate();
    }

    public void ApplyShieldBoost(float amount)
    {
        currentShield = Mathf.Min(maxShield, currentShield + amount);
        targetShieldValue = currentShield;
        UpdateSlidersImmediate();
    }

    public void TakeDamage(float damage)
    {
        if (isDead || isInvincible) return;

        if (currentShield > 0)
        {
            currentShield = Mathf.Max(0, currentShield - damage);
            targetShieldValue = currentShield;
        }
        else
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            targetHealthValue = currentHealth;
            UpdateDamageEffects();

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                targetHealthValue = 0;
                UpdateSlidersImmediate();
                Die();
                return;
            }
        }
    }

    private void SetEnemyCollision(bool enabled)
    {
        
        var enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (var enemy in enemyObjects)
        {
            var enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null && enemyHealth.mainCollider != null)
            {
                enemyHealth.mainCollider.enabled = enabled;
            }
        }
    }

    public void OnEnemyCollision()
    {
        if (isDead || isInvincible) return;

        StartCoroutine(InvincibilityRoutine());
    }

    public void RestoreAll()
    {
        if (isDead) return;

        currentHealth = maxHealth;
        targetHealthValue = currentHealth;
        currentShield = maxShield;
        targetShieldValue = currentShield;

        UpdateDamageEffects();
        UpdateSlidersImmediate();

        if (messageSystem != null)
        {
            messageSystem.ShowBoostMessage("Health & Shield");
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("Plane destroyed!");

        if (planeAnimator != null)
        {
            planeAnimator.SetTrigger(PlayerDeathTrigger);
            Debug.Log("Death animation triggered");
        }

        UpdateEffect(smokeEffect1, false);
        UpdateEffect(smokeEffect2, false);
        UpdateEffect(smokeEffect3, false);
        UpdateEffect(fireEffect, false);

        if (messageSystem != null)
        {
            messageSystem.ShowDeathMessage();
        }

        if (airplaneController != null)
        {
            airplaneController.FreezePosition();
        }

        StartCoroutine(CrashSequence());
    }

    private IEnumerator CrashSequence()
    {
        isCrashing = true;
        float elapsedTime = 0f;

        HighScoreManager highScoreManager = FindObjectOfType<HighScoreManager>();
        if (highScoreManager != null)
        {
            highScoreManager.OnPlayerDeath(ScoreManager.Instance.GetCurrentScore());
        }

        ScoreManager.Instance.StopGame();
        ScoreManager.Instance.ShowHighScores();

        while (transform.position.y > groundY)
        {
            transform.Rotate(Vector3.forward * crashRotationSpeed * Time.deltaTime);

            Vector3 newPosition = transform.position;
            newPosition.y -= crashSpeed * Time.deltaTime;

            if (newPosition.y <= groundY)
            {
                newPosition.y = groundY;
                CreateExplosion();
                break;
            }

            transform.position = newPosition;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(1f);
    }

    private void CreateExplosion()
    {
        GameObject explosion = ExplosionPool.Instance.GetExplosion(ExplosionType.Large);
        explosion.transform.position = transform.position;
        ExplosionPool.Instance.ReturnExplosionToPool(explosion, 2f);

        AudioManager.Instance?.PlayBombSound(BombSoundType.Explosion);

        CameraShake cameraShake = FindObjectOfType<CameraShake>();
        if (cameraShake != null)
        {
            cameraShake.ShakaCameraVidBomb();
        }

        if (planeModel != null)
        {
            planeModel.SetActive(false);
        }
    }

    public bool IsDead()
    {
        return isDead;
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public float GetShieldPercentage()
    {
        return currentShield / maxShield;
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }
}
using UnityEngine;
using UnityEngine.UI;

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

    private bool isDead = false;
    private AirplaneController airplaneController;

    private void Start()
    {
        if (messageSystem == null)
        {
            messageSystem = FindObjectOfType<GameMessageSystem>();
            Debug.Log(messageSystem != null ? "GameMessageSystem found!" : "GameMessageSystem not found!");
        }

        if (planeModel == null)
        {
            planeModel = gameObject;
            Debug.Log("Using this GameObject as plane model");
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

    public void AddHealth(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        targetHealthValue = currentHealth;
        UpdateDamageEffects();
        UpdateSlidersImmediate();

        if (messageSystem != null)
        {
            messageSystem.ShowBoostMessage("Health");
        }
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
            effect.Play();
        }
        else if (!shouldPlay && effect.isPlaying)
        {
            effect.Stop();
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // Om det finns sköld kvar
        if (currentShield > 0)
        {
            // Ta bort från skölden först
            currentShield = Mathf.Max(0, currentShield - damage);
            targetShieldValue = currentShield;
            Debug.Log($"Shield hit! Shield remaining: {currentShield}");
        }
        // Om skölden är slut, ta skada på hälsan
        else
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            targetHealthValue = currentHealth;
            Debug.Log($"Health hit! Health remaining: {currentHealth}");

            UpdateDamageEffects();

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                targetHealthValue = 0;
                UpdateSlidersImmediate();
                Die();
            }
        }
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

        // Stop all effects
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

        if (planeModel != null)
        {
            Renderer[] renderers = planeModel.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.enabled = false;
            }

            SpriteRenderer[] spriteRenderers = planeModel.GetComponentsInChildren<SpriteRenderer>();
            foreach (var spriteRenderer in spriteRenderers)
            {
                spriteRenderer.enabled = false;
            }
        }
        // Anropa highscore-systemet
        HighScoreManager highScoreManager = FindObjectOfType<HighScoreManager>();
        if (highScoreManager != null)
        {
            highScoreManager.OnPlayerDeath(ScoreManager.Instance.GetCurrentScore());
        }

        ScoreManager.Instance.StopGame();
        ScoreManager.Instance.ShowHighScores();
    }

    public bool IsDead() => isDead;
    public float GetHealthPercentage() => currentHealth / maxHealth;
    public float GetShieldPercentage() => currentShield / maxShield;

    public void ApplyShieldBoost()
    {
        currentShield = maxShield;
        targetShieldValue = currentShield;
        UpdateSlidersImmediate();

        if (messageSystem != null)
        {
            messageSystem.ShowBoostMessage("Shield");
        }
    }
}
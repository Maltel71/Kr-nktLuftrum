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

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentShield = Mathf.Max(0, currentShield - damage);
        targetShieldValue = currentShield;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        targetHealthValue = currentHealth;

        Debug.Log($"Damage taken! Health: {currentHealth}, Shield: {currentShield}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            targetHealthValue = 0;
            UpdateSlidersImmediate();
            Die();
        }
    }

    public void RestoreAll()
    {
        if (isDead) return;

        currentHealth = maxHealth;
        targetHealthValue = currentHealth;
        currentShield = maxShield;
        targetShieldValue = currentShield;

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

        ScoreManager.Instance.ShowHighScores();
    }

    public bool IsDead() => isDead;
    public float GetHealthPercentage() => currentHealth / maxHealth;
    public float GetShieldPercentage() => currentShield / maxShield;
}
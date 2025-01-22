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
    [SerializeField] private float shieldBoostAmount = 50f;

    [Header("Slider Animation")]
    [SerializeField] private float sliderSpeed = 5f; // Hur snabbt slidern rör sig
    private float targetHealthValue;
    private float targetShieldValue;

    [Header("Test Settings")]
    [SerializeField] private float testDamageAmount = 10f;

    private void Start()
    {
        // Sätt upp health slider
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }

        // Sätt upp shield slider
        if (shieldSlider != null)
        {
            shieldSlider.maxValue = maxShield;
            shieldSlider.value = 0;
        }

        currentHealth = maxHealth;
        targetHealthValue = currentHealth;
        currentShield = 0;
        targetShieldValue = currentShield;
    }

    private void Update()
    {
        // Test knappar för utveckling
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(testDamageAmount);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            AddHealth(testDamageAmount);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            AddShieldBoost();
        }

        // Animera health slider
        if (healthSlider != null && healthSlider.value != targetHealthValue)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetHealthValue, Time.deltaTime * sliderSpeed);
        }

        // Animera shield slider
        if (shieldSlider != null && shieldSlider.value != targetShieldValue)
        {
            shieldSlider.value = Mathf.Lerp(shieldSlider.value, targetShieldValue, Time.deltaTime * sliderSpeed);
        }
    }

    public void TakeDamage(float damage)
    {
        if (currentShield > 0)
        {
            if (damage <= currentShield)
            {
                currentShield -= damage;
                targetShieldValue = currentShield;
                Debug.Log($"Shield took damage. Shield remaining: {currentShield}");
                return;
            }
            else
            {
                damage -= currentShield;
                currentShield = 0;
                targetShieldValue = 0;
                Debug.Log("Shield depleted!");
            }
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);
        targetHealthValue = currentHealth;
        Debug.Log($"Health took damage. Health remaining: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void AddShieldBoost()
    {
        currentShield = shieldBoostAmount;
        targetShieldValue = currentShield;
        Debug.Log($"Shield boosted to: {currentShield}");
    }

    public void AddHealth(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        targetHealthValue = currentHealth;
        Debug.Log($"Health restored to: {currentHealth}");
    }

    private void Die()
    {
        Debug.Log("Plane destroyed!");
        gameObject.SetActive(false);
    }

    // Getters för andra scripts
    public float GetHealthPercentage() => currentHealth / maxHealth;
    public float GetShieldPercentage() => currentShield / maxShield;
    public bool IsDead() => currentHealth <= 0;
}
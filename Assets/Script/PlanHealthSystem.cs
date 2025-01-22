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

    [Header("Slider Animation")]
    [SerializeField] private float sliderSpeed = 5f;
    private float targetHealthValue;
    private float targetShieldValue;

    [Header("Test Settings")]
    [SerializeField] private float testDamageAmount = 10f;  // Skada som tas när man trycker T

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
            shieldSlider.value = maxShield;  // Börjar på max
        }

        currentHealth = maxHealth;
        targetHealthValue = currentHealth;
        currentShield = maxShield;  // Börjar på max
        targetShieldValue = currentShield;
    }

    private void Update()
    {
        // Test knappar för utveckling
        if (Input.GetKeyDown(KeyCode.T))
        {
            // Ta skada på både health och shield samtidigt
            TakeDamage(testDamageAmount);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            // Återställ både health och shield
            RestoreAll();
        }

        // Animera health slider
        if (healthSlider != null)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetHealthValue, Time.deltaTime * sliderSpeed);
        }

        // Animera shield slider
        if (shieldSlider != null)
        {
            shieldSlider.value = Mathf.Lerp(shieldSlider.value, targetShieldValue, Time.deltaTime * sliderSpeed);
        }
    }

    public void TakeDamage(float damage)
    {
        // Minska både shield och health direkt
        currentShield = Mathf.Max(0, currentShield - damage);
        targetShieldValue = currentShield;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        targetHealthValue = currentHealth;

        Debug.Log($"Damage taken! Health: {currentHealth}, Shield: {currentShield}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void RestoreAll()
    {
        // Återställ både health och shield till max
        currentHealth = maxHealth;
        targetHealthValue = currentHealth;

        currentShield = maxShield;
        targetShieldValue = currentShield;

        Debug.Log("Health and Shield restored to max!");
    }

    private void Die()
    {
        Debug.Log("Plane destroyed!");
        // Implementera här vad som ska hända när planet förstörs
    }

    // Getters för andra scripts
    public float GetHealthPercentage() => currentHealth / maxHealth;
    public float GetShieldPercentage() => currentShield / maxShield;
    public bool IsDead() => currentHealth <= 0;
}
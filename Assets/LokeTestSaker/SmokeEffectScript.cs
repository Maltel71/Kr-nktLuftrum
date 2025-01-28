using UnityEngine;

public class SmokeEffectScript : MonoBehaviour
{
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem normalSmokeEffect;
    [SerializeField] private ParticleSystem damagedSmokeEffect;
    [SerializeField] private ParticleSystem criticalSmokeEffect;

    [Header("Health Settings")]
    [SerializeField] private int health = 100;
    [SerializeField] private int damagedThreshold = 50;
    [SerializeField] private int criticalThreshold = 20;

    private ParticleSystem currentActiveEffect;
    private bool isInitialized = false;

    private void Start()
    {
        ValidateComponents();
        InitializeSmoke();
    }

    private void ValidateComponents()
    {
        if (normalSmokeEffect == null)
            Debug.LogWarning("Normal smoke effect is not assigned to " + gameObject.name);
        if (damagedSmokeEffect == null)
            Debug.LogWarning("Damaged smoke effect is not assigned to " + gameObject.name);
        if (criticalSmokeEffect == null)
            Debug.LogWarning("Critical smoke effect is not assigned to " + gameObject.name);
    }

    private void InitializeSmoke()
    {
        // Make sure all effects are stopped initially
        normalSmokeEffect?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        damagedSmokeEffect?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        criticalSmokeEffect?.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        // Clear any existing particles
        normalSmokeEffect?.Clear();
        damagedSmokeEffect?.Clear();
        criticalSmokeEffect?.Clear();

        isInitialized = true;
        UpdateSmoke(); // Start the appropriate smoke effect
    }

    public void UpdateHealth(int newHealth)
    {
        int oldHealth = health;
        health = Mathf.Clamp(newHealth, 0, 100);

        if (oldHealth != health)
        {
            UpdateSmoke();
        }
    }

    private void UpdateSmoke()
    {
        if (!isInitialized) return;

        // Stop current effect if it exists
        if (currentActiveEffect != null)
        {
            currentActiveEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            currentActiveEffect.Clear();
        }

        // Get and play new effect
        ParticleSystem newEffect = DetermineEffectBasedOnHealth();
        if (newEffect != null && newEffect != currentActiveEffect)
        {
            currentActiveEffect = newEffect;
            currentActiveEffect.Clear();
            currentActiveEffect.Play();
        }
    }

    private ParticleSystem DetermineEffectBasedOnHealth()
    {
        if (health > damagedThreshold)
            return normalSmokeEffect;
        else if (health > criticalThreshold)
            return damagedSmokeEffect;
        else
            return criticalSmokeEffect;
    }

    // Test method to damage the object (you can call this from other scripts)
    public void TakeDamage(int amount)
    {
        UpdateHealth(health - amount);
    }
}
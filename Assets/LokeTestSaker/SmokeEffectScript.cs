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

    private void Start()
    {
        ValidateComponents();
        UpdateSmoke();
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

    public void UpdateHealth(int newHealth)
    {
        health = Mathf.Clamp(newHealth, 0, 100);
        UpdateSmoke();
    }

    private void UpdateSmoke()
    {
        // Stop current effect if it exists
        if (currentActiveEffect != null)
            currentActiveEffect.Stop();

        // Determine and play new effect
        ParticleSystem newEffect = DetermineEffectBasedOnHealth();
        if (newEffect != null)
        {
            newEffect.Play();
            currentActiveEffect = newEffect;
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
}
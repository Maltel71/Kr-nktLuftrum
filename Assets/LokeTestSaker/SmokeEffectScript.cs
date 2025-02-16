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

    // Track current health state
    private enum HealthState { Normal, Damaged, Critical }
    private HealthState currentState;

    private void Start()
    {
        ValidateComponents();
        InitializeSmoke();
        SetInitialState();
    }

    private void SetInitialState()
    {
        if (health > damagedThreshold) currentState = HealthState.Normal;
        else if (health > criticalThreshold) currentState = HealthState.Damaged;
        else currentState = HealthState.Critical;
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
        normalSmokeEffect?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        damagedSmokeEffect?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        criticalSmokeEffect?.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        isInitialized = true;
        UpdateSmoke();
    }

    public void UpdateHealth(int newHealth)
    {
        int oldHealth = health;
        health = Mathf.Clamp(newHealth, 0, 100);

        HealthState newState;
        if (health > damagedThreshold) newState = HealthState.Normal;
        else if (health > criticalThreshold) newState = HealthState.Damaged;
        else newState = HealthState.Critical;

        if (currentState != newState)
        {
            currentState = newState;
            UpdateSmoke();
        }
    }

    private void UpdateSmoke()
    {
        if (!isInitialized) return;

        ParticleSystem newEffect = DetermineEffectBasedOnHealth();

        if (newEffect != null && newEffect != currentActiveEffect)
        {
            if (currentActiveEffect != null)
            {
                var main = currentActiveEffect.main;
                main.stopAction = ParticleSystemStopAction.None;
                currentActiveEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            currentActiveEffect = newEffect;
            if (!currentActiveEffect.isPlaying)
            {
                var main = currentActiveEffect.main;
                main.stopAction = ParticleSystemStopAction.None;
                currentActiveEffect.Play();
            }
        }
    }

    private ParticleSystem DetermineEffectBasedOnHealth()
    {
        switch (currentState)
        {
            case HealthState.Normal:
                return normalSmokeEffect;
            case HealthState.Damaged:
                return damagedSmokeEffect;
            case HealthState.Critical:
                return criticalSmokeEffect;
            default:
                return normalSmokeEffect;
        }
    }

    public void TakeDamage(int amount)
    {
        UpdateHealth(health - amount);
    }

    public void HealHealth(int amount)
    {
        UpdateHealth(health + amount);
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20), $"Health: {health} (State: {currentState})");
    }
}
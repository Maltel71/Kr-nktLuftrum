using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeEffectScript : MonoBehaviour
{
    public ParticleSystem normalSmokeEffect; // Normal rök particle
    public ParticleSystem damagedSmokeEffect; // Skadad rök particle
    public ParticleSystem criticalSmokeEffect; // Kritisk rök particle
    public int health = 100; // Hälsovärde

    void Start()
    {
        Debug.Log("SmokeEffectScript started.");

        if (normalSmokeEffect != null)
        {
            Debug.Log("Starting normal smoke effect.");
            normalSmokeEffect.Play();
        }
        else
        {
            Debug.LogError("Normal smoke effect is not assigned.");
        }
    }

    void Update()
    {
        // Uppdaterar motorrök baserat på hälsa
        UpdateSmoke();
    }

    void UpdateSmoke()
    {
        // Stoppa alla partikelsystem först
        if (normalSmokeEffect != null) normalSmokeEffect.Stop();
        if (damagedSmokeEffect != null) damagedSmokeEffect.Stop();
        if (criticalSmokeEffect != null) criticalSmokeEffect.Stop();

        // Kontrollera hälsa och spela rätt partikelsystem
        if (health > 50)
        {
            if (normalSmokeEffect != null)
            {
                normalSmokeEffect.Play();
            }
        }
        else if (health > 20)
        {
            if (damagedSmokeEffect != null)
            {
                damagedSmokeEffect.Play();
            }
        }
        else
        {
            if (criticalSmokeEffect != null)
            {
                criticalSmokeEffect.Play();
            }
        }
    }
}

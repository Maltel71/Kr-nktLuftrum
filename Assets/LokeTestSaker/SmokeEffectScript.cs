using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeEffectScript : MonoBehaviour
{
    public ParticleSystem normalSmokeEffect; // Normal r�k particle
    public ParticleSystem damagedSmokeEffect; // Skadad r�k particle
    public ParticleSystem criticalSmokeEffect; // Kritisk r�k particle
    public int health = 100; // H�lsov�rde

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
        // Uppdaterar motorr�k baserat p� h�lsa
        UpdateSmoke();
    }

    void UpdateSmoke()
    {
        // Stoppa alla partikelsystem f�rst
        if (normalSmokeEffect != null) normalSmokeEffect.Stop();
        if (damagedSmokeEffect != null) damagedSmokeEffect.Stop();
        if (criticalSmokeEffect != null) criticalSmokeEffect.Stop();

        // Kontrollera h�lsa och spela r�tt partikelsystem
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

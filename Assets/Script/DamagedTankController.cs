using UnityEngine;
using System.Collections;

public class DamagedTankController : MonoBehaviour
{
    [Header("Damage Effect Settings")]
    [SerializeField] private GameObject smokePrefab; // Rökprefab för skadeeffekt
    [SerializeField] private GameObject firePrefab; // Brandens prefab

    [Header("Burning Effect Settings")]
    [SerializeField] private float smokeIntensity = 1f;
    [SerializeField] private float fireIntensity = 1f;

    [Header("Randomization Settings")]
    [SerializeField] private float smokeRandomDelay = 0.5f; // Slumpmässig fördröjning mellan rökpulser
    [SerializeField] private float fireRandomDelay = 0.5f; // Slumpmässig fördröjning mellan eldpulser

    [Header("Damage Particle Points")]
    [SerializeField] private Transform[] smokeDamagePoints; // Punkter för rök
    [SerializeField] private Transform[] fireDamagePoints; // Punkter för eld

    private GameObject[] currentSmokeEmitters;
    private GameObject[] currentFireEmitters;

    private void Start()
    {
        // Initiera arrayer baserat på antalet punkter
        currentSmokeEmitters = new GameObject[smokeDamagePoints.Length];
        currentFireEmitters = new GameObject[fireDamagePoints.Length];

        // Starta effekterna
        StartCoroutine(CreateContinuousSmokeEffect());
        StartCoroutine(CreateContinuousFireEffect());
    }

    private IEnumerator CreateContinuousSmokeEffect()
    {
        while (true)
        {
            // Skapa rök vid varje punkt
            for (int i = 0; i < smokeDamagePoints.Length; i++)
            {
                // Destroya tidigare rök vid denna punkt
                if (currentSmokeEmitters[i] != null)
                {
                    Destroy(currentSmokeEmitters[i]);
                }

                // Skapa ny rök
                if (smokePrefab != null && smokeDamagePoints[i] != null)
                {
                    currentSmokeEmitters[i] = Instantiate(smokePrefab, smokeDamagePoints[i].position, Quaternion.identity, transform);

                    // Justera rökintensitet
                    var particleSystems = currentSmokeEmitters[i].GetComponentsInChildren<ParticleSystem>();
                    foreach (var ps in particleSystems)
                    {
                        var main = ps.main;
                        main.startLifetime = main.startLifetime.constant * smokeIntensity;
                        main.startSize = main.startSize.constant * smokeIntensity;
                        main.startSpeed = main.startSpeed.constant * smokeIntensity;
                    }
                }
            }

            // Vänta en slumpmässig tid
            yield return new WaitForSeconds(Random.Range(1f, 1f + smokeRandomDelay));
        }
    }

    private IEnumerator CreateContinuousFireEffect()
    {
        while (true)
        {
            // Skapa eld vid varje punkt
            for (int i = 0; i < fireDamagePoints.Length; i++)
            {
                // Destroya tidigare eld vid denna punkt
                if (currentFireEmitters[i] != null)
                {
                    Destroy(currentFireEmitters[i]);
                }

                // Skapa ny eld
                if (firePrefab != null && fireDamagePoints[i] != null)
                {
                    currentFireEmitters[i] = Instantiate(firePrefab, fireDamagePoints[i].position, Quaternion.identity, transform);

                    // Justera eldintensitet
                    var particleSystems = currentFireEmitters[i].GetComponentsInChildren<ParticleSystem>();
                    foreach (var ps in particleSystems)
                    {
                        var main = ps.main;
                        main.startLifetime = main.startLifetime.constant * fireIntensity;
                        main.startSize = main.startSize.constant * fireIntensity;
                        main.startSpeed = main.startSpeed.constant * fireIntensity;
                    }
                }
            }

            // Vänta en slumpmässig tid
            yield return new WaitForSeconds(Random.Range(1f, 1f + fireRandomDelay));
        }
    }

    private void OnDestroy()
    {
        // Rensa upp alla effekter när objektet förstörs
        foreach (var smokeEmitter in currentSmokeEmitters)
        {
            if (smokeEmitter != null)
                Destroy(smokeEmitter);
        }

        foreach (var fireEmitter in currentFireEmitters)
        {
            if (fireEmitter != null)
                Destroy(fireEmitter);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingScript : MonoBehaviour
{
    public GameObject bulletPrefab; // Prefab för skott
    public Transform bulletSpawnPoint; // Plats där skottet spawnas
    public float bulletSpeed = 100f; // Hastighet på skottet
    public ParticleSystem particleEffect; // ParticleEffect för att skjuta
    public AudioSource shootingSound; // Ljud för att skjuta

    void Update()
    {
        // Skjuter skott när 'E' trycks ned eller pekskärm inom område
        if (Input.GetKeyDown(KeyCode.E) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // Spela upp skjutljud
        shootingSound.Play();

        // Aktivera particle system
        particleEffect.Play();

        // Skapa skott utan att sätta hastighet
        Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
    }
}

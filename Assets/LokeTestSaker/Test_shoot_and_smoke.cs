using UnityEngine;

public class NewEmptyCSharpScript : MonoBehaviour
{
    public GameObject bulletPrefab; // Prefab f�r skott
    public Transform bulletSpawnPoint; // Plats d�r skottet spawnas
    //public float bulletSpeed = 100f; // Hastighet p� skottet
    public ParticleSystem particleEffect; // ParticleEffect f�r att skjuta
    public AudioSource shootingSound; // Ljud f�r att skjuta
    public ParticleSystem normalSmokeEffect; // Normal r�k particle
    public ParticleSystem damagedSmokeEffect; // Skadad r�k particle
    public ParticleSystem criticalSmokeEffect; // Kritisk r�k particle
    public int health = 100; // H�lsov�rde

    void Start()
    {
        // Börja med att spela den normala rökeffekten
        normalSmokeEffect.Play();
    }

    void Update()
    {
        // Skjuter skott n�r 'E' trycks ned eller peksk�rm inom omr�de
        if (Input.GetKeyDown(KeyCode.E) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Shoot();
        }

        // Uppdaterar motorr�k baserat p� h�lsa
        UpdateSmoke();
    }

    void Shoot()
    {
        // Spela upp skjutljud
        shootingSound.Play();

        // Aktivera particle system
        particleEffect.Play();

        // Skapa skott och s�tt hastighet
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        //Rigidbody rb = bullet.GetComponent<Rigidbody>();
        //rb.linearVelocity = bulletSpawnPoint.forward * bulletSpeed;
    }

    void UpdateSmoke()
    {
        // Stoppa alla partikelsystem f�rst
        normalSmokeEffect.Stop();
        damagedSmokeEffect.Stop();
        criticalSmokeEffect.Stop();

        // Kontrollera h�lsa och spela r�tt partikelsystem
        if (health > 50)
        {
            normalSmokeEffect.Play();
        }
        else if (health > 20)
        {
            damagedSmokeEffect.Play();
        }
        else
        {
            criticalSmokeEffect.Play();
        }
    }
}

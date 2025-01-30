using UnityEngine;

public class BombHandler : MonoBehaviour
{
    [Header("Audio")]
    private AudioManager audioManager;

    [Header("Effects")]
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private float explosionDuration = 2f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionForce = 500f;
    [SerializeField] private float explosionDamage = 50f;

    private CameraShake cameraShake;
    private bool hasPlayedFallingSound = false;

    private void Start()
    {
        audioManager = AudioManager.Instance;
        cameraShake = CameraShake.Instance;

        if (cameraShake == null)
        {
            Debug.LogWarning("Kunde inte hitta CameraShake!");
        }
    }

    private void Update()
    {
        if (!hasPlayedFallingSound && GetComponent<Rigidbody>().linearVelocity.y < 0)
        {
            audioManager?.PlayBombFallingSound();
            hasPlayedFallingSound = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Bomb kolliderade med: {collision.gameObject.name}");

        // Skapa explosionseffekt
        if (explosionEffectPrefab != null)
        {
            GameObject explosionEffect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(explosionEffect, explosionDuration);
        }

        // Hitta alla närliggande objekt och påverka dem med explosionen
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            // Påverka rigidbodies med explosionskraft
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1.0f, ForceMode.Impulse);
            }

            // Hantera skada på fiender
            if (hit.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    float distance = Vector3.Distance(transform.position, hit.transform.position);
                    float damageMultiplier = 1 - (distance / explosionRadius);
                    float damage = explosionDamage * damageMultiplier;
                    enemyHealth.TakeDamage(damage);
                }
            }
        }

        // Aktivera kameraskakning
        if (cameraShake != null)
        {
            Debug.Log("Aktiverar kameraskakning för bomb");
            cameraShake.ShakaCameraVidBomb();
        }

        // Spela explosionsljud
        audioManager?.PlayBombExplosionSound();

        // Förstör bomben
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Rita explosionsradien i editorn
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
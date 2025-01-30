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
    private ScoreManager scoreManager;
    private bool hasPlayedFallingSound = false;
    private bool hasExploded = false;

    private void Start()
    {
        audioManager = AudioManager.Instance;
        cameraShake = CameraShake.Instance;
        scoreManager = ScoreManager.Instance;
    }

    private void Update()
    {
        if (!hasPlayedFallingSound && GetComponent<Rigidbody>().linearVelocity.y < 0)
        {
            audioManager?.PlayBombFallingSound();
            hasPlayedFallingSound = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;

        Debug.Log($"OnTriggerEnter: Collided with {other.gameObject.name}, tag: {other.gameObject.tag}");

        if (other.CompareTag("BombTarget"))
        {
            Debug.Log("Hit BombTarget! Adding points...");
            if (scoreManager != null)
            {
                scoreManager.AddBombTargetPoints();
            }
            // Använd bombens position för explosionen vid trigger
            HandleExplosion(transform.position);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;

        Debug.Log($"OnCollisionEnter: Collided with {collision.gameObject.name}, tag: {collision.gameObject.tag}");

        if (collision.gameObject.CompareTag("BombTarget"))
        {
            if (scoreManager != null)
            {
                scoreManager.AddBombTargetPoints();
            }
        }

        // Använd kollisionspunkten för explosionen
        HandleExplosion(collision.contacts[0].point);
    }

    private void HandleExplosion(Vector3 position)
    {
        if (hasExploded) return;
        hasExploded = true;

        // Skapa explosionseffekt
        if (explosionEffectPrefab != null)
        {
            GameObject explosionEffect = Instantiate(explosionEffectPrefab, position, Quaternion.identity);
            Destroy(explosionEffect, explosionDuration);
        }

        // Hitta alla närliggande objekt och påverka dem med explosionen
        Collider[] colliders = Physics.OverlapSphere(position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            // Påverka rigidbodies med explosionskraft
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, position, explosionRadius, 1.0f, ForceMode.Impulse);
            }

            // Hantera skada på fiender
            if (hit.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    float distance = Vector3.Distance(position, hit.transform.position);
                    float damageMultiplier = 1 - (distance / explosionRadius);
                    float damage = explosionDamage * damageMultiplier;
                    enemyHealth.TakeDamage(damage);
                }
            }
        }

        // Aktivera kameraskakning
        if (cameraShake != null)
        {
            cameraShake.ShakaCameraVidBomb();
        }

        // Spela explosionsljud
        audioManager?.PlayBombExplosionSound();

        // Förstör bomben
        Destroy(gameObject);
    }
}
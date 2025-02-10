using UnityEngine;

public class MissilePlayer : MonoBehaviour
{
    [Header("Missile Settings")]
    [SerializeField] private float speed = 50f;
    [SerializeField] private float damage = 50f;
    [SerializeField] private float missileLifetime = 3f;

    [Header("Effects")]
    [SerializeField] private GameObject explosionEffectPrefab;

    private Rigidbody rb;
    private float fixedHeight;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Spara startpositionen Y-värde
        fixedHeight = transform.position.y;

        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
            rb.useGravity = false;
            rb.freezeRotation = true;
        }

        Destroy(gameObject, missileLifetime);
    }

    private void Update()
    {
        // Behåll samma höjd som vi startade på
        if (rb != null)
        {
            Vector3 pos = transform.position;
            pos.y = fixedHeight;
            transform.position = pos;

            // Uppdatera hastigheten för att behålla konstant fart framåt
            rb.linearVelocity = transform.forward * speed;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Missile hit: {collision.gameObject.name} with tag: {collision.gameObject.tag}");

        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (collision.gameObject.TryGetComponent<EnemyHealth>(out var enemyHealth))
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log($"Enemy hit! Dealing {damage} damage");
            }

            if (explosionEffectPrefab != null)
            {
                Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            }

            AudioManager.Instance?.PlayBombSound(BombSoundType.Explosion);
        }

        Destroy(gameObject);
    }
}
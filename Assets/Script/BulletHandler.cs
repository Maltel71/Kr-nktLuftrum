using UnityEngine;

public class BulletHandler : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private bool isEnemyProjectile;
    [SerializeField] private float damage = 10f;  // Flyttad från ProjectileDamage
    [SerializeField] private float bulletLifetime = 3f;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float effectDuration = 1f;

    private bool hasCollided;
    private AudioManager audioManager;

    private void Start()
    {
        audioManager = AudioManager.Instance;
        Destroy(gameObject, bulletLifetime);
    }

    public void SetAsEnemyProjectile(float damageAmount)
    {
        isEnemyProjectile = true;
        damage = damageAmount;
    }

    // Ny funktion för att sätta skada (kan vara användbar för power-ups etc)
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasCollided) return;
        hasCollided = true;

        // Handle player bullet hitting enemy
        if (!isEnemyProjectile && other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<EnemyHealth>(out var enemyHealth))
            {
                enemyHealth.TakeDamage(damage);
                PlayHitEffect();
                AudioManager.Instance?.PlayCombatSound(CombatSoundType.Hit);
            }
        }
        // Handle enemy bullet hitting player
        else if (isEnemyProjectile && other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlaneHealthSystem>(out var playerHealth))
            {
                playerHealth.TakeDamage(damage);
                PlayHitEffect();
                AudioManager.Instance?.PlayCombatSound(CombatSoundType.Hit);
            }
        }
        // Handle hitting any other collider
        else if (!other.CompareTag("Bullet"))
        {
            PlayHitEffect();
        }

        Destroy(gameObject);
    }

    private void PlayHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }
    }
}
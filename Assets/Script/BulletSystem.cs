using UnityEngine;

public class BulletSystem : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private bool isEnemyProjectile;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float speed = 1000f;
    [SerializeField] private float bulletLifetime = 3f;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float effectDuration = 1f;

    private Vector3 direction;
    private float fixedHeight;
    private bool hasCollided;
    private AudioManager audioManager;

    private void Start()
    {
        audioManager = AudioManager.Instance;
        fixedHeight = transform.position.y;
        Destroy(gameObject, bulletLifetime);

        //Debug.Log($"Bullet initialized: Enemy={isEnemyProjectile}, Tag={gameObject.tag}, Layer={gameObject.layer}");
        Collider bulletCollider = GetComponent<Collider>();
        //Debug.Log($"Bullet Collider: IsTrigger={bulletCollider?.isTrigger}");
    }

    private void Update()
    {
        if (hasCollided) return;
        Vector3 movement = direction * speed * Time.deltaTime;
        Vector3 newPosition = transform.position + movement;
        newPosition.y = fixedHeight;
        transform.position = newPosition;
    }

    public void Initialize(Vector3 shootDirection, bool isEnemy = false, float damageAmount = 10f)
    {
        direction = shootDirection;
        isEnemyProjectile = isEnemy;
        damage = damageAmount;
        gameObject.tag = isEnemy ? "Enemy Bullet" : "Player Bullet";
        //Debug.Log($"Bullet {gameObject.name} initialized. Direction={direction}, IsEnemy={isEnemyProjectile}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasCollided) return;

        // Skip bullet-bullet collisions
        if (isEnemyProjectile && other.CompareTag("Enemy") ||
            other.CompareTag("Enemy Bullet"))
            return;

        // Skip player bullet-player collisions    
        if (!isEnemyProjectile && other.CompareTag("Player") ||
            other.CompareTag("Player Bullet"))
            return;

        //Debug.Log($"Bullet collision: Me={gameObject.tag}, Other={other.tag}");

        // Enemy bullet hits player
        if (isEnemyProjectile && other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlaneHealthSystem>(out var playerHealth) && !playerHealth.IsDead())
            {
                playerHealth.TakeDamage(damage);
                PlayHitEffect();
                audioManager?.PlayCombatSound(CombatSoundType.Hit);
            }
        }
        // Player bullet hits enemy
        else if (!isEnemyProjectile && other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<EnemyHealth>(out var enemyHealth))
            {
                enemyHealth.TakeDamage(damage);
                PlayHitEffect();
                audioManager?.PlayCombatSound(CombatSoundType.Hit);
                //Debug.Log($"Enemy hit with {damage} damage!");
            }
        }

        hasCollided = true;
        Destroy(gameObject);
    }

    private void PlayHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            var effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }
    }
}
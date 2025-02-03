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

    public Vector3 direction;
    private float fixedHeight;
    private Vector3 lastPosition;
    private float distanceTraveled;
    private bool hasCollided;
    private AudioManager audioManager;

    private void Start()
    {
        audioManager = AudioManager.Instance;
        fixedHeight = transform.position.y;
        lastPosition = transform.position;
        Destroy(gameObject, bulletLifetime);
    }

    private void Update()
    {
        if (hasCollided) return;

        // Uppdatera position
        Vector3 movement = direction * speed * Time.deltaTime;
        Vector3 newPosition = transform.position + movement;
        newPosition.y = fixedHeight;
        transform.position = newPosition;

        // Beräkna tillryggalagd sträcka
        float frameDistance = Vector3.Distance(lastPosition, transform.position);
        distanceTraveled += frameDistance;

        // Debug-visualisering
        Debug.DrawLine(lastPosition, transform.position, Color.yellow, 0.1f);
        lastPosition = transform.position;
    }

    public void Initialize(Vector3 shootDirection, bool isEnemy = false, float damageAmount = 10f)
    {
        direction = shootDirection;
        isEnemyProjectile = isEnemy;
        damage = damageAmount;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasCollided) return;
        hasCollided = true;

        // Hantera träff på spelare
        if (isEnemyProjectile && other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlaneHealthSystem>(out var playerHealth))
            {
                playerHealth.TakeDamage(damage);
                PlayHitEffect();
                audioManager?.PlayCombatSound(CombatSoundType.Hit);
            }
        }
        // Hantera träff på fiende
        else if (!isEnemyProjectile && other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<EnemyHealth>(out var enemyHealth))
            {
                enemyHealth.TakeDamage(damage);
                PlayHitEffect();
                audioManager?.PlayCombatSound(CombatSoundType.Hit);
            }
        }
        // Hantera träff på andra objekt
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
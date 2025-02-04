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

        Collider bulletCollider = GetComponent<Collider>();
        //Debug.Log($"Bullet Collider finns: {bulletCollider != null}, Är Trigger: {bulletCollider?.isTrigger}");

        Component[] components = GetComponents<Component>();
        //Debug.Log($"Components on Bullet Object ({gameObject.name}):");
        foreach (Component comp in components)
        {
            //Debug.Log(comp.GetType().Name);
        }
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
        //Debug.DrawLine(lastPosition, transform.position, Color.yellow, 0.1f);
        lastPosition = transform.position;
    }

    public void Initialize(Vector3 shootDirection, bool isEnemy = false, float damageAmount = 10f)
    {
        direction = shootDirection;
        isEnemyProjectile = isEnemy;
        damage = damageAmount;
        //Debug.Log($"Bullet {gameObject.name} initialized. Direction: {direction}, IsEnemy: {isEnemyProjectile}, Damage: {damage}");
    }

    private void OnTriggerEnter(Collider otherCollider)
    {
        if (hasCollided) return;
        hasCollided = true;

        // Hantera träff på spelare
        if (isEnemyProjectile && otherCollider.CompareTag("Player"))
        {
            PlaneHealthSystem playerHealth = otherCollider.gameObject.GetComponent<PlaneHealthSystem>();

            // Debug-loggning av komponenter
            Component[] components = otherCollider.GetComponents<Component>();
            Debug.Log($"Components on Player Object ({otherCollider.gameObject.name}):");
            foreach (Component comp in components)
            {
                //Debug.Log(comp.GetType().Name);
            }

            if (playerHealth != null)
            {
                //Debug.Log($"Player Health Component Found on {otherCollider.gameObject.name}. Current Health: {playerHealth.GetHealthPercentage() * 100}%");

                if (!playerHealth.IsDead())
                {
                    playerHealth.TakeDamage(damage);
                    PlayHitEffect();
                    audioManager?.PlayCombatSound(CombatSoundType.Hit);
                }
            }
            else
            {
                //Debug.LogError($"NO PlaneHealthSystem found on Player Object: {otherCollider.gameObject.name}!");
            }
        }
        // Hantera träff på fiende
        else if (!isEnemyProjectile && otherCollider.CompareTag("Enemy"))
        {
            //Debug.Log($"{gameObject.name} hit enemy: {otherCollider.gameObject.name}");
            if (otherCollider.TryGetComponent<EnemyHealth>(out var enemyHealth))
            {
                enemyHealth.TakeDamage(damage);
                PlayHitEffect();
                audioManager?.PlayCombatSound(CombatSoundType.Hit);
            }
        }
        // Hantera träff på andra objekt
        else if (!otherCollider.CompareTag("Bullet"))
        {
            //Debug.Log($"{gameObject.name} hit non-bullet object: {otherCollider.gameObject.name}");
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
            Debug.Log($"Hit effect played at {transform.position} for bullet {gameObject.name}");
        }
    }
}
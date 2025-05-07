using UnityEngine;

public class RandomEnemyWeapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private Transform[] firePoints;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float bulletDamage = 20f;
    [SerializeField] private float shootingRange = 20f;

    [Header("Shooting Behavior")]
    [SerializeField] private bool continuousShooting = false;
    [SerializeField] private float shootAngleLimit = 15f;

    [Header("Collision Settings")]
    [SerializeField] private float playerCollisionDamage = 25f;
    [SerializeField] private ExplosionType collisionExplosionType = ExplosionType.Small;

    //[Header("Destruction")]
    //[SerializeField] private float destroyDistanceBehindPlayer = 20f;

    private Transform playerTarget;
    private float nextFireTime;
    private int currentFirePoint = 0;
    private AudioManager audioManager;
    private EnemyHealth healthSystem;

    private void Start()
    {
        audioManager = AudioManager.Instance;
        healthSystem = GetComponent<EnemyHealth>();
        FindPlayer();

        // Om inga firePoints angivits, använd transform position
        if (firePoints == null || firePoints.Length == 0)
        {
            firePoints = new Transform[1] { transform };
        }
    }

    private void FindPlayer()
    {
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
            }
            else
            {
                Debug.LogWarning("Kunde inte hitta spelaren!");
            }
        }
    }

    private void Update()
    {
        if (playerTarget == null || (healthSystem != null && healthSystem.IsDying))
            return;

        // Kontrollera skjutning
        HandleShooting();

        // Kontrollera om fienden är för långt bakom spelaren
        //CheckIfBehindPlayer();
    }

    private void HandleShooting()
    {
        float distance = Vector3.Distance(transform.position, playerTarget.position);
        bool shouldShoot = continuousShooting || CanShootAtPlayer();

        if (distance <= shootingRange && Time.time >= nextFireTime && shouldShoot)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    //private void CheckIfBehindPlayer()
    //{
    //    if (playerTarget != null)
    //    {
    //        // Om fienden är tillräckligt långt bakom spelaren på z-axeln
    //        if (transform.position.z < playerTarget.position.z - destroyDistanceBehindPlayer)
    //        {
    //            Destroy(gameObject);
    //        }
    //    }
    //}

    private bool CanShootAtPlayer()
    {
        if (playerTarget == null) return false;

        Vector3 directionToTarget = playerTarget.position - transform.position;
        float angle = Vector3.Angle(transform.forward, directionToTarget);
        return angle < shootAngleLimit;
    }

    private void Shoot()
    {
        if (firePoints.Length == 0) return;

        Transform currentPoint = firePoints[currentFirePoint];
        Vector3 shootDirection = transform.forward;

        // Använd bullet pool
        GameObject bullet = BulletPool.Instance.GetBullet(false); // false för fiendeskott
        bullet.transform.position = currentPoint.position;
        bullet.transform.rotation = Quaternion.LookRotation(shootDirection);

        if (bullet.TryGetComponent<BulletSystem>(out var bulletSystem))
        {
            bulletSystem.Initialize(shootDirection, true, bulletDamage);
        }

        audioManager?.PlayEnemyShootSound();

        // Gå till nästa skjutpunkt i sekvens
        currentFirePoint = (currentFirePoint + 1) % firePoints.Length;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent<PlaneHealthSystem>(out var playerHealth))
            {
                // Skada spelaren
                playerHealth.TakeDamage(playerCollisionDamage);

                // Spela hit-ljud
                AudioManager.Instance?.PlayCombatSound(CombatSoundType.Hit);

                // Skapa explosion
                GameObject explosion = ExplosionPool.Instance.GetExplosion(collisionExplosionType);
                explosion.transform.position = collision.contacts[0].point;
                ExplosionPool.Instance.ReturnExplosionToPool(explosion, 2f);

                // Lägg till kameraskakning
                CameraShake.Instance?.ShakaCameraVidBomb();

                // Starta dödssekvens för fienden
                if (healthSystem != null)
                {
                    healthSystem.StartDying();
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
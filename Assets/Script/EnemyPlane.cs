using UnityEngine;

public class EnemyPlane : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform playerPlane;
    [SerializeField] private float shootingRange = 100f;  // Ökad range för bättre gameplay
    [SerializeField] private float minShootingRange = 5f;

    [Header("Weapon Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform leftGun;
    [SerializeField] private Transform rightGun;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float bulletSpeed = 200f;     // Ökad hastighet för bättre träffsäkerhet
    [SerializeField] private float bulletDamage = 10f;
    [SerializeField] private float bulletLifetime = 3f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;   // Satt till true som default för att lättare kunna debugga

    private float nextFireTime;
    private bool useLeftGun = true;
    private AudioManager audioManager;

    private void Start()
    {
        audioManager = AudioManager.Instance;
        FindPlayer();

        // Visa varning om något saknas
        if (bulletPrefab == null) Debug.LogError($"Bullet Prefab saknas på {gameObject.name}");
        if (leftGun == null) Debug.LogError($"Left Gun saknas på {gameObject.name}");
        if (rightGun == null) Debug.LogError($"Right Gun saknas på {gameObject.name}");
    }

    private void Update()
    {
        if (playerPlane == null)
        {
            FindPlayer();
            return;
        }

        HandleShooting();
    }

    private void FindPlayer()
    {
        if (playerPlane == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerPlane = player.transform;
                Debug.Log($"Hittade spelaren: {player.name}");
            }
        }
    }

    private void HandleShooting()
    {
        if (!CanShoot()) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerPlane.position);

        if (distanceToPlayer <= shootingRange && distanceToPlayer >= minShootingRange)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
            useLeftGun = !useLeftGun;
        }
    }

    private bool CanShoot()
    {
        return Time.time >= nextFireTime &&
               leftGun != null &&
               rightGun != null &&
               bulletPrefab != null;
    }

    private void Shoot()
    {
        Transform currentGun = useLeftGun ? leftGun : rightGun;
        Vector3 directionToPlayer = (playerPlane.position - currentGun.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, currentGun.position, Quaternion.identity);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb == null)
            rb = bullet.AddComponent<Rigidbody>();

        // Nollställ alla constraints och inställningar
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.freezeRotation = true;  // Bara frys rotation, inte position
        rb.constraints = RigidbodyConstraints.FreezeRotation;  // Bara frys rotation
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Multiplicera hastigheten med bulletSpeed
        Vector3 bulletVelocity = directionToPlayer * bulletSpeed;
        rb.linearVelocity = bulletVelocity;  // Använd velocity istället för linearVelocity

        var bulletHandler = bullet.GetComponent<BulletHandler>();
        if (bulletHandler == null)
            bulletHandler = bullet.AddComponent<BulletHandler>();
        bulletHandler.SetAsEnemyProjectile(bulletDamage);

        audioManager?.PlayShootSound();
        Destroy(bullet, bulletLifetime);
    }

    private void OnDrawGizmos()
    {
        if (!showDebugInfo) return;

        // Rita skjutavstånd
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        // Rita min-avstånd
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minShootingRange);

        // Rita skjutriktning om spelaren finns
        if (playerPlane != null)
        {
            Gizmos.color = Color.blue;
            Vector3 directionToPlayer = (playerPlane.position - transform.position).normalized;
            Gizmos.DrawRay(transform.position, directionToPlayer * 10f);
        }
    }
}
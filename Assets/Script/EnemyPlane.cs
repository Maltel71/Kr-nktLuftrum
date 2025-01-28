using UnityEngine;

public class EnemyPlane : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform playerPlane;
    [SerializeField] private float shootingRange = 15f;
    [SerializeField] private float minShootingRange = 5f; // Minimum avstånd för att skjuta

    [Header("Weapon Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform leftGun;
    [SerializeField] private Transform rightGun;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletDamage = 10f;
    [SerializeField] private float bulletLifetime = 3f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    private float nextFireTime;
    private bool useLeftGun = true;
    private AudioManager audioManager;
    private bool isInitialized = false;

    private void Start()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        audioManager = AudioManager.Instance;
        FindPlayer();
        ValidateComponents();
        isInitialized = true;
    }

    private void FindPlayer()
    {
        if (playerPlane == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerPlane = player.transform;
                if (showDebugInfo) Debug.Log($"Found player: {player.name}");
            }
            else if (showDebugInfo)
            {
                Debug.LogWarning("No player found with Player tag!");
            }
        }
    }

    private void ValidateComponents()
    {
        if (bulletPrefab == null) Debug.LogError($"Missing bulletPrefab on {gameObject.name}");
        if (leftGun == null) Debug.LogError($"Missing leftGun on {gameObject.name}");
        if (rightGun == null) Debug.LogError($"Missing rightGun on {gameObject.name}");
    }

    private void Update()
    {
        if (!isInitialized) return;

        if (playerPlane == null)
        {
            FindPlayer();
            return;
        }

        HandleShooting();
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

            if (showDebugInfo)
            {
                Debug.Log($"Shooting at player. Distance: {distanceToPlayer}");
            }
        }
    }

    private bool CanShoot()
    {
        bool timeCheck = Time.time >= nextFireTime;
        bool gunsCheck = leftGun != null && rightGun != null;
        bool prefabCheck = bulletPrefab != null;

        //if (showDebugInfo && !timeCheck) Debug.Log("Waiting for fire rate cooldown");
        if (showDebugInfo && !gunsCheck) Debug.Log("Missing gun references");
        if (showDebugInfo && !prefabCheck) Debug.Log("Missing bullet prefab");

        return timeCheck && gunsCheck && prefabCheck;
    }

    private void Shoot()
    {
        Transform currentGun = useLeftGun ? leftGun : rightGun;
        Vector3 directionToPlayer = (playerPlane.position - currentGun.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, currentGun.position, Quaternion.LookRotation(directionToPlayer));

        ConfigureBullet(bullet, directionToPlayer);
        SetupBulletComponents(bullet);

        audioManager?.PlayShootSound();
        Destroy(bullet, bulletLifetime);
    }

    private void ConfigureBullet(GameObject bullet, Vector3 direction)
    {
        if (bullet.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.useGravity = false;
            rb.linearVelocity = direction * bulletSpeed;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        else if (showDebugInfo)
        {
            Debug.LogWarning("Bullet missing Rigidbody component!");
        }
    }

    private void SetupBulletComponents(GameObject bullet)
    {
        var bulletHandler = bullet.GetComponent<BulletHandler>();
        if (bulletHandler == null)
        {
            bulletHandler = bullet.AddComponent<BulletHandler>();
        }
        bulletHandler.SetAsEnemyProjectile(bulletDamage);
    }

    // Public metoder för extern kontroll
    public void SetShootingRange(float range)
    {
        shootingRange = Mathf.Max(range, minShootingRange);
    }

    public void SetFireRate(float rate)
    {
        fireRate = Mathf.Max(0.1f, rate);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugInfo) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minShootingRange);
    }
}
using UnityEngine;

public enum ShootingPattern
{
    Alternate,    // Växla mellan vänster/höger
    Simultaneous, // Skjut från båda samtidigt 
    Random       // Slumpmässig kanon
}

public class EnemyPlane : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform playerPlane;
    [SerializeField] private float shootingRange = 1500f;  // Ökad range för bättre gameplay
    [SerializeField] private float minShootingRange = 5f;

    [Header("Weapon Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform leftGun;
    [SerializeField] private Transform rightGun;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float bulletSpeed = 200f;     // Ökad hastighet för bättre träffsäkerhet
    [SerializeField] private float bulletDamage = 10f;
    [SerializeField] private float bulletLifetime = 3f;
    [SerializeField] private ShootingPattern shootPattern = ShootingPattern.Alternate;

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
        if (!CanShoot())
        {
            Debug.Log("Kan inte skjuta: CanShoot returnerade false");
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerPlane.position);
        Debug.Log($"Avstånd till spelare: {distanceToPlayer}, ShootingRange: {shootingRange}");

        if (distanceToPlayer <= shootingRange && distanceToPlayer >= minShootingRange)
        {
            Debug.Log("Försöker skjuta");
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private bool CanShoot()
    {
        if (Time.time < nextFireTime)
        {
            Debug.Log("Väntar på fireRate");
            return false;
        }
        if (leftGun == null || rightGun == null)
        {
            Debug.Log("Saknar gun references");
            return false;
        }
        if (bulletPrefab == null)
        {
            Debug.Log("Saknar bulletPrefab");
            return false;
        }
        return true;
    }

    private void Shoot()
    {
        switch (shootPattern)
        {
            case ShootingPattern.Simultaneous:
                FireFromGun(leftGun);
                FireFromGun(rightGun);
                break;

            case ShootingPattern.Random:
                FireFromGun(Random.value > 0.5f ? leftGun : rightGun);
                break;

            case ShootingPattern.Alternate:
            default:
                FireFromGun(useLeftGun ? leftGun : rightGun);
                useLeftGun = !useLeftGun;
                break;
        }

        nextFireTime = Time.time + fireRate;
    }

    private void FireFromGun(Transform gunPoint)
    {
        // Debug höjdskillnad
        float heightDifference = playerPlane.position.y - gunPoint.position.y;
        Debug.Log($"Height difference to player: {heightDifference}");

        // Skapa målposition som är spelarens faktiska position
        Vector3 targetPosition = playerPlane.position;
        Vector3 directionToPlayer = (targetPosition - gunPoint.position).normalized;

        // Debug riktning
        Debug.Log($"Shooting direction: {directionToPlayer}");
        Debug.DrawRay(gunPoint.position, directionToPlayer * 10f, Color.red, 1f);

        GameObject bullet = Instantiate(bulletPrefab, gunPoint.position, Quaternion.LookRotation(directionToPlayer));
        SetupBullet(bullet, directionToPlayer);
    }

    private void SetupBullet(GameObject bullet, Vector3 direction)
    {
        Rigidbody rb = bullet.GetComponent<Rigidbody>() ?? bullet.AddComponent<Rigidbody>();

        // Grundläggande inställningar
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Sätt hastighet och riktning
        rb.linearVelocity = direction * bulletSpeed;
        Debug.Log($"Bullet velocity set to: {rb.linearVelocity}");

        // Sätt rotation för att matcha rörelseriktningen
        bullet.transform.forward = direction;

        // Bullet handler setup
        var bulletHandler = bullet.GetComponent<BulletHandler>() ?? bullet.AddComponent<BulletHandler>();
        bulletHandler.SetAsEnemyProjectile(bulletDamage);

        // Kontrollera collider
        if (bullet.GetComponent<Collider>() == null)
        {
            var collider = bullet.AddComponent<SphereCollider>();
            collider.isTrigger = true;
        }

        // Debug
        Debug.DrawRay(bullet.transform.position, direction * 10f, Color.red, 2f);
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
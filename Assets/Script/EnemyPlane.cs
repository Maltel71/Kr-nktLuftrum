using UnityEngine;

public class EnemyPlane : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform playerPlane;
    [SerializeField] private float shootingRange = 1000f;

    [Header("Weapon Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform leftGun;
    [SerializeField] private Transform rightGun;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float bulletDamage = 10f;
    [SerializeField] private float bulletSpawnOffset = 5f;

    [Header("Collision Settings")]
    [SerializeField] private float collisionDamage = 25f;
    [SerializeField] private bool destroyOnCollision = true;

    private float nextFireTime;
    private bool useLeftGun = true;
    private AudioManager audioManager;

    private void Start()
    {
        audioManager = AudioManager.Instance;
        if (playerPlane == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerPlane = player.transform;
            }
        }



        // Debug komponentinfo
        Collider col = GetComponent<Collider>();
        Rigidbody rb = GetComponent<Rigidbody>();
        Debug.Log($"Fiende {gameObject.name} startar:");
        Debug.Log($"- Collider: {(col != null ? "JA" : "NEJ")}");
        Debug.Log($"- Collider är Trigger: {(col?.isTrigger == true ? "JA" : "NEJ")}");
        Debug.Log($"- Rigidbody: {(rb != null ? "JA" : "NEJ")}");
        Debug.Log($"- Rigidbody är Kinematic: {(rb?.isKinematic == true ? "JA" : "NEJ")}");
        Debug.Log($"Fiende {gameObject.name} Layer: {LayerMask.LayerToName(gameObject.layer)}");
        Debug.Log($"Fiende {gameObject.name} Tag: {gameObject.tag}");
    }

    private void Update()
    {
        if (playerPlane == null) return;
        float distance = Vector3.Distance(transform.position, playerPlane.position);
        if (distance <= shootingRange && CanShoot())
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
            useLeftGun = !useLeftGun;
        }
    }

    private bool CanShoot()
    {
        return Time.time >= nextFireTime && leftGun != null && rightGun != null && bulletPrefab != null;
    }

    private void Shoot()
    {
        Transform currentGun = useLeftGun ? leftGun : rightGun;
        Vector3 shootDirection = Vector3.forward * -1;
        Vector3 spawnPosition = currentGun.position + shootDirection * bulletSpawnOffset;

        Debug.DrawLine(currentGun.position, spawnPosition, Color.red, 1f);
        Debug.DrawRay(spawnPosition, shootDirection * 20f, Color.yellow, 1f);

        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.LookRotation(shootDirection));

        var bulletMover = bullet.GetComponent<BulletMover>();
        if (bulletMover != null)
        {
            bulletMover.direction = shootDirection;
        }

        var bulletHandler = bullet.GetComponent<BulletHandler>() ?? bullet.AddComponent<BulletHandler>();
        bulletHandler.SetAsEnemyProjectile(bulletDamage);

        audioManager?.PlayShootSound();
        Destroy(bullet, 3f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"FIENDE {gameObject.name} kolliderade med: {collision.gameObject.name}");
        Debug.Log($"- Layer: {LayerMask.LayerToName(collision.gameObject.layer)}");
        Debug.Log($"- Tag: {collision.gameObject.tag}");

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log($"Fiende {gameObject.name} träffade spelaren!");

            var playerHealth = collision.gameObject.GetComponent<PlaneHealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(collisionDamage);
                Debug.Log($"Gjorde {collisionDamage} skada på spelaren");

                AudioManager.Instance?.PlayCombatSound(CombatSoundType.Hit);

                if (destroyOnCollision)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.LogWarning("Hittade inte PlaneHealthSystem på spelaren!");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        Gizmos.color = Color.blue;
        if (leftGun != null)
            Gizmos.DrawRay(leftGun.position, Vector3.forward * -20f);
        if (rightGun != null)
            Gizmos.DrawRay(rightGun.position, Vector3.forward * -20f);
    }
}
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

        // Skjut alltid i negativ Z-riktning (framåt i din värld)
        Vector3 shootDirection = Vector3.forward * -1;

        // Spawn position framför vapnet
        Vector3 spawnPosition = currentGun.position + shootDirection * bulletSpawnOffset;

        // Debug-ritning
        Debug.DrawLine(currentGun.position, spawnPosition, Color.red, 1f);
        Debug.DrawRay(spawnPosition, shootDirection * 20f, Color.yellow, 1f);

        // Skapa kulan med rätt rotation
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.LookRotation(shootDirection));

        var bulletMover = bullet.GetComponent<BulletMover>();
        if (bulletMover != null)
        {
            bulletMover.direction = shootDirection;
            //Debug.Log($"Skjuter framåt - Position: {spawnPosition}, Riktning: {shootDirection}");
        }

        var bulletHandler = bullet.GetComponent<BulletHandler>() ?? bullet.AddComponent<BulletHandler>();
        bulletHandler.SetAsEnemyProjectile(bulletDamage);

        audioManager?.PlayShootSound();
        Destroy(bullet, 3f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        // Rita skjutriktningen
        Gizmos.color = Color.blue;
        if (leftGun != null)
            Gizmos.DrawRay(leftGun.position, Vector3.forward * -20f);
        if (rightGun != null)
            Gizmos.DrawRay(rightGun.position, Vector3.forward * -20f);
    }
}
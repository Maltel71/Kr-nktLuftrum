using UnityEngine;

public class EnemyPlane : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform playerPlane;
    [SerializeField] private float shootingRange = 15f;

    [Header("Weapon Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform leftGun;
    [SerializeField] private Transform rightGun;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletDamage = 10f;

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
        if (playerPlane == null || !CanShoot()) return;

        if (Vector3.Distance(transform.position, playerPlane.position) <= shootingRange)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
            useLeftGun = !useLeftGun;
        }
    }

    private bool CanShoot() => Time.time >= nextFireTime && leftGun != null && rightGun != null;

    private void Shoot()
    {
        Transform currentGun = useLeftGun ? leftGun : rightGun;

        // Beräkna riktning mot spelaren
        Vector3 directionToPlayer = (playerPlane.position - currentGun.position).normalized;

        // Skapa kulan och rikta den mot spelaren
        GameObject bullet = Instantiate(bulletPrefab, currentGun.position, Quaternion.LookRotation(directionToPlayer));

        if (bullet.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.useGravity = false;
            rb.linearVelocity = directionToPlayer * bulletSpeed;  // Sätt hastighet i riktning mot spelaren
        }

        // Sätt upp bullet handler
        var bulletHandler = bullet.GetComponent<BulletHandler>() ?? bullet.AddComponent<BulletHandler>();
        bulletHandler.SetAsEnemyProjectile(bulletDamage);

        audioManager?.PlayShootSound();
        Destroy(bullet, 3f);
    }
}
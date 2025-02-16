using UnityEngine;

public class EnemyPlane : MonoBehaviour
{
    [SerializeField] private Transform playerPlane;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform leftGun;
    [SerializeField] private Transform rightGun;
    [SerializeField] private float shootingRange = 1500f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletDamage = 10f;

    private float nextFireTime;
    private bool useLeftGun = true;
    private AudioManager audioManager;

    private void Start()
    {
        audioManager = AudioManager.Instance;
        playerPlane = GameObject.FindGameObjectWithTag("Player")?.transform;
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
        GameObject bullet = Instantiate(bulletPrefab, currentGun.position, Quaternion.LookRotation(Vector3.back));

        if (bullet.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.useGravity = false;
            rb.linearVelocity = Vector3.back * bulletSpeed;
        }

        var bulletHandler = bullet.GetComponent<BulletHandler>() ?? bullet.AddComponent<BulletHandler>();
        bulletHandler.SetAsEnemyProjectile(bulletDamage);

        audioManager?.PlayShootSound();
        Destroy(bullet, 3f);
    }
}
using UnityEngine;

public class EnemyPlane : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform playerPlane;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform leftGun;
    [SerializeField] private Transform rightGun;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletDamage = 10f;
    [SerializeField] private float bulletRadius = 0.5f;

    private float nextFireTime;
    private bool useLeftGun = true;
    private AudioManager audioManager;

    private void Start()
    {
        audioManager = AudioManager.Instance;
        if (playerPlane == null)
        {
            playerPlane = GameObject.FindGameObjectWithTag("Player").transform;
            Debug.Log(playerPlane != null ? "Player found!" : "Player not found!");
        }
        Debug.Log($"LeftGun assigned: {leftGun != null}");
        Debug.Log($"RightGun assigned: {rightGun != null}");
        Debug.Log($"BulletPrefab assigned: {bulletPrefab != null}");
    }

    private void Update()
    {
        if (Time.time >= nextFireTime && playerPlane != null)
        {
            Debug.Log("Attempting to shoot");
            Shoot();
            nextFireTime = Time.time + fireRate;
            useLeftGun = !useLeftGun;
        }
    }

    private void Shoot()
    {
        Transform currentGun = useLeftGun ? leftGun : rightGun;
        if (currentGun == null)
        {
            Debug.LogError("Gun reference missing!");
            return;
        }

        Vector3 direction = (playerPlane.position - currentGun.position).normalized;
        Debug.DrawRay(currentGun.position, direction * 10f, Color.red, 1f);
        Debug.Log($"Shooting direction: {direction}");

        Quaternion rotation = Quaternion.LookRotation(direction);
        GameObject bullet = Instantiate(bulletPrefab, currentGun.position, rotation);

        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = direction * bulletSpeed;
            Debug.Log($"Bullet velocity set to: {bulletRb.linearVelocity}");
        }
        else
        {
            Debug.LogError("Bullet prefab missing Rigidbody!");
        }

        bullet.AddComponent<BulletBehavior>().Initialize(bulletDamage, bulletRadius);
        audioManager?.PlayShootSound();
        Destroy(bullet, 3f);
    }
}

public class BulletBehavior : MonoBehaviour
{
    private float damage;
    private float bulletRadius;

    public void Initialize(float bulletDamage, float radius)
    {
        damage = bulletDamage;
        bulletRadius = radius;
        Debug.Log("Bullet initialized");
    }

    private void Start()
    {
        Debug.Log("Bullet created at: " + transform.position);
    }

    private void FixedUpdate()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, bulletRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                PlaneHealthSystem playerHealth = hitCollider.GetComponent<PlaneHealthSystem>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    Destroy(gameObject);
                    break;
                }
            }
        }
    }
}
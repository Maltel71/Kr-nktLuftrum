using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private Transform weaponPoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float bulletLifetime = 2f;

    [Header("Shell Settings")]
    [SerializeField] private Transform shellEjectionPoint;
    [SerializeField] private GameObject shellPrefab;
    [SerializeField] private float shellEjectionForce = 2f;
    [SerializeField] private float shellTorque = 2f;
    [SerializeField] private float shellLifetime = 3f;

    [Header("Bomb Settings")]
    [SerializeField] private Transform bombPoint;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private float bombCooldown = 1f;
    [SerializeField] private float bombDropForce = 10f;

    private float nextFireTime;
    private float nextBombTime;
    private AudioManager audioManager;
    private bool canFire = true;

    private void Start()
    {
        audioManager = AudioManager.Instance;
    }

    private void Update()
    {
        if (!canFire) return;

#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            Fire();
        }
        if (Input.GetKeyDown(KeyCode.B) && Time.time >= nextBombTime)
        {
            DropBomb();
        }
#else
        if (Input.touchCount > 1 && Time.time >= nextFireTime)
        {
            Fire();
        }
        
        if (Input.touchCount > 2 && Time.time >= nextBombTime)
        {
            DropBomb();
        }
#endif
    }

    private void Fire()
    {
        if (weaponPoint == null || bulletPrefab == null) return;

        // Spawn bullet
        GameObject bullet = Instantiate(bulletPrefab, weaponPoint.position, Quaternion.identity);
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = Vector3.forward * bulletSpeed;
            bulletRb.useGravity = false;
        }
        Destroy(bullet, bulletLifetime);

        // Eject shell casing
        if (shellEjectionPoint != null && shellPrefab != null)
        {
            GameObject shell = Instantiate(shellPrefab, shellEjectionPoint.position, shellEjectionPoint.rotation);
            Rigidbody shellRb = shell.GetComponent<Rigidbody>();
            if (shellRb != null)
            {
                // Add random variation to ejection
                Vector3 ejectionDir = (shellEjectionPoint.right + Vector3.up * 0.5f).normalized;
                shellRb.AddForce(ejectionDir * shellEjectionForce, ForceMode.Impulse);
                shellRb.AddTorque(Random.insideUnitSphere * shellTorque, ForceMode.Impulse);
            }
            Destroy(shell, shellLifetime);
        }

        audioManager?.PlayShootSound();
        nextFireTime = Time.time + fireRate;
    }

    private void DropBomb()
    {
        if (bombPoint == null || bombPrefab == null) return;
        GameObject bomb = Instantiate(bombPrefab, bombPoint.position, bombPoint.rotation);
        Rigidbody bombRb = bomb.GetComponent<Rigidbody>();
        if (bombRb != null)
        {
            bombRb.useGravity = true;
            bombRb.linearVelocity = Vector3.zero;
            bombRb.AddForce(Vector3.down * bombDropForce, ForceMode.Impulse);
        }
        audioManager?.PlayBombSound();
        nextBombTime = Time.time + bombCooldown;
    }

    public void EnableWeapons(bool enable)
    {
        canFire = enable;
    }
}
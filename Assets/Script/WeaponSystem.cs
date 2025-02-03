using UnityEngine;
using System;

public class WeaponSystem : MonoBehaviour
{
    [Header("Vapen Inställningar")]
    [SerializeField] private Transform weaponPoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float bulletLifetime = 2f;

    [Header("Hylsor")]
    [SerializeField] private Transform shellEjectionPoint;
    [SerializeField] private GameObject shellPrefab;
    [SerializeField] private float shellEjectionForce = 2f;
    [SerializeField] private float shellTorque = 2f;
    [SerializeField] private float shellLifetime = 3f;

    [Header("Bomber")]
    [SerializeField] private Transform bombPoint;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private float bombCooldown = 1f;
    [SerializeField] private float bombDropForce = 10f;

    [Header("Missile")]
    [SerializeField] private Transform missilePoint;
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private float missileCooldown = 1f;
    [SerializeField] private float missileforce = 10f;

    [Header("Other")]
    private float nextFireTime;
    private float nextBombTime;
    private AudioManager audioManager;
    private bool canFire = true;
    private bool isInitialized;
    private readonly Vector3 BulletDirection = Vector3.forward;
    private readonly Vector3 ShellEjectionOffset = Vector3.up * 0.5f;

    private void Start()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        audioManager = AudioManager.Instance;
        ValidateComponents();
        isInitialized = true;
    }

    private void ValidateComponents()
    {
        if (weaponPoint == null) Debug.LogWarning("WeaponPoint saknas!");
        if (bulletPrefab == null) Debug.LogWarning("BulletPrefab saknas!");
        if (shellEjectionPoint == null) Debug.LogWarning("ShellEjectionPoint saknas!");
        if (shellPrefab == null) Debug.LogWarning("ShellPrefab saknas!");
        if (bombPoint == null) Debug.LogWarning("BombPoint saknas!");
        if (bombPrefab == null) Debug.LogWarning("BombPrefab saknas!");
        if (missilePrefab == null) Debug.LogWarning("MissilePrefab saknas!");
    }

    private void Update()
    {
        if (!isInitialized || !canFire) return;
        HandleWeaponInput();
    }

    private void HandleWeaponInput()
    {
#if UNITY_EDITOR
        HandleEditorInput();
#else
            HandleMobileInput();
#endif
    }

    private void HandleEditorInput()
    {
        if (Input.GetKey(KeyCode.Space) && CanFire())
        {
            Fire();
        }
        if (Input.GetKeyDown(KeyCode.B) && CanDropBomb())
        {
            DropBomb();
        }
    }

    private void HandleMobileInput()
    {
        if (Input.touchCount > 1 && CanFire())
        {
            Fire();
        }
        if (Input.touchCount > 2 && CanDropBomb())
        {
            DropBomb();
        }
    }

    private bool CanFire() => Time.time >= nextFireTime;
    private bool CanDropBomb() => Time.time >= nextBombTime;

    private void Fire()
    {
        if (!ValidateWeaponComponents()) return;

        SpawnBullet();
        EjectShell();
        PlayFireEffects();
        UpdateFireCooldown();
    }

    private bool ValidateWeaponComponents()
    {
        return weaponPoint != null && bulletPrefab != null;
    }

    private void SpawnBullet()
    {
        GameObject bullet = InstantiateBullet();
        ConfigureBullet(bullet);
        Destroy(bullet, bulletLifetime);
    }

    private GameObject InstantiateBullet()
    {
        return Instantiate(bulletPrefab, weaponPoint.position, Quaternion.identity);
    }

    private void ConfigureBullet(GameObject bullet)
    {
        if (bullet.TryGetComponent<Rigidbody>(out var bulletRb))
        {
            bulletRb.useGravity = false;
            bulletRb.linearVelocity = BulletDirection * bulletSpeed;
        }
    }

    private void EjectShell()
    {
        if (!ValidateShellComponents()) return;

        GameObject shell = InstantiateShell();
        ConfigureShell(shell);
        Destroy(shell, shellLifetime);
    }

    private bool ValidateShellComponents()
    {
        return shellEjectionPoint != null && shellPrefab != null;
    }

    private GameObject InstantiateShell()
    {
        return Instantiate(shellPrefab, shellEjectionPoint.position, shellEjectionPoint.rotation);
    }

    private void ConfigureShell(GameObject shell)
    {
        if (shell.TryGetComponent<Rigidbody>(out var shellRb))
        {
            Vector3 ejectionDir = (shellEjectionPoint.right + ShellEjectionOffset).normalized;
            shellRb.AddForce(ejectionDir * shellEjectionForce, ForceMode.Impulse);
            shellRb.AddTorque(UnityEngine.Random.insideUnitSphere * shellTorque, ForceMode.Impulse);
        }
    }

    private void PlayFireEffects()
    {
        AudioManager.Instance?.PlayCombatSound(CombatSoundType.Shoot);
    }

    private void UpdateFireCooldown()
    {
        nextFireTime = Time.time + fireRate;
    }

    private void DropBomb()
    {
        if (!ValidateBombComponents()) return;

        GameObject bomb = InstantiateBomb();
        ConfigureBomb(bomb);
        PlayBombEffects();
        UpdateBombCooldown();

    }

    private bool ValidateBombComponents()
    {
        return bombPoint != null && bombPrefab != null;
    }

    private GameObject InstantiateBomb()
    {
        return Instantiate(bombPrefab, bombPoint.position, bombPoint.rotation);
    }

    private void ConfigureBomb(GameObject bomb)
    {
        if (bomb.TryGetComponent<Rigidbody>(out var bombRb))
        {
            bombRb.useGravity = true;
            bombRb.linearVelocity = Vector3.zero;
            bombRb.AddForce(Vector3.down * bombDropForce, ForceMode.Impulse);
        }
    }

    private void PlayBombEffects()
    {
        AudioManager.Instance?.PlayBombSound(BombSoundType.Drop);
    }

    private void UpdateBombCooldown()
    {
        nextBombTime = Time.time + bombCooldown;
    }

    public void EnableWeapons(bool enable)
    {
        canFire = enable;
    }

    public void ApplyWeaponBoost()
    {
        // Implementera weapon boost logik
    }

    public void ApplyBombBoost()
    {
        // Implementera bomb boost logik
    }
}

//using UnityEngine;

//public class WeaponSystem : MonoBehaviour
//{
//    [Header("Weapon Settings")]
//    [SerializeField] private Transform weaponPoint;
//    [SerializeField] private GameObject bulletPrefab;
//    [SerializeField] private float bulletSpeed = 20f;
//    [SerializeField] private float fireRate = 0.2f;
//    [SerializeField] private float bulletLifetime = 2f;

//    [Header("Shell Settings")]
//    [SerializeField] private Transform shellEjectionPoint;
//    [SerializeField] private GameObject shellPrefab;
//    [SerializeField] private float shellEjectionForce = 2f;
//    [SerializeField] private float shellTorque = 2f;
//    [SerializeField] private float shellLifetime = 3f;

//    [Header("Bomb Settings")]
//    [SerializeField] private Transform bombPoint;
//    [SerializeField] private GameObject bombPrefab;
//    [SerializeField] private float bombCooldown = 1f;
//    [SerializeField] private float bombDropForce = 10f;

//    private float nextFireTime;
//    private float nextBombTime;
//    private AudioManager audioManager;
//    private bool canFire = true;

//    private void Start()
//    {
//        audioManager = AudioManager.Instance;
//    }

//    private void Update()
//    {
//        if (!canFire) return;

//#if UNITY_EDITOR
//        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
//        {
//            Fire();
//        }
//        if (Input.GetKeyDown(KeyCode.B) && Time.time >= nextBombTime)
//        {
//            DropBomb();
//        }
//#else
//        if (Input.touchCount > 1 && Time.time >= nextFireTime)
//        {
//            Fire();
//        }

//        if (Input.touchCount > 2 && Time.time >= nextBombTime)
//        {
//            DropBomb();
//        }
//#endif
//    }

//    private void Fire()
//    {
//        if (weaponPoint == null || bulletPrefab == null) return;

//        // Spawn bullet
//        //GameObject bullet = Instantiate(bulletPrefab, weaponPoint.position, bulletPrefab.transform.rotation);
//        GameObject bullet = Instantiate(bulletPrefab, weaponPoint.position, Quaternion.identity);
//        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
//        if (bulletRb != null)
//        {
//            //bulletRb.linearVelocity = bullet.transform.forward * bulletSpeed;
//            bulletRb.linearVelocity = Vector3.forward * bulletSpeed;
//            bulletRb.useGravity = false;
//        }
//        Destroy(bullet, bulletLifetime);

//        // Eject shell casing
//        if (shellEjectionPoint != null && shellPrefab != null)
//        {
//            GameObject shell = Instantiate(shellPrefab, shellEjectionPoint.position, shellEjectionPoint.rotation);
//            Rigidbody shellRb = shell.GetComponent<Rigidbody>();
//            if (shellRb != null)
//            {
//                // Add random variation to ejection
//                Vector3 ejectionDir = (shellEjectionPoint.right + Vector3.up * 0.5f).normalized;
//                shellRb.AddForce(ejectionDir * shellEjectionForce, ForceMode.Impulse);
//                shellRb.AddTorque(Random.insideUnitSphere * shellTorque, ForceMode.Impulse);
//            }
//            Destroy(shell, shellLifetime);
//        }

//        audioManager?.PlayShootSound();
//        nextFireTime = Time.time + fireRate;
//    }

//    private void DropBomb()
//    {
//        if (bombPoint == null || bombPrefab == null) return;
//        GameObject bomb = Instantiate(bombPrefab, bombPoint.position, bombPoint.rotation);
//        Rigidbody bombRb = bomb.GetComponent<Rigidbody>();
//        if (bombRb != null)
//        {
//            bombRb.useGravity = true;
//            bombRb.linearVelocity = Vector3.zero;
//            bombRb.AddForce(Vector3.down * bombDropForce, ForceMode.Impulse);
//        }
//        audioManager?.PlayBombSound();
//        nextBombTime = Time.time + bombCooldown;
//    }

//    public void EnableWeapons(bool enable)
//    {
//        canFire = enable;
//    }
//}
using UnityEngine;
using System.Collections;

public class WeaponSystem : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private Transform weaponPoint;
    [SerializeField] private Transform leftGunPoint;
    [SerializeField] private Transform rightGunPoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float bulletLifetime = 2f;

    [Header("Shells Settings")]
    [SerializeField] private Transform shellEjectionPoint;
    [SerializeField] private GameObject shellPrefab;
    [SerializeField] private float shellLifetime = 3f;
    [SerializeField] private float minShellEjectionForce = 3f;
    [SerializeField] private float maxShellEjectionForce = 6f;
    [SerializeField] private float minShellTorque = 3f;
    [SerializeField] private float maxShellTorque = 8f;

    [Header("Boost Settings")]
    [SerializeField] private float fireRateBoostDuration = 10f;
    [SerializeField] private float dualWeaponsDuration = 15f;

    private float nextFireTime;
    private float originalFireRate;
    private bool dualWeaponsEnabled = false;
    private AudioManager audioManager;
    private bool canFire = true;
    private bool isInitialized;
    private readonly Vector3 BulletDirection = Vector3.forward;
    private readonly Vector3 ShellEjectionOffset = Vector3.up * 0.5f;

    private PlaneHealthSystem planeHealth;

    private void Start()
    {
        InitializeComponents();
        originalFireRate = fireRate;
    }

    private void InitializeComponents()
    {
        audioManager = AudioManager.Instance;
        planeHealth = GetComponent<PlaneHealthSystem>();
        if (planeHealth == null)
        {
            Debug.LogWarning("PlaneHealthSystem hittades inte på spelaren!");
        }
        ValidateComponents();
        isInitialized = true;
    }

    private void ValidateComponents()
    {
        if (weaponPoint == null) Debug.LogWarning("WeaponPoint saknas!");
        if (bulletPrefab == null) Debug.LogWarning("BulletPrefab saknas!");
        if (shellEjectionPoint == null) Debug.LogWarning("ShellEjectionPoint saknas!");
        if (shellPrefab == null) Debug.LogWarning("ShellPrefab saknas!");
    }

    private void Update()
    {
        if (!isInitialized || !canFire || (planeHealth != null && planeHealth.IsDead()))
        {
            return;
        }
        HandleWeaponInput();
    }

    public void EnableWeapons(bool enable)
    {
        // Tillåt bara aktivering av vapen om spelaren lever
        if (planeHealth != null && planeHealth.IsDead())
        {
            canFire = false;
            return;
        }
        canFire = enable;
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
    }

    private void HandleMobileInput()
    {
        if (Input.touchCount > 1 && CanFire())
        {
            Fire();
        }
    }

    private bool CanFire() => Time.time >= nextFireTime;

    private void Fire()
    {
        if (!ValidateWeaponComponents()) return;

        if (dualWeaponsEnabled)
        {
            SpawnBullet(leftGunPoint.position);
            SpawnBullet(rightGunPoint.position);
        }
        else
        {
            SpawnBullet(weaponPoint.position);
        }

        EjectShell();
        PlayFireEffects();
        UpdateFireCooldown();
    }

    private bool ValidateWeaponComponents()
    {
        return weaponPoint != null && bulletPrefab != null;
    }

    private void SpawnBullet(Vector3 spawnPosition)
    {
        // Cacha komponenter för att minska GetComponent-anrop
        GameObject bullet = BulletPool.Instance.GetBullet(true);

        BulletSystem bulletSystem = bullet.GetComponent<BulletSystem>();
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

        bullet.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);

        bulletSystem.Initialize(Vector3.forward, false, 10f);

        bulletRb.useGravity = false;
        bulletRb.linearVelocity = BulletDirection * bulletSpeed;
    }

    public void ResetFireRate()
    {
        fireRate = originalFireRate;
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
            // Aktivera gravitation
            shellRb.useGravity = true;

            // Grundkrafter för utkast
            float baseForce = Random.Range(3f, 5f);
            float upwardForce = Random.Range(2f, 4f);
            // Ändrad för att favorisera bakåtriktning (-4 till 1 istället för -2 till 2)
            float sideForce = Random.Range(-4f, 1f);

            // Applicera krafter
            Vector3 totalForce = transform.right * baseForce +      // Huvudriktning (åt höger)
                               transform.up * upwardForce +         // Uppåt
                               transform.forward * sideForce;       // Mestadels bakåt

            shellRb.AddForce(totalForce, ForceMode.Impulse);

            // Rotation
            shellRb.AddTorque(
                Random.Range(-5f, 5f),
                Random.Range(-5f, 5f),
                Random.Range(-5f, 5f),
                ForceMode.Impulse
            );

            // Se till att den inte "sover"
            shellRb.sleepThreshold = 0;
            shellRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    private void PlayFireEffects()
    {
        AudioManager.Instance?.PlayCombatSound(CombatSoundType.PlayerShoot);
    }

    private void UpdateFireCooldown()
    {
        nextFireTime = Time.time + fireRate;
    }

    // Boost metoder
    public IEnumerator ApplyFireRateBoost(float duration)
    {
        float boostedFireRate = fireRate * 0.5f;
        fireRate = boostedFireRate;
        yield return new WaitForSeconds(duration);
        fireRate = originalFireRate;
    }

    public IEnumerator EnableDualWeapons(float duration)
    {
        dualWeaponsEnabled = true;
        yield return new WaitForSeconds(duration);
        dualWeaponsEnabled = false;
    }

    public IEnumerator ApplyFireRateBoost(float multiplier, float duration)
    {
        float originalFireRate = fireRate;
        fireRate /= multiplier;
        yield return new WaitForSeconds(duration);
        fireRate = originalFireRate;
    }
}

//using UnityEngine;
//using System.Collections;

//public class WeaponSystem : MonoBehaviour
//{
//    [Header("Weapon Settings")]
//    [SerializeField] private Transform weaponPoint;
//    [SerializeField] private Transform leftGunPoint;
//    [SerializeField] private Transform rightGunPoint;
//    [SerializeField] private GameObject bulletPrefab;
//    [SerializeField] private float bulletSpeed = 20f;
//    [SerializeField] private float fireRate = 0.2f;
//    [SerializeField] private float bulletLifetime = 2f;

//    [Header("Shells Settings")]
//    [SerializeField] private Transform shellEjectionPoint;
//    [SerializeField] private GameObject shellPrefab;
//    [SerializeField] private float shellLifetime = 3f;
//    [SerializeField] private float minShellEjectionForce = 3f;
//    [SerializeField] private float maxShellEjectionForce = 6f;
//    [SerializeField] private float minShellTorque = 3f;
//    [SerializeField] private float maxShellTorque = 8f;

//    [Header("Boost Settings")]
//    [SerializeField] private float fireRateBoostDuration = 10f;
//    [SerializeField] private float dualWeaponsDuration = 15f;

//    private float nextFireTime;
//    private float originalFireRate;
//    private bool dualWeaponsEnabled = false;
//    private AudioManager audioManager;
//    private bool canFire = true;
//    private bool isInitialized;
//    private readonly Vector3 BulletDirection = Vector3.forward;
//    private readonly Vector3 ShellEjectionOffset = Vector3.up * 0.5f;

//    private PlaneHealthSystem planeHealth;

//    private void Start()
//    {
//        InitializeComponents();
//        originalFireRate = fireRate;
//    }

//    private void InitializeComponents()
//    {
//        audioManager = AudioManager.Instance;
//        planeHealth = GetComponent<PlaneHealthSystem>();
//        if (planeHealth == null)
//        {
//            Debug.LogWarning("PlaneHealthSystem hittades inte på spelaren!");
//        }
//        ValidateComponents();
//        isInitialized = true;
//    }

//    private void ValidateComponents()
//    {
//        if (weaponPoint == null) Debug.LogWarning("WeaponPoint saknas!");
//        if (bulletPrefab == null) Debug.LogWarning("BulletPrefab saknas!");
//        if (shellEjectionPoint == null) Debug.LogWarning("ShellEjectionPoint saknas!");
//        if (shellPrefab == null) Debug.LogWarning("ShellPrefab saknas!");
//    }

//    private void Update()
//    {
//        if (!isInitialized || !canFire || (planeHealth != null && planeHealth.IsDead()))
//        {
//            return;
//        }
//        HandleWeaponInput();
//    }

//    public void EnableWeapons(bool enable)
//    {
//        // Tillåt bara aktivering av vapen om spelaren lever
//        if (planeHealth != null && planeHealth.IsDead())
//        {
//            canFire = false;
//            return;
//        }
//        canFire = enable;
//    }

//    private void HandleWeaponInput()
//    {
//#if UNITY_EDITOR
//        HandleEditorInput();
//#else
//        HandleMobileInput();
//#endif
//    }

//    private void HandleEditorInput()
//    {
//        if (Input.GetKey(KeyCode.Space) && CanFire())
//        {
//            Fire();
//        }
//    }

//    private void HandleMobileInput()
//    {
//        if (Input.touchCount > 1 && CanFire())
//        {
//            Fire();
//        }
//    }

//    private bool CanFire() => Time.time >= nextFireTime;

//    private void Fire()
//    {
//        if (!ValidateWeaponComponents()) return;

//        if (dualWeaponsEnabled)
//        {
//            SpawnBullet(leftGunPoint.position);
//            SpawnBullet(rightGunPoint.position);
//        }
//        else
//        {
//            SpawnBullet(weaponPoint.position);
//        }

//        EjectShell();
//        PlayFireEffects();
//        UpdateFireCooldown();
//    }

//    private bool ValidateWeaponComponents()
//    {
//        return weaponPoint != null && bulletPrefab != null;
//    }

//    private void SpawnBullet(Vector3 spawnPosition)
//    {
//        GameObject bullet = BulletPool.Instance.GetBullet(true);
//        BulletSystem bulletSystem = bullet.GetComponent<BulletSystem>();
//        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

//        bullet.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
//        bulletSystem.Initialize(Vector3.forward, false, 10f);
//        bulletRb.useGravity = false;
//        bulletRb.linearVelocity = BulletDirection * bulletSpeed;
//    }

//    public void ResetFireRate()
//    {
//        fireRate = originalFireRate;
//    }

//    private void EjectShell()
//    {
//        if (!ValidateShellComponents()) return;

//        GameObject shell = InstantiateShell();
//        ConfigureShell(shell);
//        Destroy(shell, shellLifetime);
//    }

//    private bool ValidateShellComponents()
//    {
//        return shellEjectionPoint != null && shellPrefab != null;
//    }

//    private GameObject InstantiateShell()
//    {
//        return Instantiate(shellPrefab, shellEjectionPoint.position, shellEjectionPoint.rotation);
//    }

//    private void ConfigureShell(GameObject shell)
//    {
//        if (shell.TryGetComponent<Rigidbody>(out var shellRb))
//        {
//            // Aktivera gravitation
//            shellRb.useGravity = true;

//            // Grundkrafter för utkast
//            float baseForce = Random.Range(3f, 5f);
//            float upwardForce = Random.Range(2f, 4f);
//            // Ändrad för att favorisera bakåtriktning (-4 till 1 istället för -2 till 2)
//            float sideForce = Random.Range(-4f, 1f);

//            // Applicera krafter
//            Vector3 totalForce = transform.right * baseForce +      // Huvudriktning (åt höger)
//                               transform.up * upwardForce +         // Uppåt
//                               transform.forward * sideForce;       // Mestadels bakåt

//            shellRb.AddForce(totalForce, ForceMode.Impulse);

//            // Rotation
//            shellRb.AddTorque(
//                Random.Range(-5f, 5f),
//                Random.Range(-5f, 5f),
//                Random.Range(-5f, 5f),
//                ForceMode.Impulse
//            );

//            // Se till att den inte "sover"
//            shellRb.sleepThreshold = 0;
//            shellRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
//        }
//    }

//    private void PlayFireEffects()
//    {
//        AudioManager.Instance?.PlayCombatSound(CombatSoundType.PlayerShoot);
//    }

//    private void UpdateFireCooldown()
//    {
//        nextFireTime = Time.time + fireRate;
//    }

//    public IEnumerator ApplyFireRateBoost(float duration)
//    {
//        float boostedFireRate = fireRate * 0.5f;
//        fireRate = boostedFireRate;
//        yield return new WaitForSeconds(duration);
//        fireRate = originalFireRate;
//    }

//    public IEnumerator EnableDualWeapons(float duration)
//    {
//        dualWeaponsEnabled = true;
//        yield return new WaitForSeconds(duration);
//        dualWeaponsEnabled = false;
//    }

//    public IEnumerator ApplyFireRateBoost(float multiplier, float duration)
//    {
//        float originalFireRate = fireRate;
//        fireRate /= multiplier;
//        yield return new WaitForSeconds(duration);
//        fireRate = originalFireRate;
//    }
//}
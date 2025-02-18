using UnityEngine;
using System.Collections;

public class WeaponSystem : MonoBehaviour
{
    [Header("Vapen Inställningar")]
    [SerializeField] private Transform weaponPoint;
    [SerializeField] private Transform leftGunPoint;
    [SerializeField] private Transform rightGunPoint;
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

    private void Start()
    {
        InitializeComponents();
        originalFireRate = fireRate;
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

    //private void SpawnBullet(Vector3 spawnPosition)
    //{
    //    GameObject bullet = BulletPool.Instance.GetBullet(true);
    //    bullet.transform.position = spawnPosition;
    //    bullet.transform.rotation = Quaternion.identity;

    //    if (bullet.TryGetComponent<BulletSystem>(out var bulletSystem))
    //    {
    //        bulletSystem.Initialize(Vector3.forward, false, 10f);
    //    }
    //    if (bullet.TryGetComponent<Rigidbody>(out var bulletRb))
    //    {
    //        bulletRb.useGravity = false;
    //        bulletRb.linearVelocity = BulletDirection * bulletSpeed;
    //    }
    //}

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
            shellRb.AddTorque(Random.insideUnitSphere * shellTorque, ForceMode.Impulse);
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

    public void EnableWeapons(bool enable)
    {
        canFire = enable;
    }
}
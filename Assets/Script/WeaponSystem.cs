using UnityEngine;
using System.Collections;

public class WeaponSystem : MonoBehaviour
{
    [Header("Vapen Inställningar")]
    [SerializeField] private Transform weaponPoint;
    [SerializeField] private Transform leftGunPoint;  // För dubbla vapen
    [SerializeField] private Transform rightGunPoint; // För dubbla vapen
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
    [SerializeField] private float bombDropForce = 200f;

    [Header("Boost Settings")]
    [SerializeField] private float fireRateBoostDuration = 10f;
    [SerializeField] private float dualWeaponsDuration = 15f;

    private float nextFireTime;
    private float nextBombTime;
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
        if (bombPoint == null) Debug.LogWarning("BombPoint saknas!");
        if (bombPrefab == null) Debug.LogWarning("BombPrefab saknas!");
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

        if (dualWeaponsEnabled)
        {
            // Skjut från båda vapenpunkterna
            SpawnBullet(leftGunPoint.position);
            SpawnBullet(rightGunPoint.position);
        }
        else
        {
            // Skjut från huvudvapnet
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
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        if (bullet.TryGetComponent<BulletSystem>(out var bulletSystem))
        {
            bulletSystem.Initialize(Vector3.forward, false, 10f); // Standardskada
        }
        if (bullet.TryGetComponent<Rigidbody>(out var bulletRb))
        {
            bulletRb.useGravity = false;
            bulletRb.linearVelocity = BulletDirection * bulletSpeed;
        }
        Destroy(bullet, bulletLifetime);
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
            shellRb.AddTorque(Random.insideUnitSphere * shellTorque, ForceMode.Impulse);
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

    // Boost metoder
    public IEnumerator ApplyFireRateBoost(float duration)
    {
        float boostedFireRate = fireRate * 0.5f; // Dubbelt så snabb fire rate
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

    public void EnableWeapons(bool enable)
    {
        canFire = enable;
    }
}
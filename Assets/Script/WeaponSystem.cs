using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WeaponSystem : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private Transform mainWeaponPoint;
    [SerializeField] private Transform leftWeaponPoint;
    [SerializeField] private Transform rightWeaponPoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float bulletLifetime = 2f;

    [Header("Visual Settings")]
    [SerializeField] private GameObject mainGunVisual;
    [SerializeField] private GameObject dualGunsVisual; // En enda mesh för båda extra vapnen

    [Header("Shells Settings")]
    [SerializeField] private Transform mainShellEjectionPoint;
    [SerializeField] private Transform leftShellEjectionPoint;
    [SerializeField] private Transform rightShellEjectionPoint;
    [SerializeField] private GameObject shellPrefab;
    [SerializeField] private float shellLifetime = 3f;
    [SerializeField] private float minShellEjectionForce = 3f;
    [SerializeField] private float maxShellEjectionForce = 6f;
    [SerializeField] private float minShellTorque = 3f;
    [SerializeField] private float maxShellTorque = 8f;

    [Header("Boost Settings")]
    [SerializeField] private float fireRateBoostDuration = 10f;
    [SerializeField] private float dualWeaponsDuration = 15f;

    [Header("Överhettning")]
    [SerializeField] private float maxHeat = 100f;              // Max värme innan överhettning
    [SerializeField] private float heatPerShot = 10f;           // Värmeökning per skott
    [SerializeField] private float coolingRate = 20f;           // Avkylning per sekund
    [SerializeField] private float overheatCooldownTime = 3f;   // Tid att vänta om överhettad
    [SerializeField] private Slider heatSlider;                 // UI-referens

    [Header("Överhettningseffekter")]
    [SerializeField] private AudioClip overheatingWarningSound;
    [SerializeField] private AudioClip overheatSound;
    [SerializeField] private ParticleSystem steamEffect;
    [SerializeField] private Color normalHeatColor = Color.green;
    [SerializeField] private Color warningHeatColor = Color.yellow;
    [SerializeField] private Color criticalHeatColor = Color.red;

    private float nextFireTime;
    private float originalFireRate;
    private bool dualWeaponsEnabled = false;
    private float dualWeaponsEndTime = 0f; // Ny variabel för att spåra sluttiden
    private AudioManager audioManager;
    private bool canFire = true;
    private bool isInitialized;
    private readonly Vector3 BulletDirection = Vector3.forward;
    private readonly Vector3 ShellEjectionOffset = Vector3.up * 0.5f;

    private PlaneHealthSystem planeHealth;

    // Överhettningsvariabler
    private float currentHeat = 0f;
    private bool isOverheated = false;
    private bool playedWarningSound = false;

    // CACHEA TRANSFORMS
    private Transform cachedTransform;

    private void Awake()
    {
        // Cachea transform för bättre prestanda
        cachedTransform = transform;

        // Dölj extravapenmodellen direkt i Awake för att säkerställa att den är dold från början
        if (dualGunsVisual != null)
            dualGunsVisual.SetActive(false);
    }

    private void Start()
    {
        InitializeComponents();
        originalFireRate = fireRate;

        // Dubbelkolla att main gun är aktivt och dual guns är inaktiva
        if (mainGunVisual != null)
            mainGunVisual.SetActive(true);

        if (dualGunsVisual != null)
            dualGunsVisual.SetActive(false);

        // Sätt startläget (säkerhet)
        dualWeaponsEnabled = false;

        // Initiera överhettning
        currentHeat = 0f;
        UpdateHeatDisplay();

        Debug.Log("WeaponSystem initialized. DualWeapons state: " + dualWeaponsEnabled);
    }

    private void InitializeComponents()
    {
        // Cachea komponenter
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
        if (mainWeaponPoint == null) Debug.LogWarning("MainWeaponPoint saknas!");
        if (leftWeaponPoint == null) Debug.LogWarning("LeftWeaponPoint saknas!");
        if (rightWeaponPoint == null) Debug.LogWarning("RightWeaponPoint saknas!");
        if (bulletPrefab == null) Debug.LogWarning("BulletPrefab saknas!");
        if (mainShellEjectionPoint == null) Debug.LogWarning("MainShellEjectionPoint saknas!");
        if (shellPrefab == null) Debug.LogWarning("ShellPrefab saknas!");
        if (heatSlider == null) Debug.LogWarning("HeatSlider saknas!");
    }

    private bool CanFire()
    {
        // Tillåt skjutning endast om vapnet inte är överhettat och cooldown har gått ut
        return Time.time >= nextFireTime && !isOverheated;
    }

    private void Update()
    {
        if (!isInitialized)
            return;

        // Kylning över tid
        if (currentHeat > 0)
        {
            currentHeat -= coolingRate * Time.deltaTime;
            if (currentHeat < 0) currentHeat = 0;

            UpdateHeatDisplay();
        }

        // Återställ varningsflaggan när värmen sjunker under tröskelvärdet
        if (playedWarningSound && currentHeat < maxHeat * 0.7f)
        {
            playedWarningSound = false;
        }

        // Kontrollera om dual weapons ska avaktiveras baserat på timer
        if (dualWeaponsEnabled && Time.time >= dualWeaponsEndTime)
        {
            Debug.Log("DualWeapons timer expired - returning to main weapon");
            SetDualWeaponsState(false);
        }

        if (!canFire || (planeHealth != null && planeHealth.IsDead()))
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

    private void Fire()
    {
        if (!ValidateWeaponComponents() || isOverheated) return;

        // Lägg till värme när vi skjuter
        currentHeat += heatPerShot;
        UpdateHeatDisplay();

        // Kontrollera överhettning
        if (currentHeat >= maxHeat)
        {
            StartOverheating();
            return;
        }

        // Spela varningsljud när vi närmar oss överhettning
        // Spela varningsljud när vi närmar oss överhettning
        if (currentHeat > maxHeat * 0.7f && !playedWarningSound)
        {
            if (audioManager != null)
            {
                // Ändrat från PlayOneShot till din befintliga ljudsystem-metod
                audioManager.PlayBoostSound(); // Eller använd en annan lämplig ljudmetod i din AudioManager
                playedWarningSound = true;
            }
        }

        if (dualWeaponsEnabled)
        {
            SpawnBullet(leftWeaponPoint.position);
            SpawnBullet(rightWeaponPoint.position);

            if (leftShellEjectionPoint != null)
                EjectShell(leftShellEjectionPoint);

            if (rightShellEjectionPoint != null)
                EjectShell(rightShellEjectionPoint);
        }
        else
        {
            SpawnBullet(mainWeaponPoint.position);

            if (mainShellEjectionPoint != null)
                EjectShell(mainShellEjectionPoint);
        }

        PlayFireEffects();
        UpdateFireCooldown();
    }

    private bool ValidateWeaponComponents()
    {
        return mainWeaponPoint != null && bulletPrefab != null;
    }

    private void SpawnBullet(Vector3 spawnPosition)
    {
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

    private void EjectShell(Transform ejectionPoint)
    {
        if (ejectionPoint == null || shellPrefab == null) return;

        GameObject shell = ShellAndBombPool.Instance.GetShell();
        ConfigureShell(shell, ejectionPoint);
        ShellAndBombPool.Instance.ReturnToPool(shell, shellLifetime);
    }

    private void ConfigureShell(GameObject shell, Transform ejectionPoint)
    {
        shell.transform.SetPositionAndRotation(ejectionPoint.position, ejectionPoint.rotation);

        if (shell.TryGetComponent<Rigidbody>(out var shellRb))
        {
            // Aktivera gravitation
            shellRb.useGravity = true;

            // Grundkrafter för utkast
            float baseForce = Random.Range(3f, 5f);
            float upwardForce = Random.Range(2f, 4f);
            float sideForce = Random.Range(-4f, 1f);

            // Använd cachedTransform
            Vector3 totalForce = cachedTransform.right * baseForce +
                               cachedTransform.up * upwardForce +
                               cachedTransform.forward * sideForce;

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

    private void StartOverheating()
    {
        isOverheated = true;

        // Spela överhettningsljud och -effekter
        if (audioManager != null && overheatSound != null)
        {
            // Ändrat från PlayOneShot till din befintliga ljudsystem-metod
            audioManager.PlayBoostSound(); // Eller använd en annan lämplig ljudmetod i din AudioManager
        }

        if (steamEffect != null)
        {
            steamEffect.Play();
        }

        // Visa meddelande till spelaren
        GameMessageSystem messageSystem = FindObjectOfType<GameMessageSystem>();
        if (messageSystem != null)
        {
            messageSystem.ShowBoostMessage("WEAPON OVERHEATED!");
        }

        // Starta nedkylningstimer
        StartCoroutine(OverheatCooldown());
    }

    private IEnumerator OverheatCooldown()
    {
        yield return new WaitForSeconds(overheatCooldownTime);

        // Återställ värmen till en säker nivå
        currentHeat = maxHeat * 0.5f;
        isOverheated = false;

        // Meddela spelaren att vapnen är användbara igen
        GameMessageSystem messageSystem = FindObjectOfType<GameMessageSystem>();
        if (messageSystem != null)
        {
            messageSystem.ShowBoostMessage("WEAPONS READY");
        }
    }

    private void UpdateHeatDisplay()
    {
        // Uppdatera UI om tillgänglig
        if (heatSlider != null)
        {
            heatSlider.value = currentHeat / maxHeat;

            // Ändra färg baserat på värmenivå
            Image fillImage = heatSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                if (currentHeat > maxHeat * 0.7f)
                    fillImage.color = criticalHeatColor;
                else if (currentHeat > maxHeat * 0.4f)
                    fillImage.color = warningHeatColor;
                else
                    fillImage.color = normalHeatColor;
            }
        }
    }

    public IEnumerator ApplyFireRateBoost(float multiplier, float duration)
    {
        float originalFireRate = fireRate;
        fireRate /= multiplier;
        yield return new WaitForSeconds(duration);
        fireRate = originalFireRate;
    }

    // Ändrad metod - nu använder vi timer istället för coroutine
    public void EnableDualWeapons(float duration)
    {
        // Aktivera DualWeapons
        SetDualWeaponsState(true);

        // Sätt sluttid för dualweapons
        dualWeaponsEndTime = Time.time + duration;

        Debug.Log("DualWeapons activated for " + duration + " seconds. Will expire at game time: " + dualWeaponsEndTime);
    }

    // Metod för att sätta dualWeapons-läget på ett ställe
    private void SetDualWeaponsState(bool enabled)
    {
        // Sätt flaggan först
        dualWeaponsEnabled = enabled;

        // Uppdatera visuella objekt
        if (mainGunVisual != null)
            mainGunVisual.SetActive(!enabled); // Visa huvudvapnet när dual är AV

        if (dualGunsVisual != null)
            dualGunsVisual.SetActive(enabled); // Visa extravapenm när dual är PÅ

        Debug.Log("DualWeapons state set to: " + enabled +
                  " (mainGun: " + (!enabled) + ", dualGuns: " + enabled + ")");
    }

    // Metod som kan anropas från andra klasser för att tvinga tillbaka till normalläge
    public void ResetToMainWeapon()
    {
        SetDualWeaponsState(false);
        dualWeaponsEndTime = 0f;
    }

    // Publika metoder för att kontrollera överhettningsstatus
    public float GetHeatPercentage()
    {
        return currentHeat / maxHeat;
    }

    public bool IsOverheated()
    {
        return isOverheated;
    }

    // Speciell funktion för att omedelbart kyla ned vapnen (kan användas som powerup)
    public void CooldownWeapons(float amount)
    {
        currentHeat = Mathf.Max(0, currentHeat - amount);
        if (currentHeat < maxHeat && isOverheated)
        {
            isOverheated = false;
            StopAllCoroutines();

            // Meddela spelaren att vapnen är användbara igen
            GameMessageSystem messageSystem = FindObjectOfType<GameMessageSystem>();
            if (messageSystem != null)
            {
                messageSystem.ShowBoostMessage("WEAPONS COOLED DOWN");
            }
        }
        UpdateHeatDisplay();
    }

    // För debug
    public bool IsDualWeaponsActive()
    {
        return dualWeaponsEnabled;
    }
}

//using UnityEngine;
//using System.Collections;

//public class WeaponSystem : MonoBehaviour
//{
//    [Header("Weapon Settings")]
//    [SerializeField] private Transform mainWeaponPoint;
//    [SerializeField] private Transform leftWeaponPoint;
//    [SerializeField] private Transform rightWeaponPoint;
//    [SerializeField] private GameObject bulletPrefab;
//    [SerializeField] private float bulletSpeed = 20f;
//    [SerializeField] private float fireRate = 0.2f;
//    [SerializeField] private float bulletLifetime = 2f;

//    [Header("Visual Settings")]
//    [SerializeField] private GameObject mainGunVisual;
//    [SerializeField] private GameObject dualGunsVisual; // En enda mesh för båda extra vapnen

//    [Header("Shells Settings")]
//    [SerializeField] private Transform mainShellEjectionPoint;
//    [SerializeField] private Transform leftShellEjectionPoint;
//    [SerializeField] private Transform rightShellEjectionPoint;
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
//    private float dualWeaponsEndTime = 0f; // Ny variabel för att spåra sluttiden
//    private AudioManager audioManager;
//    private bool canFire = true;
//    private bool isInitialized;
//    private readonly Vector3 BulletDirection = Vector3.forward;
//    private readonly Vector3 ShellEjectionOffset = Vector3.up * 0.5f;

//    private PlaneHealthSystem planeHealth;

//    // CACHEA TRANSFORMS
//    private Transform cachedTransform;

//    private void Awake()
//    {
//        // Cachea transform för bättre prestanda
//        cachedTransform = transform;

//        // Dölj extravapenmodellen direkt i Awake för att säkerställa att den är dold från början
//        if (dualGunsVisual != null)
//            dualGunsVisual.SetActive(false);
//    }

//    private void Start()
//    {
//        InitializeComponents();
//        originalFireRate = fireRate;

//        // Dubbelkolla att main gun är aktivt och dual guns är inaktiva
//        if (mainGunVisual != null)
//            mainGunVisual.SetActive(true);

//        if (dualGunsVisual != null)
//            dualGunsVisual.SetActive(false);

//        // Sätt startläget (säkerhet)
//        dualWeaponsEnabled = false;

//        Debug.Log("WeaponSystem initialized. DualWeapons state: " + dualWeaponsEnabled);
//    }

//    private void InitializeComponents()
//    {
//        // Cachea komponenter
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
//        if (mainWeaponPoint == null) Debug.LogWarning("MainWeaponPoint saknas!");
//        if (leftWeaponPoint == null) Debug.LogWarning("LeftWeaponPoint saknas!");
//        if (rightWeaponPoint == null) Debug.LogWarning("RightWeaponPoint saknas!");
//        if (bulletPrefab == null) Debug.LogWarning("BulletPrefab saknas!");
//        if (mainShellEjectionPoint == null) Debug.LogWarning("MainShellEjectionPoint saknas!");
//        if (shellPrefab == null) Debug.LogWarning("ShellPrefab saknas!");
//    }

//    private bool CanFire()
//    {
//        // Tillåt skjutning även när spelaren är odödlig
//        return Time.time >= nextFireTime;
//    }

//    private void Update()
//    {
//        if (!isInitialized)
//            return;

//        // Kontrollera om dual weapons ska avaktiveras baserat på timer
//        if (dualWeaponsEnabled && Time.time >= dualWeaponsEndTime)
//        {
//            Debug.Log("DualWeapons timer expired - returning to main weapon");
//            SetDualWeaponsState(false);
//        }

//        if (!canFire || (planeHealth != null && planeHealth.IsDead()))
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

//    private void Fire()
//    {
//        if (!ValidateWeaponComponents()) return;

//        if (dualWeaponsEnabled)
//        {
//            SpawnBullet(leftWeaponPoint.position);
//            SpawnBullet(rightWeaponPoint.position);

//            if (leftShellEjectionPoint != null)
//                EjectShell(leftShellEjectionPoint);

//            if (rightShellEjectionPoint != null)
//                EjectShell(rightShellEjectionPoint);
//        }
//        else
//        {
//            SpawnBullet(mainWeaponPoint.position);

//            if (mainShellEjectionPoint != null)
//                EjectShell(mainShellEjectionPoint);
//        }

//        PlayFireEffects();
//        UpdateFireCooldown();
//    }

//    private bool ValidateWeaponComponents()
//    {
//        return mainWeaponPoint != null && bulletPrefab != null;
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

//    private void EjectShell(Transform ejectionPoint)
//    {
//        if (ejectionPoint == null || shellPrefab == null) return;

//        GameObject shell = ShellAndBombPool.Instance.GetShell();
//        ConfigureShell(shell, ejectionPoint);
//        ShellAndBombPool.Instance.ReturnToPool(shell, shellLifetime);
//    }

//    private void ConfigureShell(GameObject shell, Transform ejectionPoint)
//    {
//        shell.transform.SetPositionAndRotation(ejectionPoint.position, ejectionPoint.rotation);

//        if (shell.TryGetComponent<Rigidbody>(out var shellRb))
//        {
//            // Aktivera gravitation
//            shellRb.useGravity = true;

//            // Grundkrafter för utkast
//            float baseForce = Random.Range(3f, 5f);
//            float upwardForce = Random.Range(2f, 4f);
//            float sideForce = Random.Range(-4f, 1f);

//            // Använd cachedTransform
//            Vector3 totalForce = cachedTransform.right * baseForce +
//                               cachedTransform.up * upwardForce +
//                               cachedTransform.forward * sideForce;

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

//    public IEnumerator ApplyFireRateBoost(float multiplier, float duration)
//    {
//        float originalFireRate = fireRate;
//        fireRate /= multiplier;
//        yield return new WaitForSeconds(duration);
//        fireRate = originalFireRate;
//    }

//    // Ändrad metod - nu använder vi timer istället för coroutine
//    public void EnableDualWeapons(float duration)
//    {
//        // Aktivera DualWeapons
//        SetDualWeaponsState(true);

//        // Sätt sluttid för dualweapons
//        dualWeaponsEndTime = Time.time + duration;

//        Debug.Log("DualWeapons activated for " + duration + " seconds. Will expire at game time: " + dualWeaponsEndTime);
//    }

//    // Metod för att sätta dualWeapons-läget på ett ställe
//    private void SetDualWeaponsState(bool enabled)
//    {
//        // Sätt flaggan först
//        dualWeaponsEnabled = enabled;

//        // Uppdatera visuella objekt
//        if (mainGunVisual != null)
//            mainGunVisual.SetActive(!enabled); // Visa huvudvapnet när dual är AV

//        if (dualGunsVisual != null)
//            dualGunsVisual.SetActive(enabled); // Visa extravapenm när dual är PÅ

//        Debug.Log("DualWeapons state set to: " + enabled +
//                  " (mainGun: " + (!enabled) + ", dualGuns: " + enabled + ")");
//    }

//    // Metod som kan anropas från andra klasser för att tvinga tillbaka till normalläge
//    public void ResetToMainWeapon()
//    {
//        SetDualWeaponsState(false);
//        dualWeaponsEndTime = 0f;
//    }

//    // För debug
//    public bool IsDualWeaponsActive()
//    {
//        return dualWeaponsEnabled;
//    }
//}

////using UnityEngine;
////using System.Collections;

////public class WeaponSystem : MonoBehaviour
////{
////    [Header("Weapon Settings")]
////    [SerializeField] private Transform weaponPoint;
////    [SerializeField] private Transform leftGunPoint;
////    [SerializeField] private Transform rightGunPoint;
////    [SerializeField] private GameObject bulletPrefab;
////    [SerializeField] private float bulletSpeed = 20f;
////    [SerializeField] private float fireRate = 0.2f;
////    [SerializeField] private float bulletLifetime = 2f;

////    [Header("Shells Settings")]
////    [SerializeField] private Transform shellEjectionPoint;
////    [SerializeField] private GameObject shellPrefab;
////    [SerializeField] private float shellLifetime = 3f;
////    [SerializeField] private float minShellEjectionForce = 3f;
////    [SerializeField] private float maxShellEjectionForce = 6f;
////    [SerializeField] private float minShellTorque = 3f;
////    [SerializeField] private float maxShellTorque = 8f;

////    [Header("Boost Settings")]
////    [SerializeField] private float fireRateBoostDuration = 10f;
////    [SerializeField] private float dualWeaponsDuration = 15f;

////    private float nextFireTime;
////    private float originalFireRate;
////    private bool dualWeaponsEnabled = false;
////    private AudioManager audioManager;
////    private bool canFire = true;
////    private bool isInitialized;
////    private readonly Vector3 BulletDirection = Vector3.forward;
////    private readonly Vector3 ShellEjectionOffset = Vector3.up * 0.5f;

////    private PlaneHealthSystem planeHealth;

////    // CACHEA TRANSFORMS
////    private Transform cachedTransform;

////    private void Awake()
////    {
////        // Cachea transform för bättre prestanda
////        cachedTransform = transform;
////    }

////    private void Start()
////    {
////        InitializeComponents();
////        originalFireRate = fireRate;
////    }

////    private void InitializeComponents()
////    {
////        // Cachea komponenter
////        audioManager = AudioManager.Instance;
////        planeHealth = GetComponent<PlaneHealthSystem>();
////        if (planeHealth == null)
////        {
////            Debug.LogWarning("PlaneHealthSystem hittades inte på spelaren!");
////        }
////        ValidateComponents();
////        isInitialized = true;
////    }

////    private void ValidateComponents()
////    {
////        if (weaponPoint == null) Debug.LogWarning("WeaponPoint saknas!");
////        if (bulletPrefab == null) Debug.LogWarning("BulletPrefab saknas!");
////        if (shellEjectionPoint == null) Debug.LogWarning("ShellEjectionPoint saknas!");
////        if (shellPrefab == null) Debug.LogWarning("ShellPrefab saknas!");
////    }

////    private bool CanFire()
////    {
////        // Tillåt skjutning även när spelaren är odödlig
////        return Time.time >= nextFireTime;
////    }

////    private void Update()
////    {
////        if (!isInitialized || !canFire || (planeHealth != null && planeHealth.IsDead()))
////        {
////            return;
////        }
////        HandleWeaponInput();
////    }

////    public void EnableWeapons(bool enable)
////    {
////        // Tillåt bara aktivering av vapen om spelaren lever
////        if (planeHealth != null && planeHealth.IsDead())
////        {
////            canFire = false;
////            return;
////        }
////        canFire = enable;
////    }

////    private void HandleWeaponInput()
////    {
////#if UNITY_EDITOR
////        HandleEditorInput();
////#else
////        HandleMobileInput();
////#endif
////    }

////    private void HandleEditorInput()
////    {
////        if (Input.GetKey(KeyCode.Space) && CanFire())
////        {
////            Fire();
////        }
////    }

////    private void HandleMobileInput()
////    {
////        if (Input.touchCount > 1 && CanFire())
////        {
////            Fire();
////        }
////    }

////    private void Fire()
////    {
////        if (!ValidateWeaponComponents()) return;

////        if (dualWeaponsEnabled)
////        {
////            SpawnBullet(leftGunPoint.position);
////            SpawnBullet(rightGunPoint.position);
////        }
////        else
////        {
////            SpawnBullet(weaponPoint.position);
////        }

////        EjectShell();
////        PlayFireEffects();
////        UpdateFireCooldown();
////    }

////    private bool ValidateWeaponComponents()
////    {
////        return weaponPoint != null && bulletPrefab != null;
////    }

////    private void SpawnBullet(Vector3 spawnPosition)
////    {
////        GameObject bullet = BulletPool.Instance.GetBullet(true);
////        BulletSystem bulletSystem = bullet.GetComponent<BulletSystem>();
////        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

////        bullet.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
////        bulletSystem.Initialize(Vector3.forward, false, 10f);
////        bulletRb.useGravity = false;
////        bulletRb.linearVelocity = BulletDirection * bulletSpeed;
////    }

////    public void ResetFireRate()
////    {
////        fireRate = originalFireRate;
////    }

////    private void EjectShell()
////    {
////        if (!ValidateShellComponents()) return;

////        GameObject shell = ShellAndBombPool.Instance.GetShell();
////        ConfigureShell(shell);
////        ShellAndBombPool.Instance.ReturnToPool(shell, shellLifetime);
////    }

////    private bool ValidateShellComponents()
////    {
////        return shellEjectionPoint != null && shellPrefab != null;
////    }

////    private void ConfigureShell(GameObject shell)
////    {
////        shell.transform.SetPositionAndRotation(shellEjectionPoint.position, shellEjectionPoint.rotation);

////        if (shell.TryGetComponent<Rigidbody>(out var shellRb))
////        {
////            // Aktivera gravitation
////            shellRb.useGravity = true;

////            // Grundkrafter för utkast
////            float baseForce = Random.Range(3f, 5f);
////            float upwardForce = Random.Range(2f, 4f);
////            float sideForce = Random.Range(-4f, 1f);

////            // Använd cachedTransform
////            Vector3 totalForce = cachedTransform.right * baseForce +
////                               cachedTransform.up * upwardForce +
////                               cachedTransform.forward * sideForce;

////            shellRb.AddForce(totalForce, ForceMode.Impulse);

////            // Rotation
////            shellRb.AddTorque(
////                Random.Range(-5f, 5f),
////                Random.Range(-5f, 5f),
////                Random.Range(-5f, 5f),
////                ForceMode.Impulse
////            );

////            // Se till att den inte "sover"
////            shellRb.sleepThreshold = 0;
////            shellRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
////        }
////    }

////    private void PlayFireEffects()
////    {
////        AudioManager.Instance?.PlayCombatSound(CombatSoundType.PlayerShoot);
////    }

////    private void UpdateFireCooldown()
////    {
////        nextFireTime = Time.time + fireRate;
////    }

////    public IEnumerator ApplyFireRateBoost(float multiplier, float duration)
////    {
////        float originalFireRate = fireRate;
////        fireRate /= multiplier;
////        yield return new WaitForSeconds(duration);
////        fireRate = originalFireRate;
////    }

////    public IEnumerator EnableDualWeapons(float duration)
////    {
////        dualWeaponsEnabled = true;
////        yield return new WaitForSeconds(duration);
////        dualWeaponsEnabled = false;
////    }
////}

////using UnityEngine;
////using System.Collections;

////public class WeaponSystem : MonoBehaviour
////{
////    [Header("Weapon Settings")]
////    [SerializeField] private Transform weaponPoint;
////    [SerializeField] private Transform leftGunPoint;
////    [SerializeField] private Transform rightGunPoint;
////    [SerializeField] private GameObject bulletPrefab;
////    [SerializeField] private float bulletSpeed = 20f;
////    [SerializeField] private float fireRate = 0.2f;
////    [SerializeField] private float bulletLifetime = 2f;

////    [Header("Shells Settings")]
////    [SerializeField] private Transform shellEjectionPoint;
////    [SerializeField] private GameObject shellPrefab;
////    [SerializeField] private float shellLifetime = 3f;
////    [SerializeField] private float minShellEjectionForce = 3f;
////    [SerializeField] private float maxShellEjectionForce = 6f;
////    [SerializeField] private float minShellTorque = 3f;
////    [SerializeField] private float maxShellTorque = 8f;

////    [Header("Boost Settings")]
////    [SerializeField] private float fireRateBoostDuration = 10f;
////    [SerializeField] private float dualWeaponsDuration = 15f;

////    private float nextFireTime;
////    private float originalFireRate;
////    private bool dualWeaponsEnabled = false;
////    private AudioManager audioManager;
////    private bool canFire = true;
////    private bool isInitialized;
////    private readonly Vector3 BulletDirection = Vector3.forward;
////    private readonly Vector3 ShellEjectionOffset = Vector3.up * 0.5f;

////    private PlaneHealthSystem planeHealth;

////    // CACHEA KOMPONENTER FÖR BÄTTRE PRESTANDA
////    private BulletSystem cachedBulletSystem;
////    private Rigidbody cachedBulletRb;
////    private GameObject currentBullet;

////    // CACHEA TRANSFORMS
////    private Transform cachedTransform;

////    private void Awake()
////    {
////        // Cachea transform för bättre prestanda
////        cachedTransform = transform;
////    }

////    private void Start()
////    {
////        InitializeComponents();
////        originalFireRate = fireRate;
////    }

////    private void InitializeComponents()
////    {
////        // Cachea komponenter
////        audioManager = AudioManager.Instance;
////        planeHealth = GetComponent<PlaneHealthSystem>();
////        if (planeHealth == null)
////        {
////            Debug.LogWarning("PlaneHealthSystem hittades inte på spelaren!");
////        }
////        ValidateComponents();
////        isInitialized = true;
////    }

////    private void ValidateComponents()
////    {
////        if (weaponPoint == null) Debug.LogWarning("WeaponPoint saknas!");
////        if (bulletPrefab == null) Debug.LogWarning("BulletPrefab saknas!");
////        if (shellEjectionPoint == null) Debug.LogWarning("ShellEjectionPoint saknas!");
////        if (shellPrefab == null) Debug.LogWarning("ShellPrefab saknas!");
////    }

////    private bool CanFire()
////    {
////        // Tillåt skjutning även när spelaren är odödlig
////        return Time.time >= nextFireTime;
////    }

////    private void Update()
////    {
////        if (!isInitialized || !canFire || (planeHealth != null && planeHealth.IsDead()))
////        {
////            return;
////        }
////        HandleWeaponInput();
////    }

////    public void EnableWeapons(bool enable)
////    {
////        // Tillåt bara aktivering av vapen om spelaren lever
////        if (planeHealth != null && planeHealth.IsDead())
////        {
////            canFire = false;
////            return;
////        }
////        canFire = enable;
////    }

////    private void HandleWeaponInput()
////    {
////#if UNITY_EDITOR
////        HandleEditorInput();
////#else
////        HandleMobileInput();
////#endif
////    }

////    private void HandleEditorInput()
////    {
////        if (Input.GetKey(KeyCode.Space) && CanFire())
////        {
////            Fire();
////        }
////    }

////    private void HandleMobileInput()
////    {
////        if (Input.touchCount > 1 && CanFire())
////        {
////            Fire();
////        }
////    }

////    private void Fire()
////    {
////        if (!ValidateWeaponComponents()) return;

////        if (dualWeaponsEnabled)
////        {
////            SpawnBullet(leftGunPoint.position);
////            SpawnBullet(rightGunPoint.position);
////        }
////        else
////        {
////            SpawnBullet(weaponPoint.position);
////        }

////        EjectShell();
////        PlayFireEffects();
////        UpdateFireCooldown();
////    }

////    private bool ValidateWeaponComponents()
////    {
////        return weaponPoint != null && bulletPrefab != null;
////    }

////    private void SpawnBullet(Vector3 spawnPosition)
////    {
////        // OPTIMERAD: Cachea komponenter för att minska GetComponent-anrop
////        currentBullet = BulletPool.Instance.GetBullet(true);

////        if (cachedBulletSystem == null || cachedBulletSystem.gameObject != currentBullet)
////        {
////            cachedBulletSystem = currentBullet.GetComponent<BulletSystem>();
////        }

////        if (cachedBulletRb == null || cachedBulletRb.gameObject != currentBullet)
////        {
////            cachedBulletRb = currentBullet.GetComponent<Rigidbody>();
////        }

////        currentBullet.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
////        cachedBulletSystem.Initialize(Vector3.forward, false, 10f);
////        cachedBulletRb.useGravity = false;
////        cachedBulletRb.linearVelocity = BulletDirection * bulletSpeed;
////    }

////    public void ResetFireRate()
////    {
////        fireRate = originalFireRate;
////    }

////    private void EjectShell()
////    {
////        if (!ValidateShellComponents()) return;

////        GameObject shell = ShellAndBombPool.Instance.GetShell();
////        ConfigureShell(shell);
////        ShellAndBombPool.Instance.ReturnToPool(shell, shellLifetime);
////    }

////    private bool ValidateShellComponents()
////    {
////        return shellEjectionPoint != null && shellPrefab != null;
////    }

////    private void ConfigureShell(GameObject shell)
////    {
////        shell.transform.SetPositionAndRotation(shellEjectionPoint.position, shellEjectionPoint.rotation);

////        if (shell.TryGetComponent<Rigidbody>(out var shellRb))
////        {
////            // Aktivera gravitation
////            shellRb.useGravity = true;

////            // Grundkrafter för utkast
////            float baseForce = Random.Range(3f, 5f);
////            float upwardForce = Random.Range(2f, 4f);
////            float sideForce = Random.Range(-4f, 1f);

////            // Använd cachedTransform
////            Vector3 totalForce = cachedTransform.right * baseForce +
////                               cachedTransform.up * upwardForce +
////                               cachedTransform.forward * sideForce;

////            shellRb.AddForce(totalForce, ForceMode.Impulse);

////            // Rotation
////            shellRb.AddTorque(
////                Random.Range(-5f, 5f),
////                Random.Range(-5f, 5f),
////                Random.Range(-5f, 5f),
////                ForceMode.Impulse
////            );

////            // Se till att den inte "sover"
////            shellRb.sleepThreshold = 0;
////            shellRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
////        }
////    }

////    private void PlayFireEffects()
////    {
////        AudioManager.Instance?.PlayCombatSound(CombatSoundType.PlayerShoot);
////    }

////    private void UpdateFireCooldown()
////    {
////        nextFireTime = Time.time + fireRate;
////    }

////    public IEnumerator ApplyFireRateBoost(float multiplier, float duration)
////    {
////        float originalFireRate = fireRate;
////        fireRate /= multiplier;
////        yield return new WaitForSeconds(duration);
////        fireRate = originalFireRate;
////    }

////    public IEnumerator EnableDualWeapons(float duration)
////    {
////        dualWeaponsEnabled = true;
////        yield return new WaitForSeconds(duration);
////        dualWeaponsEnabled = false;
////    }
////}

//////using UnityEngine;
//////using System.Collections;

//////public class WeaponSystem : MonoBehaviour
//////{
//////    [Header("Weapon Settings")]
//////    [SerializeField] private Transform weaponPoint;
//////    [SerializeField] private Transform leftGunPoint;
//////    [SerializeField] private Transform rightGunPoint;
//////    [SerializeField] private GameObject bulletPrefab;
//////    [SerializeField] private float bulletSpeed = 20f;
//////    [SerializeField] private float fireRate = 0.2f;
//////    [SerializeField] private float bulletLifetime = 2f;

//////    [Header("Shells Settings")]
//////    [SerializeField] private Transform shellEjectionPoint;
//////    [SerializeField] private GameObject shellPrefab;
//////    [SerializeField] private float shellLifetime = 3f;
//////    [SerializeField] private float minShellEjectionForce = 3f;
//////    [SerializeField] private float maxShellEjectionForce = 6f;
//////    [SerializeField] private float minShellTorque = 3f;
//////    [SerializeField] private float maxShellTorque = 8f;

//////    [Header("Boost Settings")]
//////    [SerializeField] private float fireRateBoostDuration = 10f;
//////    [SerializeField] private float dualWeaponsDuration = 15f;

//////    private float nextFireTime;
//////    private float originalFireRate;
//////    private bool dualWeaponsEnabled = false;
//////    private AudioManager audioManager;
//////    private bool canFire = true;
//////    private bool isInitialized;
//////    private readonly Vector3 BulletDirection = Vector3.forward;
//////    private readonly Vector3 ShellEjectionOffset = Vector3.up * 0.5f;

//////    private PlaneHealthSystem healthSystem;

//////    private PlaneHealthSystem planeHealth;

//////    private void Start()
//////    {
//////        InitializeComponents();
//////        originalFireRate = fireRate;

//////        healthSystem = GetComponent<PlaneHealthSystem>();
//////    }

//////    private void InitializeComponents()
//////    {
//////        audioManager = AudioManager.Instance;
//////        planeHealth = GetComponent<PlaneHealthSystem>();
//////        if (planeHealth == null)
//////        {
//////            Debug.LogWarning("PlaneHealthSystem hittades inte på spelaren!");
//////        }
//////        ValidateComponents();
//////        isInitialized = true;
//////    }

//////    private void ValidateComponents()
//////    {
//////        if (weaponPoint == null) Debug.LogWarning("WeaponPoint saknas!");
//////        if (bulletPrefab == null) Debug.LogWarning("BulletPrefab saknas!");
//////        if (shellEjectionPoint == null) Debug.LogWarning("ShellEjectionPoint saknas!");
//////        if (shellPrefab == null) Debug.LogWarning("ShellPrefab saknas!");
//////    }

//////    private bool CanFire()
//////    {
//////        // Tillåt skjutning även när spelaren är odödlig
//////        return Time.time >= nextFireTime;
//////    }
//////    private void Update()
//////    {
//////        if (!isInitialized || !canFire || (planeHealth != null && planeHealth.IsDead()))
//////        {
//////            return;
//////        }
//////        HandleWeaponInput();
//////    }

//////    public void EnableWeapons(bool enable)
//////    {
//////        // Tillåt bara aktivering av vapen om spelaren lever
//////        if (planeHealth != null && planeHealth.IsDead())
//////        {
//////            canFire = false;
//////            return;
//////        }
//////        canFire = enable;
//////    }

//////    private void HandleWeaponInput()
//////    {
//////#if UNITY_EDITOR
//////        HandleEditorInput();
//////#else
//////        HandleMobileInput();
//////#endif
//////    }

//////    private void HandleEditorInput()
//////    {
//////        if (Input.GetKey(KeyCode.Space) && CanFire())
//////        {
//////            Fire();
//////        }
//////    }

//////    private void HandleMobileInput()
//////    {
//////        if (Input.touchCount > 1 && CanFire())
//////        {
//////            Fire();
//////        }
//////    }



//////    private void Fire()
//////    {
//////        if (!ValidateWeaponComponents()) return;

//////        if (dualWeaponsEnabled)
//////        {
//////            SpawnBullet(leftGunPoint.position);
//////            SpawnBullet(rightGunPoint.position);
//////        }
//////        else
//////        {
//////            SpawnBullet(weaponPoint.position);
//////        }

//////        EjectShell();
//////        PlayFireEffects();
//////        UpdateFireCooldown();
//////    }

//////    private bool ValidateWeaponComponents()
//////    {
//////        return weaponPoint != null && bulletPrefab != null;
//////    }

//////    private void SpawnBullet(Vector3 spawnPosition)
//////    {
//////        // Cacha komponenter för att minska GetComponent-anrop
//////        GameObject bullet = BulletPool.Instance.GetBullet(true);

//////        BulletSystem bulletSystem = bullet.GetComponent<BulletSystem>();
//////        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

//////        bullet.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);

//////        bulletSystem.Initialize(Vector3.forward, false, 10f);

//////        bulletRb.useGravity = false;
//////        bulletRb.linearVelocity = BulletDirection * bulletSpeed;
//////    }

//////    public void ResetFireRate()
//////    {
//////        fireRate = originalFireRate;
//////    }

//////    private void EjectShell()
//////    {
//////        if (!ValidateShellComponents()) return;

//////        GameObject shell = InstantiateShell();
//////        ConfigureShell(shell);
//////        Destroy(shell, shellLifetime);
//////    }

//////    private bool ValidateShellComponents()
//////    {
//////        return shellEjectionPoint != null && shellPrefab != null;
//////    }

//////    private GameObject InstantiateShell()
//////    {
//////        return Instantiate(shellPrefab, shellEjectionPoint.position, shellEjectionPoint.rotation);
//////    }

//////    private void ConfigureShell(GameObject shell)
//////    {
//////        if (shell.TryGetComponent<Rigidbody>(out var shellRb))
//////        {
//////            // Aktivera gravitation
//////            shellRb.useGravity = true;

//////            // Grundkrafter för utkast
//////            float baseForce = Random.Range(3f, 5f);
//////            float upwardForce = Random.Range(2f, 4f);
//////            // Ändrad för att favorisera bakåtriktning (-4 till 1 istället för -2 till 2)
//////            float sideForce = Random.Range(-4f, 1f);

//////            // Applicera krafter
//////            Vector3 totalForce = transform.right * baseForce +      // Huvudriktning (åt höger)
//////                               transform.up * upwardForce +         // Uppåt
//////                               transform.forward * sideForce;       // Mestadels bakåt

//////            shellRb.AddForce(totalForce, ForceMode.Impulse);

//////            // Rotation
//////            shellRb.AddTorque(
//////                Random.Range(-5f, 5f),
//////                Random.Range(-5f, 5f),
//////                Random.Range(-5f, 5f),
//////                ForceMode.Impulse
//////            );

//////            // Se till att den inte "sover"
//////            shellRb.sleepThreshold = 0;
//////            shellRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
//////        }
//////    }

//////    private void PlayFireEffects()
//////    {
//////        AudioManager.Instance?.PlayCombatSound(CombatSoundType.PlayerShoot);
//////    }

//////    private void UpdateFireCooldown()
//////    {
//////        nextFireTime = Time.time + fireRate;
//////    }

//////    // Boost metoder
//////    public IEnumerator ApplyFireRateBoost(float duration)
//////    {
//////        float boostedFireRate = fireRate * 0.5f;
//////        fireRate = boostedFireRate;
//////        yield return new WaitForSeconds(duration);
//////        fireRate = originalFireRate;
//////    }

//////    public IEnumerator EnableDualWeapons(float duration)
//////    {
//////        dualWeaponsEnabled = true;
//////        yield return new WaitForSeconds(duration);
//////        dualWeaponsEnabled = false;
//////    }

//////    public IEnumerator ApplyFireRateBoost(float multiplier, float duration)
//////    {
//////        float originalFireRate = fireRate;
//////        fireRate /= multiplier;
//////        yield return new WaitForSeconds(duration);
//////        fireRate = originalFireRate;
//////    }
//////}

////////using UnityEngine;
////////using System.Collections;

////////public class WeaponSystem : MonoBehaviour
////////{
////////    [Header("Weapon Settings")]
////////    [SerializeField] private Transform weaponPoint;
////////    [SerializeField] private Transform leftGunPoint;
////////    [SerializeField] private Transform rightGunPoint;
////////    [SerializeField] private GameObject bulletPrefab;
////////    [SerializeField] private float bulletSpeed = 20f;
////////    [SerializeField] private float fireRate = 0.2f;
////////    [SerializeField] private float bulletLifetime = 2f;

////////    [Header("Shells Settings")]
////////    [SerializeField] private Transform shellEjectionPoint;
////////    [SerializeField] private GameObject shellPrefab;
////////    [SerializeField] private float shellLifetime = 3f;
////////    [SerializeField] private float minShellEjectionForce = 3f;
////////    [SerializeField] private float maxShellEjectionForce = 6f;
////////    [SerializeField] private float minShellTorque = 3f;
////////    [SerializeField] private float maxShellTorque = 8f;

////////    [Header("Boost Settings")]
////////    [SerializeField] private float fireRateBoostDuration = 10f;
////////    [SerializeField] private float dualWeaponsDuration = 15f;

////////    private float nextFireTime;
////////    private float originalFireRate;
////////    private bool dualWeaponsEnabled = false;
////////    private AudioManager audioManager;
////////    private bool canFire = true;
////////    private bool isInitialized;
////////    private readonly Vector3 BulletDirection = Vector3.forward;
////////    private readonly Vector3 ShellEjectionOffset = Vector3.up * 0.5f;

////////    private PlaneHealthSystem planeHealth;

////////    private void Start()
////////    {
////////        InitializeComponents();
////////        originalFireRate = fireRate;
////////    }

////////    private void InitializeComponents()
////////    {
////////        audioManager = AudioManager.Instance;
////////        planeHealth = GetComponent<PlaneHealthSystem>();
////////        if (planeHealth == null)
////////        {
////////            Debug.LogWarning("PlaneHealthSystem hittades inte på spelaren!");
////////        }
////////        ValidateComponents();
////////        isInitialized = true;
////////    }

////////    private void ValidateComponents()
////////    {
////////        if (weaponPoint == null) Debug.LogWarning("WeaponPoint saknas!");
////////        if (bulletPrefab == null) Debug.LogWarning("BulletPrefab saknas!");
////////        if (shellEjectionPoint == null) Debug.LogWarning("ShellEjectionPoint saknas!");
////////        if (shellPrefab == null) Debug.LogWarning("ShellPrefab saknas!");
////////    }

////////    private void Update()
////////    {
////////        if (!isInitialized || !canFire || (planeHealth != null && planeHealth.IsDead()))
////////        {
////////            return;
////////        }
////////        HandleWeaponInput();
////////    }

////////    public void EnableWeapons(bool enable)
////////    {
////////        // Tillåt bara aktivering av vapen om spelaren lever
////////        if (planeHealth != null && planeHealth.IsDead())
////////        {
////////            canFire = false;
////////            return;
////////        }
////////        canFire = enable;
////////    }

////////    private void HandleWeaponInput()
////////    {
////////#if UNITY_EDITOR
////////        HandleEditorInput();
////////#else
////////        HandleMobileInput();
////////#endif
////////    }

////////    private void HandleEditorInput()
////////    {
////////        if (Input.GetKey(KeyCode.Space) && CanFire())
////////        {
////////            Fire();
////////        }
////////    }

////////    private void HandleMobileInput()
////////    {
////////        if (Input.touchCount > 1 && CanFire())
////////        {
////////            Fire();
////////        }
////////    }

////////    private bool CanFire() => Time.time >= nextFireTime;

////////    private void Fire()
////////    {
////////        if (!ValidateWeaponComponents()) return;

////////        if (dualWeaponsEnabled)
////////        {
////////            SpawnBullet(leftGunPoint.position);
////////            SpawnBullet(rightGunPoint.position);
////////        }
////////        else
////////        {
////////            SpawnBullet(weaponPoint.position);
////////        }

////////        EjectShell();
////////        PlayFireEffects();
////////        UpdateFireCooldown();
////////    }

////////    private bool ValidateWeaponComponents()
////////    {
////////        return weaponPoint != null && bulletPrefab != null;
////////    }

////////    private void SpawnBullet(Vector3 spawnPosition)
////////    {
////////        GameObject bullet = BulletPool.Instance.GetBullet(true);
////////        BulletSystem bulletSystem = bullet.GetComponent<BulletSystem>();
////////        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

////////        bullet.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
////////        bulletSystem.Initialize(Vector3.forward, false, 10f);
////////        bulletRb.useGravity = false;
////////        bulletRb.linearVelocity = BulletDirection * bulletSpeed;
////////    }

////////    public void ResetFireRate()
////////    {
////////        fireRate = originalFireRate;
////////    }

////////    private void EjectShell()
////////    {
////////        if (!ValidateShellComponents()) return;

////////        GameObject shell = InstantiateShell();
////////        ConfigureShell(shell);
////////        Destroy(shell, shellLifetime);
////////    }

////////    private bool ValidateShellComponents()
////////    {
////////        return shellEjectionPoint != null && shellPrefab != null;
////////    }

////////    private GameObject InstantiateShell()
////////    {
////////        return Instantiate(shellPrefab, shellEjectionPoint.position, shellEjectionPoint.rotation);
////////    }

////////    private void ConfigureShell(GameObject shell)
////////    {
////////        if (shell.TryGetComponent<Rigidbody>(out var shellRb))
////////        {
////////            // Aktivera gravitation
////////            shellRb.useGravity = true;

////////            // Grundkrafter för utkast
////////            float baseForce = Random.Range(3f, 5f);
////////            float upwardForce = Random.Range(2f, 4f);
////////            // Ändrad för att favorisera bakåtriktning (-4 till 1 istället för -2 till 2)
////////            float sideForce = Random.Range(-4f, 1f);

////////            // Applicera krafter
////////            Vector3 totalForce = transform.right * baseForce +      // Huvudriktning (åt höger)
////////                               transform.up * upwardForce +         // Uppåt
////////                               transform.forward * sideForce;       // Mestadels bakåt

////////            shellRb.AddForce(totalForce, ForceMode.Impulse);

////////            // Rotation
////////            shellRb.AddTorque(
////////                Random.Range(-5f, 5f),
////////                Random.Range(-5f, 5f),
////////                Random.Range(-5f, 5f),
////////                ForceMode.Impulse
////////            );

////////            // Se till att den inte "sover"
////////            shellRb.sleepThreshold = 0;
////////            shellRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
////////        }
////////    }

////////    private void PlayFireEffects()
////////    {
////////        AudioManager.Instance?.PlayCombatSound(CombatSoundType.PlayerShoot);
////////    }

////////    private void UpdateFireCooldown()
////////    {
////////        nextFireTime = Time.time + fireRate;
////////    }

////////    public IEnumerator ApplyFireRateBoost(float duration)
////////    {
////////        float boostedFireRate = fireRate * 0.5f;
////////        fireRate = boostedFireRate;
////////        yield return new WaitForSeconds(duration);
////////        fireRate = originalFireRate;
////////    }

////////    public IEnumerator EnableDualWeapons(float duration)
////////    {
////////        dualWeaponsEnabled = true;
////////        yield return new WaitForSeconds(duration);
////////        dualWeaponsEnabled = false;
////////    }

////////    public IEnumerator ApplyFireRateBoost(float multiplier, float duration)
////////    {
////////        float originalFireRate = fireRate;
////////        fireRate /= multiplier;
////////        yield return new WaitForSeconds(duration);
////////        fireRate = originalFireRate;
////////    }
////////}
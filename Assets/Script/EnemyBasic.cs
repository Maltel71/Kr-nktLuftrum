using UnityEngine;

public class EnemyBasic : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private bool useWaveMovement;
    [SerializeField] private float waveAmplitude = 2f;
    [SerializeField] private float waveFrequency = 2f;

    [Header("Target & Range")]
    [SerializeField] private float shootingRange = 20f;
    private Transform target;

    [Header("Weapon Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform leftGun;
    [SerializeField] private Transform rightGun;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float bulletDamage = 20f;
    [SerializeField] private float bulletSpawnOffset = 5f;

    [Header("Collision Settings")]
    [SerializeField] private float collisionDamage = 25f;
    [SerializeField] private bool destroyOnCollision = true;

    // Private variables
    private float nextFireTime;
    private bool useLeftGun = true;
    private AudioManager audioManager;
    private Vector3 initialPosition;
    private EnemyHealth healthSystem;

    private void Start()
    {
        InitializeComponents();
        SetupTarget();
    }

    private void InitializeComponents()
    {
        audioManager = AudioManager.Instance;
        healthSystem = GetComponent<EnemyHealth>();
        initialPosition = transform.position;

        if (healthSystem == null)
        {
            Debug.LogWarning("EnemyHealth saknas p� " + gameObject.name);
        }
    }

    private void SetupTarget()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogWarning("Kunde inte hitta spelaren!");
            }
        }
    }

    private void Update()
    {
        if (target == null || healthSystem.IsDying) return;

        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        // Basr�relse fram�t
        Vector3 movement = -transform.forward * moveSpeed;

        if (useWaveMovement)
        {
            // L�gg till sidledsr�relse i en v�gform
            float sin = Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
            Vector3 sideVector = transform.right;
            movement += sideVector * sin;
        }

        transform.position += movement * Time.deltaTime;
    }

    private void HandleShooting()
    {
        float distance = Vector3.Distance(transform.position, target.position);

        // Ber�kna vinkeln mellan fienden och spelaren
        Vector3 directionToTarget = target.position - transform.position;
        float angle = Vector3.Angle(transform.forward, directionToTarget);

        // Skjut bara om:
        // 1. Spelaren �r inom r�ckh�ll
        // 2. Spelaren �r framf�r fienden (inom en 45-graders kon)
        // 3. Det �r dags att skjuta igen (cooldown �r klar)
        if (distance <= shootingRange && angle < 45f && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        // Anv�nd bara en skjutpunkt, till exempel rightGun
        Transform currentGun = rightGun;

        // Skjut alltid rakt fram i fiendensriktning
        Vector3 shootDirection = transform.forward;
        Vector3 spawnPosition = currentGun.position + shootDirection * bulletSpawnOffset;

        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.LookRotation(shootDirection));

        if (bullet.TryGetComponent<BulletSystem>(out var bulletSystem))
        {
            bulletSystem.Initialize(shootDirection, true, bulletDamage);
        }

        audioManager?.PlayShootSound();
    }

    //private void HandleShooting()
    //{
    //    float distance = Vector3.Distance(transform.position, target.position);
    //    // L�gg till CanShoot() i villkoret
    //    if (distance <= shootingRange && Time.time >= nextFireTime && CanShoot())
    //    {
    //        Shoot();
    //        nextFireTime = Time.time + fireRate;
    //        useLeftGun = !useLeftGun;
    //    }
    //}


    //private void Shoot()
    //{
    //    // H�mta aktuell kanon
    //    Transform currentGun = useLeftGun ? leftGun : rightGun;
    //    Vector3 shootDirection = (target.position - currentGun.position).normalized;
    //    Vector3 spawnPosition = currentGun.position + shootDirection * bulletSpawnOffset;

    //    GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.LookRotation(shootDirection));

    //    if (bullet.TryGetComponent<BulletSystem>(out var bulletSystem))
    //    {
    //        bulletSystem.Initialize(shootDirection, true, bulletDamage);
    //    }

    //    audioManager?.PlayShootSound();

    //    // V�xla mellan v�nster och h�ger kanon f�r n�sta skott
    //    useLeftGun = !useLeftGun;
    //}

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent<PlaneHealthSystem>(out var playerHealth))
            {
                playerHealth.TakeDamage(collisionDamage);
                AudioManager.Instance?.PlayCombatSound(CombatSoundType.Hit);

                if (destroyOnCollision)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    private bool CanShoot()
    {
        if (target == null) return false;

        // Ber�kna vinkeln mellan fienden och spelaren
        Vector3 directionToTarget = target.position - transform.position;
        float angle = Vector3.Angle(transform.forward, directionToTarget);

        // Om spelaren �r bakom fienden (mer �n 90 grader), returnera false
        return angle < 90f;
    }

    

    // Public methods f�r extern kontroll
    public void SetMoveSpeed(float speed) => moveSpeed = speed;
    public void SetFireRate(float rate) => fireRate = rate;
    public void SetBulletDamage(float damage) => bulletDamage = damage;
}
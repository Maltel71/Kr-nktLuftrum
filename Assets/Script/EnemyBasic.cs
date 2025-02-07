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

    [Header("Shooting Settings")]
    [SerializeField] private bool continuousShooting = false; // Ny inställning

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
            Debug.LogWarning("EnemyHealth saknas pе " + gameObject.name);
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
        // Basrцrelse framеt
        Vector3 movement = -transform.forward * moveSpeed;

        if (useWaveMovement)
        {
            // Lдgg till sidledsrцrelse i en vеgform
            float sin = Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
            Vector3 sideVector = transform.right;
            movement += sideVector * sin;
        }

        transform.position += movement * Time.deltaTime;
    }

    private void HandleShooting()
    {
        float distance = Vector3.Distance(transform.position, target.position);

        // Om continuousShooting är true, eller om spelaren är synlig
        bool shouldShoot = continuousShooting || CanShoot();

        if (distance <= shootingRange && Time.time >= nextFireTime && shouldShoot)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }


    private void Shoot()
    {
        // Välj aktuell kanon baserat på `useLeftGun`
        Transform currentGun = useLeftGun ? leftGun : rightGun;

        // Skapa bullet i fiendens framåtriktning
        Vector3 shootDirection = transform.forward;

        GameObject bullet = Instantiate(bulletPrefab, currentGun.position, Quaternion.LookRotation(shootDirection));

        if (bullet.TryGetComponent<BulletSystem>(out var bulletSystem))
        {
            bulletSystem.Initialize(shootDirection, true, bulletDamage);
        }

        audioManager?.PlayShootSound();

        // Växla mellan vänster och höger kanon
        useLeftGun = !useLeftGun;
    }

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

        // Beräkna riktningen till spelaren
        Vector3 directionToTarget = target.position - transform.position;

        // Beräkna vinkeln mellan fiendens framåtriktning och riktningen till spelaren
        float angle = Vector3.Angle(transform.forward, directionToTarget);

        // Begränsa skjutning till mycket liten vinkel, t.ex. 15 grader
        return angle < 15f;
    }



    // Public methods fцr extern kontroll
    public void SetMoveSpeed(float speed) => moveSpeed = speed;
    public void SetFireRate(float rate) => fireRate = rate;
    public void SetBulletDamage(float damage) => bulletDamage = damage;
}
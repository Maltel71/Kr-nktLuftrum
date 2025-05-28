using UnityEngine;

public class HelicopterEnemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float hoverHeight = 15f;
    [SerializeField] private float hoverRadius = 10f;
    [SerializeField] private float rotationSpeed = 2f;

    [Header("Rotor")]
    [SerializeField] private Transform mainRotor;
    [SerializeField] private Transform tailRotor;
    [SerializeField] private float mainRotorSpeed = 1000f;
    [SerializeField] private float tailRotorSpeed = 2000f;

    [Header("Combat")]
    [SerializeField] private Transform[] gunPoints;
    [SerializeField] private float fireRate = 0.3f;
    [SerializeField] private float bulletSpeed = 30f;
    [SerializeField] private float bulletDamage = 15f;
    [SerializeField] private float attackRange = 25f;
    [SerializeField] private float aimAheadMultiplier = 0.5f;

    [Header("Behavior")]
    [SerializeField] private float strafeSpeed = 8f;
    [SerializeField] private float strafeChangeInterval = 3f;

    private Transform target;
    private float nextFireTime;
    private float nextStrafeChange;
    private int currentGunIndex;
    private float strafeDirection = 1f;
    private Vector3 hoverCenter;
    private float hoverAngle;
    private EnemyHealth healthSystem;
    private AudioManager audioManager;

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player")?.transform;
        healthSystem = GetComponent<EnemyHealth>();
        audioManager = AudioManager.Instance;
        hoverCenter = transform.position;

        // Spela helikopterljud
        if (audioManager != null)
        {
            audioManager.PlayVehicleEngineSound(VehicleType.Helicopter, transform.position);
        }
    }

    void Update()
    {
        if (healthSystem != null && healthSystem.IsDying) return;
        if (target == null) return;

        UpdateRotors();
        HandleMovement();
        HandleCombat();
    }

    void UpdateRotors()
    {
        if (mainRotor != null)
            mainRotor.Rotate(0, mainRotorSpeed * Time.deltaTime, 0);

        if (tailRotor != null)
            tailRotor.Rotate(tailRotorSpeed * Time.deltaTime, 0, 0);
    }

    void HandleMovement()
    {
        // Cirkulär rörelse runt spelarens position
        hoverAngle += moveSpeed * Time.deltaTime;

        Vector3 offset = new Vector3(
            Mathf.Sin(hoverAngle) * hoverRadius,
            0,
            Mathf.Cos(hoverAngle) * hoverRadius
        );

        Vector3 targetPos = target.position + offset;
        targetPos.y = hoverHeight;

        // Strafing för att undvika att vara ett lätt mål
        if (Time.time > nextStrafeChange)
        {
            strafeDirection = -strafeDirection;
            nextStrafeChange = Time.time + strafeChangeInterval;
        }

        Vector3 strafeOffset = transform.right * strafeDirection * strafeSpeed * Time.deltaTime;
        targetPos += strafeOffset;

        // Mjuk förflyttning
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 2f);

        // Rotera mot spelaren
        Vector3 lookDirection = target.position - transform.position;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void HandleCombat()
    {
        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= attackRange && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Fire()
    {
        if (gunPoints.Length == 0) return;

        Transform gunPoint = gunPoints[currentGunIndex];

        // Sikta framför spelaren baserat på dess rörelse
        Vector3 targetVelocity = Vector3.zero;
        if (target.TryGetComponent<Rigidbody>(out var targetRb))
        {
            targetVelocity = targetRb.linearVelocity;
        }

        Vector3 aimPoint = target.position + targetVelocity * aimAheadMultiplier;
        Vector3 shootDirection = (aimPoint - gunPoint.position).normalized;

        // Använd bullet pool
        GameObject bullet = BulletPool.Instance.GetBullet(false);
        bullet.transform.position = gunPoint.position;
        bullet.transform.rotation = Quaternion.LookRotation(shootDirection);

        if (bullet.TryGetComponent<BulletSystem>(out var bulletSystem))
        {
            bulletSystem.Initialize(shootDirection, true, bulletDamage);
        }

        audioManager?.PlayEnemyShootSound();

        // Byt mellan vapenpunkter
        currentGunIndex = (currentGunIndex + 1) % gunPoints.Length;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Skada spelaren vid kollision
            if (collision.gameObject.TryGetComponent<PlaneHealthSystem>(out var playerHealth))
            {
                playerHealth.TakeDamage(40f);

                // Skapa explosion
                GameObject explosion = ExplosionPool.Instance.GetExplosion(ExplosionType.Large);
                explosion.transform.position = collision.contacts[0].point;
                ExplosionPool.Instance.ReturnExplosionToPool(explosion, 2f);

                AudioManager.Instance?.PlayCombatSound(CombatSoundType.Hit);
                CameraShake.Instance?.ShakaCameraVidBomb();

                // Förstör helikoptern
                if (healthSystem != null)
                {
                    healthSystem.StartDying();
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    // För att andra script ska kunna justera beteende
    public void SetCombatMode(bool aggressive)
    {
        if (aggressive)
        {
            fireRate = 0.2f;
            moveSpeed = 6f;
            strafeSpeed = 10f;
        }
        else
        {
            fireRate = 0.4f;
            moveSpeed = 4f;
            strafeSpeed = 8f;
        }
    }
}
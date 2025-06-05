using UnityEngine;

public class HelicopterEnemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float hoverHeight = 15f;
    [SerializeField] private float circleRadius = 12f;
    [SerializeField] private float rotationSpeed = 2f;

    [Header("Combat")]
    [SerializeField] private Transform[] gunPoints;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float bulletSpeed = 30f;
    [SerializeField] private float bulletDamage = 15f;
    [SerializeField] private float attackRange = 25f;
    [SerializeField] private float bulletLifetime = 2f;

    [Header("Flight Settings")]
    [SerializeField] private float minDistanceToPlayer = 8f;
    [SerializeField] private float destroyDistanceBehindPlayer = 30f;

    private Transform target;
    private float nextFireTime;
    private int currentGunIndex;
    private EnemyHealth healthSystem;
    private AudioManager audioManager;
    private bool helicopterSoundPlaying = false;
    private float circleAngle;

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player")?.transform;
        healthSystem = GetComponent<EnemyHealth>();
        audioManager = AudioManager.Instance;

        // Spela helikopterljud
        StartHelicopterSound();

        // Om inga gunPoints angivits, använd transform position
        if (gunPoints == null || gunPoints.Length == 0)
        {
            gunPoints = new Transform[1] { transform };
        }

        // Sätt slumpmässig startvinkel för cirkling
        circleAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
    }

    void Update()
    {
        if (healthSystem != null && healthSystem.IsDying)
        {
            StopHelicopterSound();
            return;
        }

        if (target == null) return;

        HandleMovement();
        HandleCombat();
        CheckIfBehindPlayer();
    }

    void HandleMovement()
    {
        Vector3 playerPos = target.position;
        Vector3 currentPos = transform.position;

        // Beräkna önskad position (cirkel runt spelaren)
        circleAngle += moveSpeed * 0.5f * Time.deltaTime;

        Vector3 targetPosition = playerPos + new Vector3(
            Mathf.Cos(circleAngle) * circleRadius,
            hoverHeight,
            Mathf.Sin(circleAngle) * circleRadius
        );

        // Flyg mot målpositionen med mjuk rörelse
        transform.position = Vector3.Lerp(currentPos, targetPosition, moveSpeed * 0.5f * Time.deltaTime);

        // ALLTID rotera för att titta mot spelaren
        Vector3 lookDirection = playerPos - transform.position;
        lookDirection.y = 0; // Håll rotation på horisontellt plan

        if (lookDirection.magnitude > 0.1f)
        {
            // Prova olika rotations-offset för att hitta rätt front
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

            // Lägg till 90 grader om helikoptern har fel front
            // Prova dessa en i taget:
            targetRotation *= Quaternion.Euler(0, 90, 0);  // Rotera 90° höger
                                                           // targetRotation *= Quaternion.Euler(0, -90, 0); // Rotera 90° vänster  
                                                           // targetRotation *= Quaternion.Euler(0, 180, 0); // Rotera 180°

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * 2f * Time.deltaTime);
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

        // Beräkna riktning till spelaren med förutsägelse
        Vector3 targetVelocity = Vector3.zero;
        if (target.TryGetComponent<Rigidbody>(out var targetRb))
        {
            targetVelocity = targetRb.linearVelocity;
        }

        // Sikta framför spelaren baserat på hastighet
        Vector3 predictedPosition = target.position + targetVelocity * 0.3f;
        Vector3 shootDirection = (predictedPosition - gunPoint.position).normalized;

        // Använd bullet pool
        GameObject bullet = BulletPool.Instance.GetBullet(false);
        bullet.transform.position = gunPoint.position;
        bullet.transform.rotation = Quaternion.LookRotation(shootDirection);

        if (bullet.TryGetComponent<BulletSystem>(out var bulletSystem))
        {
            bulletSystem.Initialize(shootDirection, true, bulletDamage, bulletLifetime);
        }

        audioManager?.PlayEnemyShootSound();

        // Byt mellan vapenpunkter
        currentGunIndex = (currentGunIndex + 1) % gunPoints.Length;
    }

    private void CheckIfBehindPlayer()
    {
        if (target != null)
        {
            // Om helikoptern är för långt bakom spelaren
            if (transform.position.z < target.position.z - destroyDistanceBehindPlayer)
            {
                StopHelicopterSound();
                Destroy(gameObject);
            }
        }
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
                    StopHelicopterSound();
                    Destroy(gameObject);
                }
            }
        }
    }

    private void StartHelicopterSound()
    {
        if (audioManager != null && !helicopterSoundPlaying)
        {
            audioManager.PlayVehicleEngineSound(VehicleType.Helicopter, transform.position);
            helicopterSoundPlaying = true;
        }
    }

    private void StopHelicopterSound()
    {
        helicopterSoundPlaying = false;
        // AudioManager hanterar själv när ljuden ska stoppas
    }

    // För att andra script ska kunna justera beteende
    public void SetCombatMode(bool aggressive)
    {
        if (aggressive)
        {
            fireRate = 0.3f;
            moveSpeed = 8f;
            attackRange = 30f;
        }
        else
        {
            fireRate = 0.5f;
            moveSpeed = 6f;
            attackRange = 25f;
        }
    }

    private void OnDestroy()
    {
        StopHelicopterSound();
    }
}
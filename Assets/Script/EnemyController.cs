using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Transform target;
    [SerializeField] private EnemyMovementType movementType;
    [SerializeField] private float sinWaveAmplitude = 2f;
    [SerializeField] private float sinWaveFrequency = 2f;
    [SerializeField] private float circleRadius = 3f;

    [Header("Attack Settings")]
    [SerializeField] private EnemyAttackType attackType;
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private float bulletSpeed = 5f;

    [Header("Advanced Attack Settings")]
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstInterval = 0.2f;
    [SerializeField] private float spreadAngle = 45f;
    [SerializeField] private int spreadCount = 5;
    [SerializeField] private float spiralRotationSpeed = 120f;
    [SerializeField] private int waveBulletCount = 8;
    [SerializeField] private float waveFrequency = 2f;
    [SerializeField] private float waveAmplitude = 1f;

    private float nextAttackTime = 0f;
    private float currentRotation = 0f;
    private float timeSinceLastBurst = 0f;
    private int currentBurstShot = 0;

    // Existing enums
    public enum EnemyMovementType
    {
        Direct,
        SineWave,
        Circle,
        Zigzag
    }

    public enum EnemyAttackType
    {
        None,
        SingleShot,
        Burst,
        Spread,
        Circle,
        Spiral,
        Wave,
        Tracking,
        RandomSpread
    }

    private void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    private void Update()
    {
        if (target == null) return;
        HandleMovement();
        HandleAttack();
    }

    private void HandleMovement()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Vector3 movement = Vector3.zero;

        switch (movementType)
        {
            case EnemyMovementType.Direct:
                movement = direction * moveSpeed;
                break;

            case EnemyMovementType.SineWave:
                float sin = Mathf.Sin(Time.time * sinWaveFrequency) * sinWaveAmplitude;
                Vector3 sideVector = Vector3.Cross(direction, Vector3.up);
                movement = (direction + sideVector * sin).normalized * moveSpeed;
                break;

            case EnemyMovementType.Circle:
                float angle = Time.time * moveSpeed;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * circleRadius;
                Vector3 targetPos = target.position + offset;
                movement = (targetPos - transform.position).normalized * moveSpeed;
                break;

            case EnemyMovementType.Zigzag:
                float zigzag = Mathf.PingPong(Time.time * moveSpeed, 2) - 1;
                Vector3 sideDir = Vector3.Cross(direction, Vector3.up);
                movement = (direction + sideDir * zigzag).normalized * moveSpeed;
                break;
        }

        transform.position += movement * Time.deltaTime;
        transform.LookAt(target);
    }

    private void HandleAttack()
    {
        if (Time.time < nextAttackTime || target == null) return;

        switch (attackType)
        {
            case EnemyAttackType.SingleShot:
                FireSingleShot();
                break;

            case EnemyAttackType.Burst:
                StartCoroutine(FireBurstCoroutine());
                break;

            case EnemyAttackType.Spread:
                FireSpread(spreadAngle, spreadCount, false);
                break;

            case EnemyAttackType.Circle:
                FireCirclePattern();
                break;

            case EnemyAttackType.Spiral:
                FireSpiral();
                break;

            case EnemyAttackType.Wave:
                FireWave();
                break;

            case EnemyAttackType.Tracking:
                FireTracking();
                break;

            case EnemyAttackType.RandomSpread:
                FireSpread(spreadAngle, spreadCount, true);
                break;
        }

        nextAttackTime = Time.time + attackInterval;
    }

    private void FireSingleShot()
    {
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        FireBullet(directionToTarget);
    }

    private void FireSpiral()
    {
        currentRotation += spiralRotationSpeed * Time.deltaTime;
        Vector3 direction = (target.position - transform.position).normalized;
        FireBullet(direction);
    }

    private void FireWave()
    {
        float angleStep = 360f / waveBulletCount;
        for (int i = 0; i < waveBulletCount; i++)
        {
            float angle = i * angleStep;
            float waveOffset = Mathf.Sin(Time.time * waveFrequency + angle * Mathf.Deg2Rad) * waveAmplitude;
            Vector3 baseDirection = (target.position - transform.position).normalized;
            Vector3 direction = Quaternion.Euler(0, angle + waveOffset, 0) * baseDirection;
            FireBullet(direction);
        }
    }

    private void FireTracking()
    {
        Vector3 predictedPosition = target.position;
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            predictedPosition += targetRb.linearVelocity * (Vector3.Distance(transform.position, target.position) / bulletSpeed);
        }
        Vector3 directionToTarget = (predictedPosition - transform.position).normalized;
        FireBullet(directionToTarget);
    }

    private void FireSpread(float totalSpreadAngle, int numberOfBullets, bool randomize)
    {
        float angleStep = totalSpreadAngle / (numberOfBullets - 1);
        float startAngle = -totalSpreadAngle / 2;

        for (int i = 0; i < numberOfBullets; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            if (randomize)
            {
                currentAngle += Random.Range(-angleStep / 2, angleStep / 2);
            }
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
            FireBullet(direction);
        }
    }

    private IEnumerator FireBurstCoroutine()
    {
        for (int i = 0; i < burstCount; i++)
        {
            FireSingleShot();
            yield return new WaitForSeconds(burstInterval);
        }
    }

    private void FireCirclePattern()
    {
        int bulletCount = 8;
        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep;
            Vector3 baseDirection = (target.position - transform.position).normalized;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * baseDirection;
            FireBullet(direction);
        }
    }

    private void FireBullet(Vector3 direction)
    {
        if (enemyBulletPrefab == null) return;

        GameObject bullet = Instantiate(enemyBulletPrefab, transform.position, Quaternion.LookRotation(direction));

        if (bullet.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.useGravity = false;
            rb.linearVelocity = direction * bulletSpeed;
        }

        BulletHandler bulletHandler = bullet.GetComponent<BulletHandler>();
        if (bulletHandler == null)
        {
            bulletHandler = bullet.AddComponent<BulletHandler>();
        }
        bulletHandler.SetAsEnemyProjectile(attackDamage);

        Destroy(bullet, 5f);
    }
}
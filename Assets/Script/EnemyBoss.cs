using UnityEngine;
using System.Collections;

public class EnemyBoss : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float hoverDistance = 20f;
    private Transform target;

    [Header("Attack Settings")]
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private float attackDamage = 40f;
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private float bulletSpeed = 5f;
    [SerializeField] private float bulletLifetime = 3f;

    [Header("Attack Patterns")]
    [SerializeField] private float spreadAngle = 45f;
    [SerializeField] private int spreadCount = 5;
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstInterval = 0.2f;
    [SerializeField] private int circleCount = 8;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 50f; // ÖKAD från 30f
    [SerializeField] private float fieldOfView = 120f;   // ÖKAD från 90f

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true; // AKTIVERAD för debugging

    private enum AttackPattern { Spread, Burst, Circle }
    private AttackPattern currentPattern;
    private float nextAttackTime;
    private AudioManager audioManager;
    private EnemyHealth healthSystem;
    private bool isInitialized;

    private void Start()
    {
        InitializeComponents();
        nextAttackTime = Time.time + attackInterval;

        if (showDebugLogs)
            Debug.Log($"Boss initialized. Target: {target?.name}, Detection Range: {detectionRange}");
    }

    private void InitializeComponents()
    {
        audioManager = AudioManager.Instance;
        healthSystem = GetComponent<EnemyHealth>();
        target = GameObject.FindGameObjectWithTag("Player")?.transform;

        // VIKTIG FIX: Kontrollera att alla komponenter finns
        isInitialized = healthSystem != null && target != null;

        if (!isInitialized)
        {
            Debug.LogError($"Boss initialization failed! HealthSystem: {healthSystem != null}, Target: {target != null}");
        }

        if (enemyBulletPrefab == null)
        {
            Debug.LogError("Boss saknar enemyBulletPrefab!");
        }
    }

    private void Update()
    {
        if (!isInitialized || (healthSystem != null && healthSystem.IsDying))
            return;

        HandleMovement();
        HandleAttacks();
    }

    private void HandleMovement()
    {
        if (target == null) return;

        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget > hoverDistance)
        {
            transform.position += directionToTarget * moveSpeed * Time.deltaTime;
        }
        else if (distanceToTarget < hoverDistance - 1f)
        {
            transform.position -= directionToTarget * moveSpeed * Time.deltaTime;
        }

        transform.LookAt(target);
    }

    private void HandleAttacks()
    {
        // FIX 1: Förenklad kontroll - skjut alltid om spelaren är inom räckvidd
        if (target == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        if (showDebugLogs && Time.frameCount % 60 == 0) // Debug varje sekund
        {
            //Debug.Log($"Boss Attack Check - Distance: {distanceToPlayer:F1}, Detection Range: {detectionRange}, Can Attack: {distanceToPlayer <= detectionRange}");
        }

        // FÖRENKLAD: Skjut bara baserat på avstånd
        if (distanceToPlayer <= detectionRange && Time.time >= nextAttackTime)
        {
            if (showDebugLogs)
                Debug.Log($"Boss attacking with pattern: {currentPattern}");

            currentPattern = (AttackPattern)(((int)currentPattern + 1) % 3);

            switch (currentPattern)
            {
                case AttackPattern.Spread:
                    FireSpreadAttack();
                    break;
                case AttackPattern.Burst:
                    StartCoroutine(FireBurstAttack());
                    break;
                case AttackPattern.Circle:
                    FireCircleAttack();
                    break;
            }

            nextAttackTime = Time.time + attackInterval;
        }
    }

    // FIX 2: Ta bort komplex CanSeePlayer - använd bara avstånd
    // private bool CanSeePlayer() - BORTTAGEN

    private void FireSpreadAttack()
    {
        if (showDebugLogs)
            Debug.Log("Boss firing spread attack");

        float angleStep = spreadAngle / (spreadCount - 1);
        float startAngle = -spreadAngle / 2;

        for (int i = 0; i < spreadCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
            FireBullet(direction);
        }

        audioManager?.PlayEnemyShootSound();
    }

    private IEnumerator FireBurstAttack()
    {
        if (showDebugLogs)
            Debug.Log("Boss firing burst attack");

        for (int i = 0; i < burstCount; i++)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            FireBullet(direction);
            audioManager?.PlayEnemyShootSound();
            yield return new WaitForSeconds(burstInterval);
        }
    }

    private void FireCircleAttack()
    {
        if (showDebugLogs)
            Debug.Log("Boss firing circle attack");

        float angleStep = 360f / circleCount;

        for (int i = 0; i < circleCount; i++)
        {
            float angle = i * angleStep;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
            FireBullet(direction);
        }

        audioManager?.PlayEnemyShootSound();
    }

    private void FireBullet(Vector3 direction)
    {
        if (enemyBulletPrefab == null)
        {
            Debug.LogError("Boss trying to fire but enemyBulletPrefab is null!");
            return;
        }

        // FIX 3: Använd BulletPool istället för Instantiate
        GameObject bullet = BulletPool.Instance.GetBullet(false); // false = enemy bullet
        bullet.transform.position = transform.position;
        bullet.transform.rotation = Quaternion.LookRotation(direction);

        if (bullet.TryGetComponent<BulletSystem>(out var bulletSystem))
        {
            bulletSystem.Initialize(direction, true, attackDamage, bulletLifetime);
        }

        if (bullet.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.useGravity = false;
            rb.linearVelocity = direction * bulletSpeed;
        }

        if (showDebugLogs)
            Debug.Log($"Boss fired bullet in direction: {direction}");
    }

    // BEHÅLL för debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Vector3 forward = transform.forward;
        if (forward != Vector3.zero)
        {
            Vector3 rightDir = Quaternion.Euler(0, fieldOfView / 2f, 0) * forward;
            Vector3 leftDir = Quaternion.Euler(0, -fieldOfView / 2f, 0) * forward;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, rightDir * detectionRange);
            Gizmos.DrawRay(transform.position, leftDir * detectionRange);
        }
    }

    public void SetAttackDamage(float damage) => attackDamage = damage;
    public void SetAttackInterval(float interval) => attackInterval = interval;
    public void SetMoveSpeed(float speed) => moveSpeed = speed;
}
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

    [Header("Attack Patterns")]
    [SerializeField] private float spreadAngle = 45f;
    [SerializeField] private int spreadCount = 5;
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstInterval = 0.2f;
    [SerializeField] private int circleCount = 8;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 30f;
    [SerializeField] private float fieldOfView = 90f;

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
    }

    private void InitializeComponents()
    {
        audioManager = AudioManager.Instance;
        healthSystem = GetComponent<EnemyHealth>();
        target = GameObject.FindGameObjectWithTag("Player")?.transform;
        isInitialized = healthSystem != null && target != null && enemyBulletPrefab != null;
    }

    private void Update()
    {
        if (!isInitialized || healthSystem.IsDying) return;

        HandleMovement();
        HandleAttacks();
    }

    private void HandleMovement()
    {
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
        if (!CanSeePlayer()) return;

        if (Time.time >= nextAttackTime)
        {
            currentPattern = (AttackPattern)(((int)currentPattern + 1) % 3);
            //Debug.Log($"Attackerar med pattern: {currentPattern}");

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

    private bool CanSeePlayer()
    {
        if (target == null) return false;

        Vector3 directionToPlayer = target.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > detectionRange) return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > fieldOfView / 2f) return false;

        if (Physics.Raycast(transform.position, directionToPlayer.normalized, out RaycastHit hit, detectionRange))
        {
            return hit.collider.CompareTag("Player");
        }

        return false;
    }

    private void FireSpreadAttack()
    {
        //Debug.Log("Startar spread attack");
        float angleStep = spreadAngle / (spreadCount - 1);
        float startAngle = -spreadAngle / 2;

        for (int i = 0; i < spreadCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
            FireBullet(direction);
        }

        // Uppdaterad ljudhantering
        audioManager?.PlayEnemyShootSound();
    }

    private IEnumerator FireBurstAttack()
    {
        //Debug.Log("Startar burst attack");
        for (int i = 0; i < burstCount; i++)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            FireBullet(direction);
            // Uppdaterad ljudhantering
            audioManager?.PlayEnemyShootSound();
            yield return new WaitForSeconds(burstInterval);
        }
    }

    private void FireCircleAttack()
    {
        //Debug.Log("Startar circle attack");
        float angleStep = 360f / circleCount;

        for (int i = 0; i < circleCount; i++)
        {
            float angle = i * angleStep;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
            FireBullet(direction);
        }

        // Uppdaterad ljudhantering
        audioManager?.PlayEnemyShootSound();
    }

    private void FireBullet(Vector3 direction)
    {
        if (enemyBulletPrefab == null) return;

        GameObject bullet = Instantiate(enemyBulletPrefab, transform.position, Quaternion.LookRotation(direction));

        if (bullet.TryGetComponent<BulletSystem>(out var bulletSystem))
        {
            bulletSystem.Initialize(direction, true, attackDamage);
        }

        if (bullet.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.useGravity = false;
            rb.linearVelocity = direction * bulletSpeed;
        }
    }

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

//using UnityEngine;
//using System.Collections;

//public class EnemyBoss : MonoBehaviour
//{
//    [Header("Movement Settings")]
//    [SerializeField] private float moveSpeed = 3f;
//    [SerializeField] private float hoverDistance = 20f;
//    private Transform target;

//    [Header("Attack Settings")]
//    [SerializeField] private GameObject enemyBulletPrefab;
//    [SerializeField] private float attackDamage = 40f;
//    [SerializeField] private float attackInterval = 2f;
//    [SerializeField] private float bulletSpeed = 5f;

//    [Header("Attack Patterns")]
//    [SerializeField] private float spreadAngle = 45f;
//    [SerializeField] private int spreadCount = 5;
//    [SerializeField] private int burstCount = 3;
//    [SerializeField] private float burstInterval = 0.2f;
//    [SerializeField] private int circleCount = 8;

//    [Header("Detection Settings")]
//    [SerializeField] private float detectionRange = 30f;
//    [SerializeField] private float fieldOfView = 90f;

//    private enum AttackPattern { Spread, Burst, Circle }
//    private AttackPattern currentPattern;
//    private float nextAttackTime;
//    private AudioManager audioManager;
//    private EnemyHealth healthSystem;
//    private bool isInitialized;

//    private void Start()
//    {
//        InitializeComponents();
//        nextAttackTime = Time.time + attackInterval;
//    }

//    private void InitializeComponents()
//    {
//        audioManager = AudioManager.Instance;
//        healthSystem = GetComponent<EnemyHealth>();
//        target = GameObject.FindGameObjectWithTag("Player")?.transform;
//        isInitialized = healthSystem != null && target != null && enemyBulletPrefab != null;
//    }

//    private void Update()
//    {
//        if (!isInitialized || healthSystem.IsDying) return;

//        HandleMovement();
//        HandleAttacks();
//    }

//    private void HandleMovement()
//    {
//        Vector3 directionToTarget = (target.position - transform.position).normalized;
//        float distanceToTarget = Vector3.Distance(transform.position, target.position);

//        if (distanceToTarget > hoverDistance)
//        {
//            transform.position += directionToTarget * moveSpeed * Time.deltaTime;
//        }
//        else if (distanceToTarget < hoverDistance - 1f)
//        {
//            transform.position -= directionToTarget * moveSpeed * Time.deltaTime;
//        }

//        transform.LookAt(target);
//    }

//    private void HandleAttacks()
//    {
//        // Kolla först om vi kan se spelaren
//        if (!CanSeePlayer())
//        {
//            //Debug.Log("Kan inte se spelaren - skippar attack");
//            return;
//        }

//        if (Time.time >= nextAttackTime)
//        {
//            currentPattern = (AttackPattern)(((int)currentPattern + 1) % 3);
//            Debug.Log($"Attackerar med pattern: {currentPattern}");

//            switch (currentPattern)
//            {
//                case AttackPattern.Spread:
//                    FireSpreadAttack();
//                    break;
//                case AttackPattern.Burst:
//                    StartCoroutine(FireBurstAttack());
//                    break;
//                case AttackPattern.Circle:
//                    FireCircleAttack();
//                    break;
//            }

//            nextAttackTime = Time.time + attackInterval;
//        }
//    }

//    private bool CanSeePlayer()
//    {
//        if (target == null)
//        {
//            Debug.Log("Inget player target hittat");
//            return false;
//        }

//        Vector3 directionToPlayer = target.position - transform.position;
//        float distanceToPlayer = directionToPlayer.magnitude;

//        if (distanceToPlayer > detectionRange)
//        {
//            Debug.Log($"Spelaren för långt borta: {distanceToPlayer} > {detectionRange}");
//            return false;
//        }

//        float angle = Vector3.Angle(transform.forward, directionToPlayer);
//        if (angle > fieldOfView / 2f)
//        {
//            Debug.Log($"Spelaren utanför synfält: {angle} > {fieldOfView / 2f}");
//            return false;
//        }

//        if (Physics.Raycast(transform.position, directionToPlayer.normalized, out RaycastHit hit, detectionRange))
//        {
//            bool canSee = hit.collider.CompareTag("Player");
//            if (!canSee)
//            {
//                //Debug.Log($"Raycast träffade {hit.collider.name} istället för spelaren");
//            }
//            return canSee;
//        }

//        Debug.Log("Raycast träffade inget");
//        return false;
//    }

//    private void FireSpreadAttack()
//    {
//        Debug.Log("Startar spread attack");
//        float angleStep = spreadAngle / (spreadCount - 1);
//        float startAngle = -spreadAngle / 2;

//        for (int i = 0; i < spreadCount; i++)
//        {
//            float currentAngle = startAngle + (angleStep * i);
//            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
//            FireBullet(direction);
//        }

//        audioManager?.PlayShootSound();
//    }

//    private IEnumerator FireBurstAttack()
//    {
//        Debug.Log("Startar burst attack");
//        for (int i = 0; i < burstCount; i++)
//        {
//            Vector3 direction = (target.position - transform.position).normalized;
//            FireBullet(direction);
//            audioManager?.PlayShootSound();
//            yield return new WaitForSeconds(burstInterval);
//        }
//    }

//    private void FireCircleAttack()
//    {
//        Debug.Log("Startar circle attack");
//        float angleStep = 360f / circleCount;

//        for (int i = 0; i < circleCount; i++)
//        {
//            float angle = i * angleStep;
//            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
//            FireBullet(direction);
//        }

//        audioManager?.PlayShootSound();
//    }

//    private void FireBullet(Vector3 direction)
//    {
//        if (enemyBulletPrefab == null) return;

//        GameObject bullet = Instantiate(enemyBulletPrefab, transform.position, Quaternion.LookRotation(direction));

//        if (bullet.TryGetComponent<BulletSystem>(out var bulletSystem))
//        {
//            bulletSystem.Initialize(direction, true, attackDamage);
//        }

//        if (bullet.TryGetComponent<Rigidbody>(out var rb))
//        {
//            rb.useGravity = false;
//            rb.linearVelocity = direction * bulletSpeed;
//        }
//    }

//    private void OnDrawGizmosSelected()
//    {
//        Gizmos.color = Color.yellow;
//        Gizmos.DrawWireSphere(transform.position, detectionRange);

//        Vector3 forward = transform.forward;
//        if (forward != Vector3.zero)  // Undvik fel i editor
//        {
//            Vector3 rightDir = Quaternion.Euler(0, fieldOfView / 2f, 0) * forward;
//            Vector3 leftDir = Quaternion.Euler(0, -fieldOfView / 2f, 0) * forward;
//            Gizmos.color = Color.red;
//            Gizmos.DrawRay(transform.position, rightDir * detectionRange);
//            Gizmos.DrawRay(transform.position, leftDir * detectionRange);
//        }
//    }

//    void SetAttackDamage(float damage) => attackDamage = damage;
//    void SetAttackInterval(float interval) => attackInterval = interval;
//    void SetMoveSpeed(float speed) => moveSpeed = speed;
//}
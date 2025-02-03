using UnityEngine;
using System.Collections;

public class EnemyBoss : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float hoverDistance = 20f;
    private Transform target;

    [Header("Attack Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float attackDamage = 40f;
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private float bulletSpeed = 5f;

    [Header("Attack Patterns")]
    [SerializeField] private float spreadAngle = 45f;
    [SerializeField] private int spreadCount = 5;
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstInterval = 0.2f;
    [SerializeField] private int circleCount = 8;

    // Attack pattern states
    private enum AttackPattern { Spread, Burst, Circle }
    private AttackPattern currentPattern;
    private float nextAttackTime;

    // Components
    private AudioManager audioManager;
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

        if (healthSystem == null)
        {
            Debug.LogWarning("EnemyHealth saknas på " + gameObject.name);
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
        }
    }

    private void Update()
    {
        if (target == null || healthSystem.IsDying) return;

        HandleMovement();
        HandleAttackPattern();
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

    private void HandleAttackPattern()
    {
        if (Time.time < nextAttackTime) return;

        // Växla mellan olika attackmönster
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

    private void FireSpreadAttack()
    {
        float angleStep = spreadAngle / (spreadCount - 1);
        float startAngle = -spreadAngle / 2;

        for (int i = 0; i < spreadCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
            FireBullet(direction);
        }

        audioManager?.PlayShootSound();
    }

    private IEnumerator FireBurstAttack()
    {
        for (int i = 0; i < burstCount; i++)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            FireBullet(direction);
            audioManager?.PlayShootSound();
            yield return new WaitForSeconds(burstInterval);
        }
    }

    private void FireCircleAttack()
    {
        float angleStep = 360f / circleCount;

        for (int i = 0; i < circleCount; i++)
        {
            float angle = i * angleStep;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
            FireBullet(direction);
        }

        audioManager?.PlayShootSound();
    }

    private void FireBullet(Vector3 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.LookRotation(direction));

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

    // Public methods för extern kontroll
    public void SetAttackDamage(float damage) => attackDamage = damage;
    public void SetAttackInterval(float interval) => attackInterval = interval;
    public void SetMoveSpeed(float speed) => moveSpeed = speed;
}
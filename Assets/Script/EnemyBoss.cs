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

    private enum AttackPattern { Spread, Burst, Circle }
    private AttackPattern currentPattern;
    private float nextAttackTime;
    private AudioManager audioManager;
    private EnemyHealth healthSystem;
    private bool isInitialized;

    private void Start()
    {
        //Debug.Log("EnemyBoss Start");
        InitializeComponents();
        nextAttackTime = Time.time + attackInterval;
    }

    private void InitializeComponents()
    {
        audioManager = AudioManager.Instance;
        healthSystem = GetComponent<EnemyHealth>();
        target = GameObject.FindGameObjectWithTag("Player")?.transform;

        //Debug.Log($"Boss komponenter: Health={healthSystem != null}, Target={target != null}, Bullet={enemyBulletPrefab != null}");
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
        if (Time.time >= nextAttackTime)
        {
            currentPattern = (AttackPattern)(((int)currentPattern + 1) % 3);
            //Debug.Log($"Boss byter pattern till: {currentPattern}");

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

    private void FireSpreadAttack()
    {
        Debug.Log("Startar spread attack");
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
        Debug.Log("Startar burst attack");
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
        //Debug.Log("Startar circle attack");
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
        if (enemyBulletPrefab == null)
        {
            //Debug.LogError("EnemyBulletPrefab är null!");
            return;
        }

        GameObject bullet = Instantiate(enemyBulletPrefab, transform.position, Quaternion.LookRotation(direction));
        //Debug.Log($"Skapar bullet: Position={transform.position}, Direction={direction}");

        if (bullet.TryGetComponent<BulletSystem>(out var bulletSystem))
        {
            //Debug.Log("BulletSystem hittad på bullet");
            bulletSystem.Initialize(direction, true, attackDamage);
            //Debug.Log($"Bullet initialized: Damage={attackDamage}");
        }
        else
        {
            //Debug.LogError("BulletSystem saknas på bullet!");
        }

        if (bullet.TryGetComponent<Rigidbody>(out var rb))
        {
            //Debug.Log("Rigidbody hittad på bullet");
            rb.useGravity = false;
            rb.linearVelocity = direction * bulletSpeed;
            //Debug.Log($"Bullet velocity satt: {direction * bulletSpeed}");
        }
        else
        {
            //Debug.LogError("Rigidbody saknas på bullet!");
        }
    }

    void SetAttackDamage(float damage) => attackDamage = damage;
    void SetAttackInterval(float interval) => attackInterval = interval;
    void SetMoveSpeed(float speed) => moveSpeed = speed;
}
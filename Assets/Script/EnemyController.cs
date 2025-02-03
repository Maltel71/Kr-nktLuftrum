using UnityEngine;
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

    [Header("Collision Settings")]
    [SerializeField] private float collisionDamage = 25f;
    [SerializeField] private bool destroyOnCollision = true;

    private float nextAttackTime = 0f;
    private Vector3 initialForward;

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
        Circle
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

        // Spara initial riktning
        initialForward = transform.forward;
    }

    private void Update()
    {
        if (target == null) return;
        HandleMovement();
        HandleAttack();
    }

    private void HandleMovement()
    {
        Vector3 movement = Vector3.zero;

        switch (movementType)
        {
            case EnemyMovementType.Direct:
                movement = initialForward * moveSpeed;
                break;

            case EnemyMovementType.SineWave:
                float sin = Mathf.Sin(Time.time * sinWaveFrequency) * sinWaveAmplitude;
                Vector3 sideVector = Vector3.Cross(initialForward, Vector3.up);
                movement = (initialForward + sideVector * sin).normalized * moveSpeed;
                break;

            case EnemyMovementType.Circle:
                float angle = Time.time * moveSpeed;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * circleRadius;
                movement = (offset + initialForward).normalized * moveSpeed;
                break;

            case EnemyMovementType.Zigzag:
                float zigzag = Mathf.PingPong(Time.time * moveSpeed, 2) - 1;
                Vector3 sideDir = Vector3.Cross(initialForward, Vector3.up);
                movement = (initialForward + sideDir * zigzag).normalized * moveSpeed;
                break;
        }

        transform.position += movement * Time.deltaTime;
    }

    private void HandleAttack()
    {
        if (Time.time < nextAttackTime || target == null) return;

        switch (attackType)
        {
            case EnemyAttackType.SingleShot:
                FireSingleShot();
                break;
            case EnemyAttackType.Spread:
                FireSpread();
                break;
            case EnemyAttackType.Circle:
                FireCirclePattern();
                break;
        }

        nextAttackTime = Time.time + attackInterval;
    }

    private void FireSingleShot()
    {
        FireBullet(initialForward);
    }

    private void FireSpread()
    {
        float spreadAngle = 30f;
        int bulletCount = 3;
        float angleStep = spreadAngle / (bulletCount - 1);
        float startAngle = -spreadAngle / 2;

        for (int i = 0; i < bulletCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * initialForward;
            FireBullet(direction);
        }
    }

    private void FireCirclePattern()
    {
        int bulletCount = 8;
        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * initialForward;
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var playerHealth = collision.gameObject.GetComponent<PlaneHealthSystem>();
            if (playerHealth != null)
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
}
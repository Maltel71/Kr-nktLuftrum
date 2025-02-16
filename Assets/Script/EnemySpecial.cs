using UnityEngine;

public class EnemySpecial : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private MovementType movementType;
    [SerializeField] private float circleRadius = 5f;
    [SerializeField] private float spiralSpeed = 2f;

    [Header("Attack Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float attackDamage = 30f;
    [SerializeField] private float attackInterval = 1.5f;
    [SerializeField] private float bulletSpeed = 8f;

    [Header("Special Attack")]
    [SerializeField] private int bulletCount = 4;
    [SerializeField] private float spiralRotationSpeed = 120f;

    private enum MovementType
    {
        Circle,    // Cirkulär rörelse runt spelaren
        Spiral,    // Spiralrörelse mot spelaren
        Orbit      // Kretsar runt spelaren
    }

    private Transform target;
    private float nextAttackTime = 0f;
    private float currentAngle = 0f;
    private Vector3 orbitCenter;
    private AudioManager audioManager;

    private void Start()
    {
        audioManager = AudioManager.Instance;
        target = GameObject.FindGameObjectWithTag("Player")?.transform;
        orbitCenter = transform.position;
    }

    private void Update()
    {
        if (target == null) return;
        HandleMovement();
        HandleAttack();
    }

    private void HandleMovement()
    {
        switch (movementType)
        {
            case MovementType.Circle:
                CircleMovement();
                break;
            case MovementType.Spiral:
                SpiralMovement();
                break;
            case MovementType.Orbit:
                OrbitMovement();
                break;
        }
    }

    private void CircleMovement()
    {
        currentAngle += moveSpeed * Time.deltaTime;
        float x = Mathf.Cos(currentAngle) * circleRadius;
        float z = Mathf.Sin(currentAngle) * circleRadius;
        Vector3 offset = new Vector3(x, 0, z);
        transform.position = target.position + offset;
        transform.LookAt(target);
    }

    private void SpiralMovement()
    {
        currentAngle += spiralSpeed * Time.deltaTime;
        float currentRadius = circleRadius * (1 - currentAngle / (2 * Mathf.PI));
        if (currentRadius < 1f) currentAngle = 0f;

        float x = Mathf.Cos(currentAngle) * currentRadius;
        float z = Mathf.Sin(currentAngle) * currentRadius;
        Vector3 offset = new Vector3(x, 0, z);
        transform.position = target.position + offset;
        transform.LookAt(target);
    }

    private void OrbitMovement()
    {
        Vector3 directionToTarget = target.position - orbitCenter;
        if (directionToTarget.magnitude > circleRadius * 2)
        {
            orbitCenter = target.position;
        }

        currentAngle += moveSpeed * Time.deltaTime;
        Vector3 orbitPosition = orbitCenter + new Vector3(
            Mathf.Cos(currentAngle) * circleRadius,
            0,
            Mathf.Sin(currentAngle) * circleRadius
        );
        transform.position = Vector3.Lerp(transform.position, orbitPosition, Time.deltaTime * moveSpeed);
        transform.LookAt(target);
    }

    private void HandleAttack()
    {
        if (Time.time >= nextAttackTime)
        {
            FireSpecialAttack();
            nextAttackTime = Time.time + attackInterval;
        }
    }

    private void FireSpecialAttack()
    {
        float angleStep = 360f / bulletCount;
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep + Time.time * spiralRotationSpeed;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
            FireBullet(direction);
        }
        // Uppdaterad ljudhantering
        audioManager?.PlayEnemyShootSound();
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
}

//using UnityEngine;

//public class EnemySpecial : MonoBehaviour
//{
//    [Header("Movement Settings")]
//    [SerializeField] private float moveSpeed = 4f;
//    [SerializeField] private MovementType movementType;
//    [SerializeField] private float circleRadius = 5f;
//    [SerializeField] private float spiralSpeed = 2f;

//    [Header("Attack Settings")]
//    [SerializeField] private GameObject bulletPrefab;
//    [SerializeField] private float attackDamage = 30f;
//    [SerializeField] private float attackInterval = 1.5f;
//    [SerializeField] private float bulletSpeed = 8f;

//    [Header("Special Attack")]
//    [SerializeField] private int bulletCount = 4;
//    [SerializeField] private float spiralRotationSpeed = 120f;

//    private enum MovementType
//    {
//        Circle,    // Cirkul�r r�relse runt spelaren
//        Spiral,    // Spiralr�relse mot spelaren
//        Orbit      // Kretsar runt spelaren
//    }

//    private Transform target;
//    private float nextAttackTime = 0f;
//    private float currentAngle = 0f;
//    private Vector3 orbitCenter;
//    private AudioManager audioManager;

//    private void Start()
//    {
//        audioManager = AudioManager.Instance;
//        target = GameObject.FindGameObjectWithTag("Player")?.transform;
//        orbitCenter = transform.position;
//    }

//    private void Update()
//    {
//        if (target == null) return;
//        HandleMovement();
//        HandleAttack();
//    }

//    private void HandleMovement()
//    {
//        switch (movementType)
//        {
//            case MovementType.Circle:
//                CircleMovement();
//                break;
//            case MovementType.Spiral:
//                SpiralMovement();
//                break;
//            case MovementType.Orbit:
//                OrbitMovement();
//                break;
//        }
//    }

//    private void CircleMovement()
//    {
//        currentAngle += moveSpeed * Time.deltaTime;
//        float x = Mathf.Cos(currentAngle) * circleRadius;
//        float z = Mathf.Sin(currentAngle) * circleRadius;
//        Vector3 offset = new Vector3(x, 0, z);
//        transform.position = target.position + offset;
//        transform.LookAt(target);
//    }

//    private void SpiralMovement()
//    {
//        currentAngle += spiralSpeed * Time.deltaTime;
//        float currentRadius = circleRadius * (1 - currentAngle / (2 * Mathf.PI));
//        if (currentRadius < 1f) currentAngle = 0f;

//        float x = Mathf.Cos(currentAngle) * currentRadius;
//        float z = Mathf.Sin(currentAngle) * currentRadius;
//        Vector3 offset = new Vector3(x, 0, z);
//        transform.position = target.position + offset;
//        transform.LookAt(target);
//    }

//    private void OrbitMovement()
//    {
//        Vector3 directionToTarget = target.position - orbitCenter;
//        if (directionToTarget.magnitude > circleRadius * 2)
//        {
//            orbitCenter = target.position;
//        }

//        currentAngle += moveSpeed * Time.deltaTime;
//        Vector3 orbitPosition = orbitCenter + new Vector3(
//            Mathf.Cos(currentAngle) * circleRadius,
//            0,
//            Mathf.Sin(currentAngle) * circleRadius
//        );
//        transform.position = Vector3.Lerp(transform.position, orbitPosition, Time.deltaTime * moveSpeed);
//        transform.LookAt(target);
//    }

//    private void HandleAttack()
//    {
//        if (Time.time >= nextAttackTime)
//        {
//            FireSpecialAttack();
//            nextAttackTime = Time.time + attackInterval;
//        }
//    }

//    private void FireSpecialAttack()
//    {
//        float angleStep = 360f / bulletCount;
//        for (int i = 0; i < bulletCount; i++)
//        {
//            float angle = i * angleStep + Time.time * spiralRotationSpeed;
//            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
//            FireBullet(direction);
//        }
//        audioManager?.PlayShootSound();
//    }

//    private void FireBullet(Vector3 direction)
//    {
//        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.LookRotation(direction));
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
//}
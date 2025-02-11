using UnityEngine;

public class EnemyGround : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private float detectionRange = 30f;
    [SerializeField] private float minAttackRange = 5f;
    [SerializeField] private float maxRotationAngle = 60f;

    [Header("Weapon Settings")]
    [SerializeField] private Transform turretPivot;
    [SerializeField] private Transform[] firePoints;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletDamage = 15f;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float bulletSpeed = 15f;

    [Header("Turret Settings")]
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private bool useMultipleGuns = false;
    [SerializeField] private float burstInterval = 0.1f;

    private Transform target;
    private float nextFireTime;
    private int currentFirePoint = 0;
    private AudioManager audioManager;
    private bool isTargetInRange = false;

    private void Start()
    {
        audioManager = AudioManager.Instance;
        target = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (firePoints.Length == 0)
        {
            Debug.LogWarning("Inga firepoints tilldelade till markfienden!");
        }
    }

    private void Update()
    {
        if (target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        isTargetInRange = distanceToTarget <= detectionRange && distanceToTarget >= minAttackRange;

        if (isTargetInRange)
        {
            AimAtTarget();
            if (Time.time >= nextFireTime)
            {
                Fire();
            }
        }
    }

    private void AimAtTarget()
    {
        Vector3 targetDirection = target.position - transform.position;
        targetDirection.y = 0;

        float targetAngle = Vector3.SignedAngle(transform.forward, targetDirection, Vector3.up);
        targetAngle = Mathf.Clamp(targetAngle, -maxRotationAngle, maxRotationAngle);

        Quaternion targetRotation = transform.rotation * Quaternion.Euler(0, targetAngle, 0);
        turretPivot.rotation = Quaternion.RotateTowards(
            turretPivot.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void Fire()
    {
        if (!useMultipleGuns)
        {
            FireSingle();
        }
        else
        {
            StartCoroutine(FireBurst());
        }
        nextFireTime = Time.time + fireRate;
    }

    private void FireSingle()
    {
        if (firePoints.Length == 0) return;

        Transform firePoint = firePoints[currentFirePoint];
        FireBullet(firePoint);

        currentFirePoint = (currentFirePoint + 1) % firePoints.Length;
        // Uppdaterad ljudhantering
        audioManager?.PlayEnemyShootSound();
    }

    private System.Collections.IEnumerator FireBurst()
    {
        foreach (Transform firePoint in firePoints)
        {
            FireBullet(firePoint);
            // Uppdaterad ljudhantering
            audioManager?.PlayEnemyShootSound();
            yield return new WaitForSeconds(burstInterval);
        }
    }

    private void FireBullet(Transform firePoint)
    {
        Vector3 shootDirection = firePoint.forward;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        if (bullet.TryGetComponent<BulletSystem>(out var bulletSystem))
        {
            bulletSystem.Initialize(shootDirection, true, bulletDamage);
        }

        if (bullet.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.useGravity = false;
            rb.linearVelocity = shootDirection * bulletSpeed;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.green;
    }
}

//using UnityEngine;

//public class EnemyGround : MonoBehaviour
//{
//    [Header("Target Settings")]
//    [SerializeField] private float detectionRange = 30f;
//    [SerializeField] private float minAttackRange = 5f;
//    [SerializeField] private float maxRotationAngle = 60f;

//    [Header("Weapon Settings")]
//    [SerializeField] private Transform turretPivot;
//    [SerializeField] private Transform[] firePoints;
//    [SerializeField] private GameObject bulletPrefab;
//    [SerializeField] private float bulletDamage = 15f;
//    [SerializeField] private float fireRate = 1.5f;
//    [SerializeField] private float bulletSpeed = 15f;

//    [Header("Turret Settings")]
//    [SerializeField] private float rotationSpeed = 3f;
//    [SerializeField] private bool useMultipleGuns = false;
//    [SerializeField] private float burstInterval = 0.1f;

//    private Transform target;
//    private float nextFireTime;
//    private int currentFirePoint = 0;
//    private AudioManager audioManager;
//    private bool isTargetInRange = false;

//    private void Start()
//    {
//        audioManager = AudioManager.Instance;
//        target = GameObject.FindGameObjectWithTag("Player")?.transform;

//        if (firePoints.Length == 0)
//        {
//            Debug.LogWarning("Inga firepoints tilldelade till markfienden!");
//        }
//    }

//    private void Update()
//    {
//        if (target == null) return;

//        float distanceToTarget = Vector3.Distance(transform.position, target.position);
//        isTargetInRange = distanceToTarget <= detectionRange && distanceToTarget >= minAttackRange;

//        if (isTargetInRange)
//        {
//            AimAtTarget();
//            if (Time.time >= nextFireTime)
//            {
//                Fire();
//            }
//        }
//    }

//    private void AimAtTarget()
//    {
//        Vector3 targetDirection = target.position - transform.position;
//        targetDirection.y = 0; // H�ll rotation p� markplanet

//        // Ber�kna vinkel till m�let
//        float targetAngle = Vector3.SignedAngle(transform.forward, targetDirection, Vector3.up);
//        targetAngle = Mathf.Clamp(targetAngle, -maxRotationAngle, maxRotationAngle);

//        // Rotera turret
//        Quaternion targetRotation = transform.rotation * Quaternion.Euler(0, targetAngle, 0);
//        turretPivot.rotation = Quaternion.RotateTowards(
//            turretPivot.rotation,
//            targetRotation,
//            rotationSpeed * Time.deltaTime
//        );
//    }

//    private void Fire()
//    {
//        if (!useMultipleGuns)
//        {
//            FireSingle();
//        }
//        else
//        {
//            StartCoroutine(FireBurst());
//        }
//        nextFireTime = Time.time + fireRate;
//    }

//    private void FireSingle()
//    {
//        if (firePoints.Length == 0) return;

//        Transform firePoint = firePoints[currentFirePoint];
//        FireBullet(firePoint);

//        currentFirePoint = (currentFirePoint + 1) % firePoints.Length;
//        audioManager?.PlayShootSound();
//    }

//    private System.Collections.IEnumerator FireBurst()
//    {
//        foreach (Transform firePoint in firePoints)
//        {
//            FireBullet(firePoint);
//            audioManager?.PlayShootSound();
//            yield return new WaitForSeconds(burstInterval);
//        }
//    }

//    private void FireBullet(Transform firePoint)
//    {
//        Vector3 shootDirection = firePoint.forward;
//        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

//        if (bullet.TryGetComponent<BulletSystem>(out var bulletSystem))
//        {
//            bulletSystem.Initialize(shootDirection, true, bulletDamage);
//        }

//        if (bullet.TryGetComponent<Rigidbody>(out var rb))
//        {
//            rb.useGravity = false;
//            rb.linearVelocity = shootDirection * bulletSpeed;
//        }
//    }

//    private void OnDrawGizmosSelected()
//    {
//        // Rita detektionsomr�de i editorn
//        Gizmos.color = Color.yellow;
//        Gizmos.DrawWireSphere(transform.position, detectionRange);
//        Gizmos.color = Color.green;
//    }
//}
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Transform target;
    [SerializeField] private EnemyMovementType movementType;
    [SerializeField] private float sinWaveAmplitude = 2f;    // För sinusvög-rörelse
    [SerializeField] private float sinWaveFrequency = 2f;    // För sinusvög-rörelse
    [SerializeField] private float circleRadius = 3f;        // För cirkelö�relse

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private GameObject healthBarPrefab;     // UI prefab för health bar
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 2f, 0); // Positionsoffset för health bar

    [Header("Attack Settings")]
    [SerializeField] private EnemyAttackType attackType;
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private float bulletSpeed = 5f;
    [SerializeField] private int burstCount = 3;             // Antal skott i en burst
    [SerializeField] private float burstInterval = 0.2f;     // Tid mellan skott i en burst

    private float nextAttackTime = 0f;
    private float timeSinceSpawn = 0f;
    private Vector3 startPosition;
    private bool isDead = false;
    private Slider healthBar;
    private Canvas healthBarCanvas;
    private Camera mainCamera;

    // Enum f�r olika r�relsem�nster
    public enum EnemyMovementType
    {
        Direct,         // R�r sig rakt mot spelaren
        SineWave,      // R�r sig i en sinusväg
        Circle,        // Cirklar runt spelaren
        Zigzag         // Zigzag-m�nster
    }

    // Enum f�r olika attackm�nster
    public enum EnemyAttackType
    {
        None,          // Ingen attack
        SingleShot,    // Ett skott i taget
        Burst,         // Flera skott i snabb följd
        Spread,        // Sprider skott i en kon
        Circle         // Skjuter i en cirkel
    }

    private void Start()
    {
        currentHealth = maxHealth;
        startPosition = transform.position;
        mainCamera = Camera.main;

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        SetupHealthBar();
    }

    private void SetupHealthBar()
    {
        if (healthBarPrefab != null)
        {
            // Skapa health bar
            GameObject healthBarObj = Instantiate(healthBarPrefab, transform.position + healthBarOffset, Quaternion.identity);
            healthBar = healthBarObj.GetComponent<Slider>();
            healthBarCanvas = healthBarObj.GetComponent<Canvas>();

            // G�r health bar till barn till fienden
            healthBarObj.transform.SetParent(transform);

            // Konfigurera health bar
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    private void Update()
    {
        if (isDead) return;

        timeSinceSpawn += Time.deltaTime;
        HandleMovement();
        HandleAttack();
        UpdateHealthBar();
    }

    private void HandleMovement()
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        Vector3 movement = Vector3.zero;

        switch (movementType)
        {
            case EnemyMovementType.Direct:
                movement = direction * moveSpeed;
                break;

            case EnemyMovementType.SineWave:
                float sin = Mathf.Sin(timeSinceSpawn * sinWaveFrequency) * sinWaveAmplitude;
                Vector3 sideVector = Vector3.Cross(direction, Vector3.up);
                movement = (direction + sideVector * sin).normalized * moveSpeed;
                break;

            case EnemyMovementType.Circle:
                float angle = timeSinceSpawn * moveSpeed;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * circleRadius;
                Vector3 targetPos = target.position + offset;
                movement = (targetPos - transform.position).normalized * moveSpeed;
                break;

            case EnemyMovementType.Zigzag:
                float zigzag = Mathf.PingPong(timeSinceSpawn * moveSpeed, 2) - 1;
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
                FireBullet(transform.forward);
                nextAttackTime = Time.time + attackInterval;
                break;

            case EnemyAttackType.Burst:
                StartCoroutine(FireBurst());
                nextAttackTime = Time.time + attackInterval;
                break;

            case EnemyAttackType.Spread:
                FireSpread();
                nextAttackTime = Time.time + attackInterval;
                break;

            case EnemyAttackType.Circle:
                FireCircle();
                nextAttackTime = Time.time + attackInterval;
                break;
        }
    }

    private void FireBullet(Vector3 direction)
    {
        if (enemyBulletPrefab != null)
        {
            GameObject bullet = Instantiate(enemyBulletPrefab, transform.position, Quaternion.LookRotation(direction));
            bullet.GetComponent<Rigidbody>().linearVelocity = direction * bulletSpeed;

            ProjectileDamage damage = bullet.GetComponent<ProjectileDamage>();
            if (damage == null)
            {
                damage = bullet.AddComponent<ProjectileDamage>();
            }
            damage.damage = attackDamage;
        }
    }

    private System.Collections.IEnumerator FireBurst()
    {
        for (int i = 0; i < burstCount; i++)
        {
            FireBullet(transform.forward);
            yield return new WaitForSeconds(burstInterval);
        }
    }

    private void FireSpread()
    {
        float spreadAngle = 45f;
        int spreadCount = 5;

        for (int i = 0; i < spreadCount; i++)
        {
            float angle = -spreadAngle / 2 + (spreadAngle / (spreadCount - 1)) * i;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
            FireBullet(direction);
        }
    }

    private void FireCircle()
    {
        int bulletCount = 8;
        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
            FireBullet(direction);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth;

            // V�nd health bar mot kameran
            if (healthBarCanvas != null && mainCamera != null)
            {
                healthBarCanvas.transform.rotation = mainCamera.transform.rotation;
            }
        }
    }

    private void Die()
    {
        isDead = true;

        // Ta bort health bar
        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
        }

        // F�rst�r fienden
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        ProjectileDamage projectile = other.GetComponent<ProjectileDamage>();
        if (projectile != null)
        {
            TakeDamage(projectile.damage);
            Destroy(other.gameObject);
        }
    }
}
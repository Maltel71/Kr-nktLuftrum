using UnityEngine;
using System.Collections;

public class MissileVehicleScript : MonoBehaviour
{
    [Header("Vehicle References")]
    public Transform player;
    public Transform missileLauncher;
    public GameObject missilePrefab;
    public Transform[] shootSpawns;
    public GameObject explosionEffectPrefab;

    [Header("Vehicle Settings")]
    public float rotSpeed = 180f;
    public float detectionRange = 40f;
    public float missileRange = 35f;
    public float shootingDelay = 2f;

    [Header("Tracking Settings")]
    public bool smoothTracking = true;
    public float trackingSpeed = 90f;

    [Header("Missile Salvo Settings")]
    public int missilesPerSalvo = 6;
    public float timeBetweenMissiles = 0.2f;
    public float timeBetweenSalvos = 5f;
    public bool alternateSpawns = true;

    [Header("Audio")]
    private AudioManager audioManager;

    private bool hasTriggeredAction = false;
    private bool playerInRange = false;
    private bool isShooting = false;
    private bool isDestroyed = false;
    private int currentSpawnIndex = 0;
    private Coroutine shootingCoroutine;
    private float trackingStartTime = 0f;

    void Start()
    {
        audioManager = AudioManager.Instance;

        // Hitta spelaren automatiskt om inte tilldelad
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        // Om inga spawn-punkter är tilldelade, använd fordonets position
        if (shootSpawns == null || shootSpawns.Length == 0)
        {
            shootSpawns = new Transform[1] { transform };
        }
    }

    void Update()
    {
        if (isDestroyed || player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool wasPlayerInRange = playerInRange;
        playerInRange = distanceToPlayer <= detectionRange;

        // Om spelaren precis kom inom räckvidd, börja spåra
        if (playerInRange && !wasPlayerInRange)
        {
            trackingStartTime = Time.time;
            Debug.Log("Player detected! Starting tracking...");
        }

        if (playerInRange)
        {
            // Följ spelaren kontinuerligt
            RotateLauncherTowardsPlayer();

            // Skjut bara om vi har spårat tillräckligt länge och inte redan skjuter
            float timeSinceTracking = Time.time - trackingStartTime;
            if (!isShooting && timeSinceTracking >= shootingDelay && distanceToPlayer <= missileRange)
            {
                Debug.Log($"Shooting delay complete ({shootingDelay}s)! Starting salvo...");
                StartShooting();
            }
        }
        else
        {
            // Stoppa skjutning om spelaren är utanför räckvidd
            if (isShooting)
            {
                StopShooting();
            }
        }
    }

    private void RotateLauncherTowardsPlayer()
    {
        Vector3 lookDirection = player.position - missileLauncher.position;

        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-lookDirection, Vector3.up);
            float rotationSpeed = smoothTracking ? trackingSpeed : rotSpeed;

            missileLauncher.rotation = Quaternion.RotateTowards(
                missileLauncher.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void StartShooting()
    {
        if (shootingCoroutine == null)
        {
            shootingCoroutine = StartCoroutine(ShootSalvoRoutine());

            // Trigga separat zoom-script (om det finns)
            if (!hasTriggeredAction)
            {
                hasTriggeredAction = true;

                // Skicka event till zoom-script
                EnemyZoomTrigger zoomTrigger = GetComponent<EnemyZoomTrigger>();
                if (zoomTrigger != null)
                {
                    zoomTrigger.TriggerZoom();
                }

                Debug.Log($"Missile vehicle starting action!");
            }
        }
    }

    private void StopShooting()
    {
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
        isShooting = false;
    }

    private IEnumerator ShootSalvoRoutine()
    {
        while (playerInRange && !isDestroyed)
        {
            isShooting = true;
            yield return StartCoroutine(FireSalvo());
            yield return new WaitForSeconds(timeBetweenSalvos);
        }

        isShooting = false;
        shootingCoroutine = null;
    }

    private IEnumerator FireSalvo()
    {
        Debug.Log($"Firing salvo of {missilesPerSalvo} missiles!");

        for (int i = 0; i < missilesPerSalvo; i++)
        {
            if (isDestroyed) yield break;

            FireSingleMissile();

            if (i < missilesPerSalvo - 1)
            {
                yield return new WaitForSeconds(timeBetweenMissiles);
            }
        }
    }

    private void FireSingleMissile()
    {
        Transform spawnPoint = GetNextSpawnPoint();
        Vector3 launchDirection = -missileLauncher.forward;
        Quaternion launchRotation = Quaternion.LookRotation(launchDirection);
        GameObject missile = Instantiate(missilePrefab, spawnPoint.position, launchRotation);
        audioManager?.PlayMissileLaunchSound();
        Debug.Log($"Missile fired in direction: {launchDirection}");
    }

    private Transform GetNextSpawnPoint()
    {
        if (alternateSpawns && shootSpawns.Length > 1)
        {
            Transform spawnPoint = shootSpawns[currentSpawnIndex];
            currentSpawnIndex = (currentSpawnIndex + 1) % shootSpawns.Length;
            return spawnPoint;
        }
        else
        {
            return shootSpawns[Random.Range(0, shootSpawns.Length)];
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isDestroyed && (other.CompareTag("Bomb") || other.CompareTag("Player Bullet")))
        {
            DestroyVehicle();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isDestroyed && collision.gameObject.CompareTag("Bomb"))
        {
            DestroyVehicle();
        }
    }

    private void DestroyVehicle()
    {
        if (isDestroyed) return;

        isDestroyed = true;
        StopShooting();

        Debug.Log("Missile vehicle destroyed!");

        // Skapa explosion med pool om möjligt
        if (ExplosionPool.Instance != null)
        {
            GameObject explosion = ExplosionPool.Instance.GetExplosion(ExplosionType.Large);
            explosion.transform.position = transform.position;
            ExplosionPool.Instance.ReturnExplosionToPool(explosion, 3f);
        }
        else if (explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 3f);
        }

        // Spela explosionsljud
        audioManager?.PlayBombSound(BombSoundType.Explosion);

        // Ge poäng till spelaren
        ScoreManager.Instance?.AddEnemyShipPoints();

        // Inaktivera colliders
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // Förstör fordonet
        Destroy(gameObject, 0.1f);
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        // Rita detektionsradie
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Rita skjutradie
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, missileRange);

        // Rita spawn-punkter
        if (shootSpawns != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform spawn in shootSpawns)
            {
                if (spawn != null)
                {
                    Gizmos.DrawWireSphere(spawn.position, 0.5f);
                    Gizmos.DrawRay(spawn.position, spawn.forward * 2f);
                }
            }
        }
    }

    // Publika metoder för att konfigurera fordonet
    public void SetSalvoSettings(int missilesPerSalvo, float timeBetweenMissiles, float timeBetweenSalvos)
    {
        this.missilesPerSalvo = missilesPerSalvo;
        this.timeBetweenMissiles = timeBetweenMissiles;
        this.timeBetweenSalvos = timeBetweenSalvos;
    }

    public void SetDetectionRange(float range)
    {
        detectionRange = range;
        missileRange = range * 0.8f;
    }
}
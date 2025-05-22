using UnityEngine;
using System.Collections;

public class SimpleSearchlight : MonoBehaviour
{
    [Header("Basic Setup")]
    [SerializeField] private Light spotlight;
    [SerializeField] private Transform rotatingPart; // Det som roterar
    [SerializeField] private Transform firePoint;

    [Header("Detection")]
    [SerializeField] private float searchRange = 25f;
    [SerializeField] private float searchSpeed = 30f; // Sökhastighet
    [SerializeField] private float trackingSpeed = 60f; // Följningshastighet

    [Header("Shooting")]
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float bulletDamage = 20f;

    private Transform player;
    private bool playerSpotted = false;
    private float currentRotation = 0f;
    private float targetRotation = 0f;
    private float nextFireTime = 0f;
    private int searchDirection = 1;

    private void Start()
    {
        // Hitta spelaren
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Setup spotlight
        if (spotlight == null)
            spotlight = GetComponentInChildren<Light>();

        if (rotatingPart == null)
            rotatingPart = transform;

        SetupLight();

        // Starta sökmönster
        StartCoroutine(SearchPattern());
    }

    private void SetupLight()
    {
        if (spotlight != null)
        {
            spotlight.type = LightType.Spot;
            spotlight.spotAngle = 45f;
            spotlight.range = searchRange;
            spotlight.intensity = 4f;
            spotlight.color = Color.white;
        }
    }

    private void Update()
    {
        CheckForPlayer();
        UpdateRotation();
        HandleShooting();
    }

    private void CheckForPlayer()
    {
        if (player == null) return;

        Vector3 directionToPlayer = player.position - transform.position;
        float distance = directionToPlayer.magnitude;

        // För långt bort?
        if (distance > searchRange)
        {
            if (playerSpotted)
            {
                PlayerLost();
            }
            return;
        }

        // Inom ljuskäglan?
        Vector3 searchlightDirection = rotatingPart.forward;
        float angle = Vector3.Angle(searchlightDirection, directionToPlayer);

        if (angle < 22.5f) // Halva spotAngle
        {
            // Raycast för att se om spelaren verkligen syns
            if (Physics.Raycast(transform.position, directionToPlayer.normalized, out RaycastHit hit, distance))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    if (!playerSpotted)
                    {
                        PlayerSpotted();
                    }

                    // Uppdatera målrotation till spelarens position
                    float angleToPlayer = Mathf.Atan2(directionToPlayer.x, directionToPlayer.z) * Mathf.Rad2Deg;
                    targetRotation = angleToPlayer;
                }
            }
        }
        else if (playerSpotted)
        {
            PlayerLost();
        }
    }

    private void PlayerSpotted()
    {
        playerSpotted = true;
        spotlight.color = Color.red;
        spotlight.intensity = 6f;

        // Spela alert ljud
        AudioManager.Instance?.PlayBossAlert();

        Debug.Log("Searchlight: Player spotted!");
    }

    private void PlayerLost()
    {
        playerSpotted = false;
        spotlight.color = Color.white;
        spotlight.intensity = 4f;

        Debug.Log("Searchlight: Player lost!");
    }

    private void UpdateRotation()
    {
        float speed = playerSpotted ? trackingSpeed : searchSpeed;
        currentRotation = Mathf.MoveTowardsAngle(currentRotation, targetRotation, speed * Time.deltaTime);

        // Applicera rotation
        rotatingPart.rotation = Quaternion.Euler(0, currentRotation, 0);
    }

    private IEnumerator SearchPattern()
    {
        while (true)
        {
            // Bara söka om spelaren inte är upptäckt
            if (!playerSpotted)
            {
                // Slumpmässig sökning mellan -90 och +90 grader
                targetRotation = Random.Range(-90f, 90f);

                yield return new WaitForSeconds(Random.Range(2f, 4f));
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private void HandleShooting()
    {
        if (playerSpotted && Time.time >= nextFireTime && firePoint != null)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        Vector3 shootDirection = rotatingPart.forward;

        // Använd bullet pool
        GameObject bullet = BulletPool.Instance.GetBullet(false);
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = Quaternion.LookRotation(shootDirection);

        if (bullet.TryGetComponent<BulletSystem>(out var bulletSystem))
        {
            bulletSystem.Initialize(shootDirection, true, bulletDamage);
        }

        // Ljud
        AudioManager.Instance?.PlayEnemyShootSound();
    }

    private void OnDrawGizmosSelected()
    {
        // Rita räckvidd
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRange);

        // Rita sökområde
        Gizmos.color = Color.red;
        Vector3 leftBound = Quaternion.Euler(0, -90, 0) * transform.forward * searchRange;
        Vector3 rightBound = Quaternion.Euler(0, 90, 0) * transform.forward * searchRange;
        Gizmos.DrawLine(transform.position, transform.position + leftBound);
        Gizmos.DrawLine(transform.position, transform.position + rightBound);
    }
}
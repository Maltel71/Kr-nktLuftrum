using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GroundVehicleController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float waypointReachedDistance = 1f;
    [SerializeField] private bool useRoadNetwork = true;
    [SerializeField] private bool isCivilian = true;

    [Header("Behavior Settings")]
    [SerializeField] private float minStopTime = 1f;
    [SerializeField] private float maxStopTime = 4f;
    [SerializeField] private float stopChance = 0.1f;
    [SerializeField] private float randomDirectionChangeChance = 0.02f;
    [SerializeField] private float patrolRadius = 40f;

    [Header("Military Vehicle Settings")]
    [SerializeField] private bool canAttackPlayer = false;
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 12f;
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform turretTransform;
    [SerializeField] private Transform firingPoint;

    // Internt tillst�nd
    private Vector3 currentWaypoint;
    private List<Vector3> availableWaypoints = new List<Vector3>();
    private bool isStopped = false;
    private bool isWaiting = false;
    private Vector3 startPosition;
    private bool playerDetected = false;
    private Transform playerTransform;
    private float nextAttackTime = 0f;
    private Coroutine stopRoutine;

    private void Start()
    {
        startPosition = transform.position;
        GenerateWaypoints();
        SetRandomWaypoint();

        // Hitta spelaren
        if (!isCivilian && canAttackPlayer)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
    }

    private void Update()
    {
        if (!isCivilian && canAttackPlayer && playerTransform != null)
        {
            // Kontrollera om spelaren �r inom detekteringsomr�det
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= detectionRange)
            {
                playerDetected = true;

                // Rikta turret mot spelaren om det finns
                if (turretTransform != null)
                {
                    Vector3 targetDirection = playerTransform.position - turretTransform.position;
                    targetDirection.y = 0;
                    Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                    turretTransform.rotation = Quaternion.Slerp(turretTransform.rotation, targetRotation, Time.deltaTime * rotationSpeed * 2);
                }

                // Attackera om inom r�ckvidd
                if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
                {
                    Attack();
                    nextAttackTime = Time.time + attackCooldown;
                }
            }
            else
            {
                playerDetected = false;
            }
        }

        if (!isStopped && !isWaiting)
        {
            MoveToWaypoint();
        }

        // Slumpm�ssig riktnings�ndring eller stopp under r�relse
        if (!isWaiting && !playerDetected && Random.value < randomDirectionChangeChance * Time.deltaTime)
        {
            if (Random.value < stopChance)
            {
                StopVehicle();
            }
            else
            {
                SetRandomWaypoint();
            }
        }
    }

    private void MoveToWaypoint()
    {
        if (currentWaypoint == Vector3.zero)
            return;

        // Ber�kna riktning till waypoint
        Vector3 direction = currentWaypoint - transform.position;
        direction.y = 0;

        // Kontrollera om vi n�tt waypoint
        if (direction.magnitude < waypointReachedDistance)
        {
            SetRandomWaypoint();

            // Chans att stanna vid waypoint
            if (Random.value < stopChance)
            {
                StopVehicle();
            }
            return;
        }

        // Rotera mot waypoint
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // Flytta fram�t
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

    private void GenerateWaypoints()
    {
        // Generera waypoints i en cirkel runt startpositionen
        int waypointCount = Random.Range(5, 10);
        for (int i = 0; i < waypointCount; i++)
        {
            float angle = i * (360f / waypointCount);
            float radius = Random.Range(patrolRadius * 0.3f, patrolRadius);
            float x = startPosition.x + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float z = startPosition.z + radius * Mathf.Sin(angle * Mathf.Deg2Rad);

            // S�tt Y-position f�r att f�lja terr�ngen
            float y = GetTerrainHeight(new Vector3(x, 0, z));

            availableWaypoints.Add(new Vector3(x, y, z));
        }
    }

    private float GetTerrainHeight(Vector3 position)
    {
        // Raycast f�r att hitta markh�jden
        if (Physics.Raycast(position + Vector3.up * 100, Vector3.down, out RaycastHit hit, 200f, LayerMask.GetMask("Terrain", "Ground")))
        {
            return hit.point.y;
        }
        return position.y;
    }

    private void SetRandomWaypoint()
    {
        if (availableWaypoints.Count == 0)
            return;

        currentWaypoint = availableWaypoints[Random.Range(0, availableWaypoints.Count)];

        // Uppdatera h�jden baserat p� terr�ngen
        currentWaypoint.y = GetTerrainHeight(currentWaypoint);
    }

    private void StopVehicle()
    {
        isStopped = true;
        if (stopRoutine != null)
            StopCoroutine(stopRoutine);

        stopRoutine = StartCoroutine(StopForTime());
    }

    private IEnumerator StopForTime()
    {
        isWaiting = true;
        float stopTime = Random.Range(minStopTime, maxStopTime);

        yield return new WaitForSeconds(stopTime);

        isWaiting = false;
        isStopped = false;
        SetRandomWaypoint();
    }

    private void Attack()
    {
        if (projectilePrefab == null || firingPoint == null)
            return;

        // Instantiera projektil och rikta den mot spelaren
        GameObject projectile = Instantiate(projectilePrefab, firingPoint.position, firingPoint.rotation);

        // Om det finns en Rigidbody, ge den hastighet
        if (projectile.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = firingPoint.forward * 30f;
        }

        // Om du har ett projektilsystem, initiera det
        if (projectile.TryGetComponent<BulletSystem>(out var bulletSystem))
        {
            bulletSystem.Initialize(firingPoint.forward, true, 15f);
        }

        // Spela ljud om det finns
        AudioManager.Instance?.PlayEnemyShootSound();

        Destroy(projectile, 3f);
    }

    private void OnDrawGizmosSelected()
    {
        // Rita waypoints och detekteringsomr�de i editorn
        Gizmos.color = Color.blue;
        foreach (Vector3 waypoint in availableWaypoints)
        {
            Gizmos.DrawSphere(waypoint, 0.5f);
        }

        if (currentWaypoint != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentWaypoint, 0.7f);
            Gizmos.DrawLine(transform.position, currentWaypoint);
        }

        if (!isCivilian && canAttackPlayer)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
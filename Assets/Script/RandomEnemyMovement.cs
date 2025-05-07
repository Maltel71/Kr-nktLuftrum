using UnityEngine;

public class RandomEnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Attack Behavior")]
    [SerializeField] private float turnAroundDistance = 20f;  // Hur l�ngt bakom spelaren fienden f�r g� innan den v�nder
    [SerializeField] private float turnSpeed = 3f;           // Hur snabbt fienden v�nder

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLogs = false;

    private Transform playerTransform;
    private Vector3 lastPosition;
    private bool isReturning = false;  // Om fienden �r p� v�g tillbaka mot spelaren

    private void Start()
    {
        // Hitta spelaren
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogError("Kunde inte hitta spelaren! Se till att spelaren har taggen 'Player'");
        }
        else
        {
            playerTransform = playerObject.transform;
            Debug.Log("Hittade spelaren p� position: " + playerTransform.position);
        }

        // Spara startposition f�r debugging
        lastPosition = transform.position;
        Debug.Log($"Enemy spawned at position: {transform.position}");
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            // Om vi inte hittar spelaren, leta igen
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null) return;
            playerTransform = playerObject.transform;
        }

        // Kontrollera om fienden �r bakom spelaren och beh�ver v�nda
        CheckIfBehindPlayer();

        // Ber�kna riktning baserat p� om fienden �r p� v�g tillbaka eller mot spelaren
        Vector3 targetDirection;
        if (isReturning)
        {
            // Riktning mot spelaren (returnera)
            targetDirection = (playerTransform.position - transform.position).normalized;
        }
        else
        {
            // Normal riktning mot spelaren
            targetDirection = (playerTransform.position - transform.position).normalized;

            // Tvinga alltid fram�tr�relse (negativt Z) n�r vi n�rmar oss spelaren f�rsta g�ngen
            if (targetDirection.z > 0)
            {
                targetDirection.z = -Mathf.Abs(targetDirection.z);
                targetDirection = targetDirection.normalized;
            }
        }

        // Flytta fienden i riktningen
        transform.position += targetDirection * moveSpeed * Time.deltaTime;

        // Rotera fienden f�r att peka mot riktningen
        if (targetDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(targetDirection),
                turnSpeed * Time.deltaTime
            );
        }

        // Logga position f�r debugging
        if (showDebugLogs && Vector3.Distance(transform.position, lastPosition) > 0.1f)
        {
            Debug.Log($"Enemy moved from {lastPosition} to {transform.position}, isReturning: {isReturning}");
            lastPosition = transform.position;
        }
    }

    private void CheckIfBehindPlayer()
    {
        if (playerTransform == null) return;

        // Om fienden �r f�r l�ngt bakom spelaren, s�tt isReturning = true
        if (transform.position.z < playerTransform.position.z - turnAroundDistance)
        {
            if (!isReturning)
            {
                isReturning = true;
                Debug.Log("Enemy is turning around to attack player again");
            }
        }
        // Om fienden �r framf�r spelaren igen, s�tt isReturning = false
        else if (transform.position.z > playerTransform.position.z && isReturning)
        {
            isReturning = false;
            Debug.Log("Enemy is now in front of player again");
        }
    }

    // Metod som anropas fr�n EnemySpawner
    public void ForcePosition(Vector3 position)
    {
        transform.position = position;
        lastPosition = position;
        Debug.Log($"Enemy position forced to: {position}");
    }
}
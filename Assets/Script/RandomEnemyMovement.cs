using UnityEngine;

public class RandomEnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Attack Behavior")]
    [SerializeField] private float turnAroundDistance = 20f;  // Hur långt bakom spelaren fienden får gå innan den vänder
    [SerializeField] private float turnSpeed = 3f;           // Hur snabbt fienden vänder

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLogs = false;

    private Transform playerTransform;
    private Vector3 lastPosition;
    private bool isReturning = false;  // Om fienden är på väg tillbaka mot spelaren

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
            Debug.Log("Hittade spelaren på position: " + playerTransform.position);
        }

        // Spara startposition för debugging
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

        // Kontrollera om fienden är bakom spelaren och behöver vända
        CheckIfBehindPlayer();

        // Beräkna riktning baserat på om fienden är på väg tillbaka eller mot spelaren
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

            // Tvinga alltid framåtrörelse (negativt Z) när vi närmar oss spelaren första gången
            if (targetDirection.z > 0)
            {
                targetDirection.z = -Mathf.Abs(targetDirection.z);
                targetDirection = targetDirection.normalized;
            }
        }

        // Flytta fienden i riktningen
        transform.position += targetDirection * moveSpeed * Time.deltaTime;

        // Rotera fienden för att peka mot riktningen
        if (targetDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(targetDirection),
                turnSpeed * Time.deltaTime
            );
        }

        // Logga position för debugging
        if (showDebugLogs && Vector3.Distance(transform.position, lastPosition) > 0.1f)
        {
            Debug.Log($"Enemy moved from {lastPosition} to {transform.position}, isReturning: {isReturning}");
            lastPosition = transform.position;
        }
    }

    private void CheckIfBehindPlayer()
    {
        if (playerTransform == null) return;

        // Om fienden är för långt bakom spelaren, sätt isReturning = true
        if (transform.position.z < playerTransform.position.z - turnAroundDistance)
        {
            if (!isReturning)
            {
                isReturning = true;
                Debug.Log("Enemy is turning around to attack player again");
            }
        }
        // Om fienden är framför spelaren igen, sätt isReturning = false
        else if (transform.position.z > playerTransform.position.z && isReturning)
        {
            isReturning = false;
            Debug.Log("Enemy is now in front of player again");
        }
    }

    // Metod som anropas från EnemySpawner
    public void ForcePosition(Vector3 position)
    {
        transform.position = position;
        lastPosition = position;
        Debug.Log($"Enemy position forced to: {position}");
    }
}
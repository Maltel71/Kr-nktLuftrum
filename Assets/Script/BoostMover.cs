using UnityEngine;

public class BoostMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float destroyDistance = 100f;

    [Header("Visuals")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private float rotationSpeed = 90f;

    private Transform playerTransform;
    private Vector3 startPosition;
    private float timeSinceSpawn;

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        startPosition = transform.position;

        if (playerTransform == null)
        {
            Debug.LogWarning("Ingen spelare hittad för boost att följa!");
        }
    }

    private void Update()
    {
        if (playerTransform != null)
        {
            timeSinceSpawn += Time.deltaTime;
            float newY = startPosition.y + Mathf.Sin(timeSinceSpawn * bobSpeed) * bobHeight;

            Vector3 targetPosition = playerTransform.position;
            targetPosition.y = newY;

            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            if (transform.position.z < playerTransform.position.z - destroyDistance)
            {
                Debug.Log($"Boost {gameObject.name} förstörs - för långt bakom spelaren");
                Destroy(gameObject);
            }
        }
    }

    public void Initialize(float speed)
    {
        moveSpeed = speed;
    }
}
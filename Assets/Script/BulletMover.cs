using UnityEngine;

public class BulletMover : MonoBehaviour
{
    [SerializeField] public float speed = 1000f; // Mycket högre hastighet
    [HideInInspector] public Vector3 direction;

    private float fixedHeight;
    private Vector3 lastPosition;
    private float distanceTraveled;

    private void Start()
    {
        fixedHeight = transform.position.y;
        lastPosition = transform.position;
        Debug.Log($"Bullet startad. Position: {transform.position}, Direction: {direction}, Speed: {speed}");
    }

    private void Update()
    {
        Vector3 movement = direction * speed * Time.deltaTime;
        Vector3 newPosition = transform.position + movement;
        newPosition.y = fixedHeight;
        transform.position = newPosition;

        float frameDistance = Vector3.Distance(lastPosition, transform.position);
        distanceTraveled += frameDistance;

        // Rita en linje som visar kulans bana
        Debug.DrawLine(lastPosition, transform.position, Color.yellow, 0.1f);

        if (Time.frameCount % 10 == 0)
        {
            Debug.Log($"Kula på position {transform.position}, total distans: {distanceTraveled:F2}");
        }

        lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Bullet träffade: {other.gameObject.name} på position {transform.position} efter att ha rest {distanceTraveled:F2} enheter");

        if (other.CompareTag("Player"))
        {
            var playerHealth = other.GetComponent<PlaneHealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(10f);
                Debug.Log("Träffade spelaren och gjorde skada!");
            }
            Destroy(gameObject);
        }
    }
}
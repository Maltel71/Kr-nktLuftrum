using UnityEngine;
using System.Collections;

public class Missile : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform missile;
    public GameObject missileExplosionPrefab;
    public Rigidbody rb;

    [Header("Missile Settings")]
    public float turnValue = 5f;
    public float speed = 5f;
    public float rotation = 4f;
    public float selfDestroyTime = 8f;

    [Header("Combat")]
    public int damagePoints = 20;

    [Header("Flare Settings")]
    [SerializeField] private float flareAttractionStrength = 2f;

    private Transform currentTarget;
    private AudioManager audioManager;
    private PlaneHealthSystem playerHealthSystem;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentTarget = player;

        playerHealthSystem = player.GetComponent<PlaneHealthSystem>();
        audioManager = AudioManager.Instance;

        StartCoroutine(SelfDestroyTimer());
    }

    private void FindTarget()
    {
        GameObject[] flares = GameObject.FindGameObjectsWithTag("Flare");

        if (flares.Length > 0)
        {
            currentTarget = flares[0].transform; // Always target the first flare if any exist
        }
        else
        {
            currentTarget = player;
        }

        //Debug.Log($"Found {flares.Length} flares");
        //Debug.Log($"Current target: {currentTarget.name}");
    }

    void FixedUpdate()
    {
        if (Time.frameCount % 10 == 0) // Check for new targets every 10 frames
        {
            FindTarget();
        }

        if (currentTarget == null) return;

        // Rotate the missile around its own axis
        missile.Rotate(new Vector3(0f, 0f, rotation));

        // Move the missile forward
        rb.linearVelocity = transform.forward * speed;

        // Aim the missile towards the target
        Quaternion missileTargetRotation = Quaternion.LookRotation(currentTarget.position - transform.position);

        // If the target is a flare, increase the attraction strength
        float turnSpeed = currentTarget.CompareTag("Flare") ? turnValue * flareAttractionStrength : turnValue;
        rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, missileTargetRotation, turnSpeed));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Collision with player detected");

            if (playerHealthSystem != null)
            {
                playerHealthSystem.TakeDamage(damagePoints);
            }

            ExplodeAndDestroy(other.transform.position);
        }
        else if (other.CompareTag("Flare"))
        {
            Debug.Log("Collision with flare detected");
            ExplodeAndDestroy(other.transform.position, other.gameObject);
        }
        else
        {
            ExplodeAndDestroy(transform.position);
        }
    }

    private void ExplodeAndDestroy(Vector3 position, GameObject otherObject = null)
    {
        CreateExplosion(position);
        audioManager?.PlayCombatSound(CombatSoundType.Hit);

        if (otherObject != null)
        {
            Destroy(otherObject); // Destroy the other object (flare)
        }

        Destroy(gameObject); // Destroy the missile
    }

    private void CreateExplosion(Vector3 position)
    {
        GameObject explosion = Instantiate(missileExplosionPrefab, position, Quaternion.identity);
        Destroy(explosion, 4f);
    }

    IEnumerator SelfDestroyTimer()
    {
        yield return new WaitForSeconds(selfDestroyTime);
        ExplodeAndDestroy(transform.position);
    }
}
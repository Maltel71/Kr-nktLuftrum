using UnityEngine;
using System.Collections;

public class MissileEnemy : MonoBehaviour
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

        if (player != null)
        {
            currentTarget = player;
            playerHealthSystem = player.GetComponent<PlaneHealthSystem>();
            Debug.Log($"PlaneHealthSystem hittat: {playerHealthSystem != null}");
        }
        else
        {
            Debug.LogError("Kunde inte hitta spelare med Player-tag!");
        }

        //playerHealthSystem = player.GetComponent<PlaneHealthSystem>();
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
            Debug.Log("Missile Collision with player detected");

            // H�mta PlaneHealthSystem direkt fr�n kollisionsobjektet
            PlaneHealthSystem targetHealth = other.GetComponent<PlaneHealthSystem>();

            if (targetHealth != null)
            {
                Debug.Log($"Applying {damagePoints} damage to player");
                targetHealth.TakeDamage(damagePoints);

                CameraShake.Instance?.ShakaCameraVidBomb();
            }
            else
            {
                Debug.LogWarning("Player hit but no PlaneHealthSystem found!");
            }

            ExplodeAndDestroy(other.transform.position);
        }
        else if (other.CompareTag("Flare"))
        {
            Debug.Log("Missile Collision with flare detected");
            ExplodeAndDestroy(other.transform.position, other.gameObject);
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
        AudioManager.Instance?.PlayMissileHitSound();
    }


    private void CreateExplosion(Vector3 position)
    {
        // Anv�nd ExplosionPool ist�llet f�r direkt instansiering
        if (ExplosionPool.Instance != null)
        {
            GameObject explosion = ExplosionPool.Instance.GetExplosion(ExplosionType.Large);
            if (explosion != null)
            {
                explosion.transform.position = position;
                ExplosionPool.Instance.ReturnExplosionToPool(explosion, 3f);
                Debug.Log("Enemy missile: Explosion created from pool");
            }
        }
        else if (missileExplosionPrefab != null)
        {
            // Fallback till original-implementering
            GameObject explosion = Instantiate(missileExplosionPrefab, position, Quaternion.identity);
            explosion.transform.localScale = Vector3.one * 2f; // G�r explosionen st�rre f�r b�ttre synlighet
            Destroy(explosion, 4f);
            Debug.Log("Enemy missile: Explosion created via Instantiate");
        }
        else
        {
            Debug.LogError("No explosion prefab assigned and no ExplosionPool available");
        }
    }

    IEnumerator SelfDestroyTimer()
    {
        yield return new WaitForSeconds(selfDestroyTime);
        ExplodeAndDestroy(transform.position);
    }
}
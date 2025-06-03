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

    [Header("Lifetime")]
    [SerializeField] private float boostLifetime = 15f;

    [Header("Collision Settings")]
    [SerializeField] private bool ignorePlayerBullets = true;

    private float aliveTime = 0f;
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

        // Sätt upp korrekt tag och layer för boost
        SetupBoostProperties();
    }

    private void SetupBoostProperties()
    {
        // Sätt tag om den inte redan är satt
        if (gameObject.tag == "Untagged")
        {
            gameObject.tag = "Boost";
        }

        // Sätt layer för boost (om du har skapat en)
        int boostLayer = LayerMask.NameToLayer("Boost");
        if (boostLayer != -1)
        {
            gameObject.layer = boostLayer;
        }

        // Ignorera kollisioner med spelarens skott
        if (ignorePlayerBullets)
        {
            IgnorePlayerBulletCollisions();
        }
    }

    private void IgnorePlayerBulletCollisions()
    {
        // Hitta alla spelarens skott och ignorera kollisioner
        GameObject[] playerBullets = GameObject.FindGameObjectsWithTag("Player Bullet");
        Collider boostCollider = GetComponent<Collider>();

        if (boostCollider != null)
        {
            foreach (GameObject bullet in playerBullets)
            {
                Collider bulletCollider = bullet.GetComponent<Collider>();
                if (bulletCollider != null)
                {
                    Physics.IgnoreCollision(boostCollider, bulletCollider);
                }
            }
        }

        // Sätt upp kontinuerlig ignorering för nya skott
        InvokeRepeating(nameof(ContinuouslyIgnoreBullets), 0.5f, 0.5f);
    }

    private void ContinuouslyIgnoreBullets()
    {
        if (!ignorePlayerBullets) return;

        // Kontinuerligt ignorera nya spelarskott
        GameObject[] playerBullets = GameObject.FindGameObjectsWithTag("Player Bullet");
        Collider boostCollider = GetComponent<Collider>();

        if (boostCollider != null)
        {
            foreach (GameObject bullet in playerBullets)
            {
                if (bullet != null)
                {
                    Collider bulletCollider = bullet.GetComponent<Collider>();
                    if (bulletCollider != null)
                    {
                        Physics.IgnoreCollision(boostCollider, bulletCollider);
                    }
                }
            }
        }
    }

    private void Update()
    {
        aliveTime += Time.deltaTime;

        if (aliveTime >= boostLifetime)
        {
            Debug.Log($"Boost {gameObject.name} förstörs - livstid uppnådd");
            Destroy(gameObject);
            return;
        }

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

    // Säkerställ att endast spelaren kan plocka upp boost
    private void OnTriggerEnter(Collider other)
    {
        // Bara reagera på spelaren, inte på skott
        if (other.CompareTag("Player"))
        {
            // Låt BoostPickup hantera resten
            BoostPickup pickup = GetComponent<BoostPickup>();
            if (pickup != null)
            {
                // BoostPickup hanterar redan spelar-kollisionen
                return;
            }
        }
        // Ignorera alla andra kollisioner (inklusive Player Bullets)
    }

    public void Initialize(float speed)
    {
        moveSpeed = speed;
    }

    private void OnDestroy()
    {
        // Stoppa invoke när objektet förstörs
        CancelInvoke();
    }

    // Debug metod för att testa
    [ContextMenu("Test Ignore Bullets")]
    private void TestIgnoreBullets()
    {
        IgnorePlayerBulletCollisions();
    }
}
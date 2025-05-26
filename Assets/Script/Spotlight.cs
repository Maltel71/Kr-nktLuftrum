using UnityEngine;

public class Spotlight : MonoBehaviour
{
    [Header("Spotlight References")]
    public Transform player;
    public Transform missileRamp;
    public GameObject missilePrefab;
    public Transform shootSpawn;
    public GameObject explosionEffectPrefab;

    [Header("Turret Settings")]
    public float rotSpeed = 3f;
    public float shootDelay = 4f;
    [SerializeField] private float shootRange = 30f;

    private bool targetAquired;
    private bool missileAway;
    private bool isDestroyed = false;
    private AudioManager audioManager;

    void Start()
    {
        audioManager = AudioManager.Instance;
    }

    void Update()
    {
        if (isDestroyed) return;

        if (player != null)
        {
            // Kontrollera avst�ndet till spelaren
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Rotera rampen bara om spelaren �r inom r�ckvidd
            if (distanceToPlayer <= shootRange)
            {
                Vector3 lookDirection = player.position - transform.position;
                missileRamp.rotation = Quaternion.Slerp(missileRamp.transform.rotation,
                                                      Quaternion.LookRotation(lookDirection),
                                                      rotSpeed * Time.deltaTime);

                // Skjut bara om spelaren �r inom r�ckvidd
                if (!missileAway)
                {
                    Invoke("Shoot", shootDelay);
                    missileAway = true;
                }
            }
        }
    }
     

    void Shoot()
    {
        if (!isDestroyed)
        {
            // Spela uppskjutningsljudet
            if (audioManager != null)
            {
                audioManager.PlayMissileLaunchSound();
            }

            Instantiate(missilePrefab, shootSpawn.position, shootSpawn.rotation);
            missileAway = false;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        // Kontrollera om det �r en bomb som tr�ffar
        if (!isDestroyed && collision.gameObject.CompareTag("Bomb"))
        {
            DestroyTurret();
        }
    }

    private void DestroyTurret()
    {
        isDestroyed = true;

        // Skapa explosionseffekt
        if (explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 3f);
        }

        // Spela explosionsljud
        //audioManager?.PlayCombatSound(CombatSoundType.Death);

        // Inaktivera alla colliders
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // F�rst�r turreten
        Destroy(gameObject, 0.1f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            targetAquired = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        targetAquired = false;
        missileAway = true;
    }
}
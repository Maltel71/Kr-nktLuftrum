using UnityEngine;

public class TurretScript : MonoBehaviour
{
    [Header("Turret References")]
    public Transform player;
    public Transform missileRamp;
    public GameObject missilePrefab;
    public Transform shootSpawn;
    public GameObject explosionEffectPrefab;

    [Header("Turret Settings")]
    public float rotSpeed = 3f;
    public float shootDelay = 4f;

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
        if (isDestroyed) return;  // Gör ingenting om turreten är förstörd

        if (player != null)
        {
            // Rotera rampen mot spelaren
            Vector3 lookDirection = player.position - transform.position;
            missileRamp.rotation = Quaternion.Slerp(missileRamp.transform.rotation,
                                                  Quaternion.LookRotation(lookDirection),
                                                  rotSpeed * Time.deltaTime);
        }

        if (!missileAway)
        {
            Invoke("Shoot", shootDelay);
            missileAway = true;
        }
    }

    void Shoot()
    {
        if (!isDestroyed)
        {
            Instantiate(missilePrefab, shootSpawn.position, shootSpawn.rotation);
            missileAway = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Kontrollera om det är en bomb som träffar
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
        audioManager?.PlayCombatSound(CombatSoundType.Death);

        // Inaktivera alla colliders
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // Förstör turreten
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

//using UnityEngine;

//public class TurreScript : MonoBehaviour
//{
//    public Transform player;               // Referens för spelaren
//    public Transform missileRamp;         //  Referens för rampen
//    public float rotSpeed = 3f;          // Rampens rotatioenshastihhet
//    private bool targetAquired;         // Bool som är sann om det finns ett mål inom triggern
//    private bool missileAway;          // Bool som är sann om missil avfyrats
//    public float shootDelay = 4f;     // Tid mellan avfyriingarna
//    public GameObject missilePrefab; // Fält för missileprefaben
//    public Transform shootSpawn;    // referens för spawnpoint
//    public GameObject explosionEffectPrefab;

//    private bool isDestroyed = false;
//    private AudioManager audioManager;

//    void Start()
//    {
//        audioManager = AudioManager.Instance;
//    }

//    void Update()
//    {
//        if (isDestroyed) return;  // om tornet är förstört, sluta uppdatera

//        if (player != null)
//        {
//            Vector3 lookDirection = player.position - transform.position;                                                                                // räkna ut riktnigen mot målet
//            missileRamp.rotation = Quaternion.Slerp(missileRamp.transform.rotation, Quaternion.LookRotation(lookDirection), rotSpeed * Time.deltaTime); // Rotera rmpen mot spelaren 
//        }

//        if (!missileAway)                  // om ingen missil skhutits iväg
//        {
//            Invoke("Shoot", shootDelay);  // kalla på shootfunktionen med fördröjning
//            missileAway = true;          // Missilen avfyrad
//        }
//    }

//    void Shoot()
//    {
//        if (!isDestroyed)
//        {
//            // Skapa en missil vid spawnpoint position med en rotationen som motsvarar soawnpointet
//            Instantiate(missilePrefab, shootSpawn.position, shootSpawn.rotation);
//            missileAway = false; // Tillåt avfyring med fördröjning på nytt
//        }
//    }

//    private void OnCollisionEnter(Collision collision)
//    {
//        // Kontrollera om det är en bomb som träffar
//        if (!isDestroyed && collision.gameObject.CompareTag("Bomb"))
//        {
//            DestroyTurret();
//        }
//    }

//    private void DestroyTurret()
//    {
//        isDestroyed = true;

//        if (explosionEffectPrefab != null)
//        {
//            GameObject explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
//            Destroy(explosion, 3f);
//        }

//        audioManager?.PlayCombatSound(CombatSoundType.Death);
//        Destroy(gameObject, 0.1f);
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            targetAquired = true;  // spelaren lämnar triggerområdet
//        }
//    }

//    private void OnTriggerExit(Collider other)
//    {
//        targetAquired = false;
//        missileAway = true;
//    }
//}
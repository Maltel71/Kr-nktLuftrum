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
        if (isDestroyed) return;  // G�r ingenting om turreten �r f�rst�rd

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
        audioManager?.PlayCombatSound(CombatSoundType.Death);

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

//using UnityEngine;

//public class TurreScript : MonoBehaviour
//{
//    public Transform player;               // Referens f�r spelaren
//    public Transform missileRamp;         //  Referens f�r rampen
//    public float rotSpeed = 3f;          // Rampens rotatioenshastihhet
//    private bool targetAquired;         // Bool som �r sann om det finns ett m�l inom triggern
//    private bool missileAway;          // Bool som �r sann om missil avfyrats
//    public float shootDelay = 4f;     // Tid mellan avfyriingarna
//    public GameObject missilePrefab; // F�lt f�r missileprefaben
//    public Transform shootSpawn;    // referens f�r spawnpoint
//    public GameObject explosionEffectPrefab;

//    private bool isDestroyed = false;
//    private AudioManager audioManager;

//    void Start()
//    {
//        audioManager = AudioManager.Instance;
//    }

//    void Update()
//    {
//        if (isDestroyed) return;  // om tornet �r f�rst�rt, sluta uppdatera

//        if (player != null)
//        {
//            Vector3 lookDirection = player.position - transform.position;                                                                                // r�kna ut riktnigen mot m�let
//            missileRamp.rotation = Quaternion.Slerp(missileRamp.transform.rotation, Quaternion.LookRotation(lookDirection), rotSpeed * Time.deltaTime); // Rotera rmpen mot spelaren 
//        }

//        if (!missileAway)                  // om ingen missil skhutits iv�g
//        {
//            Invoke("Shoot", shootDelay);  // kalla p� shootfunktionen med f�rdr�jning
//            missileAway = true;          // Missilen avfyrad
//        }
//    }

//    void Shoot()
//    {
//        if (!isDestroyed)
//        {
//            // Skapa en missil vid spawnpoint position med en rotationen som motsvarar soawnpointet
//            Instantiate(missilePrefab, shootSpawn.position, shootSpawn.rotation);
//            missileAway = false; // Till�t avfyring med f�rdr�jning p� nytt
//        }
//    }

//    private void OnCollisionEnter(Collision collision)
//    {
//        // Kontrollera om det �r en bomb som tr�ffar
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
//            targetAquired = true;  // spelaren l�mnar triggeromr�det
//        }
//    }

//    private void OnTriggerExit(Collider other)
//    {
//        targetAquired = false;
//        missileAway = true;
//    }
//}
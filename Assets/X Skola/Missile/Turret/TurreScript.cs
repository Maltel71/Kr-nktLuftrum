using UnityEngine;

public class TurreScript : MonoBehaviour
{

    public Transform player;               // Referens för spelaren
    public Transform missileRamp;         //  Referens för rampen
    public float rotSpeed = 3f;          // Rampens rotatioenshastihhet
    private bool targetAquired;         // Bool som är sann om det finns ett mål inom triggern
    private bool missileAway;          // Bool som är sann om missil avfyrats
    public float shootDelay = 4f;     // Tid mellan avfyriingarna
    public GameObject missilePrefab; // Fält för missileprefaben
    public Transform shootSpawn;    // referens för spawnpoint


    void Awake()
    {

    }

    void Update()
    {
        if (player != null)
        {
            Vector3 lookDirection = player.position - transform.position;                                                                                // räkna ut riktnigen mot målet
            missileRamp.rotation = Quaternion.Slerp(missileRamp.transform.rotation, Quaternion.LookRotation(lookDirection), rotSpeed * Time.deltaTime); // Rotera rmpen mot spelaren 
        }
        if (!missileAway)                  // om ingen missil skhutits iväg
        {
            Invoke("Shoot", shootDelay);  // kalla på shootfunktionen med fördröjning
            missileAway = true;          // Missilen avfyrad
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            targetAquired = true;  // spelaren lämnar triggerområdet
        }
    }
    private void OnTriggerExit(Collider other)
    {
        targetAquired = false;
        missileAway = true;
    }

    void Shoot()
    {
        // Skapa en missil vid spawnpoint position med en rotationen som motsvarar soawnpointet
        Instantiate(missilePrefab, shootSpawn.position, shootSpawn.rotation);
        missileAway = false; // Tillåt avfyring med fördröjning på nytt
    }
}

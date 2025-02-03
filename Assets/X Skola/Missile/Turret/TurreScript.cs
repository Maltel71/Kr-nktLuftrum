using UnityEngine;

public class TurreScript : MonoBehaviour
{

    public Transform player;               // Referens f�r spelaren
    public Transform missileRamp;         //  Referens f�r rampen
    public float rotSpeed = 3f;          // Rampens rotatioenshastihhet
    private bool targetAquired;         // Bool som �r sann om det finns ett m�l inom triggern
    private bool missileAway;          // Bool som �r sann om missil avfyrats
    public float shootDelay = 4f;     // Tid mellan avfyriingarna
    public GameObject missilePrefab; // F�lt f�r missileprefaben
    public Transform shootSpawn;    // referens f�r spawnpoint


    void Awake()
    {

    }

    void Update()
    {
        if (player != null)
        {
            Vector3 lookDirection = player.position - transform.position;                                                                                // r�kna ut riktnigen mot m�let
            missileRamp.rotation = Quaternion.Slerp(missileRamp.transform.rotation, Quaternion.LookRotation(lookDirection), rotSpeed * Time.deltaTime); // Rotera rmpen mot spelaren 
        }
        if (!missileAway)                  // om ingen missil skhutits iv�g
        {
            Invoke("Shoot", shootDelay);  // kalla p� shootfunktionen med f�rdr�jning
            missileAway = true;          // Missilen avfyrad
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            targetAquired = true;  // spelaren l�mnar triggeromr�det
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
        missileAway = false; // Till�t avfyring med f�rdr�jning p� nytt
    }
}

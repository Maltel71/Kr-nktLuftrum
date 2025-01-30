using UnityEngine;

public class Missile : MonoBehaviour
{
    [Header("References")]
    public Transform missileTarget;                  // Referens till spelaren
    public Transform missile;                       // Referens till spelaren
    public GameObject MissileExplosionPrefab;      // Fält för explosioner
    public Rigidbody rb;                          // Referens till Rigibody

    [Header("Missile Settings")]
    public float turnValue = 5f;                 // Missilens svängradie
    public float speed = 5f;                    // Misilens hastighet
    public float rotation = 4f;
    public float selfDestroyTime = 8f;         // Tid för att skälvdestruktion

    [Header("Combat")]
    public int DamagePoints = 20;             // Den skada spelaren tillfogas vid kollision 
  
      
    void Awake()
    {
        rb = GetComponent<Rigidbody>();        // Eftersom den är private
        //StartCoroutine(SelfDestroyTimer()); // Starta self destroy timer
    }

    // Update is called once per frame
    void FixedUpdate() // FixedUpdate bättre än update
    {
        missile.Rotate(new Vector3(0f, 0f, rotation));                                                               // Rotera missilen runt sin egen axel
        rb.linearVelocity = transform.forward * speed;                                                              // Driv missilen framåt
        Quaternion missileTargetRotation = Quaternion.LookRotation(missileTarget.position - transform.position);   // Räkna ut riktningen till målet
        rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, missileTargetRotation, turnValue));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Player")
        {
            //collision.transform.GetComponent<PlayerHealth>().health -= DamagePoints;
            // Instansiera en explosion vid den punktdär missilen möter spelaren
            GameObject explosion = Instantiate(MissileExplosionPrefab, collision.transform.position, Quaternion.identity);  //vad ska instaniserar , vart ska det intansiseras, hur ska den flytta(behålla orginal pos)
            Destroy(explosion, 4f);          //
            Destroy(gameObject, 0.2f);            // Förstör missilen
        }
    }

    //IEnumerator SelfDestroyTimer()
    //{
    //    yield return new WaitForSeconds(selfDestroyTime); // Timefunktion
    //    GameObject explosion = Instantiate(MissileExplosionPrefab, transform.position, Quaternion.identity); //Destruera explosionen efter 4 sekunder
    //    Destroy(gameObject, 0.2f);            // Förstör missilen
    //}
}

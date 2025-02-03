using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class Missile : MonoBehaviour
{
    [Header("References")]
    public Transform player;                  // Referens till spelaren
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
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(SelfDestroyTimer());   // Starta self destroy timer

        //// Kolla om taget finns
        //if (missileTarget == null)
        //{
        //    UnityEngine.Debug.LogError("No missile target found");
        //}
        //else
        //{
        //    UnityEngine.Debug.Log("MissileTarget found");
        //}
    }

    //Update is called once per frame
    void FixedUpdate() // FixedUpdate bättre än update
    {
        missile.Rotate(new Vector3(0f, 0f, rotation));                                                               // Rotera missilen runt sin egen axel
        rb.linearVelocity = transform.forward * speed;                                                              // Driv missilen framåt
        Quaternion missileTargetRotation = Quaternion.LookRotation(player.position - transform.position);   // Räkna ut riktningen till målet
        rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, missileTargetRotation, turnValue));
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.collider.tag == "Player")
        {
            //collision.transform.GetComponent<PlayerHealth>().health -= DamagePoints;
            //Destroy(collision.gameObject);
            //Instansiera en explosion vid den punktdär missilen möter spelaren
            GameObject explosion = Instantiate(MissileExplosionPrefab, collision.transform.position, Quaternion.identity);  //vad ska instaniserar , vart ska det intansiseras, hur ska den flytta(behålla orginal pos)
            Destroy(explosion, 4f);
            //
            Destroy(gameObject, 0.2f);            // Förstör missilen
        }
    }

    IEnumerator SelfDestroyTimer()
    {
        yield return new WaitForSeconds(selfDestroyTime); // Timefunktion
        GameObject explosion = Instantiate(MissileExplosionPrefab, transform.position, Quaternion.identity); //Destruera explosionen efter 4 sekunder
        Destroy(gameObject, 0.2f);            // Förstör missilen
    }
}


// Orginal från Lars
//using UnityEngine;
//using System.Collections;

//public class Missile : MonoBehaviour
//{
//    public Transform missileTarget;
//    public Rigidbody rb;
//    public GameObject explosionPrefab;
//    public Transform missile;
//    public float turnValue = 3f;
//    public float speed = 5f;
//    public int damagePoints = 20;
//    public float rotation = 4f;
//    public float selfDestroyTime = 8f;
//    private void Awake()
//    {
//        rb = GetComponent<Rigidbody>();
//        StartCoroutine(SelfDestroyTimer());
//    }
//    void FixedUpdate()
//    {
//        rb.linearVelocity = transform.forward * speed;

//        Quaternion missileTargetRotation = Quaternion.LookRotation(missileTarget.position -
//       transform.position);
//        rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, missileTargetRotation,
//       turnValue));
//        missile.Rotate(new Vector3(0f, 0f, rotation));
//    }
//    private void OnCollisionEnter(Collision collision)
//    {
//        if (collision.collider.tag == "Player")
//        {
//            GameObject explosion = Instantiate(explosionPrefab,
//           collision.transform.position, Quaternion.identity);
//            Destroy(explosion, 4f);
//            Destroy(gameObject, 0.2f);
//            //collision.transform.GetComponent<PlayerHealth>().health -= damagePoints;
//        }
//    }
//    IEnumerator SelfDestroyTimer()
//    {
//        yield return new WaitForSeconds(selfDestroyTime);
//        GameObject explosion = Instantiate(explosionPrefab, transform.position,
//       Quaternion.identity);
//        Destroy(explosion, 4f);
//        Destroy(gameObject, 0.2f);
//    }
//}

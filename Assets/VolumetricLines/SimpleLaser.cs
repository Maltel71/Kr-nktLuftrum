using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleLaser : MonoBehaviour
{
    public float speed = 1000f;
    public GameObject explosionPrefab;
    private int damagePoints = 10;
    public float shotForce = 250f;

    void Update()
    {
        transform.position += transform.forward * Time.deltaTime * speed;   
    }

    private void OnCollisionEnter(Collision collision)
    {
        
            GameObject explosion = Instantiate(explosionPrefab, collision.transform.position, Quaternion.identity) as GameObject;
            collision.transform.GetComponent<Rigidbody>().AddForce(transform.forward * shotForce);
            GameObject.Destroy(explosion, 4f);

        if (collision.collider.tag == "Enemy")
        {
            //collision.transform.GetComponent<EnemyHealth>().health -= damagePoints;
            
        }
    }
}

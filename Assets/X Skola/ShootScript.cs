using System.Collections;                    // Krävs för IEnumerator
using UnityEngine;


public class ShootScript : MonoBehaviour
{
    public GameObject shotPrefab;               // Referensfält för prefaben
    public Transform shootSpawn;               // Referens till spawnpointen
    public float shootRate;                     // Skjuthastighet    
    public float bulletAmount = 4;              // Antal skott
    public float spread = 4f;                   // Skottens spridningsvinkel
                                                // 
    private float totaltSpread;                 // Samtliga projectilers sammalagda spridningsvinkel
                                                // 
    public float rotationOffset;                // Offsetvärde som vrider instanserna
    private int i;

    void Start()
    {
        StartCoroutine(Shoot());                        // Starta Shoot-Coroutine
        totaltSpread = spread * bulletAmount;           // Räkna ut den totala spridningen
    }

 
    void Update()
    {
        
    }

    IEnumerator Shoot()                                 // Start coruutine
    {                                                   //
        while (true)                                    // Evig loop
        {
            for (int i = 0; i < bulletAmount; i++)
            {
                GameObject bullet = PoolingScript.poolingInstance.gameObject;
            }
             
            shootSpawn.transform.position = shootSpawn.position;                            // Rotera
            shootSpawn.transform.rotation = shootSpawn.rotation;                            // Rotera
            shootSpawn.transform.Rotate(0f, totaltSpread * (i - 1) - rotationOffset, 0f);   //
            gameObject.SetActive(true);                                                     // Aktivera prejectilen
                                                                                            //
            yield return new WaitForSeconds(shootRate);
        }

    }
}

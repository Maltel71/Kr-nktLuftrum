using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    public float speed = 80f;                                // Projectiles hastighet
    private float lifetimeTimestamp;                         // Lagrar det lokala livstidsvärdet
    public float lifetime = 2f;                              // Projectilens listid


    private void OnEnable()
    {
        lifetimeTimestamp = lifetime + Time.time;        // Lägg till livstid till nuvarande tid
    }


    void Update()
    {
        transform.Translate(Vector3.forward * speed *  Time.deltaTime);   // Förflytta projectilen frammåt
                                                                          //
        if (Time.time > lifetimeTimestamp);                               // Kolla om livstiden löpt ut
        {                                                                 //
            gameObject.SetActive(false);                                  // Inaktivera projectilen
        }
    }
}

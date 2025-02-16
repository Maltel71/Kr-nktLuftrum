using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    public float speed = 80f;                                // Projectiles hastighet
    private float lifetimeTimestamp;                         // Lagrar det lokala livstidsv�rdet
    public float lifetime = 2f;                              // Projectilens listid


    private void OnEnable()
    {
        lifetimeTimestamp = lifetime + Time.time;        // L�gg till livstid till nuvarande tid
    }


    void Update()
    {
        transform.Translate(Vector3.forward * speed *  Time.deltaTime);   // F�rflytta projectilen framm�t
                                                                          //
        if (Time.time > lifetimeTimestamp);                               // Kolla om livstiden l�pt ut
        {                                                                 //
            gameObject.SetActive(false);                                  // Inaktivera projectilen
        }
    }
}

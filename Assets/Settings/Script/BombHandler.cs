using UnityEngine;

public class BombHandler : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // H�r kan du l�gga till explosionseffekten senare
        Destroy(gameObject);
    }
}
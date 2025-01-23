using UnityEngine;

public class BombHandler : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Här kan du lägga till explosionseffekten senare
        Destroy(gameObject);
    }
}
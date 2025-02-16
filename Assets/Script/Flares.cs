using UnityEngine;

public class Flare : MonoBehaviour
{
    [Header("Flare Settings")]
    [SerializeField] private float lifetime = 3f;    // Hur l�nge flaren existerar
    [SerializeField] private float attractRadius = 20f;  // Hur l�ngt ifr�n missiler p�verkas
    [SerializeField] private float fallSpeed = 5f;    // Hur snabbt flaren faller

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Enkel fallande r�relse
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
    }

    // F�r debug/visualisering
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attractRadius);
    }
}
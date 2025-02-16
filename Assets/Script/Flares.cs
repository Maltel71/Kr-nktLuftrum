using UnityEngine;

public class Flare : MonoBehaviour
{
    [Header("Flare Settings")]
    [SerializeField] private float lifetime = 3f;    // Hur länge flaren existerar
    [SerializeField] private float attractRadius = 20f;  // Hur långt ifrån missiler påverkas
    [SerializeField] private float fallSpeed = 5f;    // Hur snabbt flaren faller

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Enkel fallande rörelse
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
    }

    // För debug/visualisering
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attractRadius);
    }
}
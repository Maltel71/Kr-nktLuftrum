using UnityEngine;

public class LevelTrigger : MonoBehaviour
{
    public LevelEnd levelEnd;
    private bool hasDebuggedOnce = false;

    private void Start()
    {
        Debug.Log("LevelTrigger started. Looking for Player tag to trigger level completion.");
        Debug.Log("LevelEnd reference: " + (levelEnd != null ? "VALID" : "MISSING"));

        Debug.Log("LevelTrigger started with size: " +
    GetComponent<BoxCollider>().size.x + "x" +
    GetComponent<BoxCollider>().size.y + "x" +
    GetComponent<BoxCollider>().size.z);

        // Hitta spelaren och kontrollera dess tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.Log("Found player with tag: " + player.tag);
        }
        else
        {
            Debug.LogError("NO PLAYER FOUND with Player tag!");
        }
    }

    private void Update()
    {
        // Hitta avståndet till spelaren
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);

            // Logga avståndet var 60:e frame för att inte spamma konsollen
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log("Distance to player: " + distance);
            }
        }
    }

    // Lägg till en OnTriggerStay också för att se om den aktiveras kontinuerligt
    private void OnTriggerStay(Collider other)
    {
        if (!hasDebuggedOnce)
        {
            Debug.Log("Something is STAYING in trigger: " + other.gameObject.name + " with tag: " + other.tag);
            hasDebuggedOnce = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something ENTERED trigger: " + other.gameObject.name + " with tag: " + other.tag);

        if (other.CompareTag("Player"))
        {
            Debug.Log("PLAYER entered trigger!");

            if (levelEnd != null)
            {
                Debug.Log("Calling CompleteLevel");
                levelEnd.CompleteLevel();
            }
            else
            {
                Debug.LogError("LevelEnd reference is missing!");
            }
        }
        else
        {
            Debug.Log("Object does not have Player tag. Its tag is: " + other.tag);
        }
    }

    // Lägg till detta för extra debugging
    private void OnDrawGizmos()
    {
        // Visualisera triggerns område i scenvyn
        Gizmos.color = Color.red;
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }
    }
}
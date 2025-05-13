using Unity.VisualScripting;
using UnityEngine;

public class LevelTrigger : MonoBehaviour
{
    public LevelEnd levelEnd;
    private bool hasTriggered = false;
    private BoxCollider triggerCollider;

    [SerializeField] private float triggerWidth = 30f;
    [SerializeField] private float triggerHeight = 20f;
    [SerializeField] private float triggerDepth = 20f;
    [SerializeField] private bool useProximityTrigger = true;
    [SerializeField] private float proximityDistance = 45f; // Ändrat till 45

    // Variabler för att spåra spelarens rörelse
    private float closestDistance = float.MaxValue;
    private float lastDistance = float.MaxValue;
    private bool isMovingAway = false;

    private void Start()
    {
        // Kontrollera och säkerställ en stor trigger
        triggerCollider = GetComponent<BoxCollider>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<BoxCollider>();
        }
        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector3(triggerWidth, triggerHeight, triggerDepth);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.Log("Found player with tag: " + player.tag);

            // Kolla efter colliders på både spelarobjektet OCH alla dess barn
            Collider[] playerColliders = player.GetComponentsInChildren<Collider>();
            if (playerColliders != null && playerColliders.Length > 0)
            {
                Debug.Log($"Player has {playerColliders.Length} colliders (including children)");
                foreach (var col in playerColliders)
                {
                    Debug.Log($"Found collider on {col.gameObject.name}: {col.GetType().Name}, IsTrigger: {col.isTrigger}");

                    // Om vi hittar en collider med isTrigger=true, ge användbar feedback
                    if (col.isTrigger)
                    {
                        Debug.LogWarning($"The collider on {col.gameObject.name} is a trigger. This might prevent trigger interactions.");
                    }
                }
            }
            else
            {
                Debug.LogError("PLAYER AND ITS CHILDREN HAVE NO COLLIDERS!");
            }
        }
    }

    private void Update()
    {
        if (hasTriggered) return;

        // Hitta avståndet till spelaren
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);

            // Spåra närmaste avstånd
            if (distance < closestDistance)
            {
                closestDistance = distance;
            }

            // Kontrollera om spelaren börjar röra sig bort efter att ha kommit nära
            if (distance < lastDistance)
            {
                // Spelaren närmar sig
                isMovingAway = false;
            }
            else if (distance > lastDistance && distance < 60f)
            {
                // Spelaren rör sig bort OCH är fortfarande relativt nära
                if (!isMovingAway)
                {
                    isMovingAway = true;
                    Debug.Log("Player has reached closest point and is now moving away!");

                    // Om spelaren har kommit tillräckligt nära och börjar röra sig bort, slutför nivån
                    if (closestDistance < 45f)
                    {
                        Debug.Log("Triggering level completion at closest approach: " + closestDistance);
                        TriggerLevelCompletion();
                    }
                }
            }

            lastDistance = distance;

            // Använd standardnärhetstriggning också
            if (useProximityTrigger && distance < proximityDistance && !hasTriggered)
            {
                Debug.Log("Proximity trigger activated! Distance: " + distance);
                TriggerLevelCompletion();
            }

            // Logga avståndet var 60:e frame för att inte spamma konsollen
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log("Distance to player: " + distance + ", Closest so far: " + closestDistance);
            }
        }
    }

    // Om något kolliderar med triggern
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something ENTERED trigger: " + other.gameObject.name + " with tag: " + other.tag);

        if (other.CompareTag("Player"))
        {
            Debug.Log("PLAYER entered trigger!");
            TriggerLevelCompletion();
        }
        else
        {
            Debug.Log("Object does not have Player tag. Its tag is: " + other.tag);
        }
    }

    // Om något stannar kvar inuti triggern
    private void OnTriggerStay(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            Debug.Log("Player STAYING in trigger - triggering completion");
            TriggerLevelCompletion();
        }
    }

    private void TriggerLevelCompletion()
    {
        if (hasTriggered) return;
        hasTriggered = true;

        Debug.Log("Triggering level completion!");

        if (levelEnd != null)
        {
            Debug.Log("Calling LevelEnd.CompleteLevel()");
            levelEnd.CompleteLevel();
        }
        else
        {
            Debug.LogError("Level End reference is missing - cannot complete level!");
        }
    }

    // Visualisera triggerzonen i editorn
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }
    }
}
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
    [SerializeField] private float proximityDistance = 45f; // �ndrat till 45

    // Variabler f�r att sp�ra spelarens r�relse
    private float closestDistance = float.MaxValue;
    private float lastDistance = float.MaxValue;
    private bool isMovingAway = false;

    private void Start()
    {
        // Kontrollera och s�kerst�ll en stor trigger
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

            // Kolla efter colliders p� b�de spelarobjektet OCH alla dess barn
            Collider[] playerColliders = player.GetComponentsInChildren<Collider>();
            if (playerColliders != null && playerColliders.Length > 0)
            {
                Debug.Log($"Player has {playerColliders.Length} colliders (including children)");
                foreach (var col in playerColliders)
                {
                    Debug.Log($"Found collider on {col.gameObject.name}: {col.GetType().Name}, IsTrigger: {col.isTrigger}");

                    // Om vi hittar en collider med isTrigger=true, ge anv�ndbar feedback
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

        // Hitta avst�ndet till spelaren
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);

            // Sp�ra n�rmaste avst�nd
            if (distance < closestDistance)
            {
                closestDistance = distance;
            }

            // Kontrollera om spelaren b�rjar r�ra sig bort efter att ha kommit n�ra
            if (distance < lastDistance)
            {
                // Spelaren n�rmar sig
                isMovingAway = false;
            }
            else if (distance > lastDistance && distance < 60f)
            {
                // Spelaren r�r sig bort OCH �r fortfarande relativt n�ra
                if (!isMovingAway)
                {
                    isMovingAway = true;
                    Debug.Log("Player has reached closest point and is now moving away!");

                    // Om spelaren har kommit tillr�ckligt n�ra och b�rjar r�ra sig bort, slutf�r niv�n
                    if (closestDistance < 45f)
                    {
                        Debug.Log("Triggering level completion at closest approach: " + closestDistance);
                        TriggerLevelCompletion();
                    }
                }
            }

            lastDistance = distance;

            // Anv�nd standardn�rhetstriggning ocks�
            if (useProximityTrigger && distance < proximityDistance && !hasTriggered)
            {
                Debug.Log("Proximity trigger activated! Distance: " + distance);
                TriggerLevelCompletion();
            }

            // Logga avst�ndet var 60:e frame f�r att inte spamma konsollen
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log("Distance to player: " + distance + ", Closest so far: " + closestDistance);
            }
        }
    }

    // Om n�got kolliderar med triggern
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

    // Om n�got stannar kvar inuti triggern
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
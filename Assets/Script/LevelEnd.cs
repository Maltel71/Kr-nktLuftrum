using UnityEngine;

public class LevelEnd : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private float triggerWidth = 10f;
    [SerializeField] private float triggerHeight = 5f;
    [SerializeField] private bool showDebugGizmo = true;

    [Header("Level Completion")]
    [SerializeField] private GameObject completionEffect;

    private bool levelCompleted = false;

    private void Start()
    {
        // Konfigurera trigger
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }

        boxCollider.isTrigger = true;
        boxCollider.size = new Vector3(triggerWidth, triggerHeight, 1f);

        Debug.Log("LevelEnd initialized. Waiting for player to reach end of level.");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Something entered level end trigger: {other.gameObject.name} with tag: {other.tag}");

        if (other.CompareTag("Player") && !levelCompleted)
        {
            Debug.Log("Player reached end of level!");
            CompleteLevel();
        }
    }

    // Detta anropas när spelaren når slutet av nivån
    public void CompleteLevel()
    {
        if (levelCompleted) return;

        levelCompleted = true;
        Debug.Log("Level completed!");

        // Visa effekt/animation för nivåslut
        if (completionEffect != null)
        {
            Instantiate(completionEffect, transform.position, Quaternion.identity);
        }

        // Anropa LevelManager för att gå till nästa nivå
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.CompleteLevel();
        }
        else
        {
            Debug.LogError("LevelManager.Instance is null! Cannot complete level.");
        }
    }

    // Rita en visuell representation av triggern i editorn
    private void OnDrawGizmos()
    {
        if (showDebugGizmo)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(triggerWidth, triggerHeight, 1f));
            Gizmos.DrawLine(transform.position, transform.position + Vector3.forward * 10f);
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
    }
}
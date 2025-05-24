using UnityEngine;

public class LevelEnd : MonoBehaviour
{
    [Header("Level Completion")]
    [SerializeField] private GameObject completionEffect;

    private bool levelCompleted = false;

    private void Start()
    {
        Debug.Log("LevelEnd initialized. Waiting for player to reach end of level.");
    }

    // Detta anropas från LevelTrigger när spelaren når slutet av nivån
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

            // Hitta eller skapa LevelManager om den saknas
            Debug.LogWarning("No LevelManager found - creating one for testing");
            GameObject lmObj = new GameObject("LevelManager");
            lmObj.AddComponent<LevelManager>();
            LevelManager.Instance.CompleteLevel();
        }
    }
}
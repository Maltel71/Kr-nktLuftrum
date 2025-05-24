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

    // Detta anropas fr�n LevelTrigger n�r spelaren n�r slutet av niv�n
    public void CompleteLevel()
    {
        if (levelCompleted) return;
        levelCompleted = true;
        Debug.Log("Level completed!");

        // Visa effekt/animation f�r niv�slut
        if (completionEffect != null)
        {
            Instantiate(completionEffect, transform.position, Quaternion.identity);
        }

        // Anropa LevelManager f�r att g� till n�sta niv�
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
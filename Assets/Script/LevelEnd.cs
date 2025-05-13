using UnityEngine;

public class LevelEnd : MonoBehaviour
{
    [Header("Level Completion")]
    [SerializeField] private GameObject completionEffect;
    [SerializeField] private float delayBeforeNextLevel = 3f;

    private bool levelCompleted = false;

    // Detta anropas när spelaren nått slutet av nivån
    public void CompleteLevel()
    {
        if (levelCompleted) return;

        levelCompleted = true;

        // Visa effekt/animation för nivåslut
        if (completionEffect != null)
        {
            Instantiate(completionEffect, transform.position, Quaternion.identity);
        }

        // Visa meddelande till spelaren via ditt befintliga system
        GameMessageSystem messageSystem = FindObjectOfType<GameMessageSystem>();
        if (messageSystem != null)
        {
            messageSystem.ShowBoostMessage("LEVEL COMPLETED!");
        }

        // Gå till nästa nivå efter fördröjning
        Invoke("GoToNextLevel", delayBeforeNextLevel);
    }

    private void GoToNextLevel()
    {
        // Använd LevelManager för att gå till nästa nivå
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.StartNextLevel();
        }
    }
}
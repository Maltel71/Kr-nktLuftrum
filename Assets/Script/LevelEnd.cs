using UnityEngine;

public class LevelEnd : MonoBehaviour
{
    [Header("Level Completion")]
    [SerializeField] private GameObject completionEffect;
    [SerializeField] private float delayBeforeNextLevel = 3f;

    private bool levelCompleted = false;

    // Detta anropas n�r spelaren n�tt slutet av niv�n
    public void CompleteLevel()
    {
        if (levelCompleted) return;

        levelCompleted = true;

        // Visa effekt/animation f�r niv�slut
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

        // G� till n�sta niv� efter f�rdr�jning
        Invoke("GoToNextLevel", delayBeforeNextLevel);
    }

    private void GoToNextLevel()
    {
        // Anv�nd LevelManager f�r att g� till n�sta niv�
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.StartNextLevel();
        }
    }
}
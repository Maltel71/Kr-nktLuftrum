using UnityEngine;
using UnityEngine.SceneManagement;

public class Startmenu : MonoBehaviour
{
    /// <summary>
    /// Anropas när spelaren trycker "Play" - startar tutorial (Level0)
    /// </summary>
    public void Play()
    {
        Debug.Log("Play button pressed - starting new game");

        if (LevelManager.Instance != null)
        {
            // Använd LevelManager för att starta nytt spel (går till tutorial)
            LevelManager.Instance.StartNewGame();
        }
        else
        {
            // Fallback: Ladda Level0 direkt
            Debug.LogWarning("LevelManager not found - loading Level0 directly");
            LoadSceneByName("Level0");
        }
    }

    /// <summary>
    /// Avsluta spelet
    /// </summary>
    public void Quit()
    {
        Debug.Log("Player has left the game");
        Application.Quit();

        // För testning i Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    /// <summary>
    /// Extra metod för att hoppa direkt till en viss level (för testning)
    /// </summary>
    public void PlayLevel(int levelNumber)
    {
        Debug.Log($"Play Level {levelNumber} button pressed");

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.currentLevel = levelNumber;
            LevelManager.Instance.StartNextLevel();
        }
        else
        {
            LoadSceneByName($"Level{levelNumber}");
        }
    }

    /// <summary>
    /// Fallback metod för att ladda scener
    /// </summary>
    private void LoadSceneByName(string sceneName)
    {
        string[] possibleNames = {
            sceneName,
            $"Scenes/{sceneName}",
            $"{sceneName}_Scene"
        };

        foreach (string name in possibleNames)
        {
            try
            {
                if (Application.CanStreamedLevelBeLoaded(name))
                {
                    SceneManager.LoadScene(name);
                    return;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not load {name}: {e.Message}");
            }
        }

        // Om ingen scen hittades, gå till nästa i build order
        Debug.LogError($"Could not find scene {sceneName} - loading next scene in build");
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
    }

    /// <summary>
    /// För att öppna inställningar (om du vill lägga till det senare)
    /// </summary>
    public void OpenSettings()
    {
        Debug.Log("Settings button pressed");
        // Implementera senare om du vill ha inställningsmeny
    }

    /// <summary>
    /// För att visa credits (om du vill lägga till det senare)
    /// </summary>
    public void ShowCredits()
    {
        Debug.Log("Credits button pressed");
        // Implementera senare om du vill ha credits
    }
}
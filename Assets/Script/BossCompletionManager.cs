using UnityEngine;
using System.Collections;

public class BossCompletionManager : MonoBehaviour
{
    private static BossCompletionManager instance;
    public static BossCompletionManager Instance => instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void TriggerBossDefeated()
    {
        if (Instance != null)
        {
            Instance.StartCoroutine(Instance.CompleteLevelAfterDelay(3f));
        }
        else
        {
            Debug.LogError("BossCompletionManager instance not found!");
        }
    }

    private IEnumerator CompleteLevelAfterDelay(float delay)
    {
        Debug.Log("=== BOSS DEATH SEQUENCE STARTED ===");

        GameMessageSystem messageSystem = FindObjectOfType<GameMessageSystem>();
        if (messageSystem != null)
        {
            messageSystem.ShowBoostMessage("BOSS DEFEATED!");
            Debug.Log("Boss defeated message shown");
        }

        Debug.Log($"Waiting {delay} seconds before completing level...");
        yield return new WaitForSeconds(delay);

        Debug.Log("Delay completed! Now checking for LevelManager...");

        if (LevelManager.Instance != null)
        {
            Debug.Log($"LevelManager found! Current level: {LevelManager.Instance.currentLevel}");
            Debug.Log("Calling LevelManager.CompleteLevel()...");

            try
            {
                LevelManager.Instance.CompleteLevel();
                Debug.Log("LevelManager.CompleteLevel() called successfully!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error calling CompleteLevel: {e.Message}");
                StartFallbackLevelCompletion();
            }
        }
        else
        {
            Debug.LogError("=== LevelManager.Instance är NULL! ===");
            StartFallbackLevelCompletion();
        }

        Debug.Log("=== BOSS DEATH SEQUENCE COMPLETED ===");
    }

    private void StartFallbackLevelCompletion()
    {
        Debug.LogWarning("FALLBACK: Laddar LoadingScreen direkt...");

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.StopGame();
        }

        try
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("LoadingScreen");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Couldn't load LoadingScreen: {e.Message}");

            // Last resort - nästa scen
            int currentIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            int nextIndex = currentIndex + 1;

            if (nextIndex < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextIndex);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private static LevelManager instance;
    public static LevelManager Instance => instance;

    [Header("Level Settings")]
    public int currentLevel = -1; // -1 = StartMenu, 0 = Tutorial, 1+ = Game levels
    public int maxGameLevel = 4; // Level1, Level2, Level3, Level4

    [Header("Scene Names - VIKTIGT: Ange rätt scennamn")]
    [SerializeField] private string startMenuScene = "MainScene";
    [SerializeField] private string tutorialScene = "Level0";
    [SerializeField] private string loadingScene = "LoadingScreen";
    [SerializeField] private string endScene = "EndScene";

    [Header("Level Transition")]
    //[SerializeField] private float levelTransitionDelay = 2f;
    [SerializeField] private GameObject levelCompleteUI;

    // Spara spelardata mellan nivåer
    private float playerHealth;
    private int playerScore;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Sätt currentLevel baserat på nuvarande scen
            DetermineCurrentLevel();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void DetermineCurrentLevel()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.Contains("MainScene") || sceneName.Contains("StartMenu"))
            currentLevel = -1;
        else if (sceneName.Contains("Level0"))
            currentLevel = 0;
        else if (sceneName.Contains("Level1"))
            currentLevel = 1;
        else if (sceneName.Contains("Level2"))
            currentLevel = 2;
        else if (sceneName.Contains("Level3"))
            currentLevel = 3;
        else if (sceneName.Contains("Level4"))
            currentLevel = 4;
        else if (sceneName.Contains("Loading"))
            currentLevel = currentLevel; // Behåll nuvarande level

        Debug.Log($"LevelManager: Determined current level as {currentLevel} from scene {sceneName}");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");

        // Återställ spelardata när vi laddar gameplay-scener
        if (scene.name.Contains("Level") && !scene.name.Contains("Loading"))
        {
            RestorePlayerData();
            isTransitioning = false;
        }
    }

    // ===== PUBLIKA METODER FÖR ATT STARTA SPELET =====

    /// <summary>
    /// Anropas från StartMenu när spelaren trycker "Play"
    /// </summary>
    public void StartNewGame()
    {
        Debug.Log("Starting new game - going to tutorial");
        currentLevel = 0; // Tutorial
        LoadScene(tutorialScene);
        ResetPlayerData();
    }

    /// <summary>
    /// Anropas när en level är klar
    /// </summary>
    public void CompleteLevel()
    {
        Debug.Log("=== LevelManager.CompleteLevel() CALLED ===");
        Debug.Log($"isTransitioning: {isTransitioning}");
        Debug.Log($"currentLevel: {currentLevel}");

        if (isTransitioning)
        {
            Debug.LogWarning("Already transitioning - returning early");
            return;
        }

        isTransitioning = true;

        Debug.Log($"Level {currentLevel} completed!");
        SavePlayerData();

        // Resten av din befintliga CompleteLevel() kod...
        if (currentLevel == 0) // Tutorial klar
        {
            Debug.Log("Tutorial complete - going to Level 1");
            currentLevel = 1;
            LoadScene("Level1");
        }
        else if (currentLevel == 1) // Level1 klar → LoadingScreen
        {
            Debug.Log("Level 1 complete - going to loading screen");
            currentLevel = 2;
            LoadScene(loadingScene);
        }

        if (isTransitioning) return;
        isTransitioning = true;

        Debug.Log($"Level {currentLevel} completed!");
        SavePlayerData();

        // Bestäm vad som händer härnäst
        if (currentLevel == 0) // Tutorial klar
        {
            Debug.Log("Tutorial complete - going to Level 1");
            currentLevel = 1;
            LoadScene("Level1");
        }
        else if (currentLevel == 1) // Level1 klar → LoadingScreen
        {
            Debug.Log("Level 1 complete - going to loading screen");
            currentLevel = 2; // Sätt nästa level
            LoadScene(loadingScene);
        }
        else if (currentLevel == 2) // Level2 klar → LoadingScreen  
        {
            Debug.Log("Level 2 complete - going to loading screen");
            currentLevel = 3;
            LoadScene(loadingScene);
        }
        else if (currentLevel == 3) // Level3 klar → LoadingScreen
        {
            Debug.Log("Level 3 complete - going to loading screen");
            currentLevel = 4;
            LoadScene(loadingScene);
        }
        else if (currentLevel == 4) // Level4 klar → EndScene
        {
            Debug.Log("Level 4 complete - going to end scene");
            LoadScene(endScene);
        }
        else if (currentLevel >= maxGameLevel) // Säkerhet
        {
            Debug.Log("All levels complete - going to end scene");
            LoadScene(endScene);
        }
    }

    /// <summary>
    /// Anropas från LoadingScreen när spelaren trycker continue
    /// </summary>
    public void StartNextLevel()
    {
        Debug.Log($"Starting Level {currentLevel}");
        LoadScene($"Level{currentLevel}");
    }

    /// <summary>
    /// Starta om nuvarande level
    /// </summary>
    public void RestartLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    /// <summary>
    /// Gå tillbaka till huvudmenyn
    /// </summary>
    public void ReturnToMainMenu()
    {
        currentLevel = -1;
        LoadScene(startMenuScene);
    }

    // ===== PRIVATA METODER =====

    private void LoadScene(string sceneName)
    {
        Debug.Log($"LevelManager: Attempting to load scene: {sceneName}");

        // Kontrollera om scenen finns
        if (SceneExists(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' does not exist! Check Build Settings.");

            // Fallback baserat på vad som förväntades
            if (sceneName.Contains("Level"))
            {
                // Försök ladda nästa scen i build settings
                int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
                if (nextIndex < SceneManager.sceneCountInBuildSettings)
                {
                    SceneManager.LoadScene(nextIndex);
                }
            }
            else
            {
                // Gå tillbaka till första scenen
                SceneManager.LoadScene(0);
            }
        }
    }

    private bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (sceneNameFromPath == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    private void SavePlayerData()
    {
        PlaneHealthSystem player = FindObjectOfType<PlaneHealthSystem>();
        if (player != null)
        {
            playerHealth = player.GetHealthPercentage() * 100f;
            Debug.Log($"Saved player health: {playerHealth}%");
        }

        if (ScoreManager.Instance != null)
        {
            playerScore = ScoreManager.Instance.GetCurrentScore();
            Debug.Log($"Saved player score: {playerScore}");
        }
    }

    private void RestorePlayerData()
    {
        PlaneHealthSystem player = FindObjectOfType<PlaneHealthSystem>();
        if (player != null)
        {
            // Ge lite extra hälsa för nya nivåer
            float healthToRestore = Mathf.Max(playerHealth, 50f);
            player.ApplyShieldBoost(healthToRestore);
            Debug.Log($"Restored player with health bonus");
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SetScore(playerScore);
            Debug.Log($"Restored player score: {playerScore}");
        }
    }

    private void ResetPlayerData()
    {
        playerHealth = 100f;
        playerScore = 0;
    }

    // ===== DEBUG METODER =====
    [ContextMenu("Test Complete Level")]
    public void TestCompleteLevel()
    {
        CompleteLevel();
    }

    [ContextMenu("Test Start New Game")]
    public void TestStartNewGame()
    {
        StartNewGame();
    }

    // ===== GETTERS =====
    public int GetCurrentLevel() => currentLevel;
    public bool IsInTutorial() => currentLevel == 0;
    public bool IsInGameLevel() => currentLevel >= 1 && currentLevel <= maxGameLevel;
}
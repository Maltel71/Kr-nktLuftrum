using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private static LevelManager instance;
    public static LevelManager Instance => instance;

    [Header("Level Settings")]
    public int currentLevel = 0;
    public int maxLevel = 2; // 0-baserat, så 0, 1, 2 = tre nivåer

    [Header("Level Data")]
    public float[] playerStartingHealth = { 100f, 100f, 100f };
    public int[] enemyDifficulty = { 1, 2, 3 }; // Svårighetsgrad per nivå

    [Header("Level Transition")]
    [SerializeField] private float levelTransitionDelay = 2f; // Tid innan nästa nivå laddas
    [SerializeField] private GameObject levelCompleteUI; // UI som visas när nivån är klar

    // Spara spelardata mellan nivåer
    private float playerHealth;
    private int playerScore;
    private int collectedPowerups;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Lyssna på när en scen har laddats klart
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Viktigt för att undvika minneslöckor
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Anropas när en ny scen har laddats
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Kontrollera om det är en spelnivå
        if (scene.name.StartsWith("Level"))
        {
            Debug.Log($"Level {currentLevel} loaded, restoring player data");
            RestorePlayerData();
            isTransitioning = false;
        }
    }

    // Denna metod anropas från LevelEnd-skriptet när spelaren når slutet av en nivå
    public void CompleteLevel()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        Debug.Log($"Level {currentLevel} completed!");

        // Visa level complete UI om det finns
        if (levelCompleteUI != null)
        {
            levelCompleteUI.SetActive(true);
        }

        // Meddela spelaren
        GameMessageSystem messageSystem = FindObjectOfType<GameMessageSystem>();
        if (messageSystem != null)
        {
            messageSystem.ShowBoostMessage("LEVEL COMPLETE!");
        }

        // Spara spelardata
        SavePlayerData();

        // Starta nästa nivå efter en kort fördröjning
        Invoke("StartNextLevel", levelTransitionDelay);
    }

    public void StartNextLevel()
    {
        // Gå till nästa nivå
        currentLevel++;

        if (currentLevel > maxLevel)
        {
            Debug.Log("All levels completed! Loading end scene.");
            // Sista nivån klar, gå till slutscen
            SceneManager.LoadScene("EndScene");
        }
        else
        {
            Debug.Log($"Loading next level: Level{currentLevel}");
            // Ladda nästa nivå
            SceneManager.LoadScene("Level" + currentLevel);
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void SavePlayerData()
    {
        // Hitta spelaren och spara dess data
        PlaneHealthSystem player = FindObjectOfType<PlaneHealthSystem>();
        if (player != null)
        {
            playerHealth = player.GetHealthPercentage() * 100f;
            Debug.Log($"Saved player health: {playerHealth}%");
        }

        // Spara score
        if (ScoreManager.Instance != null)
        {
            playerScore = ScoreManager.Instance.GetCurrentScore();
            Debug.Log($"Saved player score: {playerScore}");
        }

        // Spara powerups (detta behöver implementeras i ett PowerupManager-skript)
        // collectedPowerups = PowerupManager.Instance.GetCollectedCount();
    }

    public void RestorePlayerData()
    {
        // Återställ spelardata i nya nivån
        PlaneHealthSystem player = FindObjectOfType<PlaneHealthSystem>();
        if (player != null)
        {
            // Anpassa om du behöver sätta exakt värde
            // Sätt hälsan baserad på sparad hälsa (med ett minimum)
            float healthToSet = Mathf.Max(playerHealth, 50f);

            // Detta kräver en ny metod i PlaneHealthSystem för att sätta hälsan direkt
            // player.SetHealth(healthToSet);

            // Ge lite skydd vid ny nivå
            player.ApplyShieldBoost(50f);

            Debug.Log($"Restored player health and added shield boost");
        }

        // Återställ score
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SetScore(playerScore);
            Debug.Log($"Restored player score: {playerScore}");
        }
    }

    // Debug-metod för att testa nivåövergång
    [ContextMenu("Test Complete Level")]
    public void TestCompleteLevel()
    {
        CompleteLevel();
    }
}
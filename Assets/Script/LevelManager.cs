using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private static LevelManager instance;
    public static LevelManager Instance => instance;

    [Header("Level Settings")]
    public int currentLevel = 0;
    public int maxLevel = 2; // 0-baserat, s� 0, 1, 2 = tre niv�er

    [Header("Level Data")]
    public float[] playerStartingHealth = { 100f, 100f, 100f };
    public int[] enemyDifficulty = { 1, 2, 3 }; // Sv�righetsgrad per niv�

    [Header("Level Transition")]
    [SerializeField] private float levelTransitionDelay = 2f; // Tid innan n�sta niv� laddas
    [SerializeField] private GameObject levelCompleteUI; // UI som visas n�r niv�n �r klar

    // Spara spelardata mellan niv�er
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

            // Lyssna p� n�r en scen har laddats klart
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Viktigt f�r att undvika minnesl�ckor
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Anropas n�r en ny scen har laddats
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Kontrollera om det �r en spelniv�
        if (scene.name.StartsWith("Level"))
        {
            Debug.Log($"Level {currentLevel} loaded, restoring player data");
            RestorePlayerData();
            isTransitioning = false;
        }
    }

    // Denna metod anropas fr�n LevelEnd-skriptet n�r spelaren n�r slutet av en niv�
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

        // Starta n�sta niv� efter en kort f�rdr�jning
        Invoke("StartNextLevel", levelTransitionDelay);
    }

    public void StartNextLevel()
    {
        // G� till n�sta niv�
        currentLevel++;

        if (currentLevel > maxLevel)
        {
            Debug.Log("All levels completed! Loading end scene.");
            // Sista niv�n klar, g� till slutscen
            SceneManager.LoadScene("EndScene");
        }
        else
        {
            Debug.Log($"Loading next level: Level{currentLevel}");
            // Ladda n�sta niv�
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

        // Spara powerups (detta beh�ver implementeras i ett PowerupManager-skript)
        // collectedPowerups = PowerupManager.Instance.GetCollectedCount();
    }

    public void RestorePlayerData()
    {
        // �terst�ll spelardata i nya niv�n
        PlaneHealthSystem player = FindObjectOfType<PlaneHealthSystem>();
        if (player != null)
        {
            // Anpassa om du beh�ver s�tta exakt v�rde
            // S�tt h�lsan baserad p� sparad h�lsa (med ett minimum)
            float healthToSet = Mathf.Max(playerHealth, 50f);

            // Detta kr�ver en ny metod i PlaneHealthSystem f�r att s�tta h�lsan direkt
            // player.SetHealth(healthToSet);

            // Ge lite skydd vid ny niv�
            player.ApplyShieldBoost(50f);

            Debug.Log($"Restored player health and added shield boost");
        }

        // �terst�ll score
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SetScore(playerScore);
            Debug.Log($"Restored player score: {playerScore}");
        }
    }

    // Debug-metod f�r att testa niv��verg�ng
    [ContextMenu("Test Complete Level")]
    public void TestCompleteLevel()
    {
        CompleteLevel();
    }
}
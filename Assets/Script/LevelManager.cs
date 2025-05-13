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

    // Spara spelardata mellan nivåer
    private float playerHealth;
    private int playerScore;
    private int collectedPowerups;

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

    public void StartNextLevel()
    {
        // Spara spelardata
        SavePlayerData();

        // Gå till nästa nivå
        currentLevel++;
        if (currentLevel > maxLevel)
        {
            // Sista nivån klar, gå till slutscen
            SceneManager.LoadScene("EndScene");
        }
        else
        {
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
        }

        // Spara score
        if (ScoreManager.Instance != null)
        {
            playerScore = ScoreManager.Instance.GetCurrentScore();
        }
    }

    public void RestorePlayerData()
    {
        // Återställ spelardata i nya nivån
        PlaneHealthSystem player = FindObjectOfType<PlaneHealthSystem>();
        if (player != null)
        {
            // Anpassa om du behöver sätta exakt värde
            player.ApplyShieldBoost(50f); // Ge lite skydd vid ny nivå
        }

        // Återställ score
        if (ScoreManager.Instance != null)
        {
            // Ändra i ScoreManager så du kan sätta poäng, inte bara nollställa
            ScoreManager.Instance.SetScore(playerScore);
        }
    }
}
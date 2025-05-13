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

    // Spara spelardata mellan niv�er
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

        // G� till n�sta niv�
        currentLevel++;
        if (currentLevel > maxLevel)
        {
            // Sista niv�n klar, g� till slutscen
            SceneManager.LoadScene("EndScene");
        }
        else
        {
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
        }

        // Spara score
        if (ScoreManager.Instance != null)
        {
            playerScore = ScoreManager.Instance.GetCurrentScore();
        }
    }

    public void RestorePlayerData()
    {
        // �terst�ll spelardata i nya niv�n
        PlaneHealthSystem player = FindObjectOfType<PlaneHealthSystem>();
        if (player != null)
        {
            // Anpassa om du beh�ver s�tta exakt v�rde
            player.ApplyShieldBoost(50f); // Ge lite skydd vid ny niv�
        }

        // �terst�ll score
        if (ScoreManager.Instance != null)
        {
            // �ndra i ScoreManager s� du kan s�tta po�ng, inte bara nollst�lla
            ScoreManager.Instance.SetScore(playerScore);
        }
    }
}
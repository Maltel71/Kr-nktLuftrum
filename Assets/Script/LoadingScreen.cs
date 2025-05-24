using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI Elements - DRA FR�N SCENEN")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI levelDescriptionText;
    [SerializeField] private TextMeshProUGUI enemyCountText;
    [SerializeField] private TextMeshProUGUI missionsText;
    [SerializeField] private TextMeshProUGUI continueInstructionText;
    [SerializeField] private Image levelPreviewImage;
    [SerializeField] private Button continueButton;
    [SerializeField] private Slider loadingProgressBar;

    [Header("Level Descriptions")]
    [SerializeField]
    private LevelDescription[] levelDescriptions = new LevelDescription[]
    {
        new LevelDescription
        {
            levelName = "BASIC TRAINING",
            description = "Learn the controls and basic combat maneuvers.",
            expectedEnemyCount = 5,
            missions = new string[] { "Complete weapon training", "Destroy practice targets", "Master movement controls" }
        },
        new LevelDescription
        {
            levelName = "CITY ASSAULT",
            description = "Defend the city from enemy aircraft and ground forces.",
            expectedEnemyCount = 15,
            missions = new string[] { "Destroy enemy aircraft", "Eliminate missile launchers", "Defeat the boss" }
        },
        new LevelDescription
        {
            levelName = "DESERT STORM",
            description = "Survive the desert helicopter assault and destroy ground vehicles.",
            expectedEnemyCount = 20,
            missions = new string[] { "Destroy helicopters", "Eliminate ground forces", "Survive the sandstorm" }
        },
        new LevelDescription
        {
            levelName = "NAVAL COMBAT",
            description = "Sink the enemy fleet and destroy their aircraft carrier.",
            expectedEnemyCount = 25,
            missions = new string[] { "Sink enemy ships", "Destroy naval aircraft", "Defeat the carrier boss" }
        }
    };

    [System.Serializable]
    public class LevelDescription
    {
        public string levelName = "Unknown Level";
        [TextArea(2, 4)]
        public string description = "No description available.";
        public int expectedEnemyCount = 0;
        public string[] missions = new string[] { "Complete the level" };
        public Sprite previewImage;
        public Color backgroundColor = Color.black;
    }

    [Header("Loading Settings")]
    [SerializeField] private float minLoadingTime = 2f;
    [SerializeField] private float maxLoadingTime = 4f;

    private void Awake()
    {
        // S�kerst�ll att panelen �r dold fr�n start om den inte redan �r det
        if (loadingPanel != null)
            loadingPanel.SetActive(true);
    }

    private void Start()
    {
        // Starta automatiskt om LevelManager finns
        if (LevelManager.Instance != null)
        {
            ShowLoadingScreen(LevelManager.Instance.currentLevel);
        }
        else
        {
            // Fallback - f�rs�k gissa n�sta level
            int nextLevel = 1; // Standard till Level 1
            ShowLoadingScreen(nextLevel);
        }

        // L�gg till lyssnare f�r continue-knappen om den finns
        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueToNextLevel);
    }

    private void Update()
    {
        // Till�t Enter-tangenten att forts�tta till n�sta niv�
        if (loadingPanel != null && loadingPanel.activeSelf &&
            loadingProgressBar != null && loadingProgressBar.value >= 1f &&
            (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            ContinueToNextLevel();
        }
    }

    public void ShowLoadingScreen(int currentLevel)
    {
        Debug.Log($"LoadingScreen: Showing screen for level {currentLevel}");

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        // Uppdatera po�ng
        UpdateScoreDisplay();

        // Uppdatera niv�beskrivning
        UpdateLevelDescription(currentLevel);

        // D�lj continue-instruktion fr�n start
        if (continueInstructionText != null)
            continueInstructionText.gameObject.SetActive(false);

        // Starta progressbar
        StartLoadingProgress();
    }

    private void StartLoadingProgress()
    {
        if (loadingProgressBar != null)
        {
            StartCoroutine(SimulateLoadingProgress());
        }
        else
        {
            // Om ingen progress bar finns, v�nta bara en kort stund
            StartCoroutine(DelayThenContinue());
        }
    }

    private IEnumerator DelayThenContinue()
    {
        yield return new WaitForSeconds(2f);
        ContinueToNextLevel();
    }

    private IEnumerator SimulateLoadingProgress()
    {
        float loadTime = Random.Range(minLoadingTime, maxLoadingTime);
        float elapsedTime = 0f;

        while (elapsedTime < loadTime)
        {
            elapsedTime += Time.deltaTime;
            if (loadingProgressBar != null)
                loadingProgressBar.value = elapsedTime / loadTime;
            yield return null;
        }

        if (loadingProgressBar != null)
            loadingProgressBar.value = 1f;

        // Visa Continue-instruktion n�r laddningen �r klar
        if (continueInstructionText != null)
        {
            continueInstructionText.gameObject.SetActive(true);
            continueInstructionText.text = "Tryck ENTER f�r att forts�tta";
            StartCoroutine(BlinkContinueText());
        }
        else
        {
            // Om ingen continue text finns, g� automatiskt vidare
            yield return new WaitForSeconds(1f);
            ContinueToNextLevel();
        }
    }

    private IEnumerator BlinkContinueText()
    {
        if (continueInstructionText == null) yield break;

        while (loadingPanel != null && loadingPanel.activeSelf &&
               loadingProgressBar != null && loadingProgressBar.value >= 1f)
        {
            continueInstructionText.enabled = !continueInstructionText.enabled;
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null && ScoreManager.Instance != null)
        {
            int currentScore = ScoreManager.Instance.GetCurrentScore();
            scoreText.text = $"Current Score: {currentScore}";
        }

        if (highScoreText != null)
        {
            // H�mta h�gsta po�ngen fr�n PlayerPrefs
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = $"High Score: {highScore}";
        }
    }

    private void UpdateLevelDescription(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levelDescriptions.Length)
        {
            Debug.LogWarning($"No description found for level {levelIndex}");

            // Fallback beskrivning
            if (levelDescriptionText != null)
                levelDescriptionText.text = $"LEVEL {levelIndex}\nPrepare for combat!";

            return;
        }

        LevelDescription currentLevel = levelDescriptions[levelIndex];

        // Uppdatera bakgrundsf�rg om m�jligt
        if (loadingPanel != null)
        {
            Image panelBackground = loadingPanel.GetComponent<Image>();
            if (panelBackground != null)
            {
                panelBackground.color = currentLevel.backgroundColor;
            }
        }

        // Uppdatera niv�namn och beskrivning
        if (levelDescriptionText != null)
        {
            levelDescriptionText.text = $"{currentLevel.levelName}\n{currentLevel.description}";
        }

        // Uppdatera fiendeantal
        if (enemyCountText != null)
        {
            enemyCountText.text = $"Expected Enemies: {currentLevel.expectedEnemyCount}";
        }

        // Uppdatera uppdrag
        if (missionsText != null)
        {
            string missionsString = "Missions:\n";
            foreach (string mission in currentLevel.missions)
            {
                missionsString += $"� {mission}\n";
            }
            missionsText.text = missionsString;
        }

        // Uppdatera f�rhandsvisningsbild
        if (levelPreviewImage != null && currentLevel.previewImage != null)
        {
            levelPreviewImage.sprite = currentLevel.previewImage;
        }
    }

    private void ContinueToNextLevel()
    {
        Debug.Log("LoadingScreen: ContinueToNextLevel called");

        // D�lj laddningssk�rmen
        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        // Ladda n�sta niv� via LevelManager om m�jligt
        if (LevelManager.Instance != null)
        {
            int nextLevel = LevelManager.Instance.currentLevel;
            LoadLevel(nextLevel);
        }
        else
        {
            // Fallback: Ladda Level1
            LoadLevel(1);
        }
    }

    private void LoadLevel(int levelIndex)
    {
        string[] possibleLevelNames = {
            $"Level{levelIndex}",
            $"Level {levelIndex}",
            $"Scenes/Level{levelIndex}",
            $"Level_{levelIndex}"
        };

        foreach (string levelName in possibleLevelNames)
        {
            try
            {
                if (Application.CanStreamedLevelBeLoaded(levelName))
                {
                    Debug.Log($"LoadingScreen: Loading {levelName}");
                    SceneManager.LoadScene(levelName);
                    return;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"LoadingScreen: Failed to load {levelName}: {e.Message}");
            }
        }

        // Om ingen niv� hittades
        Debug.LogError($"LoadingScreen: Could not find Level {levelIndex}!");

        // Sista fallback: g� till n�sta scen i build order
        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentBuildIndex + 1 < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(currentBuildIndex + 1);
        }
        else
        {
            // Om vi �r p� sista scenen, g� till f�rsta
            SceneManager.LoadScene(0);
        }
    }

    // Metod f�r att visa sk�rmen manuellt fr�n andra skript
    public void Show(int currentLevel)
    {
        ShowLoadingScreen(currentLevel);
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI Elements - DRA FRÅN SCENEN")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI highScoreHolderText;
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
        new LevelDescription // Level 0 - Tutorial
        {
            levelName = "BASIC TRAINING",
            description = "Learn the controls and basic combat maneuvers.",
            briefing = "Your first mission, pilot! Master all controls and destroy training targets. Show that you're ready for real combat.",
            expectedEnemyCount = 5,
            missions = new string[] { "Complete weapon training", "Destroy all training targets", "Master the controls" }
        },
        new LevelDescription // Level 1 - City Assault  
        {
            levelName = "CITY ASSAULT",
            description = "Defend the city from enemy aircraft and ground forces.",
            briefing = "Enemy forces are attacking the city! Destroy all hostile aircraft and eliminate missile launchers. A boss aircraft awaits at the end - be ready!",
            expectedEnemyCount = 15,
            missions = new string[] { "Destroy enemy aircraft", "Eliminate missile launchers", "Defeat the boss plane" }
        },
        new LevelDescription // Level 2 - Desert Storm
        {
            levelName = "DESERT STORM",
            description = "Survive the desert assault and destroy ground vehicles.",
            briefing = "You're flying over enemy territory in the desert. Helicopters and armored vehicles await. Sandstorms may limit visibility - fly carefully!",
            expectedEnemyCount = 20,
            missions = new string[] { "Destroy helicopters", "Eliminate ground vehicles", "Survive the sandstorm" }
        },
        new LevelDescription // Level 3 - Naval Combat
        {
            levelName = "NAVAL COMBAT",
            description = "Sink the enemy fleet and destroy their aircraft carrier.",
            briefing = "The enemy fleet is blocking our supply routes! Sink all enemy ships and destroy their aircraft carrier. Kamikaze pilots will defend it to the death!",
            expectedEnemyCount = 25,
            missions = new string[] { "Sink enemy ships", "Destroy naval aircraft", "Defeat the carrier boss" }
        },
        new LevelDescription // Level 4 - Night Bombing
        {
            levelName = "NIGHT BOMBING RUN",
            description = "Night bombing of enemy targets under dark conditions.",
            briefing = "Dark clouds cover the moon. You must bomb strategic targets in total darkness. Spotlights are searching for you - avoid them or destroy them with bombs!",
            expectedEnemyCount = 18,
            missions = new string[] { "Bomb all primary targets", "Avoid spotlights", "Destroy air defenses" }
        }
    };

    [System.Serializable]
    public class LevelDescription
    {
        public string levelName = "Unknown Level";
        [TextArea(2, 4)]
        public string description = "No description available.";
        [TextArea(3, 5)]
        public string briefing = "Ingen briefing tillgänglig.";
        public int expectedEnemyCount = 0;
        public string[] missions = new string[] { "Complete the level" };
        public Sprite previewImage;
        public Color backgroundColor = Color.black;
    }

    [Header("Loading Settings")]
    [SerializeField] private float minLoadingTime = 2f;
    [SerializeField] private float maxLoadingTime = 4f;

    private bool canContinue = false;

    private void Awake()
    {
        // VIKTIGT: Stoppa poängräkning så fort LoadingScreen laddas
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.PauseSurvivalScoring();
            Debug.Log("[LoadingScreen] Pausade poängräkning i Awake()");
        }

        if (loadingPanel != null)
            loadingPanel.SetActive(true);
    }

    private void Start()
    {
        Debug.Log("LoadingScreen Start() called");

        // EXTRA säkerhet: Stoppa poängräkning igen
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.PauseSurvivalScoring();
            Debug.Log("[LoadingScreen] Pausade poängräkning i Start()");
        }

        if (LevelManager.Instance != null)
        {
            Debug.Log($"LevelManager found, current level: {LevelManager.Instance.currentLevel}");
            ShowLoadingScreen(LevelManager.Instance.currentLevel);
        }
        else
        {
            int nextLevel = 1;
            Debug.Log($"No LevelManager found, using fallback level: {nextLevel}");
            ShowLoadingScreen(nextLevel);
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueToNextLevel);
            Debug.Log("Continue button listener added");
        }

        // TEMP: Tvinga fram completion efter 5 sekunder för testning
        StartCoroutine(ForceCompleteAfterDelay(5f));
    }

    // FÖRBÄTTRAD Update() metod med ALL input-hantering
    private void Update()
    {
        // Kontrollera input för att fortsätta till nästa level
        if (canContinue && loadingPanel != null && loadingPanel.activeSelf)
        {
            bool inputDetected = false;

            // Keyboard input
            if (Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.KeypadEnter) ||
                Input.GetKeyDown(KeyCode.Space))
            {
                inputDetected = true;
                Debug.Log("Keyboard input detected: Continue to next level");
            }

            // Mouse click input
            if (Input.GetMouseButtonDown(0))
            {
                inputDetected = true;
                Debug.Log("Mouse click detected: Continue to next level");
            }

            // Touch input for mobile
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                inputDetected = true;
                Debug.Log("Touch input detected: Continue to next level");
            }

            if (inputDetected)
            {
                ContinueToNextLevel();
            }
        }
    }

    public void ShowLoadingScreen(int currentLevel)
    {
        Debug.Log($"LoadingScreen: Showing screen for level {currentLevel}");

        canContinue = false;

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        UpdateScoreDisplay();
        UpdateLevelDescription(currentLevel);

        if (continueInstructionText != null)
            continueInstructionText.gameObject.SetActive(false);

        StartLoadingProgress();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null && ScoreManager.Instance != null)
        {
            int currentScore = ScoreManager.Instance.GetCurrentScore();
            scoreText.text = $"Current Score: {currentScore:N0}";
        }

        if (highScoreText != null)
        {
            int highScore = PlayerPrefs.GetInt("HighScore0", 0);
            string highScoreName = PlayerPrefs.GetString("HighScoreName0", "");

            if (highScore > 0 && !string.IsNullOrEmpty(highScoreName))
            {
                highScoreText.text = $"High Score: {highScoreName} - {highScore:N0}";
            }
            else
            {
                highScoreText.text = "High Score: None yet";
            }
        }

        if (highScoreHolderText != null)
        {
            string highScoreName = PlayerPrefs.GetString("HighScoreName0", "");
            int highScore = PlayerPrefs.GetInt("HighScore0", 0);

            if (string.IsNullOrEmpty(highScoreName) || highScore == 0)
            {
                highScoreHolderText.text = "Be the first record holder!";
            }
            else
            {
                highScoreHolderText.text = $"Record Holder: {highScoreName}";
            }
        }
    }

    private void UpdateLevelDescription(int levelIndex)
    {
        int arrayIndex = levelIndex;

        if (arrayIndex < 0 || arrayIndex >= levelDescriptions.Length)
        {
            Debug.LogWarning($"No description found for level {levelIndex} (array index {arrayIndex})");

            if (levelDescriptionText != null)
                levelDescriptionText.text = $"LEVEL {levelIndex}\nPrepare for combat!";

            return;
        }

        LevelDescription currentLevel = levelDescriptions[arrayIndex];
        Debug.Log($"Using description for level {levelIndex}: {currentLevel.levelName}");

        if (loadingPanel != null)
        {
            Image panelBackground = loadingPanel.GetComponent<Image>();
            if (panelBackground != null)
            {
                panelBackground.color = currentLevel.backgroundColor;
            }
        }

        if (levelDescriptionText != null)
        {
            levelDescriptionText.text = $"{currentLevel.levelName}\n\n{currentLevel.briefing}";
        }

        if (enemyCountText != null)
        {
            enemyCountText.text = $"Expected Enemies: {currentLevel.expectedEnemyCount}";
        }

        if (missionsText != null)
        {
            string missionsString = "PRIMARY OBJECTIVES:\n";
            for (int i = 0; i < currentLevel.missions.Length; i++)
            {
                missionsString += $"• {currentLevel.missions[i]}\n";
            }
            missionsText.text = missionsString;
        }

        if (levelPreviewImage != null && currentLevel.previewImage != null)
        {
            levelPreviewImage.sprite = currentLevel.previewImage;
        }
    }

    private void StartLoadingProgress()
    {
        if (loadingProgressBar != null)
        {
            StartCoroutine(SimulateLoadingProgress());
        }
        else
        {
            StartCoroutine(DelayThenContinue());
        }
    }

    private IEnumerator DelayThenContinue()
    {
        yield return new WaitForSeconds(2f);
        ContinueToNextLevel();
    }

    // FÖRBÄTTRAD SimulateLoadingProgress() metod
    private IEnumerator SimulateLoadingProgress()
    {
        float loadTime = Random.Range(minLoadingTime, maxLoadingTime);
        float elapsedTime = 0f;

        Debug.Log($"Starting loading simulation for {loadTime} seconds...");

        while (elapsedTime < loadTime)
        {
            elapsedTime += Time.deltaTime;
            if (loadingProgressBar != null)
            {
                loadingProgressBar.value = elapsedTime / loadTime;
            }
            yield return null;
        }

        if (loadingProgressBar != null)
        {
            loadingProgressBar.value = 1f;
        }

        canContinue = true;
        Debug.Log("Loading complete - player can now continue with ANY INPUT");

        if (continueInstructionText != null)
        {
            continueInstructionText.gameObject.SetActive(true);
            continueInstructionText.text = "Press ENTER, SPACE, or CLICK to continue";
            StartCoroutine(BlinkContinueText());
        }

        Debug.Log($"canContinue: {canContinue}, loadingPanel active: {loadingPanel?.activeSelf}");
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

    // FÖRBÄTTRAD ContinueToNextLevel() metod med debug
    private void ContinueToNextLevel()
    {
        Debug.Log("=== ContinueToNextLevel called ===");
        Debug.Log($"canContinue: {canContinue}");
        Debug.Log($"loadingPanel active: {loadingPanel?.activeSelf}");

        if (!canContinue)
        {
            Debug.LogWarning("Cannot continue yet - loading not complete");
            return;
        }

        canContinue = false;
        Debug.Log("Setting canContinue to false");

        StopAllCoroutines();

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
            Debug.Log("Loading panel deactivated");
        }

        // VIKTIGT: När vi lämnar LoadingScreen, återuppta poängräkningen för nästa level
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResumeSurvivalScoring();
            Debug.Log("[LoadingScreen] Återupptog poängräkning innan scenändring");
        }

        if (LevelManager.Instance != null)
        {
            int nextLevel = LevelManager.Instance.currentLevel;
            Debug.Log($"Loading level {nextLevel} via LevelManager");
            LoadLevel(nextLevel);
        }
        else
        {
            Debug.LogWarning("No LevelManager found - loading Level1 as fallback");
            LoadLevel(1);
        }
    }

    // FÖRBÄTTRAD LoadLevel() metod med debug
    private void LoadLevel(int levelIndex)
    {
        Debug.Log($"=== LoadLevel called with index: {levelIndex} ===");

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
                Debug.Log($"Trying to load: {levelName}");

                for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                    string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);

                    if (sceneNameFromPath == levelName)
                    {
                        Debug.Log($"Found scene {levelName} at build index {i}. Loading...");
                        SceneManager.LoadScene(levelName);
                        return;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"LoadingScreen: Failed to load {levelName}: {e.Message}");
            }
        }

        Debug.LogError($"LoadingScreen: Could not find Level {levelIndex} in any format!");

        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
        int nextBuildIndex = currentBuildIndex + 1;

        Debug.LogWarning($"Fallback: Loading build index {nextBuildIndex}");

        if (nextBuildIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextBuildIndex);
        }
        else
        {
            Debug.LogError("No more scenes in build settings - loading main menu");
            SceneManager.LoadScene(0);
        }
    }

    // Temporär metod för att tvinga fram completion
    private IEnumerator ForceCompleteAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!canContinue)
        {
            Debug.Log("FORCING canContinue = true for testing");
            canContinue = true;

            if (continueInstructionText != null)
            {
                continueInstructionText.gameObject.SetActive(true);
                continueInstructionText.text = "READY! Press ENTER, SPACE, or CLICK";
            }
        }
    }

    // Test-metod för manuell testning
    [ContextMenu("Test Continue")]
    public void TestContinue()
    {
        Debug.Log("=== MANUAL TEST: Forcing continue ===");
        canContinue = true;
        ContinueToNextLevel();
    }

    public void Show(int currentLevel)
    {
        ShowLoadingScreen(currentLevel);
    }

    // VIKTIGT: Säkerställ att poängen pausas om objektet förstörs
    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.PauseSurvivalScoring();
            Debug.Log("[LoadingScreen] Pausade poängräkning i OnDestroy()");
        }
    }
}
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Values")]
    [SerializeField] private int _enemyShipPoints = 100;
    [SerializeField] private int _bossPoints = 1000;
    [SerializeField] private int _bombTargetPoints = 50;
    [SerializeField] private int _pointsPerSecond = 10;

    // Properties som låter oss ändra värdena i runtime
    public int enemyShipPoints
    {
        get => _enemyShipPoints;
        set => _enemyShipPoints = value;
    }

    public int bossPoints
    {
        get => _bossPoints;
        set => _bossPoints = value;
    }

    public int bombTargetPoints
    {
        get => _bombTargetPoints;
        set => _bombTargetPoints = value;
    }

    public int pointsPerSecond
    {
        get => _pointsPerSecond;
        set => _pointsPerSecond = value;
    }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject highScorePanel;
    [SerializeField] private TextMeshProUGUI[] highScoreTexts;

    [Header("Auto-find UI")]
    [SerializeField] private bool autoFindScoreText = true;

    private int currentScore = 0;
    private List<int> highScores = new List<int>();
    private const int MAX_SCORES = 5;
    private static ScoreManager instance;

    // FIXA: Endast överlevnadspoäng pausas, inte hela systemet
    private bool survivalScoringActive = true;
    private float timeSurvived = 0f;

    public static ScoreManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ScoreManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ScoreManager");
                    instance = go.AddComponent<ScoreManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            // Om en instans redan finns, kopiera dess värden innan vi förstör objektet
            _enemyShipPoints = instance.enemyShipPoints;
            _bossPoints = instance.bossPoints;
            _bombTargetPoints = instance.bombTargetPoints;
            _pointsPerSecond = instance.pointsPerSecond;
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadHighScores();
        }
    }

    private void Start()
    {
        // Försök hitta score text automatiskt om den inte är tilldelad
        if (scoreText == null && autoFindScoreText)
        {
            FindScoreText();
        }

        UpdateScoreDisplay();

        if (highScorePanel != null)
        {
            highScorePanel.SetActive(false);
        }

        Debug.Log($"ScoreManager Start: Score text = {(scoreText != null ? "Found" : "NULL")}, Current score = {currentScore}");
    }

    private void FindScoreText()
    {
        // Leta efter score text med olika möjliga namn
        string[] possibleNames = { "ScoreText", "Score", "ScoreDisplay", "UI_Score", "Text_Score" };

        foreach (string name in possibleNames)
        {
            GameObject found = GameObject.Find(name);
            if (found != null)
            {
                scoreText = found.GetComponent<TextMeshProUGUI>();
                if (scoreText != null)
                {
                    Debug.Log($"ScoreManager: Hittade score text automatiskt: {name}");
                    return;
                }
            }
        }

        // Backup: Leta i alla TextMeshPro komponenter efter text som innehåller "score"
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>();
        foreach (var text in allTexts)
        {
            if (text.text.ToLower().Contains("score"))
            {
                scoreText = text;
                Debug.Log($"ScoreManager: Hittade score text via innehåll: {text.gameObject.name}");
                return;
            }
        }

        Debug.LogWarning("ScoreManager: Kunde inte hitta score text automatiskt!");
    }

    private void Update()
    {
        // FIXA: Bara räkna överlevnadspoäng om det är aktiverat
        if (survivalScoringActive)
        {
            timeSurvived += Time.deltaTime;
            if (timeSurvived >= 1f)
            {
                AddPoints((int)(timeSurvived * _pointsPerSecond));
                timeSurvived = 0f;
            }
        }
    }

    public void AddEnemyShipPoints()
    {
        AddPoints(_enemyShipPoints);
    }

    public void AddBossPoints()
    {
        AddPoints(_bossPoints);
    }

    public void AddBombTargetPoints()
    {
        AddPoints(_bombTargetPoints);
    }

    private void AddPoints(int points)
    {
        currentScore += points;
        Debug.Log($"Added {points} points. New total: {currentScore}");
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        // Försök hitta score text igen om den är null
        if (scoreText == null && autoFindScoreText)
        {
            FindScoreText();
        }

        if (scoreText != null)
        {
            scoreText.text = $"Score {currentScore}";
        }
        else
        {
            Debug.LogWarning("ScoreManager: Kan inte uppdatera score - scoreText är null!");
        }
    }

    // FIXA: Nya metoder för att kontrollera överlevnadspoäng
    public void PauseSurvivalScoring()
    {
        survivalScoringActive = false;
        Debug.Log("ScoreManager: Överlevnadspoäng pausad");
    }

    public void ResumeSurvivalScoring()
    {
        survivalScoringActive = true;
        timeSurvived = 0f; // Reset timer
        Debug.Log("ScoreManager: Överlevnadspoäng återupptagen");
    }

    // Bakåtkompatibilitet - dessa metoder finns kvar men gör samma sak
    public void StopGame()
    {
        PauseSurvivalScoring();
    }

    public void StartSurvivalScoring()
    {
        ResumeSurvivalScoring();
    }

    public void PauseGame()
    {
        PauseSurvivalScoring();
    }

    public void ResumeGame()
    {
        ResumeSurvivalScoring();
    }

    public void ShowHighScores()
    {
        CheckAndAddScore(currentScore);
        if (highScorePanel != null)
        {
            highScorePanel.SetActive(true);
        }
    }

    private void CheckAndAddScore(int score)
    {
        highScores.Add(score);
        highScores = highScores.OrderByDescending(x => x).Take(MAX_SCORES).ToList();
        SaveHighScores();
        DisplayHighScores();
    }

    private void LoadHighScores()
    {
        highScores.Clear();
        for (int i = 0; i < MAX_SCORES; i++)
        {
            int score = PlayerPrefs.GetInt($"HighScore{i}", 0);
            if (score > 0) highScores.Add(score);
        }
    }

    private void SaveHighScores()
    {
        for (int i = 0; i < highScores.Count; i++)
        {
            PlayerPrefs.SetInt($"HighScore{i}", highScores[i]);
        }
        PlayerPrefs.Save();
    }

    private void DisplayHighScores()
    {
        if (highScoreTexts == null) return;

        for (int i = 0; i < highScoreTexts.Length; i++)
        {
            if (highScoreTexts[i] != null)
            {
                highScoreTexts[i].text = i < highScores.Count
                    ? $"{i + 1}. {highScores[i]}"
                    : $"{i + 1}. ---";
            }
        }
    }

    public void HideHighScores()
    {
        if (highScorePanel != null)
        {
            highScorePanel.SetActive(false);
        }
    }

    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreDisplay();
    }

    public int GetCurrentScore() => currentScore;

    public void SetScore(int score)
    {
        currentScore = score;
        UpdateScoreDisplay();
    }

    // Status-kontroll
    public bool IsSurvivalScoringActive() => survivalScoringActive;

    // Debug metoder
    [ContextMenu("Force Find Score Text")]
    public void ForceFindScoreText()
    {
        FindScoreText();
        UpdateScoreDisplay();
    }

    [ContextMenu("Test Add Points")]
    public void TestAddPoints()
    {
        AddEnemyShipPoints();
    }

    [ContextMenu("Toggle Survival Scoring")]
    public void ToggleSurvivalScoring()
    {
        if (survivalScoringActive)
            PauseSurvivalScoring();
        else
            ResumeSurvivalScoring();
    }
}
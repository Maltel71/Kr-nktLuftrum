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

    private int currentScore = 0;
    private List<int> highScores = new List<int>();
    private const int MAX_SCORES = 5;
    private static ScoreManager instance;
    private bool isGameActive = true;
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
        UpdateScoreDisplay();
        if (highScorePanel != null)
        {
            highScorePanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (isGameActive)
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
        Debug.Log($"Adding enemy ship points: {_enemyShipPoints}");
        AddPoints(_enemyShipPoints);
    }

    public void AddBossPoints()
    {
        Debug.Log($"Adding boss points: {_bossPoints}");
        AddPoints(_bossPoints);
    }

    public void AddBombTargetPoints()
    {
        Debug.Log($"Adding bomb target points: {_bombTargetPoints}");
        AddPoints(_bombTargetPoints);
    }

    private void AddPoints(int points)
    {
        currentScore += points;
        //Debug.Log($"Added {points} points. New total: {currentScore}");
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }

    public void StopGame()
    {
        isGameActive = false;
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
}
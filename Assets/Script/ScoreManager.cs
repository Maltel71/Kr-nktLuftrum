using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Values")]
    [SerializeField] private int enemyShipPoints = 100;
    [SerializeField] private int bossPoints = 1000;
    [SerializeField] private int bombTargetPoints = 50;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject highScorePanel;
    [SerializeField] private TextMeshProUGUI[] highScoreTexts;

    private int currentScore = 0;
    private List<int> highScores = new List<int>();
    private const int MAX_SCORES = 5;
    private static ScoreManager instance;

    public static ScoreManager Instance => instance;

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
        LoadHighScores();
    }

    private void Start()
    {
        UpdateScoreDisplay();
        highScorePanel.SetActive(false);
    }

    public void AddEnemyShipPoints() => AddPoints(enemyShipPoints);
    public void AddBossPoints() => AddPoints(bossPoints);
    public void AddBombTargetPoints() => AddPoints(bombTargetPoints);

    private void AddPoints(int points)
    {
        currentScore += points;
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }

    public void ShowHighScores()
    {
        CheckAndAddScore(currentScore);
        highScorePanel.SetActive(true);
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
        for (int i = 0; i < highScoreTexts.Length; i++)
        {
            highScoreTexts[i].text = i < highScores.Count
                ? $"{i + 1}. {highScores[i]}"
                : $"{i + 1}. ---";
        }
    }

    public void HideHighScores() => highScorePanel.SetActive(false);
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreDisplay();
    }
}
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class HighScoreManager : MonoBehaviour
{
    [SerializeField] private GameObject highScorePanel;
    [SerializeField] private TextMeshProUGUI[] highScoreTexts;
    private const int MAX_SCORES = 5;
    private List<int> highScores = new List<int>();

    private void Start()
    {
        LoadHighScores();
        highScorePanel.SetActive(false);
    }

    public void CheckAndAddScore(int score)
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
        highScorePanel.SetActive(true);

        for (int i = 0; i < highScoreTexts.Length; i++)
        {
            if (i < highScores.Count)
            {
                highScoreTexts[i].text = $"{i + 1}. {highScores[i]}";
            }
            else
            {
                highScoreTexts[i].text = $"{i + 1}. ---";
            }
        }
    }

    public void HideHighScores()
    {
        highScorePanel.SetActive(false);
    }
}
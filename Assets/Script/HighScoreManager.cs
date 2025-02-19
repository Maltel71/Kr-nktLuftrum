using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class HighScoreManager : MonoBehaviour
{
    [SerializeField] private GameObject highScorePanel;
    [SerializeField] private GameObject nameInputPanel;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TextMeshProUGUI[] scoreTexts;

    [SerializeField] private Button retryButton;
    [SerializeField] private Button quitButton;

    private const int MAX_SCORES = 5;
    private List<HighScoreEntry> highScores = new List<HighScoreEntry>();
    private int currentScore;

    private void Start()
    {
        LoadHighScores();
        highScorePanel.SetActive(false);
        nameInputPanel.SetActive(false);

        // Lägg till lyssnare för Enter-tangenten på input-fältet
        if (nameInputField != null)
        {
            nameInputField.onSubmit.AddListener(OnInputFieldSubmit);
        }

        if (retryButton != null)
            retryButton.onClick.AddListener(RetryGame);
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitToTitleScreen);
    }

    // Ny metod för att hantera Enter-tangenten
    private void OnInputFieldSubmit(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            OnNameSubmitted();
        }
    }

    public void OnPlayerDeath(int score)
    {
        currentScore = score;

        if (IsHighScore(score))
        {
            nameInputPanel.SetActive(true);
            nameInputField.text = "";
            nameInputField.Select();
            nameInputField.ActivateInputField();
        }
        else
        {
            ShowHighScores();
        }
    }

    private bool IsHighScore(int score)
    {
        return highScores.Count < MAX_SCORES || score > highScores.Min(x => x.score);
    }

    public void OnNameSubmitted()
    {
        string playerName = nameInputField.text;
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "Player";
        }

        highScores.Add(new HighScoreEntry(playerName, currentScore));
        highScores = highScores.OrderByDescending(x => x.score).Take(MAX_SCORES).ToList();

        SaveHighScores();
        nameInputPanel.SetActive(false);
        ShowHighScores();
    }

    private void ShowHighScores()
    {
        highScorePanel.SetActive(true);

        for (int i = 0; i < scoreTexts.Length; i++)
        {
            if (i < highScores.Count)
            {
                scoreTexts[i].text = $"{i + 1}. {highScores[i].name} - {highScores[i].score}";
            }
            else
            {
                scoreTexts[i].text = $"{i + 1}. ---";
            }
        }
    }

    private void SaveHighScores()
    {
        for (int i = 0; i < highScores.Count; i++)
        {
            PlayerPrefs.SetString($"HighScoreName{i}", highScores[i].name);
            PlayerPrefs.SetInt($"HighScore{i}", highScores[i].score);
        }
        PlayerPrefs.Save();
    }

    private void LoadHighScores()
    {
        highScores.Clear();
        for (int i = 0; i < MAX_SCORES; i++)
        {
            string name = PlayerPrefs.GetString($"HighScoreName{i}", "");
            int score = PlayerPrefs.GetInt($"HighScore{i}", 0);
            if (score > 0)
            {
                highScores.Add(new HighScoreEntry(name, score));
            }
        }
    }

    public void HideHighScores()
    {
        highScorePanel.SetActive(false);
    }

    private void RetryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void QuitToTitleScreen()
    {
        SceneManager.LoadScene(0);
    }
}
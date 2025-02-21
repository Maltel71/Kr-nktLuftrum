using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class HighScoreManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject highScorePanel;
    [SerializeField] private GameObject nameInputPanel;

    [Header("UI Elements")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TextMeshProUGUI[] scoreTexts;
    [SerializeField] private GameObject[] gameplayUIElements;

    [Header("Buttons")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button quitButton;

    private const int MAX_SCORES = 5;
    private List<HighScoreEntry> highScores = new List<HighScoreEntry>();
    private int currentScore;

    private void Start()
    {
        InitializeManager();
        SetupEventListeners();
    }

    private void InitializeManager()
    {
        LoadHighScores();

        if (highScorePanel != null) highScorePanel.SetActive(false);
        if (nameInputPanel != null) nameInputPanel.SetActive(false);
    }

    private void SetupEventListeners()
    {
        if (nameInputField != null)
        {
            nameInputField.onSubmit.AddListener(OnInputFieldSubmit);
        }

        if (retryButton != null) retryButton.onClick.AddListener(RetryGame);
        if (quitButton != null) quitButton.onClick.AddListener(QuitToTitleScreen);
    }

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

        // St�ng av alla gameplay UI element f�rst
        foreach (GameObject uiElement in gameplayUIElements)
        {
            if (uiElement != null)
            {
                uiElement.SetActive(false);
            }
        }

        // Kontrollera om det �r en high score INNAN vi visar n�got
        if (IsHighScore(score))
        {
            ShowNameInputPanel();
        }
        else
        {
            // Om det inte �r en high score, g� direkt till att visa high scores
            nameInputPanel.SetActive(false);  // F�rs�kra att input panel �r dold
            ShowHighScores();
        }
    }

    private void DisableGameplayUI()
    {
        foreach (var ui in gameplayUIElements)
        {
            if (ui != null)
            {
                ui.SetActive(false);
            }
        }
    }

    private void ShowNameInputPanel()
    {
        if (nameInputPanel == null || nameInputField == null) return;

        nameInputPanel.SetActive(true);
        nameInputField.text = "";
        nameInputField.Select();
        nameInputField.ActivateInputField();
    }

    public void ActivateGameplayUI()
    {
        foreach (var ui in gameplayUIElements)
        {
            if (ui != null)
            {
                ui.SetActive(true);
            }
        }
    }

    private bool IsHighScore(int score)
    {
        bool isHighScore = highScores.Count < MAX_SCORES || score > highScores.Min(x => x.score);
        Debug.Log($"Score: {score}, Is High Score: {isHighScore}, Current High Scores: {highScores.Count}");
        if (highScores.Count > 0)
        {
            Debug.Log($"Minimum high score: {highScores.Min(x => x.score)}");
        }
        return isHighScore;
    }

    public void OnNameSubmitted()
    {
        if (!IsHighScore(currentScore))
        {
            nameInputPanel.SetActive(false);
            ShowHighScores();
            return;
        }

        string playerName = string.IsNullOrEmpty(nameInputField.text) ? "Player" : nameInputField.text;
        highScores.Add(new HighScoreEntry(playerName, currentScore));
        highScores = highScores.OrderByDescending(x => x.score).Take(MAX_SCORES).ToList();

        SaveHighScores();
        nameInputPanel.SetActive(false);
        ShowHighScores();
    }

    private void ShowHighScores()
    {
        if (highScorePanel == null || scoreTexts == null) return;

        highScorePanel.SetActive(true);

        for (int i = 0; i < scoreTexts.Length; i++)
        {
            if (scoreTexts[i] != null)
            {
                scoreTexts[i].text = i < highScores.Count
                    ? $"{i + 1}. {highScores[i].name} - {highScores[i].score}"
                    : $"{i + 1}. ---";
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
        if (highScorePanel != null) highScorePanel.SetActive(false);
    }

    private void RetryGame()
    {
        ScoreManager.Instance.ResetScore();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void QuitToTitleScreen()
    {
        SceneManager.LoadScene(0);
    }
}
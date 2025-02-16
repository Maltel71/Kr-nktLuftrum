using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class HighScoreManager : MonoBehaviour
{
    // UI Referenser
    [SerializeField] private GameObject highScorePanel;      // Panel f�r toplistan
    [SerializeField] private GameObject nameInputPanel;      // Panel f�r namn-input
    [SerializeField] private TMP_InputField nameInputField;  // Input-f�ltet
    [SerializeField] private TextMeshProUGUI[] scoreTexts;   // Array med text-elementen f�r topplistan

    [SerializeField] private Button retryButton;
    [SerializeField] private Button quitButton;

    private const int MAX_SCORES = 5;
    private List<HighScoreEntry> highScores = new List<HighScoreEntry>();
    private int currentScore;  // Tempor�r lagring av aktuell po�ng

    // Denna struktur h�ller data f�r varje highscore-post
    [System.Serializable]
    private struct HighScoreEntry
    {
        public string name;
        public int score;

        public HighScoreEntry(string name, int score)
        {
            this.name = name;
            this.score = score;
        }
    }

    private void Start()
    {
        LoadHighScores();         // Ladda sparade highscores
        highScorePanel.SetActive(false);
        nameInputPanel.SetActive(false);

        if (retryButton != null)
            retryButton.onClick.AddListener(RetryGame);
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitToTitleScreen);
    }

    // Anropas n�r spelaren d�r
    public void OnPlayerDeath(int score)
    {
        currentScore = score;

        // Kolla om po�ngen �r tillr�ckligt h�g
        if (IsHighScore(score))
        {
            nameInputPanel.SetActive(true);  // Visa namn-input f�rst
            nameInputField.text = "";
            nameInputField.Select();
        }
        else
        {
            ShowHighScores();  // Visa bara topplistan
        }
    }

    // Kollar om po�ngen �r tillr�ckligt h�g f�r topplistan
    private bool IsHighScore(int score)
    {
        return highScores.Count < MAX_SCORES || score > highScores.Min(x => x.score);
    }

    // Anropas n�r spelaren klickar p� OK-knappen
    public void OnNameSubmitted()
    {
        string playerName = nameInputField.text;
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "Player";  // Default namn om inget anges
        }

        // L�gg till ny highscore
        highScores.Add(new HighScoreEntry(playerName, currentScore));

        // Sortera och beh�ll bara top 5
        highScores = highScores.OrderByDescending(x => x.score).Take(MAX_SCORES).ToList();

        SaveHighScores();        // Spara till PlayerPrefs
        nameInputPanel.SetActive(false);
        ShowHighScores();        // Visa topplistan
    }

    // Visar highscore-panelen och uppdaterar texten
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

    // Sparar highscores i PlayerPrefs
    private void SaveHighScores()
    {
        for (int i = 0; i < highScores.Count; i++)
        {
            PlayerPrefs.SetString($"HighScoreName{i}", highScores[i].name);
            PlayerPrefs.SetInt($"HighScore{i}", highScores[i].score);
        }
        PlayerPrefs.Save();
    }

    // Laddar highscores fr�n PlayerPrefs
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

    // Knapp f�r att st�nga highscore-panelen
    public void HideHighScores()
    {
        highScorePanel.SetActive(false);
    }

    private void RetryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        SceneManager.LoadScene(1);
    }

    private void QuitToTitleScreen()
    {
        SceneManager.LoadScene(0);
        SceneManager.LoadScene("Andreas Test scen"); // Se till att du har en scen med detta namn
    }

}
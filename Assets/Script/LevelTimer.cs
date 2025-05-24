using UnityEngine;
using TMPro;

public class LevelTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    private float currentTime = 0f;
    private bool isRunning = true;

    private void Start()
    {
        // Läs senaste sparade tid för denna bana
        string levelName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        float bestTime = PlayerPrefs.GetFloat($"BestTime_{levelName}", 0f);

        if (bestTime > 0)
        {
            Debug.Log($"Bästa tid för {levelName}: {FormatTime(bestTime)}");
        }
    }

    private void Update()
    {
        if (isRunning)
        {
            currentTime += Time.deltaTime;
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (timerText != null)
        {
            string levelName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            float bestTime = PlayerPrefs.GetFloat($"BestTime_{levelName}", 0f);

            string currentTimeStr = FormatTime(currentTime);
            string bestTimeStr = bestTime > 0 ? FormatTime(bestTime) : "--:--";

            timerText.text = $"Nu: {currentTimeStr}\nBäst: {bestTimeStr}";
        }
    }

    public void StopAndSave()
    {
        isRunning = false;

        string levelName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        float oldBestTime = PlayerPrefs.GetFloat($"BestTime_{levelName}", float.MaxValue);

        // Spara tiden (ersätt alltid)
        PlayerPrefs.SetFloat($"BestTime_{levelName}", currentTime);
        PlayerPrefs.Save();

        string timeStr = FormatTime(currentTime);
        if (currentTime < oldBestTime)
        {
            Debug.Log($"🏆 NY REKORDTID för {levelName}: {timeStr}");
        }
        else
        {
            Debug.Log($"⏰ Bana klar på: {timeStr}");
        }

        UpdateDisplay();
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    // Anropa detta när banan är klar
    public void LevelComplete()
    {
        StopAndSave();
    }
}
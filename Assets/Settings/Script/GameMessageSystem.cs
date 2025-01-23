using UnityEngine;
using TMPro;
using System.Collections;

public class GameMessageSystem : MonoBehaviour
{
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Message Display Times")]
    [SerializeField] private float gameOverDisplayTime = 3f;  // Tid för game over meddelande
    [SerializeField] private float boostDisplayTime = 1.5f;   // Tid för boost meddelande
    [SerializeField] private float bossDisplayTime = 2f;      // Tid för boss meddelande

    [Header("Message Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private Color boostColor = Color.green;

    private void Start()
    {
        if (messagePanel != null)
        {
            messagePanel.SetActive(true);
            if (messageText != null)
            {
                messageText.enabled = false;
            }
        }
    }

    public void ShowDeathMessage()
    {
        Debug.Log("ShowDeathMessage called");
        DisplayMessage("GAME OVER!", warningColor, gameOverDisplayTime);
    }

    public void ShowBossFight()
    {
        DisplayMessage("BOSS FIGHT!", warningColor, bossDisplayTime);
    }

    public void ShowBoostMessage(string boostType)
    {
        DisplayMessage($"{boostType} Aktiverad!", boostColor, boostDisplayTime);
    }

    private void DisplayMessage(string message, Color color, float displayTime)
    {
        if (messagePanel == null || messageText == null)
        {
            Debug.LogError("Message Panel or Text component missing!");
            return;
        }

        Debug.Log($"Displaying message: {message} for {displayTime} seconds");
        StopAllCoroutines();

        messagePanel.SetActive(true);
        messageText.enabled = true;
        messageText.text = message;
        messageText.color = color;

        StartCoroutine(HideMessageCoroutine(displayTime));
    }

    private IEnumerator HideMessageCoroutine(float displayTime)
    {
        yield return new WaitForSeconds(displayTime);

        if (messageText != null)
        {
            messageText.enabled = false;
        }
    }
}
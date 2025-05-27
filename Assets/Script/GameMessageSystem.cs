using UnityEngine;
using TMPro;
using System.Collections;

public class GameMessageSystem : MonoBehaviour
{
    [Header("Normal Messages (Top of screen)")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Overheat Messages (Bottom of screen)")]
    [SerializeField] private GameObject overheatMessagePanel;
    [SerializeField] private TextMeshProUGUI overheatMessageText;

    [Header("Message Display Times")]
    [SerializeField] private float gameOverDisplayTime = 3f;  // Tid för game over meddelande
    [SerializeField] private float boostDisplayTime = 1.5f;   // Tid för boost meddelande
    [SerializeField] private float bossDisplayTime = 2f;      // Tid för boss meddelande
    [SerializeField] private float overheatDisplayTime = 2f;  // Tid för overheat meddelanden

    [Header("Message Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private Color boostColor = Color.green;
    [SerializeField] private Color overheatWarningColor = Color.red;
    [SerializeField] private Color overheatReadyColor = Color.green;

    private void Start()
    {
        // Setup normal messages (top)
        if (messagePanel != null)
        {
            messagePanel.SetActive(true);
            if (messageText != null)
            {
                messageText.enabled = false;
            }
        }

        // Setup overheat messages (bottom)
        if (overheatMessagePanel != null)
        {
            overheatMessagePanel.SetActive(true);
            if (overheatMessageText != null)
            {
                overheatMessageText.enabled = false;
            }
        }
    }

    // ===== NORMAL MESSAGES (TOP) =====
    public void ShowDeathMessage()
    {
        Debug.Log("ShowDeathMessage called");
        DisplayMessage("GAME OVER!", warningColor, gameOverDisplayTime);
    }

    public void ShowBossFight()
    {
        DisplayMessage("BOSS FIGHT!", warningColor, bossDisplayTime);
    }

    public void ShowBoostMessage(string message)
    {
        DisplayMessage(message, boostColor, boostDisplayTime);
    }

    private void DisplayMessage(string message, Color color, float displayTime)
    {
        if (messagePanel == null || messageText == null)
        {
            Debug.LogError("Message Panel or Text component missing!");
            return;
        }

        Debug.Log($"Displaying message: {message} for {displayTime} seconds");
        StopCoroutine(nameof(HideMessageCoroutine));

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

    // ===== OVERHEAT MESSAGES (BOTTOM) =====
    public void ShowOverheatWarning(string message)
    {
        DisplayOverheatMessage(message, overheatWarningColor, overheatDisplayTime);
    }

    public void ShowOverheatReady(string message)
    {
        DisplayOverheatMessage(message, overheatReadyColor, overheatDisplayTime);
    }

    private void DisplayOverheatMessage(string message, Color color, float displayTime)
    {
        if (overheatMessagePanel == null || overheatMessageText == null)
        {
            Debug.LogWarning("Overheat message components missing! Using normal message instead.");
            // Fallback till vanligt meddelande om overheat-komponenter saknas
            DisplayMessage(message, color, displayTime);
            return;
        }

        Debug.Log($"Displaying overheat message: {message}");
        StopCoroutine(nameof(HideOverheatMessageCoroutine));

        overheatMessagePanel.SetActive(true);
        overheatMessageText.enabled = true;
        overheatMessageText.text = message;
        overheatMessageText.color = color;

        StartCoroutine(HideOverheatMessageCoroutine(displayTime));
    }

    private IEnumerator HideOverheatMessageCoroutine(float displayTime)
    {
        yield return new WaitForSeconds(displayTime);

        if (overheatMessageText != null)
        {
            overheatMessageText.enabled = false;
        }
    }
}
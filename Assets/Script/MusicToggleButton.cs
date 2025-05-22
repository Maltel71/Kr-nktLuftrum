using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MusicToggleButton : MonoBehaviour
{
    [Header("UI Referenser")]
    [SerializeField] private Button toggleButton;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Image buttonImage;

    [Header("Visual Settings")]
    [SerializeField] private string musicOnText = "♪ ON";
    [SerializeField] private string musicOffText = "♪ OFF";
    [SerializeField] private Color musicOnColor = Color.green;
    [SerializeField] private Color musicOffColor = Color.red;

    [Header("Alternative Icons (valfritt)")]
    [SerializeField] private Sprite musicOnIcon;
    [SerializeField] private Sprite musicOffIcon;

    private AudioManager audioManager;

    private void Start()
    {
        // Hitta AudioManager
        audioManager = AudioManager.Instance;

        if (audioManager == null)
        {
            Debug.LogError("AudioManager hittades inte!");
            return;
        }

        // Sätt upp knappen
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleMusic);
        }

        // Uppdatera visuellt baserat på nuvarande status
        UpdateVisuals();
    }

    public void ToggleMusic()
    {
        if (audioManager != null)
        {
            audioManager.ToggleMusic();
            UpdateVisuals();
        }
    }

    private void UpdateVisuals()
    {
        if (audioManager == null) return;

        bool musicEnabled = audioManager.IsMusicEnabled();

        // Uppdatera text
        if (buttonText != null)
        {
            buttonText.text = musicEnabled ? musicOnText : musicOffText;
            buttonText.color = musicEnabled ? musicOnColor : musicOffColor;
        }

        // Uppdatera knappfärg
        if (buttonImage != null)
        {
            buttonImage.color = musicEnabled ? musicOnColor : musicOffColor;
        }

        // Uppdatera ikon om du har olika ikoner
        if (buttonImage != null && musicOnIcon != null && musicOffIcon != null)
        {
            buttonImage.sprite = musicEnabled ? musicOnIcon : musicOffIcon;
        }
    }

    // Metod som kan anropas från andra script om behövs
    public void SetMusicEnabled(bool enabled)
    {
        if (audioManager != null)
        {
            audioManager.SetMusicEnabled(enabled);
            UpdateVisuals();
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class ActiveBoostUI : MonoBehaviour
{
    [System.Serializable]
    public class BoostUIElement
    {
        public GameObject uiPanel;
        public Image boostIcon;
        public TextMeshProUGUI timerText;
    }

    [Header("UI References")]
    [SerializeField] private GameObject boostUIPrefab;
    [SerializeField] private Transform boostContainer;
    [SerializeField] private float spacing = 60f;

    private Dictionary<string, BoostUIElement> activeBoosts = new Dictionary<string, BoostUIElement>();

    public static ActiveBoostUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("ActiveBoostUI initialized");
        }
        else
        {
            Debug.LogWarning("Multiple ActiveBoostUI instances found!");
            Destroy(gameObject);
        }

        // Validera referenser
        if (boostUIPrefab == null)
            Debug.LogError("BoostUIPrefab är inte satt!");
        if (boostContainer == null)
            Debug.LogError("BoostContainer är inte satt!");
    }

    public void AddBoost(string boostName, Sprite icon, float duration)
    {
        Debug.Log($"Försöker lägga till boost: {boostName}, Duration: {duration}");

        if (boostUIPrefab == null)
        {
            Debug.LogError("Kan inte lägga till boost - boostUIPrefab saknas!");
            return;
        }

        if (boostContainer == null)
        {
            Debug.LogError("Kan inte lägga till boost - boostContainer saknas!");
            return;
        }

        if (icon == null)
        {
            Debug.LogWarning($"Boost ikon saknas för {boostName}!");
        }

        // Om boost redan finns, uppdatera bara tiden
        if (activeBoosts.ContainsKey(boostName))
        {
            Debug.Log($"Uppdaterar existerande boost: {boostName}");
            StopCoroutine(activeBoosts[boostName].uiPanel.name);
            StartCoroutine(UpdateBoostTimer(boostName, duration));
            return;
        }

        // Skapa nytt UI element
        GameObject newBoostUI = Instantiate(boostUIPrefab, boostContainer);
        Debug.Log($"Skapade ny boost UI för: {boostName}");

        BoostUIElement uiElement = new BoostUIElement
        {
            uiPanel = newBoostUI,
            boostIcon = newBoostUI.GetComponentInChildren<Image>(),
            timerText = newBoostUI.GetComponentInChildren<TextMeshProUGUI>()
        };

        // Validera komponenter
        if (uiElement.boostIcon == null)
            Debug.LogError($"Image komponent saknas på boost UI för {boostName}");
        if (uiElement.timerText == null)
            Debug.LogError($"TextMeshProUGUI komponent saknas på boost UI för {boostName}");

        // Konfigurera UI element
        uiElement.boostIcon.sprite = icon;
        activeBoosts.Add(boostName, uiElement);

        // Organisera om alla boost UI element
        UpdateUILayout();

        // Starta timer
        StartCoroutine(UpdateBoostTimer(boostName, duration));
    }

    private void UpdateUILayout()
    {
        int index = 0;
        foreach (var boost in activeBoosts.Values)
        {
            boost.uiPanel.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(index * spacing, 0);
            index++;
        }
        Debug.Log($"UI layout uppdaterad. Antal aktiva boosts: {activeBoosts.Count}");
    }

    private IEnumerator UpdateBoostTimer(string boostName, float duration)
    {
        if (!activeBoosts.TryGetValue(boostName, out BoostUIElement uiElement))
        {
            Debug.LogError($"Kunde inte hitta boost: {boostName} för timer uppdatering");
            yield break;
        }

        float timeLeft = duration;
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            uiElement.timerText.text = timeLeft.ToString("F1");
            yield return null;
        }

        RemoveBoost(boostName);
    }

    private void RemoveBoost(string boostName)
    {
        Debug.Log($"Tar bort boost: {boostName}");
        if (activeBoosts.TryGetValue(boostName, out BoostUIElement uiElement))
        {
            Destroy(uiElement.uiPanel);
            activeBoosts.Remove(boostName);
            UpdateUILayout();
        }
    }
}
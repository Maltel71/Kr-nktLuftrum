using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ActiveBoostUI : MonoBehaviour
{
    [System.Serializable]
    public class BoostUIElement
    {
        public GameObject uiPanel;
        public Image boostIcon;
        public TextMeshProUGUI timerText;
        public TextMeshProUGUI boostNameText;
    }

    [Header("UI Referenser")]
    [SerializeField] private GameObject boostUIPrefab;
    [SerializeField] private RectTransform boostContainer;  // Ändrad till RectTransform
    [SerializeField] private float spacing = 100f;          // Ökad spacing för bättre läsbarhet
    [SerializeField] private float bottomOffset = 20f;      // Avstånd från skärmens nederkant

    [Header("Boost Ikoner")]
    [SerializeField] private Sprite healthIcon;
    [SerializeField] private Sprite shieldIcon;
    [SerializeField] private Sprite speedIcon;
    [SerializeField] private Sprite dualWeaponIcon;
    [SerializeField] private Sprite flareIcon;
    [SerializeField] private Sprite bombIcon;

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
    }

    public void AddBoost(string boostName, Sprite icon, float duration)
    {
        // Om boost redan finns, uppdatera bara tiden
        if (activeBoosts.ContainsKey(boostName))
        {
            StopCoroutine(activeBoosts[boostName].uiPanel.name);
            StartCoroutine(UpdateBoostTimer(boostName, duration));
            return;
        }

        GameObject newBoostUI = Instantiate(boostUIPrefab, boostContainer);

        BoostUIElement uiElement = new BoostUIElement
        {
            uiPanel = newBoostUI,
            boostIcon = newBoostUI.GetComponentInChildren<Image>(),
            timerText = newBoostUI.GetComponentInChildren<TextMeshProUGUI>(),
            boostNameText = newBoostUI.GetComponentInChildren<TextMeshProUGUI>()
        };

        // Sätt boost-namn och ikon
        uiElement.boostIcon.sprite = icon;
        uiElement.boostNameText.text = boostName;

        activeBoosts.Add(boostName, uiElement);

        // Organisera om boost UI element
        UpdateUILayout();

        // Starta timer
        StartCoroutine(UpdateBoostTimer(boostName, duration));

        // Visa meddelande via GameMessageSystem
        GameMessageSystem messageSystem = FindObjectOfType<GameMessageSystem>();
        if (messageSystem != null)
        {
            messageSystem.ShowBoostMessage($"{boostName} Aktiverad!");
        }
    }

    public Sprite GetBoostIcon(string boostType)
    {
        return boostType switch
        {
            "Health" => healthIcon,
            "Shield" => shieldIcon,
            "Speed" => speedIcon,
            "DualWeapons" => dualWeaponIcon,
            "Flare" => flareIcon,
            "Bomb" => bombIcon,
            _ => null
        };
    }

    private void UpdateUILayout()
    {
        int index = 0;
        float totalWidth = activeBoosts.Count * spacing;
        float startX = -totalWidth / 2f;  // Centrera alla boosts

        foreach (var boost in activeBoosts.Values)
        {
            RectTransform rect = boost.uiPanel.GetComponent<RectTransform>();

            // Sätt ankarpunkter för nederkanten av skärmen
            rect.anchorMin = new Vector2(0.5f, 0);
            rect.anchorMax = new Vector2(0.5f, 0);
            rect.pivot = new Vector2(0.5f, 0);

            // Positionera relativt till container
            rect.anchoredPosition = new Vector2(startX + (index * spacing), bottomOffset);

            // Sätt storlek för boost-elementet
            rect.sizeDelta = new Vector2(80, 80);  // Justera storleken efter behov

            index++;
        }
    }

    private System.Collections.IEnumerator UpdateBoostTimer(string boostName, float duration)
    {
        if (!activeBoosts.TryGetValue(boostName, out BoostUIElement uiElement))
        {
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
        if (activeBoosts.TryGetValue(boostName, out BoostUIElement uiElement))
        {
            Destroy(uiElement.uiPanel);
            activeBoosts.Remove(boostName);
            UpdateUILayout();
        }
    }
}
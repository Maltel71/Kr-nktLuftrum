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
        public float remainingTime;
    }

    [Header("UI References")]
    [SerializeField] private GameObject boostUIPrefab;
    [SerializeField] private Transform boostContainer;
    [SerializeField] private float spacing = 60f;

    [Header("Boost Icons")]
    [SerializeField] private Sprite healthBoostIcon;
    [SerializeField] private Sprite speedBoostIcon;
    [SerializeField] private Sprite shieldBoostIcon;
    [SerializeField] private Sprite fireRateBoostIcon;
    [SerializeField] private Sprite dualWeaponsIcon;

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

    private void Update()
    {
        List<string> boostsToRemove = new List<string>();

        foreach (var boost in activeBoosts)
        {
            boost.Value.remainingTime -= Time.deltaTime;

            if (boost.Value.remainingTime <= 0)
            {
                boostsToRemove.Add(boost.Key);
            }
            else
            {
                boost.Value.timerText.text = boost.Value.remainingTime.ToString("F1");
            }
        }

        foreach (var boostName in boostsToRemove)
        {
            RemoveBoost(boostName);
        }
    }

    public void AddBoost(string boostName, Sprite boostIcon, float duration)
    {
        if (activeBoosts.ContainsKey(boostName))
        {
            // Om boost redan finns, återställ bara tiden
            activeBoosts[boostName].remainingTime = duration;
            return;
        }

        // Skapa nytt UI element
        GameObject newBoostUI = Instantiate(boostUIPrefab, boostContainer);

        BoostUIElement uiElement = new BoostUIElement
        {
            uiPanel = newBoostUI,
            boostIcon = newBoostUI.GetComponentInChildren<Image>(),
            timerText = newBoostUI.GetComponentInChildren<TextMeshProUGUI>(),
            remainingTime = duration
        };

        // Sätt ikon
        if (uiElement.boostIcon != null && boostIcon != null)
        {
            uiElement.boostIcon.sprite = boostIcon;
        }

        activeBoosts.Add(boostName, uiElement);
        UpdateUILayout();
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

    private void UpdateUILayout()
    {
        int index = 0;
        foreach (var boost in activeBoosts.Values)
        {
            if (boost.uiPanel != null)
            {
                boost.uiPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(index * spacing, 0);
                index++;
            }
        }
    }

    public Sprite GetBoostIcon(string boostType)
    {
        return boostType switch
        {
            "HealthBoost" => healthBoostIcon,
            "SpeedBoost" => speedBoostIcon,
            "ShieldBoost" => shieldBoostIcon,
            "FireRateBoost" => fireRateBoostIcon,
            "DualWeapons" => dualWeaponsIcon,
            _ => null
        };
    }
}
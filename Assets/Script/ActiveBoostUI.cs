using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ActiveBoostUI : MonoBehaviour
{
    public enum BoostType
    { 
        SpeedBoost,       // Tidsbegränsad
        FireRateBoost,    // Tidsbegränsad
        DualWeapons       // Tidsbegränsad
    }

    [System.Serializable]
    public class BoostUIElement
    {
        public GameObject uiPanel;
        public Image boostIcon;
        public TextMeshProUGUI timerText;
        public float remainingTime;
        public BoostType boostType;
    }

    [Header("UI References")]
    [SerializeField] private GameObject boostUIPrefab;
    [SerializeField] private Transform boostContainer;
    [SerializeField] private float spacing = 60f;

    [Header("Boost Icons")]
    [SerializeField] private Sprite speedBoostIcon;
    [SerializeField] private Sprite fireRateBoostIcon;
    [SerializeField] private Sprite dualWeaponsIcon;

    private Dictionary<BoostType, BoostUIElement> activeBoosts = new Dictionary<BoostType, BoostUIElement>();
    public static ActiveBoostUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        List<BoostType> boostsToRemove = new List<BoostType>();

        foreach (var boost in activeBoosts)
        {
            boost.Value.remainingTime -= Time.deltaTime;

            if (boost.Value.remainingTime <= 0)
            {
                boostsToRemove.Add(boost.Key);
            }
            else
            {
                UpdateBoostTimer(boost.Value);
            }
        }

        foreach (var boostType in boostsToRemove)
        {
            RemoveBoost(boostType);
        }
    }

    private void UpdateBoostTimer(BoostUIElement boost)
    {
        if (boost.timerText != null)
        {
            boost.timerText.text = $"{boost.remainingTime:F1}s";
        }
    }

    public void AddBoost(BoostType boostType, float duration)
    {
        Debug.Log($"AddBoost called. Type: {boostType}, Duration: {duration}");
        if (activeBoosts.ContainsKey(boostType))
        {
            RefreshBoostDuration(boostType, duration);
            return;
        }

        CreateNewBoostUI(boostType, duration);
        UpdateUILayout();
    }

    private void RefreshBoostDuration(BoostType boostType, float duration)
    {
        if (activeBoosts.TryGetValue(boostType, out BoostUIElement element))
        {
            element.remainingTime = duration;
        }
    }

    private void CreateNewBoostUI(BoostType boostType, float duration)
    {
        GameObject newBoostUI = Instantiate(boostUIPrefab, boostContainer);
        Image iconImage = newBoostUI.GetComponentInChildren<Image>();

        RectTransform rectTransform = iconImage.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(50, 50);

        iconImage.preserveAspect = true;
        iconImage.color = new Color(1, 1, 1, 1);

        BoostUIElement uiElement = new BoostUIElement
        {
            uiPanel = newBoostUI,
            boostIcon = iconImage,
            timerText = newBoostUI.GetComponentInChildren<TextMeshProUGUI>(),
            remainingTime = duration,
            boostType = boostType
        };

        if (uiElement.boostIcon != null)
        {
            uiElement.boostIcon.sprite = GetBoostIcon(boostType);
        }

        activeBoosts.Add(boostType, uiElement);
        UpdateBoostTimer(uiElement);
    }

    private void RemoveBoost(BoostType boostType)
    {
        if (activeBoosts.TryGetValue(boostType, out BoostUIElement uiElement))
        {
            OnBoostExpired(boostType);
            Destroy(uiElement.uiPanel);
            activeBoosts.Remove(boostType);
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
                RectTransform rectTransform = boost.uiPanel.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(index * spacing, 0);
                index++;
            }
        }
    }

    private Sprite GetBoostIcon(BoostType boostType)
    {
        return boostType switch
        {
            BoostType.SpeedBoost => speedBoostIcon,
            BoostType.FireRateBoost => fireRateBoostIcon,
            BoostType.DualWeapons => dualWeaponsIcon,
            _ => null
        };
    }

    public bool IsBoostActive(BoostType boostType)
    {
        return activeBoosts.ContainsKey(boostType);
    }

    public float GetRemainingTime(BoostType boostType)
    {
        if (activeBoosts.TryGetValue(boostType, out BoostUIElement element))
        {
            return element.remainingTime;
        }
        return 0f;
    }

    public void OnBoostExpired(BoostType boostType)
    {
        if (boostType == BoostType.SpeedBoost)
        {
            AirplaneController airplaneController = FindObjectOfType<AirplaneController>();
            if (airplaneController != null)
            {
                airplaneController.ResetMoveSpeed();
            }
        }

        else if (boostType == BoostType.FireRateBoost)
        {
            WeaponSystem weaponSystem = FindObjectOfType<WeaponSystem>();
            if (weaponSystem != null)
            {
                weaponSystem.ResetFireRate();
            }
        }
    }
}

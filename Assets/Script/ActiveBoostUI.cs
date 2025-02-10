using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ActiveBoostUI : MonoBehaviour
{
    public enum BoostType
    {
        SpeedBoost,
        FireRateBoost,
        DualWeapons
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
        Debug.Log($"Adding boost: {boostType}, Icon: {GetBoostIcon(boostType)}");
        // Om boost redan finns, återställ bara tiden
        if (activeBoosts.ContainsKey(boostType))
        {
            RefreshBoostDuration(boostType, duration);
            return;
        }

        // Skapa nytt UI element
        CreateNewBoostUI(boostType, duration);
        UpdateUILayout();
    }

    private void RefreshBoostDuration(BoostType boostType, float duration)
    {
        if (activeBoosts.TryGetValue(boostType, out BoostUIElement element))
        {
            element.remainingTime = duration;
            // Spela eventuell refresh animation här
        }
    }

    private void CreateNewBoostUI(BoostType boostType, float duration)
    {
        GameObject newBoostUI = Instantiate(boostUIPrefab, boostContainer);
        Image iconImage = newBoostUI.GetComponentInChildren<Image>();

        // Justera storlek
        RectTransform rectTransform = iconImage.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(50, 50); // Mindre storlek, justera dessa värden

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
    }

    private void RemoveBoost(BoostType boostType)
    {
        if (activeBoosts.TryGetValue(boostType, out BoostUIElement uiElement))
        {
            // Spela eventuell försvinnande-animation här
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
        Sprite icon = boostType switch
        {
            BoostType.SpeedBoost => speedBoostIcon,
            BoostType.FireRateBoost => fireRateBoostIcon,
            BoostType.DualWeapons => dualWeaponsIcon,
            _ => null
        };

        Debug.Log($"Getting icon for {boostType}. Icon name: {icon?.name}, Icon null: {icon == null}");
        return icon;
    }

    // Public metod för att kolla om en boost är aktiv
    public bool IsBoostActive(BoostType boostType)
    {
        return activeBoosts.ContainsKey(boostType);
    }

    // Public metod för att få återstående tid för en boost
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
    }


}

//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using System.Collections.Generic;

//public class ActiveBoostUI : MonoBehaviour
//{
//    [System.Serializable]
//    public class BoostUIElement
//    {
//        public GameObject uiPanel;
//        public Image boostIcon;
//        public TextMeshProUGUI timerText;
//        public float remainingTime;
//    }

//    [Header("UI References")]
//    [SerializeField] private GameObject boostUIPrefab;
//    [SerializeField] private Transform boostContainer;
//    [SerializeField] private float spacing = 60f;

//    [Header("Boost Icons")]
//    [SerializeField] private Sprite healthBoostIcon;
//    [SerializeField] private Sprite speedBoostIcon;
//    [SerializeField] private Sprite shieldBoostIcon;
//    [SerializeField] private Sprite fireRateBoostIcon;
//    [SerializeField] private Sprite dualWeaponsIcon;

//    private Dictionary<string, BoostUIElement> activeBoosts = new Dictionary<string, BoostUIElement>();

//    public static ActiveBoostUI Instance { get; private set; }

//    private void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//            Debug.Log("ActiveBoostUI initialized");
//        }
//        else
//        {
//            Debug.LogWarning("Multiple ActiveBoostUI instances found!");
//            Destroy(gameObject);
//        }
//    }

//    private void Update()
//    {
//        List<string> boostsToRemove = new List<string>();

//        foreach (var boost in activeBoosts)
//        {
//            boost.Value.remainingTime -= Time.deltaTime;

//            if (boost.Value.remainingTime <= 0)
//            {
//                boostsToRemove.Add(boost.Key);
//            }
//            else
//            {
//                boost.Value.timerText.text = boost.Value.remainingTime.ToString("F1");
//            }
//        }

//        foreach (var boostName in boostsToRemove)
//        {
//            RemoveBoost(boostName);
//        }
//    }

//    public void AddBoost(string boostName, Sprite boostIcon, float duration)
//    {
//        if (activeBoosts.ContainsKey(boostName))
//        {
//            // Om boost redan finns, återställ bara tiden
//            activeBoosts[boostName].remainingTime = duration;
//            return;
//        }

//        // Skapa nytt UI element
//        GameObject newBoostUI = Instantiate(boostUIPrefab, boostContainer);

//        BoostUIElement uiElement = new BoostUIElement
//        {
//            uiPanel = newBoostUI,
//            boostIcon = newBoostUI.GetComponentInChildren<Image>(),
//            timerText = newBoostUI.GetComponentInChildren<TextMeshProUGUI>(),
//            remainingTime = duration
//        };

//        // Sätt ikon
//        if (uiElement.boostIcon != null && boostIcon != null)
//        {
//            uiElement.boostIcon.sprite = boostIcon;
//        }

//        activeBoosts.Add(boostName, uiElement);
//        UpdateUILayout();
//    }

//    private void RemoveBoost(string boostName)
//    {
//        if (activeBoosts.TryGetValue(boostName, out BoostUIElement uiElement))
//        {
//            Destroy(uiElement.uiPanel);
//            activeBoosts.Remove(boostName);
//            UpdateUILayout();
//        }
//    }

//    private void UpdateUILayout()
//    {
//        int index = 0;
//        foreach (var boost in activeBoosts.Values)
//        {
//            if (boost.uiPanel != null)
//            {
//                boost.uiPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(index * spacing, 0);
//                index++;
//            }
//        }
//    }

//    public Sprite GetBoostIcon(string boostType)
//    {
//        return boostType switch
//        {
//            "HealthBoost" => healthBoostIcon,
//            "SpeedBoost" => speedBoostIcon,
//            "ShieldBoost" => shieldBoostIcon,
//            "FireRateBoost" => fireRateBoostIcon,
//            "DualWeapons" => dualWeaponsIcon,
//            _ => null
//        };
//    }
//}
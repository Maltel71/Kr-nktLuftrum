using UnityEngine;

public class BoostPickup : MonoBehaviour
{
    public enum BoostType
    {
        HealthBoost,        // Instant effect
        SpeedBoost,         // Timed effect
        ShieldBoost,        // Instant effect
        FireRateBoost,      // Timed effect
        DualWeapons,        // Timed effect
        Flare,              // Numbers of flares
        Missile,            // Numbers of missiles
        Bomb,               // Numbers om Bombs
    }

    [Header("Boost Settings")]
    [SerializeField] private BoostType boostType;
    [SerializeField] private Color boostColor = Color.white;
    [SerializeField] private Sprite boostIcon;

    [Header("Visual Settings")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;

    [Header("Boost Values")]
    [SerializeField] private float healthAmount = 50f;        // Hur mycket hälsa som återställs
    [SerializeField] private float speedBoostAmount = 1.5f;   // Multiplicerar hastigheten med detta värde
    [SerializeField] private float shieldAmount = 100f;       // Hur mycket sköld som återställs
    [SerializeField] private float fireRateMultiplier = 2f;   // Multiplicerar fire rate med detta värde
    [SerializeField] private float boostDuration = 10f;       // Hur länge boost effekten varar


    private Vector3 startPosition;
    private float timeSinceSpawn;
    private bool isPickedUp = false;
    private bool isDestroyed = false;
    private Renderer objectRenderer;

    private void Start()
    {
        startPosition = transform.position;
        objectRenderer = GetComponent<Renderer>();
        InitializeVisuals();
    }

    private void InitializeVisuals()
    {
        if (objectRenderer != null)
        {
            objectRenderer.material.color = boostColor;
        }
    }

    private void Update()
    {
        if (isPickedUp) return;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        timeSinceSpawn += Time.deltaTime;
        float newY = startPosition.y + Mathf.Sin(timeSinceSpawn * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || isPickedUp || isDestroyed) return;

        Debug.Log($"Boost kollision med: {other.gameObject.name}");
        isPickedUp = true;
        isDestroyed = true;

        HandleBoostPickup(other.gameObject);
    }

    private void HandleBoostPickup(GameObject player)
    {
        // Visa meddelande
        var messageSystem = FindObjectOfType<GameMessageSystem>();
        if (messageSystem != null)
        {
            string message = $"{GetBoostMessage()} ";
            messageSystem.ShowBoostMessage(message);
        }

        // Lägg till UI-ikon endast för tidsbaserade boosts
        if (IsTimedBoost(boostType))
        {
            var boostUIType = ConvertToActiveBoostType(boostType);
            ActiveBoostUI.Instance?.AddBoost(boostUIType, boostDuration);
        }

        // Spela ljud
        AudioManager.Instance?.PlayBoostSound();

        // Applicera boost effekt
        ApplyBoostEffect(player);

        // Förstör objektet efter att alla effekter har applicerats
        Destroy(gameObject);
    }

    private bool IsTimedBoost(BoostType type)
    {
        return type == BoostType.SpeedBoost ||
               type == BoostType.FireRateBoost ||
               type == BoostType.DualWeapons;
    }

    private ActiveBoostUI.BoostType ConvertToActiveBoostType(BoostType type)
    {
        return type switch
        {
            BoostType.SpeedBoost => ActiveBoostUI.BoostType.SpeedBoost,
            BoostType.FireRateBoost => ActiveBoostUI.BoostType.FireRateBoost,
            BoostType.DualWeapons => ActiveBoostUI.BoostType.DualWeapons,
            _ => throw new System.ArgumentException("Invalid timed boost type")
        };
    }

    private string GetBoostMessage() => boostType switch
    {
        BoostType.HealthBoost => "Health Restored",
        BoostType.SpeedBoost => "Speed Boost",
        BoostType.ShieldBoost => "Shield Restored",
        BoostType.FireRateBoost => "Fire Rate Boost",
        BoostType.DualWeapons => "Dual Weapons",
        BoostType.Flare => "Flares Added",
        BoostType.Missile => "Missile Added",
        BoostType.Bomb =>"Bomb Added",
        _ => "Boost"
    };

    private void ApplyBoostEffect(GameObject player)
    {
        var planeHealth = player.GetComponent<PlaneHealthSystem>();
        var airplaneController = player.GetComponent<AirplaneController>();
        var weaponSystem = player.GetComponent<WeaponSystem>();

        switch (boostType)
        {
            case BoostType.HealthBoost:
                planeHealth?.AddHealth(healthAmount);
                break;

            case BoostType.ShieldBoost:
                planeHealth?.ApplyShieldBoost(shieldAmount); // Uppdatera denna metod i PlaneHealthSystem
                break;

            case BoostType.SpeedBoost:
                if (airplaneController != null)
                {
                    Debug.Log($"Applying Speed Boost. Multiplier: {speedBoostAmount}, Duration: {boostDuration}");

                    if (ActiveBoostUI.Instance.IsBoostActive(ActiveBoostUI.BoostType.SpeedBoost))
                    {
                        float remainingTime = ActiveBoostUI.Instance.GetRemainingTime(ActiveBoostUI.BoostType.SpeedBoost);
                        Debug.Log($"Existing Speed Boost extended. Previous remaining time: {remainingTime}");
                        ActiveBoostUI.Instance.AddBoost(ActiveBoostUI.BoostType.SpeedBoost, remainingTime + boostDuration);
                    }
                    else
                    {
                        Debug.Log("New Speed Boost started");
                        airplaneController.StartCoroutine(airplaneController.ApplySpeedBoost(speedBoostAmount, boostDuration));
                        ActiveBoostUI.Instance.AddBoost(ActiveBoostUI.BoostType.SpeedBoost, boostDuration);
                    }
                }
                break;

            case BoostType.FireRateBoost:
                if (weaponSystem != null)
                {
                    StartCoroutine(weaponSystem.ApplyFireRateBoost(fireRateMultiplier, boostDuration));
                }
                break;

            case BoostType.DualWeapons:
                if (weaponSystem != null)
                {
                    StartCoroutine(weaponSystem.EnableDualWeapons(boostDuration));
                }
                break;

            case BoostType.Flare:
                if (airplaneController != null)
                {
                    airplaneController.AddFlares(1); // Lägg till 1 flare
                }
                break;

            case BoostType.Missile:
                if (airplaneController != null)
                {
                    airplaneController.AddMissiles(1); // Lägg till 1 missile
                }
                break;

            case BoostType.Bomb:
                if (airplaneController != null)
                {
                    airplaneController.AddBombs(1); // Lägg till 1 Bomb
                }
                break;
        }
    }
    private void OnDestroy()
    {
        // Extra säkerhet för att förhindra eventuella race conditions
        isDestroyed = true;
        isPickedUp = true;
    }

}


//using UnityEngine;

//public class BoostPickup : MonoBehaviour
//{
//    public enum BoostType
//    {
//        HealthBoost,
//        SpeedBoost,
//        ShieldBoost,
//        FireRateBoost,
//        DualWeapons
//    }

//    [Header("Boost Settings")]
//    [SerializeField] private BoostType boostType;
//    [SerializeField] private Color boostColor = Color.white;
//    [SerializeField] private Sprite boostIcon;

//    [Header("Visual Settings")]
//    [SerializeField] private float rotationSpeed = 50f;
//    [SerializeField] private float bobSpeed = 2f;
//    [SerializeField] private float bobHeight = 0.5f;

//    [Header("Boost Values")]
//    [SerializeField] private float healthAmount = 50f;
//    [SerializeField] private float speedBoostAmount = 1.5f;
//    [SerializeField] private float boostDuration = 10f;

//    private Vector3 startPosition;
//    private float timeSinceSpawn;
//    private bool isPickedUp = false;
//    private Renderer objectRenderer;

//    private void Start()
//    {
//        startPosition = transform.position;
//        objectRenderer = GetComponent<Renderer>();
//        InitializeVisuals();
//    }

//    private void InitializeVisuals()
//    {
//        if (objectRenderer != null)
//        {
//            objectRenderer.material.color = boostColor;
//        }
//    }

//    private void Update()
//    {
//        if (isPickedUp) return;
//        UpdateVisuals();
//    }

//    private void UpdateVisuals()
//    {
//        // Rotera boost
//        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

//        // Bob upp och ner
//        timeSinceSpawn += Time.deltaTime;
//        float newY = startPosition.y + Mathf.Sin(timeSinceSpawn * bobSpeed) * bobHeight;
//        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        if (!other.CompareTag("Player") || isPickedUp) return;

//        isPickedUp = true;
//        HandleBoostPickup(other.gameObject);
//    }

//    // BoostPickup.cs - Uppdatera så det bara visar ett meddelande
//    private void HandleBoostPickup(GameObject player)
//    {
//        // Visa ett enda meddelande
//        var messageSystem = FindObjectOfType<GameMessageSystem>();
//        if (messageSystem != null)
//        {
//            string message = $"{GetBoostMessage()} ";
//            messageSystem.ShowBoostMessage(message);
//        }

//        // Lägg till boost i UI
//        ActiveBoostUI.Instance?.AddBoost(boostType.ToString(), boostIcon, boostDuration);

//        // Spela ljud
//        AudioManager.Instance?.PlayBoostSound();

//        // Applicera boost effekt
//        ApplyBoostEffect(player);

//        // Förstör pickup
//        Destroy(gameObject);
//    }

//    private string GetBoostMessage() => boostType switch
//    {
//        BoostType.HealthBoost => "Health",
//        BoostType.SpeedBoost => "Speed",
//        BoostType.ShieldBoost => "Shield",
//        BoostType.FireRateBoost => "Fire Rate",
//        BoostType.DualWeapons => "Dual Weapons",
//        _ => "Boost"
//    };

//    private void ApplyBoostEffect(GameObject player)
//    {
//        var planeHealth = player.GetComponent<PlaneHealthSystem>();
//        var airplaneController = player.GetComponent<AirplaneController>();
//        var weaponSystem = player.GetComponent<WeaponSystem>();

//        switch (boostType)
//        {
//            case BoostType.HealthBoost:
//                planeHealth?.AddHealth(healthAmount);
//                break;

//            case BoostType.SpeedBoost:
//                if (airplaneController != null)
//                {
//                    StartCoroutine(airplaneController.ApplySpeedBoost(speedBoostAmount, boostDuration));
//                }
//                break;

//            case BoostType.ShieldBoost:
//                planeHealth?.ApplyShieldBoost();
//                break;

//            case BoostType.FireRateBoost:
//                if (weaponSystem != null)
//                {
//                    StartCoroutine(weaponSystem.ApplyFireRateBoost(boostDuration));
//                }
//                break;

//            case BoostType.DualWeapons:
//                if (weaponSystem != null)
//                {
//                    StartCoroutine(weaponSystem.EnableDualWeapons(boostDuration));
//                }
//                break;
//        }
//    }
//}
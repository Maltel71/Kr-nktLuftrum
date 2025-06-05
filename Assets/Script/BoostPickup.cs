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
        Bomb,               // Numbers of bombs
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
    [SerializeField] private float healthAmount = 50f;
    [SerializeField] private float speedBoostAmount = 1.3f;   // MINSKAT från 1.5f till 1.3f (30% snabbare istället för 50%)
    [SerializeField] private float shieldAmount = 100f;
    [SerializeField] private float fireRateMultiplier = 2f;
    [SerializeField] private float boostDuration = 8f;        // MINSKAT från 10f till 8f sekunder       

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

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

        if (showDebugLogs)
            Debug.Log($"[BoostPickup] Kollision med: {other.gameObject.name}, Boost: {boostType}");

        isPickedUp = true;
        isDestroyed = true;

        HandleBoostPickup(other.gameObject);
    }

    private void HandleBoostPickup(GameObject player)
    {
        // Visa meddelande
        ShowBoostMessage();

        // Spela ljud
        AudioManager.Instance?.PlayBoostSound();

        // Applicera boost effekt
        ApplyBoostEffect(player);

        // Förstör objektet
        Destroy(gameObject);
    }

    private void ShowBoostMessage()
    {
        var messageSystem = FindObjectOfType<GameMessageSystem>();
        if (messageSystem != null)
        {
            string message = GetBoostMessage();
            messageSystem.ShowBoostMessage(message);

            if (showDebugLogs)
                Debug.Log($"[BoostPickup] Visar meddelande: {message}");
        }
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
        BoostType.Bomb => "Bomb Added",
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
                ApplyHealthBoost(planeHealth);
                break;

            case BoostType.ShieldBoost:
                ApplyShieldBoost(planeHealth);
                break;

            case BoostType.SpeedBoost:
                ApplySpeedBoost(airplaneController);
                break;

            case BoostType.FireRateBoost:
                ApplyFireRateBoost(weaponSystem);
                break;

            case BoostType.DualWeapons:
                ApplyDualWeaponsBoost(weaponSystem);
                break;

            case BoostType.Flare:
                ApplyFlareBoost(airplaneController);
                break;

            case BoostType.Missile:
                ApplyMissileBoost(airplaneController);
                break;

            case BoostType.Bomb:
                ApplyBombBoost(airplaneController);
                break;
        }
    }

    private void ApplyHealthBoost(PlaneHealthSystem planeHealth)
    {
        if (planeHealth != null)
        {
            planeHealth.AddHealth(healthAmount);
            if (showDebugLogs)
                Debug.Log($"[BoostPickup] Health boost applied: +{healthAmount}");
        }
    }

    private void ApplyShieldBoost(PlaneHealthSystem planeHealth)
    {
        if (planeHealth != null)
        {
            planeHealth.ApplyShieldBoost(shieldAmount);
            if (showDebugLogs)
                Debug.Log($"[BoostPickup] Shield boost applied: +{shieldAmount}");
        }
    }

    private void ApplySpeedBoost(AirplaneController airplaneController)
    {
        if (airplaneController == null) return;

        if (showDebugLogs)
            Debug.Log($"[BoostPickup] Speed Boost - Multiplier: {speedBoostAmount}, Duration: {boostDuration}");

        // FÖRENKLAD VERSION: Använd bara ActiveBoostUI systemet
        if (ActiveBoostUI.Instance != null)
        {
            // Kolla om speed boost redan är aktiv
            if (ActiveBoostUI.Instance.IsBoostActive(ActiveBoostUI.BoostType.SpeedBoost))
            {
                // NYTT: Sätt till MAX istället för att lägga ihop
                float remainingTime = ActiveBoostUI.Instance.GetRemainingTime(ActiveBoostUI.BoostType.SpeedBoost);
                float newDuration = Mathf.Max(remainingTime, boostDuration); // Max istället för +

                if (showDebugLogs)
                    Debug.Log($"[BoostPickup] Speed Boost already active ({remainingTime}s left) - setting to max: {newDuration}s");

                ActiveBoostUI.Instance.AddBoost(ActiveBoostUI.BoostType.SpeedBoost, newDuration);
            }
            else
            {
                // Starta ny speed boost
                if (showDebugLogs)
                    Debug.Log($"[BoostPickup] Starting new Speed Boost");

                // Starta både coroutine OCH UI
                airplaneController.StartCoroutine(airplaneController.ApplySpeedBoost(speedBoostAmount, boostDuration));
                ActiveBoostUI.Instance.AddBoost(ActiveBoostUI.BoostType.SpeedBoost, boostDuration);
            }
        }
        else
        {
            // Fallback om ActiveBoostUI saknas
            if (showDebugLogs)
                Debug.Log($"[BoostPickup] ActiveBoostUI missing - using fallback coroutine");

            airplaneController.StartCoroutine(airplaneController.ApplySpeedBoost(speedBoostAmount, boostDuration));
        }
    }

    private void ApplyFireRateBoost(WeaponSystem weaponSystem)
    {
        if (weaponSystem != null)
        {
            if (showDebugLogs)
                Debug.Log($"[BoostPickup] Fire Rate Boost - Multiplier: {fireRateMultiplier}, Duration: {boostDuration}");

            // Lägg till i UI med MAX-logik
            if (ActiveBoostUI.Instance != null)
            {
                if (ActiveBoostUI.Instance.IsBoostActive(ActiveBoostUI.BoostType.FireRateBoost))
                {
                    // NYTT: Max istället för att lägga ihop
                    float remainingTime = ActiveBoostUI.Instance.GetRemainingTime(ActiveBoostUI.BoostType.FireRateBoost);
                    float newDuration = Mathf.Max(remainingTime, boostDuration);

                    if (showDebugLogs)
                        Debug.Log($"[BoostPickup] Fire Rate already active ({remainingTime}s) - setting to max: {newDuration}s");

                    ActiveBoostUI.Instance.AddBoost(ActiveBoostUI.BoostType.FireRateBoost, newDuration);
                }
                else
                {
                    ActiveBoostUI.Instance.AddBoost(ActiveBoostUI.BoostType.FireRateBoost, boostDuration);
                    // Starta coroutine bara för nya boosts
                    StartCoroutine(weaponSystem.ApplyFireRateBoost(fireRateMultiplier, boostDuration));
                }
            }
            else
            {
                // Fallback
                StartCoroutine(weaponSystem.ApplyFireRateBoost(fireRateMultiplier, boostDuration));
            }
        }
    }

    private void ApplyDualWeaponsBoost(WeaponSystem weaponSystem)
    {
        if (weaponSystem != null)
        {
            if (showDebugLogs)
                Debug.Log($"[BoostPickup] Dual Weapons Boost - Duration: {boostDuration}");

            // Lägg till i UI med MAX-logik
            if (ActiveBoostUI.Instance != null)
            {
                if (ActiveBoostUI.Instance.IsBoostActive(ActiveBoostUI.BoostType.DualWeapons))
                {
                    // NYTT: Max istället för att lägga ihop
                    float remainingTime = ActiveBoostUI.Instance.GetRemainingTime(ActiveBoostUI.BoostType.DualWeapons);
                    float newDuration = Mathf.Max(remainingTime, boostDuration);

                    if (showDebugLogs)
                        Debug.Log($"[BoostPickup] Dual Weapons already active ({remainingTime}s) - setting to max: {newDuration}s");

                    ActiveBoostUI.Instance.AddBoost(ActiveBoostUI.BoostType.DualWeapons, newDuration);
                }
                else
                {
                    ActiveBoostUI.Instance.AddBoost(ActiveBoostUI.BoostType.DualWeapons, boostDuration);
                    // Aktivera dual weapons bara för nya boosts
                    weaponSystem.EnableDualWeapons(boostDuration);
                }
            }
            else
            {
                // Fallback
                weaponSystem.EnableDualWeapons(boostDuration);
            }
        }
    }

    private void ApplyFlareBoost(AirplaneController airplaneController)
    {
        if (airplaneController != null)
        {
            airplaneController.AddFlares(1);
            if (showDebugLogs)
                Debug.Log($"[BoostPickup] Added 1 flare");
        }
    }

    private void ApplyMissileBoost(AirplaneController airplaneController)
    {
        if (airplaneController != null)
        {
            airplaneController.AddMissiles(1);
            if (showDebugLogs)
                Debug.Log($"[BoostPickup] Added 1 missile");
        }
    }

    private void ApplyBombBoost(AirplaneController airplaneController)
    {
        if (airplaneController != null)
        {
            airplaneController.AddBombs(1);
            if (showDebugLogs)
                Debug.Log($"[BoostPickup] Added 1 bomb");
        }
    }

    private void OnDestroy()
    {
        // Extra säkerhet för att förhindra eventuella race conditions
        isDestroyed = true;
        isPickedUp = true;
    }

    // DEBUG METODER
    [ContextMenu("Test Speed Boost")]
    public void TestSpeedBoost()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var airplaneController = player.GetComponent<AirplaneController>();
            if (airplaneController != null)
            {
                Debug.Log("[BoostPickup] Testing Speed Boost...");
                ApplySpeedBoost(airplaneController);
            }
        }
    }

    [ContextMenu("Show Current Config")]
    public void ShowCurrentConfig()
    {
        Debug.Log($"[BoostPickup] === BOOST CONFIG ===");
        Debug.Log($"Type: {boostType}");
        Debug.Log($"Speed Multiplier: {speedBoostAmount}");
        Debug.Log($"Duration: {boostDuration}");
        Debug.Log($"Health Amount: {healthAmount}");
        Debug.Log($"Shield Amount: {shieldAmount}");
    }
}
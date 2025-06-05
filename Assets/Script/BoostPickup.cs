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
    [SerializeField] private float speedBoostAmount = 1.3f;
    [SerializeField] private float shieldAmount = 100f;
    [SerializeField] private float fireRateMultiplier = 2f;
    [SerializeField] private float boostDuration = 8f;

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
        }
    }

    private void ApplyShieldBoost(PlaneHealthSystem planeHealth)
    {
        if (planeHealth != null)
        {
            planeHealth.ApplyShieldBoost(shieldAmount);
        }
    }

    private void ApplySpeedBoost(AirplaneController airplaneController)
    {
        if (airplaneController == null) return;

        if (ActiveBoostUI.Instance != null)
        {
            if (ActiveBoostUI.Instance.IsBoostActive(ActiveBoostUI.BoostType.SpeedBoost))
            {
                // Sätt till max istället för att lägga ihop
                float remainingTime = ActiveBoostUI.Instance.GetRemainingTime(ActiveBoostUI.BoostType.SpeedBoost);
                float newDuration = Mathf.Max(remainingTime, boostDuration);
                ActiveBoostUI.Instance.AddBoost(ActiveBoostUI.BoostType.SpeedBoost, newDuration);
            }
            else
            {
                // Starta ny speed boost
                airplaneController.StartCoroutine(airplaneController.ApplySpeedBoost(speedBoostAmount, boostDuration));
                ActiveBoostUI.Instance.AddBoost(ActiveBoostUI.BoostType.SpeedBoost, boostDuration);
            }
        }
        else
        {
            // Fallback om ActiveBoostUI saknas
            airplaneController.StartCoroutine(airplaneController.ApplySpeedBoost(speedBoostAmount, boostDuration));
        }
    }

    private void ApplyFireRateBoost(WeaponSystem weaponSystem)
    {
        if (weaponSystem != null)
        {
            if (ActiveBoostUI.Instance != null)
            {
                if (ActiveBoostUI.Instance.IsBoostActive(ActiveBoostUI.BoostType.FireRateBoost))
                {
                    // Max istället för att lägga ihop
                    float remainingTime = ActiveBoostUI.Instance.GetRemainingTime(ActiveBoostUI.BoostType.FireRateBoost);
                    float newDuration = Mathf.Max(remainingTime, boostDuration);
                    ActiveBoostUI.Instance.AddBoost(ActiveBoostUI.BoostType.FireRateBoost, newDuration);
                }
                else
                {
                    ActiveBoostUI.Instance.AddBoost(ActiveBoostUI.BoostType.FireRateBoost, boostDuration);
                    StartCoroutine(weaponSystem.ApplyFireRateBoost(fireRateMultiplier, boostDuration));
                }
            }
            else
            {
                StartCoroutine(weaponSystem.ApplyFireRateBoost(fireRateMultiplier, boostDuration));
            }
        }
    }

    private void ApplyDualWeaponsBoost(WeaponSystem weaponSystem)
    {
        if (weaponSystem != null)
        {
            if (ActiveBoostUI.Instance != null)
            {
                if (ActiveBoostUI.Instance.IsBoostActive(ActiveBoostUI.BoostType.DualWeapons))
                {
                    // Max istället för att lägga ihop
                    float remainingTime = ActiveBoostUI.Instance.GetRemainingTime(ActiveBoostUI.BoostType.DualWeapons);
                    float newDuration = Mathf.Max(remainingTime, boostDuration);
                    ActiveBoostUI.Instance.AddBoost(ActiveBoostUI.BoostType.DualWeapons, newDuration);
                }
                else
                {
                    ActiveBoostUI.Instance.AddBoost(ActiveBoostUI.BoostType.DualWeapons, boostDuration);
                    weaponSystem.EnableDualWeapons(boostDuration);
                }
            }
            else
            {
                weaponSystem.EnableDualWeapons(boostDuration);
            }
        }
    }

    private void ApplyFlareBoost(AirplaneController airplaneController)
    {
        if (airplaneController != null)
        {
            airplaneController.AddFlares(1);
        }
    }

    private void ApplyMissileBoost(AirplaneController airplaneController)
    {
        if (airplaneController != null)
        {
            airplaneController.AddMissiles(1);
        }
    }

    private void ApplyBombBoost(AirplaneController airplaneController)
    {
        if (airplaneController != null)
        {
            airplaneController.AddBombs(1);
        }
    }

    private void OnDestroy()
    {
        isDestroyed = true;
        isPickedUp = true;
    }
}
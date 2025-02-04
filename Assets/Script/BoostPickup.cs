using UnityEngine;

public class BoostPickup : MonoBehaviour
{
    public enum BoostType
    {
        HealthBoost,
        SpeedBoost,
        ShieldBoost,
        FireRateBoost,
        DualWeapons
    }

    [Header("Boost Settings")]
    [SerializeField] private BoostType boostType;
    [SerializeField] private Color boostColor;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;

    [Header("Boost Values")]
    [SerializeField] private float healthAmount = 50f;
    [SerializeField] private float speedBoostAmount = 1.5f;
    [SerializeField] private float boostDuration = 10f;

    private Vector3 startPosition;
    private float timeSinceSpawn;
    private bool isPickedUp = false;

    private void Start()
    {
        startPosition = transform.position;

        // Hämta boost-objektets renderer och ändra färg baserat på boost-typ
        if (TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = boostColor;
        }
    }

    private void Update()
    {
        if (isPickedUp) return;

        // Rotera boost
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Bob upp och ner
        timeSinceSpawn += Time.deltaTime;
        float newY = startPosition.y + Mathf.Sin(timeSinceSpawn * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPickedUp)
        {
            isPickedUp = true;

            // Visa meddelande
            var messageSystem = FindObjectOfType<GameMessageSystem>();
            if (messageSystem != null)
            {
                string message = GetBoostMessage();
                messageSystem.ShowBoostMessage(message);
            }

            // Spela ljud
            AudioManager.Instance?.PlayBoostSound();

            // Applicera boost
            ApplyBoost(other.gameObject);

            // Förstör objektet
            Destroy(gameObject);
        }
    }

    private string GetBoostMessage()
    {
        return boostType switch
        {
            BoostType.HealthBoost => "Health Boost!",
            BoostType.SpeedBoost => "Speed Boost!",
            BoostType.ShieldBoost => "Shield Boost!",
            BoostType.FireRateBoost => "Fire Rate Boost!",
            BoostType.DualWeapons => "Dual Weapons!",
            _ => "Boost Picked Up!"
        };
    }

    private void ApplyBoost(GameObject player)
    {
        var planeHealth = player.GetComponent<PlaneHealthSystem>();
        var airplaneController = player.GetComponent<AirplaneController>();
        var weaponSystem = player.GetComponent<WeaponSystem>();

        switch (boostType)
        {
            case BoostType.HealthBoost:
                planeHealth?.AddHealth(healthAmount);
                break;

            case BoostType.SpeedBoost:
                if (airplaneController != null)
                {
                    StartCoroutine(airplaneController.ApplySpeedBoost(speedBoostAmount, boostDuration));
                }
                break;

            case BoostType.ShieldBoost:
                planeHealth?.ApplyShieldBoost();
                break;

            case BoostType.FireRateBoost:
                if (weaponSystem != null)
                {
                    StartCoroutine(weaponSystem.ApplyFireRateBoost(boostDuration));
                }
                break;

            case BoostType.DualWeapons:
                if (weaponSystem != null)
                {
                    StartCoroutine(weaponSystem.EnableDualWeapons(boostDuration));
                }
                break;
        }
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

//    private void Start()
//    {
//        startPosition = transform.position;
//    }

//    private void Update()
//    {
//        if (isPickedUp) return;

//        // Rotera boost
//        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

//        // Bob upp och ner
//        timeSinceSpawn += Time.deltaTime;
//        float newY = startPosition.y + Mathf.Sin(timeSinceSpawn * bobSpeed) * bobHeight;
//        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Player") && !isPickedUp)
//        {
//            isPickedUp = true;

//            // Visa meddelande
//            var messageSystem = FindObjectOfType<GameMessageSystem>();
//            if (messageSystem != null)
//            {
//                string message = GetBoostMessage();
//                messageSystem.ShowBoostMessage(message);
//            }

//            // Spela ljud
//            AudioManager.Instance?.PlayBoostSound();

//            // Applicera boost
//            ApplyBoost(other.gameObject);

//            // Förstör objektet
//            Destroy(gameObject);
//        }
//    }

//    private string GetBoostMessage()
//    {
//        return boostType switch
//        {
//            BoostType.HealthBoost => "Health Boost!",
//            BoostType.SpeedBoost => "Speed Boost!",
//            BoostType.ShieldBoost => "Shield Boost!",
//            BoostType.FireRateBoost => "Fire Rate Boost!",
//            BoostType.DualWeapons => "Dual Weapons!",
//            _ => "Boost Picked Up!"
//        };
//    }

//    private void ApplyBoost(GameObject player)
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
using UnityEngine;

public class BoostPickup : MonoBehaviour
{
    public enum BoostType
    {
        Health,
        Weapons,
        Bombs,
        Shield
    }

    [Header("Boost Settings")]
    [SerializeField] private BoostType boostType;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private float lifeTime = 10f;

    private Vector3 startPosition;
    private float timeSinceSpawn;

    private void Start()
    {
        startPosition = transform.position;
        if (lifeTime > 0)
        {
            Destroy(gameObject, lifeTime);
        }
    }

    private void Update()
    {
        // Rotera powerup
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Bob up and down
        timeSinceSpawn += Time.deltaTime;
        float newY = startPosition.y + Mathf.Sin(timeSinceSpawn * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlaneHealthSystem>(out var healthSystem))
            {
                ApplyBoost(healthSystem);
            }
            Destroy(gameObject);
        }
    }

    private void ApplyBoost(PlaneHealthSystem healthSystem)
    {
        switch (boostType)
        {
            case BoostType.Health:
                healthSystem.RestoreAll();
                break;
            case BoostType.Shield:
                healthSystem.ApplyShieldBoost();
                break;
            case BoostType.Weapons:
                if (healthSystem.gameObject.TryGetComponent<WeaponSystem>(out var weaponSystem))
                {
                    weaponSystem.ApplyWeaponBoost();
                }
                break;
            case BoostType.Bombs:
                if (healthSystem.gameObject.TryGetComponent<WeaponSystem>(out var bombSystem))
                {
                    bombSystem.ApplyBombBoost();
                }
                break;
        }
    }
}
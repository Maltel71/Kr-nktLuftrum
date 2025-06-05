using UnityEngine;

public class BoostDropSystem : MonoBehaviour
{
    [System.Serializable]
    public class BoostDropConfig
    {
        public GameObject boostPrefab;
        [Range(0f, 1f)]
        public float dropChance = 0.2f;
    }

    [Header("Boost Prefabs")]
    [SerializeField] private BoostDropConfig[] boostConfigs;

    [Header("Drop Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float spreadRadius = 1f;

    public void TryDropBoost(Vector3 position)
    {
        foreach (var config in boostConfigs)
        {
            if (config.boostPrefab == null) continue;

            if (Random.value <= config.dropChance)
            {
                DropBoost(position, config.boostPrefab);
                //Debug.Log($"Droppar boost: {config.boostPrefab.name}");
                break;
            }
        }
    }

    private void DropBoost(Vector3 position, GameObject boostPrefab)
    {
        // Slumpa position inom en radie
        Vector2 randomCircle = Random.insideUnitCircle * spreadRadius;
        Vector3 dropPosition = position + new Vector3(randomCircle.x, 0, randomCircle.y);

        // Skapa boost-objektet
        GameObject boost = Instantiate(boostPrefab, dropPosition, Quaternion.identity);

        // Initiera BoostMover om det finns
        if (boost.TryGetComponent<BoostMover>(out var boostMover))
        {
            boostMover.Initialize(moveSpeed);
        }
        else
        {
            Debug.LogWarning($"Boost prefab saknar BoostMover-komponenten: {boostPrefab.name}");
        }
    }
}
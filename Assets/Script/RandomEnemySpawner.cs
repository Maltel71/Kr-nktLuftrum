using UnityEngine;

public class RandomEnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private int startingEnemyCount = 2;

    private void Start()
    {
        if (spawnOnStart)
        {
            for (int i = 0; i < startingEnemyCount; i++)
            {
                // Spawna på olika punkter
                SpawnEnemyAtPoint(i % enemyPrefabs.Length, i % spawnPoints.Length);
            }
        }
    }

    public GameObject SpawnEnemyAtPoint(int enemyIndex, int spawnPointIndex)
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            //Debug.LogError("No enemy prefabs defined!");
            return null;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            //Debug.LogError("No spawn points defined!");
            return null;
        }

        GameObject enemyPrefab = enemyPrefabs[enemyIndex];
        Transform spawnPoint = spawnPoints[spawnPointIndex];

        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        //Debug.Log($"Spawned enemy {enemyIndex} at spawnpoint {spawnPointIndex}: {spawnPoint.position}");

        RandomEnemyMovement movement = enemy.GetComponent<RandomEnemyMovement>();
        if (movement != null)
        {
            movement.ForcePosition(spawnPoint.position);
        }

        return enemy;
    }
}
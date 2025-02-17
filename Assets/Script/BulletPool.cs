using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    private static BulletPool instance;
    public static BulletPool Instance => instance;

    [SerializeField] private GameObject playerBulletPrefab;
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private int poolSizePerType = 30;

    private List<GameObject> playerBulletPool;
    private List<GameObject> enemyBulletPool;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePools()
    {
        playerBulletPool = new List<GameObject>();
        enemyBulletPool = new List<GameObject>();

        for (int i = 0; i < poolSizePerType; i++)
        {
            CreateBullet(true);
            CreateBullet(false);
        }
    }

    private void CreateBullet(bool isPlayerBullet)
    {
        GameObject bullet = Instantiate(
            isPlayerBullet ? playerBulletPrefab : enemyBulletPrefab,
            transform
        );
        bullet.SetActive(false);

        if (isPlayerBullet)
            playerBulletPool.Add(bullet);
        else
            enemyBulletPool.Add(bullet);
    }

    public GameObject GetBullet(bool isPlayerBullet)
    {
        List<GameObject> pool = isPlayerBullet ? playerBulletPool : enemyBulletPool;

        // Hitta första inaktiva skottet
        foreach (GameObject bullet in pool)
        {
            if (!bullet.activeInHierarchy)
            {
                bullet.SetActive(true);
                return bullet;
            }
        }

        // Om alla skott är aktiva, skapa ett nytt
        GameObject newBullet = Instantiate(
            isPlayerBullet ? playerBulletPrefab : enemyBulletPrefab,
            transform
        );
        pool.Add(newBullet);
        return newBullet;
    }

    public void ReturnBulletToPool(GameObject bullet)
    {
        bullet.SetActive(false);
        bullet.transform.position = transform.position;
    }
}
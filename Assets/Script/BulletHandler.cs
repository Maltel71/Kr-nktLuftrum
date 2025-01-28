using UnityEngine;

public class BulletHandler : MonoBehaviour
{
    [SerializeField] private bool isEnemyProjectile;
    [SerializeField] private float damage = 10f;
    private bool hasCollided;

    public void SetAsEnemyProjectile(float damageAmount)
    {
        isEnemyProjectile = true;
        damage = damageAmount;
    }

    private void OnTriggerEnter(Collider other)
    {        if (hasCollided) return;

        // Handle player hitting enemy
        if (!isEnemyProjectile && other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<EnemyHealth>(out var enemyHealth))
            {
                enemyHealth.TakeDamage(damage);
                hasCollided = true;
                Destroy(gameObject);
            }
        }
        // Handle enemy hitting player
        else if (isEnemyProjectile && other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlaneHealthSystem>(out var playerHealth))
            {
                playerHealth.TakeDamage(damage);
                hasCollided = true;
                Destroy(gameObject);
            }
        }
    }
}
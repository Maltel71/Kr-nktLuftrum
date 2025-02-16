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

    private void OnCollisionEnter(Collision collision)
    {
        if (hasCollided) return;
        hasCollided = true;

        if (isEnemyProjectile && collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent<PlaneHealthSystem>(out var health))
            {
                health.TakeDamage(damage);
            }
        }
        Destroy(gameObject);
    }
}
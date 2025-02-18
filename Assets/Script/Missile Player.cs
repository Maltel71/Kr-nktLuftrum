using UnityEngine;

public class MissilePlayer : MonoBehaviour
{
    [Header("Missile Settings")]
    [SerializeField] private float speed = 50f;
    [SerializeField] private float damage = 50f;
    [SerializeField] private float missileLifetime = 3f;

    private Rigidbody rb;
    private float fixedHeight;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        fixedHeight = transform.position.y;

        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
            rb.useGravity = false;
            rb.freezeRotation = true;
        }

        Destroy(gameObject, missileLifetime);
    }

    private void Update()
    {
        if (rb != null)
        {
            Vector3 pos = transform.position;
            pos.y = fixedHeight;
            transform.position = pos;
            rb.linearVelocity = transform.forward * speed;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void HandleCollision(GameObject hitObject)
    {
        if (hitObject.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = hitObject.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // Kolla om det är en boss
                bool isBoss = enemyHealth.GetComponent<EnemyBoss>() != null;

                enemyHealth.TakeDamage(damage);
                enemyHealth.StartSmokeEffects();

                // Skapa explosion om det är en boss
                if (isBoss)
                {
                    GameObject explosion = ExplosionPool.Instance.GetExplosion(ExplosionType.Small);
                    explosion.transform.position = transform.position;
                    ExplosionPool.Instance.ReturnExplosionToPool(explosion, 2f);
                }
            }

            AudioManager.Instance?.PlayBombSound(BombSoundType.Explosion);
        }

        Destroy(gameObject);
    }

    //private void HandleCollision(GameObject hitObject)
    //{
    //    if (hitObject.CompareTag("Enemy"))
    //    {
    //        EnemyHealth enemyHealth = hitObject.GetComponent<EnemyHealth>();
    //        if (enemyHealth != null)
    //        {
    //            enemyHealth.TakeDamage(damage);

    //            // Tvinga fram röksystem
    //            enemyHealth.StartSmokeEffects();
    //        }

    //        // Explosionshantering
    //        //GameObject explosion = ExplosionPool.Instance.GetExplosion(ExplosionType.Small);
    //        //explosion.transform.position = transform.position;
    //        //ExplosionPool.Instance.ReturnExplosionToPool(explosion, 2f);

    //        AudioManager.Instance?.PlayBombSound(BombSoundType.Explosion);
    //    }

    //    Destroy(gameObject);
    //}

    //private void HandleCollision(GameObject hitObject)
    //{
    //    if (hitObject.CompareTag("Enemy"))
    //    {
    //        if (hitObject.TryGetComponent<EnemyHealth>(out var enemyHealth))
    //        {
    //            enemyHealth.TakeDamage(damage);

    //        }

    //        // Använd explosionspoolen för missilexplosion 
    //        // Ta bort dessa om enemyhealth ska hantera exolosionerna
    //        GameObject explosion = ExplosionPool.Instance.GetExplosion(ExplosionType.Small);
    //        explosion.transform.position = transform.position;
    //        ExplosionPool.Instance.ReturnExplosionToPool(explosion, 2f);

    //        AudioManager.Instance?.PlayBombSound(BombSoundType.Explosion);
    //    }

    //    Destroy(gameObject);
    //}
}

//using UnityEngine;

//public class MissilePlayer : MonoBehaviour
//{
//    [Header("Missile Settings")]
//    [SerializeField] private float speed = 50f;
//    [SerializeField] private float damage = 50f;
//    [SerializeField] private float missileLifetime = 3f;

//    [Header("Effects")]
//    [SerializeField] private GameObject explosionEffectPrefab;

//    private Rigidbody rb;
//    private float fixedHeight;

//    private void Start()
//    {
//        rb = GetComponent<Rigidbody>();
//        // Spara startpositionen Y-värde
//        fixedHeight = transform.position.y;

//        if (rb != null)
//        {
//            rb.linearVelocity = transform.forward * speed;
//            rb.useGravity = false;
//            rb.freezeRotation = true;
//        }

//        Destroy(gameObject, missileLifetime);
//    }

//    private void Update()
//    {
//        // Behåll samma höjd som vi startade på
//        if (rb != null)
//        {
//            Vector3 pos = transform.position;
//            pos.y = fixedHeight;
//            transform.position = pos;

//            // Uppdatera hastigheten för att behålla konstant fart framåt
//            rb.linearVelocity = transform.forward * speed;

//            // Debug-ray för att se missilens bana
//            Debug.DrawRay(transform.position, transform.forward * 10f, Color.red);
//        }
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        Debug.Log($"OnTriggerEnter: Collided with {other.gameObject.name}, tag: {other.gameObject.tag}");
//        HandleCollision(other.gameObject);
//    }

//    private void OnCollisionEnter(Collision collision)
//    {
//        Debug.Log($"OnCollisionEnter: Collided with {collision.gameObject.name}, tag: {collision.gameObject.tag}");
//        HandleCollision(collision.gameObject);
//    }

//    private void HandleCollision(GameObject hitObject)
//    {
//        if (hitObject.CompareTag("Enemy"))
//        {
//            if (hitObject.TryGetComponent<EnemyHealth>(out var enemyHealth))
//            {
//                enemyHealth.TakeDamage(damage);
//                Debug.Log($"Enemy hit! Dealing {damage} damage");
//            }

//            if (explosionEffectPrefab != null)
//            {
//                Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
//            }

//            AudioManager.Instance?.PlayBombSound(BombSoundType.Explosion);
//        }

//        Destroy(gameObject);
//    }



//}
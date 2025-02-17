using UnityEngine;

public class BombHandler : MonoBehaviour
{
    [Header("Audio")]
    private AudioManager audioManager;

    [Header("Effects")]
    [SerializeField] private float explosionDuration = 2f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionForce = 500f;
    [SerializeField] private float explosionDamage = 50f;

    private CameraShake cameraShake;
    private ScoreManager scoreManager;
    private bool hasPlayedFallingSound = false;
    private bool hasExploded = false;

    private void Start()
    {
        audioManager = AudioManager.Instance;
        cameraShake = CameraShake.Instance;
        scoreManager = ScoreManager.Instance;
    }

    private void Update()
    {
        if (!hasPlayedFallingSound && GetComponent<Rigidbody>().linearVelocity.y < 0)
        {
            audioManager?.PlayBombSound(BombSoundType.Falling);
            hasPlayedFallingSound = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;

        if (other.CompareTag("Ground") || other.gameObject.name == "Ground")
        {
            HandleExplosion(transform.position);
        }

        if (other.CompareTag("BombTarget"))
        {
            if (scoreManager != null)
            {
                scoreManager.AddBombTargetPoints();
            }
            HandleExplosion(transform.position);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;

        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.name == "Ground")
        {
            HandleExplosion(collision.contacts[0].point);
        }

        if (collision.gameObject.CompareTag("BombTarget"))
        {
            scoreManager?.AddBombTargetPoints();
        }

        HandleExplosion(collision.contacts[0].point);
    }

    private void HandleExplosion(Vector3 position)
    {
        if (hasExploded) return;
        hasExploded = true;

        // Använd explosionspoolen
        GameObject explosionEffect = ExplosionPool.Instance.GetExplosion(ExplosionType.Large);
        explosionEffect.transform.position = position;
        ExplosionPool.Instance.ReturnExplosionToPool(explosionEffect, explosionDuration);

        Collider[] colliders = Physics.OverlapSphere(position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, position, explosionRadius, 1.0f, ForceMode.Impulse);
            }

            if (hit.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    float distance = Vector3.Distance(position, hit.transform.position);
                    float damageMultiplier = 1 - (distance / explosionRadius);
                    float damage = explosionDamage * damageMultiplier;
                    enemyHealth.TakeDamage(damage);
                }
            }
        }

        if (cameraShake != null)
        {
            cameraShake.ShakaCameraVidBomb();
        }

        audioManager?.PlayBombSound(BombSoundType.Explosion);
        Destroy(gameObject);
    }
}


//using UnityEngine;

//public class BombHandler : MonoBehaviour
//{
//    [Header("Audio")]
//    private AudioManager audioManager;

//    [Header("Effects")]
//    [SerializeField] private GameObject explosionEffectPrefab;
//    [SerializeField] private float explosionDuration = 2f;
//    [SerializeField] private float explosionRadius = 5f;
//    [SerializeField] private float explosionForce = 500f;
//    [SerializeField] private float explosionDamage = 50f;

//    private CameraShake cameraShake;
//    private ScoreManager scoreManager;
//    private bool hasPlayedFallingSound = false;
//    private bool hasExploded = false;

//    private void Start()
//    {
//        audioManager = AudioManager.Instance;
//        cameraShake = CameraShake.Instance;
//        scoreManager = ScoreManager.Instance;
//    }

//    private void Update()
//    {
//        if (!hasPlayedFallingSound && GetComponent<Rigidbody>().linearVelocity.y < 0)
//        {
//            // Uppdaterad ljudhantering
//            audioManager?.PlayBombSound(BombSoundType.Falling);
//            hasPlayedFallingSound = true;
//        }
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        if (hasExploded) return;

//        if (other.CompareTag("Ground") || other.gameObject.name == "Ground")
//        {
//            HandleExplosion(transform.position);
//        }

//        Debug.Log($"OnTriggerEnter: Collided with {other.gameObject.name}, tag: {other.gameObject.tag}");

//        if (other.CompareTag("BombTarget"))
//        {
//            Debug.Log("Hit BombTarget! Adding points...");
//            if (scoreManager != null)
//            {
//                scoreManager.AddBombTargetPoints();
//            }
//            HandleExplosion(transform.position);
//        }
//    }

//    private void OnCollisionEnter(Collision collision)
//    {
//        if (hasExploded) return;

//        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.name == "Ground")
//        {
//            HandleExplosion(collision.contacts[0].point);
//        }

//        Debug.Log($"Bomb collided with: {collision.gameObject.name}, Layer: {collision.gameObject.layer}, Tag: {collision.gameObject.tag}");

//        HandleExplosion(collision.contacts[0].point);

//        if (collision.gameObject.CompareTag("BombTarget"))
//        {
//            scoreManager?.AddBombTargetPoints();
//        }
//    }

//    private void HandleExplosion(Vector3 position)
//    {
//        if (hasExploded) return;
//        hasExploded = true;

//        if (explosionEffectPrefab != null)
//        {
//            GameObject explosionEffect = Instantiate(explosionEffectPrefab, position, Quaternion.identity);
//            Destroy(explosionEffect, explosionDuration);
//        }

//        Collider[] colliders = Physics.OverlapSphere(position, explosionRadius);
//        foreach (Collider hit in colliders)
//        {
//            Rigidbody rb = hit.GetComponent<Rigidbody>();
//            if (rb != null)
//            {
//                rb.AddExplosionForce(explosionForce, position, explosionRadius, 1.0f, ForceMode.Impulse);
//            }

//            if (hit.CompareTag("Enemy"))
//            {
//                EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
//                if (enemyHealth != null)
//                {
//                    float distance = Vector3.Distance(position, hit.transform.position);
//                    float damageMultiplier = 1 - (distance / explosionRadius);
//                    float damage = explosionDamage * damageMultiplier;
//                    enemyHealth.TakeDamage(damage);
//                }
//            }
//        }

//        if (cameraShake != null)
//        {
//            cameraShake.ShakaCameraVidBomb();
//        }

//        // Uppdaterad ljudhantering
//        audioManager?.PlayBombSound(BombSoundType.Explosion);

//        Destroy(gameObject);
//    }
//}


////using UnityEngine;

////public class BombHandler : MonoBehaviour
////{
////    [Header("Audio")]
////    private AudioManager audioManager;

////    [Header("Effects")]
////    [SerializeField] private GameObject explosionEffectPrefab;
////    [SerializeField] private float explosionDuration = 2f;
////    [SerializeField] private float explosionRadius = 5f;
////    [SerializeField] private float explosionForce = 500f;
////    [SerializeField] private float explosionDamage = 50f;

////    private CameraShake cameraShake;
////    private ScoreManager scoreManager;
////    private bool hasPlayedFallingSound = false;
////    private bool hasExploded = false;

////    private void Start()
////    {
////        audioManager = AudioManager.Instance;
////        cameraShake = CameraShake.Instance;
////        scoreManager = ScoreManager.Instance;
////    }

////    private void Update()
////    {
////        if (!hasPlayedFallingSound && GetComponent<Rigidbody>().linearVelocity.y < 0)
////        {
////            audioManager?.PlayBombFallingSound();
////            hasPlayedFallingSound = true;
////        }
////    }

////    private void OnTriggerEnter(Collider other)
////    {
////        if (hasExploded) return;

////        if (other.CompareTag("Ground") || other.gameObject.name == "Ground")
////        {
////            HandleExplosion(transform.position);
////        }

////        Debug.Log($"OnTriggerEnter: Collided with {other.gameObject.name}, tag: {other.gameObject.tag}");

////        if (other.CompareTag("BombTarget"))
////        {
////            Debug.Log("Hit BombTarget! Adding points...");
////            if (scoreManager != null)
////            {
////                scoreManager.AddBombTargetPoints();
////            }
////            // Använd bombens position för explosionen vid trigger
////            HandleExplosion(transform.position);
////        }
////    }

////    private void OnCollisionEnter(Collision collision)
////    {
////        if (hasExploded) return;

////        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.name == "Ground")
////        {
////            HandleExplosion(collision.contacts[0].point);
////        }

////        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.name == "Ground")
////        {
////            HandleExplosion(collision.contacts[0].point);
////        }

////        Debug.Log($"Bomb collided with: {collision.gameObject.name}, Layer: {collision.gameObject.layer}, Tag: {collision.gameObject.tag}");

////        // Explodera vid alla kollisioner, inte bara BombTarget
////        HandleExplosion(collision.contacts[0].point);

////        // Lägg till poäng om det är ett bombmål
////        if (collision.gameObject.CompareTag("BombTarget"))
////        {
////            scoreManager?.AddBombTargetPoints();

////        }
////    }

////    private void HandleExplosion(Vector3 position)
////    {
////        if (hasExploded) return;
////        hasExploded = true;

////        // Skapa explosionseffekt
////        if (explosionEffectPrefab != null)
////        {
////            GameObject explosionEffect = Instantiate(explosionEffectPrefab, position, Quaternion.identity);
////            Destroy(explosionEffect, explosionDuration);
////        }

////        // Hitta alla närliggande objekt och påverka dem med explosionen
////        Collider[] colliders = Physics.OverlapSphere(position, explosionRadius);
////        foreach (Collider hit in colliders)
////        {
////            // Påverka rigidbodies med explosionskraft
////            Rigidbody rb = hit.GetComponent<Rigidbody>();
////            if (rb != null)
////            {
////                rb.AddExplosionForce(explosionForce, position, explosionRadius, 1.0f, ForceMode.Impulse);
////            }

////            // Hantera skada på fiender
////            if (hit.CompareTag("Enemy"))
////            {
////                EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
////                if (enemyHealth != null)
////                {
////                    float distance = Vector3.Distance(position, hit.transform.position);
////                    float damageMultiplier = 1 - (distance / explosionRadius);
////                    float damage = explosionDamage * damageMultiplier;
////                    enemyHealth.TakeDamage(damage);
////                }
////            }
////        }

////        // Aktivera kameraskakning
////        if (cameraShake != null)
////        {
////            cameraShake.ShakaCameraVidBomb();
////        }

////        // Spela explosionsljud
////        audioManager?.PlayBombExplosionSound();

////        // Förstör bomben
////        Destroy(gameObject);
////    }
////}
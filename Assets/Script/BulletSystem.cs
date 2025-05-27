using UnityEngine;
using System.Collections;

public class BulletSystem : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private bool isEnemyProjectile;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float speed = 1000f;
    //[SerializeField] private float bulletLifetime = 3f;
    private float assignedLifetime; // Ny variabel f�r mottagen lifetime

    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float effectDuration = 1f;

    private Vector3 direction;
    private float fixedHeight;
    private bool hasCollided;
    private AudioManager audioManager;
    private Coroutine deactivateCoroutine;

    private void OnEnable()
    {
        hasCollided = false;
        if (deactivateCoroutine != null)
        {
            StopCoroutine(deactivateCoroutine);
        }
        deactivateCoroutine = StartCoroutine(AutoDeactivate());
    }

    private void Start()
    {
        audioManager = AudioManager.Instance;
        fixedHeight = transform.position.y;
    }

    private void Update()
    {
        if (hasCollided) return;

        Vector3 movement = direction * speed * Time.deltaTime;
        Vector3 newPosition = transform.position + movement;
        newPosition.y = fixedHeight;
        transform.position = newPosition;
    }

    private IEnumerator AutoDeactivate()
    {
        yield return new WaitForSeconds(assignedLifetime); // Anv�nd assignedLifetime ist�llet
        if (gameObject.activeInHierarchy && !hasCollided)
        {
            ReturnToPool();
        }
    }

    public void Initialize(Vector3 shootDirection, bool isEnemy = false, float damageAmount = 10f, float lifetime = 3f)
    {
        direction = shootDirection;
        isEnemyProjectile = isEnemy;
        damage = damageAmount;
        assignedLifetime = lifetime; // Spara den mottagna lifetime
        gameObject.tag = isEnemy ? "Enemy Bullet" : "Player Bullet";
        hasCollided = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (hasCollided) return;

        // Fixa: F�renklad kollisionskontroll
        if ((isEnemyProjectile && (other.CompareTag("Enemy") || other.CompareTag("Enemy Bullet"))) ||
            (!isEnemyProjectile && (other.CompareTag("Player") || other.CompareTag("Player Bullet"))))
        {
            return;
        }

        // Fixa: N�r spelarens skott tr�ffar fiend - TA BORT ljudet
        if (!isEnemyProjectile && other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<EnemyHealth>(out var enemyHealth))
            {
                enemyHealth.TakeDamage(damage);
                PlayHitEffect();
                // TA BORT: audioManager?.PlayCombatSound(CombatSoundType.Hit);
            }
            hasCollided = true;
            ReturnToPool();
            return;
        }

        // Fixa: N�r fiendens skott tr�ffar spelare - BEH�LL ljudet h�r
        if (isEnemyProjectile && other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlaneHealthSystem>(out var playerHealth) &&
                !playerHealth.IsInvincible() &&
                !playerHealth.IsDead())
            {
                playerHealth.TakeDamage(damage);
                PlayHitEffect();
                audioManager?.PlayCombatSound(CombatSoundType.Hit);  // Beh�ll h�r
                CameraShake.Instance?.ShakaCameraVidTraff();
            }
        }

        hasCollided = true;
        ReturnToPool();
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (hasCollided) return;

    //    Fixa: F�renklad kollisionskontroll
    //    if ((isEnemyProjectile && (other.CompareTag("Enemy") || other.CompareTag("Enemy Bullet"))) ||
    //        (!isEnemyProjectile && (other.CompareTag("Player") || other.CompareTag("Player Bullet"))))
    //    {
    //        return;
    //    }

    //Fixa: B�ttre od�dlighetskontroll
    //    if (!isEnemyProjectile && other.CompareTag("Enemy"))
    //    {
    //        if (other.TryGetComponent<EnemyHealth>(out var enemyHealth))
    //        {
    //            enemyHealth.TakeDamage(damage);
    //            PlayHitEffect();
    //            audioManager?.PlayCombatSound(CombatSoundType.Hit);
    //        }
    //        hasCollided = true;
    //        ReturnToPool();
    //        return;
    //    }

    //Fixa: F�rb�ttrad spelarskadekontroll
    //    if (isEnemyProjectile && other.CompareTag("Player"))
    //    {
    //        if (other.TryGetComponent<PlaneHealthSystem>(out var playerHealth) &&
    //            !playerHealth.IsInvincible() &&
    //            !playerHealth.IsDead())
    //        {
    //            playerHealth.TakeDamage(damage);
    //            PlayHitEffect();
    //            audioManager?.PlayCombatSound(CombatSoundType.Hit);
    //            CameraShake.Instance?.ShakaCameraVidTraff();
    //        }
    //    }

    //    hasCollided = true;
    //    ReturnToPool();
    //}

    private void PlayHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            var effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }
    }

    private void ReturnToPool()
    {
        if (deactivateCoroutine != null)
        {
            StopCoroutine(deactivateCoroutine);
            deactivateCoroutine = null;
        }
        BulletPool.Instance.ReturnBulletToPool(gameObject);
    }

    private void OnDisable()
    {
        if (deactivateCoroutine != null)
        {
            StopCoroutine(deactivateCoroutine);
            deactivateCoroutine = null;
        }
    }
}

//using UnityEngine;
//using System.Collections;

//public class BulletSystem : MonoBehaviour
//{
//    [Header("Bullet Settings")]
//    [SerializeField] private bool isEnemyProjectile;
//    [SerializeField] private float damage = 10f;
//    [SerializeField] private float speed = 1000f;
//    [SerializeField] private float bulletLifetime = 3f;

//    [Header("Effects")]
//    [SerializeField] private GameObject hitEffectPrefab;
//    [SerializeField] private float effectDuration = 1f;

//    private Vector3 direction;
//    private float fixedHeight;
//    private bool hasCollided;
//    private AudioManager audioManager;
//    private Coroutine deactivateCoroutine;

//    private void OnEnable()
//    {
//        hasCollided = false;
//        if (deactivateCoroutine != null)
//        {
//            StopCoroutine(deactivateCoroutine);
//        }
//        deactivateCoroutine = StartCoroutine(AutoDeactivate());
//    }

//    private void Start()
//    {
//        audioManager = AudioManager.Instance;
//        fixedHeight = transform.position.y;
//    }

//    private void Update()
//    {
//        if (hasCollided) return;
//        Vector3 movement = direction * speed * Time.deltaTime;
//        Vector3 newPosition = transform.position + movement;
//        newPosition.y = fixedHeight;
//        transform.position = newPosition;
//    }

//    private IEnumerator AutoDeactivate()
//    {
//        yield return new WaitForSeconds(bulletLifetime);
//        if (gameObject.activeInHierarchy && !hasCollided)
//        {
//            ReturnToPool();
//        }
//    }

//    public void Initialize(Vector3 shootDirection, bool isEnemy = false, float damageAmount = 10f)
//    {
//        direction = shootDirection;
//        isEnemyProjectile = isEnemy;
//        damage = damageAmount;
//        gameObject.tag = isEnemy ? "Enemy Bullet" : "Player Bullet";
//        hasCollided = false;
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        Debug.Log($"Bullet collision start with: {other.gameObject.name}");

//        if (hasCollided) return;

//        // Skip bullet-bullet collisions
//        if (isEnemyProjectile && other.CompareTag("Enemy") ||
//            other.CompareTag("Enemy Bullet"))
//            return;

//        // Skip player bullet-player collisions    
//        if (!isEnemyProjectile && other.CompareTag("Player") ||
//            other.CompareTag("Player Bullet"))
//            return;

//        // N�r spelaren �r od�dlig, l�t fortfarande spelarens skott tr�ffa fiender
//        if (!isEnemyProjectile && other.CompareTag("Enemy"))
//        {
//            if (other.TryGetComponent<EnemyHealth>(out var enemyHealth))
//            {
//                enemyHealth.TakeDamage(damage);
//                PlayHitEffect();
//            }
//            hasCollided = true;
//            ReturnToPool();
//            return;
//        }

//        // Enemy bullet hits player - check invincibility
//        if (isEnemyProjectile && other.CompareTag("Player"))
//        {
//            Debug.Log("Enemy hit process complete");
//            if (other.TryGetComponent<PlaneHealthSystem>(out var playerHealth) && !playerHealth.IsInvincible() && !playerHealth.IsDead())
//            {
//                playerHealth.TakeDamage(damage);
//                PlayHitEffect();
//                audioManager?.PlayCombatSound(CombatSoundType.Hit);
//                CameraShake.Instance?.ShakaCameraVidTraff();
//            }
//            Debug.Log("Enemy hit process complete");
//        }

//        hasCollided = true;
//        ReturnToPool();
//        Debug.Log("Bullet returned to pool");
//    }

//    private void PlayHitEffect()
//    {
//        if (hitEffectPrefab != null)
//        {
//            var effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
//            Destroy(effect, effectDuration);
//        }
//    }

//    private void ReturnToPool()
//    {
//        if (deactivateCoroutine != null)
//        {
//            StopCoroutine(deactivateCoroutine);
//            deactivateCoroutine = null;
//        }
//        BulletPool.Instance.ReturnBulletToPool(gameObject);
//    }

//    private void OnDisable()
//    {
//        if (deactivateCoroutine != null)
//        {
//            StopCoroutine(deactivateCoroutine);
//            deactivateCoroutine = null;
//        }
//    }
//}

////using UnityEngine;

////public class BulletSystem : MonoBehaviour
////{
////    [Header("Bullet Settings")]
////    [SerializeField] private bool isEnemyProjectile;
////    [SerializeField] private float damage = 10f;
////    [SerializeField] private float speed = 1000f;
////    [SerializeField] private float bulletLifetime = 3f;

////    [Header("Effects")]
////    [SerializeField] private GameObject hitEffectPrefab;
////    [SerializeField] private float effectDuration = 1f;

////    private Vector3 direction;
////    private float fixedHeight;
////    private bool hasCollided;
////    private AudioManager audioManager;

////    private void Start()
////    {
////        audioManager = AudioManager.Instance;
////        fixedHeight = transform.position.y;
////        Destroy(gameObject, bulletLifetime);

////        //Debug.Log($"Bullet initialized: Enemy={isEnemyProjectile}, Tag={gameObject.tag}, Layer={gameObject.layer}");
////        Collider bulletCollider = GetComponent<Collider>();
////        //Debug.Log($"Bullet Collider: IsTrigger={bulletCollider?.isTrigger}");
////    }

////    private void Update()
////    {
////        if (hasCollided) return;
////        Vector3 movement = direction * speed * Time.deltaTime;
////        Vector3 newPosition = transform.position + movement;
////        newPosition.y = fixedHeight;
////        transform.position = newPosition;
////    }

////    public void Initialize(Vector3 shootDirection, bool isEnemy = false, float damageAmount = 10f)
////    {
////        direction = shootDirection;
////        isEnemyProjectile = isEnemy;
////        damage = damageAmount;
////        gameObject.tag = isEnemy ? "Enemy Bullet" : "Player Bullet";
////        //Debug.Log($"Bullet {gameObject.name} initialized. Direction={direction}, IsEnemy={isEnemyProjectile}");
////    }

////    private void OnTriggerEnter(Collider other)
////    {
////        if (hasCollided) return;

////        // Skip bullet-bullet collisions
////        if (isEnemyProjectile && other.CompareTag("Enemy") ||
////            other.CompareTag("Enemy Bullet"))
////            return;

////        // Skip player bullet-player collisions    
////        if (!isEnemyProjectile && other.CompareTag("Player") ||
////            other.CompareTag("Player Bullet"))
////            return;

////        //Debug.Log($"Bullet collision: Me={gameObject.tag}, Other={other.tag}");

////        // Enemy bullet hits player
////        if (isEnemyProjectile && other.CompareTag("Player"))
////        {
////            if (other.TryGetComponent<PlaneHealthSystem>(out var playerHealth) && !playerHealth.IsDead())
////            {
////                playerHealth.TakeDamage(damage);
////                PlayHitEffect();
////                audioManager?.PlayCombatSound(CombatSoundType.Hit);
////            }
////        }
////        // Player bullet hits enemy
////        else if (!isEnemyProjectile && other.CompareTag("Enemy"))
////        {
////            if (other.TryGetComponent<EnemyHealth>(out var enemyHealth))
////            {
////                enemyHealth.TakeDamage(damage);
////                PlayHitEffect();
////                audioManager?.PlayCombatSound(CombatSoundType.Hit);
////                //Debug.Log($"Enemy hit with {damage} damage!");
////            }
////        }

////        hasCollided = true;
////        Destroy(gameObject);
////    }

////    private void PlayHitEffect()
////    {
////        if (hitEffectPrefab != null)
////        {
////            var effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
////            Destroy(effect, effectDuration);
////        }
////    }
////}
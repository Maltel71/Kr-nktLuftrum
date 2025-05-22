using UnityEngine;
using UnityEngine.Splines;
using System.Collections;

public class TankEnemySpline : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private float speed = 5f;
    [SerializeField] private bool alignWithSpline = false;
    [SerializeField] private bool moveAlongSpline = true;

    [Header("Spline Settings")]
    [SerializeField] private float splineLength = 1600f;

    [Header("Shooting Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private GameObject smokePrefab;
    [SerializeField] private float minFireInterval = 0.3f;
    [SerializeField] private float maxFireInterval = 0.7f;

    [Header("Damage Effect Settings")]
    [SerializeField] private GameObject damageSmokeEmitterPrefab; // Rökprefab för skadeeffekt
    [SerializeField] private float timeToDamageEffect = 30f; // Tid innan skadeeffekt börjar
    [SerializeField] private float damageEffectIntensity = 1f; // Intensitet på skadeeffekten

    [Header("Bullet Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletDamage = 10f;

    [Header("Endpoints")]
    [SerializeField] private bool keepShootingAtEndpoint = true;

    [Header("Explosion Settings")]
    [SerializeField] private float explosionScale = 0.5f;

    // Privata variabler
    private float currentDistance = 0f;
    private bool hasReachedEnd = false;
    private float nextFireTime = 0f;
    private AudioManager audioManager;
    private GameObject currentDamageSmokeEmitter;

    [SerializeField] private Transform damageParticlePoint;

    private void Start()
    {
        audioManager = AudioManager.Instance;

        if (splineContainer == null)
        {
            Debug.LogError("Ingen SplineContainer tilldelad till " + gameObject.name);
            return;
        }

        nextFireTime = Time.time + Random.Range(minFireInterval, maxFireInterval);

        // Starta koroutine för skadeeffekt
        StartCoroutine(StartDamageEffectAfterDelay());
    }

    private IEnumerator StartDamageEffectAfterDelay()
    {
        // Vänta angiven tid
        yield return new WaitForSeconds(timeToDamageEffect);

        // Skapa skadeeffekt
        CreateDamageEffect();
    }

    private void CreateDamageEffect()
    {
        // Ta bort tidigare skadeeffekt om den finns
        if (currentDamageSmokeEmitter != null)
        {
            Destroy(currentDamageSmokeEmitter);
        }

        // Skapa ny skadeeffekt
        if (damageSmokeEmitterPrefab != null)
        {
            currentDamageSmokeEmitter = Instantiate(damageSmokeEmitterPrefab, transform.position, Quaternion.identity, transform);

            // Justera rökintensitet
            var particleSystems = currentDamageSmokeEmitter.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                var main = ps.main;
                main.startLifetime = main.startLifetime.constant * damageEffectIntensity;
                main.startSize = main.startSize.constant * damageEffectIntensity;
                main.startSpeed = main.startSpeed.constant * damageEffectIntensity;
            }
        }
    }

    // Resten av koden förblir samma som tidigare
    private void Update()
    {
        if (keepShootingAtEndpoint || !hasReachedEnd)
        {
            HandleShooting();
        }

        if (!hasReachedEnd && moveAlongSpline)
        {
            MoveAlongSpline();
        }
    }

    private void MoveAlongSpline()
    {
        currentDistance += speed * Time.deltaTime;

        if (currentDistance >= splineLength)
        {
            currentDistance = splineLength;
            hasReachedEnd = true;

            Vector3 endPosition = splineContainer.EvaluatePosition(1.0f);
            transform.position = endPosition;

            return;
        }

        float normalizedDistance = Mathf.Clamp01(currentDistance / splineLength);

        Vector3 position = splineContainer.EvaluatePosition(normalizedDistance);
        Vector3 tangent = splineContainer.EvaluateTangent(normalizedDistance);

        // Uppdatera position längs tangenten
        transform.position = position;

        // Rotera endast om alignWithSpline är markerad
        if (alignWithSpline)
        {
            transform.rotation = Quaternion.LookRotation(tangent, Vector3.up);
        }
    }

    private void HandleShooting()
    {
        if (Time.time > nextFireTime)
        {
            FireWeapon();
            nextFireTime = Time.time + Random.Range(minFireInterval, maxFireInterval);
        }
    }

    private void FireWeapon()
    {
        if (firePoint == null || bulletPrefab == null) return;

        // ÄNDRING: Skapa skott med MOTSATT riktning (Vector3.back istället för Vector3.forward)
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(Vector3.back));

        if (bullet.TryGetComponent<BulletSystem>(out var bulletSystem))
        {
            // ÄNDRING: Initialisera med Vector3.back istället för Vector3.forward
            bulletSystem.Initialize(Vector3.back, true, bulletDamage);
        }

        if (bullet.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.useGravity = false;
            // ÄNDRING: Använd Vector3.back istället för Vector3.forward för hastighet
            rb.linearVelocity = Vector3.back * bulletSpeed;
        }

        CreateExplosionAtFirePoint();
        CreateMuzzleFlash();
        CreateSmokeEffect();
        audioManager?.PlayEnemyShootSound();
    }

    private void CreateExplosionAtFirePoint()
    {
        try
        {
            GameObject explosion = ExplosionPool.Instance.GetExplosion(ExplosionType.Small);
            explosion.transform.position = firePoint.position;
            explosion.transform.localScale = Vector3.one * explosionScale;
            ExplosionPool.Instance.ReturnExplosionToPool(explosion, 0.5f);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Kunde inte skapa explosion: {e.Message}");
        }
    }

    private void CreateMuzzleFlash()
    {
        if (muzzleFlashPrefab != null)
        {
            // ÄNDRING: Ändra riktning på muzzle flash till Vector3.back
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, Quaternion.LookRotation(Vector3.back));
            flash.transform.localScale = new Vector3(2f, 2f, 3f);
            Destroy(flash, 0.2f);
        }
        else
        {
            GameObject debugFlash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            debugFlash.transform.position = firePoint.position;
            debugFlash.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            debugFlash.GetComponent<Renderer>().material.color = Color.yellow;
            Destroy(debugFlash, 0.2f);
        }
    }

    private void CreateSmokeEffect()
    {
        if (smokePrefab != null)
        {
            // ÄNDRING: Ändra riktning på rökeffekt till Vector3.back
            GameObject smoke = Instantiate(smokePrefab, firePoint.position, Quaternion.LookRotation(Vector3.back));
            Destroy(smoke, 2f); // Förstör röken efter 2 sekunder
        }
    }

    public void StopMoving() => moveAlongSpline = false;
    public void StartMoving() => moveAlongSpline = true;
}
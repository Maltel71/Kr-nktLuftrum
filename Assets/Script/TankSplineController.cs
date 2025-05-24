using UnityEngine;
using UnityEngine.Splines;
using System.Collections;

public class TankSplineController : MonoBehaviour
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
    [SerializeField] private bool keepShootingAtEndpoint = false; // Ska tanken fortsätta skjuta när den stannar?

    [Header("Damage Effect Settings")]
    [SerializeField] private GameObject damageSmokeEmitterPrefab; // Rökprefab för skadeeffekt
    [SerializeField] private Transform damageParticlePoint; // Punkt där röken ska komma från
    [SerializeField] private float damageEffectIntensity = 1f; // Intensitet på skadeeffekten
    [SerializeField] private float delayBeforeDamageSmoke = 2f; // Kort paus innan röken börjar när tanken stannar

    [Header("Bullet Settings")]
    [SerializeField] private bool shootActualBullets = false; // Sätt till false för att bara ha visuella effekter
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletDamage = 10f;

    [Header("Explosion Settings")]
    [SerializeField] private float explosionScale = 0.5f;

    // Privata variabler
    private float currentDistance = 0f;
    private bool hasReachedEnd = false;
    private float nextFireTime = 0f;
    private AudioManager audioManager;
    private GameObject currentDamageSmokeEmitter;
    private bool isDamaged = false; // Flag för att spåra om tanken är skadad

    private void Start()
    {
        audioManager = AudioManager.Instance;

        if (splineContainer == null)
        {
            Debug.LogError("Ingen SplineContainer tilldelad till " + gameObject.name);
            return;
        }

        nextFireTime = Time.time + Random.Range(minFireInterval, maxFireInterval);
    }

    private void Update()
    {
        // Skjut om tanken inte nått slutet ÄN, eller om den ska fortsätta skjuta vid slutet
        if ((keepShootingAtEndpoint || !hasReachedEnd) && moveAlongSpline)
        {
            HandleShooting();
        }

        // Rörelse bara om tanken inte nått slutet
        if (!hasReachedEnd && moveAlongSpline)
        {
            MoveAlongSpline();
        }
        // Om tanken precis stannat (nått slutet) och inte är skadad än
        else if (hasReachedEnd && !isDamaged)
        {
            // Starta skadeprocessen
            StartCoroutine(StartDamageProcess());
        }
    }

    private IEnumerator StartDamageProcess()
    {
        isDamaged = true; // Sätt flaggan direkt för att förhindra upprepning

        Debug.Log($"{gameObject.name} har nått slutet av splinen och kommer snart börja ryka");

        // Vänta lite innan röken börjar (ger intryck av att tanken blir träffad)
        yield return new WaitForSeconds(delayBeforeDamageSmoke);

        // Skapa skadeeffekt
        CreateDamageEffect();
    }

    private void MoveAlongSpline()
    {
        currentDistance += speed * Time.deltaTime;

        if (currentDistance >= splineLength)
        {
            // Tanken har nått slutet
            if (!hasReachedEnd)
            {
                hasReachedEnd = true;
                Debug.Log($"{gameObject.name} har nått slutet av splinen");
            }

            currentDistance = splineLength;
            Vector3 endPosition = splineContainer.EvaluatePosition(1.0f);
            transform.position = endPosition;
            return;
        }

        float normalizedDistance = Mathf.Clamp01(currentDistance / splineLength);

        Vector3 position = splineContainer.EvaluatePosition(normalizedDistance);
        Vector3 tangent = splineContainer.EvaluateTangent(normalizedDistance);

        // Uppdatera position längs splinen
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
        if (firePoint == null) return;

        // Skapa bara kulor om shootActualBullets är true
        if (shootActualBullets && bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(Vector3.forward));

            if (bullet.TryGetComponent<BulletSystem>(out var bulletSystem))
            {
                bulletSystem.Initialize(Vector3.forward, true, bulletDamage);
            }

            if (bullet.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.useGravity = false;
                rb.linearVelocity = Vector3.forward * bulletSpeed;
            }
        }

        // Visuella effekter spelas alltid
        CreateExplosionAtFirePoint();
        CreateMuzzleFlash();
        CreateSmokeEffect();
        audioManager?.PlayEnemyShootSound();
    }

    private void CreateDamageEffect()
    {
        Debug.Log($"CreateDamageEffect anropad för {gameObject.name}");

        // Ta bort tidigare skadeeffekt om den finns
        if (currentDamageSmokeEmitter != null)
        {
            Destroy(currentDamageSmokeEmitter);
        }

        // Skapa ny skadeeffekt
        if (damageSmokeEmitterPrefab != null)
        {
            // Använd damageParticlePoint om den finns, annars tankens centrum
            Vector3 smokePosition = damageParticlePoint != null ? damageParticlePoint.position : transform.position;
            Debug.Log($"Skapar rök på position: {smokePosition}");

            currentDamageSmokeEmitter = Instantiate(damageSmokeEmitterPrefab, smokePosition, Quaternion.identity, transform);

            // Justera rökintensitet
            var particleSystems = currentDamageSmokeEmitter.GetComponentsInChildren<ParticleSystem>();
            Debug.Log($"Hittade {particleSystems.Length} partikelsystem");

            foreach (var ps in particleSystems)
            {
                var main = ps.main;
                main.startLifetime = main.startLifetime.constant * damageEffectIntensity;
                main.startSize = main.startSize.constant * damageEffectIntensity;
                main.startSpeed = main.startSpeed.constant * damageEffectIntensity;
            }

            Debug.Log($"{gameObject.name} börjar ryka - ser skadad ut");
        }
        else
        {
            //Debug.LogError($"damageSmokeEmitterPrefab är null för {gameObject.name}!");
        }
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
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, Quaternion.LookRotation(Vector3.forward));
            flash.transform.localScale = new Vector3(2f, 2f, 3f);
            Destroy(flash, 0.2f);
        }
        else
        {
            // Debug flash om inget prefab finns
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
            GameObject smoke = Instantiate(smokePrefab, firePoint.position, Quaternion.LookRotation(Vector3.forward));
            Destroy(smoke, 2f);
        }
    }

    // Publika metoder för kontroll
    public void StopMoving() => moveAlongSpline = false;
    public void StartMoving() => moveAlongSpline = true;

    // För debugging - tvinga igång skadeeffekt
    [ContextMenu("Force Damage Effect")]
    public void ForceDamageEffect()
    {
        if (!isDamaged)
        {
            StartCoroutine(StartDamageProcess());
        }
    }

    // Kontrollera status
    public bool HasReachedEnd() => hasReachedEnd;
    public bool IsDamaged() => isDamaged;
}
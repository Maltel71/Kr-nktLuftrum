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
    [SerializeField] private float explosionHeightOffset = 1f; // NY: Höjd ovanför marken

    [Header("Collision Detection")]
    [SerializeField] private LayerMask groundLayers = -1; // Alla lager som default
    [SerializeField] private string[] explodeOnTags = { "Ground", "Building", "Vehicle", "BombTarget", "Enemy" }; // Alla tags som ska explodera bomben

    private CameraShake cameraShake;
    private ScoreManager scoreManager;
    private bool hasPlayedFallingSound = false;
    private bool hasExploded = false;
    private Vector3 lastPosition;

    private void Start()
    {
        audioManager = AudioManager.Instance;
        cameraShake = CameraShake.Instance;
        scoreManager = ScoreManager.Instance;
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (!hasPlayedFallingSound && GetComponent<Rigidbody>().linearVelocity.y < 0)
        {
            audioManager?.PlayBombSound(BombSoundType.Falling);
            hasPlayedFallingSound = true;
        }

        // Spara position för exakt explosionsplats
        lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;

        // FIX 1: Explodera på alla relevanta objekt
        if (ShouldExplodeOn(other))
        {
            Vector3 explosionPoint = GetExactContactPoint(other);
            Debug.Log($"Bomb triggered explosion on: {other.gameObject.name} (Tag: {other.tag})");

            // Ge poäng om det är BombTarget
            if (other.CompareTag("BombTarget"))
            {
                scoreManager?.AddBombTargetPoints();
            }

            HandleExplosion(explosionPoint);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;

        // FIX 2: Använd collision.contacts för EXAKT träffpunkt
        Vector3 exactContactPoint = Vector3.zero;

        if (collision.contacts.Length > 0)
        {
            // Använd första kontaktpunkten - detta är EXAKT där bomben träffar
            exactContactPoint = collision.contacts[0].point;
        }
        else
        {
            // Fallback till bombens position
            exactContactPoint = transform.position;
        }

        // Explodera på alla relevanta objekt
        if (ShouldExplodeOn(collision.collider))
        {
            Debug.Log($"Bomb collision explosion on: {collision.gameObject.name} (Tag: {collision.collider.tag})");

            // Ge poäng om det är BombTarget
            if (collision.gameObject.CompareTag("BombTarget"))
            {
                scoreManager?.AddBombTargetPoints();
            }

            HandleExplosion(exactContactPoint);
        }
    }

    // FIX 3: Förbättrad explosionsdetektering
    private bool ShouldExplodeOn(Collider other)
    {
        // Kontrollera om objektet har någon av våra explosive tags
        foreach (string tag in explodeOnTags)
        {
            if (other.CompareTag(tag))
            {
                return true;
            }
        }

        // Extra kontroller för objektnamn (ifall tags saknas)
        string objectName = other.gameObject.name.ToLower();
        if (objectName.Contains("ground") ||
            objectName.Contains("terrain") ||
            objectName.Contains("floor") ||
            objectName.Contains("building") ||
            objectName.Contains("house") ||
            objectName.Contains("wall") ||
            objectName.Contains("vehicle") ||
            objectName.Contains("car") ||
            objectName.Contains("truck") ||
            objectName.Contains("tank"))
        {
            return true;
        }

        // Kontrollera layer (för objekt utan rätt tags)
        return ((groundLayers.value & (1 << other.gameObject.layer)) > 0);
    }

    // Behåll som backup (nu används inte)
    private bool IsGroundCollision(Collider other)
    {
        return ShouldExplodeOn(other);
    }

    // FIX 4: Exakt kontaktpunkt för triggers
    private Vector3 GetExactContactPoint(Collider other)
    {
        // För triggers, använd closest point
        Vector3 closestPoint = other.ClosestPoint(transform.position);

        // Om closest point är för långt från bomben, använd bombens position
        float distance = Vector3.Distance(closestPoint, transform.position);
        if (distance > 2f) // Om för långt bort
        {
            return transform.position;
        }

        return closestPoint;
    }

    private void HandleExplosion(Vector3 explosionPoint)
    {
        if (hasExploded) return;
        hasExploded = true;

        // FIX 5: EXAKT explosionspositionering med höjdoffset
        Vector3 finalExplosionPoint = explosionPoint;

        // Justera explosionspunkten till bombens position om den är för långt bort
        float distanceFromBomb = Vector3.Distance(explosionPoint, transform.position);
        if (distanceFromBomb > 1f) // Om explosionspunkten är mer än 1 meter från bomben
        {
            finalExplosionPoint = transform.position; // Använd bombens exakta position istället
        }

        // NYTT: Lägg till höjdoffset så explosionen hamnar ovanför marken
        finalExplosionPoint.y += explosionHeightOffset;

        Debug.Log($"Bomb exploding at: {finalExplosionPoint} (raised by {explosionHeightOffset}m), Distance from bomb: {distanceFromBomb:F2}");

        // VIKTIGT: Stoppa bombens rörelse OMEDELBART
        Rigidbody bombRb = GetComponent<Rigidbody>();
        if (bombRb != null)
        {
            bombRb.linearVelocity = Vector3.zero;
            bombRb.angularVelocity = Vector3.zero;
            bombRb.isKinematic = true; // Stoppa all fysik
        }

        // Debug-visualisering (röd linje i 3 sekunder)
        Debug.DrawRay(finalExplosionPoint, Vector3.up * 5f, Color.red, 3f);
        Debug.DrawRay(finalExplosionPoint, Vector3.forward * 3f, Color.yellow, 3f);
        Debug.DrawRay(finalExplosionPoint, Vector3.right * 3f, Color.blue, 3f);
        Debug.DrawRay(finalExplosionPoint, Vector3.down * explosionHeightOffset, Color.green, 3f); // Grön linje ner till marken

        // Använd explosionspoolen med EXAKT position (höjd-justerad)
        GameObject explosionEffect = ExplosionPool.Instance.GetExplosion(ExplosionType.Large);
        explosionEffect.transform.position = finalExplosionPoint; // EXAKT position med höjdoffset

        // EXTRA: Säkerställ att explosionen är i rätt skala för perspective camera
        explosionEffect.transform.localScale = Vector3.one; // Reset scale

        ExplosionPool.Instance.ReturnExplosionToPool(explosionEffect, explosionDuration);

        // Skadeområde från explosionspunkten (använd original-positionen för skada, inte den höjda)
        Vector3 damageCenter = explosionPoint; // Skada från markpositionen
        Collider[] colliders = Physics.OverlapSphere(damageCenter, explosionRadius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, damageCenter, explosionRadius, 1.0f, ForceMode.Impulse);
            }

            if (hit.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    float distance = Vector3.Distance(damageCenter, hit.transform.position);
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

        // OMEDELBAR förstöring för att undvika "dubbla explosioner"
        Destroy(gameObject, 0.05f);
    }

    // Hjälpmetod för att visualisera explosionsradien i Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
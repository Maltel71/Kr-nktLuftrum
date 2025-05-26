using UnityEngine;
using UnityEngine.Splines;

public class EnemySplineController : MonoBehaviour
{
    [Header("Spline Settings")]
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private float speed = 5f;
    [SerializeField] private bool loopPath = true;

    [Header("Performance Settings")]
    [SerializeField] private int updateFrequency = 1; // Uppdatera varje frame (1) eller var X frame
    [SerializeField] private bool destroyAtEnd = true; // Förstör när spline är slut
    [SerializeField] private bool alignWithSpline = true; // Rotera med spline-riktning

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLogs = false;

    // Cachade värden för prestanda
    private float currentDistance = 0f;
    private float splineLength;
    private int frameCounter = 0;
    private bool hasReachedEnd = false;
    private bool isInitialized = false;

    // Cachade transforms för bättre prestanda
    private Transform cachedTransform;

    private void Awake()
    {
        // Cacha transform
        cachedTransform = transform;
    }

    private void Start()
    {
        InitializeSpline();
    }

    private void InitializeSpline()
    {
        // SÄKERHETSKONTROLL 1: Kontrollera att splineContainer finns
        if (splineContainer == null)
        {
            Debug.LogError($"SplineContainer saknas på {gameObject.name} - stänger av script");
            enabled = false;
            return;
        }

        // SÄKERHETSKONTROLL 2: Beräkna spline-längd säkert
        try
        {
            splineLength = splineContainer.CalculateLength();

            if (splineLength <= 0)
            {
                Debug.LogError($"Spline har ingen längd ({splineLength}) på {gameObject.name} - stänger av script");
                enabled = false;
                return;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Fel vid beräkning av spline-längd på {gameObject.name}: {e.Message}");
            enabled = false;
            return;
        }

        DebugLog($"Spline initialiserad. Längd: {splineLength}");

        // Sätt random startposition för variation
        if (loopPath)
        {
            currentDistance = Random.Range(0f, splineLength * 0.3f); // Starta inom första 30%
        }

        // Positionera på spline
        if (!UpdatePosition())
        {
            Debug.LogError($"Kunde inte positionera {gameObject.name} på spline");
            enabled = false;
            return;
        }

        // Ta bort SplineAnimate om den finns (vi gör vår egen implementation)
        var splineAnimate = GetComponent<SplineAnimate>();
        if (splineAnimate != null)
        {
            Destroy(splineAnimate);
        }

        isInitialized = true;
        DebugLog("Spline initialization complete");
    }

    private void Update()
    {
        // SÄKERHETSKONTROLL 3: Kontrollera att allt är OK innan update
        if (!isInitialized || hasReachedEnd || splineContainer == null || splineLength <= 0)
            return;

        // Optimering: Uppdatera inte varje frame för många objekt
        if (updateFrequency > 1)
        {
            frameCounter++;
            if (frameCounter < updateFrequency)
                return;
            frameCounter = 0;
        }

        MoveAlongSpline();
    }

    private void MoveAlongSpline()
    {
        // SÄKERHETSKONTROLL 4: Dubbelkolla innan rörelse
        if (splineContainer == null || splineLength <= 0)
        {
            Debug.LogError($"Spline problem på {gameObject.name}!");
            enabled = false;
            return;
        }

        // Beräkna ny position
        currentDistance += speed * Time.deltaTime * updateFrequency;

        // Hantera slutet av spline
        if (currentDistance >= splineLength)
        {
            if (loopPath)
            {
                // SÄKER loop-hantering
                if (splineLength > 0)
                {
                    currentDistance = currentDistance % splineLength;
                }
                else
                {
                    currentDistance = 0;
                }
            }
            else
            {
                // Nått slutet
                currentDistance = splineLength;
                hasReachedEnd = true;

                if (destroyAtEnd)
                {
                    HandleEndOfSpline();
                    return;
                }
            }
        }

        // Uppdatera position
        if (!UpdatePosition())
        {
            Debug.LogError($"UpdatePosition misslyckades för {gameObject.name}");
            enabled = false;
        }
    }

    private bool UpdatePosition()
    {
        // SÄKERHETSKONTROLL 5: Kontrollera innan position update
        if (splineContainer == null || splineLength <= 0)
        {
            DebugLog("Cannot update position - invalid spline data");
            return false;
        }

        try
        {
            float normalizedDistance = Mathf.Clamp01(currentDistance / splineLength);

            // Få position från spline
            Vector3 position = splineContainer.EvaluatePosition(normalizedDistance);
            cachedTransform.position = position;

            // Rotera med spline-riktning om aktiverat
            if (alignWithSpline)
            {
                Vector3 tangent = splineContainer.EvaluateTangent(normalizedDistance);
                if (tangent != Vector3.zero)
                {
                    cachedTransform.rotation = Quaternion.LookRotation(tangent);
                }
            }

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Spline evaluation error på {gameObject.name}: {e.Message}");
            return false;
        }
    }

    private void HandleEndOfSpline()
    {
        DebugLog("Reached end of spline");

        // Ge poäng om det är en fiende
        if (gameObject.CompareTag("Enemy"))
        {
            // ScoreManager.Instance?.AddEnemyEscapedPenalty(); // Om du vill straffa spelaren
        }

        // Förstör objektet efter en kort delay för att undvika frame-hickups
        Destroy(gameObject, 0.1f);
    }

    // Publika metoder för kontroll
    public void SetSpeed(float newSpeed)
    {
        if (newSpeed >= 0)
        {
            speed = newSpeed;
        }
    }

    public void SetUpdateFrequency(int frequency)
    {
        updateFrequency = Mathf.Max(1, frequency);
    }

    public float GetProgress()
    {
        if (splineLength <= 0) return 0f;
        return currentDistance / splineLength;
    }

    public bool HasReachedEnd()
    {
        return hasReachedEnd;
    }

    public bool IsInitialized()
    {
        return isInitialized;
    }

    // Debug helper
    private void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[EnemySplineController - {gameObject.name}] {message}");
        }
    }

    // För debugging i Editor
    private void OnDrawGizmosSelected()
    {
        if (splineContainer == null || !isInitialized) return;

        try
        {
            // Rita progress längs spline
            Gizmos.color = Color.yellow;
            float normalizedDistance = splineLength > 0 ? currentDistance / splineLength : 0;
            Vector3 currentPos = splineContainer.EvaluatePosition(normalizedDistance);
            Gizmos.DrawWireSphere(currentPos, 1f);

            // Rita spline-riktning
            if (alignWithSpline)
            {
                Vector3 tangent = splineContainer.EvaluateTangent(normalizedDistance);
                Gizmos.color = Color.red;
                Gizmos.DrawRay(currentPos, tangent * 3f);
            }
        }
        catch (System.Exception)
        {
            // Tyst hantering av gizmo-fel
        }
    }

    // Cleanup när objektet förstörs
    private void OnDestroy()
    {
        isInitialized = false;
    }

    // För manuell återställning
    [ContextMenu("Reset Spline Controller")]
    public void ResetController()
    {
        hasReachedEnd = false;
        currentDistance = 0f;
        isInitialized = false;
        InitializeSpline();
    }
}
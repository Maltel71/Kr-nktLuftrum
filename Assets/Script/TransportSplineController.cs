using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics; // För att hantera float3

public class TransportSplineController : MonoBehaviour
{
    [Header("Spline Settings")]
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private float baseSpeed = 3f;
    [SerializeField] private bool loopPath = true;
    [SerializeField] private float heightOffset = 0.2f; // Håller fordonet lite ovanför marken

    [Header("Military Transport Settings")]
    [SerializeField] private MilitaryTransportType vehicleType = MilitaryTransportType.SupplyTruck;
    [SerializeField] private bool useRandomizedSpeed = true;
    [SerializeField] private float speedVariation = 0.3f; // ±30% av basSpeed

    [Header("Wheels")]
    [SerializeField] private Transform[] wheels;
    [SerializeField] private float wheelRotationMultiplier = 50f;

    // Enum för olika militära transporttyper
    public enum MilitaryTransportType
    {
        SupplyTruck,     // Försörjningslastbil: standardfart
        ArmoredTruck,    // Bepansrad lastbil: långsammare, men stabil
        FuelTanker,      // Bränsletankbil: långsammare
        TroopTransport   // Trupptransport: snabbare
    }

    // Interna variabler
    private float currentDistance = 0f;
    private float splineLength;
    private float actualSpeed;
    private Vector3 previousPosition;

    void Start()
    {
        if (splineContainer == null)
        {
            Debug.LogError("Ingen SplineContainer tilldelad till " + gameObject.name);
            enabled = false;
            return;
        }

        // Beräkna splinelängd
        splineLength = splineContainer.CalculateLength();

        // Ställ fordonet på en slumpmässig position på spline
        currentDistance = UnityEngine.Random.Range(0f, splineLength);
        PositionOnSpline(currentDistance / splineLength);

        // Sätt faktisk hastighet baserat på fordonstyp
        SetSpeedByVehicleType();

        // Spara startposition för hjulrotation
        previousPosition = transform.position;
    }

    void Update()
    {
        // Flytta längs spline
        MoveAlongSpline();

        // Rotera hjulen
        RotateWheels();
    }

    private void MoveAlongSpline()
    {
        // Öka avstånd baserat på hastighet
        currentDistance += actualSpeed * Time.deltaTime;

        // Hantera när vi når slutet
        if (loopPath)
        {
            // Loop tillbaka till början
            currentDistance %= splineLength;
        }
        else if (currentDistance >= splineLength)
        {
            // När vi når slutet, börja om från början
            currentDistance = 0f;
        }

        // Uppdatera position och rotation
        PositionOnSpline(currentDistance / splineLength);
    }

    private void PositionOnSpline(float normalizedDistance)
    {
        // Hämta position och riktning från spline
        Vector3 position = splineContainer.EvaluatePosition(normalizedDistance);

        // Konvertera float3 till Vector3 för att använda normalized
        Vector3 tangent = (Vector3)splineContainer.EvaluateTangent(normalizedDistance);
        Vector3 forward = tangent.normalized;
        Vector3 up = Vector3.up; // Standard upp-riktning

        // Justera höjden för att undvika att sjunka in i vägen
        position += Vector3.up * heightOffset;

        // Positionera fordonet
        transform.position = position;

        // Rotera fordonet i rörelseriktningen
        if (forward != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(forward, up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }

    private void SetSpeedByVehicleType()
    {
        // Sätt hastighet baserat på fordonstyp
        float typeSpeedMultiplier = vehicleType switch
        {
            MilitaryTransportType.SupplyTruck => 1.0f,
            MilitaryTransportType.ArmoredTruck => 0.8f,
            MilitaryTransportType.FuelTanker => 0.7f,
            MilitaryTransportType.TroopTransport => 1.2f,
            _ => 1.0f
        };

        // Applicera slumpmässig variation om aktiverad
        if (useRandomizedSpeed)
        {
            float randomVariation = 1.0f + UnityEngine.Random.Range(-speedVariation, speedVariation);
            actualSpeed = baseSpeed * typeSpeedMultiplier * randomVariation;
        }
        else
        {
            actualSpeed = baseSpeed * typeSpeedMultiplier;
        }
    }

    private void RotateWheels()
    {
        if (wheels == null || wheels.Length == 0)
            return;

        // Beräkna faktisk rörelsehastighet
        float travelDistance = Vector3.Distance(transform.position, previousPosition);
        float wheelRotationAmount = (travelDistance * wheelRotationMultiplier) / (wheels[0].localScale.y * 0.5f);

        // Rotera varje hjul baserat på verklig hastighet
        foreach (Transform wheel in wheels)
        {
            if (wheel != null)
                wheel.Rotate(wheelRotationAmount * Mathf.Rad2Deg, 0, 0, Space.Self);
        }

        previousPosition = transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        if (splineContainer == null)
            return;

        // Visa fordonets position på spline i editorn
        if (Application.isPlaying)
        {
            float normalizedDistance = currentDistance / splineLength;
            Vector3 position = splineContainer.EvaluatePosition(normalizedDistance);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(position, 0.5f);

            // Visa hjulen
            if (wheels != null && wheels.Length > 0)
            {
                Gizmos.color = Color.cyan;
                foreach (Transform wheel in wheels)
                {
                    if (wheel != null)
                        Gizmos.DrawWireSphere(wheel.position, wheel.localScale.y * 0.5f);
                }
            }
        }
    }
}
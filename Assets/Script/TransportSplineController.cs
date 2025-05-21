using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics; // F�r att hantera float3

public class TransportSplineController : MonoBehaviour
{
    [Header("Spline Settings")]
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private float baseSpeed = 3f;
    [SerializeField] private bool loopPath = true;
    [SerializeField] private float heightOffset = 0.2f; // H�ller fordonet lite ovanf�r marken

    [Header("Military Transport Settings")]
    [SerializeField] private MilitaryTransportType vehicleType = MilitaryTransportType.SupplyTruck;
    [SerializeField] private bool useRandomizedSpeed = true;
    [SerializeField] private float speedVariation = 0.3f; // �30% av basSpeed

    [Header("Wheels")]
    [SerializeField] private Transform[] wheels;
    [SerializeField] private float wheelRotationMultiplier = 50f;

    // Enum f�r olika milit�ra transporttyper
    public enum MilitaryTransportType
    {
        SupplyTruck,     // F�rs�rjningslastbil: standardfart
        ArmoredTruck,    // Bepansrad lastbil: l�ngsammare, men stabil
        FuelTanker,      // Br�nsletankbil: l�ngsammare
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

        // Ber�kna splinel�ngd
        splineLength = splineContainer.CalculateLength();

        // St�ll fordonet p� en slumpm�ssig position p� spline
        currentDistance = UnityEngine.Random.Range(0f, splineLength);
        PositionOnSpline(currentDistance / splineLength);

        // S�tt faktisk hastighet baserat p� fordonstyp
        SetSpeedByVehicleType();

        // Spara startposition f�r hjulrotation
        previousPosition = transform.position;
    }

    void Update()
    {
        // Flytta l�ngs spline
        MoveAlongSpline();

        // Rotera hjulen
        RotateWheels();
    }

    private void MoveAlongSpline()
    {
        // �ka avst�nd baserat p� hastighet
        currentDistance += actualSpeed * Time.deltaTime;

        // Hantera n�r vi n�r slutet
        if (loopPath)
        {
            // Loop tillbaka till b�rjan
            currentDistance %= splineLength;
        }
        else if (currentDistance >= splineLength)
        {
            // N�r vi n�r slutet, b�rja om fr�n b�rjan
            currentDistance = 0f;
        }

        // Uppdatera position och rotation
        PositionOnSpline(currentDistance / splineLength);
    }

    private void PositionOnSpline(float normalizedDistance)
    {
        // H�mta position och riktning fr�n spline
        Vector3 position = splineContainer.EvaluatePosition(normalizedDistance);

        // Konvertera float3 till Vector3 f�r att anv�nda normalized
        Vector3 tangent = (Vector3)splineContainer.EvaluateTangent(normalizedDistance);
        Vector3 forward = tangent.normalized;
        Vector3 up = Vector3.up; // Standard upp-riktning

        // Justera h�jden f�r att undvika att sjunka in i v�gen
        position += Vector3.up * heightOffset;

        // Positionera fordonet
        transform.position = position;

        // Rotera fordonet i r�relseriktningen
        if (forward != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(forward, up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }

    private void SetSpeedByVehicleType()
    {
        // S�tt hastighet baserat p� fordonstyp
        float typeSpeedMultiplier = vehicleType switch
        {
            MilitaryTransportType.SupplyTruck => 1.0f,
            MilitaryTransportType.ArmoredTruck => 0.8f,
            MilitaryTransportType.FuelTanker => 0.7f,
            MilitaryTransportType.TroopTransport => 1.2f,
            _ => 1.0f
        };

        // Applicera slumpm�ssig variation om aktiverad
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

        // Ber�kna faktisk r�relsehastighet
        float travelDistance = Vector3.Distance(transform.position, previousPosition);
        float wheelRotationAmount = (travelDistance * wheelRotationMultiplier) / (wheels[0].localScale.y * 0.5f);

        // Rotera varje hjul baserat p� verklig hastighet
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

        // Visa fordonets position p� spline i editorn
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
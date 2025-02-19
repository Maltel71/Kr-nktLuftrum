using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    private static CameraShake instance;
    private Vector3 originalPosition;
    private bool isShaking = false;

    [Header("Kamerareferens")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool anv�nderCameraParent = true;

    [Header("Skakningsinst�llningar")]
    [SerializeField] private float standardIntensitet = 0.2f;
    [SerializeField] private float standardTid = 0.3f;
    [SerializeField] private float bossIntensitet = 0.5f;
    [SerializeField] private float bossTid = 1f;
    [SerializeField] private float d�mpningHastighet = 2f;
    

    public static CameraShake Instance => instance;

    private void Awake()
    {
        Debug.Log("CameraShake Awake k�rs");
        if (instance == null)
        {
            instance = this;
            SetupCamera();
        }
        else
        {
            Debug.LogWarning("Multiple CameraShake instances detected!");
            Destroy(gameObject);
        }
    }

    private void SetupCamera()
    {
        // Om ingen kamera �r tilldelad, f�rs�k hitta den
        if (cameraTransform == null)
        {
            if (anv�nderCameraParent)
            {
                // Anv�nd parent om det finns
                var mainCamera = Camera.main;
                if (mainCamera != null && mainCamera.transform.parent != null)
                {
                    cameraTransform = mainCamera.transform.parent;
                    Debug.Log($"Hittade camera parent: {cameraTransform.name}");
                }
            }

            if (cameraTransform == null)
            {
                // Fallback till huvudkameran om ingen parent finns
                cameraTransform = Camera.main?.transform;
                Debug.LogWarning($"Ingen camera parent hittad. Anv�nder huvudkameran direkt: {(cameraTransform != null ? cameraTransform.name : "null")}");
            }
        }

        if (cameraTransform != null)
        {
            originalPosition = cameraTransform.localPosition;
            //Debug.Log($"Sparar originalPosition: {originalPosition}");
        }
        else
        {
            //Debug.LogError("Ingen kameratransform hittad!");
        }
    }

    public void ShakaCameraVidTraff()
    {
        StartCoroutine(Skaka(3.0f, 0.4f)); // �nnu l�gre intensitet och kortare varaktighet
    }

    public void ShakaCameraVidBomb()
    {
        Debug.Log("ShakaCameraVidBomb anropad");
        if (cameraTransform == null)
        {
            Debug.LogError("F�rs�ker skaka kamera men cameraTransform �r null!");
            return;
        }
        StartCoroutine(Skaka(standardIntensitet, standardTid));
    }

    public void ShakaCameraVidFiendePlanKollision()
    {
        // Kan ha specifika parametrar f�r intensitet och varaktighet
        StartCoroutine(Skaka(0.2f, 0.3f)); // L�gre intensitet, kortare varaktighet
    }

    public void ShakaCameraVidBossD�d()
    {
        Debug.Log("ShakaCameraVidBossD�d anropad");
        if (cameraTransform == null)
        {
            Debug.LogError("F�rs�ker skaka kamera men cameraTransform �r null!");
            return;
        }
        StartCoroutine(Skaka(bossIntensitet, bossTid));
    }

    private IEnumerator Skaka(float intensitet, float varaktighet)
    {
        //Debug.Log($"Startar skakning med intensitet {intensitet} och varaktighet {varaktighet}");
        isShaking = true;
        float f�rflutenTid = 0f;
        Vector3 startPos = cameraTransform.localPosition;

        while (f�rflutenTid < varaktighet)
        {
            if (!isShaking)
            {
                Debug.Log("Skakning avbruten");
                break;
            }

            f�rflutenTid += Time.deltaTime;
            float d�mpning = 1.0f - Mathf.Clamp01(f�rflutenTid / varaktighet);

            float x = Random.Range(-1f, 1f) * intensitet * d�mpning;
            float y = Random.Range(-1f, 1f) * intensitet * d�mpning;
            Vector3 nyPosition = originalPosition + new Vector3(x, y, 0);

            cameraTransform.localPosition = nyPosition;

            if (f�rflutenTid % 0.5f < Time.deltaTime) // Logga var 0.5 sekund
            {
                //Debug.Log($"Skakar kamera. Position: {nyPosition}, D�mpning: {d�mpning}");
            }

            yield return null;
        }

        //Debug.Log("Skakning klar, �terg�r till originalposition");
        StartCoroutine(�terg�TillOriginalPosition());
    }

    private IEnumerator �terg�TillOriginalPosition()
    {
        //Debug.Log($"B�rjar �terg� till originalPosition: {originalPosition}");

        while (Vector3.Distance(cameraTransform.localPosition, originalPosition) > 0.01f)
        {
            cameraTransform.localPosition = Vector3.Lerp(
                cameraTransform.localPosition,
                originalPosition,
                Time.deltaTime * d�mpningHastighet
            );
            yield return null;
        }

        cameraTransform.localPosition = originalPosition;
        isShaking = false;
        //Debug.Log("�terg�ng till originalposition klar");
    }

    public void StoppaSkakning()
    {
        Debug.Log("Stoppar skakning");
        isShaking = false;
        StopAllCoroutines();
        StartCoroutine(�terg�TillOriginalPosition());
    }
}
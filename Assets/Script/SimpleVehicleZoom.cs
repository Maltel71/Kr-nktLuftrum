using UnityEngine;
using System.Collections;

public class SimpleVehicleZoom : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private bool enableCameraZoom = true;
    [SerializeField] private float zoomDuration = 3f;
    [SerializeField] private float zoomInSize = 20f;
    [SerializeField] private float normalSize = 25f;
    [SerializeField] private float zoomSpeed = 3f;

    [Header("Zoom Trigger")]
    [SerializeField] private ZoomTriggerType triggerType = ZoomTriggerType.OnStart;
    [SerializeField] private float delayBeforeZoom = 0f;

    public enum ZoomTriggerType
    {
        OnStart,        // Zooma när fordonet spawnar
        OnPlayerNear,   // Zooma när spelaren kommer nära
        Manual          // Zooma bara när du anropar ZoomToThis()
    }

    [Header("Player Detection (för OnPlayerNear)")]
    [SerializeField] private float detectionRange = 30f;

    private Camera mainCamera;
    private Vector3 originalCameraPosition;
    private bool isZooming = false;
    private bool hasTriggeredZoom = false;
    private Transform player;

    private void Start()
    {
        // Setup kamera
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            normalSize = mainCamera.orthographicSize;
            originalCameraPosition = mainCamera.transform.localPosition;
        }

        // Hitta spelaren
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Trigga zoom baserat på inställning
        if (triggerType == ZoomTriggerType.OnStart)
        {
            if (delayBeforeZoom > 0)
            {
                StartCoroutine(DelayedZoom());
            }
            else
            {
                ZoomToThis();
            }
        }
    }

    private void Update()
    {
        // Kolla spelare-närhet om det är valt som trigger
        if (triggerType == ZoomTriggerType.OnPlayerNear && !hasTriggeredZoom && player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= detectionRange)
            {
                ZoomToThis();
            }
        }
    }

    private IEnumerator DelayedZoom()
    {
        yield return new WaitForSeconds(delayBeforeZoom);
        ZoomToThis();
    }

    /// <summary>
    /// Zooma till detta fordon - kan anropas från andra scripts
    /// </summary>
    public void ZoomToThis()
    {
        if (!enableCameraZoom || hasTriggeredZoom || isZooming) return;

        hasTriggeredZoom = true;
        StartCoroutine(ZoomSequence());
        Debug.Log($"Camera zooming to {gameObject.name} for {zoomDuration} seconds");
    }

    /// <summary>
    /// Avbryt pågående zoom
    /// </summary>
    public void CancelZoom()
    {
        if (isZooming)
        {
            StopAllCoroutines();
            StartCoroutine(ZoomOut());
        }
    }

    private IEnumerator ZoomSequence()
    {
        if (mainCamera == null) yield break;

        isZooming = true;

        // Zoom in
        yield return StartCoroutine(ZoomIn());

        // Håll zoom
        yield return new WaitForSeconds(zoomDuration);

        // Zoom ut
        yield return StartCoroutine(ZoomOut());

        isZooming = false;
    }

    private IEnumerator ZoomIn()
    {
        float elapsedTime = 0f;
        float startSize = mainCamera.orthographicSize;
        Vector3 startPos = mainCamera.transform.position;
        Vector3 targetPos = transform.position + new Vector3(0, 20f, -5f);
        float zoomTime = 1f / zoomSpeed;

        while (elapsedTime < zoomTime)
        {
            float t = elapsedTime / zoomTime;
            t = Mathf.SmoothStep(0f, 1f, t);

            mainCamera.orthographicSize = Mathf.Lerp(startSize, zoomInSize, t);
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Säkerställ slutvärden
        mainCamera.orthographicSize = zoomInSize;
        mainCamera.transform.position = targetPos;
    }

    private IEnumerator ZoomOut()
    {
        float elapsedTime = 0f;
        float startSize = mainCamera.orthographicSize;
        Vector3 startPos = mainCamera.transform.position;
        Vector3 originalPos = mainCamera.transform.parent.position + originalCameraPosition;
        float zoomTime = 1f / zoomSpeed;

        while (elapsedTime < zoomTime)
        {
            float t = elapsedTime / zoomTime;
            t = Mathf.SmoothStep(0f, 1f, t);

            mainCamera.orthographicSize = Mathf.Lerp(startSize, normalSize, t);
            mainCamera.transform.position = Vector3.Lerp(startPos, originalPos, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Återställ till original
        mainCamera.orthographicSize = normalSize;
        mainCamera.transform.localPosition = originalCameraPosition;
    }

    /// <summary>
    /// Återställ zoom-triggern så den kan köras igen
    /// </summary>
    public void ResetZoomTrigger()
    {
        hasTriggeredZoom = false;
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        if (triggerType == ZoomTriggerType.OnPlayerNear)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }

    // Cleanup när objektet förstörs
    private void OnDestroy()
    {
        if (isZooming && mainCamera != null)
        {
            mainCamera.orthographicSize = normalSize;
            mainCamera.transform.localPosition = originalCameraPosition;
        }
    }
}
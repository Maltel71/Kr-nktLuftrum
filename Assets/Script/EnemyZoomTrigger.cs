using UnityEngine;
using System.Collections;

public class EnemyZoomTrigger : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private bool enableZoom = true;
    [SerializeField] private float zoomDuration = 3f;
    [SerializeField] private float zoomInSize = 100f;
    [SerializeField] private float normalSize = 500f;
    [SerializeField] private float zoomSpeed = 2f;

    [Header("Camera Positioning")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 100f, 0);
    [SerializeField] private bool followTarget = true;

    private Camera mainCamera;
    private bool hasTriggered = false;
    private bool isZooming = false;

    void Start()
    {
        mainCamera = Camera.main;

        // Sätt normal storlek om kameran har fel storlek
        if (mainCamera != null && mainCamera.orthographicSize != normalSize)
        {
            Debug.Log($"Setting camera to normal size: {normalSize}");
            mainCamera.orthographicSize = normalSize;
        }
    }

    public void TriggerZoom()
    {
        if (!enableZoom || hasTriggered || isZooming || mainCamera == null) return;

        hasTriggered = true;
        StartCoroutine(ZoomSequence());
        Debug.Log($"Zoom triggered on {gameObject.name}");
    }

    private IEnumerator ZoomSequence()
    {
        isZooming = true;

        // Spara ursprunglig kamera-info
        float originalSize = mainCamera.orthographicSize;
        Vector3 originalPos = mainCamera.transform.position;
        Transform originalParent = mainCamera.transform.parent;

        Debug.Log($"Starting zoom sequence: {originalSize} -> {zoomInSize}");

        // Zoom in
        yield return StartCoroutine(ZoomToSize(originalSize, zoomInSize));

        // Håll zoom
        Debug.Log($"Holding zoom for {zoomDuration} seconds");
        yield return new WaitForSeconds(zoomDuration);

        // Zoom ut
        yield return StartCoroutine(ZoomToSize(zoomInSize, normalSize));

        // Återställ kamera
        if (originalParent != null)
        {
            mainCamera.transform.SetParent(originalParent);
            mainCamera.transform.position = originalPos;
        }

        Debug.Log("Zoom sequence complete");
        isZooming = false;
    }

    private IEnumerator ZoomToSize(float fromSize, float toSize)
    {
        float elapsedTime = 0f;
        float duration = 1f / zoomSpeed;

        Vector3 startPos = mainCamera.transform.position;
        Vector3 targetPos = followTarget ? transform.position + cameraOffset : startPos;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            // Interpolera zoom
            mainCamera.orthographicSize = Mathf.Lerp(fromSize, toSize, t);

            // Interpolera position om vi ska följa
            if (followTarget)
            {
                Vector3 currentTargetPos = transform.position + cameraOffset;
                mainCamera.transform.position = Vector3.Lerp(startPos, currentTargetPos, t);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Sätt exakt slutvärden
        mainCamera.orthographicSize = toSize;
        if (followTarget)
        {
            mainCamera.transform.position = transform.position + cameraOffset;
        }
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        if (!enableZoom) return;

        // Rita zoom-position
        Gizmos.color = Color.cyan;
        Vector3 zoomPos = transform.position + cameraOffset;
        Gizmos.DrawWireSphere(zoomPos, 5f);
        Gizmos.DrawLine(transform.position, zoomPos);

        // Rita zoom-området
        Gizmos.color = Color.blue;
        float zoomRadius = zoomInSize;
        Gizmos.DrawWireSphere(transform.position, zoomRadius);
    }

    // Publika metoder för konfiguration
    public void SetZoomSettings(float inSize, float duration, float speed)
    {
        zoomInSize = inSize;
        zoomDuration = duration;
        zoomSpeed = speed;
    }

    public void SetCameraOffset(Vector3 offset)
    {
        cameraOffset = offset;
    }
}
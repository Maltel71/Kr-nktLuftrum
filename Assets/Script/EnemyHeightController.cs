using UnityEngine;

public class EnemyHeightController : MonoBehaviour
{
    [SerializeField] private Transform playerPlane;
    [SerializeField] private float heightMatchSpeed = 5f;
    [SerializeField] private float targetHeightOffset = 0f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    private bool isEnabled = true;

    private void Start()
    {
        if (playerPlane == null)
        {
            playerPlane = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerPlane == null)
            {
                Debug.LogWarning("Kunde inte hitta spelaren! Se till att spelaren har taggen 'Player'");
            }
        }

        if (playerPlane != null)
        {
            Vector3 pos = transform.position;
            pos.y = playerPlane.position.y + targetHeightOffset;
            transform.position = pos;
        }
    }

    private void Update()
    {
        if (!isEnabled || playerPlane == null) return;

        Vector3 currentPos = transform.position;
        float targetHeight = playerPlane.position.y + targetHeightOffset;
        float newHeight = Mathf.Lerp(currentPos.y, targetHeight, Time.deltaTime * heightMatchSpeed);
        transform.position = new Vector3(currentPos.x, newHeight, currentPos.z);

        if (showDebugInfo)
        {
            float heightDifference = Mathf.Abs(currentPos.y - playerPlane.position.y);
            //Debug.Log($"Höjdskillnad till spelare: {heightDifference:F2} enheter");
        }
    }

    public void SetHeightOffset(float newOffset)
    {
        targetHeightOffset = newOffset;
    }

    public void DisableHeightControl()
    {
        isEnabled = false;
    }
}
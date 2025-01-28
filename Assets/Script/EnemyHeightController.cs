using UnityEngine;

public class EnemyHeightController : MonoBehaviour
{
    // Variabler som kan justeras i Unity Inspector
    [SerializeField] private Transform playerPlane;         // Referens till spelarens flygplan
    [SerializeField] private float heightMatchSpeed = 5f;   // Hur snabbt fienden anpassar sin h�jd
    [SerializeField] private float targetHeightOffset = 0f; // Om fienden ska vara �ver/under spelaren

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;    // F�r att visa debug information

    private void Start()
    {
        // Om ingen spelare �r tilldelad, hitta spelaren med Player-taggen
        if (playerPlane == null)
        {
            playerPlane = GameObject.FindGameObjectWithTag("Player")?.transform;

            if (playerPlane == null)
            {
                Debug.LogWarning("Kunde inte hitta spelaren! Se till att spelaren har taggen 'Player'");
            }
        }

        // S�tt initial h�jd till samma som spelaren
        if (playerPlane != null)
        {
            Vector3 pos = transform.position;
            pos.y = playerPlane.position.y + targetHeightOffset;
            transform.position = pos;
        }
    }

    private void Update()
    {
        if (playerPlane != null)
        {
            // H�mta nuvarande position
            Vector3 currentPos = transform.position;

            // Ber�kna m�lh�jden (spelarens h�jd plus offset)
            float targetHeight = playerPlane.position.y + targetHeightOffset;

            // Anv�nd Lerp f�r mjuk �verg�ng till ny h�jd
            // Time.deltaTime * heightMatchSpeed avg�r hur snabbt h�jden �ndras
            float newHeight = Mathf.Lerp(currentPos.y, targetHeight, Time.deltaTime * heightMatchSpeed);

            // Uppdatera positionen med den nya h�jden
            transform.position = new Vector3(currentPos.x, newHeight, currentPos.z);

            // Visa debug information om aktiverat
            if (showDebugInfo)
            {
                float heightDifference = Mathf.Abs(currentPos.y - playerPlane.position.y);
                //Debug.Log($"H�jdskillnad till spelare: {heightDifference:F2} enheter");
            }
        }
    }

    // Metod f�r att manuellt s�tta h�jdoffset under spelets g�ng
    public void SetHeightOffset(float newOffset)
    {
        targetHeightOffset = newOffset;
    }
}
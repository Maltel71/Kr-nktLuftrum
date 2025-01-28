using UnityEngine;

public class EnemyHeightController : MonoBehaviour
{
    // Variabler som kan justeras i Unity Inspector
    [SerializeField] private Transform playerPlane;         // Referens till spelarens flygplan
    [SerializeField] private float heightMatchSpeed = 5f;   // Hur snabbt fienden anpassar sin höjd
    [SerializeField] private float targetHeightOffset = 0f; // Om fienden ska vara över/under spelaren

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;    // För att visa debug information

    private void Start()
    {
        // Om ingen spelare är tilldelad, hitta spelaren med Player-taggen
        if (playerPlane == null)
        {
            playerPlane = GameObject.FindGameObjectWithTag("Player")?.transform;

            if (playerPlane == null)
            {
                Debug.LogWarning("Kunde inte hitta spelaren! Se till att spelaren har taggen 'Player'");
            }
        }

        // Sätt initial höjd till samma som spelaren
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
            // Hämta nuvarande position
            Vector3 currentPos = transform.position;

            // Beräkna målhöjden (spelarens höjd plus offset)
            float targetHeight = playerPlane.position.y + targetHeightOffset;

            // Använd Lerp för mjuk övergång till ny höjd
            // Time.deltaTime * heightMatchSpeed avgör hur snabbt höjden ändras
            float newHeight = Mathf.Lerp(currentPos.y, targetHeight, Time.deltaTime * heightMatchSpeed);

            // Uppdatera positionen med den nya höjden
            transform.position = new Vector3(currentPos.x, newHeight, currentPos.z);

            // Visa debug information om aktiverat
            if (showDebugInfo)
            {
                float heightDifference = Mathf.Abs(currentPos.y - playerPlane.position.y);
                //Debug.Log($"Höjdskillnad till spelare: {heightDifference:F2} enheter");
            }
        }
    }

    // Metod för att manuellt sätta höjdoffset under spelets gång
    public void SetHeightOffset(float newOffset)
    {
        targetHeightOffset = newOffset;
    }
}
using UnityEngine;
using UnityEngine.Splines;

public class SplineTriggerEnter : MonoBehaviour
{
    [SerializeField] private GameObject[] splineGroupObjects;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Kontrollera att det är spelaren
        {
            ActivateSplineGroup();
        }
    }

    private void ActivateSplineGroup()
    {
        foreach (GameObject splineObject in splineGroupObjects)
        {
            if (splineObject != null)
            {
                var splineController = splineObject.GetComponent<EnemySplineController>();
                var splineAnimate = splineObject.GetComponent<SplineAnimate>();

                if (splineController != null)
                {
                    splineController.enabled = true; // Aktivera spline-rörelsen
                }

                if (splineAnimate != null)
                {
                    splineAnimate.Play(); // Starta animationen direkt
                }
            }
        }
    }
}
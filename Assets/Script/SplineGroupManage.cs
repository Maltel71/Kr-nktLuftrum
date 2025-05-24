using Unity.Splines;
using UnityEngine;
using UnityEngine.Splines;

public class SplineGroupManager : MonoBehaviour
{
    [Header("Group Settings")]
    [SerializeField] private Color groupColor = Color.red;
    [SerializeField] private string groupType = "Wave Group";

    void OnDrawGizmos()
    {
        // Hitta child splines varje gång för att undvika cache-problem
        SplineContainer[] childSplines = GetComponentsInChildren<SplineContainer>();

        if (childSplines == null || childSplines.Length == 0)
            return;

        // Använd denna gruppens färg
        Gizmos.color = groupColor;

        foreach (SplineContainer spline in childSplines)
        {
            if (spline != null)
            {
                Vector3 pos = spline.transform.position;

                // Rita solid sfär med gruppens färg
                Gizmos.DrawSphere(pos, 25f);

                // Rita wireframe med samma färg
                Gizmos.color = groupColor;
                Gizmos.DrawWireSphere(pos, 35f);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        SplineContainer[] childSplines = GetComponentsInChildren<SplineContainer>();

        if (childSplines == null || childSplines.Length == 0)
            return;

        // Använd en ljusare version när vald
        Gizmos.color = Color.Lerp(groupColor, Color.white, 0.3f);

        foreach (SplineContainer spline in childSplines)
        {
            if (spline != null)
            {
                Vector3 pos = spline.transform.position;
                Gizmos.DrawSphere(pos, 30f);
            }
        }
    }
}
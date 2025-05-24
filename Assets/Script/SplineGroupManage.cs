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
        // Hitta child splines varje g�ng f�r att undvika cache-problem
        SplineContainer[] childSplines = GetComponentsInChildren<SplineContainer>();

        if (childSplines == null || childSplines.Length == 0)
            return;

        // Anv�nd denna gruppens f�rg
        Gizmos.color = groupColor;

        foreach (SplineContainer spline in childSplines)
        {
            if (spline != null)
            {
                Vector3 pos = spline.transform.position;

                // Rita solid sf�r med gruppens f�rg
                Gizmos.DrawSphere(pos, 25f);

                // Rita wireframe med samma f�rg
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

        // Anv�nd en ljusare version n�r vald
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
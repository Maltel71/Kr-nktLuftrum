using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemyFormationController : MonoBehaviour
{
   
    public enum FormationType
    {
        Line,
        Triangle,
        Circle,
        V_Formation
    }

    [SerializeField] private FormationType currentFormation = FormationType.Line;
    [SerializeField] private float formationSpacing = 5f;
    [SerializeField] private float formationChangeInterval = 30f;

    private List<Transform> formationMembers = new List<Transform>();
    private float nextFormationChangeTime;

    private void Start()
    {
        FindFormationMembers();
    }

    private void Update()
    {
        if (Time.time >= nextFormationChangeTime)
        {
            ChangeFormation();
            nextFormationChangeTime = Time.time + formationChangeInterval;
        }

        UpdateFormationPositions();
    }

    private void FindFormationMembers()
    {
        // Hitta alla fiender med detta script
        formationMembers = new List<Transform>(
            FindObjectsOfType<EnemyFormationController>().Select(f => f.transform)
        );
    }

    private void ChangeFormation()
    {
        // Rotera genom formationstyper
        currentFormation = (FormationType)(((int)currentFormation + 1) %
            System.Enum.GetValues(typeof(FormationType)).Length);
    }

    private void UpdateFormationPositions()
    {
        switch (currentFormation)
        {
            case FormationType.Line:
                UpdateLineFormation();
                break;
            case FormationType.Triangle:
                UpdateTriangleFormation();
                break;
            case FormationType.Circle:
                UpdateCircleFormation();
                break;
            case FormationType.V_Formation:
                UpdateVFormation();
                break;
        }
    }

    private void UpdateLineFormation()
    {
        for (int i = 0; i < formationMembers.Count; i++)
        {
            Vector3 offset = new Vector3(i * formationSpacing, 0, 0);
            formationMembers[i].position = transform.position + offset;
        }
    }

    // Implementera övriga formationsmetoder
    private void UpdateTriangleFormation() { /* ... */ }
    private void UpdateCircleFormation() { /* ... */ }
    private void UpdateVFormation() { /* ... */ }
}
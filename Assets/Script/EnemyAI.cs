using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("AI Beteende")]
    [SerializeField] private float decisionInterval = 2f;
    [SerializeField] private float detectionRange = 30f;
    [SerializeField] private float retreatHealthThreshold = 0.3f;

    // Gör enumerationen publik
    public enum AIState
    {
        Attacking,
        Retreating,
        Patrolling
    }

    private AIState currentState = AIState.Patrolling;
    private Transform target;
    private EnemyHealth healthSystem;
    private float nextDecisionTime;

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player")?.transform;
        healthSystem = GetComponent<EnemyHealth>();
    }

    private void Update()
    {
        if (Time.time >= nextDecisionTime)
        {
            MakeDecision();
            nextDecisionTime = Time.time + decisionInterval;
        }

        ExecuteCurrentState();
    }

    private void MakeDecision()
    {
        if (healthSystem.GetHealthPercentage() <= retreatHealthThreshold)
        {
            currentState = AIState.Retreating;
            return;
        }

        float distanceToTarget = target != null
            ? Vector3.Distance(transform.position, target.position)
            : float.MaxValue;

        if (distanceToTarget <= detectionRange)
        {
            currentState = AIState.Attacking;
        }
        else
        {
            currentState = AIState.Patrolling;
        }
    }

    private void ExecuteCurrentState()
    {
        switch (currentState)
        {
            case AIState.Attacking:
                // Implementera attacklogik
                break;
            case AIState.Retreating:
                // Implementera reträttprocedur
                break;
            case AIState.Patrolling:
                // Implementera patrulleringslogik
                break;
        }
    }

    // Returnera den publika AIState
    public AIState GetCurrentState() => currentState;
}
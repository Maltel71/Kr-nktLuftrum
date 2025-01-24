using UnityEngine;
using UnityEngine.Splines;

public class EnemySplineController : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private float speed = 5f;
    [SerializeField] private bool loopPath = true;

    private float currentDistance = 0f;
    private float splineLength;
    private SplineAnimate splineAnimate;

    private void Start()
    {
        splineLength = splineContainer.CalculateLength();
        splineAnimate = GetComponent<SplineAnimate>();

        if (splineAnimate == null)
        {
            splineAnimate = gameObject.AddComponent<SplineAnimate>();
        }

        splineAnimate.Container = splineContainer;
        splineAnimate.Loop = loopPath ? SplineAnimate.LoopMode.Loop : SplineAnimate.LoopMode.Once;
    }

    private void Update()
    {
        currentDistance += speed * Time.deltaTime;

        if (loopPath)
        {
            currentDistance %= splineLength;
        }
        else if (currentDistance > splineLength)
        {
            currentDistance = splineLength;
        }

        float normalizedDistance = currentDistance / splineLength;
        Vector3 position = splineContainer.EvaluatePosition(normalizedDistance);
        transform.position = position;
    }
}
using UnityEngine;
using System.Collections;

public class AirplaneController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float smoothness = 0.1f;
    [SerializeField] private float backwardSpeedMultiplier = 1.5f;

    [Header("Movement Boundaries")]
    [SerializeField] private float maxForwardDistance = 4f;
    [SerializeField] private float maxBackwardDistance = -2f;
    [SerializeField] private float horizontalBoundary = 8f;

    private Vector2 touchStart;
    private Vector2 movement;
    private Vector3 velocity = Vector3.zero;
    private bool isTouching = false;
    private Vector3 startPosition;
    private bool isFrozen = false;
    private Rigidbody rb;
    private float originalMoveSpeed;

    private void Start()
    {
        startPosition = transform.position;
        originalMoveSpeed = moveSpeed;
        rb = GetComponent<Rigidbody>();

        Collider col = GetComponent<Collider>();
        Debug.Log($"Flygplan Collider finns: {col != null}, Är Trigger: {col?.isTrigger}");
        Debug.Log($"Flygplan Rigidbody finns: {rb != null}, Är Kinematic: {rb?.isKinematic}");
    }

    private void Update()
    {
        if (isFrozen)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            return;
        }

        HandleInput();
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (!isFrozen)
        {
            float currentSpeed = movement.y < 0 ? moveSpeed * backwardSpeedMultiplier : moveSpeed;
            Vector3 targetPosition = transform.position + new Vector3(movement.x, 0, movement.y) * currentSpeed * Time.deltaTime;
            targetPosition.x = Mathf.Clamp(targetPosition.x, startPosition.x - horizontalBoundary, startPosition.x + horizontalBoundary);
            targetPosition.z = Mathf.Clamp(targetPosition.z, startPosition.z + maxBackwardDistance, startPosition.z + maxForwardDistance);
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothness);
        }
    }

    private void HandleInput()
    {
        movement = Vector2.zero;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStart = touch.position;
                    isTouching = true;
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (isTouching)
                    {
                        Vector2 touchDelta = touch.position - touchStart;
                        movement = touchDelta.normalized;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isTouching = false;
                    break;
            }
        }

#if UNITY_EDITOR
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        if (horizontalInput != 0 || verticalInput != 0)
        {
            movement = new Vector2(horizontalInput, verticalInput).normalized;
        }
#endif
    }

    public void FreezePosition()
    {
        isFrozen = true;
        velocity = Vector3.zero;
        movement = Vector2.zero;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        this.enabled = true;
    }

    public void UnfreezePosition()
    {
        isFrozen = false;
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }

    public void ResetPosition()
    {
        transform.position = startPosition;
        velocity = Vector3.zero;
        movement = Vector2.zero;
        isFrozen = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
        }
    }

    public void SetMovementBoundaries(float forward, float backward, float horizontal)
    {
        maxForwardDistance = forward;
        maxBackwardDistance = backward;
        horizontalBoundary = horizontal;
    }

    public IEnumerator ApplySpeedBoost(float multiplier, float duration)
    {
        moveSpeed *= multiplier;

        GameMessageSystem messageSystem = FindObjectOfType<GameMessageSystem>();
        if (messageSystem != null)
        {
            messageSystem.ShowBoostMessage("Speed Boost");
        }

        yield return new WaitForSeconds(duration);

        moveSpeed = originalMoveSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"SPELARE kolliderade med: {collision.gameObject.name}");
        Debug.Log($"- Layer: {LayerMask.LayerToName(collision.gameObject.layer)}");
        Debug.Log($"- Tag: {collision.gameObject.tag}");
    }
}
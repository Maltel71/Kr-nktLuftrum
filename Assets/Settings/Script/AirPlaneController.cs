using UnityEngine;

public class AirplaneController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float smoothness = 0.1f;

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
    private Rigidbody rb; // Reference till Rigidbody om det finns

    private void Start()
    {
        startPosition = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (isFrozen)
        {
            // Om planet är fryst, stoppa all rörelse omedelbart
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
            Vector3 targetPosition = transform.position + new Vector3(movement.x, 0, movement.y) * moveSpeed * Time.deltaTime;
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

        // Om det finns en Rigidbody, hantera den också
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; // Gör Rigidbody opåverkad av fysik
        }

        // Se till att scriptet �r aktivt men stoppar r�relse
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
}
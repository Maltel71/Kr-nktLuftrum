using UnityEngine;

public class AirplaneController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float smoothness = 0.1f;

    [Header("Screen Boundaries")]
    [SerializeField] private float horizontalBoundary = 8f;
    [SerializeField] private float verticalBoundary = 4f;

    private Vector2 touchStart;
    private Vector2 movement;
    private Vector3 velocity = Vector3.zero;
    private bool isTouching = false;

    private void Update()
    {
        // Hantera input för både touch och tangentbord
        HandleInput();

        // Beräkna ny position
        Vector3 targetPosition = transform.position + new Vector3(movement.x, 0, movement.y) * moveSpeed * Time.deltaTime;

        // Begränsa position inom skärmgränserna
        targetPosition.x = Mathf.Clamp(targetPosition.x, -horizontalBoundary, horizontalBoundary);
        targetPosition.z = Mathf.Clamp(targetPosition.z, -verticalBoundary, verticalBoundary);

        // Applicera mjuk rörelse
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothness);
    }

    private void HandleInput()
    {
        // Återställ movement
        movement = Vector2.zero;

        // Hantera touch input
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
                        // Beräkna förflyttning baserat på touch-position
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

        // Hantera tangentbordsinput för testning i Unity Editor
#if UNITY_EDITOR
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        if (horizontalInput != 0 || verticalInput != 0)
        {
            movement = new Vector2(horizontalInput, verticalInput).normalized;
        }
#endif
    }

    // Funktion för att justera inställningar via Unity Inspector
    public void SetMovementSettings(float speed, float smooth)
    {
        moveSpeed = speed;
        smoothness = smooth;
    }

    // Funktion för att justera skärmgränser
    public void SetBoundaries(float horizontal, float vertical)
    {
        horizontalBoundary = horizontal;
        verticalBoundary = vertical;
    }
}
using UnityEngine;
using System.Collections;
using TMPro;

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

    [Header("Flare System")]
    [SerializeField] private GameObject flarePrefab;
    [SerializeField] private Transform flareSpawnPoint;
    [SerializeField] private float flareOffset = 2f;
    [SerializeField] private int startFlares = 0;
    [SerializeField] private int maxFlares = 10;
    [SerializeField] private float flareCooldown = 1f;
    private int currentFlares;
    private float nextFlareTime;

    [Header("Missile System")]
    [SerializeField] private GameObject MissilePrefab;
    [SerializeField] private Transform missileSpawnPoint;
    [SerializeField] private float missileOffset = 2f;
    [SerializeField] private int startMissiles = 0;
    [SerializeField] private int maxMissiles = 5;
    [SerializeField] private float missileCooldown = 1f;
    private int currentMissiles;
    private float nextmissileTime;

    [Header("Animation")]
    [SerializeField] private Animator planeAnimator;
    [SerializeField] private float animationThreshold = 0.1f;
    private static readonly int MoveL = Animator.StringToHash("Move_L");
    private static readonly int MoveR = Animator.StringToHash("Move_R");
    private static readonly int IsDead = Animator.StringToHash("isDead");

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI flaresText;
    [SerializeField] private TextMeshProUGUI missilesText;

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
        InitializeComponents();
        SetupInitialState();
    }

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody>();

        // Hitta Animator explicit
        planeAnimator = GetComponentInChildren<Animator>();

        if (planeAnimator == null)
        {
            Debug.LogError("KRITISKT: Ingen Animator hittad på flygplanet eller dess barn!");

            // Sök igenom alla barn
            Animator[] childAnimators = GetComponentsInChildren<Animator>();
            foreach (var animator in childAnimators)
            {
                Debug.Log($"Hittade Animator på: {animator.gameObject.name}");
            }
        }
        else
        {
            Debug.Log($"Animator hittad på: {planeAnimator.gameObject.name}");
        }
    }

    private void SetupInitialState()
    {
        startPosition = transform.position;
        originalMoveSpeed = moveSpeed;
        currentFlares = startFlares;
        currentMissiles = startMissiles;
        UpdateUI();
    }

    private void Update()
    {
        if (isFrozen)
        {
            HandleFrozenState();
            return;
        }

        HandleInput();
        UpdatePosition();
        HandleWeaponInput();
        UpdateUI();
    }

    private void HandleFrozenState()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (planeAnimator != null)
        {
            planeAnimator.SetBool(MoveL, false);
            planeAnimator.SetBool(MoveR, false);
            planeAnimator.SetBool(IsDead, true);
        }
    }

    private void HandleWeaponInput()
    {
        if (Input.GetKeyDown(KeyCode.F) && CanShootFlare())
        {
            ShootFlare();
        }

        if (Input.GetKeyDown(KeyCode.G) && CanShootMissile())
        {
            ShootMissile();
        }
    }

    private bool CanShootFlare()
    {
        return currentFlares > 0 && Time.time >= nextFlareTime;
    }

    private void ShootFlare()
    {
        GameObject flare = Instantiate(flarePrefab,
            flareSpawnPoint.position,
            flareSpawnPoint.rotation);

        currentFlares--;
        nextFlareTime = Time.time + flareCooldown;

        AudioManager.Instance?.PlayFlareSound();
        UpdateUI();
        Debug.Log($"Flare skjuten! Återstående flares: {currentFlares}");
    }

    private bool CanShootMissile()
    {
        return currentMissiles > 0 && Time.time >= nextmissileTime;
    }

    private void ShootMissile()
    {
        GameObject missile = Instantiate(MissilePrefab,
            missileSpawnPoint.position,
            missileSpawnPoint.rotation);

        if (missile.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = transform.forward * 50f;
            rb.freezeRotation = true;
        }

        currentMissiles--;
        nextmissileTime = Time.time + missileCooldown;

        AudioManager.Instance?.PlayMissileLaunchSound();
        UpdateUI();
        Debug.Log("Missile fired!");
    }

    private void UpdateUI()
    {
        if (flaresText != null)
        {
            flaresText.text = $"Flares: {currentFlares}";
        }
        if (missilesText != null)
        {
            missilesText.text = $"Missiles: {currentMissiles}";
        }
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
            HandleTouchInput();
        }

#if UNITY_EDITOR
        HandleKeyboardInput();
#endif
        //Debug.Log($"Movement efter input: X={movement.x}, Y={movement.y}");

        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        if (planeAnimator == null)
        {
            Debug.LogError("Ingen Animator hittad!");
            return;
        }

        // Skriv ut mer detaljerad information
        //Debug.Log($"Animation Debug - Movement X: {movement.x}, Threshold: {animationThreshold}");

        // Mer explicit logik för animationer
        bool isMovingLeft = movement.x < -animationThreshold;
        bool isMovingRight = movement.x > animationThreshold;

        // Sätt animationsparametrar
        planeAnimator.SetBool("Move_L", isMovingLeft);
        planeAnimator.SetBool("Move_R", isMovingRight);

        // Detaljerad loggning
        //if (isMovingLeft)
        //    Debug.Log("Triggering Left Movement Animation");
        //else if (isMovingRight)
        //   Debug.Log("Triggering Right Movement Animation");
        //else
           // Debug.Log("No significant movement");
    }

    private void HandleTouchInput()
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


    private void HandleKeyboardInput()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        if (horizontalInput != 0 || verticalInput != 0)
        {
            movement = new Vector2(horizontalInput, verticalInput).normalized;
            // Debug för input
            //Debug.Log($"Keyboard Input - Horizontal: {horizontalInput}, Movement X: {movement.x}");
            //Debug.Log($"Rörelse efter input - X: {movement.x}, Y: {movement.y}");
        }
    }

    public void AddFlares(int amount)
    {
        currentFlares = Mathf.Min(currentFlares + amount, maxFlares);
        UpdateUI();
        Debug.Log($"Added {amount} flares. Total: {currentFlares}");
    }

    public void AddMissiles(int amount)
    {
        currentMissiles = Mathf.Min(currentMissiles + amount, maxMissiles);
        UpdateUI();
        Debug.Log($"Added {amount} missiles. Total: {currentMissiles}");
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

        if (planeAnimator != null)
        {
            planeAnimator.SetBool(IsDead, true);
        }
    }

    public void UnfreezePosition()
    {
        isFrozen = false;
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        if (planeAnimator != null)
        {
            planeAnimator.SetBool(IsDead, false);
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

        if (planeAnimator != null)
        {
            planeAnimator.SetBool(IsDead, false);
            planeAnimator.SetBool(MoveL, false);
            planeAnimator.SetBool(MoveR, false);
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
        float originalMoveSpeed = moveSpeed;
        moveSpeed *= multiplier;
        yield return new WaitForSeconds(duration);
        moveSpeed = originalMoveSpeed;
    }

    public void ResetMoveSpeed()
    {
        moveSpeed = originalMoveSpeed;
    }

    public float GetOriginalMoveSpeed()
    {
        return originalMoveSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"SPELARE kolliderade med: {collision.gameObject.name}");
        Debug.Log($"- Layer: {LayerMask.LayerToName(collision.gameObject.layer)}");
        Debug.Log($"- Tag: {collision.gameObject.tag}");
    }
}
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

    [Header("Bomb System")]
    [SerializeField] private Transform bombSpawnPoint;
    [SerializeField] private float bombOffset = 2f;
    [SerializeField] private int startBombs = 0;
    [SerializeField] private int maxBombs = 5;
    [SerializeField] private float bombCooldown = 1f;
    private int currentBombs;
    private float nextBombTime;

    [Header("Animation")]
    [SerializeField] private Animator planeAnimator;
    [SerializeField] private float animationThreshold = 0.1f;
    private static readonly int MoveL = Animator.StringToHash("Move_L");
    private static readonly int MoveR = Animator.StringToHash("Move_R");
    private static readonly int IsDead = Animator.StringToHash("isDead");

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI flaresText;
    [SerializeField] private TextMeshProUGUI missilesText;
    [SerializeField] private TextMeshProUGUI bombsText;

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
        planeAnimator = GetComponentInChildren<Animator>();

        if (planeAnimator == null)
        {
            Debug.LogError("KRITISKT: Ingen Animator hittad på flygplanet eller dess barn!");
            Animator[] childAnimators = GetComponentsInChildren<Animator>();
            foreach (var animator in childAnimators)
            {
                Debug.Log($"Hittade Animator på: {animator.gameObject.name}");
            }
        }
    }

    private void SetupInitialState()
    {
        startPosition = transform.position;
        originalMoveSpeed = moveSpeed;
        currentFlares = startFlares;
        currentMissiles = startMissiles;
        currentBombs = startBombs;
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

        if (Input.GetKeyDown(KeyCode.B) && CanDropBomb())
        {
            DropBomb();
        }
    }

    private bool CanShootFlare()
    {
        return currentFlares > 0 && Time.time >= nextFlareTime;
    }

    private void ShootFlare()
    {
        GameObject flare = Instantiate(flarePrefab, flareSpawnPoint.position, flareSpawnPoint.rotation);
        currentFlares--;
        nextFlareTime = Time.time + flareCooldown;
        AudioManager.Instance?.PlayFlareSound();
        UpdateUI();
    }

    private bool CanShootMissile()
    {
        return currentMissiles > 0 && Time.time >= nextmissileTime;
    }

    private void ShootMissile()
    {
        GameObject missile = Instantiate(MissilePrefab, missileSpawnPoint.position, missileSpawnPoint.rotation);
        if (missile.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = transform.forward * 50f;
            rb.freezeRotation = true;
        }

        currentMissiles--;
        nextmissileTime = Time.time + missileCooldown;
        AudioManager.Instance?.PlayMissileLaunchSound();
        UpdateUI();
    }

    private bool CanDropBomb()
    {
        return currentBombs > 0 && Time.time >= nextBombTime;
    }

    private void DropBomb()
    {
        GameObject bomb = ShellAndBombPool.Instance.GetBomb();
        bomb.transform.position = bombSpawnPoint.position;
        bomb.transform.rotation = bombSpawnPoint.rotation;

        if (bomb.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(Vector3.down * 200f, ForceMode.Impulse);
        }

        currentBombs--;
        nextBombTime = Time.time + bombCooldown;
        AudioManager.Instance?.PlayBombSound(BombSoundType.Drop);
        UpdateUI();
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
        if (bombsText != null)
        {
            bombsText.text = $"Bombs: {currentBombs}";
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
        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        if (planeAnimator == null) return;

        bool isMovingLeft = movement.x < -animationThreshold;
        bool isMovingRight = movement.x > animationThreshold;

        planeAnimator.SetBool("Move_L", isMovingLeft);
        planeAnimator.SetBool("Move_R", isMovingRight);
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
        }
    }

    public void AddFlares(int amount)
    {
        currentFlares = Mathf.Min(currentFlares + amount, maxFlares);
        UpdateUI();
    }

    public void AddMissiles(int amount)
    {
        currentMissiles = Mathf.Min(currentMissiles + amount, maxMissiles);
        UpdateUI();
    }

    public void AddBombs(int amount)
    {
        currentBombs = Mathf.Min(currentBombs + amount, maxBombs);
        UpdateUI();
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
        Debug.Log($"Speed Boost started. Original speed: {moveSpeed}, Multiplier: {multiplier}, Duration: {duration}");

        float originalMoveSpeed = moveSpeed;
        moveSpeed *= multiplier;

        yield return new WaitForSeconds(duration);

        moveSpeed = originalMoveSpeed;
        Debug.Log($"Speed Boost ended. Restored to original speed: {moveSpeed}");
    }

    //public IEnumerator ApplySpeedBoost(float multiplier, float duration)
    //{
    //    float originalMoveSpeed = moveSpeed;
    //    moveSpeed *= multiplier;
    //    yield return new WaitForSeconds(duration);
    //    moveSpeed = originalMoveSpeed;
    //}

    public void ResetMoveSpeed()
    {
        moveSpeed = originalMoveSpeed;
    }

    public float GetOriginalMoveSpeed()
    {
        return originalMoveSpeed;
    }
}
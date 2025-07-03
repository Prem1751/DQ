using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SimpleRunMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;

    [Header("Animation Settings")]
    [SerializeField] private string idleBool = "IsIdle";
    [SerializeField] private string walkBool = "IsWalking";
    [SerializeField] private string runBool = "IsRunning";
    [SerializeField] private string speedParam = "Speed";

    [Header("Visual Settings")]
    [SerializeField] private bool flipSprite = true;
    [SerializeField] private ParticleSystem runParticles;

    [Header("Map Boundaries")]
    [SerializeField] private SpriteRenderer[] backgroundSprites; // Assign all background sprites in the inspector
    [SerializeField] private float edgePadding = 0.5f; // Padding to prevent character from going off-screen

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    private float horizontalInput;
    private bool isRunning;
    private bool movementEnabled = true;
    private float currentSpeed;
    private bool isFacingRight = true;
    private float minXBoundary;
    private float maxXBoundary;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (animator == null) animator = GetComponent<Animator>();

        CalculateMapBoundaries();
    }

    private void CalculateMapBoundaries()
    {
        if (backgroundSprites == null || backgroundSprites.Length == 0)
        {
            Debug.LogWarning("No background sprites assigned for boundary calculation!");
            minXBoundary = -Mathf.Infinity;
            maxXBoundary = Mathf.Infinity;
            return;
        }

        // Initialize with first sprite's bounds
        Bounds combinedBounds = backgroundSprites[0].bounds;

        // Combine bounds of all background sprites
        for (int i = 1; i < backgroundSprites.Length; i++)
        {
            if (backgroundSprites[i] != null)
            {
                combinedBounds.Encapsulate(backgroundSprites[i].bounds);
            }
        }

        // Calculate boundaries with padding
        minXBoundary = combinedBounds.min.x + edgePadding;
        maxXBoundary = combinedBounds.max.x - edgePadding;

        Debug.Log($"Map boundaries calculated: MinX={minXBoundary}, MaxX={maxXBoundary}");
    }

    private void Update()
    {
        if (!movementEnabled) return;

        GetInput();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (!movementEnabled) return;
        HandleMovement();
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        isRunning = Input.GetKey(runKey);

        if (flipSprite && horizontalInput != 0)
        {
            isFacingRight = horizontalInput > 0;
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x) * (isFacingRight ? 1 : -1),
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }

    private void HandleMovement()
    {
        float targetSpeed = isRunning ? runSpeed : walkSpeed;
        targetSpeed *= horizontalInput;

        currentSpeed = Mathf.Lerp(
            currentSpeed,
            targetSpeed,
            (Mathf.Abs(targetSpeed) > 0.1f ? acceleration : deceleration) * Time.fixedDeltaTime
        );

        // Calculate new position
        Vector2 newPosition = rb.position + Vector2.right * currentSpeed * Time.fixedDeltaTime;

        // Clamp position to boundaries
        newPosition.x = Mathf.Clamp(newPosition.x, minXBoundary, maxXBoundary);

        // Only move if we're not at the boundary or moving away from it
        if ((newPosition.x > rb.position.x && newPosition.x < maxXBoundary) ||
            (newPosition.x < rb.position.x && newPosition.x > minXBoundary) ||
            (newPosition.x == rb.position.x))
        {
            rb.MovePosition(newPosition);
        }

        if (runParticles != null)
        {
            if (Mathf.Abs(currentSpeed) > 0.1f && isRunning && !runParticles.isPlaying)
                runParticles.Play();
            else if ((Mathf.Abs(currentSpeed) < 0.1f || !isRunning) && runParticles.isPlaying)
                runParticles.Stop();
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        bool isMoving = Mathf.Abs(currentSpeed) > 0.1f;
        float speedPercent = Mathf.Abs(currentSpeed) / (isRunning ? runSpeed : walkSpeed);

        animator.SetBool(idleBool, !isMoving);
        animator.SetBool(walkBool, isMoving && !isRunning);
        animator.SetBool(runBool, isMoving && isRunning);
        animator.SetFloat(speedParam, speedPercent);
    }

    public void SetMovement(bool canMove) => movementEnabled = canMove;
    public bool IsMoving() => movementEnabled && Mathf.Abs(horizontalInput) > 0.1f;
    public bool IsRunning() => movementEnabled && isRunning;

    // Call this when you add/remove background sprites at runtime
    public void UpdateBoundaries()
    {
        CalculateMapBoundaries();
    }

    // Draw boundaries in editor for visualization
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying && backgroundSprites != null && backgroundSprites.Length > 0)
        {
            Bounds combinedBounds = backgroundSprites[0].bounds;
            for (int i = 1; i < backgroundSprites.Length; i++)
            {
                if (backgroundSprites[i] != null)
                {
                    combinedBounds.Encapsulate(backgroundSprites[i].bounds);
                }
            }

            float minX = combinedBounds.min.x + edgePadding;
            float maxX = combinedBounds.max.x - edgePadding;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(minX, combinedBounds.min.y - 10, 0), new Vector3(minX, combinedBounds.max.y + 10, 0));
            Gizmos.DrawLine(new Vector3(maxX, combinedBounds.min.y - 10, 0), new Vector3(maxX, combinedBounds.max.y + 10, 0));
        }
    }
}
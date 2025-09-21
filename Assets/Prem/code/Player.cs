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
    [SerializeField] private string attackBool = "IsAttacking"; // เพิ่มพารามิเตอร์การโจมตี
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string directionParam = "Direction"; // พารามิเตอร์ทิศทาง

    [Header("Visual Settings")]
    [SerializeField] private bool flipSprite = true;
    [SerializeField] private ParticleSystem runParticles;

    [Header("Map Boundaries")]
    [SerializeField] private SpriteRenderer[] backgroundSprites;
    [SerializeField] private float edgePadding = 0.5f;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    private float horizontalInput;
    private bool isRunning;
    private bool isAttacking; // สถานะการโจมตี
    private bool movementEnabled = true;
    private float currentSpeed;
    private bool isFacingRight = true;
    private bool isMovingRight = true; // ทิศทางการเคลื่อนที่ปัจจุบัน
    private float minXBoundary;
    private float maxXBoundary;
    private float idleDirection = 1f; // ทิศทางเริ่มต้นเมื่อหยุด (1 = ขวา, -1 = ซ้าย)

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

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

        Bounds combinedBounds = backgroundSprites[0].bounds;
        for (int i = 1; i < backgroundSprites.Length; i++)
        {
            if (backgroundSprites[i] != null)
            {
                combinedBounds.Encapsulate(backgroundSprites[i].bounds);
            }
        }

        minXBoundary = combinedBounds.min.x + edgePadding;
        maxXBoundary = combinedBounds.max.x - edgePadding;
    }

    private void Update()
    {
        if (!movementEnabled) return;

        GetInput();
        HandleAttackInput(); // จัดการการโจมตี
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

        // อัปเดตทิศทางการเคลื่อนที่
        if (horizontalInput != 0)
        {
            isMovingRight = horizontalInput > 0;
        }

        // จัดการการพลิก sprite ตามการเคลื่อนที่
        if (flipSprite && horizontalInput != 0)
        {
            bool shouldFaceRight = horizontalInput > 0;

            if (shouldFaceRight != isFacingRight)
            {
                isFacingRight = shouldFaceRight;
                FlipCharacter();
            }
        }
    }

    private void HandleAttackInput()
    {
        // ตรวจสอบการกดปุ่มโจมตี (ตัวอย่างใช้ Space)
        if (Input.GetKeyDown(KeyCode.Space) && !isAttacking)
        {
            StartAttack();
        }
    }

    private void StartAttack()
    {
        isAttacking = true;

        // ตั้งค่าทิศทางโจมตีตามทิศทางที่กำลังหันอยู่
        float attackDirection = isFacingRight ? 1f : -1f;
        animator.SetFloat(directionParam, attackDirection);

        // เริ่มอนิเมชันโจมตี
        animator.SetBool(attackBool, true);

        // พักการเคลื่อนไหวชั่วคราวขณะโจมตี
        movementEnabled = false;

        Debug.Log($"เริ่มโจมตี ทิศทาง: {(attackDirection > 0 ? "ขวา" : "ซ้าย")}");

        // รีเซ็ตสถานะโจมตีหลังจากอนิเมชันจบ
        Invoke("EndAttack", 0.5f); // ปรับเวลาตามความยาวอนิเมชันโจมตี
    }

    private void EndAttack()
    {
        isAttacking = false;
        animator.SetBool(attackBool, false);
        movementEnabled = true;

        // กลับไปหันทิศทางตามการเคลื่อนที่
        UpdateFacingDirection();
    }

    private void FlipCharacter()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !isFacingRight;
        }
        else
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (isFacingRight ? 1 : -1);
            transform.localScale = scale;
        }
    }

    private void UpdateFacingDirection()
    {
        // อัปเดตทิศทางที่หันตามการเคลื่อนที่
        if (horizontalInput != 0)
        {
            isFacingRight = horizontalInput > 0;
            FlipCharacter();
        }
        else if (!isAttacking)
        {
            // เมื่อหยุดเคลื่อนที่ ให้หันตามทิศทางเริ่มต้น
            isFacingRight = idleDirection > 0;
            FlipCharacter();
        }
    }

    private void HandleMovement()
    {
        if (isAttacking) return; // ไม่เคลื่อนที่ขณะโจมตี

        float targetSpeed = isRunning ? runSpeed : walkSpeed;
        targetSpeed *= horizontalInput;

        currentSpeed = Mathf.Lerp(
            currentSpeed,
            targetSpeed,
            (Mathf.Abs(targetSpeed) > 0.1f ? acceleration : deceleration) * Time.fixedDeltaTime
        );

        Vector2 newPosition = rb.position + Vector2.right * currentSpeed * Time.fixedDeltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, minXBoundary, maxXBoundary);

        if ((newPosition.x > rb.position.x && newPosition.x < maxXBoundary) ||
            (newPosition.x < rb.position.x && newPosition.x > minXBoundary) ||
            (newPosition.x == rb.position.x))
        {
            rb.MovePosition(newPosition);
        }

        // จัดการ particle effects
        HandleParticles();
    }

    private void HandleParticles()
    {
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

        bool isMoving = Mathf.Abs(currentSpeed) > 0.1f && !isAttacking;
        float speedPercent = Mathf.Abs(currentSpeed) / (isRunning ? runSpeed : walkSpeed);

        // ตั้งค่าพารามิเตอร์อนิเมชัน
        animator.SetBool(idleBool, !isMoving && !isAttacking);
        animator.SetBool(walkBool, isMoving && !isRunning);
        animator.SetBool(runBool, isMoving && isRunning);
        animator.SetFloat(speedParam, speedPercent);

        // ตั้งค่าทิศทางในอนิเมชัน
        float currentDirection = isMoving ? (isMovingRight ? 1f : -1f) : idleDirection;
        animator.SetFloat(directionParam, currentDirection);
    }

    public void SetMovement(bool canMove) => movementEnabled = canMove;
    public bool IsMoving() => movementEnabled && Mathf.Abs(horizontalInput) > 0.1f;
    public bool IsRunning() => movementEnabled && isRunning;
    public bool IsAttacking() => isAttacking;

    public void UpdateBoundaries()
    {
        CalculateMapBoundaries();
    }

    // ตั้งค่าทิศทางเริ่มต้นเมื่อหยุด
    public void SetIdleDirection(float direction)
    {
        idleDirection = Mathf.Sign(direction);
        if (!IsMoving() && !isAttacking)
        {
            isFacingRight = idleDirection > 0;
            FlipCharacter();
        }
    }

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
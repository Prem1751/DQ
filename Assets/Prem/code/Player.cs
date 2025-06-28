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
    [SerializeField] private string speedParam = "Speed"; // เปลี่ยนจาก float เป็น string เพราะเป็นชื่อพารามิเตอร์

    [Header("Visual Settings")]
    [SerializeField] private bool flipSprite = true;
    [SerializeField] private ParticleSystem runParticles;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    private float horizontalInput;
    private bool isRunning;
    private bool movementEnabled = true;
    private float currentSpeed;
    private bool isFacingRight = true;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (animator == null) animator = GetComponent<Animator>();
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

        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);

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

        // ตั้งค่า Boolean พารามิเตอร์
        animator.SetBool(idleBool, !isMoving);
        animator.SetBool(walkBool, isMoving && !isRunning);
        animator.SetBool(runBool, isMoving && isRunning);

        // ตั้งค่า Speed parameter สำหรับ blend tree (ถ้ามี)
        animator.SetFloat(speedParam, speedPercent);
    }

    public void SetMovement(bool canMove) => movementEnabled = canMove;
    public bool IsMoving() => movementEnabled && Mathf.Abs(horizontalInput) > 0.1f;
    public bool IsRunning() => movementEnabled && isRunning;
}
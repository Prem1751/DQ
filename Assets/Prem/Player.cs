using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SimpleRunMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;

    [Header("Visual Settings")]
    [SerializeField] private bool flipSprite = true;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    private Vector2 movementInput;
    private bool isRunning;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        // รับค่าการเคลื่อนที่
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");

        // ตรวจสอบการกดปุ่มวิ่ง
        isRunning = Input.GetKey(runKey);

        // พลิก Sprite ตามทิศทาง
        if (flipSprite && movementInput.x != 0)
        {
            spriteRenderer.flipX = movementInput.x < 0;
        }

        // อัปเดต Animation
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        // คำนวณความเร็วปัจจุบัน
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // เคลื่อนที่ทันทีโดยไม่มีแรงเฉื่อย
        rb.linearVelocity = movementInput.normalized * currentSpeed;
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        animator.SetFloat("Horizontal", movementInput.x);
        animator.SetFloat("Vertical", movementInput.y);
        animator.SetFloat("Speed", movementInput.magnitude);
        animator.SetBool("IsRunning", isRunning);

        // เก็บทิศทางล่าสุดสำหรับ Idle Animation
        if (movementInput.magnitude > 0.1f)
        {
            animator.SetFloat("LastHorizontal", movementInput.x);
            animator.SetFloat("LastVertical", movementInput.y);
        }
    }

    // ฟังก์ชันตรวจสอบสถานะ
    public bool IsMoving()
    {
        return movementInput.magnitude > 0.1f;
    }

    public bool IsRunning()
    {
        return isRunning;
    }
}
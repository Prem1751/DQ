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
    private bool movementEnabled = true; // เพิ่มตัวแปรควบคุมการเคลื่อนที่

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        if (!movementEnabled) return; // ไม่ประมวลผลถ้าเคลื่อนที่ไม่ได้

        // รับค่าการเคลื่อนที่
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");

        // ตรวจสอบการกดปุ่มวิ่ง
        isRunning = Input.GetKey(runKey);

        if (flipSprite && movementInput.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (movementInput.x > 0 ? 1 : -1);
            transform.localScale = scale;
        }

        // อัปเดต Animation
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (!movementEnabled) // หยุดเคลื่อนที่ถ้าถูกปิด
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // คำนวณความเร็วปัจจุบัน
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // เคลื่อนที่ทันทีโดยไม่มีแรงเฉื่อย
        rb.linearVelocity = movementInput.normalized * currentSpeed;
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        // อัปเดตพารามิเตอร์ Animator
        float speed = movementEnabled ? movementInput.magnitude : 0;
        animator.SetFloat("Horizontal", movementEnabled ? movementInput.x : 0);
        animator.SetFloat("Vertical", movementEnabled ? movementInput.y : 0);
        animator.SetFloat("Speed", speed);
        animator.SetBool("IsRunning", movementEnabled && isRunning);

        // เก็บทิศทางล่าสุดสำหรับ Idle Animation
        if (movementInput.magnitude > 0.1f)
        {
            animator.SetFloat("LastHorizontal", movementInput.x);
            animator.SetFloat("LastVertical", movementInput.y);
        }
    }

    // ฟังก์ชันสำหรับระบบสนทนา (ใหม่)
    public void SetMovement(bool canMove)
    {
        movementEnabled = canMove;

        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero; // หยุดการเคลื่อนที่ทันที
            if (animator != null)
            {
                animator.SetFloat("Speed", 0); // รีเซ็ต Animation
            }
        }
    }

    // ฟังก์ชันตรวจสอบสถานะ (เดิม)
    public bool IsMoving()
    {
        return movementEnabled && movementInput.magnitude > 0.1f;
    }

    public bool IsRunning()
    {
        return movementEnabled && isRunning;
    }
}
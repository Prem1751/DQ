using UnityEngine;

public class ItemCollector : MonoBehaviour
{
    [Header("การตั้งค่า")]
    public float collectDistance = 1.5f; // ระยะที่ผู้เล่นสามารถเก็บไอเทมได้
    public KeyCode collectKey = KeyCode.F; // ปุ่มสำหรับเก็บไอเทม

    private GameObject player;
    private bool isPlayerInRange = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        // ถ้าหาไม่พบผู้เล่น สามารถแสดงข้อความเตือนได้
        // if (player == null) Debug.LogError("ไม่พบผู้เล่น! ตรวจสอบว่าผู้เล่นมีแท็ก 'Player' หรือไม่");
    }

    void Update()
    {
        if (player == null) return;

        // คำนวณระยะห่างระหว่างผู้เล่นกับไอเทม
        float distance = Vector2.Distance(transform.position, player.transform.position);

        // ตรวจสอบว่าผู้เล่นอยู่ในระยะเก็บไอเทมหรือไม่
        if (distance <= collectDistance)
        {
            isPlayerInRange = true;

            // ตรวจสอบการกดปุ่ม
            if (Input.GetKeyDown(collectKey))
            {
                CollectItem();
            }
        }
        else
        {
            isPlayerInRange = false;
        }
    }

    void CollectItem()
    {
        // เพิ่มเอฟเฟกต์เมื่อเก็บไอเทม (เช่น เสียง, เพิ่มคะแนน)
        Debug.Log("เก็บไอเทมแล้ว!");

        // ทำลายวัตถุ
        Destroy(gameObject);
    }

    // ส่วนเสริม: แสดงระยะเก็บไอเทมใน Scene View
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectDistance);
    }
}
using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // ตัวละครผู้เล่นที่จะให้กล้องตาม
    public bool autoFindPlayer = true;

    [Header("Follow Settings")]
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Background Boundaries")]
    public SpriteRenderer[] backgrounds; // ลากพื้นหลังทั้งหมดมาใส่ที่นี่
    private float minCamX, maxCamX, minCamY, maxCamY;
    private bool boundsActive = false;

    void Start()
    {
        if (autoFindPlayer)
        {
            FindPlayerTarget();
        }

        CalculateCameraBounds();
    }

    void FindPlayerTarget()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) target = playerObj.transform;
    }

    void CalculateCameraBounds()
    {
        if (backgrounds == null || backgrounds.Length == 0)
        {
            Debug.LogWarning("ไม่ได้กำหนดพื้นหลัง - ระบบขอบเขตจะไม่ทำงาน");
            boundsActive = false;
            return;
        }

        // เริ่มต้นด้วยขอบเขตของพื้นหลังแรก
        SpriteRenderer firstBg = backgrounds[0];
        float camHeight = Camera.main.orthographicSize;
        float camWidth = camHeight * Camera.main.aspect;

        float bgWidth = firstBg.bounds.size.x / 2f;
        float bgHeight = firstBg.bounds.size.y / 2f;

        minCamX = firstBg.transform.position.x - bgWidth + camWidth;
        maxCamX = firstBg.transform.position.x + bgWidth - camWidth;
        minCamY = firstBg.transform.position.y - bgHeight + camHeight;
        maxCamY = firstBg.transform.position.y + bgHeight - camHeight;

        // ตรวจสอบพื้นหลังอื่นๆเพื่อหาขอบเขตที่ใหญ่ที่สุด
        for (int i = 1; i < backgrounds.Length; i++)
        {
            if (backgrounds[i] == null) continue;

            bgWidth = backgrounds[i].bounds.size.x / 2f;
            bgHeight = backgrounds[i].bounds.size.y / 2f;

            float currentMinX = backgrounds[i].transform.position.x - bgWidth + camWidth;
            float currentMaxX = backgrounds[i].transform.position.x + bgWidth - camWidth;
            float currentMinY = backgrounds[i].transform.position.y - bgHeight + camHeight;
            float currentMaxY = backgrounds[i].transform.position.y + bgHeight - camHeight;

            // ขยายขอบเขตหากพื้นหลังนี้ใหญ่กว่า
            minCamX = Mathf.Min(minCamX, currentMinX);
            maxCamX = Mathf.Max(maxCamX, currentMaxX);
            minCamY = Mathf.Min(minCamY, currentMinY);
            maxCamY = Mathf.Max(maxCamY, currentMaxY);
        }

        boundsActive = true;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            if (autoFindPlayer)
            {
                FindPlayerTarget();
                if (target == null) return;
            }
            else return;
        }

        Vector3 desiredPosition = target.position + offset;

        // จำกัดขอบเขตตามพื้นหลังทั้งหมด
        if (boundsActive)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minCamX, maxCamX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minCamY, maxCamY);
        }

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    void OnDrawGizmosSelected()
    {
        if (backgrounds != null && backgrounds.Length > 0)
        {
            // วาดขอบเขตของพื้นหลังทั้งหมด
            Gizmos.color = Color.green;
            foreach (var bg in backgrounds)
            {
                if (bg != null)
                {
                    Gizmos.DrawWireCube(bg.bounds.center, bg.bounds.size);
                }
            }

            // วาดขอบเขตที่กล้องสามารถเคลื่อนที่ได้ (ถ้ามีการคำนวณแล้ว)
            if (Application.isPlaying && boundsActive)
            {
                Vector3 cameraBoundsCenter = new Vector3(
                    (minCamX + maxCamX) / 2f,
                    (minCamY + maxCamY) / 2f,
                    0
                );
                Vector3 cameraBoundsSize = new Vector3(
                    maxCamX - minCamX,
                    maxCamY - minCamY,
                    0.1f
                );
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(cameraBoundsCenter, cameraBoundsSize);
            }
        }
    }

    // ฟังก์ชันสำหรับอัพเดทขอบเขตใหม่ (เรียกใช้เมื่อเปลี่ยนพื้นหลังในเกม)
    public void UpdateBackgroundBounds()
    {
        CalculateCameraBounds();
    }
}
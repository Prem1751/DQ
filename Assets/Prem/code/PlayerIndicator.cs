using UnityEngine;

public class PlayerIndicator : MonoBehaviour
{
    public float floatHeight = 0.5f; // ความสูงของการลอยขึ้นลง
    public float floatSpeed = 1f; // ความเร็วของการลอย

    private Vector3 startPosition;
    private float randomOffset;

    void Start()
    {
        startPosition = transform.localPosition;
        randomOffset = Random.Range(0f, 2f * Mathf.PI); // ทำให้การเคลื่อนไหวไม่ซ้ำกัน
    }

    void Update()
    {
        // เคลื่อนที่ขึ้นลงแบบ Sine Wave
        float newY = startPosition.y + Mathf.Sin((Time.time + randomOffset) * floatSpeed) * floatHeight;
        transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);

        // หมุนให้หันหน้าเข้าหากล้อง (สำหรับ 2D อาจไม่จำเป็น)
        transform.rotation = Quaternion.identity;
    }
}
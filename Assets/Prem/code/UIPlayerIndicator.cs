using UnityEngine;

public class UIPlayerIndicator : MonoBehaviour
{
    public Transform playerTransform;
    public float floatHeight = 50f; // ในหน่วย pixel
    public float floatSpeed = 1f;

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private float randomOffset;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
        randomOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        // อัพเดทตำแหน่งตามผู้เล่น (สำหรับ World Space Canvas)
        if (playerTransform != null)
        {
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(playerTransform.position);
            rectTransform.position = screenPoint;
        }

        // เคลื่อนไหวขึ้นลง
        float newY = originalPosition.y + Mathf.Sin((Time.time + randomOffset) * floatSpeed) * floatHeight;
        rectTransform.anchoredPosition = new Vector2(originalPosition.x, newY);
    }
}
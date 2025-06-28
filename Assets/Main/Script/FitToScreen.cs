using UnityEngine;

public class FitToScreen : MonoBehaviour
{
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // ตั้งค่าให้วัตถุอยู่ที่ตำแหน่งกลางจอ
        transform.position = new Vector3(0, 0, 0);

        // คำนวณขนาดของภาพ
        float width = sr.sprite.bounds.size.x;
        float height = sr.sprite.bounds.size.y;

        // คำนวณขนาดหน้าจอโลก
        float worldScreenHeight = Camera.main.orthographicSize * 2.0f;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        // ปรับขนาดให้พอดีกับหน้าจอ
        transform.localScale = new Vector3(worldScreenWidth / width, worldScreenHeight / height, 1);
    }
}
using UnityEngine;

public class FitToScreen : MonoBehaviour
{
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // ��駤������ѵ����������˹觡�ҧ��
        transform.position = new Vector3(0, 0, 0);

        // �ӹǳ��Ҵ�ͧ�Ҿ
        float width = sr.sprite.bounds.size.x;
        float height = sr.sprite.bounds.size.y;

        // �ӹǳ��Ҵ˹�Ҩ��š
        float worldScreenHeight = Camera.main.orthographicSize * 2.0f;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        // ��Ѻ��Ҵ���ʹաѺ˹�Ҩ�
        transform.localScale = new Vector3(worldScreenWidth / width, worldScreenHeight / height, 1);
    }
}
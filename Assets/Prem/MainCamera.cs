using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // ����Фü����蹷��������ͧ���
    public bool autoFindPlayer = true;

    [Header("Follow Settings")]
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Background Boundaries")]
    public SpriteRenderer background; // �ҡ�����ѧ���������
    private float minCamX, maxCamX, minCamY, maxCamY;

    void Start()
    {
        if (autoFindPlayer)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) target = playerObj.transform;
        }

        CalculateCameraBounds();
    }

    void CalculateCameraBounds()
    {
        if (background == null)
        {
            Debug.LogWarning("������˹������ѧ - �к��ͺࢵ�����ӧҹ");
            return;
        }

        // �ӹǳ��Ҵ�ͧ���ͧ
        float camHeight = Camera.main.orthographicSize;
        float camWidth = camHeight * Camera.main.aspect;

        // �ӹǳ�ͺࢵ�ͧ�����ѧ
        float bgWidth = background.bounds.size.x / 2f;
        float bgHeight = background.bounds.size.y / 2f;

        // �ӹǳ�ͺࢵ�����ͧ����ö����͹�����
        minCamX = background.transform.position.x - bgWidth + camWidth;
        maxCamX = background.transform.position.x + bgWidth - camWidth;
        minCamY = background.transform.position.y - bgHeight + camHeight;
        maxCamY = background.transform.position.y + bgHeight - camHeight;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            if (autoFindPlayer)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) target = playerObj.transform;
                else return;
            }
            else return;
        }

        Vector3 desiredPosition = target.position + offset;

        // �ӡѴ�ͺࢵ��������ѧ
        if (background != null)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minCamX, maxCamX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minCamY, maxCamY);
        }

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    void OnDrawGizmosSelected()
    {
        if (background != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(background.bounds.center, background.bounds.size);

            // �Ҵ�ͺࢵ�����ͧ����ö����͹�����
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
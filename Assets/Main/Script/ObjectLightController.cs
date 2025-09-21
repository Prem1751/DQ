using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ObjectLightController : MonoBehaviour
{
    [Header("Light Settings")]
    public GameObject lightPrefab;
    public float activationDistance = 5f;
    public bool destroyWhenFar = true;

    [Header("Sorting Settings")]
    public string sortingLayerName = "Default";
    public int orderInLayer = -1;

    private GameObject player;
    private GameObject currentLight;
    private Light2D lightComponent;
    private bool isLightActive = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("Player not found! Make sure your player has the 'Player' tag.");
        }

        if (lightPrefab == null)
        {
            Debug.LogError("Light Prefab is not assigned!");
        }
    }

    void Update()
    {
        if (player == null || lightPrefab == null) return;

        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (distance <= activationDistance && !isLightActive)
        {
            ActivateLight();
        }
        else if (distance > activationDistance && isLightActive && destroyWhenFar)
        {
            DeactivateLight();
        }
    }

    void ActivateLight()
    {
        if (currentLight == null)
        {
            currentLight = Instantiate(lightPrefab, transform.position, Quaternion.identity);
            currentLight.transform.SetParent(transform);

            // ตั้งค่า Sorting ให้แสงอยู่ด้านหลัง
            SetLightSortingOrder();

            isLightActive = true;
            Debug.Log("Light activated behind object!");
        }
    }

    void DeactivateLight()
    {
        if (currentLight != null)
        {
            Destroy(currentLight);
            isLightActive = false;
            Debug.Log("Light deactivated!");
        }
    }

    void SetLightSortingOrder()
    {
        // หา SpriteRenderer ของวัตถุหลัก
        SpriteRenderer objectSprite = GetComponent<SpriteRenderer>();

        if (objectSprite != null)
        {
            // สำหรับ Unity เวอร์ชันใหม่ ใช้การตั้งค่าแบบนี้
            Light2D light2D = currentLight.GetComponent<Light2D>();
            if (light2D != null)
            {
                // ตั้งค่า Sorting Layer สำหรับ Light2D ในเวอร์ชันใหม่
                // ใช้ Renderer ในการควบคุม sorting แทน
                SetupLightRenderer(objectSprite);
            }
        }
        else
        {
            // ถ้าวัตถุไม่มี SpriteRenderer ให้ใช้ค่าที่กำหนดใน Inspector
            SetupSortingGroup();
        }
    }

    void SetupLightRenderer(SpriteRenderer objectSprite)
    {
        // วิธีที่ปลอดภัย: ใช้ Sorting Group สำหรับควบคุมลำดับ layer
        SortingGroup lightSortingGroup = currentLight.GetComponent<SortingGroup>();
        if (lightSortingGroup == null)
        {
            lightSortingGroup = currentLight.AddComponent<SortingGroup>();
        }

        lightSortingGroup.sortingLayerName = objectSprite.sortingLayerName;
        lightSortingGroup.sortingOrder = objectSprite.sortingOrder - 1;
    }

    void SetupSortingGroup()
    {
        SortingGroup lightSortingGroup = currentLight.GetComponent<SortingGroup>();
        if (lightSortingGroup == null)
        {
            lightSortingGroup = currentLight.AddComponent<SortingGroup>();
        }
        lightSortingGroup.sortingLayerName = sortingLayerName;
        lightSortingGroup.sortingOrder = orderInLayer;
    }

    // วิธีทางเลือก: ใช้ Renderer ในการควบคุม sorting
    void SetupRendererBasedSorting(SpriteRenderer objectSprite)
    {
        // เพิ่ม SpriteRenderer ให้กับแสง (ถ้ายังไม่มี)
        SpriteRenderer lightRenderer = currentLight.GetComponent<SpriteRenderer>();
        if (lightRenderer == null)
        {
            lightRenderer = currentLight.AddComponent<SpriteRenderer>();
            // ตั้งค่า sprite เป็นวงกลมสีขาวหรือ texture ของแสง
        }

        lightRenderer.sortingLayerName = objectSprite.sortingLayerName;
        lightRenderer.sortingOrder = objectSprite.sortingOrder - 1;

        // ทำให้แสงโปร่งใสบ้าง
        Color lightColor = lightRenderer.color;
        lightColor.a = 0.7f;
        lightRenderer.color = lightColor;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationDistance);
    }
}
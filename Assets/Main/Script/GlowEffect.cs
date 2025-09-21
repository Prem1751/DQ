using UnityEngine;

public class GlowEffect : MonoBehaviour
{
    [Header("Glow Settings")]
    public float glowDistance = 5f;
    public Color glowColor = Color.yellow;
    public float glowIntensity = 2f;

    [Header("Visual Settings")]
    public string sortingLayerName = "Overlay";
    public int sortingOrder = 5000;

    private GameObject player;
    private Material originalMaterial;
    private Material glowMaterial;
    private bool isGlowing = false;
    private GameObject glowObject;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        CreateGlowMaterial();

        // บันทึก material เดิม
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            originalMaterial = renderer.material;
        }
    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance <= glowDistance && !isGlowing)
            {
                EnableGlow();
            }
            else if (distance > glowDistance && isGlowing)
            {
                DisableGlow();
            }
        }
    }

    void CreateGlowMaterial()
    {
        // สร้าง material พิเศษสำหรับการเรืองแสง
        glowMaterial = new Material(Shader.Find("Standard"));
        glowMaterial.EnableKeyword("_EMISSION");
        glowMaterial.SetColor("_EmissionColor", glowColor * glowIntensity);
        glowMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

        // ตั้งค่าให้สว่างกว่าเดิม
        glowMaterial.SetFloat("_Metallic", 0f);
        glowMaterial.SetFloat("_Glossiness", 0.9f);
    }

    void EnableGlow()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = glowMaterial;
            isGlowing = true;

            // สร้างวัตถุเปล่งแสงเพิ่มเติม (optional)
            CreateAdditionalGlow();
        }

        Debug.Log("เปิดการเรืองแสง: " + gameObject.name);
    }

    void CreateAdditionalGlow()
    {
        // สร้าง sphere เปล่งแสงรอบๆ วัตถุ
        if (glowObject == null)
        {
            glowObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glowObject.name = "GlowEffect";
            glowObject.transform.SetParent(transform);
            glowObject.transform.localPosition = Vector3.zero;
            glowObject.transform.localScale = Vector3.one * 1.2f;

            // ตั้งค่า material
            Renderer glowRenderer = glowObject.GetComponent<Renderer>();
            Material glowMat = new Material(Shader.Find("Standard"));
            glowMat.EnableKeyword("_EMISSION");
            glowMat.SetColor("_EmissionColor", glowColor * glowIntensity * 0.5f);
            glowMat.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0.3f);
            glowRenderer.material = glowMat;

            // ตั้งค่า render order สูงสุด
            glowRenderer.sortingLayerName = sortingLayerName;
            glowRenderer.sortingOrder = sortingOrder;

            // ลบ collider ที่ไม่จำเป็น
            Destroy(glowObject.GetComponent<Collider>());
        }
    }

    void DisableGlow()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && originalMaterial != null)
        {
            renderer.material = originalMaterial;
            isGlowing = false;
        }

        // ลบวัตถุเปล่งแสง
        if (glowObject != null)
        {
            Destroy(glowObject);
            glowObject = null;
        }
    }

    void OnDrawGizmosSelected()
    {
        // วาดเส้นแสดงระยะการเรืองแสง
        Gizmos.color = glowColor;
        Gizmos.DrawWireSphere(transform.position, glowDistance);
    }
}
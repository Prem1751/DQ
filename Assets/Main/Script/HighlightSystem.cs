using UnityEngine;

public class HighlightSystem : MonoBehaviour
{
    [Header("Highlight Settings")]
    public float highlightDistance = 5f;
    public Color highlightColor = Color.yellow;
    public float highlightIntensity = 2f;

    [Header("Visual Effects")]
    public GameObject highlightEffect; // เอฟเฟกต์ visual ที่ต้องการแสดง
    public bool useEmission = true; // ใช้การเปล่งแสงหรือไม่

    private GameObject player;
    private Material originalMaterial;
    private Material highlightMaterial;
    private bool isHighlighted = false;
    private GameObject currentEffect;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        // สร้าง material สำหรับ highlight
        CreateHighlightMaterial();

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

            if (distance <= highlightDistance && !isHighlighted)
            {
                EnableHighlight();
            }
            else if (distance > highlightDistance && isHighlighted)
            {
                DisableHighlight();
            }
        }
    }

    void CreateHighlightMaterial()
    {
        // สร้าง material พิเศษสำหรับ highlight
        highlightMaterial = new Material(Shader.Find("Standard"));

        if (useEmission)
        {
            highlightMaterial.EnableKeyword("_EMISSION");
            highlightMaterial.SetColor("_EmissionColor", highlightColor * highlightIntensity);
            highlightMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        else
        {
            highlightMaterial.color = highlightColor;
        }
    }

    void EnableHighlight()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = highlightMaterial;
            isHighlighted = true;
        }

        // สร้างเอฟเฟกต์ visual ถ้ามี
        if (highlightEffect != null && currentEffect == null)
        {
            currentEffect = Instantiate(highlightEffect, transform.position, transform.rotation);
            currentEffect.transform.SetParent(transform);
            currentEffect.transform.localScale = Vector3.one;
        }

        Debug.Log("เปิดไฮไลต์: " + gameObject.name);
    }

    void DisableHighlight()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && originalMaterial != null)
        {
            renderer.material = originalMaterial;
            isHighlighted = false;
        }

        // ลบเอฟเฟกต์ visual
        if (currentEffect != null)
        {
            Destroy(currentEffect);
            currentEffect = null;
        }
    }

    void OnDrawGizmosSelected()
    {
        // วาดเส้นแสดงระยะการไฮไลต์
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, highlightDistance);
    }
}
using UnityEngine;

public class GlowHighlightEffect : MonoBehaviour
{
    [Header("Glow Settings")]
    public float highlightDistance = 5f;
    public Color glowColor = Color.yellow;
    public float glowIntensity = 2f;
    public float glowSize = 1.2f;

    [Header("Rendering Settings")]
    public string sortingLayerName = "Overlay";
    public int sortingOrder = 1000;

    private GameObject player;
    private Material originalMaterial;
    private Material glowMaterial;
    private bool isHighlighted = false;
    private GameObject glowEffectObject;

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

            if (distance <= highlightDistance && !isHighlighted)
            {
                EnableGlow();
            }
            else if (distance > highlightDistance && isHighlighted)
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
    }

    void EnableGlow()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = glowMaterial;
            isHighlighted = true;
        }

        // สร้าง Effect เรืองแสง
        CreateGlowEffect();

        Debug.Log("เปิดการเรืองแสง: " + gameObject.name);
    }

    void CreateGlowEffect()
    {
        if (glowEffectObject == null)
        {
            // สร้าง GameObject สำหรับ Effect
            glowEffectObject = new GameObject("GlowEffect");
            glowEffectObject.transform.SetParent(transform);
            glowEffectObject.transform.localPosition = Vector3.zero;
            glowEffectObject.transform.localRotation = Quaternion.identity;
            glowEffectObject.transform.localScale = Vector3.one * glowSize;

            // เพิ่ม Particle System
            ParticleSystem ps = glowEffectObject.AddComponent<ParticleSystem>();
            SetupParticleSystem(ps);

            // ตั้งค่า Render Order สูงๆ
            ParticleSystemRenderer psRenderer = glowEffectObject.GetComponent<ParticleSystemRenderer>();
            psRenderer.sortingLayerName = sortingLayerName;
            psRenderer.sortingOrder = sortingOrder;
        }
        else
        {
            glowEffectObject.SetActive(true);
        }
    }

    void SetupParticleSystem(ParticleSystem ps)
    {
        // ตั้งค่าหลัก
        var main = ps.main;
        main.loop = true;
        main.duration = 1f;
        main.startLifetime = 1f;
        main.startSpeed = 0f;
        main.startSize = 0.5f;
        main.maxParticles = 100;

        // ตั้งค่า Emission
        var emission = ps.emission;
        emission.rateOverTime = 30f;

        // ตั้งค่า Shape (ทรงกลมรอบวัตถุ)
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 1f;
        shape.radiusThickness = 0.8f;

        // ตั้งค่า Color
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(glowColor, 0.0f),
                new GradientColorKey(glowColor, 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.8f, 0.0f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        colorOverLifetime.color = gradient;

        // ตั้งค่า Renderer
        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.alignment = ParticleSystemRenderSpace.World;

        // ใช้ Material พิเศษสำหรับการเรืองแสง
        Material glowMat = new Material(Shader.Find("Particles/Standard Unlit"));
        glowMat.SetColor("_TintColor", new Color(glowColor.r, glowColor.g, glowColor.b, 0.3f));
        renderer.material = glowMat;
    }

    void DisableGlow()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && originalMaterial != null)
        {
            renderer.material = originalMaterial;
            isHighlighted = false;
        }

        // ปิด Effect เรืองแสง
        if (glowEffectObject != null)
        {
            glowEffectObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        // ลบ material ที่สร้างขึ้นเพื่อป้องกัน memory leak
        if (glowMaterial != null)
        {
            Destroy(glowMaterial);
        }

        if (glowEffectObject != null)
        {
            Destroy(glowEffectObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        // วาดเส้นแสดงระยะการเรืองแสง
        Gizmos.color = glowColor;
        Gizmos.DrawWireSphere(transform.position, highlightDistance);
    }
}
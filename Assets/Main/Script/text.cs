using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class TextObject2D : MonoBehaviour
{
    [Header("Text Settings")]
    public string textContent = "Hello World";
    public Color textColor = Color.white;
    public int fontSize = 24;
    public TextAlignmentOptions alignment = TextAlignmentOptions.Center;

    [Header("Font Settings")]
    public TMP_FontAsset fontAsset; // ฟอนต์ที่ต้องการใช้
    public FontStyles fontStyle = FontStyles.Normal;
    public bool autoSize = false;
    public float fontSizeMin = 12f;
    public float fontSizeMax = 36f;

    [Header("Sorting Settings")]
    public string sortingLayerName = "Default";
    public int orderInLayer = 0;

    [Header("Additional Effects")]
    public bool enableShadow = false;
    public Color shadowColor = new Color(0, 0, 0, 0.5f);
    public Vector2 shadowOffset = new Vector2(2, -2);

    public bool enableOutline = false;
    public Color outlineColor = Color.black;
    public float outlineWidth = 0.1f;

    private TextMeshPro textMeshPro;
    private GameObject textObject;

    void Start()
    {
        CreateTextObject();
    }

    void CreateTextObject()
    {
        // สร้าง GameObject สำหรับข้อความ
        textObject = new GameObject("TextObject");
        textObject.transform.SetParent(transform);
        textObject.transform.localPosition = Vector3.zero;

        // เพิ่ม TextMeshPro component
        textMeshPro = textObject.AddComponent<TextMeshPro>();

        // ตั้งค่าฟอนต์และข้อความ
        SetupFontAndText();

        // ตั้งค่า Sorting
        SetupTextSorting();

        // ตั้งค่าเพิ่มเติม
        textMeshPro.raycastTarget = false;
    }

    void SetupFontAndText()
    {
        // ตั้งค่าฟอนต์
        if (fontAsset != null)
        {
            textMeshPro.font = fontAsset;
        }
        else
        {
            // ใช้ฟอนต์ default ถ้าไม่ได้กำหนด
            textMeshPro.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }

        // ตั้งค่าข้อความ
        textMeshPro.text = textContent;
        textMeshPro.color = textColor;
        textMeshPro.fontSize = fontSize;
        textMeshPro.alignment = alignment;
        textMeshPro.enableWordWrapping = false;
        textMeshPro.fontStyle = fontStyle;

        // ตั้งค่า Auto Size
        textMeshPro.enableAutoSizing = autoSize;
        if (autoSize)
        {
            textMeshPro.fontSizeMin = fontSizeMin;
            textMeshPro.fontSizeMax = fontSizeMax;
        }

        // ตั้งค่าเอฟเฟกต์
        SetupTextEffects();
    }

    void SetupTextEffects()
    {
        // ตั้งค่า Shadow
        if (enableShadow)
        {
            textMeshPro.fontMaterial.EnableKeyword("UNDERLAY_ON");
            textMeshPro.fontMaterial.SetColor("_UnderlayColor", shadowColor);
            textMeshPro.fontMaterial.SetFloat("_UnderlayOffsetX", shadowOffset.x);
            textMeshPro.fontMaterial.SetFloat("_UnderlayOffsetY", shadowOffset.y);
            textMeshPro.fontMaterial.SetFloat("_UnderlayDilate", 0.5f);
        }
        else
        {
            textMeshPro.fontMaterial.DisableKeyword("UNDERLAY_ON");
        }

        // ตั้งค่า Outline
        if (enableOutline)
        {
            textMeshPro.outlineWidth = outlineWidth;
            textMeshPro.outlineColor = outlineColor;
        }
    }

    void SetupTextSorting()
    {
        // ใช้ Renderer ในการควบคุม Sorting
        Renderer textRenderer = textObject.GetComponent<Renderer>();
        if (textRenderer != null)
        {
            textRenderer.sortingLayerName = sortingLayerName;
            textRenderer.sortingOrder = orderInLayer;
        }

        // หรือใช้ Sorting Group (ถ้าต้องการ)
        SortingGroup sortingGroup = textObject.GetComponent<SortingGroup>();
        if (sortingGroup == null)
        {
            sortingGroup = textObject.AddComponent<SortingGroup>();
        }
        sortingGroup.sortingLayerName = sortingLayerName;
        sortingGroup.sortingOrder = orderInLayer;
    }

    // เมธอดสำหรับเปลี่ยนข้อความขณะ runtime
    public void SetText(string newText)
    {
        if (textMeshPro != null)
        {
            textMeshPro.text = newText;
        }
    }

    // เมธอดสำหรับเปลี่ยนสีข้อความ
    public void SetTextColor(Color newColor)
    {
        if (textMeshPro != null)
        {
            textMeshPro.color = newColor;
        }
    }

    // เมธอดสำหรับเปลี่ยนฟอนต์
    public void SetFont(TMP_FontAsset newFont)
    {
        if (textMeshPro != null && newFont != null)
        {
            textMeshPro.font = newFont;
            fontAsset = newFont;
        }
    }

    // เมธอดสำหรับเปลี่ยนขนาดฟอนต์
    public void SetFontSize(int newSize)
    {
        if (textMeshPro != null)
        {
            textMeshPro.fontSize = newSize;
            fontSize = newSize;
        }
    }

    // เมธอดสำหรับเปลี่ยนสไตล์ฟอนต์
    public void SetFontStyle(FontStyles newStyle)
    {
        if (textMeshPro != null)
        {
            textMeshPro.fontStyle = newStyle;
            fontStyle = newStyle;
        }
    }

    // เมธอดสำหรับเปิด/ปิด Auto Size
    public void SetAutoSize(bool enableAutoSize)
    {
        if (textMeshPro != null)
        {
            textMeshPro.enableAutoSizing = enableAutoSize;
            autoSize = enableAutoSize;
        }
    }

    // เมธอดสำหรับเปิด/ปิด Shadow
    public void SetShadow(bool enable, Color? color = null, Vector2? offset = null)
    {
        if (textMeshPro != null)
        {
            enableShadow = enable;
            if (color.HasValue) shadowColor = color.Value;
            if (offset.HasValue) shadowOffset = offset.Value;

            SetupTextEffects();
        }
    }

    // เมธอดสำหรับเปิด/ปิด Outline
    public void SetOutline(bool enable, Color? color = null, float? width = null)
    {
        if (textMeshPro != null)
        {
            enableOutline = enable;
            if (color.HasValue) outlineColor = color.Value;
            if (width.HasValue) outlineWidth = width.Value;

            SetupTextEffects();
        }
    }

    // เมธอดสำหรับรีเฟรชข้อความ (ใช้เมื่อแก้ไข properties ใน Inspector)
    void OnValidate()
    {
        if (textMeshPro != null && Application.isPlaying)
        {
            SetupFontAndText();
            SetupTextSorting();
        }
    }

    // เมธอดสำหรับทำลายข้อความ
    public void DestroyText()
    {
        if (textObject != null)
        {
            Destroy(textObject);
            textMeshPro = null;
        }
    }

    // เมธอดสำหรับรับ Component TextMeshPro
    public TextMeshPro GetTextMeshPro()
    {
        return textMeshPro;
    }

    // เมธอดสำหรับรับ GameObject ของข้อความ
    public GameObject GetTextObject()
    {
        return textObject;
    }
}
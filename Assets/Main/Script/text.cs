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

    [Header("Sorting Settings")]
    public string sortingLayerName = "Object";
    public int orderInLayer = 0;

    private TextMeshPro textMeshPro;

    void Start()
    {
        CreateTextObject();
    }

    void CreateTextObject()
    {
        // สร้าง GameObject สำหรับข้อความ
        GameObject textObject = new GameObject("TextObject");
        textObject.transform.SetParent(transform);
        textObject.transform.localPosition = Vector3.zero;

        // เพิ่ม TextMeshPro component
        textMeshPro = textObject.AddComponent<TextMeshPro>();

        // ตั้งค่าข้อความ
        textMeshPro.text = textContent;
        textMeshPro.color = textColor;
        textMeshPro.fontSize = fontSize;
        textMeshPro.alignment = alignment;
        textMeshPro.enableWordWrapping = false;

        // ตั้งค่า Sorting ให้อยู่บนพื้นหลัง
        SetupTextSorting(textObject);

        // ตั้งค่าเพิ่มเติม
        textMeshPro.raycastTarget = false;
    }

    void SetupTextSorting(GameObject textObject)
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
}
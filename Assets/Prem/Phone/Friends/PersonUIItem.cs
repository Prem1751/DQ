using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PersonUIItem : MonoBehaviour
{
    [Header("UI References")]
    public Image personImage;
    public TextMeshProUGUI personNameText;
    public Image stateIndicator;
    public TextMeshProUGUI stateSymbolText; // Text สำหรับแสดงสัญลักษณ์

    [Header("State Settings")]
    public Color normalColor = Color.green;
    public Color suspiciousColor = Color.yellow;
    public Color dangerousColor = Color.red;
    public Color defaultColor = Color.gray;

    [Header("State Symbols")]
    public string normalSymbol = "✓"; // สัญลักษณ์สถานะปกติ
    public string suspiciousSymbol = "?"; // สัญลักษณ์สถานะน่าสงสัย
    public string dangerousSymbol = "!"; // สัญลักษณ์สถานะอันตราย
    public string defaultSymbol = "?"; // สัญลักษณ์เริ่มต้น

    private PersonData personData;
    private PhoneUI phoneUI;
    private Button button;

    public void Setup(PersonData data, PhoneUI uiController)
    {
        personData = data;
        phoneUI = uiController;

        // อัปเดต UI
        if (personImage != null)
            personImage.sprite = data.personImage;
        else
            Debug.LogError("Person Image reference is missing!");

        if (personNameText != null)
            personNameText.text = data.personName;
        else
            Debug.LogError("Person Name Text reference is missing!");

        UpdateStateIndicator();

        // ตั้งค่า Button
        SetupButton();
    }

    private void SetupButton()
    {
        // หา Button component
        button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }

        // ตั้งค่า transition type เป็น None เพื่อป้องกันการเปลี่ยนสี
        button.transition = Selectable.Transition.None;

        // ลบ listeners เก่าและเพิ่มใหม่
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnItemClick);

        // ตั้งค่าให้ไม่ interactable ถ้าไม่มีข้อมูล
        button.interactable = (personData != null);
    }

    public void UpdateStateIndicator()
    {
        if (stateIndicator != null && personData != null)
        {
            // ตั้งค่าสีและสัญลักษณ์ตามสถานะ
            switch (personData.currentState)
            {
                case SuspicionState.Normal:
                    stateIndicator.color = normalColor;
                    SetStateSymbol(normalSymbol, normalColor);
                    break;
                case SuspicionState.Suspicious:
                    stateIndicator.color = suspiciousColor;
                    SetStateSymbol(suspiciousSymbol, suspiciousColor);
                    break;
                case SuspicionState.Dangerous:
                    stateIndicator.color = dangerousColor;
                    SetStateSymbol(dangerousSymbol, dangerousColor);
                    break;
                default:
                    stateIndicator.color = defaultColor;
                    SetStateSymbol(defaultSymbol, defaultColor);
                    break;
            }

            Debug.Log($"Updated state indicator for {personData.personName} to {personData.currentState}");
        }
    }

    private void SetStateSymbol(string symbol, Color color)
    {
        // ตั้งค่าสัญลักษณ์ถ้ามี Text component
        if (stateSymbolText != null)
        {
            stateSymbolText.text = symbol;
            stateSymbolText.color = color;
        }
        else
        {
            // ถ้าไม่มี Text component ให้ลองหาอัตโนมัติ
            stateSymbolText = GetComponentInChildren<TextMeshProUGUI>();
            if (stateSymbolText != null)
            {
                stateSymbolText.text = symbol;
                stateSymbolText.color = color;
            }
        }
    }

    // เรียกเมื่อคลิกที่アイเทม
    private void OnItemClick()
    {
        if (personData == null) return;

        Debug.Log($"Clicked on {personData.personName}");

        // เปลี่ยนสถานะเมื่อคลิก
        SwitchToNextState();

        // บันทึกข้อมูล
        if (phoneUI != null)
        {
            phoneUI.SaveData();
        }
    }

    private void SwitchToNextState()
    {
        if (personData != null)
        {
            // เปลี่ยนไปยังสถานะถัดไป
            switch (personData.currentState)
            {
                case SuspicionState.Normal:
                    personData.currentState = SuspicionState.Suspicious;
                    break;
                case SuspicionState.Suspicious:
                    personData.currentState = SuspicionState.Dangerous;
                    break;
                case SuspicionState.Dangerous:
                    personData.currentState = SuspicionState.Normal;
                    break;
            }

            // อัปเดตสีและสัญลักษณ์
            UpdateStateIndicator();

            Debug.Log($"{personData.personName} state changed to: {personData.currentState}");
        }
    }

    // เมธอดสำหรับเปลี่ยนสถานะจากภายนอก
    public void SetState(SuspicionState newState)
    {
        if (personData != null)
        {
            personData.currentState = newState;
            UpdateStateIndicator();
        }
    }

    // ตรวจสอบว่า GameObject ยังอยู่
    private void OnEnable()
    {
        Debug.Log($"{gameObject.name} is enabled");
    }

    private void OnDisable()
    {
        Debug.Log($"{gameObject.name} is disabled");
    }
}
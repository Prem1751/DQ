using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PersonUIItem : MonoBehaviour
{
    [Header("UI Parts")]
    public Image personImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Image statusIcon;
    public Button clickButton;

    [Header("Status Sprites")]
    public Sprite questionSprite;  // รูป ?
    public Sprite crossSprite;     // รูป X
    public Sprite checkSprite;     // รูป ✓

    private PersonData myData;
    private PhoneUI phoneManager;

    public void Setup(PersonData data, PhoneUI manager)
    {
        myData = data;
        phoneManager = manager;

        // ใส่ข้อมูล
        personImage.sprite = data.personImage;
        nameText.text = data.personName;
        descriptionText.text = data.description;

        // ตั้งปุ่ม
        clickButton.onClick.RemoveAllListeners();
        clickButton.onClick.AddListener(OnClick);

        // แสดงสถานะ
        UpdateIcon();
    }

    private void OnClick()
    {
        // เปลี่ยนสถานะ: ไม่มี → ? → X → ✓ → ไม่มี
        switch (myData.currentState)
        {
            case SuspicionState.None:
                myData.currentState = SuspicionState.Question;
                break;
            case SuspicionState.Question:
                myData.currentState = SuspicionState.Cross;
                break;
            case SuspicionState.Cross:
                myData.currentState = SuspicionState.Check;
                break;
            case SuspicionState.Check:
                myData.currentState = SuspicionState.None;
                break;
        }

        UpdateIcon();
        phoneManager.SaveData(); // บันทึกทันที
    }

    private void UpdateIcon()
    {
        switch (myData.currentState)
        {
            case SuspicionState.None:
                statusIcon.gameObject.SetActive(false);
                break;
            case SuspicionState.Question:
                statusIcon.gameObject.SetActive(true);
                statusIcon.sprite = questionSprite;
                statusIcon.color = Color.yellow;
                break;
            case SuspicionState.Cross:
                statusIcon.gameObject.SetActive(true);
                statusIcon.sprite = crossSprite;
                statusIcon.color = Color.red;
                break;
            case SuspicionState.Check:
                statusIcon.gameObject.SetActive(true);
                statusIcon.sprite = checkSprite;
                statusIcon.color = Color.green;
                break;
        }
    }

    public void RefreshIcon()
    {
        UpdateIcon();
    }
}
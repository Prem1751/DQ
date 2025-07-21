using UnityEngine;
using UnityEngine.UI;

public class PhoneSystem : MonoBehaviour
{
    public GameObject phoneScreen; // หน้าจอโทรศัพท์
    public GameObject[] appCanvases; // Canvas ของแอปทั้งหมด
    public GameObject chatNotificationIcon; // ไอคอนแจ้งเตือนแชท

    private bool isPhoneOpen = false;

    public float slideSpeed = 10f;
    private Vector3 phoneOffScreenPos;
    private Vector3 phoneOnScreenPos;

    void Start()
    {
        // เริ่มต้นปิดโทรศัพท์และทุกแอป
        phoneScreen.SetActive(false);
        CloseAllApps();
        phoneOnScreenPos = phoneScreen.transform.position;
        phoneOffScreenPos = phoneOnScreenPos - new Vector3(0, Screen.height, 0);
        phoneScreen.transform.position = phoneOffScreenPos;

        // ซ่อนไอคอนแจ้งเตือนเริ่มต้น
        if (chatNotificationIcon != null)
            chatNotificationIcon.SetActive(false);
    }

    void Update()
    {
        // ตรวจสอบการกดปุ่ม M (หรือปุ่มอื่นตามที่กำหนด)
        if (Input.GetKeyDown(KeyCode.M))
        {
            TogglePhone();
        }
        Vector3 targetPos = isPhoneOpen ? phoneOnScreenPos : phoneOffScreenPos;
        phoneScreen.transform.position = Vector3.Lerp(phoneScreen.transform.position, targetPos, slideSpeed * Time.deltaTime);
    }

    public void TogglePhone()
    {
        isPhoneOpen = !isPhoneOpen;
        phoneScreen.SetActive(isPhoneOpen);

        // ถ้าปิดโทรศัพท์ ให้ปิดทุกแอปด้วย
        if (!isPhoneOpen)
        {
            CloseAllApps();
        }
    }

    public void OpenApp(int appIndex)
    {
        // ปิดทุกแอปก่อนเปิดแอปใหม่
        CloseAllApps();

        // ตรวจสอบว่า index อยู่ในช่วงที่ถูกต้อง
        if (appIndex >= 0 && appIndex < appCanvases.Length)
        {
            appCanvases[appIndex].SetActive(true);

            // ถ้าเปิดแอปแชท ให้เรียก OnAppOpened
            if (appCanvases[appIndex].GetComponentInChildren<ChatManager>() != null)
            {
                ChatManager.Instance.OnAppOpened();
            }
        }

        // ปิดโทรศัพท์เมื่อเปิดแอป (optional)
        phoneScreen.SetActive(false);
        isPhoneOpen = false;
    }

    private void CloseAllApps()
    {
        foreach (GameObject appCanvas in appCanvases)
        {
            appCanvas.SetActive(false);

            // ถ้าเป็นแอปแชท ให้เรียก OnAppClosed
            if (appCanvas.GetComponentInChildren<ChatManager>() != null)
            {
                ChatManager.Instance.OnAppClosed();
            }
        }
    }

    // เรียกจาก ChatManager เมื่อมีข้อความใหม่
    public void ShowChatNotification(bool show)
    {
        if (chatNotificationIcon != null)
            chatNotificationIcon.SetActive(show);
    }
}
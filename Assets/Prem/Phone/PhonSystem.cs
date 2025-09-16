using UnityEngine;
using UnityEngine.UI;

public class PhoneSystem : MonoBehaviour
{
    public GameObject phoneScreen; // หน้าจอโทรศัพท์
    public GameObject[] appCanvases; // Canvas ของแอปทั้งหมด

    private bool isPhoneOpen = false;
    private bool isAnimating = false;

    public float slideSpeed = 10f;
    private Vector3 phoneOffScreenPos;
    private Vector3 phoneOnScreenPos;
    private RectTransform phoneRectTransform;

    void Start()
    {
        // ตรวจสอบว่า phoneScreen ถูกกำหนดค่าไว้
        if (phoneScreen == null)
        {
            Debug.LogError("PhoneScreen is not assigned in the inspector!");
            return;
        }

        // เริ่มต้นปิดโทรศัพท์และทุกแอป
        phoneScreen.SetActive(false);
        CloseAllApps();

        // กำหนดตำแหน่งสำหรับแสดงและซ่อนโทรศัพท์
        phoneRectTransform = phoneScreen.GetComponent<RectTransform>();
        phoneOnScreenPos = phoneRectTransform.anchoredPosition;
        phoneOffScreenPos = phoneOnScreenPos - new Vector3(0, phoneRectTransform.rect.height, 0);

        // ตั้งค่าเริ่มต้นให้โทรศัพท์อยู่ข้างนอกหน้าจอ
        phoneRectTransform.anchoredPosition = phoneOffScreenPos;
    }

    void Update()
    {
        // ตรวจสอบการกดปุ่ม M เพื่อเปิด/ปิดโทรศัพท์
        if (Input.GetKeyDown(KeyCode.M))
        {
            TogglePhone();
        }

        // เคลื่อนย้ายโทรศัพท์อย่างลื่นไหล
        if (isAnimating)
        {
            Vector3 targetPos = isPhoneOpen ? phoneOnScreenPos : phoneOffScreenPos;
            phoneRectTransform.anchoredPosition = Vector3.Lerp(phoneRectTransform.anchoredPosition, targetPos, slideSpeed * Time.deltaTime);

            // ตรวจสอบว่าเคลื่อนที่ถึงตำแหน่งเป้าหมายแล้วหรือยัง
            if (Vector3.Distance(phoneRectTransform.anchoredPosition, targetPos) < 5f)
            {
                phoneRectTransform.anchoredPosition = targetPos;
                isAnimating = false;

                // ถ้าปิดโทรศัพท์ ให้ปิด GameObject
                if (!isPhoneOpen)
                {
                    phoneScreen.SetActive(false);
                }
            }
        }
    }

    public void TogglePhone()
    {
        isPhoneOpen = !isPhoneOpen;
        isAnimating = true;

        if (isPhoneOpen)
        {
            phoneScreen.SetActive(true);
        }

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
        }

        // ปิดโทรศัพท์เมื่อเปิดแอป
        isPhoneOpen = false;
        isAnimating = true;
    }

    private void CloseAllApps()
    {
        foreach (GameObject appCanvas in appCanvases)
        {
            if (appCanvas != null)
                appCanvas.SetActive(false);
        }
    }
}
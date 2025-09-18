using UnityEngine;
using UnityEngine.UI;

public class PhoneSystem : MonoBehaviour
{
    public GameObject phoneScreen; // หน้าจอโทรศัพท์
    public GameObject[] appCanvases; // Canvas ของแอปทั้งหมด
    public PhoneUI phoneUI; // Reference ไปที่ PhoneUI

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

        // ค้นหา PhoneUI ถ้าไม่ได้กำหนดไว้
        if (phoneUI == null)
        {
            phoneUI = FindObjectOfType<PhoneUI>();
            if (phoneUI == null)
            {
                Debug.LogError("PhoneUI not found in scene!");
            }
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
                else
                {
                    // เมื่อเปิดโทรศัพท์เสร็จ ให้รีเฟรช UI
                    if (phoneUI != null)
                    {
                        phoneUI.RefreshUI();
                    }
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
            // รีเฟรช UI เมื่อเปิดโทรศัพท์
            if (phoneUI != null)
            {
                phoneUI.RefreshUI();
            }
        }

        // ถ้าปิดโทรศัพท์ ให้ปิดทุกแอปด้วย
        if (!isPhoneOpen)
        {
            CloseAllApps();
        }
    }

    public void OpenApp(int appIndex)
    {
        // เปิดโทรศัพท์ก่อนถ้ายังไม่เปิด
        if (!isPhoneOpen)
        {
            TogglePhone();

            // ใช้ Coroutine เพื่อรอให้โทรศัพท์เปิดเสร็จก่อนแล้วค่อยเปิดแอป
            StartCoroutine(OpenAppAfterPhone(appIndex));
            return;
        }

        // ปิดทุกแอปก่อนเปิดแอปใหม่
        CloseAllApps();

        // ตรวจสอบว่า index อยู่ในช่วงที่ถูกต้อง
        if (appIndex >= 0 && appIndex < appCanvases.Length && appCanvases[appIndex] != null)
        {
            appCanvases[appIndex].SetActive(true);
            Debug.Log($"Opened app index: {appIndex}");
        }
        else
        {
            Debug.LogError($"Invalid app index: {appIndex}");
        }

        // ไม่ปิดโทรศัพท์เมื่อเปิดแอป (ให้แอปแสดงบนโทรศัพท์)
    }

    private System.Collections.IEnumerator OpenAppAfterPhone(int appIndex)
    {
        // รอให้โทรศัพท์เปิดเสร็จ
        while (isAnimating)
        {
            yield return null;
        }

        // รออีกเล็กน้อยเพื่อให้แน่ใจ
        yield return new WaitForSeconds(0.1f);

        // เปิดแอป
        CloseAllApps();

        if (appIndex >= 0 && appIndex < appCanvases.Length && appCanvases[appIndex] != null)
        {
            appCanvases[appIndex].SetActive(true);
            Debug.Log($"Opened app index: {appIndex} after phone animation");
        }
    }

    public void OpenAppImmediately(int appIndex)
    {
        // เปิดโทรศัพท์ทันทีโดยไม่แสดง animation
        isPhoneOpen = true;
        isAnimating = false;
        phoneScreen.SetActive(true);
        phoneRectTransform.anchoredPosition = phoneOnScreenPos;

        // ปิดทุกแอปก่อนเปิดแอปใหม่
        CloseAllApps();

        // เปิดแอปที่ต้องการ
        if (appIndex >= 0 && appIndex < appCanvases.Length && appCanvases[appIndex] != null)
        {
            appCanvases[appIndex].SetActive(true);
            Debug.Log($"Immediately opened app index: {appIndex}");
        }

        // รีเฟรช UI
        if (phoneUI != null)
        {
            phoneUI.RefreshUI();
        }
    }

    private void CloseAllApps()
    {
        foreach (GameObject appCanvas in appCanvases)
        {
            if (appCanvas != null)
                appCanvas.SetActive(false);
        }
    }

    // เมธอดสำหรับปิดแอปทั้งหมดและโทรศัพท์
    public void ClosePhoneAndApps()
    {
        CloseAllApps();
        isPhoneOpen = false;
        isAnimating = true;
    }
}
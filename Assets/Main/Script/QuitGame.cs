using UnityEngine;
using UnityEngine.UI;

public class QuitGame : MonoBehaviour
{
    [Header("การตั้งค่า")]
    public bool useConfirmationDialog = true; // ใช้หน้าต่างยืนยันก่อนออก
    public GameObject confirmationDialog;    // UI Dialog สำหรับยืนยัน

    void Start()
    {
        // ถ้ามีปุ่ม ให้กำหนดการทำงานอัตโนมัติ
        Button button = GetComponent<Button>();
        if (button != null)
        {
            if (useConfirmationDialog)
                button.onClick.AddListener(ShowConfirmationDialog);
            else
                button.onClick.AddListener(Quit);
        }
    }

    void Update()
    {
        // ตรวจสอบการกดปุ่มลัด (เช่น Esc)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (useConfirmationDialog)
                ShowConfirmationDialog();
            else
                Quit();
        }
    }

    // แสดงหน้าต่างยืนยันการออกจากเกม
    public void ShowConfirmationDialog()
    {
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(true);
        }
        else
        {
            // ถ้าไม่มี Dialog ให้แสดง確認แบบง่ายๆ
#if UNITY_EDITOR
            if (UnityEditor.EditorUtility.DisplayDialog("ออกจากเกม", "คุณต้องการออกจากเกมจริงๆ หรือไม่?", "ออก", "ยกเลิก"))
            {
                Quit();
            }
#else
            Quit(); // ใน build จริงให้ออกเลยถ้าไม่มี Dialog
#endif
        }
    }

    // ฟังก์ชันออกจากเกม
    public void Quit()
    {
        Debug.Log("กำลังออกจากเกม...");

        // บันทึกข้อมูลก่อนออก (ถ้ามี)
        SaveGameData();

        // ออกจากเกม
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    // ยกเลิกการออกจากเกม
    public void CancelQuit()
    {
        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);

        Debug.Log("ยกเลิกการออกจากเกม");
    }

    // บันทึกข้อมูลเกม (Optional)
    private void SaveGameData()
    {
        // ตัวอย่างการบันทึกข้อมูล
        PlayerPrefs.SetFloat("LastPlayTime", Time.time);
        PlayerPrefs.Save();
        Debug.Log("บันทึกข้อมูลเกมเรียบร้อยแล้ว");
    }
}
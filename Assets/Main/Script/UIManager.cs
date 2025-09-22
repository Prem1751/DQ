using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("การตั้งค่า UI")]
    public GameObject uiPanel;          // Panel ของ UI
    public Button openButton;           // ปุ่มสำหรับเปิด UI
    public Button closeButton;          // ปุ่มสำหรับปิด UI

    [Header("เอฟเฟกต์เสียง (Optional)")]
    public AudioClip openSound;
    public AudioClip closeSound;
    private AudioSource audioSource;

    void Start()
    {
        // ตั้งค่าเริ่มต้น - ซ่อน UI
        if (uiPanel != null)
            uiPanel.SetActive(false);

        // กำหนดการทำงานให้ปุ่ม
        if (openButton != null)
            openButton.onClick.AddListener(OpenUI);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseUI);

        // เตรียม AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    // ฟังก์ชันเปิด UI
    public void OpenUI()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(true);
            PlaySound(openSound);
            Debug.Log("เปิด UI แล้ว!");
        }
    }

    // ฟังก์ชันปิด UI
    public void CloseUI()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
            PlaySound(closeSound);
            Debug.Log("ปิด UI แล้ว!");
        }
    }

    // ฟังก์ชันเล่นเสียง
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // ฟังก์ชันสำหรับปุ่มสลับเปิด-ปิด
    public void ToggleUI()
    {
        if (uiPanel != null)
        {
            bool isActive = uiPanel.activeSelf;
            uiPanel.SetActive(!isActive);

            if (!isActive)
                PlaySound(openSound);
            else
                PlaySound(closeSound);
        }
    }
}
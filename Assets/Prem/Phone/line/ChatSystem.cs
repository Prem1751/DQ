using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Message
{
    public string text;
    public string sender;
    public bool isPlayer;
}

public class ChatSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject messagePrefab;
    public Transform messageContainer;
    public GameObject optionButtonPrefab;
    public Transform optionsPanel;
    public ScrollRect scrollRect;
    public GameObject notificationPanel;
    public Text notificationText;

    [Header("Settings")]
    public float typingSpeed = 0.05f;
    public AudioClip notificationSound;

    private bool isAppOpen = true;
    private Coroutine typingCoroutine;

    void Start()
    {
        // ตัวอย่างการเริ่มบทสนทนา
        ReceiveMessage("เพื่อน", "สวัสดี! มีอะไรให้ช่วยไหม?", false);
    }

    public void ReceiveMessage(string sender, string text, bool isPlayer)
    {
        if (!isAppOpen)
        {
            ShowNotification(sender + ": " + text);
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(DisplayMessage(sender, text, isPlayer));
    }

    IEnumerator DisplayMessage(string sender, string text, bool isPlayer)
    {
        GameObject newMessage = Instantiate(messagePrefab, messageContainer);
        ChatMessage msg = newMessage.GetComponent<ChatMessage>();
        msg.SetSender(sender, isPlayer);

        // แสดงข้อความทีละตัวอักษร
        for (int i = 0; i <= text.Length; i++)
        {
            msg.SetText(text.Substring(0, i));
            ScrollToBottom();
            yield return new WaitForSeconds(typingSpeed);
        }

        // เล่นเสียงถ้ามี
        if (!isPlayer && notificationSound != null)
        {
            AudioSource.PlayClipAtPoint(notificationSound, Camera.main.transform.position);
        }
    }

    public void ShowOptions(List<string> options)
    {
        // ล้างตัวเลือกเก่า
        foreach (Transform child in optionsPanel)
        {
            Destroy(child.gameObject);
        }

        // สร้างตัวเลือกใหม่
        foreach (string option in options)
        {
            GameObject optionBtn = Instantiate(optionButtonPrefab, optionsPanel);
            optionBtn.GetComponentInChildren<Text>().text = option;
            optionBtn.GetComponent<Button>().onClick.AddListener(() => {
                SendPlayerMessage(option);
            });
        }
    }

    public void SendPlayerMessage(string text)
    {
        ReceiveMessage("คุณ", text, true);
    }

    void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    void ShowNotification(string message)
    {
        notificationPanel.SetActive(true);
        notificationText.text = message;
    }

    public void OnAppOpened()
    {
        isAppOpen = true;
        notificationPanel.SetActive(false);
    }

    public void OnAppClosed()
    {
        isAppOpen = false;
    }
}
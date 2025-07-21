using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MessageData
{
    public string text;
    public bool isPlayerMessage;
    public Sprite avatar;

    public MessageData(string text, bool isPlayerMessage, Sprite avatar = null)
    {
        this.text = text;
        this.isPlayerMessage = isPlayerMessage;
        this.avatar = avatar;
    }
}

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance;

    [Header("UI Settings")]
    public RectTransform chatWindow; // Set to 600x900 in Inspector
    public ScrollRect scrollRect;
    public GameObject messagePrefab;
    public GameObject choiceButtonPrefab;
    public Transform contentPanel;
    public GameObject notificationPanel;
    public TMP_Text notificationText;
    public AudioClip newMessageSound;

    [Header("NPC Settings")]
    public string npcName = "NPC";
    public Sprite npcAvatar;
    public float typingSpeed = 0.05f;

    private Queue<MessageData> messageQueue = new Queue<MessageData>();
    private AudioSource audioSource;
    private bool isTyping = false;
    private bool hasUnreadMessages = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
        chatWindow.gameObject.SetActive(false);
        notificationPanel.SetActive(false);

        // เริ่มต้นด้วยการส่งข้อความต้อนรับ
        StartCoroutine(InitialGreeting());
    }

    public void OnAppOpened()
    {
        chatWindow.gameObject.SetActive(true);
        notificationPanel.SetActive(false);
        hasUnreadMessages = false;

        // แสดงข้อความที่ค้างอยู่ทั้งหมด
        while (messageQueue.Count > 0)
        {
            MessageData msg = messageQueue.Dequeue();
            AddMessageToChat(msg.text, msg.isPlayerMessage);
        }
    }

    // เมื่อแอปถูกปิดผ่าน PhoneSystem
    public void OnAppClosed()
    {
        chatWindow.gameObject.SetActive(false);
    }

    IEnumerator InitialGreeting()
    {
        yield return new WaitForSeconds(1f); // รอ 1 วินาทีก่อนแสดงข้อความแรก
        SendNPCMessage("สวัสดี! มีข้อความใหม่เข้ามา");
    }

    public void SendNPCMessage(string message)
    {
        if (!chatWindow.gameObject.activeInHierarchy)
        {
            ShowNotification(message);
            return;
        }

        StartCoroutine(TypeMessage(message, false));
    }

    public void SendPlayerMessage(string message)
    {
        AddMessageToChat(message, true);
    }

    public void ShowDialogueChoices(List<string> choices)
    {
        foreach (string choice in choices)
        {
            GameObject choiceBtn = Instantiate(choiceButtonPrefab, contentPanel);
            choiceBtn.GetComponentInChildren<TMP_Text>().text = choice;
            choiceBtn.GetComponent<Button>().onClick.AddListener(() => OnChoiceSelected(choice));
        }
    }

    private void OnChoiceSelected(string choice)
    {
        AddMessageToChat(choice, true);
        DialogueSystem.Instance.ProcessPlayerChoice(choice);
    }

    private IEnumerator TypeMessage(string message, bool isPlayer)
    {
        isTyping = true;
        GameObject newMsg = Instantiate(messagePrefab, contentPanel);
        MessageUI msgUI = newMsg.GetComponent<MessageUI>();
        msgUI.Setup("", isPlayer, isPlayer ? null : npcAvatar);

        foreach (char c in message)
        {
            msgUI.AppendText(c.ToString());
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        ScrollToBottom();
    }

    private void AddMessageToChat(string message, bool isPlayer)
    {
        if (isTyping) return;

        GameObject newMsg = Instantiate(messagePrefab, contentPanel);
        newMsg.GetComponent<MessageUI>().Setup(message, isPlayer, isPlayer ? null : npcAvatar);
        ScrollToBottom();
    }

    private void ShowNotification(string message)
    {
        hasUnreadMessages = true;
        notificationText.text = $"{npcName}: {message}";
        notificationPanel.SetActive(true);
        PlayNotificationSound();
        messageQueue.Enqueue(new MessageData(message, false, npcAvatar));
    }

    private void PlayNotificationSound()
    {
        if (newMessageSound != null)
        {
            audioSource.PlayOneShot(newMessageSound);
        }
    }

    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.normalizedPosition = Vector2.zero;
    }

    public void ToggleChatWindow()
    {
        chatWindow.gameObject.SetActive(!chatWindow.gameObject.activeSelf);

        if (chatWindow.gameObject.activeSelf)
        {
            notificationPanel.SetActive(false);
            while (messageQueue.Count > 0)
            {
                MessageData msg = messageQueue.Dequeue();
                AddMessageToChat(msg.text, msg.isPlayerMessage);
            }
        }
    }
}
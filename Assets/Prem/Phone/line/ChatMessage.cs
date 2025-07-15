using UnityEngine;
using UnityEngine.UI;

public class ChatMessage : MonoBehaviour
{
    public Text messageText;
    public Text senderText;
    public Image bubbleImage;

    [Header("Colors")]
    public Color playerBubbleColor = new Color(0.1f, 0.6f, 1f);
    public Color npcBubbleColor = Color.white;
    public Color playerTextColor = Color.white;
    public Color npcTextColor = Color.black;

    public void SetText(string text)
    {
        messageText.text = text;
    }

    public void SetSender(string sender, bool isPlayer)
    {
        senderText.text = sender;
        bubbleImage.color = isPlayer ? playerBubbleColor : npcBubbleColor;
        messageText.color = isPlayer ? playerTextColor : npcTextColor;
        senderText.color = isPlayer ? playerTextColor : npcTextColor;

        // จัดตำแหน่งตามผู้ส่ง
        RectTransform rt = GetComponent<RectTransform>();
        if (isPlayer)
        {
            rt.anchorMin = new Vector2(0.7f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            messageText.alignment = TextAnchor.MiddleRight;
            senderText.alignment = TextAnchor.MiddleRight;
        }
        else
        {
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0.3f, 0.5f);
            messageText.alignment = TextAnchor.MiddleLeft;
            senderText.alignment = TextAnchor.MiddleLeft;
        }
    }
}
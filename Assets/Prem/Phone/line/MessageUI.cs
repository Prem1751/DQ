using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MessageUI : MonoBehaviour
{
    public TMP_Text messageText;
    public Image avatarImage;
    public Image bubbleBackground;
    public RectTransform messageRect;

    [Header("Colors")]
    public Color playerColor = new Color(0.1f, 0.7f, 0.1f);
    public Color npcColor = new Color(0.8f, 0.8f, 0.8f);

    public void Setup(string text, bool isPlayer, Sprite avatar)
    {
        messageText.text = text;
        bubbleBackground.color = isPlayer ? playerColor : npcColor;

        if (avatar != null)
        {
            avatarImage.sprite = avatar;
            avatarImage.gameObject.SetActive(true);
        }
        else
        {
            avatarImage.gameObject.SetActive(false);
        }

        messageRect.anchorMin = isPlayer ? new Vector2(0.7f, 0.5f) : new Vector2(0.3f, 0.5f);
        messageRect.anchorMax = isPlayer ? new Vector2(0.7f, 0.5f) : new Vector2(0.3f, 0.5f);
        messageRect.pivot = isPlayer ? new Vector2(1, 0.5f) : new Vector2(0, 0.5f);
    }

    public void AppendText(string text)
    {
        messageText.text += text;
    }
}
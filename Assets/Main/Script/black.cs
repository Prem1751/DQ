using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueController : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(3, 10)]
        public string text;
    }

    [Header("UI References")]
    [SerializeField] private Image blackPanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject continueIcon;

    [Header("Dialogue Settings")]
    public DialogueLine[] dialogueLines;
    public float fadeDuration = 1f;
    public float textSpeed = 0.05f;
    public KeyCode advanceKey = KeyCode.Space;

    private int currentLine = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private Coroutine fadeCoroutine;

    void Start()
    {
        InitializeUI();
    }

    void InitializeUI()
    {
        if (blackPanel != null)
            blackPanel.color = new Color(0, 0, 0, 0);

        if (dialogueText != null)
            dialogueText.text = "";

        if (continueIcon != null)
            continueIcon.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(advanceKey))
        {
            if (isTyping)
            {
                FinishTyping();
            }
            else
            {
                NextLine();
            }
        }
    }

    public void StartDialogue()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning("No dialogue lines assigned!");
            return;
        }

        currentLine = 0;
        StartFade(0f, 1f, fadeDuration, ShowLine);
    }

    void ShowLine()
    {
        if (currentLine < dialogueLines.Length)
        {
            typingCoroutine = StartCoroutine(TypeText(dialogueLines[currentLine].text));
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        if (dialogueText != null) dialogueText.text = "";
        if (continueIcon != null) continueIcon.SetActive(false);

        foreach (char c in text)
        {
            if (dialogueText != null) dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
        if (continueIcon != null) continueIcon.SetActive(true);
    }

    void FinishTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (currentLine < dialogueLines.Length && dialogueText != null)
            dialogueText.text = dialogueLines[currentLine].text;

        isTyping = false;
        if (continueIcon != null) continueIcon.SetActive(true);
    }

    void NextLine()
    {
        currentLine++;
        ShowLine();
    }

    public void EndDialogue()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        if (blackPanel == null) yield break;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            blackPanel.color = new Color(0, 0, 0, Mathf.Lerp(1, 0, timer / fadeDuration));
            yield return null;
        }

        if (dialogueText != null)
            dialogueText.text = "";

        if (continueIcon != null)
            continueIcon.SetActive(false);
    }

    void StartFade(float fromAlpha, float toAlpha, float duration, System.Action onComplete = null)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeRoutine(fromAlpha, toAlpha, duration, onComplete));
    }

    IEnumerator FadeRoutine(float fromAlpha, float toAlpha, float duration, System.Action onComplete)
    {
        if (blackPanel == null) yield break;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            blackPanel.color = new Color(0, 0, 0, Mathf.Lerp(fromAlpha, toAlpha, timer / duration));
            yield return null;
        }

        onComplete?.Invoke();
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class Discene : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image speakerImage;

    [Header("Dialogue Settings")]
    public string defaultNpcName = "Hyung-gil";
    public DialogueLine[] dialogueLines;
    public float interactionRadius = 2f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Typing Effect")]
    public float typingSpeed = 0.05f;
    public bool useTypingEffect = true;

    [Header("Image Pop Effect")]
    public float popScale = 1.2f;
    public float popDuration = 0.2f;
    public AudioClip popSound;

    [Header("Sound Effects")]
    public AudioClip typingSound;
    public AudioClip dialogueOpenSound;
    public AudioClip dialogueCloseSound;

    [Header("Scene Transition")]
    public bool changeSceneAfterDialogue = false;
    public string sceneNameToLoad = "";
    public float sceneTransitionDelay = 1f;
    public float fadeDuration = 1f;
    public Color fadeColor = Color.black;

    [Header("Score-Based Scene Transition")]
    public bool useScoreBasedTransition = false;
    public int requiredScore = 100; // คะแนนที่ต้องใช้ในการเปลี่ยนฉาก
    public string scoreBasedSceneName = ""; // ชื่อฉากที่จะไปเมื่อคะแนนถึงที่กำหนด

    private bool isInRange = false;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private int currentLineIndex = 0;
    private Vector3 originalScale;
    private AudioSource audioSource;

    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(3, 10)]
        public string text;
        public string characterName;
        public Sprite characterSprite;
        public AudioClip voiceOver;
    }

    private void Start()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        originalScale = speakerImage.transform.localScale;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Update()
    {
        CheckPlayerInRange();

        if (isInRange && Input.GetKeyDown(interactKey))
        {
            if (!isDialogueActive)
            {
                StartDialogue();
            }
            else if (isTyping)
            {
                CompleteSentence();
            }
            else
            {
                NextLine();
            }
        }
    }

    private void CheckPlayerInRange()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactionRadius);
        isInRange = false;

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                isInRange = true;
                break;
            }
        }
    }

    public void StartDialogue()
    {
        isDialogueActive = true;
        currentLineIndex = 0;
        dialoguePanel.SetActive(true);
        DisplayLine(dialogueLines[currentLineIndex]);

        if (dialogueOpenSound != null)
        {
            audioSource.PlayOneShot(dialogueOpenSound);
        }
    }

    private void DisplayLine(DialogueLine line)
    {
        nameText.text = string.IsNullOrEmpty(line.characterName) ? defaultNpcName : line.characterName;

        if (line.characterSprite != null)
        {
            speakerImage.sprite = line.characterSprite;
            StartCoroutine(PopImageEffect());
        }

        if (line.voiceOver != null)
        {
            audioSource.PlayOneShot(line.voiceOver);
        }

        if (useTypingEffect)
        {
            StartCoroutine(TypeLine(line.text));
        }
        else
        {
            dialogueText.text = line.text;
        }
    }

    private IEnumerator PopImageEffect()
    {
        if (popSound != null)
        {
            audioSource.PlayOneShot(popSound);
        }

        float timer = 0f;
        Vector3 targetScale = originalScale * popScale;

        while (timer < popDuration / 2)
        {
            timer += Time.deltaTime;
            speakerImage.transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / (popDuration / 2));
            yield return null;
        }

        timer = 0f;

        while (timer < popDuration / 2)
        {
            timer += Time.deltaTime;
            speakerImage.transform.localScale = Vector3.Lerp(targetScale, originalScale, timer / (popDuration / 2));
            yield return null;
        }

        speakerImage.transform.localScale = originalScale;
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;

            if (typingSound != null && char.IsLetterOrDigit(letter))
            {
                audioSource.PlayOneShot(typingSound);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void CompleteSentence()
    {
        StopAllCoroutines();
        dialogueText.text = dialogueLines[currentLineIndex].text;
        speakerImage.transform.localScale = originalScale;
        isTyping = false;
    }

    private void NextLine()
    {
        currentLineIndex++;

        if (currentLineIndex < dialogueLines.Length)
        {
            DisplayLine(dialogueLines[currentLineIndex]);
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);

        if (dialogueCloseSound != null)
        {
            audioSource.PlayOneShot(dialogueCloseSound);
        }

        // ตรวจสอบเงื่อนไขการเปลี่ยนฉาก
        if (useScoreBasedTransition && GameManager.Instance != null)
        {
            int currentScore = GameManager.Instance.GetScore();
            if (currentScore >= requiredScore && !string.IsNullOrEmpty(scoreBasedSceneName))
            {
                StartCoroutine(TransitionToScene(scoreBasedSceneName));
                return;
            }
        }

        // ถ้าไม่เข้าเงื่อนไขคะแนน ให้ใช้การเปลี่ยนฉากแบบเดิม
        if (changeSceneAfterDialogue && !string.IsNullOrEmpty(sceneNameToLoad))
        {
            StartCoroutine(TransitionToScene(sceneNameToLoad));
        }
    }

    private IEnumerator TransitionToScene(string targetScene)
    {
        yield return new WaitForSeconds(sceneTransitionDelay);

        // สร้าง effect Fade อย่างง่าย
        GameObject fadeObject = new GameObject("FadeObject");
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        Image fadeImage = fadeObject.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
        fadeObject.transform.SetParent(canvas.transform, false);
        RectTransform rt = fadeObject.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        // Fade in
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        // โหลดฉากใหม่
        SceneManager.LoadScene(targetScene);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
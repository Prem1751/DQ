using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

[System.Serializable]
public class DialogueLine
{
    [Tooltip("ข้อความบทสนทนา")]
    [TextArea(3, 10)]
    public string text;

    [Tooltip("ชื่อตัวละคร (ถ้าว่างจะใช้ชื่อเริ่มต้น)")]
    public string characterName;

    [Tooltip("รูปภาพตัวละคร")]
    public Sprite characterSprite;

    [Tooltip("เสียงพูดของตัวละคร")]
    public AudioClip voiceOver;

    [Tooltip("มีตัวเลือกหรือไม่")]
    public bool hasChoices;

    [Tooltip("ข้อความตัวเลือกต่างๆ")]
    public string[] choices;

    [Tooltip("คะแนนสำหรับแต่ละตัวเลือก")]
    public int[] choiceScores;

    [Tooltip("บรรทัดต่อไปสำหรับแต่ละตัวเลือก (ใส่ -1 เพื่อจบบทสนทนา)")]
    public int[] choiceLeadsTo;

    [Tooltip("บทสนทนาหลังเลือกตัวเลือกนี้")]
    public string postChoiceDialogue;

    [Tooltip("เปลี่ยนฉากหลังจากบรรทัดนี้หรือไม่")]
    public bool changeSceneAfterThisLine;

    [Tooltip("ชื่อฉากที่จะเปลี่ยน")]
    public string sceneNameToLoad;

    [Tooltip("ดีเลย์ก่อนเปลี่ยนฉาก")]
    public float sceneTransitionDelay = 1f;

    [Tooltip("ใช้การเปลี่ยนฉากตามคะแนนหรือไม่")]
    public bool useScoreBasedTransition;

    [Tooltip("คะแนนที่ต้องการสำหรับการเปลี่ยนฉาก")]
    public int requiredScore;

    [Tooltip("ชื่อฉากเมื่อได้คะแนนตามต้องการ")]
    public string scoreBasedSceneName;
}

public class AdvancedDialogueSystem : MonoBehaviour
{
    [Header("ส่วนประกอบ UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image speakerImage;

    [Header("การตั้งค่าบทสนทนา")]
    public string defaultNpcName = "NPC";
    public DialogueLine[] dialogueLines;
    public float interactionRadius = 2f;
    public KeyCode interactKey = KeyCode.E;

    [Header("เอฟเฟกต์การพิมพ์")]
    public float typingSpeed = 0.05f;
    public bool useTypingEffect = true;

    [Header("เอฟเฟกต์ภาพ")]
    public float popScale = 1.2f;
    public float popDuration = 0.2f;
    public AudioClip popSound;

    [Header("เสียงประกอบ")]
    public AudioClip typingSound;
    public AudioClip dialogueOpenSound;
    public AudioClip dialogueCloseSound;

    [Header("การตั้งค่าตัวเลือก")]
    public GameObject choicePanel;
    public Button[] choiceButtons;
    public TextMeshProUGUI[] choiceTexts;

    [Header("เอฟเฟกต์เปลี่ยนฉาก")]
    public float fadeDuration = 1f;
    public Color fadeColor = Color.black;

    // ตัวแปรภายใน
    private bool isInRange = false;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private int currentLineIndex = 0;
    private Vector3 originalScale;
    private AudioSource audioSource;
    private int lastChoiceIndex = -1;

    private void Start()
    {
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);

        originalScale = speakerImage.transform.localScale;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // ✅ แก้ไขส่วนนี้ - ตั้งค่าปุ่มตัวเลือก
        if (choiceButtons != null)
        {
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                int choiceIndex = i;

                if (choiceButtons[i] != null)
                {
                    choiceButtons[i].onClick.RemoveAllListeners(); // ลบ listener เดิม
                    choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(choiceIndex));
                }
                else
                {
                    Debug.LogError($"Choice button at index {i} is null! Please assign in Inspector.", this);
                }
            }
        }
        else
        {
            Debug.LogError("ChoiceButtons array is null! Please assign in Inspector.", this);
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
        // ตรวจสอบว่าต้องการเปลี่ยนฉากหลังบรรทัดนี้หรือไม่
        if (dialogueLines[currentLineIndex].changeSceneAfterThisLine)
        {
            HandleSceneTransition();
            return;
        }

        // ตรวจสอบว่ามีตัวเลือกหรือไม่
        if (dialogueLines[currentLineIndex].hasChoices &&
            dialogueLines[currentLineIndex].choices != null &&
            dialogueLines[currentLineIndex].choices.Length > 0)
        {
            ShowChoices(dialogueLines[currentLineIndex].choices);
            return;
        }

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

    private void ShowChoices(string[] choices)
    {
        if (choicePanel == null || choiceButtons == null || choiceTexts == null)
        {
            Debug.LogError("Choice UI components are not assigned!", this);
            return;
        }

        dialoguePanel.SetActive(false);
        choicePanel.SetActive(true);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < choices.Length && choiceButtons[i] != null)
            {
                choiceButtons[i].gameObject.SetActive(true);
                if (choiceTexts[i] != null)
                {
                    choiceTexts[i].text = choices[i];
                }
            }
            else
            {
                if (choiceButtons[i] != null)
                {
                    choiceButtons[i].gameObject.SetActive(false);
                }
            }
        }
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        lastChoiceIndex = choiceIndex;
        choicePanel.SetActive(false);

        DialogueLine currentLine = dialogueLines[currentLineIndex];

        // เพิ่มคะแนนถ้ามี
        if (choiceIndex < currentLine.choiceScores.Length)
        {
            GameManager.Instance?.AddScore(currentLine.choiceScores[choiceIndex]);
        }

        // ตรวจสอบว่ามีบทสนทนาหลังเลือกหรือไม่
        if (!string.IsNullOrEmpty(currentLine.postChoiceDialogue))
        {
            // แสดงบทสนทนาหลังเลือก
            dialoguePanel.SetActive(true);
            DisplayLine(new DialogueLine()
            {
                text = currentLine.postChoiceDialogue,
                characterName = currentLine.characterName,
                characterSprite = currentLine.characterSprite
            });
            return;
        }

        // ตรวจสอบว่ามีการกำหนดบรรทัดต่อไปหรือไม่
        if (currentLine.choiceLeadsTo != null &&
            choiceIndex < currentLine.choiceLeadsTo.Length)
        {
            int nextLine = currentLine.choiceLeadsTo[choiceIndex];

            if (nextLine == -1) // จบบทสนทนา
            {
                EndDialogue();
                return;
            }
            else if (nextLine >= 0 && nextLine < dialogueLines.Length)
            {
                currentLineIndex = nextLine;
                dialoguePanel.SetActive(true);
                DisplayLine(dialogueLines[currentLineIndex]);
                return;
            }
        }

        // ไม่มีอะไรกำหนด - ไปบรรทัดต่อไป
        currentLineIndex++;
        if (currentLineIndex < dialogueLines.Length)
        {
            dialoguePanel.SetActive(true);
            DisplayLine(dialogueLines[currentLineIndex]);
        }
        else
        {
            EndDialogue();
        }
    }

    private void HandleSceneTransition()
    {
        DialogueLine currentLine = dialogueLines[currentLineIndex];

        // ตรวจสอบการเปลี่ยนฉากตามคะแนน
        if (currentLine.useScoreBasedTransition && GameManager.Instance != null)
        {
            int currentScore = GameManager.Instance.GetScore();
            if (currentScore >= currentLine.requiredScore && !string.IsNullOrEmpty(currentLine.scoreBasedSceneName))
            {
                StartCoroutine(TransitionToScene(currentLine.scoreBasedSceneName));
                return;
            }
        }

        // เปลี่ยนฉากปกติ
        if (!string.IsNullOrEmpty(currentLine.sceneNameToLoad))
        {
            StartCoroutine(TransitionToScene(currentLine.sceneNameToLoad));
        }
        else
        {
            EndDialogue();
        }
    }

    private IEnumerator TransitionToScene(string targetScene)
    {
        EndDialogue();
        yield return new WaitForSeconds(dialogueLines[currentLineIndex].sceneTransitionDelay);

        // สร้าง effect Fade ระหว่างเปลี่ยนฉาก
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

    public void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);

        if (dialogueCloseSound != null)
        {
            audioSource.PlayOneShot(dialogueCloseSound);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }

    // ฟังก์ชันสำหรับตรวจสอบตัวเลือกล่าสุด
    public int GetLastChoiceIndex()
    {
        return lastChoiceIndex;
    }
}
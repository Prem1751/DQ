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

    [Tooltip("บรรทัดต่อไปสำหรับแต่ละตัวเลือก")]
    public int[] choiceLeadsTo;

    [Tooltip("บทสนทนาหลังเลือกตัวเลือกนี้")]
    public string postChoiceDialogue;

    [Tooltip("เปลี่ยนฉากหลังจากบทสนทนานี้หรือไม่")]
    public bool changeSceneAfterThis;

    [Tooltip("ชื่อฉากที่จะเปลี่ยน")]
    public string sceneToLoad;

    [Tooltip("ดีเลย์ก่อนเปลี่ยนฉาก (วินาที)")]
    public float sceneChangeDelay = 1f;

    [Tooltip("ใช้ Fade Animation เมื่อเปลี่ยนฉากหรือไม่")]
    public bool useFadeEffect = true;

    [Tooltip("สี Fade Effect")]
    public Color fadeColor = Color.black;

    [Tooltip("ระยะเวลา Fade (วินาที)")]
    public float fadeDuration = 1f;
}

public class AdvancedDialogueSystem : MonoBehaviour
{
    [Header("ส่วนประกอบ UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image speakerImage;
    public Image fadeOverlay;

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
    public int[] choiceScores;

    // ตัวแปรภายใน
    private bool isInRange = false;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private int currentLineIndex = 0;
    private Vector3 originalScale;
    private AudioSource audioSource;
    private int lastChoiceIndex = -1;
    private bool waitingForSceneChange = false;

    private void Start()
    {
        InitializeDialogueSystem();
    }

    private void InitializeDialogueSystem()
    {
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);

        // ตั้งค่า Fade Overlay
        if (fadeOverlay != null)
        {
            fadeOverlay.color = new Color(fadeOverlay.color.r, fadeOverlay.color.g, fadeOverlay.color.b, 0);
            fadeOverlay.gameObject.SetActive(false);
        }

        originalScale = speakerImage.transform.localScale;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        SetupChoiceButtons();
    }

    private void SetupChoiceButtons()
    {
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int choiceIndex = i;
            choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(choiceIndex));
        }
    }

    private void Update()
    {
        if (waitingForSceneChange) return;

        CheckPlayerInRange();

        if (isInRange && Input.GetKeyDown(interactKey))
        {
            HandleDialogueInput();
        }
    }

    private void HandleDialogueInput()
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

        PlaySoundEffect(dialogueOpenSound);
    }

    private void DisplayLine(DialogueLine line)
    {
        nameText.text = string.IsNullOrEmpty(line.characterName) ? defaultNpcName : line.characterName;

        if (line.characterSprite != null)
        {
            speakerImage.sprite = line.characterSprite;
            StartCoroutine(PopImageEffect());
        }

        PlaySoundEffect(line.voiceOver);

        if (useTypingEffect)
        {
            StartCoroutine(TypeLine(line.text));
        }
        else
        {
            dialogueText.text = line.text;
            CheckForImmediateSceneChange();
        }
    }

    private IEnumerator PopImageEffect()
    {
        PlaySoundEffect(popSound);

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
                PlaySoundEffect(typingSound);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        CheckForImmediateSceneChange();
    }

    private void CheckForImmediateSceneChange()
    {
        if (!isTyping && dialogueLines[currentLineIndex].changeSceneAfterThis)
        {
            waitingForSceneChange = true;
            StartCoroutine(ChangeSceneWithFade(
                dialogueLines[currentLineIndex].sceneToLoad,
                dialogueLines[currentLineIndex].sceneChangeDelay,
                dialogueLines[currentLineIndex].useFadeEffect,
                dialogueLines[currentLineIndex].fadeColor,
                dialogueLines[currentLineIndex].fadeDuration
            ));
        }
    }

    private void PlaySoundEffect(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void CompleteSentence()
    {
        StopAllCoroutines();
        dialogueText.text = dialogueLines[currentLineIndex].text;
        speakerImage.transform.localScale = originalScale;
        isTyping = false;
        CheckForImmediateSceneChange();
    }

    private void NextLine()
    {
        if (dialogueLines[currentLineIndex].hasChoices)
        {
            ShowChoices(dialogueLines[currentLineIndex].choices);
            return;
        }

        currentLineIndex++;

        if (currentLineIndex < dialogueLines.Length)
        {
            DisplayLine(dialogueLines[currentLineIndex]);

            if (dialogueLines[currentLineIndex].changeSceneAfterThis)
            {
                waitingForSceneChange = true;
                StartCoroutine(ChangeSceneWithFade(
                    dialogueLines[currentLineIndex].sceneToLoad,
                    dialogueLines[currentLineIndex].sceneChangeDelay,
                    dialogueLines[currentLineIndex].useFadeEffect,
                    dialogueLines[currentLineIndex].fadeColor,
                    dialogueLines[currentLineIndex].fadeDuration
                ));
            }
        }
        else
        {
            EndDialogue();
        }
    }

    private void ShowChoices(string[] choices)
    {
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(true);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            bool shouldActivate = i < choices.Length;
            choiceButtons[i].gameObject.SetActive(shouldActivate);

            if (shouldActivate)
            {
                choiceTexts[i].text = choices[i];
            }
        }
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        lastChoiceIndex = choiceIndex;
        choicePanel.SetActive(false);

        // เพิ่มคะแนนถ้ามี
        if (choiceIndex < choiceScores.Length)
        {
            GameManager.Instance?.AddScore(choiceScores[choiceIndex]);
        }

        // ตรวจสอบว่ามีบทสนทนาหลังเลือกหรือไม่
        if (!string.IsNullOrEmpty(dialogueLines[currentLineIndex].postChoiceDialogue))
        {
            // แสดงบทสนทนาหลังเลือก (เหมือนภาพตัวอย่าง)
            dialoguePanel.SetActive(true);

            if (useTypingEffect)
            {
                StartCoroutine(TypePostChoiceDialogue(dialogueLines[currentLineIndex].postChoiceDialogue));
            }
            else
            {
                dialogueText.text = dialogueLines[currentLineIndex].postChoiceDialogue;
                CheckForPostChoiceSceneChange();
            }
            return;
        }

        ProcessChoiceSelection(choiceIndex);
    }

    private IEnumerator TypePostChoiceDialogue(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;

            if (typingSound != null && char.IsLetterOrDigit(letter))
            {
                PlaySoundEffect(typingSound);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        CheckForPostChoiceSceneChange();
    }

    private void CheckForPostChoiceSceneChange()
    {
        if (dialogueLines[currentLineIndex].changeSceneAfterThis)
        {
            waitingForSceneChange = true;
            StartCoroutine(ChangeSceneAfterPostChoiceDialogue());
        }
    }

    private IEnumerator ChangeSceneAfterPostChoiceDialogue()
    {
        // รอจนกว่าผู้เล่นจะกดเพื่อปิดบทสนทนา
        while (dialoguePanel.activeSelf)
        {
            if (Input.GetKeyDown(interactKey))
            {
                break;
            }
            yield return null;
        }

        // เปลี่ยนฉาก
        yield return ChangeSceneWithFade(
            dialogueLines[currentLineIndex].sceneToLoad,
            dialogueLines[currentLineIndex].sceneChangeDelay,
            dialogueLines[currentLineIndex].useFadeEffect,
            dialogueLines[currentLineIndex].fadeColor,
            dialogueLines[currentLineIndex].fadeDuration
        );
    }

    private void ProcessChoiceSelection(int choiceIndex)
    {
        if (dialogueLines[currentLineIndex].choiceLeadsTo != null &&
            choiceIndex < dialogueLines[currentLineIndex].choiceLeadsTo.Length)
        {
            int nextLine = dialogueLines[currentLineIndex].choiceLeadsTo[choiceIndex];

            if (nextLine == -1)
            {
                EndDialogue();
                return;
            }
            else if (nextLine >= 0 && nextLine < dialogueLines.Length)
            {
                currentLineIndex = nextLine;
                dialoguePanel.SetActive(true);
                DisplayLine(dialogueLines[currentLineIndex]);

                if (dialogueLines[currentLineIndex].changeSceneAfterThis)
                {
                    waitingForSceneChange = true;
                    StartCoroutine(ChangeSceneWithFade(
                        dialogueLines[currentLineIndex].sceneToLoad,
                        dialogueLines[currentLineIndex].sceneChangeDelay,
                        dialogueLines[currentLineIndex].useFadeEffect,
                        dialogueLines[currentLineIndex].fadeColor,
                        dialogueLines[currentLineIndex].fadeDuration
                    ));
                }
                return;
            }
        }

        // ไม่มีอะไรกำหนด - ไปบรรทัดต่อไป
        currentLineIndex++;
        if (currentLineIndex < dialogueLines.Length)
        {
            dialoguePanel.SetActive(true);
            DisplayLine(dialogueLines[currentLineIndex]);

            if (dialogueLines[currentLineIndex].changeSceneAfterThis)
            {
                waitingForSceneChange = true;
                StartCoroutine(ChangeSceneWithFade(
                    dialogueLines[currentLineIndex].sceneToLoad,
                    dialogueLines[currentLineIndex].sceneChangeDelay,
                    dialogueLines[currentLineIndex].useFadeEffect,
                    dialogueLines[currentLineIndex].fadeColor,
                    dialogueLines[currentLineIndex].fadeDuration
                ));
            }
        }
        else
        {
            EndDialogue();
        }
    }

    private IEnumerator ChangeSceneWithFade(string sceneName, float delay, bool useFade, Color fadeColor, float fadeDuration)
    {
        EndDialogue();

        yield return new WaitForSeconds(delay);

        if (useFade && fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);

            // Fade Out
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
                fadeOverlay.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                yield return null;
            }

            fadeOverlay.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1);
        }

        SceneManager.LoadScene(sceneName);
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
        PlaySoundEffect(dialogueCloseSound);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }

    public int GetLastChoiceIndex()
    {
        return lastChoiceIndex;
    }
}
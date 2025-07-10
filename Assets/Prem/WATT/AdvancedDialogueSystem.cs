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

    [Tooltip("บรรทัดต่อไปสำหรับแต่ละตัวเลือก (ใส่ -1 เพื่อจบบทสนทนา)")]
    public int[] choiceLeadsTo;

    [Tooltip("บทสนทนาหลังเลือกตัวเลือกนี้ (จะใช้แทน choiceLeadsTo ถ้ามีการกำหนด)")]
    public string postChoiceDialogue;
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
    public int[] choiceScores;

    [Header("การจัดการฉาก")]
    public string[] sceneNamesForChoices;
    public float sceneChangeDelay = 1f;

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

        // ตั้งค่าปุ่มตัวเลือก
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int choiceIndex = i;
            choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(choiceIndex));
        }

        // ตรวจสอบความยาวของอาร์เรย์ชื่อฉาก
        if (sceneNamesForChoices == null || sceneNamesForChoices.Length != choiceButtons.Length)
        {
            sceneNamesForChoices = new string[choiceButtons.Length];
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
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(true);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < choices.Length)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceTexts[i].text = choices[i];
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
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
            // แสดงบทสนทนาหลังเลือก
            dialoguePanel.SetActive(true);
            dialogueText.text = dialogueLines[currentLineIndex].postChoiceDialogue;
            isTyping = false;
            return;
        }

        // ตรวจสอบว่ามีการกำหนดบรรทัดต่อไปหรือไม่
        if (dialogueLines[currentLineIndex].choiceLeadsTo != null &&
            choiceIndex < dialogueLines[currentLineIndex].choiceLeadsTo.Length)
        {
            int nextLine = dialogueLines[currentLineIndex].choiceLeadsTo[choiceIndex];

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

        // ตรวจสอบว่าต้องการเปลี่ยนฉากหรือไม่
        if (choiceIndex < sceneNamesForChoices.Length &&
            !string.IsNullOrEmpty(sceneNamesForChoices[choiceIndex]))
        {
            StartCoroutine(ChangeSceneAfterDelay(sceneNamesForChoices[choiceIndex]));
        }
        else
        {
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
    }

    private IEnumerator ChangeSceneAfterDelay(string sceneName)
    {
        EndDialogue();
        yield return new WaitForSeconds(sceneChangeDelay);
        SceneManager.LoadScene(sceneName);
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
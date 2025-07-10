using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image speakerImage;

    [Header("Dialogue Settings")]
    public string defaultNpcName = "NPC";
    public DialogueLine[] dialogueLines;
    public float interactionRadius = 2f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Typing Effect")]
    public float typingSpeed = 0.05f;
    public bool useTypingEffect = true;

    [Header("Choice Settings")]
    public GameObject choicePanel;
    public Button[] choiceButtons;
    public TextMeshProUGUI[] choiceTexts;

    [Header("Scene Management")]
    public string[] sceneNamesForChoices;
    public float sceneChangeDelay = 1f;

    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(3, 10)]
        public string text;
        public string characterName;
        public Sprite characterSprite;
        public bool hasChoices;
        public string[] choices;
        public DialogueLine[] followUpLines; // บทสนทนาต่อเนื่องหลังเลือกคำตอบ
    }

    private bool isInRange = false;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private int currentLineIndex = 0;
    private DialogueLine[] currentDialogueLines;

    private void Start()
    {
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
        currentDialogueLines = dialogueLines;

        // ตั้งค่าปุ่มตัวเลือก
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int choiceIndex = i;
            choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(choiceIndex));
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
        DisplayLine(currentDialogueLines[currentLineIndex]);
    }

    private void DisplayLine(DialogueLine line)
    {
        nameText.text = string.IsNullOrEmpty(line.characterName) ? defaultNpcName : line.characterName;

        if (line.characterSprite != null)
        {
            speakerImage.sprite = line.characterSprite;
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

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void CompleteSentence()
    {
        StopAllCoroutines();
        dialogueText.text = currentDialogueLines[currentLineIndex].text;
        isTyping = false;
    }

    private void NextLine()
    {
        currentLineIndex++;

        if (currentLineIndex < currentDialogueLines.Length)
        {
            DisplayLine(currentDialogueLines[currentLineIndex]);
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
        choicePanel.SetActive(false);
        DialogueLine[] selectedLines = currentDialogueLines[currentLineIndex].followUpLines;

        // ตรวจสอบว่ามีบทสนทนาต่อหรือไม่
        if (selectedLines != null && selectedLines.Length > 0)
        {
            currentDialogueLines = selectedLines;
            currentLineIndex = 0;
            dialoguePanel.SetActive(true);
            DisplayLine(currentDialogueLines[currentLineIndex]);
        }
        // ตรวจสอบว่าต้องการเปลี่ยนฉากหรือไม่
        else if (choiceIndex < sceneNamesForChoices.Length &&
                !string.IsNullOrEmpty(sceneNamesForChoices[choiceIndex]))
        {
            StartCoroutine(ChangeSceneAfterDelay(sceneNamesForChoices[choiceIndex]));
        }
        else
        {
            EndDialogue();
        }
    }

    private IEnumerator ChangeSceneAfterDelay(string sceneName)
    {
        yield return new WaitForSeconds(sceneChangeDelay);
        SceneManager.LoadScene(sceneName);
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
        currentDialogueLines = dialogueLines;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NPCDialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image speakerImage;

    [Header("Dialogue Settings")]
    public string defaultNpcName = "Hyung-gil"; // Default name if line doesn't specify
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
        public string characterName; // Added field for per-line character name
        public Sprite characterSprite;
        public AudioClip voiceOver;
    }

    // Static reference for global control
    private static NPCDialogueSystem[] allDialogueSystems;
    private static DialogueLine[] globalDialogueLines;
    private static string globalDefaultName = "";

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

    private void OnEnable()
    {
        // Register this dialogue system when enabled
        if (allDialogueSystems == null)
        {
            allDialogueSystems = new NPCDialogueSystem[0];
        }

        var list = new System.Collections.Generic.List<NPCDialogueSystem>(allDialogueSystems);
        if (!list.Contains(this))
        {
            list.Add(this);
            allDialogueSystems = list.ToArray();
        }

        // Apply global settings if they exist
        if (globalDialogueLines != null && globalDialogueLines.Length > 0)
        {
            dialogueLines = globalDialogueLines;
        }

        if (!string.IsNullOrEmpty(globalDefaultName))
        {
            defaultNpcName = globalDefaultName;
        }
    }

    private void OnDisable()
    {
        // Unregister this dialogue system when disabled
        if (allDialogueSystems != null)
        {
            var list = new System.Collections.Generic.List<NPCDialogueSystem>(allDialogueSystems);
            if (list.Contains(this))
            {
                list.Remove(this);
                allDialogueSystems = list.ToArray();
            }
        }
    }

    // Static method to change dialogue for all NPCs
    public static void SetGlobalDialogue(DialogueLine[] newLines, string newDefaultName = "")
    {
        globalDialogueLines = newLines;
        globalDefaultName = newDefaultName;

        if (allDialogueSystems != null)
        {
            foreach (var system in allDialogueSystems)
            {
                if (system != null)
                {
                    system.dialogueLines = newLines;
                    if (!string.IsNullOrEmpty(newDefaultName))
                    {
                        system.defaultNpcName = newDefaultName;
                    }
                }
            }
        }
    }

    // Static method to change dialogue settings for all NPCs
    public static void SetGlobalDialogueSettings(float newTypingSpeed, bool newUseTypingEffect)
    {
        if (allDialogueSystems != null)
        {
            foreach (var system in allDialogueSystems)
            {
                if (system != null)
                {
                    system.typingSpeed = newTypingSpeed;
                    system.useTypingEffect = newUseTypingEffect;
                }
            }
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

        // Set initial name from line or use default
        DisplayLine(dialogueLines[currentLineIndex]);

        if (dialogueOpenSound != null)
        {
            audioSource.PlayOneShot(dialogueOpenSound);
        }
    }

    private void DisplayLine(DialogueLine line)
    {
        // Set character name - use line-specific name if available, otherwise use default
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
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
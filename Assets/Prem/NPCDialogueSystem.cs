using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NPCDialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;       // ���ͧ UI �������ͧ��ʹ��� / The main dialogue panel UI
    public TextMeshProUGUI nameText;      // ��ͤ����ʴ����͵���Ф� / Text for character name display
    public TextMeshProUGUI dialogueText;  // ��ͤ�����ʹ��� / Dialogue content text
    public Image speakerImage;            // �Ҿ����Ф÷��ٴ / Image of speaking character

    [Header("Dialogue Settings")]
    public string npcName = "Hyung-gil";  // ���� NPC ������� / Default NPC name
    public DialogueLine[] dialogueLines;  // ��������ͧ��ʹ��� / Array of dialogue lines
    public float interactionRadius = 2f;  // ���з�����������ö��ͺ�� / Interaction distance
    public KeyCode interactKey = KeyCode.E; // ��������Ѻ��ͺ / Interaction key

    [Header("Typing Effect")]
    public float typingSpeed = 0.05f;     // �������ǡ�þ�����ͤ��� / Text typing speed (seconds per character)
    public bool useTypingEffect = true;   // �Դ/�Դ�Ϳ࿡�������ͤ��� / Toggle typing effect

    [Header("Image Pop Effect")]
    public float popScale = 1.2f;         // ��Ҵ��â���������� / Scale multiplier for pop effect
    public float popDuration = 0.2f;      // ���������Ϳ࿡���� / Duration of pop animation
    public AudioClip popSound;            // ���§������ٻ�� / Sound when image pops

    [Header("Sound Effects")]
    public AudioClip typingSound;         // ���§������ͤ��� / Typing sound effect
    public AudioClip dialogueOpenSound;   // ���§�Դ��ʹ��� / Dialogue open sound
    public AudioClip dialogueCloseSound;  // ���§�Դ��ʹ��� / Dialogue close sound

    // �����ʶҹ� / State variables
    private bool isInRange = false;       // ���������������������� / Is player in interaction range
    private bool isDialogueActive = false; // ���ѧʹ�������������� / Is dialogue currently active
    private bool isTyping = false;        // ���ѧ������ͤ�������������� / Is text currently typing
    private int currentLineIndex = 0;     // ��÷Ѵ�Ѩ�غѹ㹺�ʹ��� / Current line index in dialogue
    private Vector3 originalScale;        // ��Ҵ����ͧ�ٻ�Ҿ / Original scale of speaker image
    private AudioSource audioSource;      // ����Ѻ������§ / Audio source component

    // ��������Ѻ�红��������к�÷Ѵ��ʹ��� / Class for storing each dialogue line data
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(3, 10)]
        public string text;               // ��ͤ�����ʹ��� / Dialogue text
        public Sprite characterSprite;    // �ٻ����Ф�����Ѻ��÷Ѵ��� / Character sprite for this line
        public AudioClip voiceOver;       // ���§�ٴ੾�к�÷Ѵ / Voice over audio for this line
    }

    private void Start()
    {
        // �Դ���ͧ��ʹ��������������� / Disable dialogue panel at start
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // �红�Ҵ����ͧ�ٻ�Ҿ / Store original image scale
        originalScale = speakerImage.transform.localScale;

        // ����� AudioSource / Set up audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Update()
    {
        // ��Ǩ�ͺ��Ҽ��������������������� / Check if player is in range
        CheckPlayerInRange();

        // ����͡�������ͺ / When interaction key is pressed
        if (isInRange && Input.GetKeyDown(interactKey))
        {
            if (!isDialogueActive)
            {
                StartDialogue();  // �������ʹ��� / Start dialogue
            }
            else if (isTyping)
            {
                CompleteSentence(); // ������þ�����ͤ��� / Skip typing animation
            }
            else
            {
                NextLine();       // ��÷Ѵ���� / Go to next line
            }
        }
    }

    // ��Ǩ�ͺ��Ҽ����������������ͺ������� / Check if player is within interaction range
    private void CheckPlayerInRange()
    {
        // ���ѵ�ط����������շ���˹� / Find all colliders in interaction radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactionRadius);
        isInRange = false;

        // ��Ǩ�ͺ����ռ������������ / Check if player is among them
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                isInRange = true;
                break;
            }
        }
    }

    // �������ʹ��� / Start the dialogue
    public void StartDialogue()
    {
        isDialogueActive = true;
        currentLineIndex = 0;  // ������ҡ��÷Ѵ�á / Start from first line
        dialoguePanel.SetActive(true);

        // ��駤�Ҫ�������ʴ���÷Ѵ�á / Set name and show first line
        nameText.text = npcName;
        DisplayLine(dialogueLines[currentLineIndex]);

        // ������§�Դ��ʹ��� (�����) / Play open sound if available
        if (dialogueOpenSound != null)
        {
            audioSource.PlayOneShot(dialogueOpenSound);
        }
    }

    // �ʴ���÷Ѵ��ʹ��� / Display a dialogue line
    private void DisplayLine(DialogueLine line)
    {
        // ����¹�ٻ����Ф��������Ϳ࿡���� / Change character sprite and play pop effect
        if (line.characterSprite != null)
        {
            speakerImage.sprite = line.characterSprite;
            StartCoroutine(PopImageEffect());
        }

        // ������§�ٴ੾�к�÷Ѵ (�����) / Play voice over if available
        if (line.voiceOver != null)
        {
            audioSource.PlayOneShot(line.voiceOver);
        }

        // �ʴ���ͤ��������Ϳ࿡�����������ʴ��ѹ�� / Show text with typing effect or immediately
        if (useTypingEffect)
        {
            StartCoroutine(TypeLine(line.text));
        }
        else
        {
            dialogueText.text = line.text;
        }
    }

    // �Ϳ࿡�����ٻ�Ҿ / Image pop animation effect
    private IEnumerator PopImageEffect()
    {
        // ������§�� (�����) / Play pop sound if available
        if (popSound != null)
        {
            audioSource.PlayOneShot(popSound);
        }

        float timer = 0f;
        Vector3 targetScale = originalScale * popScale;

        // ���¢�Ҵ / Scale up
        while (timer < popDuration / 2)
        {
            timer += Time.deltaTime;
            speakerImage.transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / (popDuration / 2));
            yield return null;
        }

        timer = 0f;

        // ��Ѻ��袹Ҵ���� / Scale back down
        while (timer < popDuration / 2)
        {
            timer += Time.deltaTime;
            speakerImage.transform.localScale = Vector3.Lerp(targetScale, originalScale, timer / (popDuration / 2));
            yield return null;
        }

        speakerImage.transform.localScale = originalScale;
    }

    // ������ͤ������е���ѡ�� / Type text character by character
    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";  // ��ҧ��ͤ������ / Clear previous text

        // ��������е���ѡ�� / Type each character
        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;

            // ������§���������Ѻ����ѡ����е���Ţ / Play typing sound for letters and numbers
            if (typingSound != null && char.IsLetterOrDigit(letter))
            {
                audioSource.PlayOneShot(typingSound);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    // ������þ��������ʴ���ͤ����������ѹ�� / Skip typing and show full text immediately
    private void CompleteSentence()
    {
        StopAllCoroutines();  // ��ش Coroutine ������ / Stop all running coroutines
        dialogueText.text = dialogueLines[currentLineIndex].text;
        speakerImage.transform.localScale = originalScale;  // ���絢�Ҵ�Ҿ / Reset image scale
        isTyping = false;
    }

    // ��ѧ��÷Ѵ���� / Advance to next dialogue line
    private void NextLine()
    {
        currentLineIndex++;

        // ��Ǩ�ͺ����ѧ�պ�ʹ����ա������� / Check if more lines exist
        if (currentLineIndex < dialogueLines.Length)
        {
            DisplayLine(dialogueLines[currentLineIndex]);
        }
        else
        {
            EndDialogue();  // ����ʹ��� / End dialogue
        }
    }

    // ����ʹ��� / End the dialogue
    public void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);

        // ������§�Դ��ʹ��� (�����) / Play close sound if available
        if (dialogueCloseSound != null)
        {
            audioSource.PlayOneShot(dialogueCloseSound);
        }
    }

    // �Ҵ����ͺǧ����Ѻ��Ǩ�ͺ������ͺ� Scene / Draw interaction radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
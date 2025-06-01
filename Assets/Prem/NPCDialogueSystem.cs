using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NPCDialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;       // กล่อง UI ทั้งหมดของบทสนทนา / The main dialogue panel UI
    public TextMeshProUGUI nameText;      // ข้อความแสดงชื่อตัวละคร / Text for character name display
    public TextMeshProUGUI dialogueText;  // ข้อความบทสนทนา / Dialogue content text
    public Image speakerImage;            // ภาพตัวละครที่พูด / Image of speaking character

    [Header("Dialogue Settings")]
    public string npcName = "Hyung-gil";  // ชื่อ NPC เริ่มต้น / Default NPC name
    public DialogueLine[] dialogueLines;  // อาร์เรย์ของบทสนทนา / Array of dialogue lines
    public float interactionRadius = 2f;  // ระยะที่ผู้เล่นสามารถโต้ตอบได้ / Interaction distance
    public KeyCode interactKey = KeyCode.E; // ปุ่มสำหรับโต้ตอบ / Interaction key

    [Header("Typing Effect")]
    public float typingSpeed = 0.05f;     // ความเร็วการพิมพ์ข้อความ / Text typing speed (seconds per character)
    public bool useTypingEffect = true;   // เปิด/ปิดเอฟเฟกต์พิมพ์ข้อความ / Toggle typing effect

    [Header("Image Pop Effect")]
    public float popScale = 1.2f;         // ขนาดการขยายเมื่อเด้ง / Scale multiplier for pop effect
    public float popDuration = 0.2f;      // ระยะเวลาเอฟเฟกต์เด้ง / Duration of pop animation
    public AudioClip popSound;            // เสียงเมื่อรูปเด้ง / Sound when image pops

    [Header("Sound Effects")]
    public AudioClip typingSound;         // เสียงพิมพ์ข้อความ / Typing sound effect
    public AudioClip dialogueOpenSound;   // เสียงเปิดบทสนทนา / Dialogue open sound
    public AudioClip dialogueCloseSound;  // เสียงปิดบทสนทนา / Dialogue close sound

    // ตัวแปรสถานะ / State variables
    private bool isInRange = false;       // ผู้เล่นอยู่ในระยะหรือไม่ / Is player in interaction range
    private bool isDialogueActive = false; // กำลังสนทนาอยู่หรือไม่ / Is dialogue currently active
    private bool isTyping = false;        // กำลังพิมพ์ข้อความอยู่หรือไม่ / Is text currently typing
    private int currentLineIndex = 0;     // บรรทัดปัจจุบันในบทสนทนา / Current line index in dialogue
    private Vector3 originalScale;        // ขนาดเดิมของรูปภาพ / Original scale of speaker image
    private AudioSource audioSource;      // สำหรับเล่นเสียง / Audio source component

    // คลาสสำหรับเก็บข้อมูลแต่ละบรรทัดบทสนทนา / Class for storing each dialogue line data
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(3, 10)]
        public string text;               // ข้อความบทสนทนา / Dialogue text
        public Sprite characterSprite;    // รูปตัวละครสำหรับบรรทัดนี้ / Character sprite for this line
        public AudioClip voiceOver;       // เสียงพูดเฉพาะบรรทัด / Voice over audio for this line
    }

    private void Start()
    {
        // ปิดกล่องบทสนทนาเมื่อเริ่มเกม / Disable dialogue panel at start
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // เก็บขนาดเดิมของรูปภาพ / Store original image scale
        originalScale = speakerImage.transform.localScale;

        // เตรียม AudioSource / Set up audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Update()
    {
        // ตรวจสอบว่าผู้เล่นอยู่ในระยะหรือไม่ / Check if player is in range
        CheckPlayerInRange();

        // เมื่อกดปุ่มโต้ตอบ / When interaction key is pressed
        if (isInRange && Input.GetKeyDown(interactKey))
        {
            if (!isDialogueActive)
            {
                StartDialogue();  // เริ่มบทสนทนา / Start dialogue
            }
            else if (isTyping)
            {
                CompleteSentence(); // ข้ามการพิมพ์ข้อความ / Skip typing animation
            }
            else
            {
                NextLine();       // บรรทัดต่อไป / Go to next line
            }
        }
    }

    // ตรวจสอบว่าผู้เล่นอยู่ในระยะโต้ตอบหรือไม่ / Check if player is within interaction range
    private void CheckPlayerInRange()
    {
        // หาวัตถุทั้งหมดในรัศมีที่กำหนด / Find all colliders in interaction radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactionRadius);
        isInRange = false;

        // ตรวจสอบว่ามีผู้เล่นหรือไม่ / Check if player is among them
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                isInRange = true;
                break;
            }
        }
    }

    // เริ่มบทสนทนา / Start the dialogue
    public void StartDialogue()
    {
        isDialogueActive = true;
        currentLineIndex = 0;  // เริ่มจากบรรทัดแรก / Start from first line
        dialoguePanel.SetActive(true);

        // ตั้งค่าชื่อและแสดงบรรทัดแรก / Set name and show first line
        nameText.text = npcName;
        DisplayLine(dialogueLines[currentLineIndex]);

        // เล่นเสียงเปิดบทสนทนา (ถ้ามี) / Play open sound if available
        if (dialogueOpenSound != null)
        {
            audioSource.PlayOneShot(dialogueOpenSound);
        }
    }

    // แสดงบรรทัดบทสนทนา / Display a dialogue line
    private void DisplayLine(DialogueLine line)
    {
        // เปลี่ยนรูปตัวละครและเล่นเอฟเฟกต์เด้ง / Change character sprite and play pop effect
        if (line.characterSprite != null)
        {
            speakerImage.sprite = line.characterSprite;
            StartCoroutine(PopImageEffect());
        }

        // เล่นเสียงพูดเฉพาะบรรทัด (ถ้ามี) / Play voice over if available
        if (line.voiceOver != null)
        {
            audioSource.PlayOneShot(line.voiceOver);
        }

        // แสดงข้อความด้วยเอฟเฟกต์พิมพ์หรือแสดงทันที / Show text with typing effect or immediately
        if (useTypingEffect)
        {
            StartCoroutine(TypeLine(line.text));
        }
        else
        {
            dialogueText.text = line.text;
        }
    }

    // เอฟเฟกต์เด้งรูปภาพ / Image pop animation effect
    private IEnumerator PopImageEffect()
    {
        // เล่นเสียงเด้ง (ถ้ามี) / Play pop sound if available
        if (popSound != null)
        {
            audioSource.PlayOneShot(popSound);
        }

        float timer = 0f;
        Vector3 targetScale = originalScale * popScale;

        // ขยายขนาด / Scale up
        while (timer < popDuration / 2)
        {
            timer += Time.deltaTime;
            speakerImage.transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / (popDuration / 2));
            yield return null;
        }

        timer = 0f;

        // กลับสู่ขนาดปกติ / Scale back down
        while (timer < popDuration / 2)
        {
            timer += Time.deltaTime;
            speakerImage.transform.localScale = Vector3.Lerp(targetScale, originalScale, timer / (popDuration / 2));
            yield return null;
        }

        speakerImage.transform.localScale = originalScale;
    }

    // พิมพ์ข้อความทีละตัวอักษร / Type text character by character
    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";  // ล้างข้อความเก่า / Clear previous text

        // พิมพ์แต่ละตัวอักษร / Type each character
        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;

            // เล่นเสียงพิมพ์สำหรับตัวอักษรและตัวเลข / Play typing sound for letters and numbers
            if (typingSound != null && char.IsLetterOrDigit(letter))
            {
                audioSource.PlayOneShot(typingSound);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    // ข้ามการพิมพ์และแสดงข้อความทั้งหมดทันที / Skip typing and show full text immediately
    private void CompleteSentence()
    {
        StopAllCoroutines();  // หยุด Coroutine ทั้งหมด / Stop all running coroutines
        dialogueText.text = dialogueLines[currentLineIndex].text;
        speakerImage.transform.localScale = originalScale;  // รีเซ็ตขนาดภาพ / Reset image scale
        isTyping = false;
    }

    // ไปยังบรรทัดต่อไป / Advance to next dialogue line
    private void NextLine()
    {
        currentLineIndex++;

        // ตรวจสอบว่ายังมีบทสนทนาอีกหรือไม่ / Check if more lines exist
        if (currentLineIndex < dialogueLines.Length)
        {
            DisplayLine(dialogueLines[currentLineIndex]);
        }
        else
        {
            EndDialogue();  // จบบทสนทนา / End dialogue
        }
    }

    // จบบทสนทนา / End the dialogue
    public void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);

        // เล่นเสียงปิดบทสนทนา (ถ้ามี) / Play close sound if available
        if (dialogueCloseSound != null)
        {
            audioSource.PlayOneShot(dialogueCloseSound);
        }
    }

    // วาดเส้นรอบวงสำหรับตรวจสอบระยะโต้ตอบใน Scene / Draw interaction radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
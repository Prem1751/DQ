using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [System.Serializable]
    public class DialogueContent
    {
        public string npcName;
        [TextArea(3, 10)]
        public string[] messages;
    }

    [Header("UI Components")]
    public GameObject dialogueUI;
    public TMP_Text nameDisplay;
    public TMP_Text messageDisplay;
    public float textSpeed = 0.05f;

    public GameObject questionUI;
    public TMP_Text questionText;

    [Header("Static Answer Buttons (Set in Inspector)")]
    public Button answerButton1;
    public Button answerButton2;
    public Button answerButton3;
    public Button answerButton4;
    public Button answerButton5;
    public Button answerButton6;

    [Header("Dialogue Content")]
    public DialogueContent dialogue;
    public string question;

    [Header("Answer Settings")]
    [Tooltip("คำตอบที่แสดงบนปุ่ม (สูงสุด 6 คำตอบ)")]
    public string[] answerTexts = new string[6];

    [Tooltip("ชื่อ Scene ที่ต้องการไปเมื่อกดปุ่ม (ต้องตรงกับชื่อใน Build Settings)")]
    public string[] targetScenes = new string[6];

    [Header("Interaction Settings")]
    public float interactionRange = 2f;
    public KeyCode interactKey = KeyCode.E;
    public LayerMask npcLayerMask = 1;

    private Queue<string> sentenceQueue;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private string currentSentence;
    private string currentNPCName;
    private bool canContinueDialogue = false;

    private Button[] answerButtons;
    private TMP_Text[] buttonTextComponents;

    void Start()
    {
        Debug.Log("=== DialogueManager เริ่มทำงาน ===");

        // Initialize button arrays
        answerButtons = new Button[] { answerButton1, answerButton2, answerButton3, answerButton4, answerButton5, answerButton6 };
        buttonTextComponents = new TMP_Text[6];

        sentenceQueue = new Queue<string>();

        // Setup UI
        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        if (questionUI != null)
            questionUI.SetActive(false);

        // Get text components from buttons
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] != null)
            {
                buttonTextComponents[i] = answerButtons[i].GetComponentInChildren<TMP_Text>();
                answerButtons[i].gameObject.SetActive(false);
            }
        }

        HideAllAnswerButtons();
        ValidateComponents();
    }

    void ValidateComponents()
    {
        Debug.Log("=== ตรวจสอบ Component ===");

        if (dialogueUI == null) Debug.LogError("❌ dialogueUI ไม่ได้กำหนด!");
        if (nameDisplay == null) Debug.LogError("❌ nameDisplay ไม่ได้กำหนด!");
        if (messageDisplay == null) Debug.LogError("❌ messageDisplay ไม่ได้กำหนด!");
        if (questionUI == null) Debug.LogError("❌ questionUI ไม่ได้กำหนด!");
        if (questionText == null) Debug.LogError("❌ questionText ไม่ได้กำหนด!");

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] == null)
                Debug.LogError($"❌ answerButton{i + 1} ไม่ได้กำหนด!");
            else if (buttonTextComponents[i] == null)
                Debug.LogWarning($"⚠️ ไม่พบ Text Component ใน answerButton{i + 1}");
        }

        Debug.Log("✅ การตรวจสอบ Component เสร็จสิ้น");
    }

    void Update()
    {
        if (Input.GetKeyDown(interactKey) && !isDialogueActive)
        {
            CheckForNPCInteraction();
        }

        if (isDialogueActive && Input.GetKeyDown(interactKey) && canContinueDialogue && !isTyping)
        {
            ShowNextSentence();
        }
    }

    void CheckForNPCInteraction()
    {
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, interactionRange, npcLayerMask);

        foreach (Collider2D collider in nearbyColliders)
        {
            if (collider.CompareTag("NPC"))
            {
                NPCInfo npcInfo = collider.GetComponent<NPCInfo>();
                if (npcInfo != null)
                {
                    currentNPCName = npcInfo.npcName;
                }
                else
                {
                    currentNPCName = "เพื่อน";
                }

                StartDialogue();
                break;
            }
        }
    }

    public void StartDialogue()
    {
        Debug.Log("🎬 เริ่มบทสนทนา");

        isDialogueActive = true;
        canContinueDialogue = false;

        if (dialogueUI != null)
            dialogueUI.SetActive(true);

        if (nameDisplay != null)
        {
            nameDisplay.text = currentNPCName;
        }

        sentenceQueue.Clear();

        if (dialogue != null && dialogue.messages != null)
        {
            foreach (string sentence in dialogue.messages)
            {
                sentenceQueue.Enqueue(sentence);
            }
        }

        ShowNextSentence();
    }

    public void ShowNextSentence()
    {
        if (isTyping)
        {
            // Skip typing animation
            StopAllCoroutines();
            messageDisplay.text = currentSentence;
            isTyping = false;
            canContinueDialogue = true;
            return;
        }

        if (sentenceQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentSentence = sentenceQueue.Dequeue();
        StartCoroutine(TypeSentence(currentSentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        canContinueDialogue = false;
        messageDisplay.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            messageDisplay.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
        canContinueDialogue = true;
    }

    void EndDialogue()
    {
        Debug.Log("🏁 จบบทสนทนา");

        isDialogueActive = false;
        canContinueDialogue = false;

        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        ShowQuestion();
    }

    void ShowQuestion()
    {
        Debug.Log("❓ แสดงคำถาม");

        if (questionUI == null || questionText == null)
        {
            Debug.LogError("❌ Missing UI components for question!");
            return;
        }

        questionUI.SetActive(true);
        questionText.text = question;

        SetupAnswerButtons();
    }

    void SetupAnswerButtons()
    {
        Debug.Log("=== ตั้งค่าปุ่มคำตอบแบบ Static ===");

        HideAllAnswerButtons();

        int activeButtonCount = 0;

        for (int i = 0; i < answerTexts.Length; i++)
        {
            // ตรวจสอบว่ามีข้อความคำตอบและชื่อ Scene
            if (!string.IsNullOrEmpty(answerTexts[i]) && !string.IsNullOrEmpty(targetScenes[i]))
            {
                if (answerButtons[i] != null && buttonTextComponents[i] != null)
                {
                    answerButtons[i].gameObject.SetActive(true);
                    buttonTextComponents[i].text = answerTexts[i];
                    activeButtonCount++;

                    Debug.Log($"✅ ตั้งค่าปุ่ม {i + 1}: '{answerTexts[i]}' → Scene: '{targetScenes[i]}'");
                }
                else
                {
                    Debug.LogError($"❌ ไม่สามารถตั้งค่าปุ่ม {i + 1} ได้");
                }
            }
            else
            {
                Debug.Log($"➖ ปุ่ม {i + 1} ไม่ได้ใช้งาน (ไม่มีคำตอบหรือชื่อ Scene)");
            }
        }

        Debug.Log($"✅ เปิดใช้งานปุ่มแล้ว {activeButtonCount} ปุ่ม");
    }

    void HideAllAnswerButtons()
    {
        foreach (Button button in answerButtons)
        {
            if (button != null)
                button.gameObject.SetActive(false);
        }
    }

    // ==================== ฟังก์ชันสำหรับปุ่มคำตอบ ====================
    // ฟังก์ชันเหล่านี้จะถูกเรียกจาก Inspector เมื่อกดปุ่ม

    public void OnAnswerButton1Clicked() { OnAnswerButtonClicked(0); }
    public void OnAnswerButton2Clicked() { OnAnswerButtonClicked(1); }
    public void OnAnswerButton3Clicked() { OnAnswerButtonClicked(2); }
    public void OnAnswerButton4Clicked() { OnAnswerButtonClicked(3); }
    public void OnAnswerButton5Clicked() { OnAnswerButtonClicked(4); }
    public void OnAnswerButton6Clicked() { OnAnswerButtonClicked(5); }

    private void OnAnswerButtonClicked(int buttonIndex)
    {
        Debug.Log($"🎯 ปุ่มที่ {buttonIndex + 1} ถูกคลิก!");

        if (buttonIndex < 0 || buttonIndex >= targetScenes.Length)
        {
            Debug.LogError($"❌ Index {buttonIndex} ไม่ถูกต้อง!");
            return;
        }

        string sceneName = targetScenes[buttonIndex];
        string answerText = answerTexts[buttonIndex];

        Debug.Log($"📋 คำตอบ: {answerText}");
        Debug.Log($"🎯 เป้าหมาย: {sceneName}");

        if (questionUI != null)
        {
            questionUI.SetActive(false);
            Debug.Log("✅ ปิด Question UI แล้ว");
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("❌ Target scene name is null or empty!");
            return;
        }

        LoadTargetScene(sceneName);
    }

    // ==================== ระบบโหลด Scene ====================

    void LoadTargetScene(string sceneName)
    {
        Debug.Log($"🚀 พยายามโหลด Scene: '{sceneName}'");

        // ตรวจสอบว่า Scene อยู่ใน Build Settings หรือไม่
        if (IsSceneInBuildSettings(sceneName))
        {
            Debug.Log($"✅ Scene '{sceneName}' พบใน Build Settings");
            StartCoroutine(LoadSceneAsync(sceneName));
        }
        else
        {
            Debug.LogError($"❌ Scene '{sceneName}' ไม่พบใน Build Settings!");
            ListScenesInBuildSettings();

            // โหลด Scene แรกแทน
            Debug.Log("🔄 ลองโหลด Scene แรกแทน...");
            if (SceneManager.sceneCountInBuildSettings > 0)
            {
                SceneManager.LoadScene(0);
            }
        }
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        Debug.Log($"🔄 เริ่มโหลด Scene แบบ Async: '{sceneName}'");

        // แสดง loading screen (ถ้ามี)
        // loadingScreen.SetActive(true);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        float timer = 0f;
        float timeout = 10f;

        while (!asyncLoad.isDone && timer < timeout)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f) * 100f;
            Debug.Log($"⏳ กำลังโหลด... {progress:F1}%");

            if (asyncLoad.progress >= 0.9f)
            {
                Debug.Log("✅ โหลดเสร็จสิ้น, กำลังเปลี่ยน Scene...");
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        if (timer >= timeout)
        {
            Debug.LogError("⏰ โหลด Scene Timeout!");
            // ลองโหลด Scene เริ่มต้นแทน
            SceneManager.LoadScene(0);
        }
    }

    bool IsSceneInBuildSettings(string sceneName)
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;

        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (sceneNameInBuild.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"✅ พบ Scene '{sceneName}' ใน Build Settings (Index: {i})");
                return true;
            }
        }

        Debug.LogWarning($"⚠️ ไม่พบ Scene '{sceneName}' ใน Build Settings");
        return false;
    }

    void ListScenesInBuildSettings()
    {
        Debug.Log("=== Scenes ใน Build Settings ===");

        int sceneCount = SceneManager.sceneCountInBuildSettings;
        Debug.Log($"จำนวน Scene ทั้งหมด: {sceneCount}");

        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log($"[{i}] {sceneName}");
        }

        Debug.Log("=== จบรายการ Scene ===");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    // ==================== ฟังก์ชันสำหรับ Debug ====================

    [ContextMenu("Test Dialogue System")]
    void TestDialogueSystem()
    {
        currentNPCName = "TEST NPC";
        StartDialogue();
    }

    [ContextMenu("Test Show Question")]
    void TestShowQuestion()
    {
        ShowQuestion();
    }

    [ContextMenu("List All Scenes")]
    void TestListScenes()
    {
        ListScenesInBuildSettings();
    }

    [ContextMenu("Test Answer Button 1")]
    void TestAnswerButton1()
    {
        OnAnswerButton1Clicked();
    }
}

// NPC Info Class (แยกไว้ด้านล่าง)
public class NPCInfo : MonoBehaviour
{
    public string npcName = "NPC";

    void OnValidate()
    {
        if (string.IsNullOrEmpty(npcName))
        {
            npcName = gameObject.name;
        }
    }
}
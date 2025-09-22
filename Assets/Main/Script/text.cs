using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndDialogue : MonoBehaviour
{
    [System.Serializable]
    public class DialogueData
    {
        public string[] sentences;
    }

    [System.Serializable]
    public class QuestionData
    {
        public string questionText;
        public AnswerData[] answers;
    }

    [System.Serializable]
    public class AnswerData
    {
        public string answerText;
        public string targetScene;
    }

    [Header("UI References")]
    public GameObject dialoguePanel;
    public Text dialogueText;
    public float typingSpeed = 0.05f;

    public GameObject questionPanel;
    public GameObject answerButtonPrefab;
    public Text questionText;

    [Header("Dialogue Content")]
    public DialogueData dialogueContent;
    public QuestionData questionContent;

    [Header("Interaction Settings")]
    public float interactionRange = 2f;
    public KeyCode interactKey = KeyCode.E;

    private Queue<string> sentenceQueue;
    private bool isDialogueRunning = false;
    private bool isTypingInProgress = false;
    private string currentDialogueSentence;

    void Start()
    {
        sentenceQueue = new Queue<string>();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (questionPanel != null)
            questionPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(interactKey) && !isDialogueRunning)
        {
            FindAndInteractWithNPC();
        }

        if (isDialogueRunning && Input.GetKeyDown(KeyCode.Space))
        {
            if (isTypingInProgress)
            {
                FinishTypingImmediately();
            }
            else
            {
                ShowNextSentence();
            }
        }
    }

    void FindAndInteractWithNPC()
    {
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, interactionRange);

        foreach (Collider2D collider in nearbyObjects)
        {
            if (collider.CompareTag("NPC"))
            {
                InitiateDialogue();
                break;
            }
        }
    }

    public void InitiateDialogue()
    {
        isDialogueRunning = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        sentenceQueue.Clear();

        foreach (string sentence in dialogueContent.sentences)
        {
            sentenceQueue.Enqueue(sentence);
        }

        ShowNextSentence();
    }

    public void ShowNextSentence()
    {
        if (sentenceQueue.Count == 0)
        {
            ConcludeDialogue();
            return;
        }

        currentDialogueSentence = sentenceQueue.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeText(currentDialogueSentence));
    }

    IEnumerator TypeText(string textToType)
    {
        isTypingInProgress = true;
        dialogueText.text = "";

        foreach (char character in textToType.ToCharArray())
        {
            dialogueText.text += character;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTypingInProgress = false;
    }

    void FinishTypingImmediately()
    {
        StopAllCoroutines();
        dialogueText.text = currentDialogueSentence;
        isTypingInProgress = false;
    }

    void ConcludeDialogue()
    {
        isDialogueRunning = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        DisplayQuestion();
    }

    void DisplayQuestion()
    {
        if (questionPanel == null || questionText == null || answerButtonPrefab == null)
        {
            Debug.LogError("Question UI components are missing!");
            return;
        }

        questionPanel.SetActive(true);
        questionText.text = questionContent.questionText;

        // Remove any existing answer buttons
        foreach (Transform child in questionPanel.transform)
        {
            if (child != questionText.transform)
                Destroy(child.gameObject);
        }

        // Create new answer buttons
        foreach (AnswerData answer in questionContent.answers)
        {
            GameObject newButton = Instantiate(answerButtonPrefab, questionPanel.transform);
            newButton.GetComponentInChildren<Text>().text = answer.answerText;
            newButton.GetComponent<Button>().onClick.AddListener(() => HandleAnswerSelection(answer.targetScene));
        }
    }

    void HandleAnswerSelection(string sceneToLoad)
    {
        if (questionPanel != null)
            questionPanel.SetActive(false);

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    // Visualize interaction range in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
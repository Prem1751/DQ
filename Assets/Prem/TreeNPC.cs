using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class TreeNPC : MonoBehaviour
{
    [System.Serializable]
    public class DialogueNode
    {
        [TextArea(3, 5)] public string npcText;
        public List<DialogueOption> options = new List<DialogueOption>();
        public string nextSceneName;
    }

    [System.Serializable]
    public class DialogueOption
    {
        public string playerResponse;
        public DialogueNode nextNode;
    }

    // UI Settings
    public string npcName = "Myung-gil";
    public DialogueNode startNode;
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public GameObject[] optionButtons;
    public Image npcPortrait;

    private DialogueNode currentNode;
    private bool playerInRange = false;

    void Start()
    {
        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ToggleDialogue();
        }
    }

    public void ToggleDialogue()
    {
        if (!dialoguePanel.activeInHierarchy)
        {
            StartDialogue();
        }
        else
        {
            EndDialogue();
        }
    }

    public void StartDialogue()
    {
        if (startNode == null) return;

        dialoguePanel.SetActive(true);
        currentNode = startNode;
        UpdateDialogueUI();
    }

    void UpdateDialogueUI()
    {
        nameText.text = npcName;
        dialogueText.text = currentNode.npcText;

        // Hide all buttons first
        foreach (var button in optionButtons)
        {
            button.SetActive(false);
        }

        // Show available options
        for (int i = 0; i < currentNode.options.Count && i < optionButtons.Length; i++)
        {
            optionButtons[i].SetActive(true);
            optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentNode.options[i].playerResponse;

            int index = i;
            optionButtons[i].GetComponent<Button>().onClick.RemoveAllListeners();
            optionButtons[i].GetComponent<Button>().onClick.AddListener(() => SelectOption(index));
        }
    }

    public void SelectOption(int optionIndex)
    {
        if (optionIndex < 0 || optionIndex >= currentNode.options.Count) return;

        if (currentNode.options[optionIndex].nextNode != null)
        {
            currentNode = currentNode.options[optionIndex].nextNode;
            UpdateDialogueUI();
        }
        else if (!string.IsNullOrEmpty(currentNode.nextSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentNode.nextSceneName);
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            EndDialogue();
        }
    }
}
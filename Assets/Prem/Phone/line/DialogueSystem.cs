using UnityEngine;
using System.Collections.Generic;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance;

    [System.Serializable]
    public class DialogueNode
    {
        public string npcText;
        public List<string> playerChoices;
        public List<int> nextNodes;
    }

    public List<DialogueNode> dialogueTree;
    private int currentNode = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartDialogue(int startNode = 0)
    {
        currentNode = startNode;
        ProcessCurrentNode();
    }

    private void ProcessCurrentNode()
    {
        DialogueNode node = dialogueTree[currentNode];
        ChatManager.Instance.SendNPCMessage(node.npcText);

        if (node.playerChoices.Count > 0)
        {
            ChatManager.Instance.ShowDialogueChoices(node.playerChoices);
        }
        else if (node.nextNodes.Count > 0)
        {
            currentNode = node.nextNodes[0];
            ProcessCurrentNode();
        }
    }

    public void ProcessPlayerChoice(string choice)
    {
        DialogueNode node = dialogueTree[currentNode];
        int choiceIndex = node.playerChoices.IndexOf(choice);

        if (choiceIndex >= 0 && choiceIndex < node.nextNodes.Count)
        {
            currentNode = node.nextNodes[choiceIndex];
            ProcessCurrentNode();
        }
    }

    public void TriggerNPCMessage(string message)
    {
        ChatManager.Instance.SendNPCMessage(message);
    }
}
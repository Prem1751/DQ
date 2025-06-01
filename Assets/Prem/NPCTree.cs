using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class DialogueOption
{
    [TextArea(2, 3)] public string text;
    public int scoreValue;
    public DialogueNode nextNode;
}

[System.Serializable]
public class DialogueNode
{
    [TextArea(3, 5)] public string npcText;
    public Sprite characterSprite;
    public List<DialogueOption> options = new List<DialogueOption>();
}

public class NPCTree : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public Image characterImage;
    public TMP_Text npcNameText;
    public TMP_Text dialogueText;
    public Button optionButton1; // �����ӵͺ��� 1
    public Button optionButton2; // �����ӵͺ��� 2
    public Button optionButton3; // �����ӵͺ��� 3 (�����繵�ͧ�������� 2 ������͡)

    [Header("Settings")]
    public string npcName = "Myung-gil";
    public float interactDistance = 2f;
    public DialogueNode startNode;

    private DialogueNode currentNode;
    private bool isDialogueActive = false;
    private Transform player;

    void Start()
    {
        dialoguePanel.SetActive(false);
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // ��͹�����������������
        optionButton1.gameObject.SetActive(false);
        optionButton2.gameObject.SetActive(false);
        optionButton3.gameObject.SetActive(false);
    }

    void Update()
    {
        // ��Ǩ�ͺ��á����� E
        if (Input.GetKeyDown(KeyCode.E))
        {
            // �ӹǳ������ҧ�ҡ������
            float distance = Vector3.Distance(transform.position, player.position);

            // ��Ǩ�ͺ��������������ͺ
            if (distance <= interactDistance)
            {
                if (!isDialogueActive)
                {
                    StartDialogue();
                }
                else
                {
                    EndDialogue();
                }
            }
        }
    }

    public void StartDialogue()
    {
        isDialogueActive = true;
        dialoguePanel.SetActive(true);
        npcNameText.text = npcName;
        SetCurrentNode(startNode);
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
    }

    private void SetCurrentNode(DialogueNode node)
    {
        currentNode = node;
        dialogueText.text = node.npcText;

        if (node.characterSprite != null)
        {
            characterImage.sprite = node.characterSprite;
        }

        // ��͹������������͹
        optionButton1.gameObject.SetActive(false);
        optionButton2.gameObject.SetActive(false);
        optionButton3.gameObject.SetActive(false);

        // ��駤�һ�������ӹǹ������͡�����
        for (int i = 0; i < node.options.Count; i++)
        {
            if (i == 0)
            {
                SetupButton(optionButton1, node.options[i]);
            }
            else if (i == 1)
            {
                SetupButton(optionButton2, node.options[i]);
            }
            else if (i == 2)
            {
                SetupButton(optionButton3, node.options[i]);
            }
        }
    }

    private void SetupButton(Button button, DialogueOption option)
    {
        button.gameObject.SetActive(true);
        button.GetComponentInChildren<TMP_Text>().text = option.text;

        // ��ҧ listener ��͹�����������ͻ�ͧ�ѹ������¡���
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => {
            GameManager.Instance.AddScore(option.scoreValue);

            if (option.nextNode != null)
            {
                SetCurrentNode(option.nextNode);
            }
            else
            {
                EndDialogue();
            }
        });
    }
}
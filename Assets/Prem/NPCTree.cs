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
    public Button optionButton1; // ปุ่มคำตอบที่ 1
    public Button optionButton2; // ปุ่มคำตอบที่ 2
    public Button optionButton3; // ปุ่มคำตอบที่ 3 (ไม่จำเป็นต้องใช้ถ้ามีแค่ 2 ตัวเลือก)

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

        // ซ่อนปุ่มทั้งหมดเริ่มต้น
        optionButton1.gameObject.SetActive(false);
        optionButton2.gameObject.SetActive(false);
        optionButton3.gameObject.SetActive(false);
    }

    void Update()
    {
        // ตรวจสอบการกดปุ่ม E
        if (Input.GetKeyDown(KeyCode.E))
        {
            // คำนวณระยะห่างจากผู้เล่น
            float distance = Vector3.Distance(transform.position, player.position);

            // ตรวจสอบว่าอยู่ในระยะโต้ตอบ
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

        // ซ่อนปุ่มทั้งหมดก่อน
        optionButton1.gameObject.SetActive(false);
        optionButton2.gameObject.SetActive(false);
        optionButton3.gameObject.SetActive(false);

        // ตั้งค่าปุ่มตามจำนวนตัวเลือกที่มี
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

        // ล้าง listener ก่อนเพิ่มใหม่เพื่อป้องกันการเรียกซ้ำ
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
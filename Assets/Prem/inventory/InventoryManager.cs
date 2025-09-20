using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    public List<Item> items = new List<Item>();

    [Header("UI References")]
    public GameObject inventoryPanel;
    public Transform itemsParent; // Content ของ ScrollView
    public GameObject itemSlotPrefab; // Prefab สำหรับแต่ละช่องไอเท็ม

    [Header("Item Display")]
    public Image itemDisplay;
    public TMP_Text itemNameText;
    public TMP_Text itemDescText;

    private bool isInventoryOpen = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // เปิด Inventory ทันทีเมื่อเริ่มเกม
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(true);
                isInventoryOpen = true;
            }
            else
            {
                Debug.LogError("ไม่ได้กำหนด Inventory Panel!");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);
    }

    public void AddItem(Item newItem)
    {
        items.Add(newItem);
        Debug.Log($"เพิ่มไอเท็ม: {newItem.itemName} (Total: {items.Count})");

        // อัพเดท UI ทันทีเมื่อเพิ่มไอเท็ม
        UpdateInventoryUI();

        // แสดงไอเท็มล่าสุดทันที
        DisplayItemDetails(newItem);
    }

    void UpdateInventoryUI()
    {
        if (itemsParent == null)
        {
            Debug.LogError("ItemsParent ไม่ได้ถูกกำหนด!");
            return;
        }

        // ลบช่องเก่าทั้งหมด
        foreach (Transform child in itemsParent)
        {
            Destroy(child.gameObject);
        }

        // สร้างช่องใหม่ตามจำนวนไอเท็ม
        for (int i = 0; i < items.Count; i++)
        {
            // สร้างช่องใหม่
            GameObject newSlot = Instantiate(itemSlotPrefab, itemsParent);
            newSlot.name = "ItemSlot_" + i;

            // อัพเดท UI ของช่อง
            UpdateSlotUI(newSlot, items[i]);
        }
    }

    void UpdateSlotUI(GameObject slot, Item item)
    {
        // หา component ต่างๆ ใน slot
        Transform iconTransform = slot.transform.Find("Icon");
        Transform nameTextTransform = slot.transform.Find("NameText");
        Transform descTextTransform = slot.transform.Find("DescText");

        if (iconTransform != null)
        {
            Image icon = iconTransform.GetComponent<Image>();
            if (icon != null) icon.sprite = item.icon;
        }

        if (nameTextTransform != null)
        {
            TMP_Text nameText = nameTextTransform.GetComponent<TMP_Text>();
            if (nameText != null) nameText.text = item.itemName;
        }

        if (descTextTransform != null)
        {
            TMP_Text descText = descTextTransform.GetComponent<TMP_Text>();
            if (descText != null) descText.text = item.description;
        }
    }

    void DisplayItemDetails(Item item)
    {
        // แสดงรายละเอียดไอเท็มที่เพิ่งเก็บได้ทันที
        if (itemDisplay != null) itemDisplay.sprite = item.icon;
        if (itemNameText != null) itemNameText.text = item.itemName;
        if (itemDescText != null) itemDescText.text = item.description;

        Debug.Log($"แสดงไอเท็ม: {item.itemName}");
    }

    public void RemoveItem(Item itemToRemove)
    {
        if (items.Contains(itemToRemove))
        {
            items.Remove(itemToRemove);
            UpdateInventoryUI();
            Debug.Log($"ลบไอเท็ม: {itemToRemove.itemName}");
        }
    }
}
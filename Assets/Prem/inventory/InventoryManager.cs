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
    public Image itemDisplay;
    public TMP_Text itemNameText;
    public TMP_Text itemDescText;

    private bool isInventoryOpen = false;
    private int selectedIndex = -1;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
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
        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            UpdateInventoryUI();
            if (items.Count > 0 && selectedIndex == -1)
            {
                SelectItem(0);
            }
        }
    }

    public void AddItem(Item newItem)
    {
        items.Add(newItem);
        Debug.Log($"เพิ่มไอเท็ม: {newItem.itemName} (Total: {items.Count})");

        if (isInventoryOpen)
        {
            UpdateInventoryUI();

            // เลือกไอเท็มที่เพิ่งเพิ่มเข้ามา
            if (items.Count > 0)
            {
                SelectItem(items.Count - 1);
            }
        }
    }

    void UpdateInventoryUI()
    {
        if (itemsParent == null)
        {
            Debug.LogError("ItemsParent ไม่ได้ถูกกำหนด!");
            return;
        }

        // ตรวจสอบและสร้างช่องเพิ่มถ้าจำเป็น
        while (itemsParent.childCount < items.Count)
        {
            GameObject newSlot = Instantiate(itemsParent.GetChild(0).gameObject, itemsParent);
            newSlot.name = "ItemSlot_" + itemsParent.childCount;
        }

        // อัพเดททุกช่อง
        for (int i = 0; i < itemsParent.childCount; i++)
        {
            Transform slot = itemsParent.GetChild(i);
            if (slot.childCount > 0)
            {
                Image icon = slot.GetChild(0).GetComponent<Image>();

                if (i < items.Count)
                {
                    // แสดงไอเท็มที่มี
                    icon.sprite = items[i].icon;
                    icon.gameObject.SetActive(true);

                    // ตั้งค่าปุ่มคลิก
                    Button btn = slot.GetComponent<Button>();
                    int itemIndex = i;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => SelectItem(itemIndex));
                }
                else
                {
                    // ปิดช่องที่ไม่มีไอเท็ม
                    icon.gameObject.SetActive(false);
                }
            }
        }

        // อัพเดทการแสดงผลไอเท็มที่เลือก
        if (selectedIndex >= 0 && selectedIndex < items.Count)
        {
            UpdateSelectedItemDisplay();
        }
        else if (items.Count > 0)
        {
            SelectItem(0);
        }
    }

    void SelectItem(int index)
    {
        if (index < 0 || index >= items.Count) return;

        selectedIndex = index;
        UpdateSelectedItemDisplay();
    }

    void UpdateSelectedItemDisplay()
    {
        Item selected = items[selectedIndex];
        if (itemDisplay != null) itemDisplay.sprite = selected.icon;
        if (itemNameText != null) itemNameText.text = selected.itemName;
        if (itemDescText != null) itemDescText.text = selected.description;

        Debug.Log($"เลือกไอเท็ม: {selected.itemName} (Index: {selectedIndex})");
    }

    public void RemoveItem(int index)
    {
        if (index < 0 || index >= items.Count) return;

        items.RemoveAt(index);

        // ปรับ selectedIndex ถ้าจำเป็น
        if (selectedIndex == index)
        {
            selectedIndex = -1;
        }
        else if (selectedIndex > index)
        {
            selectedIndex--;
        }

        UpdateInventoryUI();
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhoneUI : MonoBehaviour
{
    [Header("UI Parts")]
    public GameObject phonePanel;
    public Transform personsContainer; // Container สำหรับบุคคล
    public GameObject personPrefab;
    public Button openButton;
    public Button closeButton;

    [Header("Layout Settings")]
    public float spacing = 30f;
    public Vector2 itemSize = new Vector2(200f, 250f);

    [Header("Person List")]
    public List<PersonData> allPersons = new List<PersonData>();

    private List<PersonUIItem> uiItems = new List<PersonUIItem>();
    private PhoneSystem phoneSystem;

    void Start()
    {
        // ตรวจสอบการอ้างอิงที่สำคัญ
        if (personsContainer == null)
        {
            Debug.LogError("Persons Container is not assigned!");
            // พยายามหาอัตโนมัติ
            personsContainer = transform.Find("PersonsContainer");
            if (personsContainer == null)
            {
                GameObject container = new GameObject("PersonsContainer");
                container.transform.SetParent(transform);
                personsContainer = container.transform;

                // เพิ่ม RectTransform
                RectTransform rt = container.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(400f, 500f);
                rt.anchoredPosition = Vector2.zero;
            }
        }

        if (personPrefab == null)
        {
            Debug.LogError("Person Prefab is not assigned!");
            return;
        }

        // ค้นหา PhoneSystem ใน scene
#if UNITY_2023_1_OR_NEWER
        phoneSystem = FindFirstObjectByType<PhoneSystem>();
#else
        phoneSystem = FindObjectOfType<PhoneSystem>();
#endif

        // ตั้งค่าปุ่ม
        if (openButton != null)
            openButton.onClick.AddListener(OpenPhone);
        else
            Debug.LogWarning("Open Button is not assigned!");

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePhone);
        else
            Debug.LogWarning("Close Button is not assigned!");

        if (phonePanel != null)
            phonePanel.SetActive(false);
        else
            Debug.LogError("Phone Panel is not assigned!");

        LoadData();
        CreateUI();
    }

    private void OpenPhone()
    {
        if (phoneSystem != null)
        {
            phoneSystem.TogglePhone();
        }
        else if (phonePanel != null)
        {
            phonePanel.SetActive(true);
            RefreshUI(); // รีเฟรช UI เมื่อเปิด
        }
    }

    private void ClosePhone()
    {
        if (phoneSystem != null)
        {
            phoneSystem.TogglePhone();
        }
        else if (phonePanel != null)
        {
            phonePanel.SetActive(false);
        }
    }

    public void CreateUI()
    {
        // ลบของเก่า
        foreach (PersonUIItem item in uiItems)
        {
            if (item != null && item.gameObject != null)
                Destroy(item.gameObject);
        }
        uiItems.Clear();

        // ตรวจสอบ personsContainer
        if (personsContainer == null)
        {
            Debug.LogError("PersonsContainer is null!");
            return;
        }

        // ตรวจสอบว่ามีข้อมูลใน allPersons หรือไม่
        if (allPersons == null || allPersons.Count == 0)
        {
            Debug.LogWarning("No persons in allPersons list!");
            return;
        }

        // คำนวณตำแหน่งเริ่มต้น (จากบนลงล่าง)
        float startY = 0f;
        float totalHeight = (itemSize.y + spacing) * allPersons.Count;
        startY = totalHeight / 2f - itemSize.y / 2f;

        // สร้าง UI ใหม่
        for (int i = 0; i < allPersons.Count; i++)
        {
            PersonData person = allPersons[i];

            if (person == null) continue;

            GameObject newUI = Instantiate(personPrefab, personsContainer);
            newUI.name = $"PersonUI_{person.personName}";

            // ตั้งค่า RectTransform
            RectTransform rectTransform = newUI.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.sizeDelta = itemSize;
                rectTransform.anchoredPosition = new Vector2(0f, startY - i * (itemSize.y + spacing));
                rectTransform.localScale = Vector3.one;
            }

            PersonUIItem uiItem = newUI.GetComponent<PersonUIItem>();
            if (uiItem != null)
            {
                uiItem.Setup(person, this);
                uiItems.Add(uiItem);
                newUI.SetActive(true);
            }
            else
            {
                Debug.LogError("Person Prefab doesn't have PersonUIItem component!");
                Destroy(newUI);
            }
        }

        Debug.Log($"Created {uiItems.Count} person UI items in container: {personsContainer.name}");
    }

    [System.Serializable]
    public class PhoneSaveData
    {
        public List<PersonSaveData> persons = new List<PersonSaveData>();
    }

    [System.Serializable]
    public class PersonSaveData
    {
        public string name;
        public int state;
    }

    public void SaveData()
    {
        PhoneSaveData save = new PhoneSaveData();
        foreach (PersonData person in allPersons)
        {
            if (person != null)
            {
                PersonSaveData item = new PersonSaveData();
                item.name = person.personName;
                item.state = (int)person.currentState;
                save.persons.Add(item);
            }
        }

        string json = JsonUtility.ToJson(save);
        PlayerPrefs.SetString("PhoneData", json);
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        if (PlayerPrefs.HasKey("PhoneData"))
        {
            string json = PlayerPrefs.GetString("PhoneData");
            PhoneSaveData save = JsonUtility.FromJson<PhoneSaveData>(json);

            foreach (PersonSaveData saveItem in save.persons)
            {
                PersonData person = allPersons.Find(p => p != null && p.personName == saveItem.name);
                if (person != null)
                {
                    person.currentState = (SuspicionState)saveItem.state;
                }
            }
        }
    }

    public void AddPerson(PersonData newPerson)
    {
        if (newPerson != null && !allPersons.Contains(newPerson))
        {
            allPersons.Add(newPerson);
            CreateUI();
            SaveData();
        }
    }

    public void RemovePerson(PersonData personToRemove)
    {
        if (allPersons.Contains(personToRemove))
        {
            allPersons.Remove(personToRemove);
            CreateUI();
            SaveData();
        }
    }

    // เมธอดสำหรับรีเฟรช UI
    public void RefreshUI()
    {
        CreateUI();
    }

    // เรียกเมื่อเปิดโทรศัพท์
    public void OnPhoneOpened()
    {
        RefreshUI();
    }
}
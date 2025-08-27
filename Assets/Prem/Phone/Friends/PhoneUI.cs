using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhoneUI : MonoBehaviour
{
    [Header("UI Parts")]
    public GameObject phonePanel;
    public Transform personsContainer; // Content ใน ScrollView
    public GameObject personPrefab;
    public Button openButton;
    public Button closeButton;

    [Header("Person List")]
    public List<PersonData> allPersons = new List<PersonData>();

    private List<PersonUIItem> uiItems = new List<PersonUIItem>();

    void Start()
    {
        // ตั้งปุ่ม
        openButton.onClick.AddListener(() => phonePanel.SetActive(true));
        closeButton.onClick.AddListener(() => phonePanel.SetActive(false));

        phonePanel.SetActive(false);

        LoadData();
        CreateUI();
    }

    private void CreateUI()
    {
        // ลบของเก่า
        foreach (PersonUIItem item in uiItems)
        {
            if (item != null) DestroyImmediate(item.gameObject);
        }
        uiItems.Clear();

        // สร้างใหม่
        foreach (PersonData person in allPersons)
        {
            if (person.personImage != null)
            {
                GameObject newUI = Instantiate(personPrefab, personsContainer);
                PersonUIItem uiItem = newUI.GetComponent<PersonUIItem>();
                uiItem.Setup(person, this);
                uiItems.Add(uiItem);
            }
        }
    }

    public void SaveData()
    {
        SaveSystem save = new SaveSystem();
        foreach (PersonData person in allPersons)
        {
            PersonSave item = new PersonSave();
            item.name = person.personName;
            item.state = (int)person.currentState;
            save.persons.Add(item);
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
            SaveSystem save = JsonUtility.FromJson<SaveSystem>(json);

            foreach (PersonSave saveItem in save.persons)
            {
                PersonData person = allPersons.Find(p => p.personName == saveItem.name);
                if (person != null)
                {
                    person.currentState = (SuspicionState)saveItem.state;
                }
            }
        }
    }

    // เพิ่มคนใหม่
    public void AddPerson(PersonData newPerson)
    {
        allPersons.Add(newPerson);
        CreateUI();
    }
}

[System.Serializable]
public class SaveSystem
{
    public List<PersonSave> persons = new List<PersonSave>();
}

[System.Serializable]
public class PersonSave
{
    public string name;
    public int state;
}
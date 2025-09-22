using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

[System.Serializable]
public class GameSaveData
{
    // Player data
    public string currentScene;
    public Vector3 playerPosition;
    public Quaternion playerRotation;

    // GameManager data
    public int playerScore;

    // PhoneUI data
    public PhoneUI.PhoneSaveData phoneData;

    // Inventory data
    public List<InventorySaveData> inventoryItems;

    // Settings data
    public float masterVolume;
    public bool fullscreen;
    public int qualityLevel;

    public string saveDateTime;
}

[System.Serializable]
public class InventorySaveData
{
    public string itemName;
    public string itemDescription;
}

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    private string savePath;
    private GameSaveData currentSaveData;

    [Header("UI References")]
    public GameObject saveButton;
    public GameObject loadButton;
    public GameObject saveNotification;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "savedata.json");

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // ตั้งค่าปุ่ม UI
        if (saveButton != null)
        {
            Button saveBtn = saveButton.GetComponent<Button>();
            if (saveBtn != null) saveBtn.onClick.AddListener(SaveGame);
        }

        if (loadButton != null)
        {
            Button loadBtn = loadButton.GetComponent<Button>();
            if (loadBtn != null) loadBtn.onClick.AddListener(LoadGame);
        }

        // ซ่อน notification เริ่มต้น
        if (saveNotification != null)
            saveNotification.SetActive(false);
    }

    public void SaveGame()
    {
        currentSaveData = new GameSaveData();

        // บันทึกข้อมูลซีนปัจจุบัน
        currentSaveData.currentScene = SceneManager.GetActiveScene().name;

        // บันทึกตำแหน่งผู้เล่น
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            currentSaveData.playerPosition = player.transform.position;
            currentSaveData.playerRotation = player.transform.rotation;
        }

        // บันทึกข้อมูลจาก GameManager
        if (GameManager.Instance != null)
        {
            currentSaveData.playerScore = GameManager.Instance.GetScore();
        }

        // บันทึกข้อมูลจาก PhoneUI
        PhoneUI phoneUI = FindObjectOfType<PhoneUI>();
        if (phoneUI != null)
        {
            phoneUI.SaveData();
        }

        // บันทึกข้อมูลจาก Inventory
        if (InventoryManager.instance != null)
        {
            currentSaveData.inventoryItems = new List<InventorySaveData>();
            foreach (Item item in InventoryManager.instance.items)
            {
                InventorySaveData itemData = new InventorySaveData
                {
                    itemName = item.itemName,
                    itemDescription = item.description
                };
                currentSaveData.inventoryItems.Add(itemData);
            }
        }

        // บันทึกการตั้งค่า
        currentSaveData.masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        currentSaveData.fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        currentSaveData.qualityLevel = PlayerPrefs.GetInt("QualityLevel", 2);

        // บันทึกเวลาที่เซฟ
        currentSaveData.saveDateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // แปลงเป็น JSON และบันทึกลงไฟล์
        string json = JsonUtility.ToJson(currentSaveData, true);
        File.WriteAllText(savePath, json);

        // แสดงการยืนยันการบันทึก
        ShowSaveNotification("เกมถูกบันทึกแล้ว!");

        Debug.Log("เกมถูกบันทึกที่: " + savePath);
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("ไม่พบไฟล์บันทึกเกม!");
            ShowSaveNotification("ไม่พบไฟล์บันทึกเกม!");
            return;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            currentSaveData = JsonUtility.FromJson<GameSaveData>(json);

            // โหลดซีน
            if (!string.IsNullOrEmpty(currentSaveData.currentScene))
            {
                SceneManager.LoadScene(currentSaveData.currentScene);
            }

            ShowSaveNotification("โหลดเกมสำเร็จ!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("เกิดข้อผิดพลาดในการโหลดเกม: " + e.Message);
            ShowSaveNotification("เกิดข้อผิดพลาดในการโหลดเกม!");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (currentSaveData != null && scene.name == currentSaveData.currentScene)
        {
            StartCoroutine(SetPlayerPositionAfterLoad());
        }
    }

    private System.Collections.IEnumerator SetPlayerPositionAfterLoad()
    {
        yield return new WaitForSeconds(0.1f);

        // ตั้งค่าตำแหน่งผู้เล่น
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && currentSaveData != null)
        {
            player.transform.position = currentSaveData.playerPosition;
            player.transform.rotation = currentSaveData.playerRotation;
        }

        // โหลดข้อมูลอื่นๆ
        LoadAdditionalData();
    }

    private void LoadAdditionalData()
    {
        if (currentSaveData == null) return;

        // โหลดคะแนน
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetScore();
            GameManager.Instance.AddScore(currentSaveData.playerScore);
        }

        // โหลดการตั้งค่า
        PlayerPrefs.SetFloat("MasterVolume", currentSaveData.masterVolume);
        PlayerPrefs.SetInt("Fullscreen", currentSaveData.fullscreen ? 1 : 0);
        PlayerPrefs.SetInt("QualityLevel", currentSaveData.qualityLevel);
        PlayerPrefs.Save();

        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.LoadSettings();
        }

        // โหลดอินเวนทอรี่
        LoadInventory();

        Debug.Log("โหลดข้อมูลเกมเสร็จสมบูรณ์");
    }

    private void LoadInventory()
    {
        if (InventoryManager.instance != null && currentSaveData.inventoryItems != null)
        {
            InventoryManager.instance.items.Clear();

            foreach (InventorySaveData itemData in currentSaveData.inventoryItems)
            {
                Item newItem = ScriptableObject.CreateInstance<Item>();
                newItem.itemName = itemData.itemName;
                newItem.description = itemData.itemDescription;

                InventoryManager.instance.AddItem(newItem);
            }

            InventoryManager.instance.UpdateInventoryUI();
        }
    }

    private void ShowSaveNotification(string message)
    {
        if (saveNotification != null)
        {
            Text notificationText = saveNotification.GetComponentInChildren<Text>();
            if (notificationText != null)
                notificationText.text = message;
                notificationText.text = message;

            saveNotification.SetActive(true);
            Invoke("HideSaveNotification", 3f);
        }
    }

    private void HideSaveNotification()
    {
        if (saveNotification != null)
            saveNotification.SetActive(false);
    }

    public bool SaveExists()
    {
        return File.Exists(savePath);
    }

    public string GetSaveInfo()
    {
        if (SaveExists())
        {
            string json = File.ReadAllText(savePath);
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
            return $"ซีน: {data.currentScene} | เวลา: {data.saveDateTime}";
        }
        return "ไม่มีข้อมูลบันทึกเกม";
    }
}
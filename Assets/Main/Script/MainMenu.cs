using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    public Button newGameButton;
    public Button loadGameButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Save Info Display")]
    public Text saveInfoText;

    void Start()
    {
        if (newGameButton != null)
            newGameButton.onClick.AddListener(StartNewGame);

        if (loadGameButton != null)
            loadGameButton.onClick.AddListener(LoadSavedGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        UpdateSaveInfo();
    }

    void UpdateSaveInfo()
    {
        if (saveInfoText != null && SaveSystem.Instance != null)
        {
            saveInfoText.text = SaveSystem.Instance.GetSaveInfo();

            if (loadGameButton != null)
                loadGameButton.interactable = SaveSystem.Instance.SaveExists();
        }
    }

    public void StartNewGame()
    {
        if (SaveSystem.Instance != null && SaveSystem.Instance.SaveExists())
        {
            Debug.Log("เริ่มเกมใหม่ - ลบข้อมูลเก่า");
        }

        SceneManager.LoadScene("Scene1");
    }

    public void LoadSavedGame()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.LoadGame();
        }
    }

    public void OpenSettings()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OpenSettings();
        }
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
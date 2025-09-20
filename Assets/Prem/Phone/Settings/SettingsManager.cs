using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Audio Settings")]
    public Slider masterVolumeSlider;
    public TextMeshProUGUI masterVolumeText;

    [Header("Graphics Settings")]
    public Toggle fullscreenToggle;
    public Dropdown resolutionDropdown;
    public Dropdown qualityDropdown;

    [Header("UI References")]
    public GameObject settingsPanel;
    public Button settingsButton;
    public Button closeSettingsButton;
    public Button applyButton;

    // Current settings values
    private float currentMasterVolume = 1f;
    private bool currentFullscreen = true;
    private int currentQualityLevel = 2;

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

    void Start()
    {
        InitializeSettings();
        SetupEventListeners();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void InitializeSettings()
    {
        // โหลดการตั้งค่าที่บันทึกไว้
        LoadSettings();

        // ตั้งค่า UI ตามค่าปัจจุบัน
        UpdateUIValues();

        // ตั้งค่า resolution dropdown
        SetupResolutionDropdown();

        // ตั้งค่า quality dropdown
        SetupQualityDropdown();
    }

    private void SetupEventListeners()
    {
        // ปุ่มเปิด-ปิด settings
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(CloseSettings);

        if (applyButton != null)
            applyButton.onClick.AddListener(ApplySettings);

        // Audio slider
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);

        // Graphics
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(SetResolution);

        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(SetQuality);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            // โหลดการตั้งค่าปัจจุบันเมื่อเปิด
            LoadSettings();
            UpdateUIValues();
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            // ยกเลิกการเปลี่ยนแปลงที่ยังไม่ apply
            RevertUnsavedChanges();
        }
    }

    public void ApplySettings()
    {
        // บันทึกการตั้งค่าทั้งหมด
        SaveSettings();

        // ปิด settings panel
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Debug.Log("Settings applied and saved!");
    }

    private void UpdateUIValues()
    {
        // Audio
        if (masterVolumeSlider != null) masterVolumeSlider.value = currentMasterVolume;
        if (masterVolumeText != null) masterVolumeText.text = Mathf.RoundToInt(currentMasterVolume * 100) + "%";

        // Graphics
        if (fullscreenToggle != null) fullscreenToggle.isOn = currentFullscreen;
        if (qualityDropdown != null) qualityDropdown.value = currentQualityLevel;
    }

    private void RevertUnsavedChanges()
    {
        // ยกเลิกการเปลี่ยนแปลงที่ยังไม่ apply
        // โดยโหลดค่าจากตัวแปรปัจจุบัน
        UpdateUIValues();
    }

    // Audio Settings Methods
    public void SetMasterVolume(float volume)
    {
        currentMasterVolume = volume;
        AudioListener.volume = volume;
        if (masterVolumeText != null)
            masterVolumeText.text = Mathf.RoundToInt(volume * 100) + "%";
    }

    // Graphics Settings Methods
    public void SetFullscreen(bool isFullscreen)
    {
        currentFullscreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        // ตั้งค่าความละเอียดจอ
        Resolution[] resolutions = Screen.resolutions;
        if (resolutionIndex >= 0 && resolutionIndex < resolutions.Length)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, currentFullscreen);
        }
    }

    public void SetQuality(int qualityIndex)
    {
        currentQualityLevel = qualityIndex;
    }

    private void SetupResolutionDropdown()
    {
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();

            List<string> options = new List<string>();
            int currentResolutionIndex = 0;

            for (int i = 0; i < Screen.resolutions.Length; i++)
            {
                Resolution resolution = Screen.resolutions[i];
                string option = $"{resolution.width} x {resolution.height}";
                options.Add(option);

                if (resolution.width == Screen.currentResolution.width &&
                    resolution.height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }
    }

    private void SetupQualityDropdown()
    {
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();

            List<string> options = new List<string>();
            foreach (string qualityName in QualitySettings.names)
            {
                options.Add(qualityName);
            }

            qualityDropdown.AddOptions(options);
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.RefreshShownValue();
        }
    }

    // Save/Load Settings
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", currentMasterVolume);
        PlayerPrefs.SetInt("Fullscreen", currentFullscreen ? 1 : 0);
        PlayerPrefs.SetInt("QualityLevel", currentQualityLevel);

        PlayerPrefs.Save();
        Debug.Log("Settings saved!");
    }

    public void LoadSettings()
    {
        currentMasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        currentFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        currentQualityLevel = PlayerPrefs.GetInt("QualityLevel", 2);

        // Apply loaded settings
        AudioListener.volume = currentMasterVolume;
        Screen.fullScreen = currentFullscreen;
        QualitySettings.SetQualityLevel(currentQualityLevel);

        Debug.Log("Settings loaded!");
    }

    // Reset to default settings
    public void ResetToDefault()
    {
        currentMasterVolume = 1f;
        currentFullscreen = true;
        currentQualityLevel = 2;

        UpdateUIValues();
        ApplySettings();
        Debug.Log("Settings reset to default!");
    }
}
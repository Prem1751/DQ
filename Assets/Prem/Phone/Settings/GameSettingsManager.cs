using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class GameSettingsManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioMixer audioMixer;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle muteToggle;

    [Header("Graphics Settings")]
    public Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Dropdown qualityDropdown;
    public Slider brightnessSlider;
    public Image brightnessOverlay;
    public Toggle vSyncToggle;
    public Slider frameRateSlider;
    public Text frameRateText;

    [Header("Gameplay Settings")]
    public Slider mouseSensitivitySlider;
    public Toggle invertYToggle;
    public Dropdown languageDropdown;
    public Toggle showTutorialToggle;

    [Header("Controls Settings")]
    public Text keyboardBindText;
    public Button keyboardBindButton;
    public Text gamepadBindText;
    public Button gamepadBindButton;

    private Resolution[] resolutions;
    private float currentBrightness = 1.0f;
    private bool isRebinding = false;
    private KeyCode newKeyCode;

    void Start()
    {
        InitializeSettings();
        LoadSettings();
    }

    void InitializeSettings()
    {
        // Audio Settings
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        if (muteToggle != null)
            muteToggle.onValueChanged.AddListener(ToggleMute);

        // Graphics Settings
        SetupResolutionDropdown();
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(QualitySettings.names.ToList());
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }
        if (brightnessSlider != null)
            brightnessSlider.onValueChanged.AddListener(SetBrightness);
        if (vSyncToggle != null)
            vSyncToggle.onValueChanged.AddListener(SetVSync);
        if (frameRateSlider != null)
            frameRateSlider.onValueChanged.AddListener(SetFrameRateLimit);

        // Gameplay Settings
        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
        if (invertYToggle != null)
            invertYToggle.onValueChanged.AddListener(SetInvertY);
        if (languageDropdown != null)
            languageDropdown.onValueChanged.AddListener(SetLanguage);
        if (showTutorialToggle != null)
            showTutorialToggle.onValueChanged.AddListener(SetShowTutorial);

        // Controls Settings
        if (keyboardBindButton != null)
            keyboardBindButton.onClick.AddListener(StartKeyboardRebinding);
        if (gamepadBindButton != null)
            gamepadBindButton.onClick.AddListener(StartGamepadRebinding);
    }

    #region Audio Settings
    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void ToggleMute(bool isMuted)
    {
        AudioListener.volume = isMuted ? 0 : 1;
        PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);
    }
    #endregion

    #region Graphics Settings
    private void SetupResolutionDropdown()
    {
        resolutions = Screen.resolutions
            .Where(res => res.refreshRate == 60) // ใช้เฉพาะความถี่ 60Hz
            .Distinct(new ResolutionComparer())
            .ToArray();

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width} x {resolutions[i].height}";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionWidth", resolution.width);
        PlayerPrefs.SetInt("ResolutionHeight", resolution.height);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("IsFullscreen", isFullscreen ? 1 : 0);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualityLevel", qualityIndex);
    }

    public void SetBrightness(float brightness)
    {
        currentBrightness = brightness;
        brightnessOverlay.color = new Color(0, 0, 0, 1 - brightness);
        PlayerPrefs.SetFloat("Brightness", brightness);
    }

    public void SetVSync(bool useVSync)
    {
        QualitySettings.vSyncCount = useVSync ? 1 : 0;
        PlayerPrefs.SetInt("VSync", useVSync ? 1 : 0);
    }

    public void SetFrameRateLimit(float frameRate)
    {
        int fps = Mathf.RoundToInt(frameRate);
        Application.targetFrameRate = fps;
        frameRateText.text = $"FPS Limit: {fps}";
        PlayerPrefs.SetInt("FrameRateLimit", fps);
    }
    #endregion

    #region Gameplay Settings
    public void SetMouseSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
        // ตัวอย่างการใช้งาน: CameraController.sensitivity = sensitivity;
    }

    public void SetInvertY(bool invertY)
    {
        PlayerPrefs.SetInt("InvertY", invertY ? 1 : 0);
        // ตัวอย่างการใช้งาน: CameraController.invertY = invertY;
    }

    public void SetLanguage(int languageIndex)
    {
        PlayerPrefs.SetInt("Language", languageIndex);
        // ต้องมีระบบเปลี่ยนภาษาของคุณเอง
    }

    public void SetShowTutorial(bool showTutorial)
    {
        PlayerPrefs.SetInt("ShowTutorial", showTutorial ? 1 : 0);
    }
    #endregion

    #region Controls Settings
    public void StartKeyboardRebinding()
    {
        isRebinding = true;
        keyboardBindText.text = "Press any key...";
    }

    public void StartGamepadRebinding()
    {
        isRebinding = true;
        gamepadBindText.text = "Press any button...";
    }

    void OnGUI()
    {
        if (isRebinding && Event.current.isKey && Event.current.type == EventType.KeyDown)
        {
            newKeyCode = Event.current.keyCode;
            keyboardBindText.text = newKeyCode.ToString();
            PlayerPrefs.SetString("JumpKey", newKeyCode.ToString());
            isRebinding = false;
        }
    }
    #endregion

    #region Save/Load Settings
    public void SaveSettings()
    {
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        // Audio
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        bool isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;

        masterVolumeSlider.value = masterVolume;
        musicVolumeSlider.value = musicVolume;
        sfxVolumeSlider.value = sfxVolume;
        muteToggle.isOn = isMuted;

        // Graphics
        int width = PlayerPrefs.GetInt("ResolutionWidth", Screen.currentResolution.width);
        int height = PlayerPrefs.GetInt("ResolutionHeight", Screen.currentResolution.height);
        bool isFullscreen = PlayerPrefs.GetInt("IsFullscreen", 1) == 1;
        int quality = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        float brightness = PlayerPrefs.GetFloat("Brightness", 1f);
        bool vSync = PlayerPrefs.GetInt("VSync", 0) == 1;
        int frameRate = PlayerPrefs.GetInt("FrameRateLimit", 60);

        resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", 0);
        fullscreenToggle.isOn = isFullscreen;
        qualityDropdown.value = quality;
        brightnessSlider.value = brightness;
        vSyncToggle.isOn = vSync;
        frameRateSlider.value = frameRate;

        // Gameplay
        float sensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
        bool invertY = PlayerPrefs.GetInt("InvertY", 0) == 1;
        int language = PlayerPrefs.GetInt("Language", 0);
        bool showTutorial = PlayerPrefs.GetInt("ShowTutorial", 1) == 1;

        mouseSensitivitySlider.value = sensitivity;
        invertYToggle.isOn = invertY;
        languageDropdown.value = language;
        showTutorialToggle.isOn = showTutorial;

        // Controls
        string jumpKey = PlayerPrefs.GetString("JumpKey", "Space");
        keyboardBindText.text = jumpKey;
    }

    public void ResetToDefault()
    {
        PlayerPrefs.DeleteAll();
        LoadSettings();
    }
    #endregion

    private class ResolutionComparer : IEqualityComparer<Resolution>
    {
        public bool Equals(Resolution x, Resolution y)
        {
            return x.width == y.width && x.height == y.height;
        }

        public int GetHashCode(Resolution obj)
        {
            return obj.width.GetHashCode() ^ obj.height.GetHashCode();
        }
    }
}
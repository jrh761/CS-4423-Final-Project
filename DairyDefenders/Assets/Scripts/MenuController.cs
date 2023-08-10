using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class MenuController : MonoBehaviour
{
    public GameObject optionsPanel;
    public GameObject menuUI;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullScreenToggle;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    public TextMeshProUGUI titleText;
    public Button optionsButton;
    public Button quitButton;
    public Button closeOptionsButton;
    public Button startButton;
    public Button applyButton;

    public AudioMixer audioMixer;

    private Resolution[] resolutions;
    private Dictionary<string, Resolution> uniqueResolutions = new Dictionary<string, Resolution>();
    private int selectedResolutionIndex;

    public void Start()
    {
        optionsPanel.SetActive(false);

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            if (!uniqueResolutions.ContainsKey(option))
            {
                uniqueResolutions[option] = resolutions[i];
            }
        }

        options = uniqueResolutions.Keys.ToList();

        selectedResolutionIndex = FindIndexOfHighestResolution(options);
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = selectedResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        SetActualResolution(selectedResolutionIndex);

        if (fullScreenToggle != null)
        {
            fullScreenToggle.isOn = Screen.fullScreen;
        }

        InitializeVolumeSliders();
    }

    private int FindIndexOfHighestResolution(List<string> resolutions)
    {
        int maxResolution = 0;
        int index = 0;
        for (int i = 0; i < resolutions.Count; i++)
        {
            string[] dimensions = resolutions[i].Split('x');
            int width = int.Parse(dimensions[0].Trim());
            int height = int.Parse(dimensions[1].Trim());

            if (width * height > maxResolution)
            {
                maxResolution = width * height;
                index = i;
            }
        }
        return index;
    }

    void InitializeVolumeSliders()
    {
        masterVolumeSlider.value = GetVolumeFromMixer("Master");
        SetMasterVolume(masterVolumeSlider.value);

        musicVolumeSlider.value = GetVolumeFromMixer("Music");
        SetMusicVolume(musicVolumeSlider.value);

        sfxVolumeSlider.value = GetVolumeFromMixer("SFX");
        SetSFXVolume(sfxVolumeSlider.value);
    }

    float GetVolumeFromMixer(string parameterName)
    {
        float currentVolume;
        audioMixer.GetFloat(parameterName, out currentVolume);
        return currentVolume;
    }

    public void SetMasterVolume(float dBValue)
    {
        audioMixer.SetFloat("Master", dBValue);
    }

    public void SetMusicVolume(float dBValue)
    {
        audioMixer.SetFloat("Music", dBValue);
    }

    public void SetSFXVolume(float dBValue)
    {
        audioMixer.SetFloat("SFX", dBValue);
    }

    public void StartGame()
    {
        menuUI.SetActive(false);
        SceneManager.LoadScene("Level1");
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void ShowOptions()
    {
        optionsPanel.SetActive(true);
    }

    public void HideOptions()
    {
        optionsPanel.SetActive(false);
    }

    public void SetResolution(int resolutionIndex)
    {
        selectedResolutionIndex = resolutionIndex;
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void ApplyChanges()
    {
        SetActualResolution(selectedResolutionIndex);
        SetFullscreen(fullScreenToggle.isOn);
        // Any other settings to be applied can go here
    }

    private void SetActualResolution(int resolutionIndex)
    {
        string selectedOption = resolutionDropdown.options[resolutionIndex].text;
        Resolution resolution = uniqueResolutions[selectedOption];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
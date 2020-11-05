using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public TMP_Dropdown graphicsDropDown;
    public TMP_Dropdown resolutionsDropDown;
    public Toggle fullscreen;
    public Toggle vsync;
    public Slider hunterSoundSlider;
    public Slider propSoundSlider;
    public AudioMixer audioMixer;

    private Resolution[] resolutions;

    // Start is called before the first frame update
    void Start()
    {
        // set graphics level
        graphicsDropDown.value = QualitySettings.GetQualityLevel();

        // set resolutions
        resolutions = Screen.resolutions;

        List<string> resOptions = new List<string>();
        int currentResIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " " + resolutions[i].refreshRate;
            resOptions.Add(option);

            if (resolutions[i].width == PlayerPrefs.GetInt("screenwidth", 1920) && resolutions[i].height == PlayerPrefs.GetInt("screenheight", 1080))
            {
                currentResIndex = i;
            }
        }
        resolutionsDropDown.AddOptions(resOptions);
        resolutionsDropDown.SetValueWithoutNotify(currentResIndex);
        resolutionsDropDown.RefreshShownValue();

        // set fullscreen toggle
        fullscreen.isOn = Screen.fullScreen;

        // set vsync toggle
        QualitySettings.vSyncCount = PlayerPrefs.GetInt("vsync", 1);
        vsync.isOn = (QualitySettings.vSyncCount == 1);

        // set music / sound values
        hunterSoundSlider.value = PlayerPrefs.GetFloat("HunterVolume", 1);
        audioMixer.SetFloat("HunterVolume", Mathf.Log10(hunterSoundSlider.value) * 20);
        propSoundSlider.value = PlayerPrefs.GetFloat("PropVolume", 1);
        audioMixer.SetFloat("PropVolume", Mathf.Log10(propSoundSlider.value) * 20);
    }

    public void SetGraphicsQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("screenwidth", resolution.width);
        PlayerPrefs.SetInt("screenheight", resolution.height);
    }

    public void SetVsync(bool vsync)
    {
        if (vsync)
        {
            QualitySettings.vSyncCount = 1;
            PlayerPrefs.SetInt("vsync", 1);
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            PlayerPrefs.SetInt("vsync", 0);
        }
    }

    public void SetMusicVolume()
    {
        audioMixer.SetFloat("HunterVolume", Mathf.Log10(hunterSoundSlider.value) * 20);
        PlayerPrefs.SetFloat("HunterVolume", hunterSoundSlider.value);
    }

    public void SetSoundVolume()
    {
        audioMixer.SetFloat("PropVolume", Mathf.Log10(propSoundSlider.value) * 20);
        PlayerPrefs.SetFloat("PropVolume", propSoundSlider.value);
    }
}

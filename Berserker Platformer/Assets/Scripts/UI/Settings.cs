using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Settings : MonoBehaviour
{
    public bool anySettingChanged;
    public Image ApplyButtonImage;
    
    // Display
    public TMP_Dropdown Resolution;
    public TMP_Dropdown FPS;
    public TMP_Dropdown WindowMode;
    public Toggle V_Sync;
    
    // Audio
    public Slider Master_Volume;
    public Slider Music_Volume;
    public Slider SFX_Volume;

    // Only gets called once when settings gets opened for the first time
    void Start()
    {
        PopulateUIElements();
        // Reset flags from initialization
        ResetApply();
    }

    void OnEnable()
    {
        ResetApply();
    }

    public void ResetApply()
    {
        anySettingChanged = false;
        Color currentColor = ApplyButtonImage.color;
        currentColor.a = Mathf.Clamp01(0.43f);
        ApplyButtonImage.color = currentColor;
    }
    public void ApplyChanges()
    {
        string selectedRes = Resolution.options[Resolution.value].text;
        int width = 0, height = 0;
        if (TryParseResolution(selectedRes, out width, out height))
        {
            Debug.Log($"Parsed Resolution: {width} x {height}");
        }
        else
        {
            Debug.LogWarning("Failed to parse resolution.");
        }
        int selectedFPS = int.Parse(FPS.options[FPS.value].text);

        string selectedMode = WindowMode.options[WindowMode.value].text;
        switch(selectedMode)
        {
            case "Fullscreen":
                SaveManager.Instance.SetResolution_FullScreenMode_FPS(width, height, 0, selectedFPS);
                break;
            case "Borderless":
                SaveManager.Instance.SetResolution_FullScreenMode_FPS(width, height, 1, selectedFPS);
                break;
            case "Windowed":
                SaveManager.Instance.SetResolution_FullScreenMode_FPS(width, height, 3, selectedFPS);
                break;
        }
        int vsyncValue = V_Sync.isOn ? 1 : 0;
        SaveManager.Instance.SetVSync(vsyncValue);
    }
    public void SettingChanged()
    {
        anySettingChanged = true;
        Color currentColor = ApplyButtonImage.color;
        currentColor.a = Mathf.Clamp01(0.824f);
        ApplyButtonImage.color = currentColor;
    }

    public void VolumeChanged()
    {
        // Master Volume
        SaveManager.Instance.SetMasterVolume(Master_Volume.value);
        // Music Volume
        SaveManager.Instance.SetMusicVolume(Music_Volume.value);
        // SFX Volume
        SaveManager.Instance.SetSFXVolume(SFX_Volume.value);
    }

    // Call to grab values from playerprefs and populate settings menu
    public void PopulateUIElements()
    {
        // --- Display settings ---
        PopulateResolutionDropdown();
        
        // FPS from PlayerPrefs or default (e.g., 60)
        PopulateFPSDropdown();
        
        // WindowMode (e.g., "Fullscreen" or "Windowed")
        PopulateWindowModeDropdown();
        
        // Read V-Sync setting (store as 1 for ON, 0 for OFF in PlayerPrefs)
        bool vsync = PlayerPrefs.GetInt("VSYNC") == 1;
        V_Sync.isOn = vsync;
        
        // --- Audio settings ---
        // Read Master Volume (0.0 to 1.0)
        float masterVolume = PlayerPrefs.GetFloat("MASTER_VOLUME");
        Master_Volume.value = masterVolume;
        
        // Read Music Volume (0.0 to 1.0)
        float musicVolume = PlayerPrefs.GetFloat("MUSIC_VOLUME");
        Music_Volume.value = musicVolume;
        
        // Read SFX Volume (0.0 to 1.0)
        float sfxVolume = PlayerPrefs.GetFloat("SFX_VOLUME");
        SFX_Volume.value = sfxVolume;
    }

    // Returns true if the given resolution is approximately 16:9, including fractional (1280x720) or higher/lower multiples
    bool IsFractionalMultipleOf1080p(Resolution r, float tolerance = 0.01f)
    {
        // 16:9 ratio = 1.7777...
        float ratio = (float)r.width / r.height;
        float targetRatio = 1920f / 1080f; // ~1.7777...

        // If the absolute difference between ratio and 16:9 is within tolerance, we include it
        return (Mathf.Abs(ratio - targetRatio) < tolerance);
    }
    private bool TryParseResolution(string resolution, out int width, out int height)
    {
        width = 0;
        height = 0;

        // Split the resolution string by "x" (e.g., "1920 x 1080" -> ["1920", "1080"])
        string[] parts = resolution.Split('x');
        if (parts.Length == 2 && int.TryParse(parts[0].Trim(), out width) && int.TryParse(parts[1].Trim(), out height))
        {
            return true; // Successfully parsed
        }

        return false; // Failed to parse
    }
    void PopulateResolutionDropdown()
    {
        // Clear existing dropdown options
        Resolution.ClearOptions();

        // 1) Get current resolution as a string "width x height"
        int currentRes_width  = PlayerPrefs.GetInt("RES_WIDTH");
        int currentRes_height = PlayerPrefs.GetInt("RES_HEIGHT");
        string currentResString = $"{currentRes_width} x {currentRes_height}";

        // 2) Build a set of unique resolutions that have ~16:9 aspect ratio
        HashSet<string> uniqueResStrings = new HashSet<string>();

        foreach (UnityEngine.Resolution r in Screen.resolutions)
        {
            if (IsFractionalMultipleOf1080p(r))
            {
                string resString = $"{r.width} x {r.height}";
                uniqueResStrings.Add(resString); // HashSet ensures no duplicates
            }
        }

        // Convert the HashSet to a List<string>
        List<string> options = new List<string>(uniqueResStrings);

        // 3) Sort resolutions (optional, e.g., by width then height)
        // If you want them in ascending order by width or height, you can do something like:
        // options.Sort((a, b) => {
        //     // parse 'a' and 'b' to get the width/height for more precise sorting
        //     // but if not needed, skip this step.
        // });

        // 4) Add these filtered, unique resolutions to the dropdown
        Resolution.AddOptions(options);

        // 5) Try to select the current resolution in the dropdown if it matches
        int currentIndex = options.IndexOf(currentResString);
        if (currentIndex != -1)
        {
            Resolution.value = currentIndex;
        }

        // 6) Refresh the dropdown to show the updated value
        Resolution.RefreshShownValue();
    }

    void PopulateWindowModeDropdown()
    {
        // Reference to the dropdown's list of options
        List<TMP_Dropdown.OptionData> WindowModeOptions = WindowMode.options;

        // Find the index of the option that matches the saved "FULLSCREEN_MODE" value
        int savedMode = PlayerPrefs.GetInt("FULLSCREEN_MODE"); // Default is "Borderless"
        
        switch(savedMode)
        {
            case 0:
                WindowMode.value = 0;
                break;
            case 1:
                WindowMode.value = 1;
                break;
            case 3:
                WindowMode.value = 2;
                break;
        }
        WindowMode.RefreshShownValue(); // Update the visible text
    }
    void PopulateFPSDropdown()
    {
        // Get the saved FPS value from PlayerPrefs (default to 60)
        int savedFPS = PlayerPrefs.GetInt("FRAME_RATE");

        // Find the index of the saved FPS value in the dropdown options
        int currentIndex = FPS.options.FindIndex(option => option.text == savedFPS.ToString());

        // If the index is valid, set it as the selected value
        if (currentIndex != -1)
        {
            FPS.value = currentIndex;
            FPS.RefreshShownValue(); // Update the visible text
        }
        else
        {
            Debug.LogWarning($"Saved FPS value ({savedFPS}) not found in dropdown options.");
        }
    }
}

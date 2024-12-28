using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Settings : MonoBehaviour
{
    // Display
    public TMP_Dropdown Resolution;
    public TMP_Text FPS;
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
    }


    // Call to grab values from playerprefs and populate settings menu
    public void PopulateUIElements()
    {
        // --- Display settings ---
        PopulateResolutionDropdown();
        
        // Read FPS from PlayerPrefs or use default (e.g., 60)
        int fps = PlayerPrefs.GetInt("FRAME_RATE");
        FPS.text = fps.ToString();
        
        // Read WindowMode (e.g., "Fullscreen" or "Windowed")
        string windowMode = PlayerPrefs.GetString("FULLSCREEN_MODE");

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

    void PopulateResolutionDropdown()
    {
        Resolution.ClearOptions();  // Clear existing dropdown options

        // 1) Get current resolution as a string "width x height"
        int currentRes_width = PlayerPrefs.GetInt("RES_WIDTH");
        int currentRes_height = PlayerPrefs.GetInt("RES_HEIGHT");

        string currentResString = $"{currentRes_width} x {currentRes_height}";


        // 2) Build a list of resolutions that have ~16:9 aspect ratio
        List<string> options = new List<string>();
        foreach (Resolution r in Screen.resolutions)
        {
            if (IsFractionalMultipleOf1080p(r))
            {
                options.Add($"{r.width} x {r.height}");
            }
        }

        // 3) Add these filtered options to the dropdown
        Resolution.AddOptions(options);

        // 4) Try to select the current resolution in the dropdown if it matches
        int currentIndex = options.IndexOf(currentResString);
        if (currentIndex != -1)
        {
            Resolution.value = currentIndex;
        }

        // 5) Refresh the dropdown to show the updated value
        Resolution.RefreshShownValue();
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


}

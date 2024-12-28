using UnityEngine;
using System.IO;

// Handles actually saving and using default values if no save values are found in playerprefs/save file
public class SaveManager : MonoBehaviour
{

    // We'll store all save files inside a "Saves" folder within dataPath.
    private static readonly string SAVE_FOLDER = Path.Combine(Application.dataPath, "Saves");
    private static readonly string SAVE_FILE_NAME = "PlayerProgress.json";
    private static readonly string SAVE_FILE_PATH = Path.Combine(SAVE_FOLDER, SAVE_FILE_NAME);

    private const int DEFAULT_RES_WIDTH = 1920;
    private const int DEFAULT_RES_HEIGHT = 1080;

    // Resolution keys
    private const string KEY_RES_WIDTH         = "RES_WIDTH";
    private const string KEY_RES_HEIGHT        = "RES_HEIGHT";
    private const string KEY_FULLSCREEN_MODE   = "FULLSCREEN_MODE";  // Fullscreen, Borderless, Windowed
    private const string KEY_FRAME_RATE        = "FRAME_RATE";       // e.g. 60,120
    private const string KEY_VSYNC             = "VSYNC";            // 0=Off,1=On

    // Volume keys
    private const string KEY_MASTER_VOLUME = "MASTER_VOLUME";
    private const string KEY_SFX_VOLUME    = "SFX_VOLUME";
    private const string KEY_MUSIC_VOLUME  = "MUSIC_VOLUME";


    // Singleton reference
    public static SaveManager Instance { get; private set; }
    private void Awake()
    {
        // Singleton enforcement
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure the Saves folder exists
        if (!Directory.Exists(SAVE_FOLDER))
        {
            Directory.CreateDirectory(SAVE_FOLDER);
            Debug.Log($"Created directory for saves at: {SAVE_FOLDER}");
        }
    }

    private void Start()
    {
        // Load or set default settings from PlayerPrefs
        ApplySettingsFromPlayerPrefs();
    }

    // Saves the player's progress to a JSON file on disk (inside the "Saves" folder).
    // Progress comes from GameManager
    public void SavePlayerProgress(PlayerProgress progress)
    {
        if (progress == null)
        {
            Debug.LogWarning("SavePlayerProgress called with null data. Nothing to save.");
            return;
        }

        // Convert to JSON
        string json = JsonUtility.ToJson(progress, true);

        // Write the file
        File.WriteAllText(SAVE_FILE_PATH, json);
        Debug.Log($"PlayerProgress saved to: {SAVE_FILE_PATH}");
    }

    // Loads the player's progress from the JSON file on disk.
    // Returns a new PlayerProgress if no file is found or if loading fails.
    public PlayerProgress LoadPlayerProgress()
    {
        if (!File.Exists(SAVE_FILE_PATH))
        {
            Debug.LogWarning($"No PlayerProgress file found at {SAVE_FILE_PATH}. Returning new PlayerProgress.");
            return new PlayerProgress();
        }

        string json = File.ReadAllText(SAVE_FILE_PATH);
        PlayerProgress loadedData = JsonUtility.FromJson<PlayerProgress>(json);

        if (loadedData == null)
        {
            Debug.LogWarning("Failed to parse PlayerProgress JSON. Returning new PlayerProgress.");
            return new PlayerProgress();
        }

        Debug.Log($"PlayerProgress loaded from: {SAVE_FILE_PATH}");
        return loadedData;
    }

    // Reads or applies default settings from PlayerPrefs for 
    // resolution, fullscreen mode, framerate, vsync, and volumes.
    private void ApplySettingsFromPlayerPrefs()
    {
        // Get system defaults (in case keys don't exist)
        Resolution currentRes = Screen.currentResolution;
        
        int defaultWidth;
        int defaultHeight;
        if(IsFractionalMultipleOf1080p(currentRes))
        {
            defaultWidth = currentRes.width;
            defaultHeight = currentRes.height;
        } else {
            defaultWidth = DEFAULT_RES_WIDTH;
            defaultHeight = DEFAULT_RES_HEIGHT;
        }
        
        int defaultFps        = 60;
        int defaultVsync      = 1; // 1 = On
        float defaultMaster   = 0.5f;
        float defaultSfx      = 0.5f;
        float defaultMusic    = 0.5f;

        // 1) Ensure each PlayerPrefs key is set; if not, set it to the default
        if (!PlayerPrefs.HasKey(KEY_RES_WIDTH))
            PlayerPrefs.SetInt(KEY_RES_WIDTH, defaultWidth);

        if (!PlayerPrefs.HasKey(KEY_RES_HEIGHT))
            PlayerPrefs.SetInt(KEY_RES_HEIGHT, defaultHeight);


        if (!PlayerPrefs.HasKey(KEY_FULLSCREEN_MODE))
            PlayerPrefs.SetInt(KEY_FULLSCREEN_MODE, (int)FullScreenMode.FullScreenWindow);

        if (!PlayerPrefs.HasKey(KEY_FRAME_RATE))
            PlayerPrefs.SetInt(KEY_FRAME_RATE, defaultFps);

        if (!PlayerPrefs.HasKey(KEY_VSYNC))
            PlayerPrefs.SetInt(KEY_VSYNC, defaultVsync);

        if (!PlayerPrefs.HasKey(KEY_MASTER_VOLUME))
            PlayerPrefs.SetFloat(KEY_MASTER_VOLUME, defaultMaster);

        if (!PlayerPrefs.HasKey(KEY_SFX_VOLUME))
            PlayerPrefs.SetFloat(KEY_SFX_VOLUME, defaultSfx);

        if (!PlayerPrefs.HasKey(KEY_MUSIC_VOLUME))
            PlayerPrefs.SetFloat(KEY_MUSIC_VOLUME, defaultMusic);

        // Save any newly assigned defaults
        PlayerPrefs.Save();

        Debug.Log("Res width = " + PlayerPrefs.GetInt(KEY_RES_WIDTH).ToString());
        Debug.Log("Res height = " + PlayerPrefs.GetInt(KEY_RES_HEIGHT).ToString());

        // 2) Now retrieve the (potentially just-set) values from PlayerPrefs
        int width             = PlayerPrefs.GetInt(KEY_RES_WIDTH);
        int height            = PlayerPrefs.GetInt(KEY_RES_HEIGHT);
        
        int fullscreenModeInt = PlayerPrefs.GetInt(KEY_FULLSCREEN_MODE);
        FullScreenMode fsMode = (FullScreenMode)fullscreenModeInt;
        
        int frameRate         = PlayerPrefs.GetInt(KEY_FRAME_RATE);
        int vsync             = PlayerPrefs.GetInt(KEY_VSYNC);

        float masterVolume    = PlayerPrefs.GetFloat(KEY_MASTER_VOLUME);
        float sfxVolume       = PlayerPrefs.GetFloat(KEY_SFX_VOLUME);
        float musicVolume     = PlayerPrefs.GetFloat(KEY_MUSIC_VOLUME);

        // 3) Apply these settings to the game
        // Resolution and Fullscreen
        Screen.SetResolution(width, height, fsMode);

        // Frame rate
        Application.targetFrameRate = frameRate;

        // V-Sync
        QualitySettings.vSyncCount = vsync;

        // Master volume (tied to AudioListener volume as an example)
        AudioListener.volume = masterVolume;

        // (Optional) You'd likely route sfxVolume and musicVolume to specific AudioMixers.

        // Debug output for clarity
        Debug.Log(
            $"[APPLY SETTINGS] Resolution: {width}x{height}, FullscreenMode: {fsMode}, FrameRate: {frameRate}, " +
            $"VSync: {(vsync == 1 ? "On" : "Off")}, MasterVol: {masterVolume}, SfxVol: {sfxVolume}, MusicVol: {musicVolume}"
        );
    }


    // -------------- OPTIONAL HELPER METHODS --------------
    // Call these whenever a player changes the setting in your UI or Options menu.

    public void SetResolution(int width, int height, int mode)
    {
        PlayerPrefs.SetInt(KEY_RES_WIDTH, width);
        PlayerPrefs.SetInt(KEY_RES_HEIGHT, height);
        PlayerPrefs.SetInt(KEY_FULLSCREEN_MODE, mode);
        PlayerPrefs.Save();

        Screen.SetResolution(width, height, (FullScreenMode)mode);
    }

    public void SetFrameRate(int fps)
    {
        PlayerPrefs.SetInt(KEY_FRAME_RATE, fps);
        PlayerPrefs.Save();

        Application.targetFrameRate = fps;
    }

    public void SetVSync(int vsyncCount)
    {
        PlayerPrefs.SetInt(KEY_VSYNC, vsyncCount);
        PlayerPrefs.Save();

        QualitySettings.vSyncCount = vsyncCount;
    }

    // Sets and saves the MASTER volume. (We tie AudioListener.volume to MASTER_VOLUME.)
    public void SetMasterVolume(float volume)
    {
        PlayerPrefs.SetFloat(KEY_MASTER_VOLUME, volume);
        PlayerPrefs.Save();

        AudioListener.volume = volume; // Just as an example
    }

    // Sets and saves the SFX volume (used by your SFX mixer channel or manager).
    public void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat(KEY_SFX_VOLUME, volume);
        PlayerPrefs.Save();
        // In practice, route this to your SFX mixer:
        // e.g. sfxMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        Debug.Log($"SFX volume set to {volume}");
    }

    // Sets and saves the MUSIC volume (used by your Music mixer channel or manager).
    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat(KEY_MUSIC_VOLUME, volume);
        PlayerPrefs.Save();
        // Similarly, apply to a separate music mixer
        Debug.Log($"Music volume set to {volume}");
    }


    bool IsFractionalMultipleOf1080p(Resolution r, float tolerance = 0.01f)
    {
        // 16:9 ratio = 1.7777...
        float ratio = (float)r.width / r.height;
        float targetRatio = 1920f / 1080f; // ~1.7777...

        // If the absolute difference between ratio and 16:9 is within tolerance, we include it
        return (Mathf.Abs(ratio - targetRatio) < tolerance);
    }
}

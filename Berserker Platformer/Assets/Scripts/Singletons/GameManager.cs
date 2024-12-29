using UnityEngine;
using UnityEngine.SceneManagement;

/// Example class to store player progress data.
/// This is the *only* class we'll be saving to JSON.
[System.Serializable]
public class PlayerProgress
{
    // Add whatever else is critical for the player's progress
    public int[] levelsCompleted;
}

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Example game state variables
    public bool IsPaused { get; private set; }
    public string CurrentSceneName { get; private set; }

    // Example references
    // (e.g., reference to a player GameObject or any other manager)
    // public GameObject player;    

    private void Awake()
    {
        // Enforce the singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {

        CurrentSceneName = SceneManager.GetActiveScene().name;
    }

    // Example method to pause/unpause the game.
    public void SetPause(bool pause)
    {
        IsPaused = pause;
        Debug.Log($"IsPaused set to {IsPaused}");
        if(IsPaused)
            Debug.Log("GAME PAUSED");
        Time.timeScale = pause ? 0 : 1;
    }

    // Example method to load a different scene. 
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        CurrentSceneName = sceneName;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class MainMenu : MonoBehaviour
{
    public GameObject settingsMenu;

    public void OpenSettings(){
       settingsMenu.SetActive(true);
    }

    public void PlayGame()
    {
        // Coroutine to load LevelSelection Async in background 
        StartCoroutine(LoadLevelSelection());
    }

    IEnumerator LoadLevelSelection()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Level Selection");

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        // Main Menu scene is auto-unloaded
    }

}

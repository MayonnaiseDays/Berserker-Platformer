using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class LevelSelection : MonoBehaviour
{

    public GameObject settingsMenu;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenSettings(){
       settingsMenu.SetActive(true);
    }

    public void PlayLevel(int level)
    {
        //StartCoroutine(LoadLevelScene(level));
        SceneManager.LoadScene(level);
    }

    IEnumerator LoadLevelScene(int level)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(level);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Level Selection scene is auto-unloaded
    }
}

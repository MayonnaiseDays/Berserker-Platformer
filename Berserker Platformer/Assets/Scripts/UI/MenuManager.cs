using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject levelSelectMenu;
    public GameObject settingsMenu;

    public GameObject currentMenu;


    void Start()
    {
        if(!mainMenu.active)
            OpenMainMenu();
        if(levelSelectMenu.active)
            CloseLevelSelection();
        if(settingsMenu.active)
            CloseSettings();


    }

    # region Open/Close Menus
        public void OpenMainMenu(){
            mainMenu.SetActive(true);
            currentMenu = mainMenu;
        }
        public void OpenLevelSelection(){
            levelSelectMenu.SetActive(true);
            currentMenu = levelSelectMenu;
        }
        public void CloseMainMenu(){
            mainMenu.SetActive(false);
        }
        public void CloseLevelSelection(){
            levelSelectMenu.SetActive(false);
        }

        public void OpenSettings(){
            settingsMenu.SetActive(true);
        }
        public void CloseSettings(){
            settingsMenu.SetActive(false);
        }
    # endregion

    public void PlayGame()
    {
        // Coroutine to load LevelSelection Async in background 
        //StartCoroutine(LoadLevelSelection());
    }

    private void OpenLevelSelectionMenu()
    {
        
    }
}

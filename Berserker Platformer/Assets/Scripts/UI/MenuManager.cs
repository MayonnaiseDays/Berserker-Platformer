using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameManager gameManager;
    
    // UI Screens
    public GameObject mainMenu;
    public GameObject levelSelectMenu;
    public GameObject settingsMenu;
    public GameObject currentMenu;


    void Start()
    {
        gameManager = GameManager.Instance;

        if(!mainMenu.activeSelf)
            OpenMainMenu();
        if(levelSelectMenu.activeSelf)
            CloseLevelSelection();
        if(settingsMenu.activeSelf)
            CloseSettings();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Use ternary operator to toggle settings
            if (gameManager.IsPaused)
            {
                CloseSettings();
            }
            else
            {
                OpenSettings();
            }
        }
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
            // Toggle the isPaused state
            gameManager.SetPause(!gameManager.IsPaused);
        }
        public void CloseSettings(){
            settingsMenu.SetActive(false);
            // Toggle the isPaused state
            gameManager.SetPause(!gameManager.IsPaused);
        }
    # endregion


}

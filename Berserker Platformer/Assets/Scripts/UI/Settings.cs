using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Settings : MonoBehaviour
{

    #region General Settings Functionality
    // handles things like opening/closing settings
    public void ExitSettings(){
            gameObject.SetActive(false);
    }

    #endregion





    #region Display/Graphics Settings

    // Get resolution dropdown selection and apply setting
    public void SetResolution(int dropdownIndex){
        switch(dropdownIndex) 
            {
            case 0:
                Screen.SetResolution(1280, 720, FullScreenMode.FullScreenWindow);
                Debug.Log("Resolution at index " + dropdownIndex + " chosen");
                break;
            case 1:
                Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
                break;
            case 2:
                Screen.SetResolution(2560, 1440, FullScreenMode.FullScreenWindow);
                break;
            case 3:
                Screen.SetResolution(3840, 2160, FullScreenMode.FullScreenWindow);
                break;
            default:
                Debug.LogError("Something went wrong and no resolution was selected. Defaulting to 1080p");
                Screen.SetResolution(1280, 720, FullScreenMode.FullScreenWindow);
                break;
            }
    }

    public void SetFPSLimit(){

    }


    #endregion


    #region Audio Settings





    #endregion


}

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject settingsMenu;
    public GameObject exitQuery;

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    //Settings
    public void toggleSettingsMenu()
    {
        if (settingsMenu.activeInHierarchy)
            settingsMenu.SetActive(false);
        else if (!exitQuery.activeInHierarchy)
            settingsMenu.SetActive(true);
    }

    //Exit
    public void toggleExitQuery()
    {
        if (exitQuery.activeInHierarchy)
            exitQuery.SetActive(false);
        else if (!settingsMenu.activeInHierarchy)
            exitQuery.SetActive(true);
    }
    public void exitGame()
    {
        Application.Quit();
    }
}

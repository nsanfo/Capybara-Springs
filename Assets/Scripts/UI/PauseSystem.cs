using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseSystem : MonoBehaviour
{
    public GameObject pauseAIO;
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    public GameObject mainMenuQuery;
    public GameObject exitQuery;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            pauseToggle(); 
        }
    }
    //Pause functionality
    public void pauseToggle()
    {
        //Pause or Unpause Game
        if (pauseMenu.activeInHierarchy) //if unpausing
        {
            //Close any active panels
            if (settingsMenu.activeInHierarchy)
                settingsMenu.SetActive(false);

            if (mainMenuQuery.activeInHierarchy)
                settingsMenu.SetActive(false);

            if (exitQuery.activeInHierarchy)
                settingsMenu.SetActive(false);


            Time.timeScale = 1.0f;      //Resume time to normal speed
            pauseMenu.SetActive(false); //Deactivate pause menu
        }
        else                             //if pausing
        {
            Time.timeScale = 0f;         //freeze time
            pauseMenu.SetActive(true);   //activate pause menu
        }
    }

    //Settings
    public void toggleSettingsPanel()
    {
        if (settingsMenu.activeInHierarchy)
            settingsMenu.SetActive(false);
        else
            settingsMenu.SetActive(true);
    }

    //Main Menu
    public void toggleMainMenuQuery()
    {
        if (mainMenuQuery.activeInHierarchy)
            mainMenuQuery.SetActive(false);
        else
            mainMenuQuery.SetActive(true);
    }

    public void returnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    //Exit Game
    public void toggleExitGameQuery()
    {
        if (exitQuery.activeInHierarchy)
            exitQuery.SetActive(false);
        else
            exitQuery.SetActive(true);
    }
    public void exitGame()
    {
        Application.Quit();
    }
}


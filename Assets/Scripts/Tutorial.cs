using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    enum States { wait, camera, build, buildOptions, pathSelect, pathBuild, amenitySelect, amenityBuild, capySelect, capyView, amenityUpgrade, plotSelect, plotView}
    States state = States.wait;

    GameObject welcomeScreen, cameraScreen, buildScreen, buildOptionsScreen, pathScreen, amenitySelectScreen, amenityBuildScreen, capySelectScreen, capyInfoScreen1, capyInfoScreen2, amenityUpgradeScreen, plotSelectScreen, plotViewScreen, concludeScreen;

    public GameObject buildArrow, pathArrow, amenityArrow, plotArrow;

    AudioSource clickSound, tutorialSound;

    CameraControl cameraScript;
    PathBuilder pathBuilderScript;
    AmenitiesBuilder amenitiesBuilderScript;
    UIManager uiManagerScript;

    bool buildPressed;

    void Start()
    {
        var sounds = GameObject.Find("UISounds");
        clickSound = sounds.transform.GetChild(1).GetComponent<AudioSource>();
        tutorialSound = sounds.transform.GetChild(7).GetComponent<AudioSource>();
        cameraScript = GameObject.Find("Main Camera").GetComponent<CameraControl>();
        pathBuilderScript = GameObject.Find("PlayerBuilding").GetComponent<PathBuilder>();
        amenitiesBuilderScript = GameObject.Find("PlayerBuilding").GetComponent<AmenitiesBuilder>();
        uiManagerScript = GameObject.Find("UI Manager").GetComponent<UIManager>();

        welcomeScreen = transform.GetChild(0).gameObject;
        cameraScreen = transform.GetChild(1).gameObject;
        buildScreen = transform.GetChild(2).gameObject;
        buildOptionsScreen = transform.GetChild(3).gameObject;
        pathScreen = transform.GetChild(4).gameObject;
        amenitySelectScreen = transform.GetChild(5).gameObject;
        amenityBuildScreen = transform.GetChild(6).gameObject;
        capySelectScreen = transform.GetChild(7).gameObject;
        capyInfoScreen1 = transform.GetChild(8).gameObject;
        capyInfoScreen2 = transform.GetChild(9).gameObject;
        plotSelectScreen = transform.GetChild(10).gameObject;
        plotViewScreen = transform.GetChild(11).gameObject;
        concludeScreen = transform.GetChild(12).gameObject;
        amenityUpgradeScreen = transform.GetChild(13).gameObject;
        welcomeScreen.SetActive(true);
    }

    void Update()
    {
        switch (state)
        {
            case States.camera:
                if (cameraScript.tutorialHook)
                    StartCoroutine(CameraWait());
                break;
            case States.build:
                if (buildPressed)
                {
                    StartCoroutine(BuildWait());
                }
                break;
            case States.pathBuild:
                if (pathBuilderScript.tutorialHook)
                {
                    pathScreen.SetActive(false);
                    StartCoroutine(PathWait());
                }
                break;
            case States.amenityBuild:
                if (amenitiesBuilderScript.tutorialHookBuild)
                {
                    amenityBuildScreen.SetActive(false);
                    StartCoroutine(AmenityWait());
                }
                break;
            case States.capySelect:
                if (uiManagerScript.capyTutorialHook)
                {
                    tutorialSound.Play();
                    capyInfoScreen1.SetActive(true);
                    state = States.capyView;
                }
                break;
        }
    }

    public void SkipTutorial()
    {
        clickSound.Play();
        gameObject.SetActive(false);
    }

    public void WelcomeContinue()
    {
        clickSound.Play();
        welcomeScreen.SetActive(false);
        cameraScreen.SetActive(true);
    }

    public void CameraContinue()
    {
        clickSound.Play();
        cameraScreen.SetActive(false);
        state = States.camera;
    }

    private IEnumerator CameraWait()
    {
        state = States.wait;
        yield return new WaitForSeconds(5);
        tutorialSound.Play();
        buildScreen.SetActive(true);
        buildArrow.SetActive(true);
        state = States.build;
    }

    private IEnumerator BuildWait()
    {
        state = States.wait;
        buildScreen.SetActive(false);
        buildArrow.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        buildOptionsScreen.SetActive(true);
        state = States.buildOptions;
    }

    public void BuildPress()
    {
        if (state == States.build)
            buildPressed = true;
    }

    public void BuildOptionsContinue()
    {
        clickSound.Play();
        buildOptionsScreen.SetActive(false);
        pathScreen.SetActive(true);
        pathArrow.SetActive(true);
        state = States.pathSelect;
    }

    public void PathPress()
    {
        if (state == States.pathSelect)
        {
            pathArrow.SetActive(false);
            state = States.pathBuild;
        }
    }

    private IEnumerator PathWait()
    {
        state = States.wait;
        yield return new WaitForSeconds(1);
        tutorialSound.Play();
        amenitySelectScreen.SetActive(true);
        amenityArrow.SetActive(true);
        state = States.amenitySelect;
    }

    public void AmenityPress()
    {
        if (state == States.amenitySelect)
        {
            amenitySelectScreen.SetActive(false);
            amenityArrow.SetActive(false);
            amenityBuildScreen.SetActive(true);
            state = States.amenityBuild;
        }
    }

    private IEnumerator AmenityWait()
    {
        state = States.wait;
        yield return new WaitForSeconds(3);
        tutorialSound.Play();
        capySelectScreen.SetActive(true);
    }

    public void CapySelectContinue()
    {
        clickSound.Play();
        capySelectScreen.SetActive(false);
        state = States.capySelect;
    }

    public void CapyInfoContinue1()
    {
        clickSound.Play();
        capyInfoScreen1.SetActive(false);
        capyInfoScreen2.SetActive(true);
    }

    public void CapyInfoContinue2()
    {
        clickSound.Play();
        uiManagerScript.closeInfoScreen();
        capyInfoScreen2.SetActive(false);
        amenityUpgradeScreen.SetActive(true);
    }

    public void UpgradeContinue()
    {
        clickSound.Play();
        amenityUpgradeScreen.SetActive(false);
        state = States.amenityUpgrade;
    }

    public void UpgradePress()
    {
        if (state == States.amenityUpgrade)
        {
            StartCoroutine(UpgradeWait());
        }
    }

    public IEnumerator UpgradeWait()
    {
        state = States.wait;
        yield return new WaitForSeconds(2);
        tutorialSound.Play();
        state = States.plotSelect;
        plotSelectScreen.SetActive(true);
        plotArrow.SetActive(true);
    }

    public void PlotPress()
    {
        if (state == States.plotSelect)
        {
            plotSelectScreen.SetActive(false);
            plotArrow.SetActive(false);
            StartCoroutine(PlotWait1());
        }
        else if (state == States.plotView)
        {
            plotViewScreen.SetActive(false);
            plotArrow.SetActive(false);
            StartCoroutine(PlotWait2());
        }
    }

    private IEnumerator PlotWait1()
    {
        state = States.wait;
        yield return new WaitForSeconds(1);
        plotViewScreen.SetActive(true);
        plotArrow.SetActive(true);
        state = States.plotView;
    }

    private IEnumerator PlotWait2()
    {
        state = States.wait;
        yield return new WaitForSeconds(1);
        tutorialSound.Play();
        concludeScreen.SetActive(true);
    }

    public void ConcludeContinue()
    {
        clickSound.Play();
        gameObject.SetActive(false);
    }
}

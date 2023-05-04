using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public enum BuildType
{
    Path, Amenity, Decor, NewPlot, None
}
public class BuildToggleHandler : MonoBehaviour
{
    [Header("Pop Out Panel")]
    public GameObject popoutPanel;

    [Header("Selection Panel")]
    public GameObject selectionPanel;

    private ToggleGroup toggleGroup;
    private GameObject typeObject;
    private BuildType currentBuildType;

    void Start()
    {
        toggleGroup = GetComponent<ToggleGroup>();
        currentBuildType = BuildType.None;
        selectionPanel.SetActive(false);
    }

    public void UpdateBuildingType(ButtonBuildType buttonBuildType)
    {
        typeObject = buttonBuildType.gameObject;

        if (toggleGroup.ActiveToggles().FirstOrDefault() == null)
        {
            currentBuildType = BuildType.None;
        }
        else
        {
            currentBuildType = buttonBuildType.buildType;
        }

        HandlePopOut(buttonBuildType);
        HandleSelection(buttonBuildType);
    }

    private void HandlePopOut(ButtonBuildType buttonBuildType)
    {
        if (!buttonBuildType.popout)
        {
            if (popoutPanel.GetComponent<AnimateBuildingPopOut>().IsPopOutActive())
            {
                popoutPanel.GetComponent<AnimateBuildingPopOut>().UpdateAnimation(false);
            }

            return;
        }

        if (currentBuildType != BuildType.None)
        {
            popoutPanel.GetComponent<AnimateBuildingPopOut>().UpdateAnimation(true);
        }
        else
        {
            popoutPanel.GetComponent<AnimateBuildingPopOut>().UpdateAnimation(false);
        }
    }

    private void HandleSelection(ButtonBuildType buttonBuildType)
    {
        if (currentBuildType != BuildType.None)
        {
            selectionPanel.SetActive(true);
            selectionPanel.transform.position = buttonBuildType.transform.position;
        }
        else
        {
            selectionPanel.SetActive(false);
        }
    }

    public GameObject GetSelectionLocation()
    {
        if (currentBuildType == BuildType.None)
        {
            return null;
        }
        else
        {
            return typeObject;
        }
    }

    public void AllTogglesOff()
    {
        toggleGroup.SetAllTogglesOff();
    }
}
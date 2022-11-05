using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BuildingModes
{
    public BuildingModes(bool initialVal)
    {
        buildMode = initialVal;
        pathBuilding = initialVal;
    }
    public bool buildMode;
    public bool pathBuilding;
}

[RequireComponent(typeof(PathBuilder))]
public class PlayerBuilding : MonoBehaviour
{
    [System.Serializable]
    public struct UIPanels
    {
        public CanvasRenderer helpTextPanel;
        public CanvasRenderer buildTypePanel;
        public CanvasRenderer buildPanel;
    }
    public UIPanels interfacePanels;

    private Button[] buildTypeButtons;
    private TextMeshProUGUI buildButtonText;

    public BuildingModes buildingModes = new BuildingModes(false);

    void Start()
    {
        // Get text from build button
        GameObject buildButton = interfacePanels.buildPanel.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
        buildButtonText = buildButton.GetComponent<TextMeshProUGUI>();

        List<Button> buttons = new List<Button>();
        foreach (Transform child in interfacePanels.buildTypePanel.transform)
        {
            buttons.Add(child.GetComponent<Button>());
        }

        buildTypeButtons = buttons.ToArray();
    }

    public void ToggleBuild()
    {
        buildingModes.buildMode = !buildingModes.buildMode;

        if (!buildingModes.buildMode)
        {
            if (buildingModes.pathBuilding)
            {
                TogglePathBuilding();
            }
        }
        
        // Update build UI
        IEnumerator[] uiEnumerators = AnimateBuildUI.AnimateShowTypeButtons(buildTypeButtons, buildingModes.buildMode);
        for (int i = 0; i < uiEnumerators.Length; i++)
        {
            StartCoroutine(uiEnumerators[i]);
        }

        AnimateBuildUI.AnimateBuildTip(interfacePanels.helpTextPanel, buildingModes.buildMode);
        AnimateBuildUI.AnimateBuildButton(interfacePanels.buildPanel, buildButtonText, buildingModes.buildMode);
    }
    public void TogglePathBuilding()
    {
        // Update UI elements
        if (!buildingModes.pathBuilding)
        {
            AnimateBuildUI.AnimateSelectTypeButton(buildTypeButtons, "PathsButton", "Color Layer.EnableBuildType");
        }
        else
        {
            AnimateBuildUI.AnimateSelectTypeButton(buildTypeButtons, "PathsButton", "Color Layer.DisableBuildType");
        }
        buildingModes.pathBuilding = !buildingModes.pathBuilding;
    }
}

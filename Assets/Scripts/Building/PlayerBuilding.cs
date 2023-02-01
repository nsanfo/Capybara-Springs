using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingModes
{
    public bool enableBuild, enablePath, enableAmenities, enableDecor;

    public BuildingModes()
    {
        enableBuild = false;
        enablePath = false;
        enableAmenities = false;
				enableDecor = false;
    }
}

public class MouseRaycast
{
    private RaycastHit hitInfo;
    private bool hit;

    public void UpdateRaycast(RaycastHit inputRaycast, bool inputHit)
    {
        hitInfo = inputRaycast;
        hit = inputHit;
    }

    public bool CheckHit()
    {
        return hit;
    }

    public Vector3 GetPosition()
    {
        return new Vector3(hitInfo.point.x, 0, hitInfo.point.z);
    }
}

[RequireComponent(typeof(PathBuilder))]
public class PlayerBuilding : MonoBehaviour
{
    // Interface variables
    [System.Serializable]
    public struct UIBuilderPanel
    {
        public CanvasRenderer helpTextPanel, buildTypePanel, buildPanel;
    }
    public UIBuilderPanel interfacePanels;

    // Building type buttons
    private Button[] buildTypeButtons;
    private TextMeshProUGUI buildButtonText;

    // Building modes
    public BuildingModes buildingModes = new BuildingModes();

    // Mouse raycast
    public MouseRaycast mouseRaycast = new MouseRaycast();

    void Start()
    {
        // Get text from build button
        GameObject buildButton = interfacePanels.buildPanel.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
        buildButtonText = buildButton.GetComponent<TextMeshProUGUI>();

        // Get the list of building type buttons
        List<Button> buttons = new List<Button>();
        foreach (Transform child in interfacePanels.buildTypePanel.transform)
        {
            buttons.Add(child.GetComponent<Button>());
        }
        buildTypeButtons = buttons.ToArray();
    }

    void Update()
    {
        // Raycast to mouse position
        RaycastHit hitInfo = new RaycastHit();
        bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

        // Update mouse raycast
        if (EventSystem.current.IsPointerOverGameObject()) return;
        mouseRaycast.UpdateRaycast(hitInfo, hit);
    }

    public void ToggleBuild()
    {
        buildingModes.enableBuild = !buildingModes.enableBuild;

        if (!buildingModes.enableBuild)
        {
            if (buildingModes.enablePath)
            {
                TogglePathBuilding();
            }
            if (buildingModes.enableAmenities)
            {
                ToggleAmenitiesBuilding();
            }
						if (buildingModes.enableDecor)
            {
                ToggleDecorBuilding();
            }
        }

        // Animate build UI
        // Animate building type buttons
        IEnumerator[] uiEnumerators = AnimateBuildUI.AnimateShowTypeButtons(buildTypeButtons, buildingModes.enableBuild);
        for (int i = 0; i < uiEnumerators.Length; i++)
        {
            StartCoroutine(uiEnumerators[i]);
        }

        // Animate building tip and button
        AnimateBuildUI.AnimateBuildTip(interfacePanels.helpTextPanel, buildingModes.enableBuild);
        AnimateBuildUI.AnimateBuildButton(interfacePanels.buildPanel, buildButtonText, buildingModes.enableBuild);
    }

    public void ToggleAmenitiesBuilding()
    {
        buildingModes.enableAmenities = !buildingModes.enableAmenities;
        
        if (buildingModes.enablePath)
        {
            buildingModes.enablePath = false;
            AnimateBuildUI.AnimateSelectTypeButton(buildTypeButtons, "PathsButton", buildingModes.enablePath);
            gameObject.GetComponent<PathBuilder>().pathHelper = new PathHelper();
            Destroy(gameObject.GetComponent<PathGuide>());
        }
				else if (buildingModes.enableDecor)
        {
            buildingModes.enableDecor = false;
            AnimateBuildUI.AnimateSelectTypeButton(buildTypeButtons, "DecorButton", buildingModes.enableDecor);
            var decorObject = GameObject.Find("Canvas").transform.Find("DecorOptions").gameObject;
            decorObject.SetActive(false);
            var blueprint = GameObject.FindGameObjectWithTag("Blueprint");
            if(blueprint != null)
                Destroy(blueprint);
        }
        else
        {
            var blueprint = GameObject.FindGameObjectWithTag("Blueprint");
            Destroy(blueprint);
        }

        var amenitiesObject = GameObject.Find("Canvas").transform.Find("AmenitiesOptions").gameObject;
        bool activeState = amenitiesObject.activeSelf;
        amenitiesObject.SetActive(!activeState);

        // Animate amenities UI
        AnimateBuildUI.AnimateSelectTypeButton(buildTypeButtons, "AmenitiesButton", buildingModes.enableAmenities);
    }

    public void TogglePathBuilding()
    {
        buildingModes.enablePath = !buildingModes.enablePath;
        
        if (buildingModes.enableAmenities)
        {
            buildingModes.enableAmenities = false;
            AnimateBuildUI.AnimateSelectTypeButton(buildTypeButtons, "AmenitiesButton", buildingModes.enableAmenities);
            var amenitiesObject = GameObject.Find("Canvas").transform.Find("AmenitiesOptions").gameObject;
            amenitiesObject.SetActive(false);
            var blueprint = GameObject.FindGameObjectWithTag("Blueprint");
            if(blueprint != null)
                Destroy(blueprint);
        }
				else if (buildingModes.enableDecor)
        {
            buildingModes.enableDecor = false;
            AnimateBuildUI.AnimateSelectTypeButton(buildTypeButtons, "DecorButton", buildingModes.enableDecor);
            var decorObject = GameObject.Find("Canvas").transform.Find("DecorOptions").gameObject;
            decorObject.SetActive(false);
            var blueprint = GameObject.FindGameObjectWithTag("Blueprint");
            if(blueprint != null)
                Destroy(blueprint);
        }

        if (buildingModes.enablePath)
        {
            gameObject.AddComponent<PathGuide>();
        }
        else
        {
            gameObject.GetComponent<PathBuilder>().pathHelper = new PathHelper();
            Destroy(gameObject.GetComponent<PathGuide>());
        }

        // Animate path UI
        AnimateBuildUI.AnimateSelectTypeButton(buildTypeButtons, "PathsButton", buildingModes.enablePath);
    }

		public void ToggleDecorBuilding()
    {
        buildingModes.enableDecor = !buildingModes.enableDecor;
        
        if (buildingModes.enablePath)
        {
            buildingModes.enablePath = false;
            AnimateBuildUI.AnimateSelectTypeButton(buildTypeButtons, "PathsButton", buildingModes.enablePath);
            gameObject.GetComponent<PathBuilder>().pathHelper = new PathHelper();
            Destroy(gameObject.GetComponent<PathGuide>());
        }
				else if (buildingModes.enableAmenities)
        {
            buildingModes.enableAmenities = false;
            AnimateBuildUI.AnimateSelectTypeButton(buildTypeButtons, "AmenitiesButton", buildingModes.enableAmenities);
            var amenitiesObject = GameObject.Find("Canvas").transform.Find("AmenitiesOptions").gameObject;
            amenitiesObject.SetActive(false);
            var blueprint = GameObject.FindGameObjectWithTag("Blueprint");
            if(blueprint != null)
                Destroy(blueprint);
        }
        else
        {
            var blueprint = GameObject.FindGameObjectWithTag("Blueprint");
            Destroy(blueprint);
        }

        var decorObject = GameObject.Find("Canvas").transform.Find("DecorOptions").gameObject;
        bool activeState = decorObject.activeSelf;
        decorObject.SetActive(!activeState);

        // Animate decor UI
        AnimateBuildUI.AnimateSelectTypeButton(buildTypeButtons, "DecorButton", buildingModes.enableDecor);
    }
}

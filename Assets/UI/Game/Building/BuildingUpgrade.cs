using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingUpgrade : MonoBehaviour
{
    // Building modes
    public BuildingModes buildingModes = new BuildingModes();

    // Mouse raycast
    public MouseRaycast mouseRaycast = new MouseRaycast();

    private void Update()
    {
        // Raycast to mouse position
        RaycastHit hitInfo = new RaycastHit();
        bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

        // Update mouse raycast
        if (EventSystem.current.IsPointerOverGameObject()) return;
        mouseRaycast.UpdateRaycast(hitInfo, hit);
    }

    public void ToggleAmenityBuilding()
    {
        buildingModes.enableAmenities = !buildingModes.enableAmenities;

        HandlePrevPath();
        HandlePrevDecor();
        HandlePrevPlots();
    }

    public void ToggleDecorBuilding()
    {
        buildingModes.enableDecor = !buildingModes.enableDecor;

        HandlePrevAmenity();
        HandlePrevPath();
        HandlePrevPlots();
    }

    public void TogglePathBuilding()
    {
        buildingModes.enablePath = !buildingModes.enablePath;

        if (buildingModes.enablePath)
        {
            gameObject.AddComponent<PathGuide>();
        }
        else
        {
            gameObject.GetComponent<PathBuilder>().pathHelper = new PathHelper();
            Destroy(gameObject.GetComponent<PathGuide>());
        }

        HandlePrevAmenity();
        HandlePrevDecor();
        HandlePrevPlots();
    }

    public void TogglePlotPurchasing()
    {
        buildingModes.enablePlots = !buildingModes.enablePlots;

        PlotBuyer plotBuyer = gameObject.GetComponent<PlotBuyer>();
        if (plotBuyer != null)
        {
            if (buildingModes.enablePlots)
            {
                plotBuyer.CameraZoomOut();
                plotBuyer.InstantiatePurchaseSprites();
            }
            else
            {
                plotBuyer.CameraZoomBack();
                plotBuyer.RemovePurchaseSprites();
            }
        }

        HandlePrevAmenity();
        HandlePrevDecor();
        HandlePrevPath();
    }

    private void HandlePrevPath()
    {
        if (buildingModes.enablePath) return;
        buildingModes.enablePath = false;

        gameObject.GetComponent<PathBuilder>().pathHelper = new PathHelper();
        Destroy(gameObject.GetComponent<PathGuide>());
    }

    private void HandlePrevAmenity()
    {
        if (buildingModes.enableAmenities) return;
        buildingModes.enableAmenities = false;

        HandleBlueprints();
    }

    private void HandlePrevDecor()
    {
        if (buildingModes.enableDecor) return;
        buildingModes.enableDecor = false;

        HandleBlueprints();
    }

    private void HandlePrevPlots()
    {
        if (buildingModes.enablePlots) return;
        buildingModes.enablePlots = false;

        if (gameObject.TryGetComponent<PlotBuyer>(out var plotBuyer))
        {
            plotBuyer.CameraZoomBack();
            plotBuyer.RemovePurchaseSprites();
        }
    }

    private void HandleBlueprints()
    {
        GameObject blueprint = GameObject.FindGameObjectWithTag("Blueprint");
        if (blueprint == null) return;
        Destroy(blueprint);
    }
}

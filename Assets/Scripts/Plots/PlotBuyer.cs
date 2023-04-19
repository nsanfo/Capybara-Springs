using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlotBuyer : MonoBehaviour
{
    private Vector3 originalPos = Vector3.positiveInfinity, plotPos = Vector3.positiveInfinity;
    private Quaternion originalRot = Quaternion.identity, plotRot = Quaternion.identity;

    private Vector3 targetPos = new Vector3(5, 12, 5);
    private Quaternion targetRot = Quaternion.Euler(new Vector3(90, -90, 0));

    private float duration = 0.75f, elapsedTime;
    public bool cameraAnimation = false;
    private bool zoom = true;

    // Mouse raycast
    public MouseRaycast mouseRaycast = new MouseRaycast();

    // Building variables
    private BuildingModes buildingModes;

    // Camera
    private GameObject cameraObject;

    // Start is called before the first frame update
    void Start()
    {
        PlayerBuilding buildingScript = gameObject.GetComponent<PlayerBuilding>();

        // Get building modes from building script
        buildingModes = buildingScript.buildingModes;

        // Get raycast from building script
        mouseRaycast = buildingScript.mouseRaycast;

        GameObject camera = GameObject.Find("Camera/Main Camera");
        if (camera != null)
        {
            cameraObject = camera;
        }
    }

    // Update is called once per frame
    void Update()
    {
        CameraAnimation();

        if (!buildingModes.enablePlots) return;
    }

    private void CameraAnimation()
    {
        if (!cameraAnimation) return;

        if (originalPos == Vector3.positiveInfinity || originalRot == Quaternion.identity)
        {
            cameraAnimation = false;
            return;
        }

        elapsedTime += Time.deltaTime;
        float percentageComplete = elapsedTime / duration;

        if (zoom)
        {
            cameraObject.transform.position = Vector3.Lerp(originalPos, targetPos, Mathf.SmoothStep(0, 1, percentageComplete));
            cameraObject.transform.rotation = Quaternion.Lerp(originalRot, targetRot, Mathf.SmoothStep(0, 1, percentageComplete));
            if (percentageComplete > 1)
            {
                cameraAnimation = false;
                elapsedTime = 0;
            }
        }
        else
        {
            cameraObject.transform.position = Vector3.Lerp(plotPos, originalPos, Mathf.SmoothStep(0, 1, percentageComplete));
            cameraObject.transform.rotation = Quaternion.Lerp(plotRot, originalRot, Mathf.SmoothStep(0, 1, percentageComplete));
            if (percentageComplete > 1)
            {
                cameraAnimation = false;
                elapsedTime = 0;
            }
        }
    }

    public void CameraZoomOut()
    {
        if (cameraObject.TryGetComponent<CameraControl>(out var cameraControl))
        {
            if (cameraControl.plotCamera)
            {
                return;
            }

            cameraControl.plotCamera = true;
        }

        originalPos = cameraObject.transform.position;
        originalRot = cameraObject.transform.rotation;

        cameraAnimation = true;
        zoom = true;
    }

    public void CameraZoomBack()
    {
        if (cameraObject.TryGetComponent<CameraControl>(out var cameraControl))
        {
            if (!cameraControl.plotCamera)
            {
                return;
            }

            cameraControl.plotCamera = false;
        }

        plotPos = cameraObject.transform.position;
        plotRot = cameraObject.transform.rotation;

        cameraAnimation = true;
        zoom = false;
    }
}

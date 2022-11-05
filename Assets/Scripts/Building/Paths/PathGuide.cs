using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.EventSystems;

public class PathGuide : MonoBehaviour
{
    // Guide names
    public string guideHandlerName = "GuideHandler";
    private string guideMouseName = "GuideMouse";
    private string guidePointName = "GuidePoint";
    public string guidePathName = "GuidePath";

    // Guide objects
    private GameObject guideHandlerObject;
    private GameObject guideMouseObject;
    private GameObject guidePointObject;
    private GameObject guidePathObject;

    // Guide spacing variables
    private float pointSpacing;
    private float resolution;

    // Guide path variables
    private float spacing;
    private float pathWidth;
    private float offset;

    // Guide materials
    private Material guideMaterial;
    private Material guideDisabledMaterial;

    // Guide raycast/endpoints
    private (Vector3, Vector3) endpoints;
    private Vector3 raycastPosition;

    private BuildingModes buildingModes;
    private bool buildable = true;

    void Start()
    {
        PathBuilder pathBuilder = gameObject.GetComponent<PathBuilder>();

        // Get raycast/endpoints from builder
        //endpoints = pathBuilder.endpoints;

        // Get spacing variables from builder
        pointSpacing = pathBuilder.pointSpacing;
        resolution = pathBuilder.resolution;

        // Get guide path variables from builder
        spacing = pathBuilder.spacing;
        pathWidth = pathBuilder.pathWidth;
        offset = pathBuilder.offset;

        // Get guide materials from builder
        guideMaterial = pathBuilder.guideMaterial;
        guideDisabledMaterial = pathBuilder.guideDisabledMaterial;

        // Get building modes class
        buildingModes = gameObject.GetComponent<PlayerBuilding>().buildingModes;
}

    void Update()
    {
        // Check building
        if (!buildingModes.pathBuilding)
        {
            // Destroy handler on no build
            if (guideHandlerObject != null)
            {
                Destroy(guideHandlerObject);
                guideHandlerObject = null;
            }
            return;
        }
        else
        {
            // Create handler on build
            if (guideHandlerObject == null)
            {
                guideHandlerObject = new GameObject(guideHandlerName);
                guideHandlerObject.transform.SetParent(gameObject.transform);
            }
        }

        // Check if pointer is over ui
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // Return if no raycast hit
        if (!UpdateRaycast()) return;

        #region GuideMouse
        // Initialize and update position of the guide mouse
        if (guideMouseObject == null)
        {
            InitializeGuideMouse();
        }
        else
        {
            UpdateGuideMouse();
        }
        #endregion

        #region InputLeftMouse
        // Placing points and building paths
        if (Input.GetMouseButtonDown(0))
        {
            // Set first point
            if (endpoints.Item1 == Vector3.zero)
            {
                endpoints.Item1 = raycastPosition;
                InitializeGuidePath((endpoints.Item1, raycastPosition));
                InitializeGuidePoint(endpoints.Item1);
            }
            else
            {
                if (endpoints.Item2 == Vector3.zero && buildable)
                {
                    DestroyGuidePath();
                    DestroyGuidePoint();
                    endpoints = (Vector3.zero, Vector3.zero);
                }
            }
        }
        #endregion

        #region InputRightMouse
        // Cancel building paths
        else if (Input.GetMouseButtonDown(1))
        {
            if (endpoints.Item1 != Vector3.zero)
            {
                DestroyGuidePath();
                DestroyGuidePoint();
                endpoints.Item1 = Vector3.zero;
            }
        }
        #endregion

        #region UpdateGuidePath
        if (endpoints.Item1 != Vector3.zero)
        {
            UpdateGuidePath((endpoints.Item1, raycastPosition));
        }
        #endregion

        #region CheckPathCollision
        // Check for collision and update guide material
        buildable = !PathUtilities.CheckForCollision(gameObject, guideHandlerName + "/" + guidePathName + "/Collisions");

        if (guidePathObject != null)
        {
            if (buildable)
            {
                guidePathObject.GetComponent<Renderer>().material = guideMaterial;
            }
            else
            {
                guidePathObject.GetComponent<Renderer>().material = guideDisabledMaterial;
            }
        }
        #endregion
    }

    void InitializeGuideMouse()
    {
        // Create guide mouse object
        GameObject guideObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        guideObject.name = guideMouseName;
        guideObject.GetComponent<Collider>().enabled = false;

        // Set transforms for guide mouse
        guideObject.transform.localScale += new Vector3(-0.6f, -0.9f, -0.6f);

        // Set parent for guide mouse
        guideMouseObject = guideObject;
        guideMouseObject.transform.SetParent(guideHandlerObject.transform);
    }

    void UpdateGuideMouse()
    {
        guideMouseObject.transform.position = raycastPosition;
    }

    void InitializeGuidePoint(Vector3 initialPoint)
    {
        // Create guide point object
        GameObject guideObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        guideObject.name = guidePointName;
        guideObject.GetComponent<Collider>().enabled = false;

        // Set transforms for guide point
        guideObject.transform.localScale += new Vector3(-0.6f, -0.9f, -0.6f);
        guideObject.transform.position = initialPoint;

        // Set parent for guide point
        guideObject.transform.SetParent(guideHandlerObject.transform);

        // Set rendering
        guideObject.GetComponent<Renderer>().material = guideMaterial;

        guidePointObject = guideObject;
    }

    void DestroyGuidePoint()
    {
        Destroy(guidePointObject);
        guidePointObject = null;
    }

    void InitializeGuidePath((Vector3, Vector3) guideEndpoints)
    {
        // Create guide path object
        GameObject guideObject = new GameObject(guidePathName);

        // Set parent for guide path
        guideObject.transform.SetParent(guideHandlerObject.transform);

        // Get offset spaced points on path
        Vector3 offsetVector = new Vector3(0, offset + 0.01f, 0);
        (Vector3, Vector3) offsetEndPoints = (guideEndpoints.Item1 + offsetVector, guideEndpoints.Item2 + offsetVector);

        // Add path component for drawing mesh
        guideObject.AddComponent<Path>();
        Path pathComponent = guideObject.GetComponent<Path>();
        pathComponent.endpoints = guideEndpoints;
        pathComponent.spacedPoints = PathUtilities.CalculateEvenlySpacedPoints(offsetEndPoints, pointSpacing, resolution);
        pathComponent.SetMesh(pathWidth);
        pathComponent.CreateCollision("GuideCollider", true);
        pathComponent.SetRendering(guideMaterial, spacing);

        guidePathObject = guideObject;
    }

    void DestroyGuidePath()
    {
        Destroy(guidePathObject);
        guidePathObject = null;
    }

    void UpdateGuidePath((Vector3, Vector3) guideEndpoints)
    {
        // Get offset spaced points on path
        Vector3 offsetVector = new Vector3(0, offset + 0.01f, 0);
        (Vector3, Vector3) offsetEndPoints = (guideEndpoints.Item1 + offsetVector, guideEndpoints.Item2 + offsetVector);

        // Add path component for drawing mesh
        Path pathComponent = guidePathObject.GetComponent<Path>();
        pathComponent.endpoints = guideEndpoints;
        pathComponent.spacedPoints = PathUtilities.CalculateEvenlySpacedPoints(offsetEndPoints, pointSpacing, resolution);
        pathComponent.UpdateMesh(pathWidth);
        pathComponent.DestroyCollison();
        pathComponent.CreateCollision("GuideCollider", true);
    }

    bool UpdateRaycast()
    {
        // Raycast to mouse position
        RaycastHit hitInfo = new RaycastHit();
        bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

        // Return false if no raycast hit
        if (!hit) return false;

        raycastPosition = new Vector3(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z);

        // Return true if successful raycast
        return true;
    }
}

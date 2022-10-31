using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PathGuide : MonoBehaviour
{
    // Guide names
    private string guideMouseName = "GuideMouse";
    private string guidePointName = "GuidePoint";
    private string guidePathName = "GuidePath";
    private string guidePathPointName = "GuidePathPoint";

    private GameObject pathBuilderSelf;

    private (Vector3, Vector3) points;
    private Vector3 initialGuidePoint;
    private bool buildMode = false;

    // Guide Objects
    private GameObject guideMouseObject;
    private GameObject guidePointObject;
    private GameObject guidePathObject;

    // Guide material
    private Material guideMaterial;

    // Path builder vars
    PathBuilder pathBuilderScript;
    private GameObject buildingObject;

    void Start()
    {
        pathBuilderSelf = gameObject;
        pathBuilderScript = pathBuilderSelf.GetComponent<PathBuilder>();
        buildingObject = pathBuilderScript.buildingObject;
        guideMaterial = pathBuilderScript.guideMaterial;
    }

    void Update()
    {
        pathBuilderScript = pathBuilderSelf.GetComponent<PathBuilder>();
        

        // Check for build mode
        if (!buildMode) return;

        // Disable guide over UI
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // Raycast to mouse position
        RaycastHit hitInfo = new RaycastHit();
        bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
        var position = new Vector3(hitInfo.point.x, hitInfo.point.y - 0.06f, hitInfo.point.z);
        if (!hit) return;

        #region GuideMouse
        if (guideMouseObject == null)
        {
            CreateGuideMouse();
        }
        else
        {
            if (hitInfo.collider.gameObject == buildingObject)
            {
                UpdateGuideMouse(hitInfo);
            }
        }
        #endregion

        #region InputLeftMouse
        if (Input.GetMouseButtonDown(0))
        {
            // Check if first point is zero
            if (initialGuidePoint == Vector3.zero)
            {
                initialGuidePoint = position;
                CreateGuidePoint(position);
            }
            else
            {
                initialGuidePoint = Vector3.zero;
                DeleteGuidePoint();
                DeleteGuidePath();
            }
        }
        #endregion

        #region InputRightMouse
        if (Input.GetMouseButtonDown(1))
        {
            // Check if first point is zero
            if (initialGuidePoint != Vector3.zero)
            {
                initialGuidePoint = Vector3.zero;
                DeleteGuidePoint();
                DeleteGuidePath();
            }
        }
        #endregion

        #region GuidePath
        if ((initialGuidePoint != Vector3.zero))
        {
            UpdateGuidePath(position);
        }
        #endregion
    }

    public void ToggleBuild()
    {
        buildMode = !buildMode;
        if (buildMode)
        {
            CreateGuideMouse();
        }
        else
        {
            initialGuidePoint = Vector3.zero;
            DeleteGuideMouse();
            DeleteGuidePoint();
            DeleteGuidePath();
        }
    }

    void CreateGuideMouse()
    {
        if (guideMouseObject != null) Destroy(guideMouseObject);

        // Create guide mouse object
        GameObject guideObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        guideObject.name = guideMouseName;
        guideObject.GetComponent<Collider>().enabled = false;

        // Set transforms for guide mouse
        guideObject.transform.localScale += new Vector3(-0.6f, -0.9f, -0.6f);
        guideObject.transform.position = new Vector3(0f, -2.0f, 0f);

        // Set parent for guide mouse
        guideMouseObject = guideObject;
        guideMouseObject.transform.SetParent(pathBuilderSelf.transform);
    }

    void UpdateGuideMouse(RaycastHit raycast)
    {
        guideMouseObject.transform.position = new Vector3(raycast.point.x, raycast.point.y - 0.06f, raycast.point.z);
    }

    void DeleteGuideMouse()
    {
        if (guideMouseObject != null) Destroy(guideMouseObject);
    }

    void CreateGuidePoint(Vector3 position)
    {
        if (guidePointObject != null) Destroy(guidePointObject);

        // Create guide point object
        GameObject guideObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        guideObject.name = guidePointName;
        guideObject.GetComponent<Collider>().enabled = false;

        // Set transforms for guide point
        guideObject.transform.localScale += new Vector3(-0.7f, -0.9f, -0.7f);
        guideObject.transform.position = position;

        // Set material for guide point
        guideObject.GetComponent<Renderer>().material = guideMaterial;

        // Set parent for guide point
        guidePointObject = guideObject;
        guidePointObject.transform.SetParent(pathBuilderSelf.transform);
    }

    void DeleteGuidePoint()
    {
        if (guidePointObject != null) Destroy(guidePointObject);
    }

    void UpdateGuidePath(Vector3 mousePosition)
    {
        if (guidePathObject != null) Destroy(guidePathObject);

        // Create guide path object
        GameObject guideObject = new GameObject();
        guideObject.name = guidePathName;

        // Set parent for guide path
        guidePathObject = guideObject;
        guidePathObject.transform.SetParent(pathBuilderSelf.transform);

        // Get spaced points on path
        (Vector3, Vector3) pathGuideTuple = (initialGuidePoint, mousePosition);
        Vector3[] spacedGuidePoints = PathUtilities.CalculateEvenlySpacedPoints(pathGuideTuple, 0.5f, 1);

        // Create object points on path
        for (int i = 0; i < spacedGuidePoints.Length; i++)
        {
            // Create guide path point object
            GameObject pathPointObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pathPointObject.name = guidePathPointName + (i + 1);
            pathPointObject.GetComponent<Collider>().enabled = false;

            // Set transform for guide path point
            pathPointObject.transform.localScale += new Vector3(-0.85f, -0.85f, -0.85f);
            pathPointObject.transform.position = spacedGuidePoints[i];

            // Set material for guide path point
            pathPointObject.GetComponent<Renderer>().material = guideMaterial;

            // Set parent for guide path point
            pathPointObject.transform.SetParent(guidePathObject.transform);
        }
    }

    void DeleteGuidePath()
    {
        if (guidePathObject != null) Destroy(guidePathObject);
    }
}

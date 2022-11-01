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
    //private string guidePathPointName = "GuidePathPoint";

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
    private PathBuilder pathBuilderScript;
    private GameObject buildingObject;
    private float spacing;
    private float pathWidth;
    private float offset;

    void Start()
    {
        pathBuilderScript = gameObject.GetComponent<PathBuilder>();
        buildingObject = pathBuilderScript.buildingObject;
        guideMaterial = pathBuilderScript.guideMaterial;
        spacing = pathBuilderScript.spacing;
        pathWidth = pathBuilderScript.pathWidth;
        offset = pathBuilderScript.offset;
    }

    void Update()
    {
        pathBuilderScript = gameObject.GetComponent<PathBuilder>();
        

        // Check for build mode
        if (!buildMode) return;

        // Disable guide over UI
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // Raycast to mouse position
        RaycastHit hitInfo = new RaycastHit();
        bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
        var position = new Vector3(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z);
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
        guideMouseObject.transform.SetParent(gameObject.transform);
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
        guideObject.transform.position = position + new Vector3(0, offset, 0);

        // Set material for guide point
        guideObject.GetComponent<Renderer>().material = guideMaterial;

        // Set parent for guide point
        guidePointObject = guideObject;
        guidePointObject.transform.SetParent(gameObject.transform);
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
        guidePathObject.transform.SetParent(gameObject.transform);

        // Get spaced points on path
        (Vector3, Vector3) pathGuideTuple = (initialGuidePoint, mousePosition);
        float pointSpacing = pathBuilderScript.pointSpacing;
        float resolution = pathBuilderScript.resolution;
        //Vector3[] spacedGuidePoints = PathUtilities.CalculateEvenlySpacedPoints(pathGuideTuple, spacing, resolution);

        // Set points along the path
        (Vector3, Vector3) offsetPoints = (pathGuideTuple.Item1 + new Vector3(0, offset, 0), pathGuideTuple.Item2 + new Vector3(0, offset, 0));
        Vector3[] evenPoints = PathUtilities.CalculateEvenlySpacedPoints(offsetPoints, pointSpacing, resolution);

        // Add components to draw mesh
        guidePathObject.AddComponent<PathCreator>();
        guidePathObject.AddComponent<MeshFilter>();
        guidePathObject.AddComponent<MeshRenderer>();
        guidePathObject.GetComponent<PathCreator>().UpdatePath(evenPoints, pathWidth);

        // Set material for guide path
        if (guideMaterial != null) guidePathObject.GetComponent<Renderer>().material = guideMaterial;
    }

    void DeleteGuidePath()
    {
        if (guidePathObject != null) Destroy(guidePathObject);
    }
}

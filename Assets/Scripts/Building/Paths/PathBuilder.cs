using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PathGuide))]
public class PathBuilder : MonoBehaviour
{
    [Header("Builder Variables")]
    public GameObject buildingObject;

    [Space(10)]
    [Header("Point Variables")]
    [Range(0.1f, 0.5f)]
    public float pointSpacing = 0.1f;
    public float resolution = 1f;
    public bool visualizePoints = false;

    [Space(10)]
    [Header("Path Variables")]
    [Range(0.5f, 1.5f)]
    public float spacing = 1;
    public float pathWidth = 1.0f;
    public float offset = 0.01f;

    [Space(10)]
    [Header("Textures")]
    public Material guideMaterial;
    public Material guideDisabledMaterial;
    public Material pathMaterial;

    [Space(10)]
    [Header("Nodes")]
    public float nodeRange = 10.0f;
    public GameObject nodePrefab;
    public RuntimeAnimatorController nodeAnimatorController;

    // Object to hold paths
    private GameObject pathsObject;

    // Array to hold paths
    private GameObject[] paths;

    // Array to hold paths
    private GameObject[] nodes;

    // Building raycast/endpoints
    public (Vector3, Vector3) endpoints;
    private Vector3 raycastPosition;

    // Guide variables
    private string guideHandlerName;
    private string guidePathName;

    // Building Variables
    private BuildingModes buildingModes;
    private bool buildable = true;

    void Start()
    {
        // Get building modes class
        buildingModes = gameObject.GetComponent<PlayerBuilding>().buildingModes;

        pathsObject = GameObject.Find("Paths");
        if (pathsObject == null)
        {
            pathsObject = new GameObject();
            pathsObject.name = "Paths";
        }

        // Guide variables
        PathGuide pathGuide = gameObject.GetComponent<PathGuide>();
        guideHandlerName = pathGuide.guideHandlerName;
        guidePathName = pathGuide.guidePathName;
    }

    void Update()
    {
        // Check building
        if (!buildingModes.pathBuilding) return;

        // Check if pointer is over ui
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // Return if no raycast hit
        if (!UpdateRaycast()) return;

        #region InputLeftMouse
        // Placing points and building paths
        if (Input.GetMouseButtonDown(0))
        {
            // Set first point
            if (endpoints.Item1 == Vector3.zero)
            {
                endpoints.Item1 = raycastPosition;
            }
            // Set second point
            else
            {
                if (buildable)
                {
                    endpoints.Item2 = raycastPosition;
                    CreatePath();
                    endpoints.Item1 = Vector3.zero;
                    endpoints.Item2 = Vector3.zero;
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
                endpoints.Item1 = Vector3.zero;
            }
        }
        #endregion

        if (nodes != null)
        {
            TrackNearbyNodes(raycastPosition);
        }

        buildable = !PathUtilities.CheckForCollision(gameObject, guideHandlerName + "/" + guidePathName + "/Collisions");
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

    void CreatePath()
    {
        // Create a new path object
        GameObject path = new GameObject("Path");

        // Set parent for path object
        path.transform.SetParent(pathsObject.transform);

        // Get offset spaced points on path
        Vector3 offsetVector = new Vector3(0, offset, 0);
        (Vector3, Vector3) offsetEndPoints = (endpoints.Item1 + offsetVector, endpoints.Item2 + offsetVector);

        // Get evenly spaced points between endpoints
        Vector3[] spacedPoints = PathUtilities.CalculateEvenlySpacedPoints(offsetEndPoints, pointSpacing, resolution);

        // Add path component for drawing mesh
        path.AddComponent<Path>();
        Path pathComponent = path.GetComponent<Path>();
        pathComponent.endpoints = offsetEndPoints;
        pathComponent.spacedPoints = spacedPoints;
        pathComponent.SetMesh(pathWidth);
        pathComponent.CreateCollision("PathCollider", false);
        pathComponent.InitializeNodes(nodePrefab, nodeAnimatorController);
        pathComponent.SetRendering(pathMaterial, spacing);

        // Add new path to list
        List<GameObject> pathList = new List<GameObject>();
        if (paths != null) pathList.AddRange(paths);
        pathList.Add(path);
        paths = pathList.ToArray();
        UpdateNodes();
    }

    void UpdateNodes()
    {
        List<GameObject> currentNodes = new List<GameObject>();

        for (int i = 0; i < paths.Length; i++)
        {
            currentNodes.AddRange(paths[i].GetComponent<Path>().GetNodes());
        }

        nodes = currentNodes.ToArray();
    }

    void TrackNearbyNodes(Vector3 mousePosition)
    {
        PathNode currNode;
        for (int i = 0; i < nodes.Length; i++)
        {
            currNode = nodes[i].GetComponent<PathNode>();

            Vector3 offset = currNode.transform.position - mousePosition;
            float sqrLen = offset.sqrMagnitude;

            if (sqrLen < nodeRange * nodeRange)
            {
                if (!currNode.gameObject.activeSelf)
                {
                    //Debug.Log("SHOW");
                    currNode.ShowNode();
                }
            }
            else
            {
                if (currNode.gameObject.activeSelf)
                {
                    //Debug.Log("HIDE");
                    currNode.HideNode();
                }
            }
        }
    }
}

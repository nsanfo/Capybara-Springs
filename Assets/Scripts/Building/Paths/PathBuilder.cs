using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PathingBuilder
{
    public bool pathBuildable, point1Buildable, point2Buildable, point1Snapped, pathTooShort;
    public PathNode[] snappedNodePoints;
    public PathNode currentSnappedNode;

    public PathingBuilder()
    {
        pathBuildable = true;
        point1Buildable = true;
        point2Buildable = true;
        point1Snapped = false;
        pathTooShort = false;
        currentSnappedNode = null;
        snappedNodePoints = new PathNode[2];
    }
}

public class PathBuilder : MonoBehaviour
{
    [Header("Builder Variables")]
    public GameObject buildingObject;

    [Space(10)]
    [Header("Path Variables")]
    public float pathMinLength = 5.0f;

    [Space(10)]
    [Header("Path Point Variables")]
    [Range(0.1f, 0.5f)]
    public float pointSpacing = 0.1f;
    public float pointResolution = 1f;

    [Space(10)]
    [Header("Path Mesh Variables")]
    [Range(0.5f, 1.5f)]
    public float meshSpacing = 1;
    public float meshWidth = 1.0f;
    public float meshOffset = 0.01f;

    [Space(10)]
    [Header("Textures")]
    public Material guideDefaultMaterial;
    public Material guideEnabledMaterial;
    public Material guideDisabledMaterial;
    [Space(5)]
    public Material pathMaterial;

    [Space(10)]
    [Header("Nodes")]
    public float nodeAppearRange = 10.0f;
    public float nodeSnapRange = 1.0f;
    public GameObject nodePrefab;
    public RuntimeAnimatorController nodeAnimatorController;

    // Building variables
    private BuildingModes buildingModes;
    public PathingBuilder pathingBuilder = new PathingBuilder();

    // Mouse raycast
    public MouseRaycast mouseRaycast = new MouseRaycast();

    // Path variables
    public (Vector3, Vector3) endpoints;
    private GameObject pathsHolder;
    private GameObject[] paths;

    // Node variables
    //public (PathNode, Vector3) nodeSnapPosition;
    private GameObject[] nodes;

    // Guide names
    public string guideHandlerName = "GuideHandler";
    public string guideMouseName = "GuideMouse";
    public string guidePointName = "GuidePoint";
    public string guidePathName = "GuidePath";

    // Start is called before the first frame update
    void Start()
    {
        PlayerBuilding buildingScript = gameObject.GetComponent<PlayerBuilding>();

        // Get building modes from building script
        buildingModes = buildingScript.buildingModes;

        // Get raycast from building script
        mouseRaycast = buildingScript.mouseRaycast;

        // Create a paths object if one does not exist
        pathsHolder = GameObject.Find("Paths");
        if (pathsHolder == null) pathsHolder = new GameObject("Paths");
    }

    // Update is called once per frame
    void Update()
    {
        // Check building
        if (!buildingModes.enablePath)
        {
            ResetPathingBuilder();
            return;
        }

        // Check if cursor is over UI, check for raycast hit
        #region CursorAndRaycast

        if (EventSystem.current.IsPointerOverGameObject()) return;

        // Check for raycast hit
        if (!mouseRaycast.CheckHit()) return;
        #endregion

        // Update path building depending on points snapped to
        #region UpdatePathBuilding
        if (pathingBuilder.point1Snapped && pathingBuilder.currentSnappedNode != null)
        {
            pathingBuilder.pathBuildable = !PathUtilities.CheckForCollision(gameObject, guideHandlerName + "/" + guidePathName + "/Collisions", 3, 3);
        }
        else if (!pathingBuilder.point1Snapped && pathingBuilder.currentSnappedNode != null)
        {
            pathingBuilder.pathBuildable = !PathUtilities.CheckForCollision(gameObject, guideHandlerName + "/" + guidePathName + "/Collisions", 0, 3);
        }
        else if (pathingBuilder.point1Snapped)
        {
            pathingBuilder.pathBuildable = !PathUtilities.CheckForCollision(gameObject, guideHandlerName + "/" + guidePathName + "/Collisions", 3);
        }
        else
        {
            pathingBuilder.pathBuildable = !PathUtilities.CheckForCollision(gameObject, guideHandlerName + "/" + guidePathName + "/Collisions");
        }
        #endregion

        #region UpdatePointBuildables
        if (endpoints.Item1 == Vector3.zero)
        {
            // Check for collision 1
            pathingBuilder.point1Buildable = !PathUtilities.CheckForCollision(gameObject, guideHandlerName + "/" + guideMouseName + "/Collisions");
        }
        else if (endpoints.Item2 == Vector3.zero)
        {
            // Check for collision 2
            pathingBuilder.point2Buildable = !PathUtilities.CheckForCollision(gameObject, guideHandlerName + "/" + guideMouseName + "/Collisions");
        }
        #endregion

        #region UpdatePathLength
        if (endpoints.Item1 != Vector3.zero)
        {
            if ((endpoints.Item1 - mouseRaycast.GetPosition()).sqrMagnitude < pathMinLength)
            {
                pathingBuilder.pathTooShort = true;
            }
            else
            {
                pathingBuilder.pathTooShort = false;
            }
        }
        #endregion

        // Placing points and building paths
        #region InputLeftMouse
        if (Input.GetMouseButtonDown(0))
        {
            // Set first points
            if (endpoints.Item1 == Vector3.zero)
            {
                // Check if cursor snapped to node
                if (pathingBuilder.currentSnappedNode == null)
                {
                    if (pathingBuilder.point1Buildable) endpoints.Item1 = mouseRaycast.GetPosition();
                }
                else
                {
                    pathingBuilder.point1Snapped = true;
                    pathingBuilder.snappedNodePoints[0] = pathingBuilder.currentSnappedNode;
                    endpoints.Item1 = pathingBuilder.snappedNodePoints[0].transform.position;
                }
            }
            // Set second point
            else if (endpoints.Item2 == Vector3.zero)
            {
                if (pathingBuilder.pathBuildable)
                {
                    if (!pathingBuilder.pathTooShort)
                    {
                        if (pathingBuilder.point2Buildable)
                        {
                            endpoints.Item2 = mouseRaycast.GetPosition();
                            CreatePath();
                            endpoints.Item1 = Vector3.zero;
                            endpoints.Item2 = Vector3.zero;
                            ResetPathingBuilder();
                        }
                        else if (pathingBuilder.currentSnappedNode != null)
                        {
                            pathingBuilder.snappedNodePoints[1] = pathingBuilder.currentSnappedNode;
                            endpoints.Item2 = pathingBuilder.snappedNodePoints[1].transform.position;
                            CreatePath();
                            endpoints.Item1 = Vector3.zero;
                            endpoints.Item2 = Vector3.zero;
                            ResetPathingBuilder();
                        }
                    }
                }
            }
        }
        #endregion

        // Cancel building
        #region InputRightMouse
        else if (Input.GetMouseButtonDown(1))
        {
            if (endpoints.Item1 != Vector3.zero) endpoints.Item1 = Vector3.zero;
            ResetPathingBuilder();
        }
        #endregion

        // Track path nodes near cursor
        #region TrackNodes
        if (nodes != null)
        {
            TrackNearbyNodes();
            SnapToNearbyNode();
        }
        #endregion
    }

    void CreatePath()
    {
        // Create new path object
        GameObject path = new GameObject("Path");
        path.transform.SetParent(pathsHolder.transform);

        // Get offset points (prevent z-axis fighting on terrain)
        Vector3 offsetVector = new Vector3(0, meshOffset, 0);
        (Vector3, Vector3) offsetEndpoints = (endpoints.Item1 + offsetVector, endpoints.Item2 + offsetVector);

        // Calculate spaced points
        Vector3[] spacedPoints = PathUtilities.CalculateEvenlySpacedPoints(offsetEndpoints, pointSpacing, pointResolution);

        // Add path component to handle mesh
        path.AddComponent<Path>();
        Path pathComponent = path.GetComponent<Path>();
        pathComponent.UpdateVariables(this);
        pathComponent.InitializeMesh("PathCollider", false);

        // Add new path to list
        List<GameObject> pathList = new List<GameObject>();
        if (paths != null) pathList.AddRange(paths);
        pathList.Add(path);
        paths = pathList.ToArray();
        UpdateNodes();
    }

    void UpdateNodes()
    {
        List<GameObject> currNodes = new List<GameObject>();

        for (int i = 0; i < paths.Length; i++)
        {
            currNodes.AddRange(paths[i].GetComponent<Path>().GetNodes());
        }

        nodes = currNodes.ToArray();
    }

    void TrackNearbyNodes()
    {
        PathNode currNode;
        for (int i = 0; i < nodes.Length; i++)
        {
            currNode = nodes[i].GetComponent<PathNode>();

            Vector3 offset = currNode.transform.position - mouseRaycast.GetPosition();
            float sqrLen = offset.sqrMagnitude;

            if (sqrLen < nodeAppearRange * nodeAppearRange)
            {
                if (!currNode.gameObject.activeSelf)
                {
                    currNode.ShowNode();
                }
            }
            else
            {
                if (currNode.gameObject.activeSelf)
                {
                    currNode.HideNode();
                }
            }
        }
    }

    public void HideAllNodes()
    {
        if (nodes == null) return;

        PathNode currNode;
        for (int i = 0; i < nodes.Length; i++)
        {
            currNode = nodes[i].GetComponent<PathNode>();

            if (currNode.gameObject.activeSelf) currNode.HideNode();
        }
    }

    void SnapToNearbyNode()
    {
        PathNode currNode;
        (PathNode, float) closestNode;
        closestNode.Item1 = null;
        closestNode.Item2 = 0.0f;

        int numNodesInRange = 0;
        for (int i = 0; i < nodes.Length; i++)
        {
            currNode = nodes[i].GetComponent<PathNode>();

            // Get distance
            Vector3 offset = currNode.transform.position - mouseRaycast.GetPosition();
            float sqrLen = offset.sqrMagnitude;

            // Gets node in snap range
            if (sqrLen < nodeSnapRange * nodeSnapRange)
            {
                numNodesInRange++;

                // Check for closest node in snap range
                if (closestNode.Item1 == null || sqrLen < closestNode.Item2)
                {
                    closestNode.Item1 = currNode;
                    closestNode.Item2 = sqrLen;
                }
            }
        }

        if (numNodesInRange > 0)
        {
            for (int i = 0; i < 2; i++)
            {
                if (pathingBuilder.snappedNodePoints[i] != closestNode.Item1)
                {
                    if (endpoints.Item1 != closestNode.Item1.transform.position)
                    {
                        pathingBuilder.currentSnappedNode = closestNode.Item1;
                        pathingBuilder.currentSnappedNode.SnapNode();
                        return;
                    }
                }
            }
        }
        else if (pathingBuilder.currentSnappedNode != null)
        {
            pathingBuilder.currentSnappedNode.UnsnapNode();
            pathingBuilder.currentSnappedNode = null;
        }
    }

    void ResetPathingBuilder()
    {
        pathingBuilder.pathBuildable = true;
        pathingBuilder.point1Buildable = true;
        pathingBuilder.point2Buildable = true;
        pathingBuilder.point1Snapped = false;
        pathingBuilder.currentSnappedNode = null;
        pathingBuilder.snappedNodePoints = new PathNode[2];
    }

    public void ResetEndpoints()
    {
        endpoints.Item1 = Vector3.zero;
        endpoints.Item2 = Vector3.zero;
    }
}

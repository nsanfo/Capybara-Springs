using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PathHelper
{
    public bool pathBuildable, mouseBuildable, curvedPath;
    public PathNode snappedMouseNode;
    public PathNode snappedInitialNode;
    public  (Vector3, Vector3, Vector3) pathPoints;

    public PathHelper()
    {
        pathBuildable = true;
        mouseBuildable = true;
        curvedPath = false;
        snappedMouseNode = null;
        snappedInitialNode = null;
        pathPoints = (Vector3.zero, Vector3.zero, Vector3.zero);
    }
}

public class PathBuilder : MonoBehaviour
{
    public enum GuideNames
    {
        GuideHandler, GuideMouse, GuidePoint, GuideCurvedPoint, GuidePath, GuideDottedLine, GuideCollider
    }

    public enum PathNames
    {
        PathHolder, Path, PathCollider, Collisions
    }

    public enum NodeNames
    {
        NodeHolder, Node
    }

    [Space(10)]
    [Header("Path Variables")]
    public float pathMinLength = 2.0f;
    public int pathCollisionGrace = 3;
    public float pathCurvedMinAngle = 35.0f;

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
    [Header("Path Setting UI")]
    public CanvasRenderer pathPanel;
    [HideInInspector]
    public Toggle pathCurvedToggle;

    [Space(10)]
    [Header("Textures")]
    public Material[] guideDefaultMaterials;
    public Material[] guideOnMaterials;
    public Material[] guideOffMaterials;
    public Material[] guidePathMaterials;
    [Space(5)]
    public Material pathMaterial;

    [Space(10)]
    [Header("Nodes")]
    public float nodeAppearRange = 10.0f;
    public float nodeSnapRange = 1.0f;
    public GameObject nodePrefab;
    public RuntimeAnimatorController nodeAnimatorController;
    public NodeController nodeController;

    // Mouse raycast
    public MouseRaycast mouseRaycast = new MouseRaycast();

    // Building variables
    private BuildingModes buildingModes;
    public PathHelper pathHelper = new PathHelper();

    // Path Variables
    private GameObject pathsHolder;

    void Start()
    {
        PlayerBuilding buildingScript = gameObject.GetComponent<PlayerBuilding>();

        // Get building modes from building script
        buildingModes = buildingScript.buildingModes;

        // Get raycast from building script
        mouseRaycast = buildingScript.mouseRaycast;

        // Create path holder object
        pathsHolder = GameObject.Find(PathNames.PathHolder.ToString());
        if (pathsHolder == null) pathsHolder = new GameObject(PathNames.PathHolder.ToString());

        // Hide path panel at startup
        pathPanel.gameObject.SetActive(false);
        pathCurvedToggle = pathPanel.transform.GetChild(0).gameObject.GetComponent<Toggle>();

        // Initialize node controller
        nodeController = new NodeController();
    }

    void Update()
    {
        // Check to update
        if (!CheckIfUpdate()) return;

        // Update path helper
        UpdatePathHelper();

        // Track nodes
        TrackNearbyNodes();

        // Check mouse inputs
        CheckLeftMouseInput();
        CheckRightMouseInput();
    }

    bool CheckIfUpdate()
    {
        // Check if path is buildable
        if (!buildingModes.enablePath)
        {
            SetPathCurvePanel(false);
            if (nodeController.nodesVisible) nodeController.SetNodesVisibility(false);
            return false;
        }
        else
        {
            SetPathCurvePanel(true);
            if (!nodeController.nodesVisible) nodeController.SetNodesVisibility(true);
        }

        // Check if pointer is over UI
        if (EventSystem.current.IsPointerOverGameObject()) return false;

        // Check for raycast hit
        if (!mouseRaycast.CheckHit()) return false;

        return true;
    }

    void SetPathCurvePanel(bool activeState)
    {
        if (activeState)
        {
            if (!pathPanel.gameObject.activeSelf) pathPanel.gameObject.SetActive(true);
        }
        else
        {
            if (pathPanel.gameObject.activeSelf) pathPanel.gameObject.SetActive(false);
        }
    }

    void UpdatePathHelper()
    {
        // Check for snapping
        SnapToNearbyNode();

        // Check for path collision
        #region CheckPathCollision
        string pathDirectory = GuideNames.GuideHandler.ToString() + "/" + GuideNames.GuidePath.ToString() + "/" + PathNames.Collisions.ToString();
        
        // Grace on both sides snapped
        if (pathHelper.snappedInitialNode != null && pathHelper.snappedMouseNode != null)
        {
            pathHelper.pathBuildable = !PathUtilities.CheckForCollision(gameObject, pathDirectory, pathCollisionGrace, pathCollisionGrace);
        }
        // Grace on mouse side snapped
        else if (pathHelper.snappedInitialNode == null && pathHelper.snappedMouseNode != null)
        {
            pathHelper.pathBuildable = !PathUtilities.CheckForCollision(gameObject, pathDirectory, 0, pathCollisionGrace);
        }
        // Grace on first side snapped
        else if (pathHelper.snappedInitialNode != null)
        {
            pathHelper.pathBuildable = !PathUtilities.CheckForCollision(gameObject, pathDirectory, pathCollisionGrace);
        }
        // No grace
        else
        {
            pathHelper.pathBuildable = !PathUtilities.CheckForCollision(gameObject, pathDirectory);
        }
        #endregion

        // Check for mouse collision
        #region CheckMouseCollision
        string mouseDirectory = GuideNames.GuideHandler.ToString() + "/" + GuideNames.GuideMouse.ToString() + "/" + PathNames.Collisions.ToString();
        if (pathHelper.snappedMouseNode != null)
        {
            pathHelper.mouseBuildable = true;
        }
        else
        {
            pathHelper.mouseBuildable = !PathUtilities.CheckForCollision(gameObject, mouseDirectory);
        }
        #endregion

        // Check for path minimum length
        #region CheckPathMinimumLength
        if (pathHelper.pathPoints.Item1 != Vector3.zero && pathHelper.pathBuildable)
        {
            if ((pathHelper.pathPoints.Item1 - mouseRaycast.GetPosition()).sqrMagnitude < pathMinLength)
            {
                pathHelper.pathBuildable = false;
            }
        }
        #endregion

        // Check for curved pathing
        #region CheckCurvedPathing
        if (pathHelper.curvedPath)
        {
            // Check curved path angle
            if (pathHelper.pathPoints.Item1 != Vector3.zero && pathHelper.pathPoints.Item3 != Vector3.zero)
            {
                float angle = Vector3.Angle(pathHelper.pathPoints.Item1 - pathHelper.pathPoints.Item3, mouseRaycast.GetPosition() - pathHelper.pathPoints.Item3);
                if (angle < pathCurvedMinAngle) pathHelper.pathBuildable = false;
            }

            // Check curve point building
            if (pathHelper.pathPoints.Item3 == Vector3.zero) pathHelper.mouseBuildable = true;
        }
        #endregion
    }

    void CheckLeftMouseInput()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        // Set first point
        #region SetFirstPoint
        if (pathHelper.pathPoints.Item1 == Vector3.zero)
        {
            // Set point at snapped node
            if (pathHelper.snappedMouseNode != null)
            {
                pathHelper.snappedInitialNode = pathHelper.snappedMouseNode;
                pathHelper.pathPoints.Item1 = pathHelper.snappedMouseNode.transform.position;
            }
            // Set point at mouse raycast position
            else if(pathHelper.mouseBuildable)
            {
                pathHelper.pathPoints.Item1 = mouseRaycast.GetPosition();
            }
        }
        #endregion
        // Set second point to complete path (linear path)
        #region SetLinearSecondPoint
        else if (!pathHelper.curvedPath)
        {
            // Check if second point and path can be created
            if (pathHelper.pathPoints.Item2 != Vector3.zero) return;
            if (!pathHelper.pathBuildable) return;
            if (!pathHelper.mouseBuildable) return;

            // Set point at snapped node
            if (pathHelper.snappedMouseNode != null)
            {
                pathHelper.pathPoints.Item2 = pathHelper.snappedMouseNode.transform.position;
                CreatePath();
                ResetPathHelper();
            }
            // Set point at mouse raycast position
            else
            {
                pathHelper.pathPoints.Item2 = mouseRaycast.GetPosition();
                CreatePath();
                ResetPathHelper();
            }
        }
        #endregion
        // Set third point (curved path)
        #region SetCurvedThirdPoint
        else if (pathHelper.curvedPath)
        {
            // Set curved point
            if (pathHelper.mouseBuildable && pathHelper.pathPoints.Item3 == Vector3.zero)
            {
                pathHelper.pathPoints.Item3 = mouseRaycast.GetPosition();
            }
            // Set second point to complete path
            else if (pathHelper.pathBuildable)
            {
                if (pathHelper.snappedMouseNode != null)
                {
                    pathHelper.pathPoints.Item2 = pathHelper.snappedMouseNode.transform.position;
                    CreatePath();
                    ResetPathHelper();
                }
                else if (pathHelper.mouseBuildable)
                {
                    pathHelper.pathPoints.Item2 = mouseRaycast.GetPosition();
                    CreatePath();
                    ResetPathHelper();
                }
            }
        }
        #endregion
    }

    void CheckRightMouseInput()
    {
        if (!Input.GetMouseButtonDown(1)) return;

        if (pathHelper.pathPoints.Item1 != Vector3.zero)
        {
            ResetPathHelper();
        }   
    }

    void CreatePath()
    {
        // Create new path object
        GameObject path = new GameObject(PathNames.Path.ToString());
        path.transform.SetParent(pathsHolder.transform);

        // Get offset points (prevent z-axis fighting on terrain)
        pathHelper.pathPoints.Item1 = new Vector3(pathHelper.pathPoints.Item1.x, meshOffset, pathHelper.pathPoints.Item1.z);
        pathHelper.pathPoints.Item2 = new Vector3(pathHelper.pathPoints.Item2.x, meshOffset, pathHelper.pathPoints.Item2.z);
        if (pathHelper.curvedPath) pathHelper.pathPoints.Item3 = new Vector3(pathHelper.pathPoints.Item3.x, meshOffset, pathHelper.pathPoints.Item3.z);

        // Add path component to handle mesh
        path.AddComponent<Path>();
        Path pathComponent = path.GetComponent<Path>();
        pathComponent.UpdateVariables(this, pathHelper.pathPoints);
        pathComponent.InitializeMesh(false, nodeController);

        // Add path to node controller
        nodeController.AddPath(pathComponent);
    }

    void TrackNearbyNodes()
    {
        PathNode[] nodes = nodeController.GetNodes();
        if (nodes == null) return;

        PathNode currNode;
        for (int i = 0; i < nodes.Length; i++)
        {
            currNode = nodes[i];

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

    void SnapToNearbyNode()
    {
        PathNode[] nodes = nodeController.GetNodes();
        if (nodes == null) return;

        PathNode currNode;
        (PathNode, float) closestNode;
        closestNode.Item1 = null;
        closestNode.Item2 = 0.0f;
        int numNodesInRange = 0;
        for (int i = 0; i < nodes.Length; i++)
        {
            currNode = nodes[i];

            // Get distance
            Vector3 offset = currNode.transform.position - mouseRaycast.GetPosition();
            float sqrLen = offset.sqrMagnitude;

            // Get node in snap range
            if (sqrLen < nodeSnapRange * nodeSnapRange)
            {
                numNodesInRange++;

                // Track closest node in snap range
                if (closestNode.Item1 == null || sqrLen < closestNode.Item2)
                {
                    closestNode.Item1 = currNode;
                    closestNode.Item2 = sqrLen;
                }
            }
        }

        if (numNodesInRange > 0)
        {
            // Disable snapping on curved point placement
            if (pathHelper.curvedPath && pathHelper.pathPoints.Item1 != Vector3.zero && pathHelper.pathPoints.Item3 == Vector3.zero) return;

            // Snap only on nodes not previously snapped to
            if (pathHelper.snappedInitialNode != closestNode.Item1)
            {
                pathHelper.snappedMouseNode = closestNode.Item1;
                pathHelper.snappedMouseNode.SnapNode();
                return;
            }
        }
        else if (pathHelper.snappedMouseNode != null)
        {
            pathHelper.snappedMouseNode.UnsnapNode();
            pathHelper.snappedMouseNode = null;
        }
    }

    void ResetPathHelper()
    {
        bool curvedState = pathHelper.curvedPath;
        pathHelper = new PathHelper();
        pathHelper.curvedPath = curvedState;
    }

    public void ToggleCurvedPathing()
    {
        pathHelper = new PathHelper();
        pathHelper.curvedPath = pathCurvedToggle.isOn;
    }
}

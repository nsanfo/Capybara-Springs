using System.Collections;
using System.Collections.Generic;
using TMPro;
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
        NodeHolder, Node, ImageHolder, NodeImage
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
    [HideInInspector]
    public float meshOffset = 0.0001f;

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
    public NodeGraph nodeGraph = new NodeGraph();
    public Material nodeMaterial;

    // Mouse raycast
    public MouseRaycast mouseRaycast = new MouseRaycast();

    // Building variables
    private BuildingModes buildingModes;
    public PathHelper pathHelper = new PathHelper();

    // Path Variables
    private GameObject pathsHolder;

    // Entrance Variables
    public Vector3 enterVector1 = new Vector3(0, 0, 0);
    public Vector3 enterVector2 = new Vector3(1, 0, 0);

    // Path text
    public GameObject pathTextPrefab;
    private GameObject pathText;

    // Balance
    Balance balance;
    double cost = 0;

    AudioSource buildSFX;
    AudioSource click3;

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

        // Initialize node graph with entrance
        GameObject entranceObject = GameObject.Find("EntrancePath");
        if (entranceObject == null) return;

        Path entrancePath = entranceObject.GetComponent<Path>();
        if (entrancePath == null) return;

        nodeGraph.AddPath(entrancePath);
        nodeGraph.SetNodesVisibility(false);

        PathNode entranceNode1 = entranceObject.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<PathNode>();
        PathNode entranceNode2 = entranceObject.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<PathNode>();

        entranceNode1.AddPath(entrancePath);
        entranceNode2.AddPath(entrancePath);

        meshOffset = 0.0001f;

        // Get balance
        GameObject stats = GameObject.Find("Stats");
        if (stats != null)
        {
            balance = stats.GetComponent<Balance>();
        }

        pathText = Instantiate(pathTextPrefab);
        pathText.SetActive(false);
        pathText.transform.SetParent(transform);

        var UISounds = GameObject.Find("UISounds");
        buildSFX = UISounds.transform.GetChild(2).GetComponent<AudioSource>();
        click3 = UISounds.transform.GetChild(4).GetComponent<AudioSource>();
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
            if (nodeGraph.nodesVisible) nodeGraph.SetNodesVisibility(false);
            return false;
        }
        else
        {
            SetPathCurvePanel(true);
            if (!nodeGraph.nodesVisible) nodeGraph.SetNodesVisibility(true);
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

        // Check for path cost
        #region CheckPathCost
        if (pathHelper.pathPoints.Item1 != Vector3.zero && pathHelper.pathBuildable)
        {
            if (!pathText.activeSelf)
            {
                pathText.SetActive(true);
            }

            Vector3 guideMousePosition = gameObject.GetComponent<PathGuide>().GetGuideMousePosition();

            // Handle path text rotation and sizing
            pathText.transform.rotation = Camera.main.transform.rotation;
            pathText.GetComponent<TextMeshPro>().fontSize = Vector3.Distance(pathText.transform.position, Camera.main.transform.position) / 1.6f;

            if (pathHelper.curvedPath)
            {
                if (pathHelper.pathPoints.Item3 == Vector3.zero)
                {
                    if (pathHelper.snappedMouseNode != null)
                    {
                        cost = Vector3.Distance(pathHelper.pathPoints.Item1, pathHelper.snappedMouseNode.gameObject.transform.position);
                    }
                    else
                    {
                        cost = Vector3.Distance(pathHelper.pathPoints.Item1, guideMousePosition);
                    }

                    pathText.transform.position = (pathHelper.pathPoints.Item1 + guideMousePosition) / 2;
                }
                else
                {
                    if (pathHelper.snappedMouseNode != null)
                    {
                        cost = Vector3.Distance(pathHelper.pathPoints.Item1, pathHelper.pathPoints.Item3)
                            + Vector3.Distance(pathHelper.pathPoints.Item3, pathHelper.snappedMouseNode.gameObject.transform.position);
                    }
                    else
                    {
                        cost = Vector3.Distance(pathHelper.pathPoints.Item1, pathHelper.pathPoints.Item3)
                            + Vector3.Distance(pathHelper.pathPoints.Item3, guideMousePosition);
                    }

                    pathText.transform.position = (pathHelper.pathPoints.Item1 + pathHelper.pathPoints.Item3 + guideMousePosition) / 3;
                }
            }
            else
            {
                if (pathHelper.snappedMouseNode != null)
                {
                    cost = Vector3.Distance(pathHelper.pathPoints.Item1, pathHelper.snappedMouseNode.gameObject.transform.position);
                }
                else
                {
                    cost = Vector3.Distance(pathHelper.pathPoints.Item1, guideMousePosition);
                }

                pathText.transform.position = (pathHelper.pathPoints.Item1 + guideMousePosition) / 2;
            }

            pathText.transform.position += new Vector3(0, 0.5f, 0);

            cost *= 500;
            cost = Mathf.Ceil((float) cost);
            if (balance.balance < cost)
            {
                pathText.GetComponent<TextMeshPro>().text = "$" + cost + "\nToo Expensive!";
                pathText.GetComponent<TextMeshPro>().outlineColor = new Color32(241, 125, 69, 255);
                pathHelper.pathBuildable = false;
            }
            else
            {
                pathText.GetComponent<TextMeshPro>().text = "$" + cost;
                pathText.GetComponent<TextMeshPro>().outlineColor = new Color32(136, 216, 255, 255);
            }
        }
        else
        {
            if (pathText.activeSelf)
            {
                pathText.SetActive(false);
            }
        }
        #endregion
    }

    void CheckLeftMouseInput()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        // Get guide mouse vector
        Vector3 guideMousePosition = gameObject.GetComponent<PathGuide>().GetGuideMousePosition();

        // Set first point
        #region SetFirstPoint
        if (pathHelper.pathPoints.Item1 == Vector3.zero)
        {
            // Set point at snapped node
            if (pathHelper.snappedMouseNode != null)
            {
                pathHelper.snappedInitialNode = pathHelper.snappedMouseNode;
                pathHelper.pathPoints.Item1 = pathHelper.snappedMouseNode.transform.position;

                pathHelper.snappedInitialNode.snappedNode = true;
                pathHelper.snappedMouseNode = null;
            }
            // Set point at mouse raycast position
            else if(pathHelper.mouseBuildable)
            {
                if (mouseRaycast.GetHitInfo().transform.CompareTag("Terrain"))
                {
                    pathHelper.pathPoints.Item1 = mouseRaycast.GetPosition();
                }
                else
                {
                    pathHelper.pathPoints.Item1 = guideMousePosition;
                }
            }
            click3.Play();
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

            // Unsnap from initial node
            if (pathHelper.snappedInitialNode != null)
            {
                pathHelper.snappedInitialNode.snappedNode = false;
                pathHelper.snappedInitialNode.UnsnapNode();
            }

            // Set point at snapped node
            if (pathHelper.snappedMouseNode != null)
            {
                pathHelper.pathPoints.Item2 = pathHelper.snappedMouseNode.transform.position;
            }
            // Set point at mouse raycast position
            else
            {
                if (mouseRaycast.GetHitInfo().transform.CompareTag("Terrain"))
                {
                    pathHelper.pathPoints.Item2 = mouseRaycast.GetPosition();
                }
                else
                {
                    pathHelper.pathPoints.Item2 = guideMousePosition;
                }
            }

            // Create path
            CreatePath();
            ResetPathHelper();
        }
        #endregion
        // Set third point (curved path)
        #region SetCurvedThirdPoint
        else if (pathHelper.curvedPath)
        {
            // Set curved point
            if (pathHelper.mouseBuildable && pathHelper.pathPoints.Item3 == Vector3.zero)
            {
                if (mouseRaycast.GetHitInfo().transform.CompareTag("Terrain"))
                {
                    pathHelper.pathPoints.Item3 = mouseRaycast.GetPosition();
                }
                else
                {
                    pathHelper.pathPoints.Item3 = guideMousePosition;
                }
            }
            // Set second point to complete path
            else if (pathHelper.pathBuildable)
            {
                // Unsnap from initial node
                if (pathHelper.snappedInitialNode != null)
                {
                    pathHelper.snappedInitialNode.snappedNode = false;
                    pathHelper.snappedInitialNode.UnsnapNode();
                }

                // Set point at snapped node
                if (pathHelper.snappedMouseNode != null)
                {
                    pathHelper.pathPoints.Item2 = pathHelper.snappedMouseNode.transform.position;
                }
                // Set point at mouse raycast position
                else if (pathHelper.mouseBuildable)
                {
                    if (mouseRaycast.GetHitInfo().transform.CompareTag("Terrain"))
                    {
                        pathHelper.pathPoints.Item2 = mouseRaycast.GetPosition();
                    }
                    else
                    {
                        pathHelper.pathPoints.Item2 = guideMousePosition;
                    }
                }

                // Create path
                CreatePath();
                ResetPathHelper();
            }
        }
        #endregion
    }

    void CheckRightMouseInput()
    {
        if (!Input.GetMouseButtonDown(1)) return;

        if (pathHelper.pathPoints.Item1 != Vector3.zero)
        {
            if (pathHelper.snappedInitialNode != null)
            {
                pathHelper.snappedInitialNode.snappedNode = false;
                pathHelper.snappedInitialNode.UnsnapNode();
            }
            ResetPathHelper();
        }   
    }

    void CreatePath()
    {
        GameObject path = new GameObject(PathNames.Path.ToString());
        path.transform.SetParent(pathsHolder.transform);

        // Get offset points (prevent z-axis fighting on terrain)
        pathHelper.pathPoints.Item1 = new Vector3(pathHelper.pathPoints.Item1.x, meshOffset, pathHelper.pathPoints.Item1.z);
        pathHelper.pathPoints.Item2 = new Vector3(pathHelper.pathPoints.Item2.x, meshOffset, pathHelper.pathPoints.Item2.z);
        if (pathHelper.curvedPath) pathHelper.pathPoints.Item3 = new Vector3(pathHelper.pathPoints.Item3.x, meshOffset, pathHelper.pathPoints.Item3.z);

        // Add path component to handle mesh
        Path pathComponent = path.AddComponent<Path>();
        pathComponent.UpdateVariables(this, pathHelper.pathPoints);
        pathComponent.InitializeMesh(false, nodeGraph);

        // Add path to node graph
        nodeGraph.AddPath(pathComponent);

        // Handle cost
        balance.AdjustBalance(cost * -1);

        buildSFX.Play();
    }

    void TrackNearbyNodes()
    {
        PathNode[] nodes = nodeGraph.Nodes;
        if (nodes == null || nodes.Length == 0) return;

        if (nodes[0].isActiveAndEnabled)
        {
            nodes[0].SetInactive();
        }

        PathNode currNode;
        for (int i = 1; i < nodes.Length; i++)
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
                if (currNode.gameObject.activeSelf && currNode != pathHelper.snappedInitialNode)
                {
                    currNode.HideNode();
                }
            }
        }

    }

    void SnapToNearbyNode()
    {
        PathNode[] nodes = nodeGraph.Nodes;
        if (nodes == null || nodes.Length == 0) return;

        PathNode currNode;
        (PathNode, float) closestNode;
        closestNode.Item1 = null;
        closestNode.Item2 = 0.0f;
        int numNodesInRange = 0;
        for (int i = 1; i < nodes.Length; i++)
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
            if (pathHelper.snappedInitialNode != closestNode.Item1 && pathHelper.snappedMouseNode != closestNode.Item1)
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
        if (pathHelper.snappedInitialNode != null)
        {
            pathHelper.snappedInitialNode.snappedNode = false;
            pathHelper.snappedInitialNode.UnsnapNode();
        }

        gameObject.GetComponent<PathGuide>().ToggleGuides();

        pathHelper = new PathHelper();
        pathHelper.curvedPath = pathCurvedToggle.isOn;
    }
}

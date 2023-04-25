using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PathGuide : MonoBehaviour
{
    // Guide objects
    private GameObject guideHandlerObject;
    private GameObject guideMouseObject;
    private GameObject guidePointObject;
    private GameObject guideCurvedPointObject;
    private GameObject guidePathObject;
    private GameObject guideDottedLineObject;

    // Path variables
    private PathBuilder pathBuilderScript;

    // Guide settings
    private float meshOffset;

    // Guide materials
    private Material[] guideDefaultMaterials;
    private Material[] guideOnMaterials;
    private Material[] guideOffMaterials;
    private Material[] guidePathMaterials;

    // Mouse raycast
    public MouseRaycast mouseRaycast = new MouseRaycast();

    // Building variables
    private BuildingModes buildingModes;
    public PathHelper pathHelper = new PathHelper();

    void Start()
    {
        pathBuilderScript = gameObject.GetComponent<PathBuilder>();
        PlayerBuilding buildingScript = gameObject.GetComponent<PlayerBuilding>();

        // Get guide settings from building script
        meshOffset = pathBuilderScript.meshOffset * 2;

        // Get guide materials from building script
        guideDefaultMaterials = pathBuilderScript.guideDefaultMaterials;
        guideOnMaterials = pathBuilderScript.guideOnMaterials;
        guideOffMaterials = pathBuilderScript.guideOffMaterials;
        guidePathMaterials = pathBuilderScript.guidePathMaterials;

        // Get raycast from building script
        mouseRaycast = buildingScript.mouseRaycast;

        // Get building modes from building script
        buildingModes = buildingScript.buildingModes;

        // Set guide handler
        guideHandlerObject = new GameObject(PathBuilder.GuideNames.GuideHandler.ToString());
        guideHandlerObject.transform.SetParent(gameObject.transform);
    }

    void Update()
    {
        // Check to update
        if (!CheckIfUpdate()) return;

        // Update mouse
        HandleGuideMouse();

        // Update path helper
        pathHelper = pathBuilderScript.pathHelper;

        // Update point
        HandleGuidePoint();

        // Update path
        HandleGuidePath();

        // Update dotted line
        HandleGuideDottedLine();

        // Update mouse snapped node
        HandleMouseSnappedNode();
    }

    void OnDestroy()
    {
        Destroy(guideHandlerObject);
    }

    bool CheckIfUpdate()
    {
        // Check if path is buildable
        if (!buildingModes.enablePath)
        {
            return false;
        }

        // Check if pointer is over UI
        if (EventSystem.current.IsPointerOverGameObject()) return false;

        // Check for raycast hit
        if (!mouseRaycast.CheckHit()) return false;

        return true;
    }

    void HandleGuideMouse()
    {
        // Handle object
        if (guideMouseObject == null)
        {
            InitializeGuideMouse();
        }
        else
        {
            UpdateGuideMouse();

            // Handle material
            if (pathHelper.mouseBuildable)
            {
                guideMouseObject.GetComponent<Renderer>().materials = guideDefaultMaterials;
            }
            else
            {
                guideMouseObject.GetComponent<Renderer>().materials = guideOffMaterials;
            }
        }
    }

    void InitializeGuideMouse()
    {
        // Create guide mouse object
        guideMouseObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        guideMouseObject.name = PathBuilder.GuideNames.GuideMouse.ToString();
        guideMouseObject.transform.SetParent(guideHandlerObject.transform);
        guideMouseObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        SetMouseColliders();

        // Set transforms for guide mouse
        guideMouseObject.transform.localScale = new Vector3(0.4f, 0.025f, 0.4f);

        guideMouseObject.GetComponent<Renderer>().materials = new Material[2];
    }

    void UpdateGuideMouse()
    {
        if (pathHelper.snappedMouseNode != null)
        {
            if (pathHelper.pathPoints.Item1 != pathHelper.snappedMouseNode.transform.position)
            {
                guideMouseObject.transform.position = pathHelper.snappedMouseNode.transform.position + new Vector3(0, 0.028f, 0);
            }
        }
        else
        {
            if (mouseRaycast.GetHitInfo().transform.CompareTag("Terrain"))
            {
                guideMouseObject.transform.position = mouseRaycast.GetPosition() + new Vector3(0, 0.028f, 0);
            }
        }
    }

    void SetMouseColliders()
    {
        // Set colliders
        GameObject collisionHolder = new GameObject(PathBuilder.PathNames.Collisions.ToString());
        collisionHolder.transform.SetParent(guideMouseObject.transform);
        int currentSide = 1;
        for (int i = 0; i < 5; i++)
        {
            // Create collider object
            GameObject collider = new GameObject(PathBuilder.GuideNames.GuideCollider.ToString());
            collider.transform.SetParent(collisionHolder.transform);
            collider.transform.position = guideMouseObject.transform.position;

            // Add collision components
            collider.layer = LayerMask.NameToLayer("Ignore Raycast");
            SphereCollider sphereCollider = collider.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = 1.3f;
            Rigidbody rigidBody = collider.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
            collider.AddComponent<PathColliderTrigger>();

            if (i == 2) currentSide *= -1;

            // Offset collider
            if (i % 2 == 0 && i < 4)
            {
                collider.GetComponent<SphereCollider>().center += new Vector3(1.0f * currentSide, 0.0f, 0.0f);
            }
            else if (i % 2 == 1 && i < 4)
            {
                collider.GetComponent<SphereCollider>().center += new Vector3(0.0f, 0.0f, 1.0f * currentSide);
            }
            else
            {
                collider.GetComponent<SphereCollider>().radius = 2.1f;
            }
        }

        // Set collider trigger
        guideMouseObject.GetComponent<CapsuleCollider>().isTrigger = true;
    }

    void HandleGuidePoint()
    {
        // Handle point object
        if (pathHelper.pathPoints.Item1 != Vector3.zero && guidePointObject == null)
        {
            guidePointObject = CreateGuidePoint(PathBuilder.GuideNames.GuidePoint.ToString(), pathHelper.pathPoints.Item1);
        }
        else if (pathHelper.pathPoints.Item1 == Vector3.zero && guidePointObject != null)
        {
            Destroy(guidePointObject);
            guidePointObject = null;
        }

        // Handle curved point object
        if (pathHelper.pathPoints.Item3 != Vector3.zero && guideCurvedPointObject == null)
        {
            guideCurvedPointObject = CreateGuidePoint(PathBuilder.GuideNames.GuideCurvedPoint.ToString(), pathHelper.pathPoints.Item3);
        }
        else if (pathHelper.pathPoints.Item3 == Vector3.zero && guideCurvedPointObject != null)
        {
            Destroy(guideCurvedPointObject);
            guideCurvedPointObject = null;
        }
    }

    GameObject CreateGuidePoint(string pointName, Vector3 position)
    {
        // Create guide point object
        GameObject pointObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pointObject.name = pointName;
        pointObject.transform.SetParent(guideHandlerObject.transform);
        pointObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        // Set rendering
        pointObject.GetComponent<Renderer>().materials = guideOnMaterials;

        // Set transforms for guide point
        pointObject.transform.position = position + new Vector3(0, 0.028f, 0);
        pointObject.transform.localScale = new Vector3(0.6f, 0.025f, 0.6f);

        return pointObject;
    }

    void HandleGuidePath()
    {
        // Initialize guide when first point set
        if (pathHelper.pathPoints.Item1 != Vector3.zero && guidePathObject == null)
        {
            if (pathHelper.curvedPath)
            {
                if (pathHelper.pathPoints.Item3 != Vector3.zero) InitializeGuidePath();
            }
            else
            {
                InitializeGuidePath();
            }
        }
        // Destroy guide if first point is removed
        else if (pathHelper.pathPoints.Item1 == Vector3.zero && guidePathObject != null)
        {
            Destroy(guidePathObject);
            guidePathObject = null;
        }
        // Update guide
        else if (guidePathObject != null)
        {
            UpdateGuidePath();

            // Handle material
            if (pathHelper.pathBuildable)
            {
                guidePathObject.GetComponent<Renderer>().material = guideOnMaterials[0];
            }
            else
            {
                guidePathObject.GetComponent<Renderer>().material = guideOffMaterials[0];
            }
        }
    }

    void InitializeGuidePath()
    {
        float localMeshOffset = meshOffset * 2;

        // Create guide path object
        guidePathObject = new GameObject(PathBuilder.GuideNames.GuidePath.ToString());
        guidePathObject.transform.SetParent(guideHandlerObject.transform);

        // Get offset points (prevent z-axis fighting on terrain)
        Vector3 point1 = new Vector3(pathHelper.pathPoints.Item1.x, localMeshOffset, pathHelper.pathPoints.Item1.z);
        Vector3 point2 = new Vector3(mouseRaycast.GetPosition().x, localMeshOffset, mouseRaycast.GetPosition().z);
        Vector3 point3 = Vector3.zero;
        if (pathHelper.curvedPath) point3 = new Vector3(pathHelper.pathPoints.Item3.x, localMeshOffset, pathHelper.pathPoints.Item3.z);

        // Add path component to handle mesh
        guidePathObject.AddComponent<Path>();
        Path pathComponent = guidePathObject.GetComponent<Path>();
        pathComponent.UpdateVariables(gameObject.GetComponent<PathBuilder>(), (point1, point2, point3));
        pathComponent.InitializeMesh(true, null);
    }

    void UpdateGuidePath()
    {
        float localMeshOffset = meshOffset * 2;

        // Get offset points (prevent z-axis fighting on terrain)
        Vector3 point1 = new Vector3(pathHelper.pathPoints.Item1.x, localMeshOffset, pathHelper.pathPoints.Item1.z);

        Vector3 point2;
        if (pathHelper.snappedMouseNode != null)
        {
            point2 = new Vector3(pathHelper.snappedMouseNode.transform.position.x, localMeshOffset, pathHelper.snappedMouseNode.transform.position.z);
        }
        else
        {
            if (mouseRaycast.GetHitInfo().transform.CompareTag("Terrain"))
            {
                point2 = new Vector3(mouseRaycast.GetPosition().x, localMeshOffset, mouseRaycast.GetPosition().z);
            }
            else
            {
                point2 = guideMouseObject.transform.position;
            }
        }
        
        Vector3 point3 = Vector3.zero;
        if (pathHelper.curvedPath) point3 = new Vector3(pathHelper.pathPoints.Item3.x, localMeshOffset, pathHelper.pathPoints.Item3.z);

        // Update path component to handle mesh
        Path pathComponent = guidePathObject.GetComponent<Path>();
        pathComponent.UpdateVariables(gameObject.GetComponent<PathBuilder>(), (point1, point2, point3));
        pathComponent.UpdateMesh();
    }

    void HandleGuideDottedLine()
    {
        // Initialize guide when first point set
        if (pathHelper.pathPoints.Item1 != Vector3.zero && guideDottedLineObject == null)
        {
            InitializeGuideDottedLine();
        }
        // Destroy guide if first point is removed
        else if (pathHelper.pathPoints.Item1 == Vector3.zero && guideDottedLineObject != null)
        {
            Destroy(guideDottedLineObject);
            guideDottedLineObject = null;
        }
        // Update guide
        else if (guideDottedLineObject != null)
        {
            UpdateGuideDottedLine();

            // Handle material
            if (pathHelper.pathBuildable)
            {
                guideDottedLineObject.GetComponent<LineRenderer>().material = guidePathMaterials[0];
            }
            else
            {
                guideDottedLineObject.GetComponent<LineRenderer>().material = guidePathMaterials[1];
            }
        }
    }

    void InitializeGuideDottedLine()
    {
        float localMeshOffset = meshOffset * 3;

        // Create guide dotted line object
        guideDottedLineObject = new GameObject(PathBuilder.GuideNames.GuideDottedLine.ToString());
        guideDottedLineObject.transform.SetParent(guideHandlerObject.transform);

        // Rotate to make line visible
        guideDottedLineObject.transform.Rotate(new Vector3(90, 0, 0));

        // Add line renderer component
        LineRenderer lineRenderer = guideDottedLineObject.AddComponent<LineRenderer>();
        lineRenderer.alignment = LineAlignment.TransformZ;
        lineRenderer.textureMode = LineTextureMode.Tile;
        lineRenderer.textureScale = new Vector2(0.9f, 1);
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.startWidth = 0.3f;
        lineRenderer.endWidth = 0.3f;
        lineRenderer.numCornerVertices = 10;
        lineRenderer.numCapVertices = 10;

        // Set position of points for line renderer
        Vector3[] positions = new Vector3[2];
        positions[0] = pathHelper.pathPoints.Item1 + new Vector3(0, localMeshOffset, 0);
        positions[1] = mouseRaycast.GetPosition() + new Vector3(0, localMeshOffset, 0); ;
        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }

    void UpdateGuideDottedLine()
    {
        float localMeshOffset = meshOffset * 3;

        LineRenderer lineRenderer = guideDottedLineObject.GetComponent<LineRenderer>();

        // Add point to line renderer on curved paths
        if (pathHelper.curvedPath && pathHelper.pathPoints.Item3 != Vector3.zero)
        {
            Vector3[] positions = new Vector3[3];
            positions[0] = pathHelper.pathPoints.Item1 + new Vector3(0, localMeshOffset, 0); ;
            positions[1] = pathHelper.pathPoints.Item3 + new Vector3(0, localMeshOffset, 0); ;

            if (pathHelper.snappedMouseNode != null)
            {
                positions[2] = pathHelper.snappedMouseNode.transform.position + new Vector3(0, localMeshOffset, 0); ;
            }
            else
            {
                if (mouseRaycast.GetHitInfo().transform.CompareTag("Terrain"))
                {
                    positions[2] = mouseRaycast.GetPosition() + new Vector3(0, localMeshOffset, 0);
                }
                else
                {
                    positions[2] = guideMouseObject.transform.position;
                }
            }
            
            lineRenderer.positionCount = positions.Length;
            lineRenderer.SetPositions(positions);
        }
        // Handle linear paths and first point of curved paths
        else
        {
            // Set position of points for line renderer
            Vector3[] positions = new Vector3[2];
            positions[0] = pathHelper.pathPoints.Item1 + new Vector3(0, localMeshOffset, 0); ;

            if (pathHelper.snappedMouseNode != null)
            {
                positions[1] = pathHelper.snappedMouseNode.transform.position + new Vector3(0, localMeshOffset, 0); ;
            }
            else
            {
                if (mouseRaycast.GetHitInfo().transform.CompareTag("Terrain"))
                {
                    positions[1] = mouseRaycast.GetPosition() + new Vector3(0, localMeshOffset, 0);
                }
                else
                {
                    positions[1] = guideMouseObject.transform.position;
                }
            }

            lineRenderer.positionCount = positions.Length;
            lineRenderer.SetPositions(positions);
        }
    }

    void HandleMouseSnappedNode()
    {
        if (pathHelper.snappedMouseNode == null) return;

        if (pathHelper.pathBuildable)
        {
            pathHelper.snappedMouseNode.GetComponent<Animator>().Play("SnapPathNode");
        }
        else
        {
            pathHelper.snappedMouseNode.GetComponent<Animator>().Play("SnapUnbuildablePathNode");
        }
    }

    public void ToggleGuides()
    {
        if (guidePathObject != null)
        {
            Destroy(guidePathObject);
            guidePathObject = null;
        }

        if (guideDottedLineObject != null)
        {
            Destroy(guideDottedLineObject);
            guideDottedLineObject = null;
        }

        if (guidePointObject != null)
        {
            Destroy(guidePointObject);
            guidePointObject = null;
        }

        if (guideCurvedPointObject != null)
        {
            Destroy(guideCurvedPointObject);
            guideCurvedPointObject = null;
        }
    }

    public Vector3 GetGuideMousePosition()
    {
        return guideMouseObject.transform.position;
    }
}

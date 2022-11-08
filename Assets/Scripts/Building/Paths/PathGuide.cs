using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PathGuide : MonoBehaviour
{
    // Guide names
    private string guideHandlerName;
    private string guideMouseName;
    private string guidePointName;
    private string guidePathName;

    // Guide objects
    private GameObject guideHandlerObject;
    private GameObject guideMouseObject;
    private GameObject guidePointObject;
    private GameObject guidePathObject;

    // Building variables
    private BuildingModes buildingModes;
    public PathingBuilder pathingBuilder;

    // Mouse raycast
    public MouseRaycast mouseRaycast = new MouseRaycast();

    // Guide settings
    private float meshOffset;
    private float pointSpacing;
    private float pointResolution;

    // Guide materials
    public Material guideDefaultMaterial;
    public Material guideEnabledMaterial;
    public Material guideDisabledMaterial;

    // Path variables
    private PathBuilder pathBuilderScript;
    private (Vector3, Vector3) endpoints;

    // Start is called before the first frame update
    void Start()
    {
        pathBuilderScript = gameObject.GetComponent<PathBuilder>();

        // Get variables from path builder script
        guideHandlerName = pathBuilderScript.guideHandlerName;
        guideMouseName = pathBuilderScript.guideMouseName;
        guidePointName = pathBuilderScript.guidePointName;
        guidePathName = pathBuilderScript.guidePathName;

        // Get building modes from building script
        PlayerBuilding playerBuildingScript = gameObject.GetComponent<PlayerBuilding>();
        buildingModes = playerBuildingScript.buildingModes;

        // Get raycast from building script
        mouseRaycast = playerBuildingScript.mouseRaycast;

        // Get guide settings from building script
        meshOffset = pathBuilderScript.meshOffset;
        pointSpacing = pathBuilderScript.pointSpacing;
        pointResolution = pathBuilderScript.pointResolution;

        // Get guide materials from building script
        guideDefaultMaterial = pathBuilderScript.guideDefaultMaterial;
        guideEnabledMaterial = pathBuilderScript.guideEnabledMaterial;
        guideDisabledMaterial = pathBuilderScript.guideDisabledMaterial;

        // Set guide handler
        guideHandlerObject = new GameObject(guideHandlerName);
        guideHandlerObject.transform.SetParent(gameObject.transform);

        pathingBuilder = pathBuilderScript.pathingBuilder;
    }

    // Update is called once per frame
    void Update()
    {
        // Check building
        if (!buildingModes.enableBuild) return;

        // Check if cursor is over UI, check for raycast hit
        #region CursorAndRaycast
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // Check for raycast hit
        if (!mouseRaycast.CheckHit()) return;
        #endregion

        // Handle updating mouse
        #region HandleGuideMouse
        if (guideMouseObject == null)
        {
            InitializeGuideMouse();
        }
        else
        {
            UpdateGuideMouse();
        }
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

        // Update mouse guide materials and bools
        #region UpdateMouseGuide
        if (endpoints.Item1 == Vector3.zero)
        {
            if (pathingBuilder.currentSnappedNode == null)
            {
                pathingBuilder.point1Buildable = !PathUtilities.CheckForCollision(gameObject, guideHandlerName + "/" + guideMouseName + "/Collisions");

                if (pathingBuilder.point1Buildable)
                {
                    guideMouseObject.GetComponent<Renderer>().material = guideDefaultMaterial;
                }
                else
                {
                    guideMouseObject.GetComponent<Renderer>().material = guideDisabledMaterial;
                }
            }
            else
            {
                guideMouseObject.GetComponent<Renderer>().material = guideDefaultMaterial;
                pathingBuilder.point1Buildable = true;
            }
        }
        else if (endpoints.Item2 == Vector3.zero)
        {
            if (pathingBuilder.currentSnappedNode == null)
            {
                pathingBuilder.point2Buildable = !PathUtilities.CheckForCollision(gameObject, guideHandlerName + "/" + guideMouseName + "/Collisions");

                if (pathingBuilder.point2Buildable)
                {
                    guideMouseObject.GetComponent<Renderer>().material = guideDefaultMaterial;
                }
                else
                {
                    guideMouseObject.GetComponent<Renderer>().material = guideDisabledMaterial;
                }
            }
            else
            {
                guideMouseObject.GetComponent<Renderer>().material = guideDefaultMaterial;
                pathingBuilder.point2Buildable = true;
            }
        }
        #endregion

        // Update endpoints
        endpoints = pathBuilderScript.endpoints;

        // Handle initializing and destroying guides
        #region InitializeGuide
        if (endpoints.Item1 != Vector3.zero)
        {
            if (guidePointObject == null)
            {
                if (pathingBuilder.currentSnappedNode != null)
                {
                    endpoints.Item1 = mouseRaycast.GetPosition();
                    InitializeGuidePath((endpoints.Item1, pathingBuilder.currentSnappedNode.transform.position));
                    InitializeGuidePoint(pathingBuilder.currentSnappedNode.transform.position);
                }
                else if (pathingBuilder.point1Buildable)
                {
                    endpoints.Item1 = mouseRaycast.GetPosition();
                    InitializeGuidePath((endpoints.Item1, mouseRaycast.GetPosition()));
                    InitializeGuidePoint(mouseRaycast.GetPosition());
                }
            }
        }
        else
        {
            DestroyGuides();
            pathingBuilder.point1Snapped = false;
        }
        #endregion

        // Handle updating guide path
        #region UpdatePathGuide
        if (guidePathObject != null)
        {
            if (pathingBuilder.currentSnappedNode != null)
            {
                UpdateGuidePath((endpoints.Item1, pathingBuilder.currentSnappedNode.transform.position));
            }
            else
            {
                UpdateGuidePath((endpoints.Item1, mouseRaycast.GetPosition()));
            }

            if (pathingBuilder.pathBuildable)
            {
                guidePathObject.GetComponent<Renderer>().material = guideEnabledMaterial;
            }
            else
            {
                guidePathObject.GetComponent<Renderer>().material = guideDisabledMaterial;
            }
        }
        #endregion
    }

    void OnDestroy()
    {
        DestroyGuides();
        Destroy(guideMouseObject);
        guideMouseObject = null;
    }

    void DestroyGuides()
    {
        Destroy(guidePointObject);
        Destroy(guidePathObject);
        guidePointObject = null;
        guidePathObject = null;
    }

    void InitializeGuideMouse()
    {
        // Create guide mouse object
        guideMouseObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        guideMouseObject.name = guideMouseName;
        guideMouseObject.transform.SetParent(guideHandlerObject.transform);
        guideMouseObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        // Set colliders
        GameObject collisionHolder = new GameObject("Collisions");
        collisionHolder.transform.SetParent(guideMouseObject.transform);
        int currentSide = 1;
        for (int i = 0; i < 5; i++)
        {
            // Create collider object
            GameObject collider = new GameObject("MouseCollider");
            collider.transform.SetParent(collisionHolder.transform);
            collider.transform.position = guideMouseObject.transform.position;

            // Add collision components
            collider.layer = LayerMask.NameToLayer("Ignore Raycast");
            collider.AddComponent<SphereCollider>();
            collider.GetComponent<SphereCollider>().isTrigger = true;
            collider.GetComponent<SphereCollider>().radius = 1.3f;
            collider.AddComponent<Rigidbody>();
            collider.GetComponent<Rigidbody>().isKinematic = true;
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

        // Set transforms for guide mouse
        guideMouseObject.transform.localScale += new Vector3(-0.6f, -0.9f, -0.6f);
    }

    void UpdateGuideMouse()
    {
        if (pathingBuilder.currentSnappedNode != null)
        {
            guideMouseObject.transform.position = pathingBuilder.currentSnappedNode.transform.position;
        }
        else
        {
            guideMouseObject.transform.position = mouseRaycast.GetPosition();
        }
    }

    void InitializeGuidePoint(Vector3 initialPoint)
    {
        // Create guide point object
        GameObject guideObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        guideObject.name = guidePointName;
        guideObject.GetComponent<Collider>().enabled = false;

        // Set transforms for guide point
        guideObject.transform.localScale += new Vector3(-0.6f, -0.9f, -0.6f);
        if (pathingBuilder.currentSnappedNode != null)
        {
            guideObject.transform.position = pathingBuilder.currentSnappedNode.transform.position;
        }
        else
        {
            guideObject.transform.position = initialPoint;
        }

        // Set parent for guide point
        guideObject.transform.SetParent(guideHandlerObject.transform);

        // Set rendering
        guideObject.GetComponent<Renderer>().material = guideEnabledMaterial;

        guidePointObject = guideObject;
    }

    void InitializeGuidePath((Vector3, Vector3) initialEndpoints)
    {
        // Create guide path object
        guidePathObject = new GameObject(guidePathName);
        guidePathObject.transform.SetParent(guideHandlerObject.transform);

        // Get offset points (prevent z-axis fighting on terrain)
        Vector3 offsetVector = new Vector3(0, meshOffset + 0.01f, 0);
        (Vector3, Vector3) offsetEndpoints = (initialEndpoints.Item1 + offsetVector, initialEndpoints.Item2 + offsetVector);

        // Calculate spaced points
        Vector3[] spacedPoints = PathUtilities.CalculateEvenlySpacedPoints(offsetEndpoints, pointSpacing, pointResolution);

        // Add path component to handle mesh
        guidePathObject.AddComponent<Path>();
        Path pathComponent = guidePathObject.GetComponent<Path>();
        pathComponent.UpdateVariables(gameObject.GetComponent<PathBuilder>());
        pathComponent.InitializeMesh("GuideCollider", true);
    }

    void UpdateGuidePath((Vector3, Vector3) updateEndpoints)
    {
        // Get offset points (prevent z-axis fighting on terrain)
        Vector3 offsetVector = new Vector3(0, meshOffset + 0.01f, 0);
        (Vector3, Vector3) offsetEndpoints = (updateEndpoints.Item1 + offsetVector, updateEndpoints.Item2 + offsetVector);

        // Calculate spaced points
        Vector3[] spacedPoints = PathUtilities.CalculateEvenlySpacedPoints(offsetEndpoints, pointSpacing, pointResolution);

        // Add path component to handle mesh
        Path pathComponent = guidePathObject.GetComponent<Path>();
        pathComponent.UpdateVariables(gameObject.GetComponent<PathBuilder>());
        pathComponent.UpdateMesh(offsetEndpoints);
    }
}

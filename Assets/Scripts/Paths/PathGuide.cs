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
    private Material guideDisabledMaterial;

    // Path builder vars
    private PathBuilder pathBuilderScript;
    private GameObject buildingObject;
    private float spacing;
    private float pathWidth;
    private float offset;

    // Collision point vars
    private GameObject collisionHolder;
    private Vector3[] prevCollisionPoints;
    private GameObject[] prevCollisionObjects;
    public bool canBuild = true;

    void Start()
    {
        pathBuilderScript = gameObject.GetComponent<PathBuilder>();
        buildingObject = pathBuilderScript.buildingObject;
        guideMaterial = pathBuilderScript.guideMaterial;
        guideDisabledMaterial = pathBuilderScript.guideDisabledMaterial;
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
                if (canBuild)
                {
                    initialGuidePoint = Vector3.zero;
                    DeleteGuidePoint();
                    DeleteGuidePath();
                }
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
            if (guidePathObject == null)
            {
                CreateGuidePath();
            }
            else
            {
                UpdateGuidePath(position);
            }
        }
        #endregion

        #region CheckGuide
        if (guidePathObject != null)
        {
            // Check if the guide path intersects with built paths
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

    void CreateGuidePath()
    {
        // Create guide path object
        GameObject guideObject = new GameObject();
        guideObject.name = guidePathName;

        // Set parent for guide path
        guidePathObject = guideObject;
        guidePathObject.transform.SetParent(gameObject.transform);

        // Add components to draw mesh
        guidePathObject.AddComponent<PathCreator>();
        guidePathObject.AddComponent<MeshFilter>();
        guidePathObject.AddComponent<MeshRenderer>();

        // Set material
        guidePathObject.GetComponent<Renderer>().material = guideMaterial;

        // Create empty game object for collision points
        GameObject collisionObject = new GameObject();
        collisionObject.name = "GuideCollisions";
        collisionObject.transform.SetParent(guidePathObject.transform);
        collisionHolder = collisionObject;
    }

    void UpdateGuidePath(Vector3 mousePosition)
    {
        // Get points and vars for guide
        (Vector3, Vector3) pathGuideTuple = (initialGuidePoint, mousePosition);
        float pointSpacing = pathBuilderScript.pointSpacing;
        float resolution = pathBuilderScript.resolution;

        // Get offsetted spaced points on guide path
        Vector3 offsetVector = new Vector3(0, offset + 0.01f, 0);
        (Vector3, Vector3) offsetPoints = (pathGuideTuple.Item1 + offsetVector, pathGuideTuple.Item2 + offsetVector);
        Vector3[] evenPoints = PathUtilities.CalculateEvenlySpacedPoints(offsetPoints, pointSpacing, resolution);

        // Refresh path component
        Destroy(guidePathObject.GetComponent<PathCreator>());
        guidePathObject.AddComponent<PathCreator>();
        guidePathObject.GetComponent<PathCreator>().CreatePath(evenPoints, pathWidth);

        #region CollisionPoints
        // Recalculate number of points
        int currCollisionPoints = 0;
        List<Vector3> collisionPointsList = new List<Vector3>();
        for (int i = 0; i < evenPoints.Length; i++)
        {
            // Every 10 points
            if ((i % 10) == 0)
            {
                collisionPointsList.Add(evenPoints[i]);
                currCollisionPoints++;
            }
        }

        // Set to array instead
        Vector3[] collisionPoints = collisionPointsList.ToArray();

        // Handle current collision points
        if (prevCollisionPoints == null)
        {
            #region HandleNoPoints
            // Set array for next frame
            prevCollisionPoints = collisionPoints;

            List<GameObject> gameObjects = new List<GameObject>();

            // Create collision spheres
            for (int i = 0; i < collisionPoints.Length; i++)
            {
                // Create collision sphere object
                GameObject collisionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                collisionSphere.name = "GuideCollider";

                // Set transforms for collision object
                collisionSphere.transform.SetParent(collisionHolder.transform);
                collisionSphere.transform.position = collisionPoints[i];

                // Set rendering and layer
                collisionSphere.GetComponent<Renderer>().enabled = false;
                collisionSphere.layer = LayerMask.NameToLayer("Ignore Raycast");

                // Set collider trigger
                collisionSphere.GetComponent<SphereCollider>().isTrigger = true;

                // Set rigidbody
                collisionSphere.AddComponent<Rigidbody>();
                Rigidbody rigidBody = collisionSphere.GetComponent<Rigidbody>();
                rigidBody.isKinematic = true;
                // rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                // Set guide check script
                collisionSphere.AddComponent<GuideCheck>();

                gameObjects.Add(collisionSphere);
            }

            // Set array for next frame
            prevCollisionObjects = gameObjects.ToArray();
            #endregion
        }
        else if (currCollisionPoints > prevCollisionPoints.Length)
        {
            #region HandleMorePoints
            List<GameObject> gameObjects = new List<GameObject>();
            gameObjects.AddRange(prevCollisionObjects);

            // Move existing points
            for (int i = 0; i < prevCollisionPoints.Length; i++)
            {
                prevCollisionObjects[i].gameObject.transform.position = collisionPoints[i];
            }

            // Add new points
            for (int i = prevCollisionPoints.Length; i < currCollisionPoints; i++)
            {
                // Create collision sphere object
                GameObject collisionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                collisionSphere.name = "GuideCollider";

                // Set transforms for collision object
                collisionSphere.transform.SetParent(collisionHolder.transform);
                collisionSphere.transform.position = collisionPoints[i];

                // Set rendering and layer
                collisionSphere.GetComponent<Renderer>().enabled = false;
                collisionSphere.layer = LayerMask.NameToLayer("Ignore Raycast");

                // Set collider trigger
                collisionSphere.GetComponent<SphereCollider>().isTrigger = true;

                // Set rigidbody
                collisionSphere.AddComponent<Rigidbody>();
                Rigidbody rigidBody = collisionSphere.GetComponent<Rigidbody>();
                rigidBody.isKinematic = true;

                // Set guide check script
                collisionSphere.AddComponent<GuideCheck>();

                gameObjects.Add(collisionSphere);
            }

            // Set arrays for next frame
            prevCollisionObjects = gameObjects.ToArray();
            prevCollisionPoints = collisionPoints;
            #endregion
        }
        else if (currCollisionPoints < prevCollisionPoints.Length)
        {
            #region HandleLessPoints
            int difference = prevCollisionPoints.Length - currCollisionPoints;

            List<GameObject> gameObjects = new List<GameObject>();
            gameObjects.AddRange(prevCollisionObjects);

            // Move existing points
            for (int i = 0; i < currCollisionPoints; i++)
            {
                prevCollisionObjects[i].gameObject.transform.position = collisionPoints[i];
            }

            // Delete extra points
            for (int i = prevCollisionPoints.Length; i > currCollisionPoints; i--)
            {
                Destroy(gameObjects[i - 1]);
                gameObjects.RemoveAt(i - 1);
            }

            // Set arrays for next frame
            prevCollisionObjects = gameObjects.ToArray();
            prevCollisionPoints = collisionPoints;
            #endregion
        }
        else
        {
            for (int i = 0; i < prevCollisionObjects.Length; i++)
            {
                prevCollisionObjects[i].gameObject.transform.position = collisionPoints[i];
            }
            prevCollisionPoints = collisionPoints;
        }
        #endregion

        // Update material
        GameObject guideObject = null;
        foreach (Transform child in gameObject.transform)
        {
            if (child.gameObject.name == guidePathName) guideObject = child.gameObject;
        }

        bool existsCollision = false;
        foreach (Transform child in guideObject.transform.gameObject.transform.GetChild(0))
        {
            if (child.gameObject.GetComponent<GuideCheck>().GetCollision() == true)
            {
                existsCollision = true;
                break;
            }
        }
        canBuild = !existsCollision;
        if (existsCollision)
        {
            guidePathObject.GetComponent<Renderer>().material = guideDisabledMaterial;
        }
        else
        {
            guidePathObject.GetComponent<Renderer>().material = guideMaterial;
        }
    }

    void DeleteGuidePath()
    {
        if (guidePathObject != null) Destroy(guidePathObject);

        prevCollisionObjects = null;
        prevCollisionPoints = null;
    }
}

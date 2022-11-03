using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class PathBuilder : MonoBehaviour
{
    private (Vector3, Vector3) points;
    private bool buildMode = false;

    private GameObject pathsObject;

    [Header("Builder Variables")]
    public GameObject buildingObject;
    [SerializeField]
    private TextMeshProUGUI buildText;

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

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.GetComponent<PathGuide>() == null) gameObject.AddComponent<PathGuide>();

        pathsObject = GameObject.Find("Paths");
        if (pathsObject == null)
        {
            pathsObject = new GameObject();
            pathsObject.name = "Paths";
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check for build mode
        if (!buildMode) return;

        // Disable guide over UI
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // Raycast to mouse position
        RaycastHit hitInfo = new RaycastHit();
        bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
        var position = new Vector3(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z);
        if (!hit) return;

        #region InputLeftMouse
        if (Input.GetMouseButtonDown(0))
        {
            // Check if first point is zero
            if (points.Item1 == Vector3.zero)
            {
                points.Item1 = position;
            }
            else
            {
                if (gameObject.GetComponent<PathGuide>().canBuild)
                {
                    points.Item2 = position;
                    DrawPath();
                    points.Item1 = Vector3.zero;
                    points.Item2 = Vector3.zero;
                }
            }
        }
        #endregion

        #region InputRightMouse
        if (Input.GetMouseButtonDown(1))
        {
            if (points.Item1 != Vector3.zero)
            {
                points.Item1 = Vector3.zero;
            }
        }
        #endregion
    }

    public void ToggleBuild()
    {
        buildMode = !buildMode;
        if (buildMode)
        {
            buildText.text = "Build ON";
        }
        else
        {
            buildText.text = "Build OFF";
        }

        gameObject.GetComponent<PathGuide>().ToggleBuild();
    }

    void DrawPath()
    {
        // Create path object
        GameObject path = new GameObject();
        path.name = "Path";

        // Set parent for path object
        path.transform.SetParent(pathsObject.transform);

        // Get offsetted spaced points on path
        Vector3 offsetVector = new Vector3(0, offset, 0);
        (Vector3, Vector3) offsetPoints = (points.Item1 + offsetVector, points.Item2 + offsetVector);
        Vector3[] evenPoints = PathUtilities.CalculateEvenlySpacedPoints(offsetPoints, pointSpacing, resolution);

        // Set spheres to show points
        if (visualizePoints) VisualizePoints(path, evenPoints);

        // Add components to draw mesh
        path.AddComponent<PathCreator>();
        path.AddComponent<MeshFilter>();
        path.AddComponent<MeshRenderer>();
        path.GetComponent<PathCreator>().CreatePath(evenPoints, pathWidth);

        // Set material for guide path point
        if (pathMaterial != null)
        {
            Renderer renderer = path.GetComponent<Renderer>();
            renderer.material = pathMaterial;

            // Change material tiling based on number of points
            float tiling = (-0.11f * spacing) * (evenPoints.Length / spacing);
            renderer.material.mainTextureScale = new Vector2(1, tiling);
        }

        // Update collision spheres
        List<Vector3> collisionPointsList = new List<Vector3>();
        for (int i = 0; i < evenPoints.Length; i++)
        {
            // Every 5 points
            if ((i % 10) == 0)
            {
                collisionPointsList.Add(evenPoints[i]);
            }
        }

        Vector3[] collisionPoints = collisionPointsList.ToArray();
        GameObject collisionHolder = new GameObject();
        collisionHolder.name = "CollisionsPath";

        collisionHolder.transform.SetParent(path.transform);

        for (int i = 0; i < collisionPoints.Length; i++)
        {
            GameObject collisionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            collisionSphere.name = "PathCollider";
            collisionSphere.transform.SetParent(collisionHolder.transform);
            collisionSphere.transform.position = collisionPoints[i];
            collisionSphere.GetComponent<Renderer>().enabled = false;
            collisionSphere.layer = LayerMask.NameToLayer("Ignore Raycast");

            collisionSphere.AddComponent<Rigidbody>();
            collisionSphere.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    void VisualizePoints(GameObject path, Vector3[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            // Create point object
            GameObject pointObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pointObject.name = "Point" + (i + 1);
            pointObject.GetComponent<Collider>().enabled = false;

            // Set transforms for point object
            pointObject.transform.localScale += new Vector3(-0.85f, -0.85f, -0.85f);
            pointObject.transform.position = points[i];

            // Set parent for point object
            pointObject.transform.SetParent(path.transform);
        }
    }
}

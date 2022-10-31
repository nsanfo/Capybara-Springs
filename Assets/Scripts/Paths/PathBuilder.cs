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
    [Header("Builder Variables")]
    public Material guideMaterial;

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
        var position = new Vector3(hitInfo.point.x, hitInfo.point.y - 0.06f, hitInfo.point.z);
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
                points.Item2 = position;
                DrawPath();
                points.Item1 = Vector3.zero;
                points.Item2 = Vector3.zero;
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
        GameObject path = new GameObject();
        path.name = "Path";
        path.transform.SetParent(pathsObject.transform);

        Vector3[] evenPoints = PathUtilities.CalculateEvenlySpacedPoints(points, 0.5f, 1);
        for (int i = 0; i < evenPoints.Length; i++)
        {
            GameObject shape = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shape.GetComponent<Collider>().enabled = false;
            shape.transform.SetParent(path.transform);
            shape.name = "Point" + (i + 1);

            shape.transform.localScale += new Vector3(-0.85f, -0.85f, -0.85f);
            shape.transform.position = evenPoints[i];
        }
    }
}

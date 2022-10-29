using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class PathBuilder : MonoBehaviour
{
    public bool buildMode = false;
    public TextMeshProUGUI buildText;

    private (Vector3, Vector3) points;

    public Material materialRef;

    [SerializeField]
    private float meshWidth = 1.5f;

    void Update()
    {
        // Check for build mode
        if (!buildMode)
        {
            return;
        }

        // Disable guide over UI
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // Raycast to mouse position
        RaycastHit hitInfo = new RaycastHit();
        bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
        var position = new Vector3(hitInfo.point.x, hitInfo.point.y - 0.06f, hitInfo.point.z);
        if (!hit)
        {
            return;
        }

        #region MouseInput
        // Check for left-click (Creates guide points and path)
        if (Input.GetMouseButtonDown(0))
        {
            // Check if tuple isn't empty
            if ((points.Item1 != Vector3.zero) && (points.Item2 != Vector3.zero))
            {
                return;
            }

            // Check if first point is zero
            if (points.Item1 == Vector3.zero)
            {
                points.Item1 = position;
                CreateGuidePoint(points.Item1);
            }
            else
            {
                // Check if second point is zero
                if (points.Item2 == Vector3.zero)
                {
                    points.Item2 = position;
                    DrawPath();
                }
            }
        }
        // Check for right-click (Cancels path building)
        if (Input.GetMouseButtonDown(1))
        {
            if (points.Item1 != Vector3.zero && points.Item2 == Vector3.zero)
            {
                // Reset guides
                DeleteGuidePoint();
                DeleteGuidePath();
            }
        }

        #endregion

        #region GuidePoint
        // Check if guide exists to continue
        GameObject guideShape = GameObject.Find("GuidePoint");
        if (guideShape == null)
        {
            return;
        }

        // Only tracks mouse to the game object named "Terrain"
        if (hitInfo.collider.gameObject.name == "Terrain")
        {
            guideShape.transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y - 0.06f, hitInfo.point.z);
        }
        #endregion

        #region GuidePath
        if (points.Item1 != Vector3.zero)
        {
            CreateGuidePath(position);
        }
        #endregion
    }

    void DrawPath()
    {
        GameObject pathsObject = GameObject.Find("Paths");
        if (pathsObject == null)
        {
            return;
        }

        GameObject path = new GameObject();
        path.name = "Path";
        path.transform.SetParent(pathsObject.transform);

        Vector3[] evenPoints = CalculateEvenlySpacedPoints(points, 0.5f, 1);
        for (int i = 0; i < evenPoints.Length; i++)
        {
            GameObject shape = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shape.GetComponent<Collider>().enabled = false;
            shape.transform.SetParent(path.transform);
            shape.name = "Point" + (i + 1);

            shape.transform.localScale += new Vector3(-0.85f, -0.85f, -0.85f);
            shape.transform.position = evenPoints[i];
        }

        // Reset guides
        DeleteGuidePoint();
        DeleteGuidePath();
    }

    void CreateGuide()
    {
        GameObject guideShape = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        guideShape.name = "GuidePoint";
        guideShape.GetComponent<Collider>().enabled = false;

        guideShape.transform.localScale += new Vector3(-0.6f, -0.9f, -0.6f);
        guideShape.transform.position = new Vector3(0f, -2.0f, 0f);

        GameObject pathsHoldingObject = GameObject.Find("PathBuilder");
        if (pathsHoldingObject != null)
        {
            guideShape.transform.SetParent(pathsHoldingObject.transform);
        }
    }

    void DeleteGuide()
    {
        GameObject guideShape = GameObject.Find("GuidePoint");
        if (guideShape != null)
        {
            Destroy(guideShape);
        }
    }

    void CreateGuidePoint(Vector3 position)
    {
        GameObject pointShape = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pointShape.name = "GuidePoint";
        pointShape.GetComponent<Collider>().enabled = false;

        pointShape.transform.localScale += new Vector3(-0.7f, -0.9f, -0.7f);
        pointShape.transform.position = position;

        pointShape.GetComponent<Renderer>().material = materialRef;

        GameObject pathBuilder = GameObject.Find("PathBuilder");
        if (pathBuilder != null)
        {
            pointShape.transform.SetParent(pathBuilder.transform);
        }
    }

    void DeleteGuidePoint()
    {
        GameObject guidePoint = GameObject.Find("GuidePoint");
        if (guidePoint != null)
        {
            Destroy(guidePoint);
        }

        points.Item1 = Vector3.zero;
        points.Item2 = Vector3.zero;
    }

    void CreateGuidePath(Vector3 raycastPosition)
    {
        (Vector3, Vector3) guideTuple = (points.Item1, raycastPosition);
        Vector3[] spacedPoints = CalculateEvenlySpacedPoints(guideTuple, 0.5f, 1);

        GameObject tempPath = GameObject.Find("GuidePath");
        if (tempPath != null)
        {
            Destroy(tempPath);
        }

        GameObject pathBuilder = GameObject.Find("PathBuilder");
        if (pathBuilder == null)
        {
            return;
        }

        tempPath = new GameObject();
        tempPath.name = "GuidePath";
        tempPath.transform.SetParent(pathBuilder.transform);

        for (int i = 0; i < spacedPoints.Length; i++)
        {
            GameObject tempPathShape = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tempPathShape.GetComponent<Collider>().enabled = false;
            tempPathShape.transform.SetParent(tempPath.transform);
            tempPathShape.name = "GuidePathPoint" + (i + 1);

            tempPathShape.transform.localScale += new Vector3(-0.85f, -0.85f, -0.85f);
            tempPathShape.transform.position = spacedPoints[i];
        }
    }

    void DeleteGuidePath()
    {
        GameObject guidePath = GameObject.Find("GuidePath");
        if (guidePath != null)
        {
            Destroy(guidePath);
        }
    }

    public Vector3[] CalculateEvenlySpacedPoints((Vector3, Vector3) pointTuple, float spacing, float resolution = 1)
    {
        List<Vector3> evenlySpacedPoints = new List<Vector3>();
        evenlySpacedPoints.Add(pointTuple.Item1);
        Vector3 previousPoint = pointTuple.Item1;

        float distSinceLastEvenPoint = 0;

        float controlNetLength = Vector3.Distance(pointTuple.Item1, pointTuple.Item2);
        float estimatedLength = Vector3.Distance(pointTuple.Item1, pointTuple.Item2) + controlNetLength / 2f;
        int divisions = Mathf.CeilToInt(estimatedLength * resolution * 10);
        float t = 0;
        while (t <= 1)
        {
            t += 0.1f / divisions;
            Vector3 pointOnCurve = Bezier.EvaluateLinear(pointTuple.Item1, pointTuple.Item2, t);
            distSinceLastEvenPoint += Vector3.Distance(previousPoint, pointOnCurve);

            while (distSinceLastEvenPoint >= spacing)
            {
                float overshootDist = distSinceLastEvenPoint - spacing;
                Vector3 newEvenlySpacedPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshootDist;
                evenlySpacedPoints.Add(newEvenlySpacedPoint);
                distSinceLastEvenPoint = overshootDist;
                previousPoint = newEvenlySpacedPoint;
            }

            previousPoint = pointOnCurve;
        }

        return evenlySpacedPoints.ToArray();
    }

    public void ToggleBuild()
    {
        buildMode = !buildMode;
        if (buildMode)
        {
            CreateGuide();

            buildText.text = "Build ON";
        }
        else
        {
            DeleteGuide();
            DeleteGuidePoint();
            DeleteGuidePath();

            buildText.text = "Build OFF";
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PathUtilities
{
    public static Vector3[] CalculateEvenlySpacedPoints((Vector3, Vector3) pointTuple, float spacing, float resolution = 1)
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

        //evenlySpacedPoints.Add(pointTuple.Item2);

        return evenlySpacedPoints.ToArray();
    }
}

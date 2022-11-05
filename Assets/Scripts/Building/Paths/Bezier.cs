using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Bezier
{
    public static Vector3 EvaluateLinear(Vector3 a, Vector3 b, float t)
    {
        return Vector3.Lerp(a, b, t);
    }


    //public static Vector3 EvaluateQuad(Vector3 a, Vector3 b)
}

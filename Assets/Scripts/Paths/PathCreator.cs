using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
    public void UpdatePath(Vector3[] points, float width)
    {
        gameObject.GetComponent<MeshFilter>().mesh = CreatePathMesh(points, width);
    }

    Mesh CreatePathMesh(Vector3[] points, float pathWidth)
    {
        Vector3[] verts = new Vector3[points.Length * 2];
        Vector2[] uvs = new Vector2[verts.Length];
        int[] tris = new int[2 * (points.Length - 1) * 3];
        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 forward = Vector3.zero;

            // Last point
            if (i < points.Length - 1)
            {
                forward += points[i + 1] - points[i];
            }

            // First point
            if (i > 0)
            {
                forward += points[i] - points[i - 1];
            }

            forward.Normalize();
            Vector3 left = new Vector3(forward.z, forward.y, -forward.x);

            verts[vertIndex] = points[i] + left * pathWidth * 0.5f;
            verts[vertIndex + 1] = points[i] - left * pathWidth * 0.5f;

            float completePercent = i / (float)points.Length - 1;
            uvs[vertIndex] = new Vector2(0, completePercent);
            uvs[vertIndex + 1] = new Vector2(1, completePercent);

            if (i < points.Length - 1)
            {
                // Create poly CCW
                tris[triIndex] = vertIndex;
                tris[triIndex + 1] = vertIndex + 1;
                tris[triIndex + 2] = vertIndex + 2;

                tris[triIndex + 3] = vertIndex + 3;
                tris[triIndex + 4] = vertIndex + 2;
                tris[triIndex + 5] = vertIndex + 1;
            }

            vertIndex += 2;
            triIndex += 6;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;

        return mesh;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Path : MonoBehaviour
{
    // Path point variables
    public (Vector3, Vector3) endpoints;
    public Vector3[] spacedPoints;
    public Vector3[] collisionPoints;

    private GameObject collisionObject;

    public Mesh mesh;

    public void SetMesh(float pathWidth)
    {
        if ((endpoints.Item1 == null || endpoints.Item2 == null) || (spacedPoints == null))
        {
            Debug.LogError("Endpoints or spacedpoints incomplete to create path mesh");
            return;
        }

        // Update mesh
        mesh = PathUtilities.CreateMesh(spacedPoints, pathWidth);
        gameObject.AddComponent<MeshFilter>();
        gameObject.GetComponent<MeshFilter>().mesh = mesh;

        CalculateCollisionPoints();
    }

    public void UpdateMesh(float pathWidth)
    {
        if (mesh == null)
        {
            Debug.LogError("Mesh must be set before updating it");
            return;
        }

        mesh = PathUtilities.CreateMesh(spacedPoints, pathWidth);
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        CalculateCollisionPoints();
    }

    public void SetRendering(Material material, float spacing)
    {
        // Change material tiling based on number of points
        float tiling = (-0.11f * spacing) * (spacedPoints.Length / spacing);

        gameObject.AddComponent<MeshRenderer>();
        Renderer renderer = GetComponent<Renderer>();
        renderer.material = material;
        renderer.material.mainTextureScale = new Vector2(1, tiling);
    }

    // Add method to update paths (for instances of path end connections)

    void CalculateCollisionPoints()
    {
        // Update collision spheres
        List<Vector3> collisionPointsList = new List<Vector3>();
        for (int i = 0; i < spacedPoints.Length; i++)
        {
            // Every 10 points
            if ((i % 10) == 0)
            {
                collisionPointsList.Add(spacedPoints[i]);
            }
        }

        collisionPoints = collisionPointsList.ToArray();
    }

    public void CreateCollision(string colliderName, bool enableTrigger)
    {
        if (collisionPoints == null)
        {
            Debug.LogError("Endpoints or spacedpoints incomplete to create path mesh");
            return;
        }

        // Create collision holder object
        collisionObject = new GameObject("Collisions");
        collisionObject.transform.SetParent(gameObject.transform);

        // Create collision objects for path
        for (int i = 0; i < collisionPoints.Length; i++)
        {
            // Create collision sphere object
            GameObject collisionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            collisionSphere.name = colliderName;

            // Set transforms for collision object
            collisionSphere.transform.SetParent(collisionObject.transform);
            collisionSphere.transform.position = collisionPoints[i];

            // Disable rendering and ignore raycasting on sphere
            collisionSphere.GetComponent<Renderer>().enabled = false;
            collisionSphere.layer = LayerMask.NameToLayer("Ignore Raycast");

            // Set collision component
            collisionSphere.AddComponent<GuideCheck>();
            collisionSphere.AddComponent<Rigidbody>();
            collisionSphere.GetComponent<Rigidbody>().isKinematic = true;

            // Set collider trigger
            collisionSphere.GetComponent<SphereCollider>().isTrigger = enableTrigger;
        }
    }

    public void DestroyCollison()
    {
        if (collisionObject == null) return;

        Destroy(collisionObject);
        collisionObject = null;
    }
}

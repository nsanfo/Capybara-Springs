using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Path : MonoBehaviour
{
    // Path point variables
    public (Vector3, Vector3) endpoints;
    public Vector3[] spacedPoints;
    public Vector3[] collisionPoints;

    // Mesh for path
    public Mesh mesh;

    // Collision object to hold colliders
    private GameObject collisionObject;

    // Node object to hold nodes
    private GameObject nodeObject;
    private GameObject[] nodes;

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

        collisionPointsList.Add(spacedPoints[spacedPoints.Length - 1]);

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
            collisionSphere.AddComponent<PathColliderTrigger>();
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

    public void InitializeNodes(GameObject nodeModel, RuntimeAnimatorController nodeAnimatorController)
    {
        GameObject nodeHolder = new GameObject("Nodes");
        nodeHolder.transform.SetParent(gameObject.transform);
        nodeObject = nodeHolder;

        nodes = new GameObject[2];

        GameObject node1 = Instantiate(nodeModel, endpoints.Item1, Quaternion.identity);
        node1.name = "PathNode";
        node1.transform.SetParent(nodeObject.transform);
        node1.AddComponent<PathNode>();
        //node1.GetComponent<PathNode>().animatorController = nodeAnimatorController;
        node1.GetComponent<PathNode>().InitializeAnimator(nodeAnimatorController);
        nodes[0] = node1;

        GameObject node2 = Instantiate(nodeModel, endpoints.Item2, Quaternion.identity);
        node2.name = "PathNode";
        node2.transform.SetParent(nodeObject.transform);
        node2.AddComponent<PathNode>();
        //node2.GetComponent<PathNode>().animatorController = nodeAnimatorController;
        node2.GetComponent<PathNode>().InitializeAnimator(nodeAnimatorController);
        nodes[1] = node2;
    }

    public GameObject[] GetNodes()
    {
        return nodes;
    }
}

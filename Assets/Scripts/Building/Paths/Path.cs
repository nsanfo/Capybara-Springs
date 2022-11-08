using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Path : MonoBehaviour
{
    // Path mesh
    private Mesh mesh;

    // Path settings
    private float meshOffset;
    private float meshWidth;
    private float meshSpacing;
    private float pointSpacing;
    private float pointResolution;

    // Guide materials
    private Material guideDefaultMaterial;

    // Path material
    private Material pathMaterial;

    // Path variables
    private (Vector3, Vector3) endpoints;
    private Vector3[] spacedPoints;
    private Vector3[] collisionPoints;

    // Collision holder
    private GameObject collisionHolder;
    private GameObject[] colliders;

    // Colider variables
    string colliderName;
    bool setColliderTrigger;

    // Node holder
    private GameObject nodeHolder;
    private GameObject[] nodes;

    // Node variables
    private GameObject nodePrefab;
    private RuntimeAnimatorController nodeAnimatorController;

    public void UpdateVariables(PathBuilder pathBuilderScript)
    {
        // Get variables from path builder script
        meshOffset = pathBuilderScript.meshOffset;
        meshWidth = pathBuilderScript.meshWidth;
        meshSpacing = pathBuilderScript.meshSpacing;
        pointSpacing = pathBuilderScript.pointSpacing;
        pointResolution = pathBuilderScript.pointResolution;

        // Get guide material from path builder script
        guideDefaultMaterial = pathBuilderScript.guideDefaultMaterial;

        // Get material from path builder script
        pathMaterial = pathBuilderScript.pathMaterial;

        // Get node variables from path builder script
        nodePrefab = pathBuilderScript.nodePrefab;
        nodeAnimatorController = pathBuilderScript.nodeAnimatorController;

        // Get endpoints from path builder script
        endpoints = pathBuilderScript.endpoints;

        // Get evenly spaced points between endpoints
        Vector3 offsetVector = new Vector3(0, meshOffset, 0);
        (Vector3, Vector3) offsetEndpoints = (endpoints.Item1 + offsetVector, endpoints.Item2 + offsetVector);
        spacedPoints = PathUtilities.CalculateEvenlySpacedPoints(offsetEndpoints, pointSpacing, pointResolution);

    }

    public void InitializeMesh(string inputColliderName, bool isGuide)
    {
        colliderName = inputColliderName;
        if (isGuide) setColliderTrigger = true;

        SetMesh();
        SetMaterialRendering(isGuide);
        CreateCollisions();
        if (!isGuide) InitializeNodes();
    }

    private void SetMesh()
    {
        // Add mesh components
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();

        mesh = PathUtilities.CreateMesh(spacedPoints, meshWidth);
        gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    public void UpdateMesh((Vector3, Vector3) newEndpoints)
    {
        // Get evenly spaced points between endpoints
        Vector3 offsetVector = new Vector3(0, meshOffset, 0);
        (Vector3, Vector3) offsetEndpoints = (newEndpoints.Item1 + offsetVector, newEndpoints.Item2 + offsetVector);
        spacedPoints = PathUtilities.CalculateEvenlySpacedPoints(offsetEndpoints, pointSpacing, pointResolution);

        // Update mesh
        mesh = PathUtilities.CreateMesh(spacedPoints, meshWidth);
        gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;

        // Recalculate collision points
        SetCollisionPoints();
        Destroy(collisionHolder);
        CreateCollisions();
    }

    private void SetMaterialRendering(bool isGuide)
    {
        // Change material tiling based on number of points
        float tiling = (-0.11f * meshSpacing) * (spacedPoints.Length / meshSpacing);

        // Update renderer
        Renderer renderer = gameObject.GetComponent<Renderer>();
        if (isGuide)
        {
            renderer.material = guideDefaultMaterial;
        }
        else
        {
            renderer.material = pathMaterial;
        }
        
        renderer.material.mainTextureScale = new Vector2(1, tiling);
    }

    public void UpdateMaterial(Material material)
    {
        // Update renderer
        gameObject.GetComponent<Renderer>().material = material;
    }

    private void CreateCollisions()
    {
        SetCollisionPoints();

        List<GameObject> colliderList = new List<GameObject>();

        // Create collision holder object
        collisionHolder = new GameObject("Collisions");
        collisionHolder.transform.SetParent(gameObject.transform);

        // Create collision objects
        for (int i = 0; i < collisionPoints.Length; i++)
        {
            // Create collider objects
            GameObject collider = new GameObject(colliderName);
            collider.transform.SetParent(collisionHolder.transform);
            collider.transform.position = collisionPoints[i];

            // Add collision components
            collider.layer = LayerMask.NameToLayer("Ignore Raycast");
            collider.AddComponent<SphereCollider>();
            collider.GetComponent<SphereCollider>().isTrigger = setColliderTrigger;
            collider.AddComponent<Rigidbody>();
            collider.GetComponent<Rigidbody>().isKinematic = true;
            collider.AddComponent<PathColliderTrigger>();

            colliderList.Add(collider);
        }

        colliders = colliderList.ToArray();
    }

    private void SetCollisionPoints()
    {
        List<Vector3> collisionPointsList = new List<Vector3>();
        for (int i = 0; i < spacedPoints.Length; i++)
        {
            // Every 7 points, add collision
            if ((i % 7) == 0)
            {
                collisionPointsList.Add(spacedPoints[i]);
            }
        }

        collisionPointsList.Add(spacedPoints[spacedPoints.Length - 1]);

        collisionPoints = collisionPointsList.ToArray();
    }

    public void DestroyCollision()
    {
        if (collisionHolder == null) return;

        Destroy(collisionHolder);
        collisionHolder = null;
    }

    private void InitializeNodes()
    {
        // Create node holder object
        nodeHolder = new GameObject("Nodes");
        nodeHolder.transform.SetParent(gameObject.transform);

        nodes = new GameObject[2];

        // Set nodes
        for (int i = 0; i < 2; i++)
        {
            Vector3 nodePoint;
            if (i == 0)
            {
                nodePoint = endpoints.Item1;
            }
            else
            {
                nodePoint = endpoints.Item2;
            }

            GameObject node = Instantiate(nodePrefab, nodePoint, Quaternion.identity);
            node.name = "PathNode";
            node.transform.SetParent(nodeHolder.transform);

            // Add node component
            node.AddComponent<PathNode>();
            node.GetComponent<PathNode>().InitializeAnimator(nodeAnimatorController);

            nodes[i] = node;
        }
    }

    public GameObject[] GetNodes()
    {
        return nodes;
    }
}

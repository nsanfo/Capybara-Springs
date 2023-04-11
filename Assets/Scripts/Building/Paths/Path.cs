using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class Path : MonoBehaviour
{
    // Path settings
    private float meshWidth;
    private float meshSpacing;
    private float meshOffset = 0.0001f;
    private float pointSpacing;
    private float pointResolution;

    // Path material
    private Material pathMaterial;

    // Path variables
    private (Vector3, Vector3, Vector3) pathPoints;
    public Vector3[] spacedPoints;
    private Vector3[] collisionPoints;

    // Collision holder
    private GameObject collisionHolder;

    // Collider variables
    private string colliderName;
    private bool setColliderTrigger;

    // Node holder
    private GameObject nodeHolder;
    public PathNode[] nodes;

    // Node variables
    private GameObject nodePrefab;
    private Material nodeMaterial;

    // Amenity variables
    private List<Amenity> amenities = new List<Amenity>();
    public List<Amenity> Amenities { get => amenities; }

    public void UpdateVariables(PathBuilder pathBuilderScript, (Vector3, Vector3, Vector3) pathPoints)
    {
        // Set path settings
        meshWidth = pathBuilderScript.meshWidth;
        meshSpacing = pathBuilderScript.meshSpacing;
        pointSpacing = pathBuilderScript.pointSpacing;
        pointResolution = pathBuilderScript.pointResolution;

        // Get material from path builder script
        pathMaterial = pathBuilderScript.pathMaterial;

        // Get node variables
        nodePrefab = pathBuilderScript.nodePrefab;
        nodeMaterial = pathBuilderScript.nodeMaterial;

        // Set points
        this.pathPoints = pathPoints;
    }

    public void InitializeMesh(bool isGuide, NodeGraph nodeGraph)
    {
        if (isGuide)
        {
            colliderName = PathBuilder.GuideNames.GuideCollider.ToString();
        }
        else
        {
            colliderName = PathBuilder.PathNames.PathCollider.ToString();
        }

        setColliderTrigger = isGuide;

        SetMesh();
        SetMaterialRendering(isGuide);
        CreateCollisions();
        if (!isGuide) HandleNodes(nodeGraph);
        gameObject.GetComponent<MeshFilter>().sharedMesh.RecalculateNormals();
    }

    private void SetMesh()
    {
        SetPathSpacedPoints();

        // Add mesh components
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();

        // Set mesh
        gameObject.GetComponent<MeshFilter>().sharedMesh = PathUtilities.CreateMesh(spacedPoints, meshWidth);
        gameObject.GetComponent<MeshFilter>().sharedMesh.RecalculateNormals();
    }

    public void UpdateMesh()
    {
        SetPathSpacedPoints();
        SetCollisionPoints();
        UpdateCollisions();

        // Set mesh
        gameObject.GetComponent<MeshFilter>().sharedMesh = PathUtilities.CreateMesh(spacedPoints, meshWidth);
        gameObject.GetComponent<MeshFilter>().sharedMesh.RecalculateNormals();
    }

    private void SetMaterialRendering(bool isGuide)
    {
        // Change material tiling based on number of points
        float tiling = (-0.11f * meshSpacing) * (spacedPoints.Length / meshSpacing);

        // Update renderer
        Renderer renderer = gameObject.GetComponent<Renderer>();
        if (Application.isPlaying)
        {
            renderer.material = pathMaterial;
            renderer.material.mainTextureScale = new Vector2(1, tiling);
        } 
        else
        {
            renderer.sharedMaterial = pathMaterial;
            renderer.sharedMaterial.mainTextureScale = new Vector2(1, tiling);
        }
    }

    public void UpdateMaterial(Material material)
    {
        // Update renderer material
        gameObject.GetComponent<Renderer>().material = material;
    }

    private void SetCollisionPoints()
    {
        List<Vector3> collisionPointsList = new List<Vector3>();
        for (int i = 0; i < spacedPoints.Length; i++)
        {
            // Every 5 points, add collision
            if ((i % 5) == 0)
            {
                collisionPointsList.Add(spacedPoints[i]);
            }
        }

        collisionPointsList.Add(spacedPoints[spacedPoints.Length - 1]);

        collisionPoints = collisionPointsList.ToArray();
    }

    private void CreateCollisions()
    {
        SetPathSpacedPoints();
        SetCollisionPoints();

        // Create collision holder object
        collisionHolder = new GameObject(PathBuilder.PathNames.Collisions.ToString());
        collisionHolder.transform.SetParent(gameObject.transform);

        // Create collision objects
        for (int i = 0; i < collisionPoints.Length; i++)
        {
            PathUtilities.CreateCollider(colliderName, collisionHolder.transform, collisionPoints[i], setColliderTrigger);
        }
    }

    private void UpdateCollisions()
    {
        Transform collisionsTransform = gameObject.transform.Find(PathBuilder.PathNames.Collisions.ToString());

        // Add colliders to match points
        if (collisionPoints.Length > collisionsTransform.childCount)
        {
            // Transform existing colliders
            for (int i = 0; i < collisionsTransform.childCount; i++)
            {
                collisionsTransform.transform.GetChild(i).transform.position = collisionPoints[i];
            }

            // Create new colliders
            for (int i = collisionsTransform.childCount; i < collisionPoints.Length; i++)
            {
                PathUtilities.CreateCollider(colliderName, collisionHolder.transform, collisionPoints[i], setColliderTrigger);
            }
        }
        // Destroy colliders to match points
        else if (collisionPoints.Length < collisionsTransform.childCount)
        {
            // Transform existing colliders
            for (int i = 0; i < collisionPoints.Length; i++)
            {
                collisionsTransform.transform.GetChild(i).transform.position = collisionPoints[i];
            }

            // Destroy excess colliders
            for (int i = collisionPoints.Length; i < collisionsTransform.childCount; i++)
            {
                if (Application.isPlaying)
                {
                    Destroy(collisionsTransform.GetChild(i).gameObject);
                }
                else
                {
                    DestroyImmediate(collisionsTransform.GetChild(i).gameObject);
                }
            }
        }
        // Set new position of existing colliders
        else
        {
            for (int i = 0; i < collisionsTransform.childCount; i++)
            {
                collisionsTransform.transform.GetChild(i).transform.position = collisionPoints[i];
            }
        }
    }

    private void HandleNodes(NodeGraph nodeGraph)
    {
        // Create node holder object
        nodeHolder = new GameObject(PathBuilder.NodeNames.NodeHolder.ToString());
        nodeHolder.transform.SetParent(gameObject.transform);

        nodes = new PathNode[2];

        PathNode existingNode;

        // Set nodes
        for (int i = 0; i < 2; i++)
        {
            Vector3 nodePoint;
            if (i == 0)
            {
                nodePoint = pathPoints.Item1;
                
            }
            else
            {
                nodePoint = pathPoints.Item2;
            }

            existingNode = nodeGraph.CheckExistingNode(nodePoint);

            GameObject node;
            PathNode newNode = null;
            if (existingNode != null)
            {
                node = existingNode.gameObject;
            }
            else
            {
                node = new GameObject(PathBuilder.NodeNames.Node.ToString());
                node.transform.SetParent(nodeHolder.transform);

                GameObject nodeObject = Instantiate(nodePrefab, nodePoint, Quaternion.identity);
                nodeObject.name = PathBuilder.NodeNames.Node.ToString() + "Object";
                nodeObject.transform.SetParent(node.transform);
                newNode = nodeObject.AddComponent<PathNode>();
            }

            if (newNode == null)
            {
                nodes[i] = node.GetComponent<PathNode>();
            }
            else
            {
                nodes[i] = newNode;
            }

            nodes[i].SetMaterial(nodeMaterial, node.transform, meshOffset);
            nodes[i].AddPath(this);
        }
    }

    private void SetPathSpacedPoints()
    {
        if (pathPoints.Item3 != Vector3.zero)
        {
            spacedPoints = PathUtilities.CalculateSpacedPoints(pathPoints, true, pointSpacing, pointResolution);
        }
        else
        {
            spacedPoints = PathUtilities.CalculateSpacedPoints(pathPoints, false, pointSpacing, pointResolution);
        }
    }

    public void AddAmenity(Amenity param)
    {
        amenities.Add(param);
    }

    public void RemoveAmenity(Amenity param)
    {
        amenities.Remove(param);
    }
}

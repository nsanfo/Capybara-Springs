using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode : MonoBehaviour
{
    private Path[] connectedPaths = new Path[0];
    public bool snappedNode = false;
    
    public void SetMaterial(Material nodeMaterial, Transform nodeHolder, float meshOffset)
    {
        GameObject nodeMatObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        nodeMatObj.transform.SetParent(nodeHolder.transform);
        nodeMatObj.transform.position = new Vector3(this.transform.position.x, meshOffset, this.transform.position.z);
        nodeMatObj.transform.localScale = new Vector3(0.105f, 0.105f, 0.105f);
        nodeMatObj.tag = "PathNode";
        nodeMatObj.layer = 2;
        var collider = nodeMatObj.AddComponent<SphereCollider>();
        collider.radius = 3.1f;
        nodeMatObj.GetComponent<MeshRenderer>().material = nodeMaterial;
    }

    public void ShowNode()
    {
        gameObject.SetActive(true);
        gameObject.GetComponent<Animator>().Play("ShowPathNode");
    }

    public void HideNode()
    {
        gameObject.GetComponent<Animator>().Play("HidePathNode");
    }

    public void SnapNode()
    {
        gameObject.GetComponent<Animator>().Play("SnapPathNode");
    }

    public void UnsnapNode()
    {
        if (!snappedNode) gameObject.GetComponent<Animator>().Play("UnsnapPathNode");
    }

    public void SetInactive()
    {
        gameObject.SetActive(false);
    }

    public GameObject GetNodeGameObject()
    {
        return gameObject;
    }

    public Vector3 GetNodePosition()
    {
        return gameObject.transform.position;
    }

    public void AddPath(Path path)
    {
        List<Path> pathList = new List<Path>(connectedPaths);
        pathList.Add(path);

        connectedPaths = pathList.ToArray();
    }

    public Path[] GetPaths()
    {
        return connectedPaths;
    }
}

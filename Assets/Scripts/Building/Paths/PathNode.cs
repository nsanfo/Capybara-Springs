using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode : MonoBehaviour
{
    private Path[] connectedPaths = new Path[0];
    public bool snappedNode = false;
    public Material nodeMaterial;
    public void SetMaterial(Material nodeMaterial)
    {
        this.nodeMaterial = nodeMaterial;
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

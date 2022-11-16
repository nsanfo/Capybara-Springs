using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeController
{
    public bool nodesVisible = false;
    private Path[] paths;
    private PathNode[] nodes;

    public PathNode CheckExistingNode(Vector3 nodePosition)
    {
        if (paths == null) return null;

        for (int i = 0; i < paths.Length; i++)
        {
            for (int j = 0; j < paths[i].nodes.Length; j++)
            {
                if (paths[i].nodes[j].transform.position == nodePosition) return paths[i].nodes[j];
            }
        }

        return null;
    }

    public void AddPath(Path path)
    {
        if (paths == null)
        {
            paths = new Path[1];
            paths[0] = path;
        }
        else
        {
            List<Path> list = new List<Path>(paths);
            list.Add(path);
            paths = list.ToArray();
        }

        if (nodes == null)
        {
            nodes = path.nodes;
        }
        else
        {
            List<PathNode> list = new List<PathNode>(nodes);
            list.AddRange(path.nodes);
            nodes = list.ToArray();
        }
    }

    public void SetNodesVisibility(bool visible)
    {
        nodesVisible = visible;

        if (nodes == null) return;

        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodesVisible)
            {
                nodes[i].ShowNode();
            }
            else
            {
                nodes[i].HideNode();
            }
        }
    }

    public PathNode[] GetNodes()
    {
        return nodes;
    }
}

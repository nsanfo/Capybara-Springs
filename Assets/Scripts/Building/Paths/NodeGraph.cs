using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Networking.Types;
using static PathBuilder;
using Debug = UnityEngine.Debug;

public class NodeGraph
{
    public bool nodesVisible = false;
    public bool[,] matrix = new bool[0, 0];
    private PathNode[] nodes = new PathNode[0];

    public PathNode[] GetConnectedNodes(PathNode node)
    {
        int index = GetNodeIndex(node);

        if (index == -1) return new PathNode[0];

        PathNode[] connectedNodes = new PathNode[matrix.GetLength(1)];
        
        for (int i = 0; i < connectedNodes.Length; i++)
        {
            if (matrix[index, i]) connectedNodes[i] = nodes[i];
        }

        return connectedNodes;
    }

    public void AddPath(Path path)
    {
        PathNode[] pathNodes = path.nodes;

        List<PathNode> nodeList = new List<PathNode>(nodes);
        
        // Add nodes to array if node is new
        if (!nodeList.Contains(pathNodes[0])) AddNode(pathNodes[0]);
        if (!nodeList.Contains(pathNodes[1])) AddNode(pathNodes[1]);

        // Get index of nodes
        int index1 = GetNodeIndex(pathNodes[0]);
        int index2 = GetNodeIndex(pathNodes[1]);
        if (index1 == -1 || index2 == -1) return;

        // Modify matrix
        matrix[index1, index2] = true;
        matrix[index2, index1] = true;
    }

    void AddNode(PathNode node)
    {
        List<PathNode> nodeList = new List<PathNode>(nodes);
        nodeList.Add(node);

        nodes = nodeList.ToArray();
        ExpandMatrix();
        //AddNodeMaterial(node);
    }

    /*
    void AddNodeMaterial(PathNode node)
    {
        GameObject imageHolder = GameObject.Find(NodeNames.Node.ToString() + NodeNames.ImageHolder.ToString());
        if (imageHolder == null) imageHolder = new GameObject(NodeNames.Node.ToString() + NodeNames.ImageHolder.ToString());

        // Create new node sprite object
        GameObject nodeObject = new GameObject(NodeNames.Node.ToString() + NodeNames.ImageHolder.ToString() + "Container");
        nodeObject.transform.SetParent(imageHolder.transform);
        nodeObject.transform.position = node.transform.position;

        GameObject imageObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
        imageObject.transform.SetParent(nodeObject.transform);
        imageObject.transform.position = new Vector3(nodeObject.transform.position.x, 0.0002f, nodeObject.transform.position.z);
        imageObject.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
        imageObject.GetComponent<MeshRenderer>().material = node.nodeMaterial;
    }
    */

    void ExpandMatrix()
    {
        bool[,] newMatrix = new bool[matrix.GetLength(0) + 1, matrix.GetLength(1) + 1];

        for (int i = 0; i < matrix.GetLength(0) - 1; i++)
        {
            for (int j = 0; j < matrix.GetLength(1) - 1; j++)
            {
                newMatrix[i, j] = matrix[i, j];
            }
        }

        matrix = newMatrix;
    }

    int GetNodeIndex(PathNode node)
    {
        return new List<PathNode>(nodes).IndexOf(node);
    }

    public PathNode CheckExistingNode(Vector3 nodePosition)
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].transform.position == nodePosition) return nodes[i];
        }

        return null;
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

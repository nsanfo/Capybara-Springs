using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

[ExecuteInEditMode]
public class EntranceBuilder : MonoBehaviour
{
    [SerializeField]
    public Boolean updatePath;

    void Update()
    {
        if (Application.isPlaying) return;

        // Handle initializing needed components
        InitializePathHolder();
        InitializeEntrancePath();

        // Handle node selections
        HandleNodeSelection();

        if (updatePath)
        {
            UpdatePath();
        }
    }

    void InitializePathHolder()
    {
        GameObject pathsHolder = GameObject.Find(PathBuilder.PathNames.PathHolder.ToString());
        if (pathsHolder != null) return;

        new GameObject(PathBuilder.PathNames.PathHolder.ToString());
    }

    void InitializeEntrancePath()
    {
        GameObject entrancePath = GameObject.Find("EntrancePath");
        if (entrancePath != null) return;

        CreatePath();
    }

    void CreatePath()
    {
        PathBuilder pathBuilder = GameObject.Find("PlayerBuilding").GetComponent<PathBuilder>();
        GameObject pathsHolder = GameObject.Find(PathBuilder.PathNames.PathHolder.ToString());

        // Create new path object
        GameObject path = new GameObject("EntrancePath");

        path.transform.SetParent(pathsHolder.transform);

        // Get offset points (prevent z-axis fighting on terrain)
        Vector3 initialPoint1 = new Vector3(0, pathBuilder.meshOffset, 0);
        Vector3 initialPoint2 = new Vector3(0, pathBuilder.meshOffset, 2);

        // Add path component to handle mesh
        Path pathComponent = path.AddComponent<Path>();
        pathComponent.UpdateVariables(pathBuilder, (initialPoint1, initialPoint2, Vector3.zero));
        pathComponent.InitializeMesh(false, pathBuilder.nodeGraph);
        pathBuilder.nodeGraph = new NodeGraph();

        // Set first node as disabled
        GameObject entranceNodeHolder = GameObject.Find("EntrancePath/NodeHolder");
        if (entranceNodeHolder == null) return;

        GameObject nodeObject = entranceNodeHolder.transform.GetChild(0).Find("NodeObject").gameObject;
        for (int i = 0; i < nodeObject.transform.childCount; i++)
        {
            nodeObject.transform.GetChild(i).GetComponent<Renderer>().materials = pathBuilder.guideOffMaterials;
        }
    }

    void HandleNodeSelection()
    {
        GameObject activeObject = Selection.activeGameObject;
        if (activeObject == null) return;
        if (activeObject.transform.parent == null) return;

        GameObject activeObjectParent = activeObject.transform.parent.gameObject;
        if (activeObjectParent == null) return;
        if (activeObjectParent.transform.parent == null) return;

        while (!activeObjectParent.name.Equals("Node")) {
            activeObjectParent = activeObjectParent.transform.parent.gameObject;
            if (activeObjectParent.transform.parent == null)
                return;
        }

        Selection.activeGameObject = activeObjectParent;
    }

    void UpdatePath()
    {
        GameObject entrancePath = GameObject.Find("EntrancePath");
        Path path = entrancePath.GetComponent<Path>();
        if (path == null) return;

        GameObject entranceNodeHolder = GameObject.Find("EntrancePath/NodeHolder");
        if (entranceNodeHolder == null) return;

        Vector3 initialPoint1 = Vector3.zero;
        Vector3 initialPoint2 = Vector3.zero;
        Boolean setPoint1 = false;
        Boolean setPoint2 = false;
        for (int i = 0; i < entranceNodeHolder.transform.childCount; i++)
        {
            Vector3 position = entranceNodeHolder.transform.GetChild(i).Find("NodeObject").GetComponent<PathNode>().transform.position;
            switch (i)
            {
                case 0:
                    initialPoint1 = position;
                    setPoint1 = true;
                    break;

                case 1:
                    initialPoint2 = position;
                    setPoint2 = true;
                    break;
            }
        }

        if (!setPoint1 || !setPoint2) return;

        PathBuilder pathBuilder = GameObject.Find("PlayerBuilding").GetComponent<PathBuilder>();
        initialPoint1 = new Vector3(initialPoint1.x, pathBuilder.meshOffset, initialPoint1.z);
        initialPoint2 = new Vector3(initialPoint2.x, pathBuilder.meshOffset, initialPoint2.z);

        path.UpdateVariables(pathBuilder, (initialPoint1, initialPoint2, Vector3.zero));
        path.UpdateMesh();
    }
}

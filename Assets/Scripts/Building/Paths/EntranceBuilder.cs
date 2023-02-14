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
    public Material exitMaterial;
    public Material nodeMaterial;

    void Update()
    {
        if (Application.isPlaying)
        {
            RemoveSpheres();
            return;
        }

        // Handle initializing needed components
        InitializePathHolder();
        InitializeControllerSpheres();
        InitializeEntrancePath();

        // Handle node selections
        HandleNodeSelection();

        if (updatePath)
        {
            UpdateSpheres();
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
    
    void InitializeControllerSpheres()
    {
        GameObject entranceControllers = GameObject.Find("PathHolder/EntranceControllers");
        if (entranceControllers != null) return;

        GameObject pathHolder = GameObject.Find(PathBuilder.PathNames.PathHolder.ToString());
        entranceControllers = new GameObject("EntranceControllers");
        entranceControllers.transform.SetParent(pathHolder.transform);

        GameObject exitSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        exitSphere.name = "ExitSphere";
        exitSphere.transform.SetParent(entranceControllers.transform);
        exitSphere.transform.position = new Vector3(0, 2, 0);
        exitSphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        MeshRenderer exitRenderer = exitSphere.GetComponent<MeshRenderer>();
        exitRenderer.material = exitMaterial;
        exitRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        exitRenderer.receiveShadows = false;

        GameObject nodeSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        nodeSphere.name = "NodeSphere";
        nodeSphere.transform.SetParent(entranceControllers.transform);
        nodeSphere.transform.position = new Vector3(0, 2, 3);
        nodeSphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        MeshRenderer nodeMeshRenderer = nodeSphere.GetComponent<MeshRenderer>();
        nodeMeshRenderer.material = nodeMaterial;
        nodeMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        nodeMeshRenderer.receiveShadows = false;
    }

    void CreatePath()
    {
        PathBuilder pathBuilder = GameObject.Find("PlayerBuilding").GetComponent<PathBuilder>();
        GameObject pathsHolder = GameObject.Find(PathBuilder.PathNames.PathHolder.ToString());

        // Create new path object
        GameObject path = new GameObject("EntrancePath");
        path.transform.SetParent(pathsHolder.transform);

        // Get offset points (prevent z-axis fighting on terrain) from controller spheres
        GameObject exitSphere = GameObject.Find("PathHolder/EntranceControllers/ExitSphere");
        GameObject nodeSphere = GameObject.Find("PathHolder/EntranceControllers/NodeSphere");
        if (exitSphere == null || nodeSphere == null) return;

        Vector3 initialPoint1 = new Vector3(exitSphere.transform.position.x, pathBuilder.meshOffset, exitSphere.transform.position.z);
        Vector3 initialPoint2 = new Vector3(nodeSphere.transform.position.x, pathBuilder.meshOffset, nodeSphere.transform.position.z);

        // Add path component to handle mesh
        Path pathComponent = path.AddComponent<Path>();
        pathComponent.UpdateVariables(pathBuilder, (initialPoint1, initialPoint2, Vector3.zero));
        pathComponent.InitializeMesh(false, pathBuilder.nodeGraph);
        pathBuilder.nodeGraph = new NodeGraph();

        // Set first node as disabled
        GameObject entranceNodeHolder = GameObject.Find("PathHolder/EntrancePath/NodeHolder");
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
        GameObject entrancePath = GameObject.Find("PathHolder/EntrancePath");
        Path path = entrancePath.GetComponent<Path>();
        if (path == null) return;

        DestroyImmediate(entrancePath);
        CreatePath();
    }

    void UpdateSpheres()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null) return;

        if (selected.name.Equals("ExitSphere") || selected.name.Equals("NodeSphere"))
        {
            selected.transform.position = new Vector3(selected.transform.position.x, 2, selected.transform.position.z);
        }
    }

    void RemoveSpheres()
    {
        GameObject controllers = GameObject.Find("PathHolder/EntranceControllers");
        if (controllers == null) return;

        Destroy(controllers);
    }
}

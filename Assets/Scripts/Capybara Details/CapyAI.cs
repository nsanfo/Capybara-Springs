using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapyAI : MonoBehaviour
{
    private Animator capyAnimator;

    private Pathfinder pathfinder;

    private Path currentPath;

    private NodeGraph nodeGraph;
    private PathNode[] nodes;

    private AmenityRoute destinationRoute;
    private Stack<int> nodeRoute;

    private int nextNodeIndex;
    private PathNode nextNode;

    bool travelling;
    bool usingAmenity;
    bool waiting = false;

    // Start is called before the first frame update
    void Start()
    {
        capyAnimator = gameObject.GetComponent<Animator>();

        pathfinder = gameObject.GetComponent<Pathfinder>();

        var playerBuilding = GameObject.Find("PlayerBuilding");
        var pathBuilder = playerBuilding.GetComponent<PathBuilder>();

        nodeGraph = pathBuilder.nodeGraph;
        nodes = nodeGraph.Nodes;

        travelling = false;
        usingAmenity = false;

        var entrancePath = GameObject.Find("EntrancePath");
        currentPath = entrancePath.GetComponent<Path>();
    }

    // Update is called once per frame
    void Update()
    {
        if (waiting)
            return;
        nodes = nodeGraph.Nodes;
        if (!usingAmenity && !travelling)
        {
            travelling = true;
            destinationRoute = pathfinder.FindAmenityDestination(currentPath);
            if (this.destinationRoute == null)
            {
                travelling = false;
                StartCoroutine(Wait(2));
            }
            else
            {
                capyAnimator.SetBool("Travelling", true);
                nodeRoute = destinationRoute.NodeRoute;
                if (nodeRoute.Count == 0)
                    gameObject.transform.LookAt(this.destinationRoute.Amenity.PathCollider.gameObject.transform.position);
                else
                {
                    nextNodeIndex = nodeRoute.Peek();
                    nextNode = nodes[nextNodeIndex];
                    gameObject.transform.LookAt(nextNode.gameObject.transform.position);
                }
            }
        }
        else if (travelling)
        {
            if(nodeRoute.Count == 0)
            {
                if (Vector3.Distance(gameObject.transform.position, destinationRoute.Amenity.PathCollider.gameObject.transform.position) <= 0.1)
                {
                    travelling = false;
                    capyAnimator.SetBool("Travelling", false);
                    StartCoroutine(Wait(10));
                    usingAmenity = true;
                    AmenityInteraction interaction = gameObject.AddComponent<AmenityInteraction>();
                    interaction.HandleInteraction(destinationRoute.Amenity);
                }
            }
            else
            {
                if (Vector3.Distance(gameObject.transform.position, nextNode.gameObject.transform.position) <= 0.1)
                {
                    var previousNodeIndex = nodeRoute.Pop();
                    if (nodeRoute.Count > 0)
                        currentPath = nodeGraph.GetPath(previousNodeIndex, nodeRoute.Peek());
                    else
                        currentPath = destinationRoute.Path;
                    if (nodeRoute.Count == 0)
                        gameObject.transform.LookAt(destinationRoute.Amenity.PathCollider.gameObject.transform.position);
                    else
                    {
                        nextNodeIndex = nodeRoute.Peek();
                        nextNode = nodes[nextNodeIndex];
                        gameObject.transform.LookAt(nextNode.gameObject.transform.position);
                    }
                }
            }
        }
    }

    private IEnumerator Wait(float seconds)
    {
        waiting = true;
        yield return new WaitForSeconds(seconds);
        waiting = false;
    }

    public void CompletedAmenityInteraction()
    {
        AmenityInteraction interaction = gameObject.GetComponent<AmenityInteraction>();
        if (interaction != null)
            Destroy(interaction);

        usingAmenity = false;
    }
}

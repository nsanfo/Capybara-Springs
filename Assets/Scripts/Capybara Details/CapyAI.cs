using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapyAI : MonoBehaviour
{
    private Pathfinder pathfinder;

    private NodeGraph nodeGraph;
    private PathNode[] nodes;

    private AmenityRoute destinationRoute;
    private Stack<int> nodeRoute;

    private int nextNodeIndex;
    private PathNode nextNode;

    bool travelling;
    bool usingAmenity;

    // Start is called before the first frame update
    void Start()
    {
        pathfinder = gameObject.GetComponent<Pathfinder>();

        var playerBuilding = GameObject.Find("PlayerBuilding");
        var pathBuilder = playerBuilding.GetComponent<PathBuilder>();

        nodeGraph = pathBuilder.nodeGraph;
        nodes = nodeGraph.Nodes;

        travelling = false;
        usingAmenity = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!usingAmenity && !travelling)
        {
            travelling = true;
            destinationRoute = pathfinder.FindAmenityDestination();
            nodeRoute = destinationRoute.NodeRoute;
            if (this.destinationRoute == null)
            {
                travelling = false;
                StartCoroutine(Wait(2));
            }
            else if(nodeRoute.Count == 0)
                gameObject.transform.LookAt(this.destinationRoute.Amenity.PathCollider.gameObject.transform.position);
            else
            {
                nextNodeIndex = nodeRoute.Peek();
                nextNode = nodes[nextNodeIndex];
                gameObject.transform.LookAt(nextNode.gameObject.transform.position);
            }
        }
        else if (travelling)
        {
            if(nodeRoute.Count == 0)
            {
                if (Vector3.Distance(gameObject.transform.position, destinationRoute.Amenity.gameObject.transform.position) > 0.1)
                    gameObject.transform.Translate(Vector3.forward * Time.deltaTime);
                else
                {
                    travelling = false;
                    StartCoroutine(Wait(10));
                }
            }
            else
            {
                if (Vector3.Distance(gameObject.transform.position, nextNode.gameObject.transform.position) > 0.1)
                    gameObject.transform.Translate(Vector3.forward * Time.deltaTime);
                else
                {
                    nodeRoute.Pop();
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
        yield return new WaitForSeconds(seconds);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapyAI : MonoBehaviour
{
    enum States { travelling, usingAmenity, collisionAvoidance, walkTurning, idleTurning, waiting, ready }
    States state = States.ready;

    private Animator capyAnimator;

    private Pathfinder pathfinder;

    private Path currentPath;
    public Vector3 PathPosition { get; set; } // A vector representing the capybara's distance from the center axis of the path

    private int collisions;
    public int Collisions { get => collisions; }

    private NodeGraph nodeGraph;
    private PathNode[] nodes;

    private AmenityRoute destinationRoute;
    private Stack<int> nodeRoute;

    private int nextNodeIndex;
    private PathNode nextNode;
    private int previousNodeIndex;
    private PathNode previousNode;

    float startingDirection, endDirection;
    //float turnAmount;

    // Start is called before the first frame update
    void Start()
    {
        capyAnimator = gameObject.GetComponent<Animator>();

        pathfinder = gameObject.GetComponent<Pathfinder>();

        var playerBuilding = GameObject.Find("PlayerBuilding");
        var pathBuilder = playerBuilding.GetComponent<PathBuilder>();

        nodeGraph = pathBuilder.nodeGraph;
        nodes = nodeGraph.Nodes;

        var entrancePath = GameObject.Find("EntrancePath");
        currentPath = entrancePath.GetComponent<Path>();
        previousNode = entrancePath.transform.GetChild(1).transform.GetChild(0).transform.GetChild(0).GetComponent<PathNode>();
    }

    // Update is called once per frame
    void Update()
    {
        nodes = nodeGraph.Nodes;
        switch (state)
        {
            case States.waiting:
                return;
            case States.ready:
                state = States.travelling;
                destinationRoute = pathfinder.FindAmenityDestination(currentPath);
                if (destinationRoute == null)
                {
                    state = States.waiting;
                    StartCoroutine(Wait(2));
                }
                else
                {
                    nodeRoute = destinationRoute.NodeRoute;
                    if (nodeRoute.Count == 0)
                    {
                        endDirection = Quaternion.LookRotation(destinationRoute.Amenity.PathCollider.gameObject.transform.position - previousNode.gameObject.transform.position).eulerAngles.y;
                        startingDirection = gameObject.transform.eulerAngles.y;
                        CalculateTurn(startingDirection, endDirection);
                        //gameObject.transform.LookAt(destinationRoute.Amenity.PathCollider.gameObject.transform.position);
                    }
                    else
                    {
                        nextNodeIndex = nodeRoute.Peek();
                        nextNode = nodes[nextNodeIndex];

                        endDirection = Quaternion.LookRotation(nextNode.gameObject.transform.position - previousNode.gameObject.transform.position).eulerAngles.y;
                        startingDirection = gameObject.transform.eulerAngles.y;
                        CalculateTurn(startingDirection, endDirection);
                        //gameObject.transform.LookAt(nextNode.gameObject.transform.position);
                    }
                    capyAnimator.SetBool("Travelling", true);
                }
                break;
            case States.travelling:
                {
                    if (nodeRoute.Count == 0)
                    {
                        if (Vector3.Distance(gameObject.transform.position, (destinationRoute.Amenity.PathCollider.gameObject.transform.position + PathPosition)) <= 0.1)
                        {
                            state = States.waiting;
                            capyAnimator.SetBool("Travelling", false);
                            StartCoroutine(Wait(10));
                        }
                    }
                    else
                    {
                        if (Vector3.Distance(gameObject.transform.position, nextNode.gameObject.transform.position) <= 0.5)
                        {
                            previousNodeIndex = nodeRoute.Pop();
                            previousNode = nextNode;
                            if (nodeRoute.Count > 0)
                                currentPath = nodeGraph.GetPath(previousNodeIndex, nodeRoute.Peek());
                            else
                                currentPath = destinationRoute.Path;
                            if (nodeRoute.Count == 0)
                            {
                                endDirection = Quaternion.LookRotation(destinationRoute.Amenity.PathCollider.gameObject.transform.position - previousNode.gameObject.transform.position).eulerAngles.y;
                                startingDirection = gameObject.transform.eulerAngles.y;
                                CalculateTurn(startingDirection, endDirection);
                                state = States.walkTurning;
                            }
                            else
                            {
                                nextNodeIndex = nodeRoute.Peek();
                                nextNode = nodes[nextNodeIndex];

                                endDirection = Quaternion.LookRotation(nextNode.gameObject.transform.position - previousNode.gameObject.transform.position).eulerAngles.y;
                                startingDirection = gameObject.transform.eulerAngles.y;
                                CalculateTurn(startingDirection, endDirection);
                                state = States.walkTurning;
                            }
                        }
                    }
                    break;
                }
            case States.walkTurning:
                {
                    if (Mathf.Abs(endDirection - gameObject.transform.eulerAngles.y) <= 1f)
                    {
                        state = States.travelling;
                        capyAnimator.SetBool("Turning", false);
                        PathPosition = Intersection.CalculatePathPosition(new Vector3(gameObject.transform.position.x, 0, gameObject.transform.position.z), gameObject.transform.forward, new Vector3(currentPath.spacedPoints[0].x, 0, currentPath.spacedPoints[0].z), currentPath.spacedPoints[0] - currentPath.spacedPoints[1]);
                        StartCoroutine(TurnWait(0.25f));
                    }
                }
                break;
            case States.idleTurning:
                {
                    if (Mathf.Abs(endDirection - gameObject.transform.eulerAngles.y) <= 1f)
                    {
                        state = States.travelling;
                        capyAnimator.SetBool("Turning", false);
                        capyAnimator.SetBool("Travelling", true);
                        PathPosition = Intersection.CalculatePathPosition(new Vector3(gameObject.transform.position.x, 0, gameObject.transform.position.z), gameObject.transform.forward, new Vector3(currentPath.spacedPoints[0].x, 0, currentPath.spacedPoints[0].z), currentPath.spacedPoints[0] - currentPath.spacedPoints[1]);
                        StartCoroutine(TurnWait(0.25f));
                    }
                }
                break;
        }
    }

    private IEnumerator Wait(float seconds)
    {
        state = States.waiting;
        yield return new WaitForSeconds(seconds);
        state = States.ready;
    }

    // Waits to align the capybara to its destination after the turn animation exit blending has completed
    private IEnumerator TurnWait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        gameObject.transform.eulerAngles = new Vector3(gameObject.transform.rotation.x, endDirection, gameObject.transform.rotation.z);
    }

    private void CalculateTurn(float startingDirection, float endDirection)
    {
        var difference = startingDirection - endDirection;

        if (difference > 180)
            difference = -(360 - startingDirection + endDirection);
        else if (difference < -180)
            difference = (360 - endDirection + startingDirection);

        capyAnimator.SetBool("Turning", true);

        if (difference >= 0)
        {
            capyAnimator.SetFloat("Turn", (-difference) * (1f / 90f));
            Debug.Log("Turn amount: " + capyAnimator.GetFloat("Turn"));
        }
        else
        {
            capyAnimator.SetFloat("Turn", (-difference) * (1f / 90f));
            Debug.Log("Turn amount: " + capyAnimator.GetFloat("Turn"));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Capybara")
            collisions++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Capybara")
            collisions--;
    }
}

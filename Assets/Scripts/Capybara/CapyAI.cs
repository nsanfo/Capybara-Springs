using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CapyAI : MonoBehaviour
{
    enum State { travelling, usingAmenity, opposingCollision, walkingCollision, turnCollision, walkTurning, idleTurning, waiting, ready }
    private State state = State.ready;

    enum PathDirection { direction1, direction2, noDirection } // Used to determine if two capybaras are walking opposite directions on the same path
    private PathDirection pathDirection;

    private Animator capyAnimator;

    private Pathfinder pathfinder;

    private Path currentPath;
    public Vector3 PathPosition { get; set; } // A vector representing the capybara's distance from the center axis of the path

    private int bodyCollisions;
    public int BodyCollisions { get => bodyCollisions; }
    private int frontCollisions;
    public int FrontCollisions { get => frontCollisions; }


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
            case State.waiting:
                return;
            case State.ready:
                state = State.travelling;
                destinationRoute = pathfinder.FindAmenityDestination(currentPath);
                if (destinationRoute == null)
                {
                    state = State.waiting;
                    StartCoroutine(Wait(2));
                }
                else
                {
                    nodeRoute = destinationRoute.NodeRoute;
                    if (nodeRoute.Count == 0)
                    {
                        if (Vector3.Distance(gameObject.transform.position, (destinationRoute.Amenity.PathCollider.gameObject.transform.position)) <= 0.5)
                        {
                            state = State.usingAmenity;
                            GetComponent<AmenityInteraction>().HandleInteraction(destinationRoute.Amenity);
                            break;
                        }
                        endDirection = Quaternion.LookRotation((destinationRoute.Amenity.PathCollider.gameObject.transform.position + PathPosition) - (previousNode.gameObject.transform.position + PathPosition)).eulerAngles.y;
                        startingDirection = gameObject.transform.eulerAngles.y;
                        CalculateTurn(startingDirection, endDirection);
                    }
                    else
                    {
                        nextNodeIndex = nodeRoute.Peek();
                        nextNode = nodes[nextNodeIndex];

                        endDirection = Quaternion.LookRotation((nextNode.gameObject.transform.position + PathPosition) - (previousNode.gameObject.transform.position + PathPosition)).eulerAngles.y;
                        startingDirection = gameObject.transform.eulerAngles.y;
                        CalculateTurn(startingDirection, endDirection);
                    }
                    state = State.idleTurning;
                }
                break;
            case State.travelling:
                {
                    if(frontCollisions > 0)
                    {
                        state = State.walkingCollision;
                        capyAnimator.SetBool("Travelling", false);
                        break;
                    }
                    if (nodeRoute.Count == 0)
                    {
                        if (destinationRoute.Amenity.amenitySlots.Count(capy => capy != null) == destinationRoute.Amenity.amenitySlots.Length)
                        {
                            state = State.ready;
                            capyAnimator.SetBool("Travelling", false);
                            break;
                        }
                        if (Vector3.Distance(gameObject.transform.position, (destinationRoute.Amenity.PathCollider.gameObject.transform.position + PathPosition)) <= 0.1)
                        {
                            capyAnimator.SetBool("Travelling", false);
                            state = State.usingAmenity;
                            GetComponent<AmenityInteraction>().HandleInteraction(destinationRoute.Amenity);
                        }
                    }
                    else
                    {
                        if (Vector3.Distance(gameObject.transform.position, nextNode.gameObject.transform.position + PathPosition) <= 0.5)
                        {
                            previousNodeIndex = nodeRoute.Pop();
                            previousNode = nextNode;
                            if (nodeRoute.Count > 0)
                                currentPath = nodeGraph.GetPath(previousNodeIndex, nodeRoute.Peek());
                            else
                                currentPath = destinationRoute.Path;
                            if (nodeRoute.Count == 0)
                            {
                                endDirection = Quaternion.LookRotation((destinationRoute.Amenity.PathCollider.gameObject.transform.position + PathPosition) - (previousNode.gameObject.transform.position + PathPosition)).eulerAngles.y;
                                startingDirection = gameObject.transform.eulerAngles.y;
                                CalculateTurn(startingDirection, endDirection);
                                state = State.walkTurning;
                            }
                            else
                            {
                                nextNodeIndex = nodeRoute.Peek();
                                nextNode = nodes[nextNodeIndex];

                                endDirection = Quaternion.LookRotation((nextNode.gameObject.transform.position + PathPosition) - (previousNode.gameObject.transform.position + PathPosition)).eulerAngles.y;
                                startingDirection = gameObject.transform.eulerAngles.y;
                                CalculateTurn(startingDirection, endDirection);
                                state = State.walkTurning;
                            }
                        }
                    }
                    break;
                }
            case State.walkTurning:
                {
                    if (Mathf.Abs(endDirection - gameObject.transform.eulerAngles.y) <= 1f)
                    {
                        state = State.travelling;
                        capyAnimator.SetBool("Turning", false);
                        StartCoroutine(TurnWait(0.25f));
                    }
                }
                break;
            case State.idleTurning:
                {
                    if (Mathf.Abs(endDirection - gameObject.transform.eulerAngles.y) <= 1f)
                    {
                        state = State.travelling;
                        capyAnimator.SetBool("Turning", false);
                        capyAnimator.SetBool("Travelling", true);
                        StartCoroutine(IdleTurnWait(0.25f));
                    }
                }
                break;
            case State.walkingCollision:
                {
                    if(frontCollisions == 0)
                    {
                        state = State.travelling;
                        capyAnimator.SetBool("Travelling", true);
                    }
                }
                break;
        }
    }

    private IEnumerator Wait(float seconds)
    {
        state = State.waiting;
        yield return new WaitForSeconds(seconds);
        state = State.ready;
    }

    // Waits to align the capybara to its destination after the turn animation exit blending has completed
    private IEnumerator TurnWait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        gameObject.transform.eulerAngles = new Vector3(gameObject.transform.rotation.x, endDirection, gameObject.transform.rotation.z);
        PathPosition = Intersection.CalculatePathPosition(new Vector3(gameObject.transform.position.x, 0, gameObject.transform.position.z), gameObject.transform.right, new Vector3(currentPath.spacedPoints[0].x, 0, currentPath.spacedPoints[0].z), currentPath.spacedPoints[1] - currentPath.spacedPoints[0]);
    }

    private IEnumerator IdleTurnWait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        gameObject.transform.eulerAngles = new Vector3(gameObject.transform.rotation.x, endDirection, gameObject.transform.rotation.z);
        PathPosition = Intersection.CalculatePathPosition(new Vector3(gameObject.transform.position.x, 0, gameObject.transform.position.z), gameObject.transform.right, new Vector3(currentPath.spacedPoints[0].x, 0, currentPath.spacedPoints[0].z), currentPath.spacedPoints[1] - currentPath.spacedPoints[0]);
        state = State.travelling;
        capyAnimator.SetBool("Travelling", true);
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
        }
        else
        {
            capyAnimator.SetFloat("Turn", (-difference) * (1f / 90f));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Capybara")
            bodyCollisions++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Capybara")
            bodyCollisions--;
    }

    public void CompletedAmenityInteraction()
    {
        state = State.ready;
        destinationRoute.Amenity.RemoveCapybara(gameObject);
    }

    public void FrontCollisionEnter()
    {
        if (state == State.travelling)
            frontCollisions++;
    }

    public void FrontCollisionExit()
    {
        if (state == State.travelling)
            frontCollisions--;
    }
}

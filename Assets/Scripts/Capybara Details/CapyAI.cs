using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapyAI : MonoBehaviour
{
    enum States { travelling, usingAmenity, collisionAvoidance, turning, waiting, ready }
    States state = States.ready;

    private Animator capyAnimator;

    private Pathfinder pathfinder;

    private Path currentPath;
    public float PathPosition { get; set; }

    private int collisions;
    public int Collisions { get => collisions; }

    private NodeGraph nodeGraph;
    private PathNode[] nodes;

    private AmenityRoute destinationRoute;
    private Stack<int> nodeRoute;

    private int nextNodeIndex;
    private PathNode nextNode;

    float startingDirection, endDirection;
    float turnAmount;

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
                    capyAnimator.SetBool("Travelling", true);
                    nodeRoute = destinationRoute.NodeRoute;
                    if (nodeRoute.Count == 0)
                        gameObject.transform.LookAt(destinationRoute.Amenity.PathCollider.gameObject.transform.position);
                    else
                    {
                        nextNodeIndex = nodeRoute.Peek();
                        nextNode = nodes[nextNodeIndex];
                        gameObject.transform.LookAt(nextNode.gameObject.transform.position);
                    }
                }
                break;
            case States.travelling:
                {
                    if (nodeRoute.Count == 0)
                    {
                        if (Vector3.Distance(gameObject.transform.position, destinationRoute.Amenity.PathCollider.gameObject.transform.position) <= 0.1)
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
                            var previousNodeIndex = nodeRoute.Pop();
                            var previousNode = nextNode;
                            if (nodeRoute.Count > 0)
                                currentPath = nodeGraph.GetPath(previousNodeIndex, nodeRoute.Peek());
                            else
                                currentPath = destinationRoute.Path;
                            if (nodeRoute.Count == 0)
                            {
                                endDirection = Quaternion.LookRotation(destinationRoute.Amenity.PathCollider.gameObject.transform.position - previousNode.gameObject.transform.position).eulerAngles.y;
                                startingDirection = gameObject.transform.eulerAngles.y;
                                CalculateTurn(startingDirection, endDirection);
                            }
                            else
                            {
                                nextNodeIndex = nodeRoute.Peek();
                                nextNode = nodes[nextNodeIndex];

                                endDirection = Quaternion.LookRotation(nextNode.gameObject.transform.position - previousNode.gameObject.transform.position).eulerAngles.y;
                                startingDirection = gameObject.transform.eulerAngles.y;
                                CalculateTurn(startingDirection, endDirection);
                            }
                        }
                    }
                    break;
                }
            case States.turning:
                {
                    gameObject.transform.Rotate(Vector3.up, turnAmount);
                    gameObject.transform.Translate(Vector3.forward * 0.4f * Time.deltaTime);
                    if (Mathf.Abs(endDirection - gameObject.transform.eulerAngles.y) <= 0.3f)
                    {
                        state = States.travelling;
                        gameObject.transform.eulerAngles = new Vector3(gameObject.transform.rotation.x, endDirection, gameObject.transform.rotation.z);
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

    private void CalculateTurn(float startingDirection, float endDirection)
    {
        var difference = startingDirection - endDirection;

        if (difference > 180)
            difference = -(360 - startingDirection + endDirection);
        else if (difference < -180)
            difference = (360 - endDirection + startingDirection);

        if (difference >= 0)
            turnAmount = Mathf.Min(-0.013f * Mathf.Pow(difference, 2), -difference, -1) * 0.45f * Time.deltaTime;
        else
            turnAmount = Mathf.Max(0.013f * Mathf.Pow(difference, 2), -difference, 1) * 0.45f * Time.deltaTime;
        state = States.turning;
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

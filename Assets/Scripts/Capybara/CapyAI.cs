using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CapyAI : MonoBehaviour
{
    enum State { travelling, usingAmenity, walkingCollision, turnCollision, walkTurning, idleTurning, waiting, ready, leaving }
    private State state = State.ready;

    enum StrafeDirection { strafeLeft, strafeRight }
    private StrafeDirection strafeDirection;

    private Animator capyAnimator;

    private Pathfinder pathfinder;

    private Path currentPath;
    public Vector3 PathPosition { get; set; } // A vector representing the capybara's distance from the center axis of the path

    private int bodyCollisions;
    private int initialBodyCollisions;
    private int frontCollisions = 0;
    private int rightCollisions = 0;
    private int initialRightCollisions;
    private int leftCollisions = 0;
    private int initialLeftCollisions;

    private BoxCollider bodyCollider;
    private Vector3 bodyColliderSize;
    private Vector3 bodyColliderCenter;
    private BoxCollider frontCollider;
    private Vector3 frontColliderSize;
    private Vector3 frontColliderCenter;
    private int opposingCollisions = 0;
    private bool noclip = false;
    private float stuckTime;

    private NodeGraph nodeGraph;
    private PathNode[] nodes;

    private AmenityRoute destinationRoute;
    private Stack<int> nodeRoute;

    private int nextNodeIndex;
    private GameObject nextNode;
    private int previousNodeIndex;
    public GameObject previousNode;

    private bool leaving = false;
    private int amenitiesEntered = 0;

    private CapybaraInfo capybaraInfo;

    float startingDirection, endDirection;

    const float pathWidth = 0.70f;

    // Start is called before the first frame update
    void Start()
    {
        capyAnimator = gameObject.GetComponent<Animator>();

        pathfinder = gameObject.GetComponent<Pathfinder>();

        var playerBuilding = GameObject.Find("PlayerBuilding");
        var pathBuilder = playerBuilding.GetComponent<PathBuilder>();

        nodeGraph = pathBuilder.nodeGraph;
        nodes = nodeGraph.Nodes;

        capybaraInfo = gameObject.GetComponent<CapybaraInfo>();

        var entrancePath = GameObject.Find("EntrancePath");
        currentPath = entrancePath.GetComponent<Path>();
        previousNode = entrancePath.transform.GetChild(1).transform.GetChild(0).transform.GetChild(0).gameObject;

        bodyCollider = transform.GetChild(6).GetComponent<BoxCollider>();
        bodyColliderSize = bodyCollider.size;
        bodyColliderCenter = bodyCollider.center;
        frontCollider = transform.GetChild(3).GetComponent<BoxCollider>();
        frontColliderSize = frontCollider.size;
        frontColliderCenter = frontCollider.center;
    }

    // Update is called once per frame
    void Update()
    {
        if (capybaraInfo != null)
        {
            capybaraInfo.TimeSpent = capybaraInfo.TimeSpent + Time.deltaTime;
        }

        nodes = nodeGraph.Nodes;
        switch (state)
        {
            case State.waiting:
                return;
            case State.ready:
                PathPosition = Intersection.CalculatePathPosition(new Vector3(gameObject.transform.position.x, 0, gameObject.transform.position.z), gameObject.transform.right, new Vector3(currentPath.spacedPoints[0].x, 0, currentPath.spacedPoints[0].z), currentPath.spacedPoints[1] - currentPath.spacedPoints[0]);
                state = State.travelling;

                if (pathfinder.LeaveRoute(capybaraInfo.TimeSpent, capybaraInfo.Frustration) && amenitiesEntered > 0)
                {
                    nodeRoute = pathfinder.FindExitDestination(currentPath);
                    leaving = true;
                }

                if (leaving)
                {
                    if (nodeRoute.Count == 0)
                    {
                        state = State.leaving;
                        capyAnimator.SetBool("Travelling", false);
                        break;
                    }
                    else
                    {
                        nextNodeIndex = nodeRoute.Peek();
                        nextNode = nodes[nextNodeIndex].gameObject;

                        endDirection = Quaternion.LookRotation((nextNode.transform.position + PathPosition) - (gameObject.transform.position)).eulerAngles.y;
                        startingDirection = gameObject.transform.eulerAngles.y;
                        CalculateTurn(startingDirection, endDirection);
                    }
                    state = State.idleTurning;
                    frontCollider.size.Set(bodyColliderSize.x + 0.01f, bodyColliderSize.y + 0.01f, bodyColliderSize.z + 0.01f);
                    frontCollider.center.Set(bodyColliderCenter.x, bodyColliderCenter.y, bodyColliderCenter.z);
                    if (capybaraInfo != null)
                    {
                        capybaraInfo.Frustration = 0;
                    }

                    break;
                }

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
                            amenitiesEntered++;
                            GetComponent<AmenityInteraction>().HandleInteraction(destinationRoute.Amenity);
                            break;
                        }
                        endDirection = Quaternion.LookRotation((destinationRoute.Amenity.PathCollider.gameObject.transform.position + PathPosition) - (gameObject.transform.position)).eulerAngles.y;
                        startingDirection = gameObject.transform.eulerAngles.y;
                        CalculateTurn(startingDirection, endDirection);
                    }
                    else
                    {
                        nextNodeIndex = nodeRoute.Peek();
                        nextNode = nodes[nextNodeIndex].gameObject;

                        endDirection = Quaternion.LookRotation((nextNode.transform.position + PathPosition) - (previousNode.transform.position + PathPosition)).eulerAngles.y;
                        startingDirection = gameObject.transform.eulerAngles.y;
                        CalculateTurn(startingDirection, endDirection);
                    }
                    frontCollider.size.Set(bodyColliderSize.x + 0.01f, bodyColliderSize.y + 0.01f, bodyColliderSize.z + 0.01f);
                    frontCollider.center.Set(bodyColliderCenter.x, bodyColliderCenter.y, bodyColliderCenter.z);
                    state = State.idleTurning;
                    if (capybaraInfo != null)
                    {
                        capybaraInfo.Frustration = 0;
                    }
                }

                if (amenitiesEntered > 0)
                {
                    if (capybaraInfo != null)
                    {
                        capybaraInfo.Frustration = capybaraInfo.Frustration + (capybaraInfo.Frustration * Time.deltaTime * 100);
                    }
                }

                break;
            case State.travelling:
                {
                    if(destinationRoute != null && destinationRoute.Amenity == null)
                    {
                        state = State.ready;
                        capyAnimator.SetBool("Travelling", false);
                        break;
                    }
                    if(frontCollisions > 0 && !noclip)
                    {
                        if (opposingCollisions > 0)
                        {
                            capyAnimator.SetBool("StrafeRight", true);
                            strafeDirection = StrafeDirection.strafeRight;
                            initialRightCollisions = rightCollisions;
                        }
                        else
                        {
                            strafeDirection = ResolveStrafeDirection();
                            if (strafeDirection == StrafeDirection.strafeRight)
                            {
                                initialRightCollisions = rightCollisions;
                                capyAnimator.SetBool("StrafeRight", true);
                            }
                            else
                            {
                                initialLeftCollisions = leftCollisions;
                                capyAnimator.SetBool("StrafeLeft", true);
                            }
                        }
                        state = State.walkingCollision;
                        capyAnimator.SetBool("Travelling", false);
                        break;
                    }
                    if (nodeRoute.Count == 0)
                    {
                        if (leaving)
                        {
                            endDirection = Quaternion.LookRotation((nodeGraph.Nodes[0].gameObject.transform.position + PathPosition) - (previousNode.transform.position + PathPosition)).eulerAngles.y;
                            startingDirection = gameObject.transform.eulerAngles.y;
                            CalculateTurn(startingDirection, endDirection);
                            state = State.walkTurning;
                            break;
                        }
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
                            amenitiesEntered++;
                            GetComponent<AmenityInteraction>().HandleInteraction(destinationRoute.Amenity);
                        }
                    }
                    else
                    {
                        if (Vector3.Distance(gameObject.transform.position, nextNode.transform.position + PathPosition) <= 0.5)
                        {
                            previousNodeIndex = nodeRoute.Pop();
                            previousNode = nextNode;
                            if (nodeRoute.Count > 0)
                                currentPath = nodeGraph.GetPath(previousNodeIndex, nodeRoute.Peek());
                            else if (destinationRoute != null)
                                currentPath = destinationRoute.Path;

                            if (nodeRoute.Count == 0)
                            {
                                if (leaving)
                                {
                                    state = State.leaving;
                                    capyAnimator.SetBool("Travelling", false);
                                    break;
                                }
                                endDirection = Quaternion.LookRotation((destinationRoute.Amenity.PathCollider.gameObject.transform.position + PathPosition) - (previousNode.transform.position + PathPosition)).eulerAngles.y;
                                startingDirection = gameObject.transform.eulerAngles.y;
                                CalculateTurn(startingDirection, endDirection);
                                state = State.walkTurning;
                            }
                            else
                            {
                                nextNodeIndex = nodeRoute.Peek();
                                nextNode = nodes[nextNodeIndex].gameObject;

                                endDirection = Quaternion.LookRotation((nextNode.transform.position + PathPosition) - (previousNode.transform.position + PathPosition)).eulerAngles.y;
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
                    if(bodyCollisions > 0 && !noclip)
                    {
                        initialBodyCollisions = bodyCollisions;
                        state = State.turnCollision;
                        if (strafeDirection == StrafeDirection.strafeRight)
                            capyAnimator.SetBool("StrafeRight", true);
                        else
                            capyAnimator.SetBool("StrafeLeft", true);
                        break;
                    }
                    if (Mathf.Abs(endDirection - gameObject.transform.eulerAngles.y) <= 5f)
                    {
                        state = State.travelling;
                        capyAnimator.SetBool("Turning", false);
                        StartCoroutine(TurnWait(0.25f));
                    }
                }
                break;
            case State.idleTurning:
                {
                    if (Mathf.Abs(endDirection - gameObject.transform.eulerAngles.y) <= 5f)
                    {
                        frontCollider.size.Set(frontColliderSize.x, frontColliderSize.y, frontColliderSize.z);
                        frontCollider.center.Set(frontColliderCenter.x, frontColliderCenter.y, frontColliderCenter.z);
                        state = State.travelling;
                        capyAnimator.SetBool("Turning", false);
                        capyAnimator.SetBool("Travelling", true);
                        StartCoroutine(IdleTurnWait(0.25f));
                    }
                }
                break;
            case State.walkingCollision:
                {
                    PathPosition = Intersection.CalculatePathPosition(new Vector3(gameObject.transform.position.x, 0, gameObject.transform.position.z), gameObject.transform.right, new Vector3(currentPath.spacedPoints[0].x, 0, currentPath.spacedPoints[0].z), currentPath.spacedPoints[1] - currentPath.spacedPoints[0]);
                    if (frontCollisions == 0)
                    {
                        state = State.travelling;
                        if (strafeDirection == StrafeDirection.strafeRight)
                            capyAnimator.SetBool("StrafeRight", false);
                        else
                            capyAnimator.SetBool("StrafeLeft", false);
                        capyAnimator.SetBool("Travelling", true);
                    }
                    else if (opposingCollisions > 0 && strafeDirection == StrafeDirection.strafeLeft)
                    {
                        strafeDirection = StrafeDirection.strafeRight;
                        capyAnimator.SetBool("StrafeLeft", false);
                        capyAnimator.SetBool("StrafeRight", true);
                    }
                    else if(strafeDirection == StrafeDirection.strafeRight && capyAnimator.GetBool("StrafeRight") && rightCollisions > initialRightCollisions)
                    {
                        capyAnimator.SetBool("StrafeRight", false);
                        stuckTime = 0;
                    }
                    else if(strafeDirection == StrafeDirection.strafeLeft && capyAnimator.GetBool("StrafeLeft") && leftCollisions > initialLeftCollisions)
                    {
                        capyAnimator.SetBool("StrafeLeft", false);
                        stuckTime = 0;
                    }
                    else if ((PathPosition.magnitude >= (pathWidth / 2)) && (capyAnimator.GetBool("StrafeRight") || capyAnimator.GetBool("StrafeLeft")))
                    {
                        if (strafeDirection == StrafeDirection.strafeRight)
                            capyAnimator.SetBool("StrafeRight", false);
                        else
                            capyAnimator.SetBool("StrafeLeft", false);
                        stuckTime = 0;
                    }
                    else if (!capyAnimator.GetBool("StrafeRight") && !capyAnimator.GetBool("StrafeLeft"))
                    {
                        if (stuckTime >= 5)
                        {
                            noclip = true;
                            StartCoroutine(NoclipDuration(5));
                            state = State.travelling;
                            capyAnimator.SetBool("Travelling", true);
                        }
                        stuckTime += Time.deltaTime;
                    }
                    else
                    {
                        if (strafeDirection == StrafeDirection.strafeRight)
                            capyAnimator.SetBool("StrafeRight", true);
                        else
                            capyAnimator.SetBool("StrafeLeft", true);
                    }
                }
                break;
            case State.turnCollision:
                PathPosition = Intersection.CalculatePathPosition(new Vector3(gameObject.transform.position.x, 0, gameObject.transform.position.z), gameObject.transform.right, new Vector3(currentPath.spacedPoints[0].x, 0, currentPath.spacedPoints[0].z), currentPath.spacedPoints[1] - currentPath.spacedPoints[0]);
                if (bodyCollisions == 0)
                {
                    startingDirection = gameObject.transform.eulerAngles.y;
                    CalculateTurn(startingDirection, endDirection);
                    state = State.walkTurning;
                    if (strafeDirection == StrafeDirection.strafeRight)
                        capyAnimator.SetBool("StrafeRight", false);
                    else
                        capyAnimator.SetBool("StrafeLeft", false);
                    capyAnimator.SetBool("Turning", true);
                }
                else if ((bodyCollisions > initialBodyCollisions || PathPosition.magnitude >= (pathWidth / 2)) && (capyAnimator.GetBool("StrafeRight") || capyAnimator.GetBool("StrafeLeft")))
                {
                    if (strafeDirection == StrafeDirection.strafeRight)
                        capyAnimator.SetBool("StrafeRight", false);
                    else
                        capyAnimator.SetBool("StrafeLeft", false);
                    stuckTime = 0;
                }
                else if ((bodyCollisions > initialBodyCollisions || PathPosition.magnitude >= (pathWidth / 2)) && !(capyAnimator.GetBool("StrafeRight") || capyAnimator.GetBool("StrafeLeft")))
                {
                    if (stuckTime >= 10)
                    {
                        noclip = true;
                        StartCoroutine(NoclipDuration(5));
                        startingDirection = gameObject.transform.eulerAngles.y;
                        CalculateTurn(startingDirection, endDirection);
                        state = State.walkTurning;
                        capyAnimator.SetBool("Turning", true);
                    }
                    stuckTime += Time.deltaTime;
                }
                break;
            case State.leaving:
                Leave();
                state = State.waiting;
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
        state = State.travelling;
        capyAnimator.SetBool("Travelling", true);
    }

    // Noclip (collision ignore) shuts off after specified amount of time)
    private IEnumerator NoclipDuration(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        while (true)
            if (bodyCollisions == 0)
            {
                noclip = false;
                break;
            }
            else
                yield return new WaitForSeconds(3);
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

    public void CompletedAmenityInteraction()
    {
        state = State.ready;
        destinationRoute.Amenity.RemoveCapybara(gameObject);
    }

    public void BodyCollisionEnter(Collider other)
    {
        bodyCollisions++;
        if (state == State.walkTurning)
        {
            if (Vector3.Distance(transform.right, other.transform.position) <= Vector3.Distance(-transform.right, other.transform.position))
                strafeDirection = StrafeDirection.strafeLeft;
            else
                strafeDirection = StrafeDirection.strafeRight;
        }
    }

    public void BodyCollisionExit(Collider other)
    {
        bodyCollisions--;
    }

    public void FrontCollisionEnter(Collider other)
    {
        frontCollisions++;
        if (other.gameObject.tag == "Front")
            opposingCollisions++;
    }

    public void FrontCollisionExit(Collider other)
    {
        frontCollisions--;
        if (other.gameObject.tag == "Front")
            opposingCollisions--;
    }

    public void RightCollisionEnter(Collider other)
    {
        rightCollisions++;
    }

    public void RightCollisionExit(Collider other)
    {
        rightCollisions--;
    }

    public void LeftCollisionEnter(Collider other)
    {
        leftCollisions++;
    }

    public void LeftCollisionExit(Collider other)
    {
        leftCollisions--;
    }

    private void Leave()
    {
        // Handle Money
        CapybaraInfo capyInfo = gameObject.GetComponent<CapybaraInfo>();
        if (capyInfo == null) return;
        double payment = capyInfo.happiness * 25;

        Balance balance = GameObject.Find("Stats").GetComponent<Balance>();
        balance.AdjustBalance(payment);

        // Handle capybara number
        CapybaraHandler capybaraHandler = GameObject.Find("SpawnManager").GetComponent<CapybaraHandler>();
        if (capybaraHandler == null) return;
        capybaraHandler.RemoveCapybara(gameObject);

        // Handle destroy
        Destroy(gameObject);
    }

    // Determines whether there is more room for the Capybara to strafe right or left during non-opposing collision avoidance
    private StrafeDirection ResolveStrafeDirection()
    {
        float leftBoundaryDistance, rightBoundaryDistance;
        PathPosition = Intersection.CalculatePathPosition(new Vector3(gameObject.transform.position.x, 0, gameObject.transform.position.z), gameObject.transform.right, new Vector3(currentPath.spacedPoints[0].x, 0, currentPath.spacedPoints[0].z), currentPath.spacedPoints[1] - currentPath.spacedPoints[0]);
        if(Vector3.Distance(transform.right, PathPosition) <= Vector3.Distance(-transform.right, PathPosition)) // Capybara is on left side of path
        {
            leftBoundaryDistance = (pathWidth / 2) - PathPosition.magnitude;
            rightBoundaryDistance = (pathWidth / 2) + PathPosition.magnitude;
        }
        else
        {
            rightBoundaryDistance = (pathWidth / 2) - PathPosition.magnitude; // Capybara is on right side of path
            leftBoundaryDistance = (pathWidth / 2) + PathPosition.magnitude;
        }
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.right, out hit, pathWidth) && hit.transform.gameObject.tag == "Capybara")
        {
            float rightCapyDistance = Vector3.Distance(gameObject.transform.position, hit.transform.position);
            if (rightCapyDistance < rightBoundaryDistance)
                rightBoundaryDistance = rightCapyDistance;
        }
        if (Physics.Raycast(transform.position, -transform.right, out hit, pathWidth) && hit.transform.gameObject.tag == "Capybara")
        {
            float leftCapyDistance = Vector3.Distance(gameObject.transform.position, hit.transform.position);
            if (leftCapyDistance < rightBoundaryDistance)
                leftBoundaryDistance = leftCapyDistance;
        }
        if (rightBoundaryDistance > leftBoundaryDistance)
            return StrafeDirection.strafeRight;
        else
            return StrafeDirection.strafeLeft;
    }
}

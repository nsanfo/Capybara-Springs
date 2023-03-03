using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

// Contains all the information needed to travel to an amenity
public class AmenityRoute
{
    private readonly Amenity amenity;
    private readonly int previous; // The index of the previous PathNode on the route to this amenity, -1 if the amenity is located on the starting path
    private readonly float previousDistance;
    private readonly Path path;
    private Stack<int> nodeRoute = new Stack<int>(); // A stack containing indices for PathNodes in the NodeGraph's nodes[] array, representing the path leading to an amenity

    public AmenityRoute(Amenity amenity, int previous, float previousDistance, Path path)
    {
        this.amenity = amenity;
        this.previous = previous;
        this.previousDistance = previousDistance;
        this.path = path;
    }

    public Amenity Amenity { get => amenity; }
    public int Previous { get => previous; }
    public float PreviousDistance { get => previousDistance; }
    public Stack<int> NodeRoute { get => nodeRoute; }
    public Path Path { get => path; }

    public void RoutePush(int input)
    {
        nodeRoute.Push(input);
    }
}

public class Pathfinder : MonoBehaviour
{
    private NodeGraph nodeGraph;
    private PathNode[] nodes;

    private bool[] visited;
    private float[] cost;
    private int[] previous;
    private int remainingUnvisited;

    private AmenityRoute bestDestination;
    private float bestRating;

    private CapybaraInfo capyInfo;

    private void Start()
    {
        var playerBuilding = GameObject.Find("PlayerBuilding");
        var pathBuilder = playerBuilding.GetComponent<PathBuilder>();

        nodeGraph = pathBuilder.nodeGraph;
        nodes = nodeGraph.Nodes;

        capyInfo = this.gameObject.GetComponent<CapybaraInfo>();
    }

    // Assigns a numerical rating to an amenity based on the needs of the capybara, the speed at which the amenity will fill those needs, and the distance from the capybara
    // to the amenity. The capybara will choose the amenity with the highest rating as its next destination.
    private float RateAmenity(Amenity amenity, float distance)
    {
        CapybaraInfo capyInfo = this.gameObject.GetComponent<CapybaraInfo>();;
        float hungerFill = amenity.hungerFill;
        float comfortFill = amenity.comfortFill;
        float funFill = amenity.funFill;

        float hungerRating = (1f / 10f) * Mathf.Pow(capyInfo.hunger - 100, 2) * hungerFill / distance; // the rating algorithm gives an exponentially greater priority to needs that are lower than others
        float comfortRating = (1f / 10f) * Mathf.Pow(capyInfo.comfort - 100, 2) * comfortFill / distance;
        float funRating = (1 / 10) * Mathf.Pow(capyInfo.fun - 100, 2) * funFill / distance;
        float bestRating;

        if (hungerRating > comfortRating)
            bestRating = hungerRating;
        else
            bestRating = comfortRating;
        if (bestRating < funRating)
            bestRating = funRating;
        return bestRating;
    }

    // Rates all amenities on a specified path, from a specified position. Updates the global bestAmenity variable if a better amenity is found. Used when checking the capybara's current path.
    private void RatePathAmenities(Path path, Vector3 position)
    {
        var pathAmenities = path.Amenities;
        if (pathAmenities.Count > 0)
        {
            for (int i = 0; i < pathAmenities.Count; i++)
            {
                if (pathAmenities[i].amenitySlots.Count(capy => capy != null) == pathAmenities[i].amenitySlots.Length)
                    continue;

                var distance = Vector3.Distance(pathAmenities[i].PathCollider.gameObject.transform.position, position);
                var amenityRating = RateAmenity(pathAmenities[i], distance);
                if (amenityRating > bestRating)
                {
                    bestDestination = new AmenityRoute(pathAmenities[i], -1, distance, path);
                    bestRating = amenityRating;
                }
            }
        }
    }

    // Rates all amenities on a specified path, from a specified PathNode. Updates the global bestAmenity variable if a better amenity is found.
    private void RatePathAmenities(Path path, PathNode node)
    {
        var pathAmenities = path.Amenities;
        if (pathAmenities.Count > 0)
        {
            for (int i = 0; i < pathAmenities.Count; i++)
            {
                var nodeIndex = nodeGraph.GetNodeIndex(node);
                var distance = cost[nodeIndex] + Vector3.Distance(pathAmenities[i].PathCollider.gameObject.transform.position, node.gameObject.transform.position);
                var amenityRating = RateAmenity(pathAmenities[i], distance);
                if (amenityRating > bestRating)
                {
                    bestDestination = new AmenityRoute(pathAmenities[i], nodeIndex, distance, path);
                    bestRating = amenityRating;
                }
            }
        }
    }

    // Finds the distances from this GameObject to the PathNodes at the ends of the specified path
    private ((int, float), (int, float)) PathNodeDistances(Path path)
    {
        var node1 = path.nodes[0];
        var node2 = path.nodes[1];
        var node1Index = nodeGraph.GetNodeIndex(node1);
        var node2Index = nodeGraph.GetNodeIndex(node2);
        var distance1 = Vector3.Distance(gameObject.transform.position, node1.gameObject.transform.position);
        var distance2 = Vector3.Distance(gameObject.transform.position, node2.gameObject.transform.position);
        return ((node1Index, distance1), (node2Index, distance2));
    }


    // Uses Dijkstra's Algorithm to find the path distances from the capybara to all PathNodes connected to the capybara's current path and find the next amenity that the
    // capybara should travel to based on its needs. Returns an AmenityRoute for the destination amenity.
    public AmenityRoute FindAmenityDestination(Path startingPath)
    {
        nodes = nodeGraph.Nodes;
        bestDestination = null;
        bestRating = 0;
        visited = new bool[nodes.Length];
        cost = new float[nodes.Length];
        previous = new int[nodes.Length];
        remainingUnvisited = nodes.Length;

        for (int i = 0; i < visited.Length; i++)
        {
            visited[i] = false;
        }

        for (int i = 0; i < cost.Length; i++)
        {
            cost[i] = float.PositiveInfinity;
        }

        var nodeDistances = PathNodeDistances(startingPath);
        cost[nodeDistances.Item1.Item1] = nodeDistances.Item1.Item2;
        cost[nodeDistances.Item2.Item1] = nodeDistances.Item2.Item2;
        previous[nodeDistances.Item1.Item1] = -1;
        previous[nodeDistances.Item2.Item1] = -1;

        RatePathAmenities(startingPath, gameObject.transform.position);
        var currentPath = startingPath;

        bool keepGoing = true;
        while (remainingUnvisited > 0 && keepGoing)
        {
            keepGoing = false;
            float lowestCost = float.PositiveInfinity;
            int lowestCostIndex = -1;

            for (int i = 0; i < nodes.Length; i++)
            {
                if (visited[i] == false && cost[i] < lowestCost)
                {
                    lowestCost = cost[i];
                    lowestCostIndex = i;
                }
            }

            if (lowestCost != float.PositiveInfinity)
            {
                keepGoing = true;
                visited[lowestCostIndex] = true;
                remainingUnvisited--;
                var lowestCostNode = nodes[lowestCostIndex];
                var connectedNodes = nodeGraph.GetConnectedNodes(nodes[lowestCostIndex]);
                for (int i = 0; i < connectedNodes.Length; i++)
                {
                    if (connectedNodes[i] != null)
                    {
                        var nodeIndex = nodeGraph.GetNodeIndex(connectedNodes[i]);
                        if (nodeIndex != -1 && visited[nodeIndex] == false)
                        {
                            var newDistance = cost[lowestCostIndex] + Vector3.Distance(connectedNodes[i].gameObject.transform.position, lowestCostNode.gameObject.transform.position);
                            if (newDistance < cost[nodeIndex])
                            {
                                cost[nodeIndex] = newDistance;
                                previous[nodeIndex] = lowestCostIndex;
                            }
                        }
                    }
                }
                if (previous[lowestCostIndex] != -1)
                {
                    currentPath = nodeGraph.GetPath(lowestCostIndex, previous[lowestCostIndex]);
                    RatePathAmenities(currentPath, nodes[previous[lowestCostIndex]]);
                    RatePathAmenities(currentPath, lowestCostNode);
                }
            }
        }
        CompileNodeRoute(bestDestination);
        return bestDestination;
    }

    private void CompileNodeRoute(AmenityRoute amenityRoute)
    {
        if (bestDestination != null && bestDestination.Previous != -1)
        {
            var previousIndex = bestDestination.Previous;
            while (previousIndex != -1)
            {
                amenityRoute.RoutePush(previousIndex);
                previousIndex = previous[previousIndex];
            }
        }
    }

    private IEnumerator Wait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
}

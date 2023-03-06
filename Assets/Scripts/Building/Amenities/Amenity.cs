using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Amenity : MonoBehaviour
{
    private (float, PathNode) nodeDistance1;
    private (float, PathNode) nodeDistance2;

    public GameObject PathCollider { get; set; }

    [Header("Needs Fulfillment")]
    public float hungerFill;
    public float comfortFill; 
    public float funFill;

    [Header("")]
    public int capacity;
    private int numOccupants;

    public ((float, PathNode), (float, PathNode)) GetDistances()
    {
        return (nodeDistance1, nodeDistance2);
    }

    public void PathSetup()
    {
        var pathScript = PathCollider.transform.parent.parent.gameObject.GetComponent<Path>();
        var node1 = pathScript.nodes[0];
        var node2 = pathScript.nodes[1];
        var distance1 = (gameObject.transform.position - node1.gameObject.transform.position).magnitude;
        var distance2 = (gameObject.transform.position - node2.gameObject.transform.position).magnitude;
        nodeDistance1 = (distance1, node1);
        nodeDistance2 = (distance2, node2);
        pathScript.AddAmenity(this);
    }

    public bool IncrementOccupancy()
    {
        if (numOccupants == capacity)
            return false;
        else
        {
            numOccupants++;
            return true;
        }
    }

    public void DecrementOccupancy()
    {
        numOccupants--;
    }

    public bool CheckFull()
    {
        if (numOccupants == capacity)
            return true;
        else
            return false;
    }
}

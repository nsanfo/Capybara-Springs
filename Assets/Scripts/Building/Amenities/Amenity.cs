using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Amenity : MonoBehaviour
{
    private (float, PathNode) nodeDistance1;
    private (float, PathNode) nodeDistance2;
    public GameObject[] amenitySlots;

    public GameObject PathCollider { get; set; }

    public float hungerFill, comfortFill, funFill;
    public int numSlots;

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

    public void SlotSetup()
    {
        if (numSlots == 0)
            numSlots = 1;

        amenitySlots = new GameObject[numSlots];
    }

    public void AddCapybara(GameObject capybara)
    {
        for (int i = 0; i < amenitySlots.Length; i++)
        {
            if (amenitySlots[i] == null)
            {
                amenitySlots[i] = capybara;
            }
        }
    }

    public void RemoveCapybara(GameObject capybara)
    {
        for (int i = 0; i < amenitySlots.Length; i++)
        {
            if (amenitySlots[i] == capybara)
            {
                amenitySlots[i] = null;
            }
        }
    }
}

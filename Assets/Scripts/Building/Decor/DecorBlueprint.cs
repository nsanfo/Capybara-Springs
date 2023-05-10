using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorBlueprint : MonoBehaviour
{
    public GameObject concrete;
    public double cost;
    private List<(Vector3, GameObject)> pathCollisions = new List<(Vector3, GameObject)>(); // List of tuples, each containing (Forward vector, Position) of path colliders
    public int buildCollisions = 0;

    public GameObject GetConcrete()
    {
        return concrete;
    }

    public double GetCost()
    {
        return cost;
    }
}

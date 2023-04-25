using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmenityBlueprint : MonoBehaviour
{
   public GameObject concrete;
    public double cost;
    public float entranceDistance;
    public float snapDistance;
    private List<(Vector3, GameObject)> pathCollisions = new List<(Vector3, GameObject)>(); // List of tuples, each containing (Forward vector, Position) of path colliders
    public int buildCollisions = 0;

    public GameObject GetConcrete()
    {
        return concrete;
    }

    public int GetPathCollisionCount()
    {
        return pathCollisions.Count;
    }

    public void AddPathCollision(Vector3 forwardVector, GameObject pathCollider)
    {
        pathCollisions.Add((forwardVector, pathCollider));
    }

    public bool RemovePathCollision(Vector3 forwardVector, GameObject pathCollider)
    {
        return pathCollisions.Remove((forwardVector, pathCollider));
    }

    public void ClearPathCollision()
    {
        pathCollisions.Clear();
    }

    //return a tuple containing the forward vector of the closest PathCollider, the position of the closest PathCollider, whether the blueprint is on the right or left side of the path
    //containing the closest PathCollider, and the distance between the blueprint and the closest PathCollider
    public (Vector3, GameObject, bool, float) FindClosestCollider(Vector3 value)
    {
        float shortestDistance = float.PositiveInfinity;
        Vector3 closestForward = new Vector3(0,0,0);
        GameObject closestPathCollider = pathCollisions[0].Item2;
        bool rightSide;
        for(int i = 0; i < pathCollisions.Count; i++)
        {
            var subtraction = Vector3.Distance(value, pathCollisions[i].Item2.transform.position);
            if(subtraction < shortestDistance)
            {
                shortestDistance = subtraction;
                closestPathCollider = pathCollisions[i].Item2;
                closestForward = pathCollisions[i].Item1;
            }
        }
        var up = new Vector3(0,1,0);
        var closestRight = Vector3.Scale(Vector3.Cross(closestForward.normalized, up), new Vector3(0.001f, 0.001f, 0.001f));
        var closestLeft = -closestRight;
        closestRight = closestPathCollider.transform.position + closestRight;
        closestLeft = closestPathCollider.transform.position + closestLeft;
        var rightDistance = Vector3.Distance(value, closestRight);
        var leftDistance = Vector3.Distance(value, closestLeft);

        if (rightDistance < leftDistance)
            rightSide = true;
        else
            rightSide = false;
        return (closestForward, closestPathCollider, rightSide, shortestDistance);
    }
}

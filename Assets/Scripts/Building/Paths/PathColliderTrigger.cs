using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PathColliderTrigger : MonoBehaviour
{
    public bool isPathCollision;
    public Vector3 pathForward;

    private void Start()
    {
        if (gameObject.name == "PathCollider")
        {
            var pathScript = this.gameObject.transform.parent.parent.gameObject.GetComponent<Path>();
            pathForward = pathScript.spacedPoints[0] - pathScript.spacedPoints[1];
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.name == PathBuilder.PathNames.PathCollider.ToString())
        {
            isPathCollision = true;
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.name == PathBuilder.PathNames.PathCollider.ToString())
        {
            isPathCollision = false;
        }
    }
}
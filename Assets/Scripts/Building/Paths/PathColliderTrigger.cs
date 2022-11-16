using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PathColliderTrigger : MonoBehaviour
{
    public bool isPathCollision;

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
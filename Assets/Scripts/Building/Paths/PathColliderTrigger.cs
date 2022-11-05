using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PathColliderTrigger : MonoBehaviour
{
    private bool isPathCollision;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.name == "PathCollider")
        {
            isPathCollision = true;
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.name == "PathCollider")
        {
            isPathCollision = false;
        }
    }

    public bool GetCollision()
    {
        return isPathCollision;
    }
}
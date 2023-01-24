using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapColliderTrigger : MonoBehaviour
{
    Blueprint blueprintScript;

    private void Start()
    {
        blueprintScript = this.gameObject.transform.parent.gameObject.GetComponent<Blueprint>();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.name == "PathCollider")
        {
            var pathScript = collider.gameObject.transform.parent.parent.gameObject.GetComponent<Path>();
            var pathForward = pathScript.spacedPoints[0] - pathScript.spacedPoints[1];
            blueprintScript.AddPathCollision(pathForward, collider.gameObject.transform.position);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.name == "PathCollider")
        {
            var pathScript = collider.gameObject.transform.parent.parent.gameObject.GetComponent<Path>();
            var pathForward = pathScript.spacedPoints[0] - pathScript.spacedPoints[1];
            blueprintScript.RemovePathCollision(pathForward, collider.gameObject.transform.position);
        }
    }
}

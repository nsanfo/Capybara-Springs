using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorColliderTrigger : MonoBehaviour
{
    DecorBlueprint blueprintScript;

    private void Start()
    {
        blueprintScript = this.gameObject.transform.parent.gameObject.GetComponent<DecorBlueprint>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Amenity" || other.gameObject.tag == "Decor" || other.gameObject.tag == "Fence" || other.gameObject.name == "PathCollider")
            blueprintScript.buildCollisions++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Amenity" || other.gameObject.tag == "Decor" || other.gameObject.tag == "Fence" || other.gameObject.name == "PathCollider")
            blueprintScript.buildCollisions--;
    }
}

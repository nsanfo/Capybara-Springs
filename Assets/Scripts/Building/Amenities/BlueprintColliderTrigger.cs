using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueprintColliderTrigger : MonoBehaviour
{
    AmenityBlueprint blueprintScript;

    private void Start()
    {
        blueprintScript = this.gameObject.transform.parent.gameObject.GetComponent<AmenityBlueprint>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Amenity" || other.gameObject.tag == "Decor" || other.gameObject.tag == "Fence")
            blueprintScript.buildCollisions++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Amenity" || other.gameObject.tag == "Decor" || other.gameObject.tag == "Fence")
            blueprintScript.buildCollisions--;
    }
}

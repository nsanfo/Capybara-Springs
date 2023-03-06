using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnsenInteraction : MonoBehaviour, InteractionInterface
{
    public void HandleInteraction(GameObject capybara, Amenity amenity, int slotLocation, GameObject emitterObject)
    {
        // Get position related to slot
        Quaternion rot = Quaternion.AngleAxis((float) slotLocation / amenity.numSlots * 360, Vector3.up);
        float randomForward = Random.Range(amenity.insidePositioningMulti - amenity.insidePositioningRange, amenity.insidePositioningMulti);
        Vector3 forwardMulti = Vector3.forward * randomForward;
        Vector3 amenityPos = amenity.transform.position + rot * forwardMulti;

        // Set position
        capybara.transform.position = new Vector3(amenityPos.x, amenityPos.y + amenity.insideCenteringHeight, amenityPos.z);
        capybara.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        emitterObject.GetComponent<ParticleSystem>().Play();
    }

    public void HandleInteractingAnimation()
    {
        Debug.Log("LOGGING ANIMATION");
    }
}

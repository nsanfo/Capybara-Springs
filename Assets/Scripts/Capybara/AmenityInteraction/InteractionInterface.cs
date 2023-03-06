using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface InteractionInterface
{
    void HandleInteraction(GameObject capybara, Amenity amenity, int slotLocation, GameObject emitterObject);

    void HandleInteractingAnimation();
}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public interface InteractionInterface
{
    void HandleInteraction(Amenity amenity, int slotLocation, GameObject emitterObject);

    void HandleInteractingAnimation();

    void StopEmitters();
}

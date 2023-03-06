using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public interface InteractionInterface
{
    AmenityInterface AmenityInterface { get; set; }

    void HandleInteraction(Amenity amenity, int slotLocation, GameObject emitterObject);

    void HandleInteractingAnimation();

    void StopEmitters();
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunInteraction : MonoBehaviour, InteractionInterface
{
    private Amenity amenity;
    private Vector3 amenityPosition;
    private GameObject musicEmitterObject;
    private Quaternion? rotationEndPosition;

    public AmenityInterface AmenityInterface { get; set; }
    private FunAmenity funInterface;

    public AudioSource poofSound;

    public void SetMusicEmitter(GameObject musicEmitterObject)
    {
        this.musicEmitterObject = musicEmitterObject;
    }

    public void HandleInteraction(Amenity amenity, int slotLocation, GameObject smokeEmitterObject)
    {
        if (AmenityInterface is not FunAmenity) return;
        funInterface = (FunAmenity) AmenityInterface;

        this.amenity = amenity;
    }

    public void HandleInteractingAnimation()
    {
    }

    public void HandleInteractionEnd()
    {
    }
}

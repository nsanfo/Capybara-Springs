using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunInteraction : MonoBehaviour, InteractionInterface
{
    private Amenity amenity;
    private Vector3 amenityPosition;
    private GameObject danceEmitterObject;
    private Quaternion? rotationEndPosition;

    public AmenityInterface AmenityInterface { get; set; }
    private FunAmenity funInterface;

    public AudioSource poofSound;

    public void SetDanceEmitter(GameObject danceEmitterObject)
    {
        this.danceEmitterObject = danceEmitterObject;
    }

    public void HandleInteraction(Amenity amenity, int slotLocation, GameObject smokeEmitterObject)
    {
        if (AmenityInterface is not FunAmenity) return;
        funInterface = (FunAmenity)AmenityInterface;

        this.amenity = amenity;

        SetSlotPlosition(slotLocation);

        smokeEmitterObject.GetComponent<ParticleSystem>().Play();
        poofSound.Play();

        // Play dance emitter only on largest amenity
        if (amenity.numSlots == 10)
        {
            danceEmitterObject.transform.position = transform.position;
            danceEmitterObject.GetComponent<ParticleSystem>().Play();
        }
    }

    public void HandleInteractingAnimation()
    {
    }

    public void HandleInteractionEnd()
    {
        if (amenity.numSlots == 10)
        {
            danceEmitterObject.GetComponent<ParticleSystem>().Stop();
            danceEmitterObject.GetComponent<ParticleSystem>().Clear();
        }
    }

    private void SetSlotPlosition(int slotLocation)
    {
        Vector3 rotationEuler = amenity.gameObject.transform.localRotation.eulerAngles;
        Quaternion rotation = Quaternion.Euler(rotationEuler);

        // Get position related to slot
        if (amenity.numSlots == 1)
        {
            amenityPosition = new Vector3(amenity.transform.position.x, amenity.transform.position.y + 0.18f, amenity.transform.position.z);
        }
        else if (amenity.numSlots == 4)
        {
            Quaternion rot = Quaternion.AngleAxis(((float) slotLocation / 4 * 360) + amenity.transform.localRotation.eulerAngles.y + 45, Vector3.up);
            Vector3 forwardMulti = Vector3.forward * 0.23f;
            amenityPosition = amenity.transform.position + rot * forwardMulti;
            amenityPosition = new Vector3(amenityPosition.x, amenityPosition.y + 0.18f, amenityPosition.z);
            rotation = Quaternion.Euler(rotationEuler.x, rotationEuler.y + 180, rotationEuler.z);
        }
        else if (amenity.numSlots == 10)
        {
            Quaternion rot = Quaternion.AngleAxis((float)slotLocation / 10 * 360, Vector3.up);
            Vector3 forwardMulti = Vector3.forward * 0.7f;
            amenityPosition = amenity.transform.position + rot * forwardMulti;
            amenityPosition = new Vector3(amenityPosition.x, amenityPosition.y + 0.045f, amenityPosition.z);
            //rotation = Quaternion.Euler(rotation.x, rotation.y + 200, rotation.z);
            rotation = Quaternion.Euler(rotationEuler);
        }

        // Set position
        transform.SetPositionAndRotation(amenityPosition, rotation);
    }
}

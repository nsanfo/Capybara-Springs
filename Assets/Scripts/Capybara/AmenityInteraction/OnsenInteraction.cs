using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnsenInteraction : MonoBehaviour, InteractionInterface
{
    private Vector3 amenityPosition;
    private GameObject splashEmitterObject;

    public void SetSplashEmitter(GameObject splashEmitterObject)
    {
        this.splashEmitterObject = splashEmitterObject;
    }

    public void HandleInteraction(Amenity amenity, int slotLocation, GameObject smokeEmitterObject)
    {
        // Get position related to slot
        Quaternion rot = Quaternion.AngleAxis((float) slotLocation / amenity.numSlots * 360, Vector3.up);
        float randomForward = Random.Range(amenity.insidePositioningMulti - amenity.insidePositioningRange, amenity.insidePositioningMulti);
        Vector3 forwardMulti = Vector3.forward * randomForward;
        amenityPosition = amenity.transform.position + rot * forwardMulti;

        //DebugPositions(amenity);

        // Set position
        transform.position = new Vector3(amenityPosition.x, amenityPosition.y + amenity.insideCenteringHeight, amenityPosition.z);
        transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        smokeEmitterObject.GetComponent<ParticleSystem>().Play();

        splashEmitterObject.transform.position = transform.position + new Vector3(0, 0.2f, 0);
        splashEmitterObject.GetComponent<ParticleSystem>().Play();
    }

    public void HandleInteractingAnimation()
    {
        // Handle animations
    }

    public void StopEmitters()
    {
        splashEmitterObject.GetComponent<ParticleSystem>().Stop();
    }

    private void DebugPositions(Amenity amenity)
    {
        GameObject debugHolder = GameObject.Find("DebugHolder");
        if (debugHolder == null) debugHolder = new GameObject("DebugHolder");

        int numRange = (int) (amenity.insidePositioningRange / 0.01f);
        float currentPositioning = amenity.insidePositioningMulti;
        for (int i = 0; i < numRange; i++)
        {
            for (int j = 0; j < amenity.numSlots; j++)
            {
                Quaternion rotation = Quaternion.AngleAxis((float)j / amenity.numSlots * 360, Vector3.up);
                Vector3 forwardMulti = Vector3.forward * currentPositioning;
                Vector3 position = amenity.transform.position + rotation * forwardMulti;

                GameObject debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                debugSphere.transform.position = new Vector3(position.x, position.y + amenity.insideCenteringHeight, position.z);
                debugSphere.name = "Debug " + i + j;
                debugSphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                debugSphere.transform.SetParent(debugHolder.transform);
            }

            currentPositioning -= 0.01f;
        }
    }
}

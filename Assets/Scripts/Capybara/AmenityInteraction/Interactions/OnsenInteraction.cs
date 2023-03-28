using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnsenInteraction : MonoBehaviour, InteractionInterface
{
    private Amenity amenity;
    private Vector3 amenityPosition;
    private GameObject splashEmitterObject;
    private Quaternion? rotationEndPosition;

    public AmenityInterface AmenityInterface { get; set; }
    private OnsenAmenity onsenInterface;

    public void SetSplashEmitter(GameObject splashEmitterObject)
    {
        this.splashEmitterObject = splashEmitterObject;
    }

    public void HandleInteraction(Amenity amenity, int slotLocation, GameObject smokeEmitterObject)
    {
        if (AmenityInterface is not OnsenAmenity) return;
        onsenInterface = (OnsenAmenity) AmenityInterface;

        this.amenity = amenity;

        // Get position related to slot
        Quaternion rot = Quaternion.AngleAxis((float) slotLocation / amenity.numSlots * 360, Vector3.up);
        float randomForward = Random.Range(onsenInterface.insidePositioningMulti - onsenInterface.insidePositioningRange, onsenInterface.insidePositioningMulti);
        Vector3 forwardMulti = Vector3.forward * randomForward;
        amenityPosition = amenity.transform.position + rot * forwardMulti;

        //DebugPositions(amenity);

        // Set position
        transform.position = new Vector3(amenityPosition.x, amenityPosition.y + onsenInterface.insideCenteringHeight, amenityPosition.z);
        transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        smokeEmitterObject.GetComponent<ParticleSystem>().Play();

        splashEmitterObject.transform.position = transform.position + new Vector3(0, onsenInterface.splashHeight, 0);
        splashEmitterObject.GetComponent<ParticleSystem>().Play();

        StartCoroutine(WaitForTurning());
    }

    public void HandleInteractingAnimation()
    {
        // Handle animations
        if (rotationEndPosition != null)
        {
            if (Mathf.Abs(rotationEndPosition.Value.eulerAngles.y - transform.eulerAngles.y) <= 1f)
            {
                GetComponent<Animator>().SetBool("Turning", false);
                rotationEndPosition = null;
                StartCoroutine(WaitForTurning());
            }
        }
    }

    public void HandleInteractionEnd()
    {
        splashEmitterObject.GetComponent<ParticleSystem>().Stop();
    }

    private void DebugPositions(Amenity amenity)
    {
        GameObject debugHolder = GameObject.Find("DebugHolder");
        if (debugHolder == null) debugHolder = new GameObject("DebugHolder");

        int numRange = (int) (onsenInterface.insidePositioningRange / 0.01f);
        float currentPositioning = onsenInterface.insidePositioningMulti;
        for (int i = 0; i < numRange; i++)
        {
            for (int j = 0; j < amenity.numSlots; j++)
            {
                Quaternion rotation = Quaternion.AngleAxis((float)j / amenity.numSlots * 360, Vector3.up);
                Vector3 forwardMulti = Vector3.forward * currentPositioning;
                Vector3 position = amenity.transform.position + rotation * forwardMulti;

                GameObject debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                debugSphere.transform.position = new Vector3(position.x, position.y + onsenInterface.insideCenteringHeight, position.z);
                debugSphere.name = "Debug " + i + j;
                debugSphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                debugSphere.transform.SetParent(debugHolder.transform);
            }

            currentPositioning -= 0.01f;
        }
    }

    private IEnumerator WaitForTurning()
    {
        yield return new WaitForSeconds(Random.Range(5, 8));
        GetComponent<Animator>().SetBool("Turning", true);
        CalculateRotation();
    }

    private void CalculateRotation()
    {
        Quaternion insideRot = Quaternion.AngleAxis((float)Random.Range(0, 360) / amenity.numSlots, Vector3.up);
        rotationEndPosition = insideRot;
        
        Vector3 insideForwardMulti = Vector3.forward * 0.1f;
        Vector3 rotationPosition = amenityPosition + insideRot * insideForwardMulti;

        if ((rotationPosition - (gameObject.transform.position + gameObject.transform.right)).magnitude <= (rotationPosition - (gameObject.transform.position - gameObject.transform.right)).magnitude)
        {
            GetComponent<Animator>().SetFloat("Turn", 1f);
        }
        else
        {
            GetComponent<Animator>().SetFloat("Turn", -1f);
        }
    }
}

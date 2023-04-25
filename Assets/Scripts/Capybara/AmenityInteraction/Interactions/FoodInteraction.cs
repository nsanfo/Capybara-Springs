using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;
using Random = UnityEngine.Random;

public class FoodInteraction : MonoBehaviour, InteractionInterface
{
    private Amenity amenity;
    private Vector3 amenityPosition;
    private GameObject eatEmitterObject, eatingObject;
    private Boolean eatAnimation = false;

    public AmenityInterface AmenityInterface { get; set; }
    private FoodAmenity foodInterface;

    public GameObject chewingSoundObject;
    public AudioSource poofSound;

    public void SetEatEmitter(GameObject eatEmitterObject)
    {
        this.eatEmitterObject = eatEmitterObject;
    }

    public void HandleInteraction(Amenity amenity, int slotLocation, GameObject smokeEmitterObject)
    {
        if (AmenityInterface is not FoodAmenity) return;
        foodInterface = (FoodAmenity)AmenityInterface;

        this.amenity = amenity;

        // Get position related to slot
        float slotAngle = foodInterface.angleForSlots;
        if (slotAngle < 180 && slotAngle > 360) slotAngle = 360;
        Quaternion rot = Quaternion.AngleAxis((float)slotLocation / amenity.numSlots * slotAngle, Vector3.up);
        rot *= Quaternion.Euler(0, 200f, 0);
        float randomForward = Random.Range(foodInterface.insidePositioningMulti - foodInterface.insidePositioningRange, foodInterface.insidePositioningMulti);
        Vector3 forwardMulti = Vector3.forward * randomForward;
        float multiExtender = foodInterface.positioningMultiExtender;
        if (foodInterface.positioningMultiExtender == 0) multiExtender = 1;
        amenityPosition = amenity.transform.position + rot * (forwardMulti * multiExtender);

        // Set position
        transform.position = amenityPosition;
        transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        smokeEmitterObject.GetComponent<ParticleSystem>().Play();
        poofSound.Play();

        // Set emitter path
        string path = "Root/Pelvis/Spine.1/Spine.2/Neck.1/Neck.2/Head/Joe/Tongue.1/Tongue.2";
        GameObject tongue = transform.Find(path).gameObject;

        eatEmitterObject.transform.SetParent(tongue.transform);
        eatEmitterObject.transform.localPosition = Vector3.zero;
        Quaternion tongueRot = tongue.transform.rotation;
        eatEmitterObject.transform.localRotation = Quaternion.Euler(tongueRot.x - 90, tongueRot.y, tongueRot.z);

        // Set emitter particle color
        var psMain = eatEmitterObject.GetComponent<ParticleSystem>().main;
        psMain.startColor = foodInterface.particleColor;
        eatEmitterObject.GetComponent<ParticleSystem>().Play();

        // Set animation
        GetComponent<Animator>().SetBool("Chewing", true);

        // Set object to eat
        eatingObject = foodInterface.eatingObject;
        if (eatingObject != null)
        {
            eatingObject = Instantiate(eatingObject);
            eatingObject.transform.position = transform.position + (transform.forward * 0.2f);
            eatingObject.transform.position = new Vector3(eatingObject.transform.position.x, eatingObject.transform.position.y + 0.092f, eatingObject.transform.position.z);
        }

        chewingSoundObject.SetActive(true);

        StartCoroutine(PlayEatAnimation());
        //DebugPositions(amenity);
    }

    public void HandleInteractingAnimation()
    {
        if (!eatAnimation)
        {
            StartCoroutine(PlayEatAnimation());
        }
    }

    public void HandleInteractionEnd()
    {
        eatEmitterObject.GetComponent<ParticleSystem>().Stop();
        eatEmitterObject.transform.SetParent(transform);
        GetComponent<Animator>().SetBool("Chewing", false);
        if (eatingObject != null) Destroy(eatingObject);
        chewingSoundObject.SetActive(false);
    }

    private IEnumerator PlayEatAnimation()
    {
        eatAnimation = true;
        yield return new WaitForSeconds(Random.Range(5, 8));
        GetComponent<Animator>().SetBool("HeadBob", true);
        yield return new WaitForSeconds(Random.Range(2, 4));
        GetComponent<Animator>().SetBool("HeadBob", false);
        eatAnimation = false;
    }

    private void DebugPositions(Amenity amenity)
    {
        GameObject debugHolder = GameObject.Find("DebugHolder");
        if (debugHolder == null) debugHolder = new GameObject("DebugHolder");

        int numRange = (int)(foodInterface.insidePositioningRange / 0.01f);
        float currentPositioning = foodInterface.insidePositioningMulti;
        for (int i = 0; i < numRange; i++)
        {
            for (int j = 0; j < amenity.numSlots; j++)
            {
                float slotAngle = foodInterface.angleForSlots;
                if (slotAngle < 180 && slotAngle > 360) slotAngle = 360;
                Quaternion rotation = Quaternion.AngleAxis((float)j / amenity.numSlots * slotAngle, Vector3.up);
                rotation *= Quaternion.Euler(0, 200f, 0);
                Vector3 forwardMulti = Vector3.forward * currentPositioning;
                float multiExtender = foodInterface.positioningMultiExtender;
                if (foodInterface.positioningMultiExtender == 0) multiExtender = 1;
                Vector3 position = amenity.transform.position + rotation * (forwardMulti * multiExtender);

                GameObject debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                debugSphere.transform.position = new Vector3(position.x, position.y + 0.2f, position.z);
                debugSphere.name = "Debug " + i + j;
                debugSphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                debugSphere.transform.SetParent(debugHolder.transform);
            }

            currentPositioning -= 0.01f;
        }
    }
}

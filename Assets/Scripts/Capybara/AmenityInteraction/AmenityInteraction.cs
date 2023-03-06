using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class AmenityInteraction : MonoBehaviour
{
    private Amenity amenity;
    Animator capyAnimator;
    int currentState = -1;
    int slotLocation;

    // Centering and rotation
    private Vector3 centeringStartPosition;
    private Vector3 centeringEndPosition;
    private float centeringDuration = 0.5f;
    private float centeringElapsedTime;

    private Quaternion rotationStartPosition;
    private Quaternion rotationEndPosition;
    private float rotationDuration = 0.5f;
    private float rotationElapsedTime;
    private float rotationCompleted;

    private Vector3 amenityFront;
    private InteractionInterface interactionInterface;

    public GameObject smokeEmitterPrefab;
    private GameObject smokeEmitterObject;
    public GameObject splashEmitterPrefab;
    private GameObject splashEmitterObject;
    private Renderer[] capybaraRenderer = new Renderer[2];

    private void Update()
    {
        if (amenity == null) return;

        PositionCapybara();
        EnterAmenity();
        ExitAmenity();

        if (currentState == 3 && interactionInterface != null) interactionInterface.HandleInteractingAnimation();
    }

    public void HandleInteraction(Amenity amenity)
    {
        // Set amenity
        this.amenity = amenity;
        slotLocation = amenity.AddCapybara(gameObject);

        // Position capybara in place for transport
        amenityFront = Vector3.Lerp(amenity.transform.position, amenity.PathCollider.transform.position, amenity.enteringForwardMulti);

        InstantiateEmitters();
        SetCapybaraRenderArray();
        currentState = 0;
    }

    private void SetCapybaraRenderArray()
    {
        capyAnimator = GetComponent<Animator>();

        // Get renderers for capybara
        List<Renderer> renderList = new List<Renderer>();
        GameObject capybara = transform.Find("Capybara").gameObject;
        if (capybara != null)
        {
            renderList.Add(capybara.GetComponent<Renderer>());
        }

        string path = "Root/Pelvis/Spine.1/Spine.2/Neck.1/Neck.2/Head/";
        GameObject eyeL = transform.Find(path + "EyeL").gameObject;
        if (eyeL != null)
        {
            renderList.Add(eyeL.GetComponent<Renderer>());
        }

        GameObject eyeR = transform.Find(path + "EyeR").gameObject;
        if (eyeR != null)
        {
            renderList.Add(eyeR.GetComponent<Renderer>());
        }

        capybaraRenderer = renderList.ToArray();

        rotationEndPosition = Quaternion.LookRotation(amenity.transform.position - transform.position);
        capyAnimator.SetBool("Turning", true);

        if ((amenity.transform.position - gameObject.transform.right).magnitude <= (amenity.transform.position - -gameObject.transform.right).magnitude)
            capyAnimator.SetFloat("Turn", 1f);
        else
            capyAnimator.SetFloat("Turn", -1f);
    }

    private void PositionCapybara()
    {
        if (currentState == 0 && (Mathf.Abs(rotationEndPosition.eulerAngles.y - gameObject.transform.eulerAngles.y) <= 1f))
        {
            capyAnimator.SetBool("Turning", false);
            rotationCompleted = 0;
            rotationElapsedTime = 0;
            currentState = 1;
            StartCoroutine(Wait(1));
        }
    }

    private IEnumerator Wait(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        currentState = 2;
    }

    private void EnterAmenity()
    {
        if (currentState == 2)
        {
            CreateSmoke();
            HandleHiding(false);
            StartCoroutine(AppearInAmenity());
            currentState = 3;
        }
    }

    private void InstantiateEmitters()
    {
        if (smokeEmitterObject == null)
        {
            smokeEmitterObject = Instantiate(smokeEmitterPrefab);
            smokeEmitterObject.transform.SetParent(transform);
        }

        if (splashEmitterObject == null)
        {
            splashEmitterObject = Instantiate(splashEmitterPrefab);
            splashEmitterObject.transform.SetParent(transform);
        }
    }

    private void CreateSmoke()
    {
        smokeEmitterObject.transform.position = transform.position;
        smokeEmitterObject.GetComponent<ParticleSystem>().Play();
    }

    private void HandleHiding(bool hide)
    {
        for (int i = 0; i < capybaraRenderer.Length; i++)
        {
            capybaraRenderer[i].enabled = hide;
        }
    }

    private IEnumerator AppearInAmenity()
    {
        yield return new WaitForSeconds(1);

        HandleHiding(true);
        if (amenity.amenityType == AmenityEnum.Onsen)
        {
            OnsenInteraction onsenInteraction = gameObject.AddComponent<OnsenInteraction>();
            onsenInteraction.SetSplashEmitter(splashEmitterObject);
            interactionInterface = onsenInteraction;
        }

        interactionInterface.HandleInteraction(amenity, slotLocation, smokeEmitterObject);
        StartCoroutine(UpdateCapybaraStats());
    }

    private IEnumerator UpdateCapybaraStats()
    {
        yield return new WaitForSeconds(0.5f);
        CapybaraInfo capybaraInfo = gameObject.GetComponent<CapybaraInfo>();
        capybaraInfo.hunger += amenity.hungerFill;
        capybaraInfo.comfort += amenity.comfortFill;
        capybaraInfo.fun += amenity.funFill;

        if (!HandleMaxStats(capybaraInfo))
        {
            StartCoroutine(UpdateCapybaraStats());
        }
        else
        {
            interactionInterface.StopEmitters();
            currentState = 4;
        }
    }

    private Boolean HandleMaxStats(CapybaraInfo capybaraInfo)
    {
        if (capybaraInfo.hunger > 100)
        {
            capybaraInfo.hunger = 100;
            return true;
        }
        else if (capybaraInfo.comfort > 100)
        {
            capybaraInfo.comfort = 100;
            return true;
        }
        else if (capybaraInfo.fun > 100)
        {
            capybaraInfo.fun = 100;
            return true;
        }
        else
        {
            return false;
        }
    }

    private void ExitAmenity()
    {
        if (currentState == 4)
        {
            smokeEmitterObject.GetComponent<ParticleSystem>().Play();
            HandleHiding(false);
            StartCoroutine(AppearInFront());
            currentState = 5;
            amenity.RemoveCapybara(gameObject);
        }
    }

    private IEnumerator AppearInFront()
    {
        yield return new WaitForSeconds(1);
        transform.position = amenityFront;
        HandleHiding(true);
        smokeEmitterObject.GetComponent<ParticleSystem>().Play();
        currentState = -1;
        capybaraRenderer = new Renderer[2];
        RemoveInteraction();
        GetComponent<CapyAI>().CompletedAmenityInteraction();
    }

    private void RemoveInteraction()
    {
        if (amenity.amenityType == AmenityEnum.Onsen)
        {
            Destroy((UnityEngine.Object) interactionInterface);
        }

        amenity = null;
    }
}

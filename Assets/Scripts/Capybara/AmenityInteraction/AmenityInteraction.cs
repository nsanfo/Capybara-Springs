using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class AmenityInteraction : MonoBehaviour
{
    Pathfinder pathfinderScript;
    CapyAI aiScript;
    public Amenity amenity;
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
    public GameObject eatEmitterPrefab;
    private GameObject eatEmitterObject;
    private Renderer[] capybaraRenderer = new Renderer[2];

    public GameObject capybaraPlacer; // Used to find an open spot in front of the amenity for the capybara to be placed upon exiting
    enum States { center, right, left };
    States placerStates;

    private GameObject chewingSoundObject;
    private AudioSource poofSound;

    private void Start()
    {
        pathfinderScript = GetComponent<Pathfinder>();
        aiScript = GetComponent<CapyAI>();
        chewingSoundObject = transform.GetChild(7).gameObject;
        poofSound = transform.GetChild(8).GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (amenity == null) return;

        PositionCapybara();
        EnterAmenity();
        ExitAmenity();

        if (currentState == 3 && interactionInterface != null)
        {
            interactionInterface.HandleInteractingAnimation();
        }
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

        rotationStartPosition = transform.rotation;
        rotationEndPosition = Quaternion.LookRotation(amenity.transform.position - transform.position);
        capyAnimator.SetBool("Turning", true);

        if ((amenity.transform.position - (gameObject.transform.position + gameObject.transform.right)).magnitude <= (amenity.transform.position - (gameObject.transform.position - gameObject.transform.right)).magnitude)
        {
            capyAnimator.SetFloat("Turn", 1f);
        }
        else
        {
            capyAnimator.SetFloat("Turn", -1f);
        }
    }

    private void PositionCapybara()
    {
        if (currentState == 0 && (Mathf.Abs(rotationEndPosition.eulerAngles.y - gameObject.transform.eulerAngles.y) <= 5f))
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

        if (eatEmitterObject == null)
        {
            eatEmitterObject = Instantiate(eatEmitterPrefab);
            eatEmitterObject.transform.SetParent(transform);
        }
    }

    private void CreateSmoke()
    {
        smokeEmitterObject.transform.position = transform.position;
        smokeEmitterObject.GetComponent<ParticleSystem>().Play();
        poofSound.Play();
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
            OnsenAmenity onsenAmenity = amenity.gameObject.GetComponent<OnsenAmenity>();
            OnsenInteraction onsenInteraction = gameObject.AddComponent<OnsenInteraction>();
            onsenInteraction.SetSplashEmitter(splashEmitterObject);
            onsenInteraction.AmenityInterface = onsenAmenity;
            interactionInterface = onsenInteraction;
            onsenInteraction.poofSound = poofSound;
        }
        else if (amenity.amenityType == AmenityEnum.Food)
        {
            FoodAmenity foodAmenity = amenity.gameObject.GetComponent<FoodAmenity>();
            FoodInteraction foodInteraction = gameObject.AddComponent<FoodInteraction>();
            foodInteraction.SetEatEmitter(eatEmitterObject);
            foodInteraction.AmenityInterface = foodAmenity;
            foodInteraction.chewingSoundObject = chewingSoundObject;
            foodInteraction.poofSound = poofSound;
            interactionInterface = foodInteraction;
        }
        else if (amenity.amenityType == AmenityEnum.Fun)
        {
            FunAmenity funAmenity = amenity.gameObject.GetComponent<FunAmenity>();
            FunInteraction funInteraction = gameObject.AddComponent<FunInteraction>();
            //funInteraction.SetMusicEmitter();
            funInteraction.AmenityInterface = funAmenity;
            funInteraction.poofSound = poofSound;
            interactionInterface = funInteraction;
        }

        interactionInterface.HandleInteraction(amenity, slotLocation, smokeEmitterObject);
        SetFilling();
        StartCoroutine(UpdateCapybaraStats());
    }

    // Updates boolean variables which are used to enable arrows on the Capybara Details Window, indicating which needs are currently being filled
    private void SetFilling()
    {
        CapybaraInfo capybaraInfo = gameObject.GetComponent<CapybaraInfo>();
        if (amenity.hungerFill > 0)
            capybaraInfo.HungerFilling = true;
        if (amenity.comfortFill > 0)
            capybaraInfo.ComfortFilling = true;
        if (amenity.funFill > 0)
            capybaraInfo.FunFilling = true;
    }

    private void UnsetFilling()
    {
        CapybaraInfo capybaraInfo = gameObject.GetComponent<CapybaraInfo>();
        capybaraInfo.HungerFilling = false;
        capybaraInfo.ComfortFilling = false;
        capybaraInfo.FunFilling = false;
    }

    private IEnumerator UpdateCapybaraStats()
    {
        yield return new WaitForSeconds(0.5f);
        CapybaraInfo capybaraInfo = gameObject.GetComponent<CapybaraInfo>();
        capybaraInfo.hunger += (amenity.hungerFill * Time.deltaTime * 100);
        capybaraInfo.comfort += (amenity.comfortFill * Time.deltaTime * 100);
        capybaraInfo.fun += (amenity.funFill * Time.deltaTime * 100);

        if (!HandleMaxStats(capybaraInfo))
        {
            StartCoroutine(UpdateCapybaraStats());
        }
        else
        {
            interactionInterface.HandleInteractionEnd();
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
            pathfinderScript.LastAmenityUsed = amenity.gameObject;
            aiScript.previousNode = amenity.PathCollider;
            smokeEmitterObject.GetComponent<ParticleSystem>().Play();
            poofSound.Play();
            HandleHiding(false);
            StartCoroutine(AppearInFront());
            currentState = 5;
            amenity.RemoveCapybara(gameObject);
            UnsetFilling();
        }
    }

    private IEnumerator AppearInFront()
    {
        yield return new WaitForSeconds(1);

        var capyPlacer = GameObject.Instantiate(capybaraPlacer);
        var placerScript = capyPlacer.GetComponent<CapybaraPlacer>();
        capyPlacer.transform.position = amenity.PathCollider.transform.position;
        capyPlacer.transform.rotation = rotationStartPosition;
        placerStates = States.center;
        float pathDistance = 0;

        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if (placerScript.Collisions > 0)
            {
                if (placerStates == States.center)
                {
                    placerStates = States.right;
                    capyPlacer.transform.Translate(Vector3.right * 0.13f);
                    pathDistance += 0.10f;
                }
                else if (placerStates == States.right)
                {
                    if (pathDistance < 0.30)
                    {
                        capyPlacer.transform.Translate(Vector3.right * 0.13f);
                        pathDistance += 0.10f;
                    }
                    else
                    {
                        placerStates = States.left;
                        capyPlacer.transform.position = new Vector3(0, 0, 0);
                        capyPlacer.transform.Translate(Vector3.left * 0.13f);
                        pathDistance = -0.10f;
                    }
                }
                else if (placerStates == States.left)
                {
                    if (pathDistance > -0.30)
                    {
                        capyPlacer.transform.Translate(Vector3.left * 0.13f);
                        pathDistance -= 0.10f;
                    }
                    else
                    {
                        placerStates = States.center;
                        capyPlacer.transform.position = new Vector3(0, 0, 0);
                        pathDistance = 0;
                    }
                }
            }
            else
            {
                transform.position = capyPlacer.transform.position;
                transform.rotation = rotationStartPosition;
                GameObject.Destroy(capyPlacer);
                HandleHiding(true);
                smokeEmitterObject.GetComponent<ParticleSystem>().Play();
                poofSound.Play();
                currentState = -1;
                capybaraRenderer = new Renderer[2];
                RemoveInteraction();
                aiScript.CompletedAmenityInteraction();
                break;
            }
        }
    }

    private void RemoveInteraction()
    {
        if (amenity.amenityType == AmenityEnum.Onsen)
        {
            Destroy((UnityEngine.Object) interactionInterface);
        }

        interactionInterface = null;
        amenity = null;
    }
}

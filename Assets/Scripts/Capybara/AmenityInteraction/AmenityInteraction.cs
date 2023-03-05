using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class AmenityInteraction : MonoBehaviour
{
    AmenityAnimationData animationData;
    Amenity amenity;
    Animator capyAnimator;
    int currentState = -1;

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

    public GameObject smokeParticleEmitter;
    private GameObject emitterObject;
    private Renderer[] capybaraRenderer = new Renderer[2];

    private void Update()
    {
        if (amenity == null) return;

        //PositionForTeleport();
        RotateCapybara(2);
        EnterAmenity();
        ExitAmenity();
    }

    public void HandleInteraction(Amenity amenity)
    {
        this.amenity = amenity;
        animationData = AmenityAnimationHandler.GetInstance().GetAnimationData(amenity.gameObject);
        if (animationData == null)
            return;

        emitterObject = new GameObject("SmokeEmitter");
        emitterObject.transform.SetParent(this.transform);

        // Position capybara in place for teleport
        amenityFront = Vector3.Lerp(amenity.transform.position, amenity.PathCollider.transform.position, animationData.forwardMultiplier);
        //transform.LookAt(amenityFront);
        capyAnimator = gameObject.GetComponent<Animator>();
        //capyAnimator.SetBool("Travelling", true);

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

        //currentState = 0;

        currentState = 2;
        rotationEndPosition = Quaternion.LookRotation(amenity.transform.position - transform.position);
        capyAnimator.SetBool("Turning", true);

        if((amenity.transform.position - gameObject.transform.right).magnitude <= (amenity.transform.position - -gameObject.transform.right).magnitude)
            capyAnimator.SetFloat("Turn", 1f);
        else
            capyAnimator.SetFloat("Turn", -1f);
    }

    private void PositionForTeleport()
    {
        // Approach amenity front
        if (currentState == 0 && Vector3.Distance(transform.position, amenityFront) <= 0.1)
        {
            capyAnimator.SetBool("Travelling", false);
            centeringStartPosition = transform.localPosition;
            centeringEndPosition = amenityFront;
            currentState = 1;
        }

        CenterCapybara(1, amenity.transform.position);
        RotateCapybara(2);
    }

    private void EnterAmenity()
    {
        if (currentState == 3)
        {
            CreateSmoke();
            HandleHiding(false);
            StartCoroutine(AppearInAmenity(1));
            currentState = 4;
        }
    }

    private IEnumerator AppearInAmenity(int numSeconds)
    {
        yield return new WaitForSeconds(numSeconds);
        Vector3 amenityPos = amenity.transform.position;
        transform.position = new Vector3(amenityPos.x, amenityPos.y + animationData.enteredCenteringHeight, amenityPos.z);
        transform.LookAt(new Vector3(amenityFront.x, amenityFront.y + animationData.enteredCenteringHeight, amenityFront.z));
        HandleHiding(true);
        emitterObject.GetComponent<ParticleSystem>().Play();
        StartCoroutine(UpdateCapybaraStats());
        
    }

    private IEnumerator UpdateCapybaraStats()
    {
        yield return new WaitForSeconds(Random.Range(3, 5));

        CapybaraInfo capybaraInfo = gameObject.GetComponent<CapybaraInfo>();
        capybaraInfo.hunger += amenity.hungerFill;
        capybaraInfo.comfort += amenity.comfortFill * 25;
        capybaraInfo.fun += amenity.funFill;

        yield return new WaitForSeconds(Random.Range(3, 5));
        currentState = 5;
    }

    private void ExitAmenity()
    {
        if (currentState == 5)
        {
            emitterObject.GetComponent<ParticleSystem>().Play();
            HandleHiding(false);
            StartCoroutine(AppearInFront(1));
            currentState = 6;
        }
    }

    private IEnumerator AppearInFront(int numSeconds)
    {
        yield return new WaitForSeconds(numSeconds);
        transform.position = amenityFront;
        HandleHiding(true);
        emitterObject.GetComponent<ParticleSystem>().Play();
        currentState = -1;
        amenity = null;
        capybaraRenderer = new Renderer[2];
        GetComponent<CapyAI>().CompletedAmenityInteraction();
    }

    private void CreateSmoke()
    {
        emitterObject = Instantiate(smokeParticleEmitter);
        emitterObject.transform.SetParent(transform);
        emitterObject.transform.position = transform.position;
        emitterObject.GetComponent<ParticleSystem>().Play();
    }

    private void CenterCapybara(int startState, Vector3 lookPos)
    {
        if (currentState == startState && (transform.position != centeringEndPosition))
        {
            centeringElapsedTime += Time.deltaTime;
            float percentageComplete = centeringElapsedTime / centeringDuration;

            transform.position = Vector3.Lerp(centeringStartPosition, centeringEndPosition, Mathf.SmoothStep(0, 1, percentageComplete));
        }
        else if (currentState == startState)
        {
            rotationStartPosition = transform.rotation;
            rotationEndPosition = Quaternion.LookRotation(lookPos - transform.position);
            currentState++;
        }
    }

    /*
    private void RotateCapybara(int startState)
    {
        if (currentState == startState && (rotationCompleted <= 1))
        {
            rotationElapsedTime += Time.deltaTime;
            rotationCompleted = rotationElapsedTime / rotationDuration;

            transform.rotation = Quaternion.Lerp(rotationStartPosition, rotationEndPosition, Mathf.SmoothStep(0, 1, rotationCompleted));
        }
        else if (currentState == startState)
        {
            rotationCompleted = 0;
            rotationElapsedTime = 0;
            currentState++;
        }
    }
    */

    private void RotateCapybara(int startState)
    {
        if (currentState == startState && (Mathf.Abs(rotationEndPosition.eulerAngles.y - gameObject.transform.eulerAngles.y) <= 1f))
        {
            capyAnimator.SetBool("Turning", false);
            rotationCompleted = 0;
            rotationElapsedTime = 0;
            currentState = 1;
            StartCoroutine(Wait(1));
        }
    }

    private void HandleHiding(bool hide)
    {
        for (int i = 0; i < capybaraRenderer.Length; i++)
        {
            capybaraRenderer[i].enabled = hide;
        }
    }

    private IEnumerator Wait(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        currentState = 3;
    }
}

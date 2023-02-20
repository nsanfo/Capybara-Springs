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

    void Update()
    {
        if (amenity == null) return;

        PositionToFront();
        EnterAnimation();
        HandleAmenityCentering();
    }

    public void HandleInteraction(Amenity amenity)
    {
        this.amenity = amenity;
        animationData = AmenityAnimationHandler.GetInstance().GetAnimationData(amenity.gameObject);
        if (animationData == null)
            return;

        if (GameObject.Find("TESTER") != null)
        {
            Destroy(GameObject.Find("TESTER"));
        }
        GameObject newObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        newObject.name = "TESTER";
        newObject.transform.position = amenity.PathCollider.transform.position;
        newObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        if (GameObject.Find("TARGETPOS") != null)
        {
            Destroy(GameObject.Find("TARGETPOS"));
        }
        GameObject targetObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        targetObject.name = "TARGETPOS";

        // Get area between collider and amenity
        Vector3 newPoint = Vector3.Lerp(amenity.transform.position, amenity.PathCollider.transform.position, animationData.forwardMultiplier);
        targetObject.transform.position = newPoint;
        targetObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        amenityFront = Vector3.Lerp(amenity.transform.position, amenity.PathCollider.transform.position, animationData.forwardMultiplier);
        gameObject.transform.LookAt(amenityFront);
        capyAnimator = gameObject.GetComponent<Animator>();
        capyAnimator.SetBool("Travelling", true);

        currentState = 0;
    }

    // Handles positioning the capybara in place for the animation
    private void PositionToFront()
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

    private void EnterAnimation()
    {
        if (currentState == 3)
        {
            capyAnimator.Play(animationData.animation.ToString() + "Enter");
            currentState = 4;
        }
    }

    public void EnableCentering()
    {
        centeringElapsedTime = 0;
        centeringStartPosition = transform.position;
        Vector3 amenityPos = amenity.transform.position;
        centeringEndPosition = new Vector3(amenityPos.x, amenityPos.y + animationData.enteredCenteringHeight, amenityPos.z);
        currentState = 5;
        Debug.Log("CENTERING");
    }

    private void HandleAmenityCentering()
    {
        CenterCapybara(5, amenityFront + new Vector3(0, animationData.enteredCenteringHeight, 0));
        RotateCapybara(6);

        if (currentState == 7)
        {
            currentState = 8;
            StartCoroutine(WaitInAmenity());
        }
    }

    private IEnumerator WaitInAmenity()
    {
        // Wait allotted time
        yield return new WaitForSeconds(Random.Range(3, 5));

        UpdateCapybaraInfo();

        yield return new WaitForSeconds(Random.Range(3, 5));

        capyAnimator.Play(animationData.animation.ToString() + "Exit");
        currentState = 9;
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

    private void UpdateCapybaraInfo()
    {
        CapybaraInfo capybaraInfo = gameObject.GetComponent<CapybaraInfo>();
        capybaraInfo.hunger += amenity.hungerFill;
        capybaraInfo.comfort += amenity.comfortFill;
        capybaraInfo.fun += amenity.funFill;
    }
}

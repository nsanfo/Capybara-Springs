using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FunDanceFloor : MonoBehaviour
{
    public GameObject discoBallPrefab;
    private GameObject discoBall;
    public Light splotlightPrefab;
    private Light light1, light2, light3;
    private Vector3 rotate1, rotate2, rotate3;
    public GameObject confettiPrefab;
    private GameObject confettiEmitterObject;

    private bool occupied = false, previous = false, animate = false;
    private Amenity amenity;

    private float strobeDuration = 5f;
    public Gradient gradient1, gradient2, gradient3;

    void Start()
    {
        if (discoBall == null)
        {
            discoBall = Instantiate(discoBallPrefab);
        }

        discoBall.transform.SetParent(transform);
        discoBall.transform.position = transform.position + new Vector3(0, 1, 0);

        if (confettiEmitterObject == null)
        {
            confettiEmitterObject = Instantiate(confettiPrefab);
            confettiEmitterObject.transform.SetParent(transform);
            confettiEmitterObject.transform.position = transform.position + new Vector3(0, 1.3f, 0);
        }

        amenity = GetComponent<Amenity>();

        // Initialize lights
        if (light1 == null)
        {
            light1 = Instantiate(splotlightPrefab);
            light1.GetComponent<Light>().color = Color.blue;
            light1.transform.SetParent(transform);
        }

        if (light2 == null)
        {
            light2 = Instantiate(splotlightPrefab);
            light2.GetComponent<Light>().color = Color.red;
            light2.transform.SetParent(transform);
        }

        if (light3 == null)
        {
            light3 = Instantiate(splotlightPrefab);
            light3.GetComponent<Light>().color = Color.yellow;
            light3.transform.SetParent(transform);
        }

        // Position lights
        Quaternion rot = Quaternion.AngleAxis(0, Vector3.up);
        light1.transform.position = (amenity.transform.position + rot * (Vector3.forward * 0.2f)) + new Vector3(0, 1.5f, 0);
        rotate1 = amenity.transform.position + rot * (Vector3.forward * 0.6f);

        rot = Quaternion.AngleAxis(120, Vector3.up);
        light2.transform.position = (amenity.transform.position + rot * (Vector3.forward * 0.2f)) + new Vector3(0, 1.5f, 0);
        rotate2 = amenity.transform.position + rot * (Vector3.forward * 0.6f);

        rot = Quaternion.AngleAxis(240, Vector3.up);
        light3.transform.position = (amenity.transform.position + rot * (Vector3.forward * 0.2f)) + new Vector3(0, 1.5f, 0);
        rotate3 = amenity.transform.position + rot * (Vector3.forward * 0.6f);

        light1.enabled = false;
        light2.enabled = false;
        light3.enabled = false;
    }

    void Update()
    {
        if (animate)
        {
            DiscoBallSpin();
            MoveLights();
        }

        // Update current occupied
        int currentCap = amenity.amenitySlots.Count(capy => capy != null);
        if (currentCap > 0)
        {
            occupied = true;
        }
        else if (currentCap == 0)
        {
            occupied = false;
        }

        // Check for previous
        if (occupied && !previous)
        {
            animate = true;
            light1.enabled = true;
            light2.enabled = true;
            light3.enabled = true;
            confettiEmitterObject.GetComponent<ParticleSystem>().Play();
        }
        else if (!occupied && previous)
        {
            animate = false;
            light1.enabled = false;
            light2.enabled = false;
            light3.enabled = false;
            confettiEmitterObject.GetComponent<ParticleSystem>().Stop();
        }

        // Update previous frame
        previous = occupied;
    }

    private void DiscoBallSpin()
    {
        discoBall.transform.Rotate(0, 0, 50 * Time.deltaTime);
    }

    private void MoveLights()
    {
        float t = Mathf.PingPong(Time.time / strobeDuration, 1f);
        light1.color = gradient1.Evaluate(t);
        light2.color = gradient2.Evaluate(t);
        light3.color = gradient3.Evaluate(t);
        
        light1.transform.RotateAround(rotate1, Vector3.up, 70 * Time.deltaTime);
        light2.transform.RotateAround(rotate2, Vector3.up, 90 * Time.deltaTime);
        light3.transform.RotateAround(rotate3, Vector3.up, 80 * Time.deltaTime);
    }
}

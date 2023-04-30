using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FunDanceMachine : MonoBehaviour
{
    public GameObject musicEmitterPrefab;
    private GameObject musicEmitterObject;

    private bool occupied = false, previous = false;
    private Amenity amenity;

    void Start()
    {
        if (musicEmitterObject == null)
        {
            musicEmitterObject = Instantiate(musicEmitterPrefab);
            musicEmitterObject.transform.SetParent(transform);
            musicEmitterObject.transform.position = transform.position + (Vector3.forward * 0.65f);
        }

        amenity = GetComponent<Amenity>();

        musicEmitterObject.GetComponent<ParticleSystem>().Play();
    }

    void Update()
    {
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
            musicEmitterObject.GetComponent<ParticleSystem>().Play();
        }
        else if (!occupied && previous)
        {
            musicEmitterObject.GetComponent<ParticleSystem>().Stop();
        }

        // Update previous frame
        previous = occupied;
    }
}

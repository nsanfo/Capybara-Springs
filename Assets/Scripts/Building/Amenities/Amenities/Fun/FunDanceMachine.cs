using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FunDanceMachine : MonoBehaviour
{
    public GameObject musicEmitterPrefab1, musicEmitterPrefab2, musicEmitterPrefab3;
    private GameObject musicEmitterObject1, musicEmitterObject2, musicEmitterObject3;

    private bool occupied = false, previous = false;
    private Amenity amenity;

    void Start()
    {
        if (musicEmitterObject1 == null)
        {
            musicEmitterObject1 = Instantiate(musicEmitterPrefab1);
            musicEmitterObject1.transform.SetParent(transform);
            musicEmitterObject1.transform.position = transform.position + (Vector3.forward * 0.65f);
        }

        if (musicEmitterObject2 == null)
        {
            musicEmitterObject2 = Instantiate(musicEmitterPrefab2);
            musicEmitterObject2.transform.SetParent(transform);
            musicEmitterObject2.transform.position = transform.position + (Vector3.forward * 0.65f);
        }

        if (musicEmitterObject3 == null)
        {
            musicEmitterObject3 = Instantiate(musicEmitterPrefab3);
            musicEmitterObject3.transform.SetParent(transform);
            musicEmitterObject3.transform.position = transform.position + (Vector3.forward * 0.65f);
        }

        amenity = GetComponent<Amenity>();
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
            PlayNotes();
        }
        else if (!occupied && previous)
        {
            StopNotes();
        }

        // Update previous frame
        previous = occupied;
    }

    private void PlayNotes()
    {
        musicEmitterObject1.GetComponent<ParticleSystem>().Play();
        musicEmitterObject2.GetComponent<ParticleSystem>().Play();
        musicEmitterObject3.GetComponent<ParticleSystem>().Play();
    }

    private void StopNotes()
    {
        musicEmitterObject1.GetComponent<ParticleSystem>().Stop();
        musicEmitterObject2.GetComponent<ParticleSystem>().Stop();
        musicEmitterObject3.GetComponent<ParticleSystem>().Stop();
    }
}

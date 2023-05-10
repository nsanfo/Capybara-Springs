using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunAmenity : MonoBehaviour, AmenityInterface
{
    public AmenityEnum AmenityType { get; } = AmenityEnum.Fun;
    public float insidePositioningMulti, musicHeight;
    public AudioSource currentlyPlaying;

    void Update()
    {
        if(currentlyPlaying != null && !currentlyPlaying.isPlaying)
        {
            var rand = Random.Range(0, 4);
            currentlyPlaying = transform.GetChild(1).GetChild(rand).GetComponent<AudioSource>();
            currentlyPlaying.Play();
        }
    }
}

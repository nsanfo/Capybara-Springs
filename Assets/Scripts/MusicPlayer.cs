using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    List<AudioSource> songs = new List<AudioSource>();
    List<AudioSource> unplayedSongs;
    AudioSource currentlyPlaying;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
            songs.Add(transform.GetChild(i).GetComponent<AudioSource>());
        unplayedSongs = new List<AudioSource>(songs);
        var random = Random.Range(0, unplayedSongs.Count - 1);
        unplayedSongs[random].Play();
        currentlyPlaying = unplayedSongs[random];
    }

    void LateUpdate()
    {
        if (!currentlyPlaying.isPlaying)
        {
            unplayedSongs.Remove(currentlyPlaying);
            if(unplayedSongs.Count == 0)
            {
                unplayedSongs = new List<AudioSource>(songs);
            }
            var random = Random.Range(0, unplayedSongs.Count - 1);
            unplayedSongs[random].Play();
            currentlyPlaying = unplayedSongs[random];
        }
    }
}

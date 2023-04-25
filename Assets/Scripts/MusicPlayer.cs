using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    List<AudioSource> songs = new List<AudioSource>();
    bool[] played;
    int notPlayedRemaining;
    int currentlyPlaying;

    // Start is called before the first frame update
    void Start()
    {
        played = new bool[transform.childCount];
        notPlayedRemaining = transform.childCount;
        for (int i = 0; i < transform.childCount; i++)
        {
            songs.Add(transform.GetChild(i).GetComponent<AudioSource>());
            played[i] = false;
        }
        var random = Random.Range(0, transform.childCount - 1);
        songs[random].Play();
        currentlyPlaying = random;
    }

    void LateUpdate()
    {
        if (!songs[currentlyPlaying].isPlaying)
        {
            played[currentlyPlaying] = true;
            notPlayedRemaining--;
            if(notPlayedRemaining == 0)
            {
                for (int i = 0; i < played.Length; i++)
                    played[i] = false;
                notPlayedRemaining = played.Length;
            }
            while (true)
            {
                var random = Random.Range(0, transform.childCount - 1);
                if(played[random] == false)
                {
                    songs[random].Play();
                    currentlyPlaying = random;
                }
            }
        }
    }
}

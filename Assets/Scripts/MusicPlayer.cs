using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Song
{
    readonly private AudioSource song;
    private bool played = false;

    public Song(AudioSource song)
    {
        this.song = song;
    }

    public void PlaySong()
    {
        if (played) return;
        played = true;
        song.Play();
    }

    public bool IsPlayable()
    {
        return !played;
    }

    public void Reset()
    {
        played = false;
    }

    public bool IsPlaying()
    {
        return song.isPlaying;
    }

    public void StopSong()
    {
        if (!song.isPlaying) return;
        song.Stop();
    }
}

public class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer instance;

    readonly private List<Song> songs = new();
    private int playingIndex = -1;
    private bool playing = false;
    public bool skip = false;

    void Start()
    {
        foreach (Transform child in transform)
        {
            songs.Add(new Song(child.GetComponent<AudioSource>()));
        }

        PlaySong();
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    void LateUpdate()
    {
        if (skip) Skip();

        CheckEnd();

        if (playing) return;

        PlaySong();
    }

    private void PlaySong()
    {
        List<Song> playableSongs = GetPlayableSongs();
        int playIndex = Random.Range(0, playableSongs.Count - 1);
        if (playIndex < 0) return;

        Song song = playableSongs[playIndex];
        int index = songs.IndexOf(song);
        if (index == -1) return;

        songs[index].PlaySong();
        playingIndex = index;
        playing = true;
    }

    private List<Song> GetPlayableSongs()
    {
        List<Song> playableSongs = new();
        foreach (Song song in songs)
        {
            if (song.IsPlayable())
            {
                playableSongs.Add(song);
            }
        }

        if (playableSongs.Count > 0)
        {
            return playableSongs;
        }

        // Reset all songs as playable
        foreach (Song song in songs)
        {
            if (!song.IsPlayable())
            {
                song.Reset();
            }
        }

        return new(songs);
    }

    private void Skip()
    {
        skip = false;
        playing = false;
        songs[playingIndex].StopSong();
        playingIndex = -1;
    }

    private void CheckEnd()
    {
        if (playingIndex == -1) return;

        if (songs[playingIndex].IsPlaying()) return;

        playing = false;
        playingIndex = -1;
    }
}

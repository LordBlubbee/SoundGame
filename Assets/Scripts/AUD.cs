using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AUD : MonoBehaviour
{
    // Audio object that destroys itself once completed, used for SFX!
    public AudioSource source;

    public void InitAudio(AudioClip clip)
    {
        InitAudio(clip, 1.0f, 1.0f, 0f);
    }
    public void InitAudio(AudioClip clip, float vol)
    {
        InitAudio(clip, vol, 1.0f, 0f);
    }
    public void InitAudio(AudioClip clip, float vol, float pitch, float pitchshift)
    {
        source.clip = clip;
        source.volume = vol;
        source.pitch = pitch + Random.Range(-pitchshift,pitchshift);
        source.Play();
    }
    void Update()
    {
        if (!source.isPlaying) Destroy(gameObject);
    }
}

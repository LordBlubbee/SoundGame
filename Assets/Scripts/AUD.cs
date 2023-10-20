using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AUD : MonoBehaviour
{
    // Audio object that destroys itself once completed, used for SFX!
    public AudioSource source;

    //You can use a default value instead of making two copies, with a default value in place the parameter becomes optional. Do note though, optional parameters must be after non-optional ones.
    public void InitAudio(AudioClip clip, float pitch = 1.0f, float pitchshift = 0.0f, float vol = 1.0f)
    {
        source.clip = clip;
        source.volume = vol;
        source.pitch = pitch + Random.Range(-pitchshift, pitchshift);
        source.Play();
    }

    void Update()
    {
        if (!source.isPlaying) Destroy(gameObject);
    }
}

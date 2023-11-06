using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class AudioClipType
{
    public AudioClip audioClip;
    public bool IsVoiceLine;
    public bool PlayedSequently;
}

[Serializable]
public class AudioEvent
{
    public List<AudioClipType> allAudioClips = new();

    [NonSerialized] public bool HasOccured = false;
    public int AmountOfTurnsRequiredToTrigger;

    private List<AudioSource> audioSources = new();
    private GameObject audioSourceHolder;

    public UnityEvent OnEventCompleted;

    private List<AudioClip> voiceLines = new();
    private List<AudioClipType> soundEffects = new();

    public void CheckForActivation(int currentEntityIndex, MonoBehaviour owner, GameObject audioSourceHolder)
    {
        if (HasOccured) { return; }

        if (currentEntityIndex >= AmountOfTurnsRequiredToTrigger)
        {
            this.audioSourceHolder = audioSourceHolder;
            Debug.Log("Event Start.");
            EventManager.InvokeEvent(EventType.EventStart);

            voiceLines.Clear();
            soundEffects.Clear();

            foreach (AudioClipType audioClipType in allAudioClips)
            {
                if (audioClipType.IsVoiceLine)
                {
                    voiceLines.Add(audioClipType.audioClip);
                }
                else
                {
                    soundEffects.Add(audioClipType);
                }
            }

            owner.StartCoroutine(SoundEffectsSequence());
            owner.StartCoroutine(VoiceLinesSequence());
            HasOccured = true;
        }
    }

    private AudioSource CheckForUnusedAudioSource(bool sequence)
    {
        foreach (AudioSource source in audioSources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        if (!sequence && audioSources.Count < 3)
        {
            AudioSource source = audioSourceHolder.AddComponent<AudioSource>();
            audioSources.Add(source);
            return source;
        }

        return null;
    }

    private bool IsAnAudioSourcePlaying()
    {
        foreach (AudioSource source in audioSources)
        {
            if (source.isPlaying)
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerator VoiceLinesSequence()
    {
        for (int i = 0; i < voiceLines.Count; i++)
        {
            if (voiceLines[i] == null) { continue; }

            audioSources[0].clip = voiceLines[i];
            audioSources[0].Play();

            while (audioSources[0].isPlaying)
            {
                yield return null;
            }
        }
    }

    private IEnumerator SoundEffectsSequence()
    {
        if (audioSources.Count < 1)
        {
            AudioSource source = audioSourceHolder.AddComponent<AudioSource>();
            audioSources.Add(source);
        }

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < soundEffects.Count; i++)
        {
            if (soundEffects[i] == null) { continue; }

            AudioSource source = CheckForUnusedAudioSource(soundEffects[i].PlayedSequently);
            source.volume = 0.5f;

            if (source != null)
            {
                source.clip = soundEffects[i].audioClip;
                source.Play();

                while (source.isPlaying)
                {
                    yield return null;
                }
            }

            yield return new WaitForSeconds(0.5f);
        }

        if (!IsAnAudioSourcePlaying())
        {
            foreach (AudioSource source in audioSources)
            {
                UnityEngine.Object.Destroy(source);
            }
            EventManager.InvokeEvent(EventType.EventStop);
            OnEventCompleted?.Invoke();
        }
    }
}

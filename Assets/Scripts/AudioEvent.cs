using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class AudioClipType
{
    public AudioClip audioClip;

    public bool IsVoiceLine;

    [Tooltip("Makes the audio Clip played in sequence. Voicelines are always played in sequence.")]
    public bool PlayedSequently;
}

[Serializable]
public class AudioEvent
{
    [Tooltip("It's order sensitive for the seperate clip types, voicelines share an index order with other voicelines, soundeffects share an index order with other soundeffects.")]
    public List<AudioClipType> allAudioClips = new();

    [NonSerialized] public bool HasOccured = false;

    [Tooltip("Which tile type will trigger the event upon entering.")]
    public TileType TileTypeToTriggerEvent;

    [Tooltip("Only does something if Applicable.")]
    [SerializeField] private MapType mapTypeToSwitchTo;

    private List<AudioSource> audioSources = new();
    private GameObject audioSourceHolder;

    public UnityEvent<MapType> OnEventCompleted;

    [Tooltip("The maximum amount of Audio Sources it is allowed to use, (The maximum amount of sounds played in parallel)")]
    public int MaxAmountOfAudioSources = 3;

    private List<AudioClip> voiceLines = new();
    private List<AudioClipType> soundEffects = new();

    public void CheckForActivation(MonoBehaviour owner, GameObject audioSourceHolder, Tile currentTile)
    {
        if (HasOccured) { return; }

        if (currentTile.Type == TileTypeToTriggerEvent)
        {
            TriggerAudioEvent(owner, audioSourceHolder);
        }
    }

    public void TriggerAudioEvent(MonoBehaviour owner, GameObject audioSourceHolder)
    {
        if (HasOccured) { return; }

        audioSources.Clear();

        this.audioSourceHolder = audioSourceHolder;
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

    private AudioSource CheckForUnusedAudioSource(bool sequence)
    {
        foreach (AudioSource source in audioSources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        if (!sequence && audioSources.Count <= MaxAmountOfAudioSources)
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

            if (source != null)
            {
                source.clip = soundEffects[i].audioClip;
                source.Play();

                while (source.isPlaying)
                {
                    yield return null;
                }
            }
        }

        if (!IsAnAudioSourcePlaying())
        {
            Debug.Log("Event Ended.");
            foreach (AudioSource source in audioSources)
            {
                UnityEngine.Object.Destroy(source);
            }
            OnEventCompleted?.Invoke(mapTypeToSwitchTo);
            EventManager.InvokeEvent(EventType.EventStop);
        }
    }
}
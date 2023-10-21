using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public class SoundObject
{
    public Direction Direction;
    public TileType Type;
    public List<AudioClip> AudioClipsType = new();
    public List<AudioClip> AudioClipsDirection = new();

    public bool HasOtherEntity = false;

    public AudioClip AudioClipType => Type switch
    {
        TileType.Standard => AudioClipsType[0],
        TileType.House => AudioClipsType[1],
        TileType.Woods => AudioClipsType[2],
        TileType.River => AudioClipsType[3],
        _ => throw new NotImplementedException()
    };

    public AudioClip AudioClipDirection => Direction switch
    {
        Direction.Up => AudioClipsDirection[0],
        Direction.Down => AudioClipsDirection[1],
        Direction.Left => AudioClipsDirection[2],
        Direction.Right => AudioClipsDirection[3],
        _ => throw new NotImplementedException()
    };
}

public class AudioManager : MonoBehaviour
{
    public AudioSource AudioClipPlayer;
    public AudioSource OST;

    [Tooltip("Typing is based on Index; Index 0 is standard, 1 House, 2 Woods, 3 River")]
    [SerializeField] private List<AudioClip> AudioClipsType = new();
    [Tooltip("Typing is based on Index; Index 0 is Up, 1 Down, 2 Left, 3 Right")]
    [SerializeField] private List<AudioClip> AudioClipsDirection = new();
    [Tooltip("Defines which audio clip is played when the tile has an entity. Index 0 is entity, 1 is no entity.")]
    [SerializeField] private List<AudioClip> EntityOrNotAudioClips = new();

    private List<SoundObject> soundObjects = new();
    private List<AudioClip> currentAudioClips = new();

    private void Start()
    {
        OST.loop = true;
        OST.Play();
    }

    public void OnMovement(List<SoundObject> soundObjects)
    {
        this.soundObjects.Clear();
        this.soundObjects.AddRange(soundObjects);

        foreach (SoundObject soundObject in this.soundObjects)
        {
            soundObject.AudioClipsDirection = AudioClipsDirection;
            soundObject.AudioClipsType = AudioClipsType;
        }

        GetAudioClips();
    }

    public void PlayAudio()
    {
        StartCoroutine(nameof(PlayAudioSequentially));
    }

    public void GetAudioClips()
    {
        currentAudioClips.Clear();

        foreach (SoundObject soundObject in soundObjects)
        {
            if (soundObject.AudioClipType == null) { continue; }
            currentAudioClips.Add(soundObject.AudioClipType);

            if (soundObject.AudioClipDirection == null) { continue; }
            currentAudioClips.Add(soundObject.AudioClipDirection);

            if (EntityOrNotAudioClips.Count < 2) { continue; }
            currentAudioClips.Add(soundObject.HasOtherEntity ? EntityOrNotAudioClips[0] : EntityOrNotAudioClips[1]);
        }

        PlayAudio();
    }

    IEnumerator PlayAudioSequentially()
    {
        yield return null;

        for (int i = 0; i < currentAudioClips.Count; i++)
        {
            if (currentAudioClips[i] == null) { continue; }

            AudioClipPlayer.clip = currentAudioClips[i];

            AudioClipPlayer.Play();

            while (AudioClipPlayer.isPlaying)
            {
                yield return null;
            }
        }
    }
}
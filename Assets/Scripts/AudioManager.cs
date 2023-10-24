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
        TileType.Swamp => AudioClipsType[0],
        TileType.River => AudioClipsType[1],
        TileType.Plains => AudioClipsType[2],
        TileType.House => AudioClipsType[3],
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

    [Tooltip("Typing is based on Index; Index 0 is Swamp, 1 River, 2 Plains, 3 House")]
    [SerializeField] private List<AudioClip> audioClipsType = new();

    [Tooltip("Typing is based on Index; Index 0 is Swamp, 1 River, 2 Plains, 3 House")]
    [SerializeField] private List<AudioClip> audioClipsTypeEnter = new();

    [Tooltip("The amount of time waited untill the next voiceline will be played, in seconds.")]
    [SerializeField] private float amountOfDelayBetweenVoicelines = 1.0f;

    [SerializeField] private List<AudioClip> entityAudioClips = new();
    [SerializeField] private List<AudioClip> noEntityAudioClips = new();

    private List<AudioClip> currentAudioClips = new();

    [Tooltip("Index 0 = Up, 1 = Down, 2 = Left, 3 = Right")]
    [SerializeField] private List<AudioClip> audioClipsDirection = new();

    [SerializeField] private List<AudioClip> copyThat = new();
    [SerializeField] private List<AudioClip> completedAudioClips = new();

    [SerializeField] private AudioClip tutorialClip;
    private bool gameStarted = false;
    private bool gamePaused = false;

    private void OnEnable()
    {
        EventManager.AddListener(EventType.UnPause, () => gamePaused = false);
        EventManager.AddListener(EventType.Pause, () => gamePaused = true);
        EventManager.AddListener(EventType.StartGame, () => gameStarted = true);
    }

    public void OnMovement(List<SoundObject> soundObjects, Tile tile)
    {
        if (!gameStarted || gamePaused) { return; }
        EventManager.InvokeEvent(EventType.Pause);
        StartCoroutine(StartAudioSequence(soundObjects, tile));
    }

    private void Start()
    {
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        AudioClipPlayer.clip = tutorialClip;
        AudioClipPlayer.Play();

        while (AudioClipPlayer.isPlaying)
        {
            yield return null;
        }

        OST.loop = true;
        OST.Play();
        EventManager.InvokeEvent(EventType.StartGame);
    }

    private IEnumerator StartAudioSequence(List<SoundObject> _soundObjects, Tile tile)
    {
        AudioClipPlayer.clip = copyThat[UnityEngine.Random.Range(0, copyThat.Count)];
        AudioClipPlayer.Play();

        while (AudioClipPlayer.isPlaying) { yield return new WaitForSeconds(amountOfDelayBetweenVoicelines); }

        AudioClipPlayer.clip = audioClipsTypeEnter[(int)tile.Type];
        AudioClipPlayer.Play();

        while (AudioClipPlayer.isPlaying) { yield return new WaitForSeconds(amountOfDelayBetweenVoicelines); }

        foreach (SoundObject soundObject in _soundObjects)
        {
            soundObject.AudioClipsDirection = audioClipsDirection;
            soundObject.AudioClipsType = audioClipsType;
        }

        GetAudioClips(_soundObjects);

        for (int i = 0; i < currentAudioClips.Count; i++)
        {
            if (currentAudioClips[i] == null) { continue; }

            AudioClipPlayer.clip = currentAudioClips[i];

            AudioClipPlayer.Play();

            while (AudioClipPlayer.isPlaying)
            {
                yield return new WaitForSeconds(amountOfDelayBetweenVoicelines);
            }
        }

        AudioClipPlayer.clip = completedAudioClips[UnityEngine.Random.Range(0, completedAudioClips.Count)];
        while (AudioClipPlayer.isPlaying)
        {
            yield return new WaitForSeconds(amountOfDelayBetweenVoicelines);
        }

        EventManager.InvokeEvent(EventType.UnPause);
    }

    private void GetAudioClips(List<SoundObject> soundObjects)
    {
        currentAudioClips.Clear();

        foreach (SoundObject soundObject in soundObjects)
        {
            if (soundObject.AudioClipDirection == null) { continue; }
            currentAudioClips.Add(soundObject.AudioClipDirection);

            if (soundObject.AudioClipType == null) { continue; }
            currentAudioClips.Add(soundObject.AudioClipType);

            if (noEntityAudioClips.Count < 1 || entityAudioClips.Count < 1) { continue; }
            currentAudioClips.Add(soundObject.HasOtherEntity ? entityAudioClips[UnityEngine.Random.Range(0, entityAudioClips.Count)] : noEntityAudioClips[UnityEngine.Random.Range(0, entityAudioClips.Count)]);
        }
    }
}
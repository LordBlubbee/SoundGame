using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor.XR;
using UnityEngine;
using UnityEngine.Events;

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

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
            EventManager.InvokeEvent(EventType.EventStop);
            OnEventCompleted?.Invoke();
        }
    }
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

    [Tooltip("Convenient for testing.")]
    [SerializeField] private bool skipTutorial = false;

    [Tooltip("Typing is based on Index; Index 0 is Swamp, 1 River, 2 Plains, 3 House")]
    [SerializeField] private List<AudioClip> audioClipsType = new();

    [Tooltip("Typing is based on Index; Index 0 is Swamp, 1 River, 2 Plains, 3 House")]
    [SerializeField] private List<AudioClip> audioClipsTypeEnter = new();

    [Tooltip("The amount of time waited untill the next voiceline will be played, in seconds.")]
    [SerializeField] private float amountOfDelayBetweenVoicelines = 1.0f;

    [SerializeField] private List<AudioClip> entityAudioClips = new();
    [SerializeField] private List<AudioClip> noEntityAudioClips = new();

    private readonly List<AudioClip> currentAudioClips = new();

    [Tooltip("Index 0 = Up, 1 = Down, 2 = Left, 3 = Right")]
    [SerializeField] private List<AudioClip> audioClipsDirection = new();

    [SerializeField] private List<AudioClip> copyThat = new();
    [SerializeField] private List<AudioClip> completedAudioClips = new();

    [SerializeField] private AudioClip tutorialClip;
    private bool gameStarted = false;
    private bool gamePaused = false;
    private bool eventRunning = false;

    [SerializeField] private bool unlockedRadar = false;

    private List<SoundObject> soundObjects = new();
    private Tile currentTile;

    private Player player;

    [SerializeField] private List<AudioEvent> audioEvents = new();

    private void CheckForAudioEvents(Entity currentEntity)
    {
        if (!currentEntity.isPlayer) { return; }

        foreach (AudioEvent audioEvent in audioEvents)
        {
            audioEvent.CheckForActivation(currentEntity.TurnIndex, this, gameObject);
        }
    }

    public void SetPlayer(Player _player)
    {
        player = _player;
    }

    private void StartEvent()
    {
        EventManager.InvokeEvent(EventType.Pause);
        eventRunning = true;
        StopAllCoroutines();
    }

    private void EndEvent()
    {
        eventRunning = false;
        StartCoroutine(StartAudioSequence(soundObjects, currentTile, true));
    }

    private void OnEnable()
    {
        EventManager.AddListener(EventType.UnPause, () => gamePaused = false);
        EventManager.AddListener(EventType.Pause, () => gamePaused = true);
        EventManager.AddListener(EventType.StartGame, () => gameStarted = true);
        EventManager.AddListener(EventType.EventStart, StartEvent);
        EventManager.AddListener(EventType.EventStop, EndEvent);
        EventManager.AddListener(EventType.UnlockRadar, () => unlockedRadar = true);
    }

    public void OnMovement(List<SoundObject> soundObjects, Tile tile, bool player)
    {
        if (!gameStarted || gamePaused || !player) { return; }

        EventManager.InvokeEvent(EventType.Pause);
        this.soundObjects = soundObjects;
        currentTile = tile;
        StartCoroutine(StartAudioSequence(soundObjects, tile));
    }

    private void Start()
    {
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        if (!skipTutorial)
        {
            AudioClipPlayer.clip = tutorialClip;
            AudioClipPlayer.Play();

            while (AudioClipPlayer.isPlaying)
            {
                yield return null;
            }
        }

        OST.loop = true;
        OST.Play();
        EventManager.InvokeEvent(EventType.StartGame);
    }

    private IEnumerator StartAudioSequence(List<SoundObject> _soundObjects, Tile tile, bool skipEnter = false)
    {
        if (!skipEnter)
        {
            AudioClipPlayer.clip = copyThat[UnityEngine.Random.Range(0, copyThat.Count)];
            AudioClipPlayer.Play();

            while (AudioClipPlayer.isPlaying) { yield return new WaitForSeconds(amountOfDelayBetweenVoicelines); }

            CheckForAudioEvents(player);
            while (eventRunning)
            {
                yield return null;
            }
        }

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
                yield return default;
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

            if (noEntityAudioClips.Count < 1 || entityAudioClips.Count < 1 || !unlockedRadar) { continue; }
            currentAudioClips.Add(soundObject.HasOtherEntity ? entityAudioClips[UnityEngine.Random.Range(0, entityAudioClips.Count)] : noEntityAudioClips[UnityEngine.Random.Range(0, entityAudioClips.Count)]);
        }
    }
}
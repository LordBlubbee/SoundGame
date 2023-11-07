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

public class AudioManager : MonoBehaviour
{
    public AudioSource AudioClipPlayer;
    public AudioSource OST;

    [Tooltip("Convenient for testing.")]
    [SerializeField] private bool skipTutorial = false;

    [Tooltip("Typing is based on Index; Index 0 is Swamp, 1 River, 2 Plains, 3 House, 4 = Mine, 5 = Tree, 6 = Shack")]
    [SerializeField] private List<AudioClip> audioClipsType = new();

    [Tooltip("Typing is based on Index; Index 0 is Swamp, 1 River, 2 Plains, 3 House, 4 Mine, 5 Body, 6 Tree, 7 Shack, 8 Artifact")]
    [SerializeField] private List<AudioClip> audioClipsTypeEnter = new();

    [Tooltip("The amount of time waited untill the next voiceline will be played, in seconds.")]
    [SerializeField] private float amountOfDelayBetweenVoicelines = 1.0f;

    [Tooltip("The Audio Clip to play if there is an entity in that tile.")]
    [SerializeField] private AudioClip entityAudioClips;
    [Tooltip("The Audio Clip to play if there are no entities in that tile.")]
    [SerializeField] private AudioClip nonHostileEntityAudioClip;

    private readonly List<AudioClip> currentAudioClips = new();

    [Tooltip("Index 0 = Up, 1 = Down, 2 = Left, 3 = Right")]
    [SerializeField] private List<AudioClip> audioClipsDirection = new();

    [Tooltip("Called at the start of a movement sequence.")]
    [SerializeField] private List<AudioClip> copyThat = new();

    [Tooltip("Called once the movement sequence has finished.")]
    [SerializeField] private List<AudioClip> completedAudioClips = new();

    [SerializeField] private AudioClip tutorialClip;

    [SerializeField] private AudioClip hasVisitedHouseOrTree;

    private bool gameStarted = false;
    private bool gamePaused = false;
    private bool eventRunning = false;

    private bool unlockedRadar = false;

    private List<SoundObject> soundObjects = new();
    private Tile currentTile;

    private Player player;

    [SerializeField] private List<AudioEvent> audioEvents = new();

    private void CheckForAudioEvents(Entity currentEntity, Tile currentTile)
    {
        if (!currentEntity.IsPlayer) { return; }

        foreach (AudioEvent audioEvent in audioEvents)
        {
            audioEvent.CheckForActivation(this, gameObject, currentTile);
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
        EventManager.AddListener(EventType.SwapMap, () => StopAllCoroutines());
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
            AudioClipPlayer.clip = copyThat[Random.Range(0, copyThat.Count)];
            AudioClipPlayer.Play();

            while (AudioClipPlayer.isPlaying) { yield return new WaitForSeconds(amountOfDelayBetweenVoicelines); }

            CheckForAudioEvents(player, tile);
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

        AudioClipPlayer.clip = completedAudioClips[Random.Range(0, completedAudioClips.Count)];
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

            if ((soundObject.Type == TileType.House || soundObject.Type == TileType.Tree) && player.HasVisitedHouseOrTree)
            {
                currentAudioClips.Add(hasVisitedHouseOrTree);
            }
            else
            {
                currentAudioClips.Add(soundObject.AudioClipType);
            }

            if (!unlockedRadar) { continue; }

            if (soundObject.HasOtherEntity || soundObject.HostileEntity)
            {
                currentAudioClips.Add(soundObject.HostileEntity ? entityAudioClips : nonHostileEntityAudioClip);
            }
        }
    }
}
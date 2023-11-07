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

    [Tooltip("Typing is based on Index; Index 0 is Swamp, 1 River, 2 Plains, 3 House, 4 = Mine, 5 = Tree, 6 = Shack, 7 = AlienShip")]
    [SerializeField] private List<AudioClip> audioClipsType = new();

    [Tooltip("Typing is based on Index; Index 0 is Swamp, 1 River, 2 Plains, 3 House, 4 Mine, 5 Body, 6 Tree, 7 Shack, 8 Artifact, 9 Alienship")]
    [SerializeField] private List<AudioClip> audioClipsTypeEnter = new();

    [Tooltip("Typing is based on Index; Index 0 is House, 1 Body, 2 Tree, 3 Shack, 4 Artifact.")]
    [SerializeField] private List<AudioClip> audioClipsTypeEnterFamiliar = new();

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

    [Tooltip("Index 0 = hide at house, 1 hide at shack.")]
    [SerializeField] private AudioClip[] hideAudioClips = new AudioClip[2];

    [SerializeField] private AudioClip tutorialClip;

    [Tooltip("Index Sensitive, 0 is attack 1, 1 attack 2, 2 attack 3.The on completed gets filled automatically and can be ignored, unless further function is desired. Tile to Trigger can also be ignored. Trigger for it can be found in the code, under Attack()")]
    [SerializeField] private List<AudioEvent> AttackEvents;

    [SerializeField] private AudioClip hasVisitedHouseOrTree;

    [Tooltip("Index Sensitive. 0 and 1 are handled by a seperate event. index 0 progress 2 skip 1 progress 2 regular,")]
    [SerializeField] private List<AudioEvent> SpaceShipEvents = new();

    private bool shipEncounter = false;
    private bool gameStarted = false;
    private bool gamePaused = false;
    private bool eventRunning = false;

    private bool unlockedRadar = false;

    private GameManager gameManager;

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
        EventManager.AddListener(EventType.SwapMap, StopAllCoroutines);
        EventManager.AddListener(EventType.Attack, CheckAttack);
        EventManager.AddListener(EventType.ShipEncounter, () => shipEncounter = true);
    }

    public void OnMovement(List<SoundObject> soundObjects, Tile tile, bool player)
    {
        if (!gameStarted || gamePaused || !player) { return; }

        EventManager.InvokeEvent(EventType.Pause);
        this.soundObjects = soundObjects;
        currentTile = tile;
        StartCoroutine(StartAudioSequence(soundObjects, tile));
    }

    private void CheckForProgressAdvancement()
    {

    }

    private void Start()
    {
        player = FindObjectOfType<Player>();

        gameManager = FindObjectOfType<GameManager>();

        foreach (AudioEvent audioEvent in AttackEvents)
        {
            audioEvent.OnEventCompleted.AddListener(Attack);
        }

        StartCoroutine(StartGame());
    }

    private void CheckAttack()
    {
        switch (player.Health)
        {
            case 3:
                {
                    AttackEvents[0].TriggerAudioEvent(this, gameObject);
                    break;
                }
            case 2:
                {
                    AttackEvents[1].TriggerAudioEvent(this, gameObject);
                    break;
                }
            case 1:
                {
                    AttackEvents[2].TriggerAudioEvent(this, gameObject);
                    break;
                }
        }
    }

    private void Attack(MapType mapType)
    {
        if (player.Health == 3)
        {
            if (player.Position.x < gameManager.MapSize.x)
            {
                player.Position += new Vector2Int(1, 0);
            }
            player.Health -= 1;
        }
        else if (player.Health == 2)
        {
            if (player.Position.y > 0)
            {
                player.Position += new Vector2Int(0, -1);
            }
            player.Health -= 1;
        }
        else if (player.Health == 1)
        {
            player.Health -= 1;
        }
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

        if ((tile.Type == TileType.House || tile.Type == TileType.Tree) && player.HasVisitedHouseOrTree)
        {
            AudioClipPlayer.clip = hasVisitedHouseOrTree;
        }
        else
        {
            switch (tile.Type)
            {
                case TileType.House:
                    {
                        AudioClipPlayer.clip = audioClipsTypeEnterFamiliar[0];
                        break;
                    }
                case TileType.Body:
                    {
                        AudioClipPlayer.clip = audioClipsTypeEnterFamiliar[1];
                        break;
                    }
                case TileType.Tree:
                    {
                        AudioClipPlayer.clip = audioClipsTypeEnterFamiliar[2];
                        break;
                    }
                case TileType.Shack:
                    {
                        AudioClipPlayer.clip = audioClipsTypeEnterFamiliar[3];
                        break;
                    }
                case TileType.Artifact:
                    {
                        AudioClipPlayer.clip = audioClipsTypeEnterFamiliar[4];
                        break;
                    }
                default:
                    {
                        AudioClipPlayer.clip = audioClipsTypeEnter[(int)tile.Type];
                        break;
                    }
            }
        }

        AudioClipPlayer.Play();

        while (AudioClipPlayer.isPlaying) { yield return new WaitForSeconds(amountOfDelayBetweenVoicelines); }

        foreach (SoundObject soundObject in _soundObjects)
        {
            soundObject.AudioClipsDirection = audioClipsDirection;
            soundObject.AudioClipsType = audioClipsType;
            soundObject.AudioClipsTypeFamiliar = audioClipsTypeEnterFamiliar;
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

            currentAudioClips.Add(soundObject.AudioClipType);

            if (soundObject.Type == TileType.House && soundObject.HostileEntity)
            {
                currentAudioClips.Add(hideAudioClips[0]);
                continue;
            }
            if (soundObject.Type == TileType.Shack && soundObject.HostileEntity)
            {
                currentAudioClips.Add(hideAudioClips[1]);
                continue;
            }

            if (!unlockedRadar) { continue; }

            if (soundObject.HasOtherEntity || soundObject.HostileEntity)
            {
                currentAudioClips.Add(soundObject.HostileEntity ? entityAudioClips : nonHostileEntityAudioClip);
            }
        }
    }
}
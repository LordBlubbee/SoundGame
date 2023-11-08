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

    [Tooltip("Typing is based on Index; Index 0 is Swamp, 1 River, 2 Plains, 3 House, 4 Mine, 5 Body, 6 Tree, 7 Shack, 8 Artifact")]
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

    [Tooltip("Index Sensitive, enabling the voiceline option makes it unable to end, reason of which unsure as of now. But having it function as a sound effect works fine.")]
    [SerializeField] private List<AudioEvent> AttackEvents;

    [SerializeField] private AudioClip hasVisitedHouseOrTree;

    [Tooltip("Index Sensitive. 0 and 1 are handled by a seperate event. index 0 progress 2 skip 1 progress 2 regular,")]
    [SerializeField] private List<AudioEvent> SpaceShipEvents = new();

    private bool gameStarted = false;
    private bool gamePaused = false;
    private bool eventRunning = false;

    private bool unlockedRadar = false;

    private bool spaceShipEvent = false;
    private bool reloadSoundObjects = false;

    private GameManager gameManager;

    //It'll do for now.
    private Enemy enemy;

    private int progressIndex = 0;

    private List<SoundObject> soundObjects = new();
    private Tile currentTile;

    private Player player;

    private bool attack = false;
    private bool hasSwappedMap = false;

    private Coroutine currentCoroutine;

    [SerializeField] private List<AudioEvent> audioEvents = new();

    private IEnumerator CheckForAudioEvents(Entity currentEntity, Tile currentTile)
    {
        if (currentEntity.IsPlayer)
        {
            foreach (AudioEvent audioEvent in audioEvents)
            {
                while (eventRunning)
                {
                    yield return null;
                }
                audioEvent.CheckForActivation(player.TurnIndex, this, gameObject, currentTile);
            }
        }
    }

    private void StartEvent()
    {
        EventManager.InvokeEvent(EventType.Pause);
        eventRunning = true;
        StopCoroutine(currentCoroutine);
    }

    private void EndEvent()
    {
        eventRunning = false;
        if (eventRunning) { return; }
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
        EventManager.AddListener(EventType.SwapMap, OnMapSwap);
        EventManager.AddListener(EventType.Attack, () => attack = true);
        EventManager.AddListener(EventType.ShipProgressTrigger, CheckForProgressAdvancement);
        EventManager.AddListener(EventType.Attack, CheckEnemyAttack);
        EventManager.AddListener(EventType.ShipEncounter, () => spaceShipEvent = true);
        EventManager.AddListener(EventType.ReloadSoundObjects, () => reloadSoundObjects = true);
    }

    private void OnMapSwap()
    {
        Debug.Log("Map Swapped.");
        hasSwappedMap = true;
        StopCoroutine(currentCoroutine);
        StopAllCoroutines();
        EventManager.InvokeEvent(EventType.ResetPlayer);
        EventManager.InvokeEvent(EventType.StartGame);
    }

    public void OnMovement(List<SoundObject> soundObjects, Tile tile, bool player)
    {
        if (!gameStarted || gamePaused || !player) { return; }

        EventManager.InvokeEvent(EventType.Pause);
        this.soundObjects = soundObjects;
        currentTile = tile;
        currentCoroutine = StartCoroutine(StartAudioSequence(soundObjects, tile));
    }

    private void CheckForProgressAdvancement()
    {
        switch (progressIndex)
        {
            //2 skip
            case 0:
                {
                    Debug.Log("Progress 1 skip");
                    ocurredCheck[0] = true;
                    SpaceShipEvents[progressIndex].TriggerAudioEvent(this, gameObject);
                    EventManager.InvokeEvent(EventType.SkipEnemyTurn);
                    break;
                }
            //2 regular
            case 1:
                {
                    Debug.Log("Progress 1");
                    ocurredCheck[0] = true;
                    SpaceShipEvents[progressIndex].TriggerAudioEvent(this, gameObject);
                    break;
                }
            //3 skip
            case 2:
                {
                    Debug.Log("Progress 2 skip");
                    ocurredCheck[1] = true;
                    SpaceShipEvents[progressIndex].TriggerAudioEvent(this, gameObject);
                    EventManager.InvokeEvent(EventType.SkipEnemyTurn);
                    break;
                }
            //3 regular
            case 3:
                {
                    Debug.Log("Progress 2 ");
                    ocurredCheck[1] = true;
                    SpaceShipEvents[progressIndex].TriggerAudioEvent(this, gameObject);
                    break;
                }
            //4 regular
            case 4:
                {
                    Debug.Log("Progress 3 ");
                    ocurredCheck[2] = true;
                    SpaceShipEvents[progressIndex].TriggerAudioEvent(this, gameObject);
                    break;
                }
            //Ending
            case 5:
                {
                    Debug.Log("Progress 4 ");
                    ocurredCheck[3] = true;
                    SpaceShipEvents[progressIndex].TriggerAudioEvent(this, gameObject);
                    EventManager.InvokeEvent(EventType.Pause);
                    StopAllCoroutines();
                    break;
                }
        }
    }

    private bool[] ocurredCheck = new bool[4];

    public void AdvanceProgress()
    {
        Debug.Log(progressIndex);
        if (ocurredCheck[0] == false)
        {
            if (player.HasVisitedHouseOrTree)
            {
                progressIndex = 0;
            }
            else
            {
                progressIndex = 1;
            }
        }
        else if (ocurredCheck[1] == false)
        {
            if (player.HasArtifact)
            {
                progressIndex = 2;
            }
            else
            {
                progressIndex = 3;
            }
        }
        else if (ocurredCheck[2] == false)
        {
            progressIndex = 4;
        }
        else if (ocurredCheck[3] == false)
        {
            progressIndex = 5;
        }
        Debug.Log(progressIndex);
    }

    private void Start()
    {
        enemy = FindObjectOfType<Enemy>();
        player = FindObjectOfType<Player>();

        gameManager = FindObjectOfType<GameManager>();

        StartCoroutine(StartGame());
    }

    private void CheckEnemyAttack()
    {
        if (player.CurrentTurn) { return; }

        CheckAttack();
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

    public void Attack()
    {
        Debug.Log(player.Health);
        attack = false;
        switch (player.Health)
        {
            case 3:
                {
                    if (player.Position.x < gameManager.MapSize.x)
                    {
                        player.Position += new Vector2Int(1, 0);
                    }
                    player.Health -= 1;
                    break;
                }
            case 2:
                {
                    if (player.Position.y > 0)
                    {
                        player.Position += new Vector2Int(0, -1);
                    }
                    player.Health -= 1;
                    break;
                }
            case 1:
                {
                    player.Health -= 1;
                    break;
                }
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
            if (!hasSwappedMap)
            {
                AudioClipPlayer.clip = copyThat[Random.Range(0, copyThat.Count)];
                AudioClipPlayer.Play();

                while (AudioClipPlayer.isPlaying) { yield return new WaitForSeconds(amountOfDelayBetweenVoicelines); }
            }
            hasSwappedMap = false;

            if (attack)
            {
                CheckAttack();
            }
            else
            {
                StartCoroutine(CheckForAudioEvents(player, tile));
            }

            while (eventRunning)
            {
                yield return null;
            }

            if (spaceShipEvent)
            {
                Debug.Log("SpaceShip Event.");
                CheckForProgressAdvancement();
            }

            while (eventRunning)
            {
                yield return null;
            }
        }

        if (spaceShipEvent)
        {
            enemy.SetToRandomNeighbourOfPlayer();
        }

        if (reloadSoundObjects)
        {
            Debug.Log("Reloaded Sound Objects");
            reloadSoundObjects = false;
            _soundObjects = gameManager.GridManager.ReturnNeighbourSoundObjects(player.Position);
        }

        if (tile.Type != TileType.Mine)
        {
            if (tile.Type != TileType.AlienShip)
            {
                if ((tile.Type == TileType.House || tile.Type == TileType.Tree) && player.HasVisitedHouseOrTree)
                {
                    AudioClipPlayer.clip = hasVisitedHouseOrTree;
                }
                else
                {
                    if (tile.Visited)
                    {
                        Debug.Log(tile.Visited);
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
                    else
                    {
                        AudioClipPlayer.clip = audioClipsTypeEnter[(int)tile.Type];
                        AudioClipPlayer.Play();
                    }
                }
            }

            while (AudioClipPlayer.isPlaying) { yield return new WaitForSeconds(amountOfDelayBetweenVoicelines); }

            foreach (SoundObject soundObject in _soundObjects)
            {
                soundObject.AudioClipsDirection = audioClipsDirection;
                soundObject.AudioClipsType = audioClipsType;
                soundObject.AudioClipsTypeFamiliar = audioClipsTypeEnterFamiliar;
            }

            Debug.Log("Retrieving Audio Clips.");
            GetAudioClips(_soundObjects);

            Debug.Log("Playing Audio Clips.");
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

        }

        if (tile.Type == TileType.House || tile.Type == TileType.Tree)
        {
            player.HasVisitedHouseOrTree = true;
        }

        if (tile.Type == TileType.Artifact)
        {
            player.HasArtifact = true;
        }

        tile.EntitiesInTile.Remove(player);
        tile.Visited = true;
        EventManager.InvokeEvent(EventType.UnPause);
    }

    private void GetAudioClips(List<SoundObject> soundObjects)
    {
        currentAudioClips.Clear();

        foreach (SoundObject soundObject in soundObjects)
        {
            if (soundObject.AudioClipDirection == null) { continue; }

            if (!soundObject.Tile.Visited || soundObject.Tile.EntitiesInTile.Count >= 1)
            {
                currentAudioClips.Add(soundObject.AudioClipDirection);
            }

            if (soundObject.AudioClipType == null) { continue; }

            if (!soundObject.Tile.Visited)
            {
                currentAudioClips.Add(soundObject.AudioClipType);
            }

            if (soundObject.Type == TileType.House && soundObject.Tile.EntitiesInTile.Count >= 1)
            {
                currentAudioClips.Add(hideAudioClips[0]);
                continue;
            }
            if (soundObject.Type == TileType.Shack && soundObject.Tile.EntitiesInTile.Count >= 1)
            {
                currentAudioClips.Add(hideAudioClips[1]);
                continue;
            }

            if (!unlockedRadar) { continue; }

            if (soundObject.HasOtherEntity || soundObject.Tile.EntitiesInTile.Count >= 1)
            {
                currentAudioClips.Add(soundObject.Tile.EntitiesInTile.Count >= 1 ? entityAudioClips : nonHostileEntityAudioClip);
            }
        }
    }
}
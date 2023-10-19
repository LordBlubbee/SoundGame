using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CO : MonoBehaviour
{
    // Game controller that handles basically everything!
    public AudioSource VoicelinePlayer;
    public AudioSource OST;
    public AUD spawnSFX;
    protected bool playerTurn = false; //Can player input to move in a certain direction?
    private Vector2 playerPosition = Vector2.zero; //Player position
    private List<GridObject> gridObjects = new List<GridObject>(); //Stores all objects!
    private int minX = -3; //These four constant variables are the borders of the map
    private int maxX = 3;
    private int minY = 0;
    private int maxY = 5;
    private int TurnNumber = 0;
    private List<AudioClip> Voicelines = new List<AudioClip>();
    private List<AudioClip> AddDirections = new List<AudioClip>();

    private void Start()
    {
        //Initiate all objects
        gridObjects.Add(new GridObject(0,new Vector2(0,3),3)); //The House
        //
        PerformAction(Vector2.zero);
    }

    IEnumerator WaitOnPlayerTurn()
    {
        TurnNumber++;
        playerTurn = true;
        while (playerTurn)
        {
            if (Input.GetKeyUp(KeyCode.W)) PerformAction(new Vector2(0, 1));
            else if (Input.GetKeyUp(KeyCode.S)) PerformAction(new Vector2(0, -1));
            else if(Input.GetKeyUp(KeyCode.A)) PerformAction(new Vector2(-1, 0));
            else if(Input.GetKeyUp(KeyCode.D)) PerformAction(new Vector2(1, 0));
            yield return null;
        }
    }
    private void PerformAction(Vector2 Movement)
    {
        playerTurn = false;
        playerPosition += Movement;
        Voicelines = new List<AudioClip>(); //Reset voicelines

        resolveTurn(); //Might add voicelines
        
        //Iterate through all objects
        for (int i = 0; i < gridObjects.Count; i++)
        {
            GridObject ob = gridObjects[i];
            if (scanObject(ob.Coords,ob.DetectabilityRange))
            {
                //Add object ID voice line here
                foreach (AudioClip clip in AddDirections) Voicelines.Add(clip);
            }
        }
        //Then, put sounds in a row
        StartCoroutine(PlayingVoice());
    }

    private void resolveTurn()
    {
        //Specific objects can, for example, move every turn
        switch (TurnNumber)
        {
            //Events that happen during the game
            case 0:
                //Tutorial first turn
                break;
        }
    }
    private bool scanObject(Vector2 coord, int detect)
    {
        //Try to spot an object and then add applicable sounds
        AddDirections = new List<AudioClip>();
        //NORTH sounds
        //WEST sounds
        //EAST sounds
        //SOUTH sounds
        //All in different lists
        for (int i = 0; i < detect; i++) //NORTH
        {
            if (coord == playerPosition + new Vector2(0, i + 1))
            {
                //Play Sound Here
                return true;
            }
        }
        for (int i = 0; i < detect; i++) //SOUTH
        {
            if (coord == playerPosition + new Vector2(0, -i - 1))
            {
                //Play Sound Here
                return true;
            }
        }
        for (int i = 0; i < detect; i++) //EAST
        {
            if (coord == playerPosition + new Vector2(i+1,0))
            {
                //Play Sound Here
                return true;
            }
        }
        for (int i = 0; i < detect; i++) //WEST
        {
            if (coord == playerPosition + new Vector2(-i-1,0))
            {
                //Play Sound Here
                return true;
            }
        }
        return false;
    }

    IEnumerator PlayingVoice()
    {
        int Sounds = Voicelines.Count;
        while (Sounds > 0)
        {
            VoicelinePlayer.clip = Voicelines[0];
            VoicelinePlayer.Play();
            while (VoicelinePlayer.isPlaying) yield return new WaitForSeconds(0.5f);
            Voicelines.RemoveAt(0);
        }
        StartCoroutine(WaitOnPlayerTurn()); //After voice report, players can make their next move
    }
    private bool canPlayerContinue()
    {
        return playerTurn;
    }
    //SFX Audio Players
    private void playSFX(AudioClip clip)
    {
        Instantiate(spawnSFX).InitAudio(clip);
    }
    private void playSFX(AudioClip clip, float vol)
    {
        Instantiate(spawnSFX).InitAudio(clip, vol);
    }
    private void playSFX(AudioClip clip, float vol, float pitch, float pitchshift)
    {
        Instantiate(spawnSFX).InitAudio(clip,vol,pitch, pitchshift);
    }
}

public class GridObject {
    public int ID;
    public int DetectabilityRange;
    public Vector2 Coords;
    public GridObject(int id, Vector2 pos, int detect)
    {
        ID = id;
        Coords = pos;
        DetectabilityRange = detect;
    }
}

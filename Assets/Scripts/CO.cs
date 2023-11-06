using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CO : MonoBehaviour
{
    public AUD spawnSFX;

    protected bool playerTurn = false; //Can player input to move in a certain direction?
    private Vector2 playerPosition = Vector2.zero; //Player position

    //private int TurnIndex = 0;
    private List<AudioClip> AddDirections = new();

    //private void Start()
    //{
    //    //Initiate all objects

    //    //
    //    //PerformAction(Vector2.zero);
    //}

    private void WaitOnPlayerTurn()
    {
        //TurnIndex++;
        //playerTurn = true;

        //if (playerTurn)
        //{
        //    if (Input.GetKeyUp(KeyCode.W)) PerformAction(new Vector2(0, 1));
        //    if (Input.GetKeyUp(KeyCode.S)) PerformAction(new Vector2(0, -1));
        //    if (Input.GetKeyUp(KeyCode.A)) PerformAction(new Vector2(-1, 0));
        //    if (Input.GetKeyUp(KeyCode.D)) PerformAction(new Vector2(1, 0));
        //}
    }

    private void resolveTurn()
    {
        //Specific objects can, for example, move every turn
        //switch (TurnIndex)
        //{
        //    //Events that happen during the game
        //    case 0:
        //        //Tutorial first turn
        //        break;
        //}
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
            if (coord == playerPosition + new Vector2(0, -i - 1))
            {
                //Play Sound Here
                return true;
            }
            if (coord == playerPosition + new Vector2(i + 1, 0))
            {
                //Play Sound Here
                return true;
            }
            if (coord == playerPosition + new Vector2(-i - 1, 0))
            {
                //Play Sound Here
                return true;
            }
        }
        return false;
    }

    //IEnumerator PlayingVoice()
    //{
    //    int Sounds = Voicelines.Count;
    //    while (Sounds > 0)
    //    {
    //        VoicelinePlayer.clip = Voicelines[0];
    //        VoicelinePlayer.Play();
    //        while (VoicelinePlayer.isPlaying) yield return new WaitForSeconds(0.5f);
    //        Voicelines.RemoveAt(0);
    //    }

    //    WaitOnPlayerTurn(); //After voice report, players can make their next move
    //}

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
        Instantiate(spawnSFX).InitAudio(clip, vol, pitch, pitchshift);
    }
}

public class GridObject
{
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

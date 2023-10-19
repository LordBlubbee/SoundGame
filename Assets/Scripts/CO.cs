using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CO : MonoBehaviour
{
    // Game controller that handles basically everything!
    public AudioSource VoiceLines;
    public AudioSource OST;
    public AUD spawnSFX;
    protected bool playerTurn = false; //Can player input to move in a certain direction?
    private Vector3 playerPosition = Vector3.zero; //Player position
    private List<Vector3> gridObjectLocations = new List<Vector3>(); //Stores locations of all objects
    private List<int> gridObjectTypes = new List<int>(); //Stores types of all objects using numerical ID
    private int minX = -3; //These four variables are the borders of the map
    private int maxX = 3;
    private int minY = 0;
    private int maxY = 5;

    private void Start()
    {
        //Initiate all objects
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

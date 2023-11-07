using System;
using System.Collections.Generic;
using UnityEngine;

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
        TileType.Mine => AudioClipsType[4],
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

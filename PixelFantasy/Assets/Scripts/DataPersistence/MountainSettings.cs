using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Mountain Settings", menuName = "Settings/Mountain Settings")]
public class MountainSettings : ResourceSettings
{
    // Settings
    [SerializeField] protected TileBase _ruleTile;
    [SerializeField] protected MountainTileType _mountainTileType;
        
    // Accessors
    public MountainTileType MountainTileType => _mountainTileType;
    public TileBase RuleTile => _ruleTile;
}

public enum MountainTileType
{
    Empty,
    Stone,
    Copper,
    Coal,
    Tin,
    Iron,
    Gold,
}

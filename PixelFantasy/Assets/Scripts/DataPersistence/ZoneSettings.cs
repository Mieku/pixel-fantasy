using System.Collections;
using System.Collections.Generic;
using Systems.Zones.Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;

//[CreateAssetMenu(fileName = "Zone Settings", menuName = "Settings/Zone Settings")]
public class ZoneSettings : ScriptableObject
{
    public TileBase DefaultTiles;
    public TileBase SelectedTiles;
    public EZoneType ZoneType;
    public Color ZoneColour;
    public ZoneCell CellPrefab;

    public enum EZoneType
    {
        Stockpile,
        Farm,
    }
}

using Databrain;
using Databrain.Attributes;
using Systems.Zones.Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Data.Zones
{
    public class ZoneSettings : DataObject
    {
        public TileBase DefaultTiles;
        public TileBase SelectedTiles;
        public EZoneType ZoneType;
        public Color ZoneColour;
        public ZoneCell CellPrefab;
    }

    public enum EZoneType
    {
        Stockpile,
        Farm,
    }
}

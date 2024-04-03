using Databrain;
using Databrain.Attributes;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Data.Zones
{
    // [UseOdinInspector]
    public class ZoneSettings : DataObject
    {
        public TileBase DefaultTiles;
        public TileBase SelectedTiles;
        public EZoneType ZoneType;
        public Color ZoneColour;
    }

    public enum EZoneType
    {
        Stockpile,
        Farm,
    }
}

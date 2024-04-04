using Databrain.Attributes;
using Systems.Zones.Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Data.Zones
{
    public class FarmingZoneData : ZoneData
    {
        [ExposeToInspector, DatabrainSerialize]
        public FarmingZoneSettings Settings;
        
        public void InitData(FarmingZoneSettings settings)
        {
            Settings = settings;
            IsEnabled = true;
        }

        public override Color ZoneColour => Settings.ZoneColour;
        public override TileBase DefaultTiles => Settings.DefaultTiles;
        public override TileBase SelectedTiles => Settings.SelectedTiles;
        public override EZoneType ZoneType => EZoneType.Farm;
    }
}

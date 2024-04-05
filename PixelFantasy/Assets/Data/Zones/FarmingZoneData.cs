using Databrain.Attributes;
using Systems.Zones.Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Data.Zones
{
    public class FarmingZoneData : ZoneData
    {
        [ExposeToInspector, DatabrainSerialize]
        public FarmingZoneSettings FarmingSettings;
        
        public void InitData(FarmingZoneSettings settings)
        {
            FarmingSettings = settings;
            IsEnabled = true;
        }
        
        public void CopyData(FarmingZoneData dataToCopy)
        {
            FarmingSettings = dataToCopy.FarmingSettings;
            IsEnabled = dataToCopy.IsEnabled;
        }

        public override Color ZoneColour => Settings.ZoneColour;
        public override TileBase DefaultTiles => Settings.DefaultTiles;
        public override TileBase SelectedTiles => Settings.SelectedTiles;
        public override EZoneType ZoneType => EZoneType.Farm;
        public override ZoneSettings Settings => FarmingSettings;
    }
}

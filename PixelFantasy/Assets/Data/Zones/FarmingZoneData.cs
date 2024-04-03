using Databrain.Attributes;
using UnityEngine;

namespace Data.Zones
{
    public class FarmingZoneData : ZoneData
    {
        [ExposeToInspector, DatabrainSerialize]
        public FarmingZoneSettings Settings;
        
        public void InitData(FarmingZoneSettings settings)
        {
            Settings = settings;
        }
    }
}

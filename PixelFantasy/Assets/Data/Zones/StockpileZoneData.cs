using System.Collections.Generic;
using Data.Item;
using Databrain.Attributes;
using UnityEngine;

namespace Data.Zones
{
    public class StockpileZoneData : ZoneData
    {
        [ExposeToInspector, DatabrainSerialize]
        public StockpileZoneSettings Settings;
        
        // What is stored? and Where? Maybe make a storage cell type
        
        
        // Specific storage player settings chosen for area 
        [ExposeToInspector, DatabrainSerialize]
        public StoragePlayerSettings PlayerSettings;


        public void InitData(StockpileZoneSettings settings)
        {
            Settings = settings;
            PlayerSettings = settings.DefaultPlayerSettings;

            ZoneName = $"Stockpile {AssignedLayer}";
        }
    }
}

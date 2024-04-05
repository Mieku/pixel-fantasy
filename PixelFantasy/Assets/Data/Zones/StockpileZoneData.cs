using System.Collections.Generic;
using Data.Item;
using Databrain.Attributes;
using Systems.Zones.Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Data.Zones
{
    public class StockpileZoneData : ZoneData
    {
        [ExposeToInspector, DatabrainSerialize]
        public StockpileZoneSettings StockpileSettings;
        
        // What is stored? and Where? Maybe make a storage cell type
        
        
        // Specific storage player settings chosen for area 
        [ExposeToInspector, DatabrainSerialize]
        public StoragePlayerSettings PlayerSettings;


        public void InitData(StockpileZoneSettings settings)
        {
            StockpileSettings = settings;
            PlayerSettings = new StoragePlayerSettings();
            PlayerSettings.PasteSettings(settings.DefaultPlayerSettings);
            IsEnabled = true;

            ZoneName = $"Stockpile {AssignedLayer}";
        }

        public void CopyData(StockpileZoneData dataToCopy)
        {
            StockpileSettings = dataToCopy.StockpileSettings;
            PlayerSettings = new StoragePlayerSettings();
            PlayerSettings.PasteSettings(dataToCopy.PlayerSettings);
            IsEnabled = dataToCopy.IsEnabled;
            
            ZoneName = $"Stockpile {AssignedLayer}";
        }
        
        public override Color ZoneColour => Settings.ZoneColour;
        public override TileBase DefaultTiles => Settings.DefaultTiles;
        public override TileBase SelectedTiles => Settings.SelectedTiles;
        public override EZoneType ZoneType => EZoneType.Stockpile;
        public override ZoneSettings Settings => StockpileSettings;
    }
}

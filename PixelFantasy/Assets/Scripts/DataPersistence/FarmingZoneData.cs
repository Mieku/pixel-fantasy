using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FarmingZoneData : ZoneData
{
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
    public override ZoneSettings.EZoneType ZoneType => ZoneSettings.EZoneType.Farm;
    public override ZoneSettings Settings => FarmingSettings;
}

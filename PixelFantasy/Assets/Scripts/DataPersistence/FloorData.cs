
using Newtonsoft.Json;
using ScriptableObjects;
using UnityEngine.Tilemaps;

public class FloorData : ConstructionData
{
    public string FloorStyleTileBaseID;

    [JsonIgnore] public FloorSettings FloorSettings => (FloorSettings) GameSettings.Instance.LoadConstructionSettings(SettingsID);
    [JsonIgnore] public TileBase FloorStyleTileBase => GameSettings.Instance.LoadTileBase(FloorStyleTileBaseID);

    public void AssignFloorSettings(FloorSettings settings, FloorStyle selectedStyle)
    {
        SettingsID = settings.name;
        FloorStyleTileBaseID = selectedStyle.Tiles.name;
        MaxDurability = settings.MaxDurability;
            
        InitData();
    }
}
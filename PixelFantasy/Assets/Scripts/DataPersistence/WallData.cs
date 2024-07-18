using Newtonsoft.Json;
using ScriptableObjects;

public class WallData : ConstructionData
{
    public string InteriorColourID;

    [JsonIgnore] public WallSettings SelectedWallOption => (WallSettings) GameSettings.Instance.LoadConstructionSettings(SettingsID);
    [JsonIgnore] public DyeSettings InteriorColour => GameSettings.Instance.LoadDyeSettings(InteriorColourID);
        
    public void AssignWallOption(WallSettings option, DyeSettings dye)
    {
        SettingsID = option.name;
        InteriorColourID = dye.name;
        MaxDurability = option.MaxDurability;
            
        InitData();
    }
}

using Newtonsoft.Json;
using ScriptableObjects;

public class DoorData : ConstructionData
{
    public string MatColourID;
    public bool IsVertical;
    
    [JsonIgnore] public DoorSettings DoorSettings => (DoorSettings) GameSettings.Instance.LoadConstructionSettings(SettingsID);
    [JsonIgnore] public DyeSettings MatColour => GameSettings.Instance.LoadDyeSettings(MatColourID);
        
    public void AssignDoorSettings(DoorSettings settings, DyeSettings matColour)
    {
        SettingsID = settings.name;
        MaxDurability = settings.MaxDurability;

        if (matColour != null)
        {
            MatColourID = matColour.name;
        }
            
        InitData();
    }
}
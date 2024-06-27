
public class DoorData : ConstructionData
{
    public DoorSettings DoorSettings;
    public DyeSettings MatColour;
        
    public void AssignDoorSettings(DoorSettings settings, DyeSettings matColour)
    {
        DoorSettings = settings;
        CraftRequirements = settings.CraftRequirements;
        MaxDurability = settings.MaxDurability;
        MatColour = matColour;
            
        InitData();
    }
}
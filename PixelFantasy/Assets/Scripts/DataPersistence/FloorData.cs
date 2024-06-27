
public class FloorData : ConstructionData
{
    public FloorSettings FloorSettings;
    public FloorStyle FloorStyle;


    public void AssignFloorSettings(FloorSettings settings, FloorStyle selectedStyle)
    {
        FloorSettings = settings;
        FloorStyle = selectedStyle;
        MaxDurability = settings.MaxDurability;
        CraftRequirements = settings.CraftRequirements;
            
        InitData();
    }
}
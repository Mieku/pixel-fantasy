

public class WallData : ConstructionData
{
    public WallSettings SelectedWallOption;
    public DyeSettings InteriorColour;
        
    public void AssignWallOption(WallSettings option, DyeSettings dye)
    {
        SelectedWallOption = option;
        CraftRequirements = option.CraftRequirements;
        MaxDurability = option.MaxDurability;
        InteriorColour = dye;
            
        InitData();
    }
}
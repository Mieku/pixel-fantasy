using Data.Dye;
using Databrain.Attributes;

namespace Data.Structure
{
    [DataObjectAddToRuntimeLibrary]
    public class WallData : ConstructionData
    {
        [ExposeToInspector, DatabrainSerialize] 
        public WallSettings SelectedWallOption;
        
        [ExposeToInspector, DatabrainSerialize] 
        public DyeData InteriorColour;
        
        public void AssignWallOption(WallSettings option, DyeData dye)
        {
            SelectedWallOption = option;
            CraftRequirements = option.CraftRequirements;
            MaxDurability = option.MaxDurability;
            InteriorColour = dye;
            
            InitData();
        }
    }
}

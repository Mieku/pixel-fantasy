using Data.Dye;
using Databrain;
using Databrain.Attributes;

namespace Data.Structure
{
    public class DoorData : ConstructionData
    {
        [ExposeToInspector, DatabrainSerialize] 
        public DoorSettings DoorSettings;

        [ExposeToInspector, DatabrainSerialize] 
        public DyeData DoorColour;
        
        [ExposeToInspector, DatabrainSerialize] 
        public DyeData MatColour;
        
        public void AssignDoorSettings(DoorSettings settings, DyeData doorColour, DyeData matColour)
        {
            DoorSettings = settings;
            CraftRequirements = settings.CraftRequirements;
            MaxDurability = settings.MaxDurability;
            DoorColour = doorColour;
            MatColour = matColour;
            
            InitData();
        }
    }
}

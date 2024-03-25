using Databrain;
using Databrain.Attributes;
using UnityEngine;

namespace Data.Structure
{
    public class FloorData : ConstructionData
    {
        [ExposeToInspector, DatabrainSerialize] 
        public FloorSettings FloorSettings;
        
        [ExposeToInspector, DatabrainSerialize] 
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
}

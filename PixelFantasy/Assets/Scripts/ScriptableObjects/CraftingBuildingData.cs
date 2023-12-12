using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "CraftingBuildingData", menuName = "Buildings/CraftingBuildingData", order = 1)]
    public class CraftingBuildingData : BuildingData
    {
        public JobData WorkersJob;
        public List<CraftedItemData> CraftingOptions = new List<CraftedItemData>();
        
        
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ProductionBuildingData", menuName = "Buildings/ProductionBuildingData", order = 1)]
    public class ProductionBuildingData : BuildingData
    {
        public JobData WorkersJob;
        public List<CraftedItemData> ProductionOptions = new List<CraftedItemData>();
    }
}

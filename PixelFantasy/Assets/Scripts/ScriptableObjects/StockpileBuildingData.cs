using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "StockpileBuildingData", menuName = "Buildings/StockpileBuildingData", order = 1)]
    public class StockpileBuildingData : BuildingData
    {
        public JobData WorkersJob;
        
    }
}

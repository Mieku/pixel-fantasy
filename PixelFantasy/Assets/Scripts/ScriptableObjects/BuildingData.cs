using System.Collections.Generic;
using Buildings;
using Buildings.Building_Panels;
using Characters;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "BuildingData", menuName = "Buildings/BuildingData", order = 1)]
    public class BuildingData : ConstructionData
    {
        public int MaxOccupants;
        public int DailyUpkeep;
        public List<AbilityType> RelevantAbilityTypes = new List<AbilityType>();
        public List<InventoryLogisticBill> DefaultLogistics = new List<InventoryLogisticBill>();
    }
}

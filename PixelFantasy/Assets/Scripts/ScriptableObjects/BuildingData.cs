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
        public BuildingType BuildingType;
        public int MaxOccupants;
        public List<FurnitureItemData> AllowedFurniture = new List<FurnitureItemData>();
        public Building LinkedBuilding;

        public bool IsFurnitureAllowed(FurnitureItemData furnitureItemData)
        {
            return AllowedFurniture.Contains(furnitureItemData);
        }
    }

    public enum BuildingType
    {
        Home,
        Production,
    }
}

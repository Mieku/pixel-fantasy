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
        //public GameObject Exterior; // TODO: Make this capable of having multiple options
        //public Interior Interior;
        public BuildingType BuildingType;
        public List<BuildingOld> BuildingOptions;
        [FormerlySerializedAs("BuildingPanel")] public BuildingPanelOld buildingPanelOld;
        public int MaxOccupants;
    }

    public enum BuildingType
    {
        Home,
        Production,
    }
}

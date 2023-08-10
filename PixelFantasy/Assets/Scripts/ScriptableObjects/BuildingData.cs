using System.Collections.Generic;
using Buildings;
using Buildings.Building_Panels;
using Characters;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "BuildingData", menuName = "CraftedData/BuildingData", order = 1)]
    public class BuildingData : ConstructionData
    {
        //public GameObject Exterior; // TODO: Make this capable of having multiple options
        //public Interior Interior;
        public List<BuildingOld> BuildingOptions;
        public BuildingPanel BuildingPanel;
        public int MaxOccupants;
    }
}

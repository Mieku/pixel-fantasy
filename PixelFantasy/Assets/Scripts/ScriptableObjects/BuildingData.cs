using Buildings.Building_Panels;
using Gods;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "BuildingData", menuName = "CraftedData/BuildingData", order = 1)]
    public class BuildingData : ConstructionData
    {
        public GameObject Exterior; // TODO: Make this capable of having multiple options
        public Interior Interior;
        public BuildingPanel BuildingPanel;
        public int MaxOccupants;
    }
}

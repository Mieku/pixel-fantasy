using System.Collections.Generic;
using Buildings;
using UnityEngine;

namespace Managers
{
    public class BuildingsManager : Singleton<BuildingsManager>
    {
        private List<Building> _allBuildings = new List<Building>();
        public List<Building> AllBuildings => _allBuildings;
        
        public void RegisterBuilding(Building building)
        {
            if (_allBuildings.Contains(building))
            {
                Debug.LogError("Tried to register the same Building Twice: " + building.BuildingID);
                return;
            }
            
            _allBuildings.Add(building);
        }

        public void DeregisterBuilding(Building building)
        {
            if (!_allBuildings.Contains(building))
            {
                Debug.LogError("Tried to deregister a non-registered Building: " + building.BuildingID);
                return;
            }

            _allBuildings.Remove(building);
        }
        
        public Building GetBuilding(string uniqueID)
        {
            return _allBuildings.Find(building => building.UniqueId == uniqueID);
        }
        
        public void SelectBuilding(string uniqueID)
        {
            var building = GetBuilding(uniqueID);
            if (building != null)
            {
                building.GetClickObject().TriggerSelected(true);
            }
        }
    }
}

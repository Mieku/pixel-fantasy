using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using Characters;
using UnityEngine;
using UnityEngine.AI;

namespace Managers
{
    public class BuildingsManager : Singleton<BuildingsManager>
    {
        private List<Building> _allBuildings = new List<Building>();
        public List<Building> AllBuildings => _allBuildings;

        public bool ShowInteriorByDefault;

        protected override void Awake()
        {
            base.Awake();
            GameEvents.OnHideRoofsToggled += GameEvents_ShowInteriorByDefault;
        }

        private void OnDestroy()
        {
            GameEvents.OnHideRoofsToggled -= GameEvents_ShowInteriorByDefault;
        }

        private void GameEvents_ShowInteriorByDefault(bool showInterior)
        {
            ShowInteriorByDefault = showInterior;
        }

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

        public List<HouseholdBuilding> AllHouseholds => _allBuildings.OfType<HouseholdBuilding>().ToList();

        public void ClaimEmptyHome(Unit requestingUnit)
        {
            var emptyHouses = AllHouseholds.FindAll(house => house.IsVacant);
            if (emptyHouses.Count == 0) return;
            
            List<(HouseholdBuilding, float)> houseDistances = new List<(HouseholdBuilding, float)>();
            foreach (var emptyHouse in emptyHouses)
            {
                NavMeshPath path = new NavMeshPath();
                if (NavMesh.CalculatePath(emptyHouse.ConstructionStandPosition(),
                        requestingUnit.transform.position, NavMesh.AllAreas, path))
                {
                    float distance = Helper.GetPathLength(path);
                    houseDistances.Add((emptyHouse, distance));
                }
            }

            var sortedHouses = houseDistances.OrderBy(x => x.Item2).Select(x => x.Item1).ToList();
            var selectedHouse = sortedHouses[0];
            selectedHouse.AssignHeadHousehold(requestingUnit);
        }
    }
}

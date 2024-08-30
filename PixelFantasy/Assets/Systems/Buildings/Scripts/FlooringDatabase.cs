using System.Collections.Generic;
using System.Linq;
using AI;
using Managers;
using Sirenix.OdinInspector;
using Systems.Details.Build_Details.Scripts;
using Systems.Floors.Scripts;

namespace Systems.Buildings.Scripts
{
    public class FlooringDatabase : Singleton<FlooringDatabase>
    {
        public FloorBuilder FloorBuilder;
        
        [ShowInInspector] private Dictionary<string, Floor> _registeredFloors = new Dictionary<string, Floor>();

        public void RegisterFloor(Floor floor)
        {
            _registeredFloors.Add(floor.UniqueID, floor);
            PlayerInteractableDatabase.Instance.RegisterPlayerInteractable(floor);
        }

        public void DeregisterFloor(Floor floor)
        {
            floor.CancelRequesterTasks(false);
            _registeredFloors.Remove(floor.UniqueID);
            PlayerInteractableDatabase.Instance.DeregisterPlayerInteractable(floor);
        }

        public Floor Query(string uniqueID)
        {
            return _registeredFloors[uniqueID];
        }

        public Dictionary<string, FloorData> SaveFloorData()
        {
            Dictionary<string, FloorData> results = new Dictionary<string, FloorData>();
            foreach (var kvp in _registeredFloors)
            {
                results.Add(kvp.Key, kvp.Value.RuntimeFloorData);
            }
            
            return results;
        }

        public void LoadFloorData(Dictionary<string, FloorData> loadedData)
        {
            foreach (var data in loadedData)
            {
                FloorBuilder.SpawnLoadedFloor(data.Value);
            }
        }

        public void ClearAllFloors()
        {
            var floors = _registeredFloors.ToList();
            foreach (var floor in floors)
            {
                floor.Value.DeleteFloor();
            }
            _registeredFloors.Clear();
        }
    }
}

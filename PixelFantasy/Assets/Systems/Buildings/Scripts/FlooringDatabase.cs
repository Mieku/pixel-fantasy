using System.Collections.Generic;
using System.Linq;
using Managers;
using Systems.Details.Build_Details.Scripts;
using Systems.Floors.Scripts;

namespace Systems.Buildings.Scripts
{
    public class FlooringDatabase : Singleton<FlooringDatabase>
    {
        public FloorBuilder FloorBuilder;
        
        private List<Floor> _registeredFloors = new List<Floor>();

        public void RegisterFloor(Floor floor)
        {
            _registeredFloors.Add(floor);
        }

        public void DeregisterFloor(Floor floor)
        {
            _registeredFloors.Remove(floor);
        }

        public List<FloorData> GetFloorData()
        {
            List<FloorData> results = new List<FloorData>();
            foreach (var floor in _registeredFloors)
            {
                results.Add(floor.RuntimeFloorData);
            }
            
            return results;
        }

        public void LoadFloorData(List<FloorData> loadedData)
        {
            foreach (var data in loadedData)
            {
                FloorBuilder.SpawnLoadedFloor(data);
            }
        }

        public void ClearAllFloors()
        {
            var floors = _registeredFloors.ToList();
            foreach (var floor in floors)
            {
                floor.DeleteFloor();
            }
            _registeredFloors.Clear();
        }
    }
}

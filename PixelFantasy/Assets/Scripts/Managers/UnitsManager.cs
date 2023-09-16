using System.Collections.Generic;
using System.Linq;
using Characters;
using ScriptableObjects;
using UnityEngine;

namespace Managers
{
    public class UnitsManager : Singleton<UnitsManager>
    {
        // TODO: Instead use a registering system
        private List<Unit> _allKinlings = new List<Unit>();

        public List<Unit> AllKinlings => _allKinlings;
        public List<Unit> UnemployedKinlings => _allKinlings.Where(kinling => kinling.GetUnitState().AssignedWorkplace == null).ToList();
        public List<Unit> HomelessKinlings => _allKinlings.Where(kinling => kinling.GetUnitState().AssignedHome == null).ToList();
        
        public void RegisterKinling(Unit kinling)
        {
            if (_allKinlings.Contains(kinling))
            {
                Debug.LogError("Tried to register the same Kinling Twice: " + kinling.GetUnitState().FullName);
                return;
            }
            
            _allKinlings.Add(kinling);
        }

        public void DeregisterKinling(Unit kinling)
        {
            if (!_allKinlings.Contains(kinling))
            {
                Debug.LogError("Tried to deregister a non-registered Kinling: " + kinling.GetUnitState().FullName);
                return;
            }

            _allKinlings.Remove(kinling);
        }

        
        
        
        
        public List<UnitState> AllUnits => GetComponentsInChildren<UnitState>().ToList();
        
        public UnitState GetUnit(string fullname)
        {
            return AllUnits.Find(unit => unit.FullName == fullname);
        }
    }
}

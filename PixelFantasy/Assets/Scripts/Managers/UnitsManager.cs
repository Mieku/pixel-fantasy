using System.Collections.Generic;
using System.Linq;
using Characters;
using ScriptableObjects;
using UnityEngine;

namespace Managers
{
    public class UnitsManager : Singleton<UnitsManager>
    {
        private List<Unit> _allKinlings = new List<Unit>();

        public List<Unit> AllKinlings => _allKinlings;
        public List<Unit> UnemployedKinlings => _allKinlings.Where(kinling => kinling.GetUnitState().AssignedWorkplace == null).ToList();
        public List<Unit> HomelessKinlings => _allKinlings.Where(kinling => kinling.GetUnitState().AssignedHome == null).ToList();

        public bool AnyUnitHaveJob(JobData jobData)
        {
            foreach (var kinling in _allKinlings)
            {
                if (kinling.GetUnitState().CurrentJob.JobData == jobData)
                {
                    return true;
                }
            }

            return false;
        }
        
        public List<Unit> GetAllUnitsInRadius(Vector2 startPoint, float radius)
        {
            return _allKinlings.Where(kinling => 
                Vector2.Distance(startPoint, kinling.transform.position) <= radius
            ).ToList();
        }
        
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
        
        public Unit GetUnit(string uniqueID)
        {
            return AllKinlings.Find(unit => unit.UniqueId == uniqueID);
        }
    }
}

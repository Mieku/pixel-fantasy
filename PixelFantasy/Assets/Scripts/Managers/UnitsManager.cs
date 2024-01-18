using System.Collections.Generic;
using System.Linq;
using Characters;
using QFSW.QC;
using ScriptableObjects;
using Systems.Notifications.Scripts;
using TaskSystem;
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
                if (kinling.GetUnitState().CurrentJob == jobData)
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

        public void SelectKinling(string uniqueID)
        {
            var kinling = GetUnit(uniqueID);
            if (kinling != null)
            {
                kinling.ClickObject.TriggerSelected(true);
            }
        }

        [Command("set_love")]
        private void CMD_SetLove(string instigatorFirstName, string receiverFirstName)
        {
            Unit instigator = _allKinlings.Find(unit => unit.GetUnitState().FirstName == instigatorFirstName);
            Unit receiver = _allKinlings.Find(unit => unit.GetUnitState().FirstName == receiverFirstName);

            if (instigator == null)
            {
                Debug.LogError($"Can't find Instigator: {instigatorFirstName}");
                return;
            }
            
            if (receiver == null)
            {
                Debug.LogError($"Can't find Receiver: {receiverFirstName}");
                return;
            }

            instigator.Partner = receiver;
            receiver.Partner = instigator;
            NotificationManager.Instance.CreateKinlingLog(instigator, $"{instigator.GetUnitState().FullName} is now in a relationship with {receiver.GetUnitState().FullName}!", LogData.ELogType.Positive);
        }

        [Command("mate")]
        private void CMD_Mate(string instigatorFirstName, string receiverFirstName)
        {
            Unit instigator = _allKinlings.Find(unit => unit.GetUnitState().FirstName == instigatorFirstName);
            Unit receiver = _allKinlings.Find(unit => unit.GetUnitState().FirstName == receiverFirstName);
            
            if (instigator == null)
            {
                Debug.LogError($"Can't find Instigator: {instigatorFirstName}");
                return;
            }
            
            if (receiver == null)
            {
                Debug.LogError($"Can't find Receiver: {receiverFirstName}");
                return;
            }

            Task task = new Task("Mate", instigator.AssignedBed, null, EToolType.None);
            instigator.TaskAI.QueueTask(task);
        }

        public Unit CreateChild(Unit mother, Unit father)
        {
            KinlingData childData = new KinlingData(mother.GetKinlingData(), father.GetKinlingData());
            var child = Spawner.Instance.SpawnKinling(childData, mother.transform.position, true);
            
            mother.Children.Add(child);
            father.Children.Add(child);

            var home = mother.GetUnitState().AssignedHome;
            if (home == null)
            {
                Debug.LogError($"Child was born without home??");
            }

            home.AssignChild(child);
            
            return child;
        }
    }
}

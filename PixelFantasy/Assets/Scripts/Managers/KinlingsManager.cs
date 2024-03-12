using System.Collections.Generic;
using System.Linq;
using Characters;
using Data.Item;
using QFSW.QC;
using ScriptableObjects;
using Systems.Notifications.Scripts;
using Systems.Skills.Scripts;
using TaskSystem;
using UnityEngine;

namespace Managers
{
    public class KinlingsManager : Singleton<KinlingsManager>
    {
        private List<Kinling> _allKinlings = new List<Kinling>();

        public List<Kinling> AllKinlings => _allKinlings;
        
        public List<Kinling> GetAllUnitsInRadius(Vector2 startPoint, float radius)
        {
            return _allKinlings.Where(kinling => 
                Vector2.Distance(startPoint, kinling.transform.position) <= radius
            ).ToList();
        }
        
        public void RegisterKinling(Kinling kinling)
        {
            if (_allKinlings.Contains(kinling))
            {
                Debug.LogError("Tried to register the same Kinling Twice: " + kinling.FullName);
                return;
            }
            
            _allKinlings.Add(kinling);
            
        }

        public void DeregisterKinling(Kinling kinling)
        {
            if (!_allKinlings.Contains(kinling))
            {
                Debug.LogError("Tried to deregister a non-registered Kinling: " + kinling.FullName);
                return;
            }

            _allKinlings.Remove(kinling);
        }
        
        public Kinling GetUnit(string uniqueID)
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
            Kinling instigator = _allKinlings.Find(unit => unit.FirstName == instigatorFirstName);
            Kinling receiver = _allKinlings.Find(unit => unit.FirstName == receiverFirstName);

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
            NotificationManager.Instance.CreateKinlingLog(instigator, $"{instigator.FullName} is now in a relationship with {receiver.FullName}!", LogData.ELogType.Positive);
        }

        [Command("mate")]
        private void CMD_Mate(string instigatorFirstName, string receiverFirstName)
        {
            Kinling instigator = _allKinlings.Find(unit => unit.FirstName == instigatorFirstName);
            Kinling receiver = _allKinlings.Find(unit => unit.FirstName == receiverFirstName);
            
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

            Task task = new Task("Mate", ETaskType.Personal, instigator.AssignedBed, EToolType.None);
            instigator.TaskAI.QueueTask(task);
        }

        public Kinling CreateChild(Kinling mother, Kinling father)
        {
            KinlingData childData = new KinlingData(mother.GetKinlingData(), father.GetKinlingData());
            var child = Spawner.Instance.SpawnKinling(childData, mother.transform.position, true);
            
            mother.Children.Add(child);
            father.Children.Add(child);
            
            return child;
        }
    }
}

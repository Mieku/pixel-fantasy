using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters;
using QFSW.QC;
using Systems.Appearance.Scripts;
using TaskSystem;
using UnityEngine;
using KinlingSelector = Systems.Kinling_Selector.Scripts.KinlingSelector;

namespace Managers
{
    public class KinlingsDatabase : Singleton<KinlingsDatabase>
    {
        private List<KinlingData> _registeredKinlingsData = new List<KinlingData>();

        public List<KinlingData> GetKinlingsData()
        {
            return _registeredKinlingsData;
        }

        public List<KinlingData> SaveKinlingsData()
        {
            foreach (var kinlingData in _registeredKinlingsData)
            {
                kinlingData.GetKinling()?.TaskHandler.SaveBBState();
            }

            return _registeredKinlingsData;
        }

        public void LoadKinlingsData(List<KinlingData> data)
        {
            foreach (var kinlingData in data)
            {
                SpawnKinling(kinlingData, kinlingData.Position);
            }
        }
        
        public List<KinlingData> GetAllUnitsInRadius(Vector2 startPoint, float radius)
        {
            return _registeredKinlingsData.Where(kinling => 
                Vector2.Distance(startPoint, kinling.Position) <= radius
            ).ToList();
        }
        
        public void RegisterKinling(KinlingData kinling)
        {
            if (_registeredKinlingsData.Contains(kinling))
            {
                Debug.LogError("Tried to register the same Kinling Twice: " + kinling.Fullname);
                return;
            }
            _registeredKinlingsData.Add(kinling);
            
            KinlingSelector.Instance.AddKinling(kinling);
        }

        public void DeregisterKinling(KinlingData kinling)
        {
            if (!_registeredKinlingsData.Contains(kinling))
            {
                Debug.LogError("Tried to deregister a non-registered Kinling: " + kinling.Fullname);
                return;
            }

            _registeredKinlingsData.Remove(kinling);
            
            KinlingSelector.Instance.RemoveKinling(kinling);
        }
        
        public Kinling GetKinling(string uniqueID)
        {
            var allKinlings = GetComponentsInChildren<Kinling>();
            foreach (var kinling in allKinlings)
            {
                if (kinling.RuntimeData.UniqueID == uniqueID)
                {
                    return kinling;
                }
            }

            return null;
        }

        public KinlingData GetKinlingData(string uniqueID)
        {
            return _registeredKinlingsData.Find(kinling => kinling.UniqueID == uniqueID);
        }

        public void SelectKinling(string uniqueID)
        {
            var kinling = GetKinling(uniqueID);
            if (kinling != null)
            {
                kinling.ClickObject.TriggerSelected(true);
            }
        }

        [Command("set_love")]
        private void CMD_SetLove(string instigatorFirstName, string receiverFirstName)
        {
            KinlingData instigator = _registeredKinlingsData.Find(unit => unit.Firstname == instigatorFirstName);
            KinlingData receiver = _registeredKinlingsData.Find(unit => unit.Firstname == receiverFirstName);

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
            
            instigator.GetKinling().SocialAI.FormRomanticRelationship(receiver);
            receiver.GetKinling().SocialAI.FormRomanticRelationship(instigator);
        }

        [Command("mate")]
        private void CMD_Mate(string instigatorFirstName, string receiverFirstName)
        {
            KinlingData instigator = _registeredKinlingsData.Find(unit => unit.Firstname == instigatorFirstName);
            KinlingData receiver = _registeredKinlingsData.Find(unit => unit.Firstname == receiverFirstName);
            
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

            Task task = new Task("Mate", ETaskType.Personal, instigator.AssignedBed.GetLinkedFurniture(), EToolType.None);
            instigator.GetKinling().TaskAI.QueueTask(task);
        }

        public void SpawnChild(KinlingData mother, KinlingData father)
        {
            var kinlingData = new KinlingData();
            kinlingData.InheritData(mother, father);
            var child = Spawner.Instance.SpawnKinling($"child", mother.Position);
                
            mother.ChildrenUID.Add(child.RuntimeData.UniqueID);
            father.ChildrenUID.Add(child.RuntimeData.UniqueID);
        }

        public void SpawnKinling(KinlingData dataToLoad, Vector2 spawnPos)
        {
            var kinling = Spawner.Instance.SpawnKinling($"{dataToLoad.Firstname}_{dataToLoad.Lastname}", spawnPos);
            AppearanceBuilder.Instance.UpdateAppearance(dataToLoad);
            kinling.SetKinlingData(dataToLoad);
        }
        
        public IEnumerator DeleteAllKinlings()
        {
            var kinlings = transform.GetComponentsInChildren<Kinling>();
            foreach (var kinling in kinlings.Reverse())
            {
                Destroy(kinling.gameObject);
                yield return null; // Yield to allow frame update and avoid stuttering
            }
    
            _registeredKinlingsData.Clear();
        }

        public void StopAllKinlingTasks()
        {
            foreach (var kinlingData in _registeredKinlingsData)
            {
                kinlingData.GetKinling().TaskHandler.StopTask();
            }
        }
    }
}

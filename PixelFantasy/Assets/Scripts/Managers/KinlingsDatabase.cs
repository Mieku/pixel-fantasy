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
        private List<KinlingData> _allKinlings = new List<KinlingData>();

        public List<KinlingData> GetKinlingsData() => _allKinlings;

        public void LoadKinlingsData(List<KinlingData> data)
        {
            DeleteAllKinlings();

            foreach (var kinlingData in data)
            {
                SpawnKinling(kinlingData, kinlingData.Position, false);
            }
        }
        
        public List<KinlingData> GetAllUnitsInRadius(Vector2 startPoint, float radius)
        {
            return _allKinlings.Where(kinling => 
                Vector2.Distance(startPoint, kinling.Position) <= radius
            ).ToList();
        }
        
        public void RegisterKinling(KinlingData kinling)
        {
            if (_allKinlings.Contains(kinling))
            {
                Debug.LogError("Tried to register the same Kinling Twice: " + kinling.Fullname);
                return;
            }
            
            _allKinlings.Add(kinling);
            
            KinlingSelector.Instance.AddKinling(kinling);
        }

        public void DeregisterKinling(KinlingData kinling)
        {
            if (!_allKinlings.Contains(kinling))
            {
                Debug.LogError("Tried to deregister a non-registered Kinling: " + kinling.Fullname);
                return;
            }

            _allKinlings.Remove(kinling);
            
            KinlingSelector.Instance.RemoveKinling(kinling);
        }
        
        public Kinling GetUnit(string uniqueID)
        {
            return _allKinlings.Find(unit => unit.UniqueID == uniqueID).Kinling;
        }

        public KinlingData GetKinlingData(string uniqueID)
        {
            return _allKinlings.Find(kinling => kinling.UniqueID == uniqueID);
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
            KinlingData instigator = _allKinlings.Find(unit => unit.Firstname == instigatorFirstName);
            KinlingData receiver = _allKinlings.Find(unit => unit.Firstname == receiverFirstName);

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
            
            instigator.Kinling.SocialAI.FormRomanticRelationship(receiver);
            receiver.Kinling.SocialAI.FormRomanticRelationship(instigator);
        }

        [Command("mate")]
        private void CMD_Mate(string instigatorFirstName, string receiverFirstName)
        {
            KinlingData instigator = _allKinlings.Find(unit => unit.Firstname == instigatorFirstName);
            KinlingData receiver = _allKinlings.Find(unit => unit.Firstname == receiverFirstName);
            
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

            Task task = new Task("Mate", ETaskType.Personal, instigator.AssignedBed.LinkedFurniture, EToolType.None);
            instigator.Kinling.TaskAI.QueueTask(task);
        }

        public void SpawnChild(KinlingData mother, KinlingData father)
        {
            var kinlingData = new KinlingData();
            kinlingData.InheritData(mother, father);
            var child = Spawner.Instance.SpawnKinling($"child", mother.Position);
                
            mother.ChildrenUID.Add(child.RuntimeData.UniqueID);
            father.ChildrenUID.Add(child.RuntimeData.UniqueID);
        }

        public void SpawnKinling(KinlingData dataToLoad, Vector2 spawnPos, bool isNew)
        {
            var kinling = Spawner.Instance.SpawnKinling($"{dataToLoad.Firstname}_{dataToLoad.Lastname}", spawnPos);
            AppearanceBuilder.Instance.UpdateAppearance(dataToLoad);
            kinling.SetKinlingData(dataToLoad, isNew);
        }

        public void DeleteAllKinlings()
        {
            var kinlingsToDestroy = _allKinlings.ToList();
            
            foreach (var data in kinlingsToDestroy)
            {
                DestroyImmediate(data.Kinling.gameObject);
            }
        }
    }
}

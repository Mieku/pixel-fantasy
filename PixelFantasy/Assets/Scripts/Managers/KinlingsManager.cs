using System.Collections.Generic;
using System.Linq;
using Characters;
using QFSW.QC;
using TaskSystem;
using UnityEngine;
using KinlingSelector = Systems.Kinling_Selector.Scripts.KinlingSelector;

namespace Managers
{
    public class KinlingsManager : Singleton<KinlingsManager>
    {
        private List<KinlingData> _allKinlings = new List<KinlingData>();

        public List<KinlingData> AllKinlings => _allKinlings;
        
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
            return AllKinlings.Find(unit => unit.UniqueID == uniqueID).Kinling;
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
                
            mother.Children.Add(child.RuntimeData);
            father.Children.Add(child.RuntimeData);
        }

        public void SpawnKinling(KinlingData dataToLoad, Vector2 spawnPos)
        {
            var kinling = Spawner.Instance.SpawnKinling($"{dataToLoad.Firstname}_{dataToLoad.Lastname}", spawnPos);
            kinling.SetKinlingData(dataToLoad);
            
            // DataLibrary.RegisterInitializationCallback(() =>
            // {
            //     var kinling = Spawner.Instance.SpawnKinling($"{dataToLoad.Firstname}_{dataToLoad.Lastname}", spawnPos);
            //     //var kinlingData = (KinlingData)DataLibrary.CloneDataObjectToRuntime(dataToLoad, kinling.gameObject);
            //     kinling.SetKinlingData(dataToLoad);
            //     //var kinling = Spawner.Instance.SpawnKinling(kinlingData, spawnPos);
            // });
        }
    }
}

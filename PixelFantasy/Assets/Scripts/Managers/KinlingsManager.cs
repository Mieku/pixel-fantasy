using System.Collections.Generic;
using System.Linq;
using Characters;
using Data.Item;
using Data.Structure;
using Databrain;
using QFSW.QC;
using ScriptableObjects;
using Systems.Notifications.Scripts;
using Systems.Skills.Scripts;
using TaskSystem;
using UnityEngine;
using KinlingSelector = Systems.Kinling_Selector.Scripts.KinlingSelector;

namespace Managers
{
    public class KinlingsManager : Singleton<KinlingsManager>
    {
        public DataLibrary DataLibrary;
        public KinlingData GenericKinlingData;
        
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
            
            KinlingSelector.Instance.AddKinling(kinling.RuntimeData);
        }

        public void DeregisterKinling(Kinling kinling)
        {
            if (!_allKinlings.Contains(kinling))
            {
                Debug.LogError("Tried to deregister a non-registered Kinling: " + kinling.FullName);
                return;
            }

            _allKinlings.Remove(kinling);
            
            KinlingSelector.Instance.RemoveKinling(kinling.RuntimeData);
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
            Kinling instigator = _allKinlings.Find(unit => unit.RuntimeData.Firstname == instigatorFirstName);
            Kinling receiver = _allKinlings.Find(unit => unit.RuntimeData.Firstname == receiverFirstName);

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

            instigator.RuntimeData.Partner = receiver.RuntimeData;
            receiver.RuntimeData.Partner = instigator.RuntimeData;
            NotificationManager.Instance.CreateKinlingLog(instigator, $"{instigator.FullName} is now in a relationship with {receiver.FullName}!", LogData.ELogType.Positive);
        }

        [Command("mate")]
        private void CMD_Mate(string instigatorFirstName, string receiverFirstName)
        {
            Kinling instigator = _allKinlings.Find(unit => unit.RuntimeData.Firstname == instigatorFirstName);
            Kinling receiver = _allKinlings.Find(unit => unit.RuntimeData.Firstname == receiverFirstName);
            
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

            Task task = new Task("Mate", ETaskType.Personal, instigator.RuntimeData.AssignedBed.LinkedFurniture, EToolType.None);
            instigator.TaskAI.QueueTask(task);
        }

        public void SpawnChild(KinlingData mother, KinlingData father)
        {
            DataLibrary.RegisterInitializationCallback(() =>
            {
                var kinlingData = (KinlingData)DataLibrary.CloneDataObjectToRuntime(GenericKinlingData, gameObject);
                kinlingData.InheritData(mother, father);
                var child = Spawner.Instance.SpawnKinling($"child", mother.Position);
                
                mother.Children.Add(child.RuntimeData);
                father.Children.Add(child.RuntimeData);
            });
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

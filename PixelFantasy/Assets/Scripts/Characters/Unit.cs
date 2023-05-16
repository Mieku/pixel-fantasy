using System;
using System.Collections.Generic;
using DataPersistence;
using Items;
using Managers;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;
using UnityEngine.AI;

namespace Characters
{
    public class Unit : UniqueObject, IPersistent
    {
        [SerializeField] private TaskAI _taskAI;
        [SerializeField] private UnitState _unitState;
        [SerializeField] private UnitAppearance _appearance;
        [SerializeField] private UnitInventory _inventory;

        public UnitAnimController UnitAnimController;
        public UnitAgent UnitAgent;

        private ItemData _heldTool;

        public void AssignHeldTool(ItemData toolItemData)
        {
            _heldTool = toolItemData;
        }

        public void DropHeldTool()
        {
            Spawner.Instance.SpawnItem(_heldTool, transform.position, true);
            _heldTool = null;
        }

        public bool IsHoldingTool()
        {
            return _heldTool != null;
        }

        public UnitState GetUnitState()
        {
            return _unitState;
        }
        
        public void AssignProfession(ProfessionData newProfession)
        {
            CraftingBill.RequestedItemInfo tool = null;
            if (newProfession.RequiredTool != null)
            {
                // Claim the tool
                var claimedToolStorage = InventoryManager.Instance.ClaimItem(newProfession.RequiredTool);
                tool = new CraftingBill.RequestedItemInfo(newProfession.RequiredTool, claimedToolStorage, 1);
            }
            
            List<CraftingBill.RequestedItemInfo> mats = new List<CraftingBill.RequestedItemInfo>();
            if (tool != null)
            {
                mats.Add(tool);
            }

            Task task = new Task()
            {
                TaskId = "Change Profession",
                Requestor = null,
                Payload = newProfession.ProfessionName,
                Materials = mats,
            };
            
            _taskAI.QueueTask(task);
        }
        
        public object CaptureState()
        {
            var unitStateData = _unitState.GetStateData();
            var appearanceData = _appearance.GetSaveData();

            return new UnitData
            {
                UID = UniqueId,
                Position = transform.position,
                UnitStateData = unitStateData,
                AppearanceData = appearanceData,
            };
        }

        public void RestoreState(object data)
        {
            var unitData = (UnitData)data;

            UniqueId = unitData.UID;
            transform.position = unitData.Position;
            
            // Send the data to all components
            _unitState.SetLoadData(unitData.UnitStateData);
            _appearance.SetLoadData(unitData.AppearanceData);
        }

        public struct UnitData
        {
            public string UID;
            public Vector3 Position;

            
            // Unit State
            public UnitState.UnitStateData UnitStateData;
            
            // Unit Appearance
            public UnitAppearance.AppearanceData AppearanceData;
        }
    }
}

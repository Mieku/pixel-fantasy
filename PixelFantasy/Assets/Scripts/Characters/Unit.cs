using System;
using System.Collections.Generic;
using DataPersistence;
using Items;
using Managers;
using ScriptableObjects;
using Systems.Mood.Scripts;
using Systems.SmartObjects.Scripts;
using Systems.Traits.Scripts;
using TaskSystem;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

namespace Characters
{
    public class Unit : UniqueObject, IPersistent
    {
        [SerializeField] private RaceData _race;
        [SerializeField] private TaskAI _taskAI;
        [SerializeField] private NeedsAI _needsAI;
        [SerializeField] private UnitState _unitState;
        [SerializeField] private UnitAppearance _appearance;
        [SerializeField] private UnitInventory _inventory;
        [SerializeField] private Mood _mood;
        
        [Header("Traits")] 
        [SerializeField] protected List<Trait> _traits;
 
        [SerializeField] private SortingGroup _sortingGroup;
        [SerializeField] private PositionRendererSorter _positionRendererSorter;
        
        public KinlingEquipment Equipment;
        public UnitAnimController UnitAnimController;
        public UnitAgent UnitAgent;

        public RaceData Race => _race;
        public Mood KinlingMood => _mood;
        public List<Trait> AllTraits => _traits;

        private void Start()
        {
            UnitsManager.Instance.RegisterKinling(this);
            Equipment.Init(this);
            _appearance.Init(this);
            _mood.Init();
        }

        private void OnDestroy()
        {
            UnitsManager.Instance.DeregisterKinling(this);
        }

        // public void AssignHeldTool(ItemData toolItemData)
        // {
        //     _heldTool = toolItemData;
        //     UnitAnimController.Appearance.DisplayTool(toolItemData);
        // }
        //
        // public void DropHeldTool()
        // {
        //     Spawner.Instance.SpawnItem(_heldTool, transform.position, true);
        //     _heldTool = null;
        //     UnitAnimController.Appearance.DisplayTool(null);
        // }
        //
        // public bool IsHoldingTool()
        // {
        //     return _heldTool != null;
        // }

        public void AssignAndLockLayerOrder(int orderInLayer)
        {
            _positionRendererSorter.SetLocked(true);
            _sortingGroup.sortingOrder = orderInLayer;
        }

        public void UnlockLayerOrder()
        {
            _positionRendererSorter.SetLocked(false);
        }

        public UnitState GetUnitState()
        {
            return _unitState;
        }

        public TaskAI TaskAI => _taskAI;
        public NeedsAI NeedsAI => _needsAI;

        public UnitAppearance GetAppearance()
        {
            return _appearance;
        }
        
        // public void AssignProfession(ProfessionData newProfession)
        // {
        //     CraftingBill.RequestedItemInfo tool = null;
        //     if (newProfession.RequiredTool != null)
        //     {
        //         // Claim the tool
        //         var claimedToolStorage = InventoryManager.Instance.ClaimItem(newProfession.RequiredTool);
        //         tool = new CraftingBill.RequestedItemInfo(newProfession.RequiredTool, claimedToolStorage, 1);
        //     }
        //     
        //     List<CraftingBill.RequestedItemInfo> mats = new List<CraftingBill.RequestedItemInfo>();
        //     if (tool != null)
        //     {
        //         mats.Add(tool);
        //     }
        //
        //     Task task = new Task()
        //     {
        //         TaskId = "Change Profession",
        //         Requestor = null,
        //         Payload = newProfession.ProfessionName,
        //         Materials = mats,
        //     };
        //     
        //     _taskAI.QueueTask(task);
        // }

        public List<StatTrait> GetStatTraits()
        {
            List<StatTrait> results = new List<StatTrait>();
            foreach (var trait in _traits)
            {
                var statTrait = trait as StatTrait;
                if (statTrait != null)
                {
                    results.Add(statTrait);
                }
            }

            return results;
        }
        
        public MoodThresholdTrait GetMoodThresholdTrait()
        {
            foreach (var trait in _traits)
            {
                var moodThresholdTrait = trait as MoodThresholdTrait;
                if (moodThresholdTrait != null)
                {
                    return moodThresholdTrait;
                }
            }

            return null;
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

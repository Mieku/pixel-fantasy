using System.Collections.Generic;
using AI;
using DataPersistence;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Systems.Input_Management;
using UnityEngine;

namespace Items
{
    public class FueledCraftingTable : CraftingTable, IFueledFurniture
    {
        [ShowInInspector] public FueledCraftingTableData FueledRuntimeData => RuntimeData as FueledCraftingTableData;

        [SerializeField] private SpriteRenderer _outOfFuelIcon;
        [SerializeField] private List<GameObject> _fueledGOs;

        public override void InitializeFurniture(FurnitureSettings furnitureSettings, PlacementDirection direction, DyeSettings dye)
        {
            var tableSettings = (FueledCraftingTableSettings) furnitureSettings;
            
            RuntimeData = new FueledCraftingTableData();
            RuntimeTableData.InitData(tableSettings);
            RuntimeData.Direction = direction;
            
            IsAllowed = true;
            RefreshAllowedDisplay();
            RefreshAllowCommands();
            
            SetState(RuntimeData.FurnitureState);
            AssignDirection(direction);
            
            ShowFueledSprites(false);
        }

        public override void LoadData(FurnitureData data)
        {
            base.LoadData(data);
            
            CheckFuel();
        }

        public override void StartPlanning(FurnitureSettings furnitureSettings, PlacementDirection initialDirection, DyeSettings dye)
        {
            base.StartPlanning(furnitureSettings, initialDirection, dye);
            ShowOutOfFuelIcon(false);
        }

        protected override void Built_Enter()
        {
            base.Built_Enter();
            CheckFuel();
        }

        protected override void Awake()
        {
            base.Awake();
            GameEvents.MinuteTick += OnMinuteTick;
        }

        protected override void OnDestroy()
        {
            GameEvents.MinuteTick -= OnMinuteTick;
            base.OnDestroy();
        }

        private void OnMinuteTick()
        {
            BurnFuel(1);
        }

        private void BurnFuel(float minutes)
        {
            if (_isPlanning) return;
            
            if (FueledRuntimeData.RemainingBurnTime >= 1)
            {
                FueledRuntimeData.RemainingBurnTime -= minutes;
            }

            CheckFuel();
        }

        private void CheckFuel()
        {
            if(RuntimeData.FurnitureState != EFurnitureState.Built) return;
            
            FueledRuntimeData.CheckToRefillBurnTime();
            
            if (HasFuel)
            {
                TriggerHasFuel();
            }
            else
            {
                TriggerOutOfFuel();
            }

            if (!FueledRuntimeData.IsFullyStocked)
            {
                CreateRefuelTasks();
            }
        }

        private void TriggerOutOfFuel()
        {
            ShowOutOfFuelIcon(true);
            ShowFueledSprites(false);
            
            // Check if there are currently any crafting tasks, and cancel them
            if (RuntimeTableData.CurrentOrder != null)
            {
                RuntimeTableData.CurrentOrder.CancelOrder();
                RuntimeTableData.CurrentOrderID = null;
            }
        }

        private void ShowFueledSprites(bool show)
        {
            foreach (var fueledGO in _fueledGOs)
            {
                fueledGO.SetActive(show);
            }
        }

        private void TriggerHasFuel()
        {
            ShowFueledSprites(true);
            ShowOutOfFuelIcon(false);
        }

        private void CreateRefuelTasks()
        {
            if(!string.IsNullOrEmpty(FueledRuntimeData.CurrentRefuelTaskUID)) return;
            if (FueledRuntimeData.IsFullyStocked) return;
            if(!FueledRuntimeData.IsRefuelingAllowed) return;
            
            Dictionary<string, object> taskData = new Dictionary<string, object> { { "ItemSettingsID", FueledRuntimeData.FueledSettings.FuelSettings.ItemForFuel.name } };
            Task task = new Task("Refuel", "Adding Fuel" ,ETaskType.Hauling, this, taskData);
            TasksDatabase.Instance.AddTask(task);
            FueledRuntimeData.CurrentRefuelTaskUID = task.UniqueID;
        }

        public void Refuel(ItemData itemData)
        {
            FueledRuntimeData.CurrentRefuelTaskUID = null;
            
            var item = itemData.GetLinkedItem();
            Destroy(item.gameObject);

            FueledRuntimeData.StoredFuelAmount++;
            CheckFuel();
        }

        public float GetRemainingBurnPercent()
        {
            return FueledRuntimeData.RemainingBurnPercent;
        }

        public FuelSettings GetFuelSettings()
        {
            return FueledRuntimeData.FueledSettings.FuelSettings;
        }

        public int GetAmountFuelAvailable()
        {
            return FueledRuntimeData.StoredFuelAmount;
        }

        public bool IsRefuellingAllowed()
        {
            return FueledRuntimeData.IsRefuelingAllowed;
        }

        public void SetRefuellingAllowed(bool isAllowed)
        {
            FueledRuntimeData.IsRefuelingAllowed = isAllowed;
            if (!isAllowed && !string.IsNullOrEmpty(FueledRuntimeData.CurrentRefuelTaskUID))
            {
                var refuelTask = TasksDatabase.Instance.QueryTask(FueledRuntimeData.CurrentRefuelTaskUID);
                FueledRuntimeData.CurrentRefuelTaskUID = null;
                refuelTask.Cancel();
            }
        }

        private void ShowOutOfFuelIcon(bool show)
        {
            if (show)
            {
                _outOfFuelIcon.sprite = FueledRuntimeData.FueledSettings.FuelSettings.ItemForFuel.ItemSprite;
                _outOfFuelIcon.gameObject.SetActive(true);
            }
            else
            {
                _outOfFuelIcon.gameObject.SetActive(false);
            }
        }

        public bool HasFuel => FueledRuntimeData.RemainingBurnTime > 0;

        public override bool IsAvailable
        {
            get
            {
                if (RuntimeData == null) return false;
                if (RuntimeData.FurnitureState != EFurnitureState.Built) return false;
                if (!HasFuel) return false;

                if (RuntimeData.InUse || RuntimeData.HasUseBlockingCommand || !RuntimeData.IsAllowed)
                {
                    return false;
                }
                    
                return true;
            }
        }
    }
}

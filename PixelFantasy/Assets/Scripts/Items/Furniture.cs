using System;
using System.Collections.Generic;
using Gods;
using ScriptableObjects;
using UnityEngine;

namespace Items
{
    public class Furniture : Construction
    {
        private FurnitureData _furnitureData;
        protected FurniturePrefab _prefab;
        protected bool _isCraftingTable;
        private PlacementDirection _placementDirection;

        protected Transform UsagePosition => _prefab.UsagePostion;
        public FurnitureData FurnitureData => _furnitureData;
        
        public virtual void Init(FurnitureData furnitureData, PlacementDirection direction)
        {
            _furnitureData = furnitureData;
            _remainingResourceCosts = new List<ItemAmount>(_furnitureData.ResourceCosts);
            _pendingResourceCosts = new List<ItemAmount>();
            _placementDirection = direction;
            CreatePrefab(direction);
            ShowBlueprint(true);
            PrepForConstruction();
        }

        private void CreatePrefab(PlacementDirection placementDirection)
        {
            var prefabObj = Instantiate(_furnitureData.GetFurniturePrefab(placementDirection), transform);
            _prefab = prefabObj.GetComponent<FurniturePrefab>();
        }
        
        private void ShowBlueprint(bool showBlueprint)
        {
            if (showBlueprint)
            {
                _prefab.FurnitureRenderer.color = Librarian.Instance.GetColour("Blueprint");
            }
            else
            {
                _prefab.FurnitureRenderer.color = Color.white;
                gameObject.layer = 4;
            }
        }

        private void PrepForConstruction()
        {
            if (_furnitureData.ConstructionMethod == ConstructionMethod.Hand)
            {
                // Build like you would a structure
                CreateConstructionHaulingTasks();
            }
            else
            {
                var resourceCosts = GetRemainingMissingItems();
                CraftMissingItems(resourceCosts);
                //CraftingTask craftItem = new CraftingTask();
                // craftItem.FurnitureData = _furnitureData;
                //
                // CreateCraftingTask(craftItem);
                //
                // EnqueueCreateInstallTask(craftItem);
            }
        }

        private void CreateConstructionHaulingTasks()
        {
            var resourceCosts = GetRemainingMissingItems();
            
            
            CraftMissingItems(resourceCosts);
            
            foreach (var resourceCost in resourceCosts)
            {
                for (int i = 0; i < resourceCost.Quantity; i++)
                {
                    EnqueueCreateTakeResourceToBlueprintTask(resourceCost.Item);
                }
            }
        }

        protected void CraftMissingItems(List<ItemAmount> requiredResources)
        {
            foreach (var resource in requiredResources)
            {
                // float remainingNeeded = resource.Quantity - ControllerManager.Instance.InventoryController.AvailableItemQuantity(resource.Item);
                // float amountMadePerCraft = resource.Item.CraftedQuantity;
                // var numTasks = (int)Math.Ceiling(remainingNeeded / amountMadePerCraft);

                // for (int i = 0; i < numTasks; i++)
                // {
                //     // CraftingTask craft = new CraftingTask();
                //     // craft.ItemData = resource.Item;
                //     // CreateCraftingTask(craft);
                // }
            }
        }
        
        public override float GetWorkPerResource()
        {
            return _furnitureData.GetWorkPerResource();
        }

        public override void CompleteConstruction()
        {
            base.CompleteConstruction();
            ShowBlueprint(false);
            _isBuilt = true;
        }

        protected virtual void InstallFurniture()
        {
            ShowBlueprint(false);
            _isBuilt = true;
        }

        public override List<ItemAmount> GetResourceCosts()
        {
            return _furnitureData.ResourceCosts;
        }
        
        public override object CaptureState()
        {
            return new Data
            {
                UID = this.UniqueId,
                Position = transform.position,
                FurnitureData = _furnitureData,
                IsBuilt = _isBuilt,
                IsDeconstructing = _isDeconstructing,
                IsCraftingTable = _isCraftingTable,
                RemainingResourceCosts = _remainingResourceCosts,
                PlacementDirection = _placementDirection,
                IncomingResourceCosts = _incomingResourceCosts,
                HasIncomingUnit = _hasUnitIncoming,
            };
        }

        public override void RestoreState(object data)
        {
            var state = (Data)data;

            UniqueId = state.UID;
            transform.position = state.Position;
            _furnitureData = state.FurnitureData;
            _isBuilt = state.IsBuilt;
            _isDeconstructing = state.IsDeconstructing;
            _isCraftingTable = state.IsCraftingTable;
            _remainingResourceCosts = state.RemainingResourceCosts;
            _placementDirection = state.PlacementDirection;
            _incomingResourceCosts = state.IncomingResourceCosts;
            _hasUnitIncoming = state.HasIncomingUnit;
            
            if (!_isBuilt)
            {
                PrepForConstruction();
            }
            
            CheckIfAllResourcesLoaded();
            CreatePrefab(_placementDirection);
            ShowBlueprint(!_isBuilt);
            
        }

        public struct Data
        {
            public string UID;
            public Vector3 Position;
            public FurnitureData FurnitureData;
            public bool IsBuilt;
            public bool IsDeconstructing;
            public bool HasIncomingUnit;
            public bool IsCraftingTable;
            public List<ItemAmount> RemainingResourceCosts;
            public PlacementDirection PlacementDirection;
            public List<ItemAmount> IncomingResourceCosts;
        }
    }
    
    [Serializable]
    public enum ConstructionMethod
    {
        None, // Aka: Natural Resource
        Hand,
        Carpentry
    }
}

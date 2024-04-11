using System;
using System.Collections.Generic;
using System.Linq;
using Data.Item;
using Databrain.Attributes;
using Items;
using Systems.Zones.Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Data.Zones
{
    [UseOdinInspector]
    public class StockpileZoneData : ZoneData, IStorage
    {
        [ExposeToInspector, DatabrainSerialize]
        public StockpileZoneSettings StockpileSettings;
        
        // What is stored? and Where? Maybe make a storage cell type
        public List<StockpileCell> StockpileCells => ZoneCells.Cast<StockpileCell>().ToList();
        
        // Specific storage player settings chosen for area 
        [field: ExposeToInspector]
        [field: DatabrainSerialize]
        [field: SerializeField]
        public StoragePlayerSettings PlayerSettings { get; private set; }


        public void InitData(StockpileZoneSettings settings)
        {
            StockpileSettings = settings;
            PlayerSettings = new StoragePlayerSettings();
            PlayerSettings.PasteSettings(settings.DefaultPlayerSettings);
            IsEnabled = true;

            ZoneName = $"Stockpile {AssignedLayer}";
        }

        public void CopyData(StockpileZoneData dataToCopy)
        {
            StockpileSettings = dataToCopy.StockpileSettings;
            PlayerSettings = new StoragePlayerSettings();
            PlayerSettings.PasteSettings(dataToCopy.PlayerSettings);
            IsEnabled = dataToCopy.IsEnabled;
            
            ZoneName = $"Stockpile {AssignedLayer}";
        }
        
        public override Color ZoneColour => Settings.ZoneColour;
        public override TileBase DefaultTiles => Settings.DefaultTiles;
        public override TileBase SelectedTiles => Settings.SelectedTiles;
        public override EZoneType ZoneType => EZoneType.Stockpile;
        public override ZoneSettings Settings => StockpileSettings;

        public List<ItemData> Stored
        {
            get
            {
                List<ItemData> results = new List<ItemData>();
                foreach (var stockpileCell in StockpileCells)
                {
                    results.AddRange(stockpileCell.Stored);
                }
                return results;
            }
        }

        public List<ItemData> Incoming
        {
            get
            {
                List<ItemData> results = new List<ItemData>();
                foreach (var stockpileCell in StockpileCells)
                {
                    results.AddRange(stockpileCell.Incoming);
                }
                return results;
            }
        }
        
        public List<ItemData> Claimed
        {
            get
            {
                List<ItemData> results = new List<ItemData>();
                foreach (var stockpileCell in StockpileCells)
                {
                    results.AddRange(stockpileCell.Claimed);
                }
                return results;
            }
        }
        
        public void SetIncoming(ItemData itemData)
        {
            if (!PlayerSettings.IsItemValidToStore(itemData.Settings))
            {
                Debug.LogError("Attempting to store the wrong item category");
                return;
            }

            var availableSpace = AmountCanBeDeposited(itemData.Settings);
            if (availableSpace <= 0)
            {
                Debug.LogError("Attempted to set incoming with no space available");
                return;
            }
            
            // Find the cell that contains the item
            StockpileCell cell = GetAvailableCellForItem(itemData.Settings);
            cell.Incoming.Add(itemData);
            
            GameEvents.Trigger_RefreshInventoryDisplay();
        }

        public void CancelIncoming(ItemData itemData)
        {
            if (IsSpecificItemDataIncoming(itemData))
            {
                Incoming.Remove(itemData);
            }
            
            GameEvents.Trigger_RefreshInventoryDisplay();
        }

        public void DepositItems(Items.Item item)
        {
            var runtimeData = item.RuntimeData;
            
            if (!IsSpecificItemDataIncoming(runtimeData))
            {
                Debug.LogError("Tried to deposit an item that was not set as incoming");
                return;
            }

            runtimeData.AssignedStorage = this;

            var cell = GetAssignedCellForSpecificItem(runtimeData);
            cell.DepositItem(runtimeData);
            
            Destroy(item.gameObject);
            OnZoneChanged.Invoke();
        }

        public Items.Item WithdrawItem(ItemData itemData)
        {
            var cell = GetAssignedCellForSpecificItem(itemData);
            var item = cell.WithdrawItem(itemData);
            OnZoneChanged.Invoke();
            return item;
        }

        public bool ClaimItem(ItemData itemToClaim)
        {
            var cell = GetAssignedCellForSpecificItem(itemToClaim);
            if (!cell.Stored.Contains(itemToClaim))
            {
                Debug.LogError($"Could not find {itemToClaim.guid} in storage");
                return false;
            }

            if (cell.Claimed.Contains(itemToClaim))
            {
                Debug.LogError($"Attempted to Claim {itemToClaim.guid}, but it was already claimed");
                return false;
            }
            
            cell.Claimed.Add(itemToClaim);
            GameEvents.Trigger_RefreshInventoryDisplay();
            return true;
        }

        public void RestoreClaimed(ItemData itemData)
        {
            var cell = GetAssignedCellForSpecificItem(itemData);
            if (!cell.Claimed.Contains(itemData))
            {
                Debug.LogError($"Item Claim: {itemData.guid} was not restored, was not found in claimed");
                return;
            }

            cell.Claimed.Remove(itemData);
            GameEvents.Trigger_RefreshInventoryDisplay();
        }

        public Vector2? AccessPosition(Vector2 requestorPosition, ItemData specificItem)
        {
            var cell = GetAssignedCellForSpecificItem(specificItem);

            if (cell == null)
            {
                Debug.LogError($"{specificItem} is not stored or incoming in any cells");
                return null;
            }
            
            return cell.Position;
        }

        public bool IsAvailable => IsEnabled;
        public bool IsItemInStorage(ItemSettings itemSettings)
        {
            foreach (var cell in StockpileCells)
            {
                if (cell.StoredItemSettings == itemSettings && cell.Claimed.Count < cell.Stored.Count)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsSpecificItemInStorage(ItemData specificItem)
        {
            var cell = GetAssignedCellForSpecificItem(specificItem);
            if (cell == null) return false;

            return true;
        }

        public ItemData GetItemDataOfType(ItemSettings itemSettings)
        {
            int amountClaimable = AmountCanBeWithdrawn(itemSettings);
            if (amountClaimable <= 0) return null;

            foreach (var cell in StockpileCells)
            {
                if (cell.StoredItemSettings == itemSettings)
                {
                    ItemData item = cell.GetUnclaimedItem(itemSettings);
                    if (item != null)
                    {
                        return item;
                    }
                }
            }
            
            return null;
        }

        public int AmountCanBeWithdrawn(ItemSettings itemSettings)
        {
            int result = 0;
            foreach (var cell in StockpileCells)
            {
                result += cell.AmountCanBeWithdrawn(itemSettings);
            }

            return result;
        }

        public int AmountCanBeDeposited(ItemSettings itemSettings)
        {
            if (!PlayerSettings.IsItemValidToStore(itemSettings)) return 0;

            int result = 0;
            foreach (var cell in StockpileCells)
            {
                result += cell.AmountCanBeDeposited(itemSettings);
            }

            return result;
        }

        public List<ToolData> GetAllToolItems(bool includeIncoming = false)
        {
            throw new System.NotImplementedException(); // Going to get rid of this.
        }
        
        private bool IsSpecificItemDataIncoming(ItemData itemData)
        {
            foreach (var incoming in Incoming)
            {
                if (incoming == itemData)
                {
                    return true;
                }
            }

            return false;
        }

        private StockpileCell GetAvailableCellForItem(ItemSettings itemSettings)
        {
            // Check if any cells can accept more
            foreach (var cell in StockpileCells)
            {
                if (cell.StoredItemSettings == itemSettings && cell.SpaceAvailable)
                {
                    return cell;
                }
            }

            foreach (var cell in StockpileCells)
            {
                if (cell.IsEmpty)
                {
                    return cell;
                }
            }

            return null;
        }

        private StockpileCell GetAssignedCellForSpecificItem(ItemData itemData)
        {
            foreach (var cell in StockpileCells)
            {
                if (cell.Stored.Contains(itemData) || cell.Incoming.Contains(itemData))
                {
                    return cell;
                }
            }

            Debug.LogError($"No cell was found for {itemData.guid}");
            return null;
        }
    }
}

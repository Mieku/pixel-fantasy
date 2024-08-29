using System;
using System.Collections.Generic;
using System.Linq;
using Handlers;
using TMPro;
using UnityEngine;

namespace Systems.Zones.Scripts
{
    [Serializable]
    public class StockpileCell : ZoneCell
    {
        [SerializeField] private SpriteRenderer _itemDisplay;
        [SerializeField] private TextMeshProUGUI _amountDisplay;
        
        public List<string> StoredUIDs = new List<string>();
        public List<string> IncomingUIDs = new List<string>();
        public List<string> ClaimedUIDs = new List<string>();

        public override void Init(ZoneData data, Vector3Int cellPos)
        {
            base.Init(data, cellPos);

            var stockpileZoneData = (StockpileZoneData)data;
            var cell = stockpileZoneData.StorageCellDatas.Find(c => c.Cell == cellPos);
            if (cell != null)
            {
                var incomingUIDs = cell.IncomingItemsData;
                var storedUIDs = cell.StoredItemsData;
                var claimedUIDs = cell.ClaimedItemsData;

                foreach (var itemUID in incomingUIDs)
                {
                    var item = ItemsDatabase.Instance.Query(itemUID);
                    IncomingUIDs.Add(item.UniqueID);
                }
                
                foreach (var itemUID in storedUIDs)
                {
                    var item = ItemsDatabase.Instance.Query(itemUID);
                    StoredUIDs.Add(item.UniqueID);
                }
                
                foreach (var itemUID in claimedUIDs)
                {
                    var item = ItemsDatabase.Instance.Query(itemUID);
                    ClaimedUIDs.Add(item.UniqueID);
                }
            }
            
            RefreshDisplay();
        }

        public override void DeleteCell()
        {
            CancelTasks();

            foreach (var storedUID in StoredUIDs)
            {
                var stored = ItemsDatabase.Instance.Query(storedUID);
                stored.State = EItemState.Loose;
                stored.AssignedStorageID = null;
                ItemsDatabase.Instance.CreateItemObject(stored, Position);
            }
            StoredUIDs.Clear();
            
            GameEvents.Trigger_RefreshInventoryDisplay();
            
            base.DeleteCell();
        }

        public override void TransferOwner(ZoneData zoneData)
        {
            CancelTasks();

            foreach (var storedUID in StoredUIDs)
            {
                var stored = ItemsDatabase.Instance.Query(storedUID);
                var stockpileZone = (StockpileZoneData)zoneData;
                stored.AssignedStorageID = stockpileZone.UniqueID;
            }
            
            GameEvents.Trigger_RefreshInventoryDisplay();
            
            base.TransferOwner(zoneData);
        }

        public void CancelTasks()
        {
            for (int i = IncomingUIDs.Count - 1; i >= 0; i--)
            {
                var incomingUID = IncomingUIDs[i];
                var incoming = ItemsDatabase.Instance.Query(incomingUID);
                if (incoming.CurrentTask != null && !string.IsNullOrEmpty(incoming.CurrentTaskID))
                {
                    incoming.CurrentTask.Cancel();
                }
            }
            
            for (int i = StoredUIDs.Count - 1; i >= 0; i--)
            {
                var storedUID = StoredUIDs[i];
                var stored = ItemsDatabase.Instance.Query(storedUID);
                if (stored.CurrentTask != null && !string.IsNullOrEmpty(stored.CurrentTaskID))
                {
                    stored.CurrentTask.Cancel();
                }
            }
        }

        public void RefreshDisplay()
        {
            if (StoredUIDs.Count == 0)
            {
                _itemDisplay.gameObject.SetActive(false);
                _amountDisplay.gameObject.SetActive(false);
            }
            else if(StoredUIDs.Count == 1)
            {
                _itemDisplay.gameObject.SetActive(true);
                var stored = ItemsDatabase.Instance.Query(StoredUIDs.First());
                _itemDisplay.sprite = stored.Settings.ItemSprite;
                
                _amountDisplay.gameObject.SetActive(false);
            }
            else
            {
                _itemDisplay.gameObject.SetActive(true);
                var stored = ItemsDatabase.Instance.Query(StoredUIDs.First());
                _itemDisplay.sprite = stored.Settings.ItemSprite;
                
                _amountDisplay.gameObject.SetActive(true);
                _amountDisplay.text = $"{StoredUIDs.Count}";
            }
        }

        public bool IsEmpty => StoredItemSettings == null;
        public ItemSettings StoredItemSettings
        {
            get
            {
                if (StoredUIDs.Count > 0)
                {
                    var item = ItemsDatabase.Instance.Query(StoredUIDs.First());
                    return item.Settings;
                }

                if (IncomingUIDs.Count > 0)
                {
                    var item = ItemsDatabase.Instance.Query(IncomingUIDs.First());
                    return item.Settings;
                }

                return null;
            }
        }

        public bool SpaceAvailable
        {
            get
            {
                var storedItemSettings = StoredItemSettings;
                if (storedItemSettings == null)
                {
                    return true;
                }
                else
                {
                    int maxStorage = storedItemSettings.MaxStackSize;
                    return (StoredUIDs.Count + IncomingUIDs.Count) < maxStorage;
                }
            }
        }

        public ItemData GetUnclaimedItem(ItemSettings itemSettings)
        {
            foreach (var storedItemUID in StoredUIDs)
            {
                var storedItem = ItemsDatabase.Instance.Query(storedItemUID);
                if (storedItem.Settings == itemSettings && !ClaimedUIDs.Contains(storedItemUID))
                {
                    return storedItem;
                }
            }

            return null;
        }

        public int AmountCanBeWithdrawn(ItemSettings itemSettings)
        {
            if (IsEmpty) return 0;
            if (StoredItemSettings != itemSettings) return 0;

            return StoredUIDs.Count - ClaimedUIDs.Count;
        }
    
        public int AmountCanBeDeposited(ItemSettings itemSettings)
        {
            if (IsEmpty) return itemSettings.MaxStackSize;

            if (StoredItemSettings != itemSettings) return 0;

            int maxStorage = itemSettings.MaxStackSize;
            return maxStorage - (StoredUIDs.Count + IncomingUIDs.Count);
        }

        public Items.Item WithdrawItem(ItemData itemData)
        {
            StoredUIDs.Remove(itemData.UniqueID);
            ClaimedUIDs.Remove(itemData.UniqueID);
        
            RefreshDisplay();
        
            GameEvents.Trigger_RefreshInventoryDisplay();

            var item = ItemsDatabase.Instance.CreateItemObject(itemData, Position);
            return item;
        }

        public void DepositItem(ItemData itemData)
        {
            StoredUIDs.Add(itemData.UniqueID);
            IncomingUIDs.Remove(itemData.UniqueID);
            
            itemData.Position = Helper.SnapToGridPos(Position);
        
            RefreshDisplay();
        
            GameEvents.Trigger_RefreshInventoryDisplay();
        }

        public void LoadInItemData(ItemData itemData)
        {
            StoredUIDs.Add(itemData.UniqueID);
        
            RefreshDisplay();
        
            GameEvents.Trigger_RefreshInventoryDisplay();
        }

        public Vector2 Position =>
            new()
            {
                x = CellPos.x,
                y = CellPos.y
            };
    }
}

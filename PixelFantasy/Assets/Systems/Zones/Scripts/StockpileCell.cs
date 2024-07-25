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
        [SerializeField] private TextMeshPro _amountDisplay;
        
        public List<ItemData> Stored = new List<ItemData>();
        public List<ItemData> Incoming = new List<ItemData>();
        public List<ItemData> Claimed = new List<ItemData>();

        public override void Init(ZoneData data, Vector3Int cellPos)
        {
            base.Init(data, cellPos);

            var stockpileZoneData = (StockpileZoneData)data;
            var cell = stockpileZoneData.StorageCellDatas.Find(c => c.Cell == cellPos);
            if (cell != null)
            {
                var incomingUIDs = cell.IncomingItemsData;
                var storedUIDs = cell.StoredItemsData;

                foreach (var itemUID in incomingUIDs)
                {
                    var item = ItemsDatabase.Instance.Query(itemUID);
                    Incoming.Add(item);
                }
                
                foreach (var itemUID in storedUIDs)
                {
                    var item = ItemsDatabase.Instance.Query(itemUID);
                    Stored.Add(item);
                }
            }
            
            RefreshDisplay();
        }

        public override void DeleteCell()
        {
            CancelTasks();

            foreach (var stored in Stored)
            {
                stored.State = EItemState.Loose;
                stored.AssignedStorageID = null;
                ItemsDatabase.Instance.CreateItemObject(stored, Position, true);
            }
            Stored.Clear();
            
            GameEvents.Trigger_RefreshInventoryDisplay();
            
            base.DeleteCell();
        }

        public override void TransferOwner(ZoneData zoneData)
        {
            CancelTasks();

            foreach (var storedItem in Stored)
            {
                var stockpileZone = (StockpileZoneData)zoneData;
                storedItem.AssignedStorageID = stockpileZone.UniqueID;
            }
            
            GameEvents.Trigger_RefreshInventoryDisplay();
            
            base.TransferOwner(zoneData);
        }

        public void CancelTasks()
        {
            for (int i = Incoming.Count - 1; i >= 0; i--)
            {
                var incoming = Incoming[i];
                if (incoming.GetLinkedItem() != null)
                {
                    incoming.GetLinkedItem().CancelTask();
                }

                if (incoming.CurrentTask != null && !string.IsNullOrEmpty(incoming.CurrentTaskID))
                {
                    incoming.CurrentTask.Cancel(true);
                }
            }
            
            for (int i = Stored.Count - 1; i >= 0; i--)
            {
                var stored = Stored[i];
                if (stored.GetLinkedItem() != null)
                {
                    stored.GetLinkedItem().CancelTask();
                }

                if (stored.CurrentTask != null && !string.IsNullOrEmpty(stored.CurrentTaskID))
                {
                    stored.CurrentTask.Cancel(true);
                }
            }
        }

        public void RefreshDisplay()
        {
            if (Stored.Count == 0)
            {
                _itemDisplay.gameObject.SetActive(false);
                _amountDisplay.gameObject.SetActive(false);
            }
            else if(Stored.Count == 1)
            {
                _itemDisplay.gameObject.SetActive(true);
                _itemDisplay.sprite = Stored.First().Settings.ItemSprite;
                
                _amountDisplay.gameObject.SetActive(false);
            }
            else
            {
                _itemDisplay.gameObject.SetActive(true);
                _itemDisplay.sprite = Stored.First().Settings.ItemSprite;
                
                _amountDisplay.gameObject.SetActive(true);
                _amountDisplay.text = $"{Stored.Count}";
            }
        }

        public bool IsEmpty => StoredItemSettings == null;
        public ItemSettings StoredItemSettings
        {
            get
            {
                if (Stored.Count > 0)
                {
                    return Stored.First().Settings;
                }

                if (Incoming.Count > 0)
                {
                    return Incoming.First().Settings;
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
                    return (Stored.Count + Incoming.Count) < maxStorage;
                }
            }
        }

        public ItemData GetUnclaimedItem(ItemSettings itemSettings)
        {
            foreach (var storedItem in Stored)
            {
                if (storedItem.Settings == itemSettings && !Claimed.Contains(storedItem))
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

            return Stored.Count - Claimed.Count;
        }
    
        public int AmountCanBeDeposited(ItemSettings itemSettings)
        {
            if (IsEmpty) return itemSettings.MaxStackSize;

            if (StoredItemSettings != itemSettings) return 0;

            int maxStorage = itemSettings.MaxStackSize;
            return maxStorage - (Stored.Count + Incoming.Count);
        }

        public Items.Item WithdrawItem(ItemData itemData)
        {
            Stored.Remove(itemData);
            Claimed.Remove(itemData);
        
            RefreshDisplay();
        
            GameEvents.Trigger_RefreshInventoryDisplay();

            var item = ItemsDatabase.Instance.CreateItemObject(itemData, Position, false);
            return item;
        }

        public void DepositItem(ItemData itemData)
        {
            Stored.Add(itemData);
            Incoming.Remove(itemData);
        
            RefreshDisplay();
        
            GameEvents.Trigger_RefreshInventoryDisplay();
        }

        public void LoadInItemData(ItemData itemData)
        {
            Stored.Add(itemData);
        
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

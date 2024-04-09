using System;
using System.Collections.Generic;
using System.Linq;
using Data.Item;
using Data.Zones;
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
            
            RefreshDisplay();
        }

        public override void DeleteCell()
        {
            CancelTasks();

            foreach (var stored in Stored)
            {
                stored.CreateItemObject(Position, true);
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
                storedItem.AssignedStorage = zoneData as StockpileZoneData;
            }
            
            GameEvents.Trigger_RefreshInventoryDisplay();
            
            base.TransferOwner(zoneData);
        }

        public void CancelTasks()
        {
            for (int i = Incoming.Count - 1; i >= 0; i--)
            {
                var incoming = Incoming[i];
                if (incoming.LinkedItem != null)
                {
                    incoming.LinkedItem.CancelTask();
                }

                if (incoming.CurrentTask != null)
                {
                    incoming.CurrentTask.Cancel();
                }
            }
            
            for (int i = Stored.Count - 1; i >= 0; i--)
            {
                var stored = Stored[i];
                if (stored.LinkedItem != null)
                {
                    stored.LinkedItem.CancelTask();
                }

                if (stored.CurrentTask != null)
                {
                    stored.CurrentTask.Cancel();
                }
            }
            
            // foreach (var incoming in Incoming)
            // {
            //     if (incoming.LinkedItem != null)
            //     {
            //         incoming.LinkedItem.CancelTask();
            //     }
            //
            //     if (incoming.CurrentTask != null)
            //     {
            //         incoming.CurrentTask.Cancel();
            //     }
            // }
            //
            // foreach (var stored in Stored)
            // {
            //     if (stored.CurrentTask != null)
            //     {
            //         stored.CurrentTask.Cancel();
            //     }
            // }
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

            var item = itemData.CreateItemObject(Position, false);
            return item;
        }

        public void DepositItem(ItemData itemData)
        {
            Stored.Add(itemData);
            Incoming.Remove(itemData);
        
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

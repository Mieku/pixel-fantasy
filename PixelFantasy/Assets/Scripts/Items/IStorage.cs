using System.Collections.Generic;
using UnityEngine;

namespace Items
{
    public interface IStorage
    {
        public string UniqueID { get; }
        public List<string> StoredUIDs { get; }
        public List<string> IncomingUIDs { get; }
        public List<string> ClaimedUIDs { get; }
        
        public StorageConfigs StorageConfigs { get; }

        public void SetIncoming(ItemData itemData);
        public void CancelIncoming(ItemData itemData);
        
        public void DepositItems(Item item);
        public Item WithdrawItem(ItemData itemData);
        public void LoadInItem(ItemData itemData);

        public bool ClaimItem(ItemData itemToClaim);
        public void RestoreClaimed(ItemData itemData);
        
        public Vector2? AccessPosition(Vector2 requestorPosition, ItemData specificItem);
        
        public bool IsAvailable { get; }
        public bool IsItemInStorage(ItemSettings itemSettings);
        public bool IsSpecificItemInStorage(ItemData specificItem);
        public ItemData GetItemDataOfType(ItemSettings itemSettings);
        public int AmountCanBeDeposited(ItemSettings itemSettings);
        public int AmountCanBeWithdrawn(ItemSettings itemSettings);
        public bool IsCategoryAllowed(EItemCategory category);
        public int MaxCapacity { get; }
        public int TotalAmountStored { get; }
        public List<InventoryAmount> GetInventoryAmounts();

        public List<ToolData> GetAllToolItems(bool includeIncoming = false); // Old, get rid of this
    }

    public class InventoryAmount
    {
        public ItemSettings ItemSettings;
        
        public int AmountStored => StoredItems.Count;
        public int AmountClaimed => ClaimedItems.Count;
        public int AmountIncoming => IncomingItems.Count;

        public List<ItemData> StoredItems = new List<ItemData>();
        public List<ItemData> ClaimedItems = new List<ItemData>();
        public List<ItemData> IncomingItems = new List<ItemData>();

        public InventoryAmount(ItemSettings itemSettings)
        {
            ItemSettings = itemSettings;
        }

        public void AddStored(ItemData item)
        {
            if (item.Settings != ItemSettings)
            {
                Debug.LogError("Added item with the wrong settings");
                return;
            }
            
            StoredItems.Add(item);
        }

        public void AddIncoming(ItemData item)
        {
            if (item.Settings != ItemSettings)
            {
                Debug.LogError("Added item with the wrong settings");
                return;
            }
            
            IncomingItems.Add(item);
        }

        public void AddClaimed(ItemData item)
        {
            if (item.Settings != ItemSettings)
            {
                Debug.LogError("Added item with the wrong settings");
                return;
            }
            
            ClaimedItems.Add(item);
        }

        public string GetIncomingClaimedString(bool showBrackets, bool hideWhenZero)
        {
            int difference = AmountIncoming - AmountClaimed;
            string result;

            if (difference == 0 && hideWhenZero)
            {
                return "";
            }

            if (difference > 0)
            {
                result = $"+{difference}";
            } 
            else
            {
                result = difference + "";
            }

            if (showBrackets)
            {
                result = $"({result})";
            }

            return result;
        }
    }
}

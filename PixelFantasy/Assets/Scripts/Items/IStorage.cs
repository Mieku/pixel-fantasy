using System.Collections.Generic;
using Data.Item;
using UnityEngine;

namespace Items
{
    public interface IStorage
    {
        public List<ItemData> Stored { get; }
        public List<ItemData> Incoming { get; }
        public List<ItemData> Claimed { get; }
        
        public StorageConfigs StorageConfigs { get; }

        public void SetIncoming(ItemData itemData);
        public void CancelIncoming(ItemData itemData);
        
        public void DepositItems(Item item);
        public Item WithdrawItem(ItemData itemData);

        public bool ClaimItem(ItemData itemToClaim);
        public void RestoreClaimed(ItemData itemData);
        
        public Vector2? AccessPosition(Vector2 requestorPosition, ItemData specificItem);
        
        public bool IsAvailable { get; }
        public bool IsItemInStorage(ItemSettings itemSettings);
        public bool IsSpecificItemInStorage(ItemData specificItem);
        public ItemData GetItemDataOfType(ItemSettings itemSettings);
        public int AmountCanBeDeposited(ItemSettings itemSettings);
        public int AmountCanBeWithdrawn(ItemSettings itemSettings);
        public int MaxCapacity { get; }
        public int TotalAmountStored { get; }

        public List<ToolData> GetAllToolItems(bool includeIncoming = false); // Old, get rid of this
    }
}

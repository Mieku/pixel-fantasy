using System.Collections.Generic;
using System.Linq;
using Items;
using ScriptableObjects;
using Zones;

namespace Gods
{
    public class InventoryManager : God<InventoryManager>
    {
        private List<Storage> _allStorage = new List<Storage>();

        public void AddStorage(Storage storage)
        {
            _allStorage.Add(storage);
        }

        public void RemoveStorage(Storage storage)
        {
            _allStorage.Remove(storage);
        }

        public Storage GetAvailableStorage(Item item)
        {
            foreach (var storage in _allStorage)
            {
                if(storage.AmountCanBeDeposited(item.GetItemData()) > 0)
                {
                    return storage;
                }
            }

            return null;
        }

        public Storage ClaimItem(ItemData itemData)
        {
            foreach (var storage in _allStorage)
            {
                if (storage.AmountCanBeWithdrawn(itemData) > 0)
                {
                    storage.SetClaimed(itemData, 1);
                    return storage;
                }
            }

            return null;
        }

        public void RestoreClaimedItems(ItemData itemData, int quantity)
        {
            
        }

        public Dictionary<ItemData, int> GetAvailableInventory()
        {
            Dictionary<ItemData, int> result = new Dictionary<ItemData, int>();
            foreach (var storage in _allStorage)
            {
                var contents = storage.AvailableInventory;
                foreach (var content in contents)
                {
                    if (result.ContainsKey(content.Key))
                    {
                        result[content.Key] += content.Value;
                    }
                    else
                    {
                        result[content.Key] = content.Value;
                    }
                }
            }

            return result;
        }
    }
}

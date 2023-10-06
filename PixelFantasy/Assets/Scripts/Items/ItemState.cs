using System;
using ScriptableObjects;

namespace Items
{
    [Serializable]
    public class ItemState
    {
        public ItemData Data;
        public int Durability;
        public string UID;
        public Storage Storage => LinkedItem.AssignedStorage;
        public Item LinkedItem;
    
        public ItemState(ItemData data, string uid, Item linkedItem)
        {
            Data = data;
            Durability = Data.Durability;
            UID = uid;
            LinkedItem = linkedItem;
        }
        
        public ItemState(ItemState other)
        {
            Data = other.Data;
            Durability = other.Durability;
            UID = other.UID;
            LinkedItem = other.LinkedItem;
        }
        
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
    
            ItemState other = (ItemState)obj;
            return UID == other.UID;
        }
        
        public override int GetHashCode()
        {
            return UID.GetHashCode();
        }
    }
}

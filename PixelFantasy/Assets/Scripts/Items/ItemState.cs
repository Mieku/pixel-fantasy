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
        public Storage Storage;

        public ItemState(ItemData data, string uid)
        {
            Data = data;
            Durability = Data.Durability;
            UID = uid;
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

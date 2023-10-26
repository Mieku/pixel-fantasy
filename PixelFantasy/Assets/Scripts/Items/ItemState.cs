using System;
using Characters;
using Managers;
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
        public string CraftersUID;

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

        public float DurabilityPercentage()
        {
            float percent = (float)Durability / Data.Durability;
            return percent;
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

        public Unit GetCrafter()
        {
            if (!WasCrafted) return null;

            return UnitsManager.Instance.GetUnit(CraftersUID);
        }

        public bool WasCrafted => !string.IsNullOrEmpty(CraftersUID);
    }
}

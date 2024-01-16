using System;
using Characters;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace Items
{
    [Serializable]
    public class ItemState
    {
        public ItemData Data;
        public int Durability;
        public string UID;
        public Storage Storage => LinkedItem.AssignedStorage;
        public string CraftersUID;

        protected Item _linkedItem;
        public Item LinkedItem
        {
            get
            {
                // Return the linked item if there is one, if not spawn one
                if (_linkedItem == null) _linkedItem = Spawner.Instance.SpawnItem(Data, Vector3.zero, false, this);
                
                return _linkedItem;
            }
        }

        public ItemState(ItemData data, string uid, Item linkedItem)
        {
            Data = data;
            Durability = Data.Durability;
            UID = uid;
            _linkedItem = linkedItem;
        }

        public ItemState(ItemState other)
        {
            Data = other.Data;
            Durability = other.Durability;
            UID = other.UID;
            _linkedItem = other.LinkedItem;
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

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

        public ItemState(ItemData data, string uid)
        {
            Data = data;
            Durability = Data.Durability;
            UID = uid;
        }
    }
}

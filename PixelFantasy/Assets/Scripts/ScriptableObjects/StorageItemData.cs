using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "StorageItemData", menuName = "CraftedData/StorageItemData", order = 1)]
    public class StorageItemData : ItemData
    {
        public float MaxWeight;
        public int NumSlots;
    }
}

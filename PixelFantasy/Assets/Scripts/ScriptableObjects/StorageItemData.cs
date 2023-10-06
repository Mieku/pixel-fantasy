using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "StorageItemData", menuName = "ItemData/CraftedItemData/FurnitureItemData/StorageItemData", order = 1)]
    public class StorageItemData : FurnitureItemData
    {
        public int NumColumns;
        public int NumRows;
        public int NumSlots => NumColumns * NumRows;
    }
}

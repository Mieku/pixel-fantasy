using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "StorageItemData", menuName = "CraftedData/StorageItemData", order = 1)]
    public class StorageItemData : ItemData
    {
        public int NumColumns;
        public int NumRows;
        public int NumSlots => NumColumns * NumRows;
    }
}

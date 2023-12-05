using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "StorageItemData", menuName = "ItemData/CraftedItemData/FurnitureItemData/StorageItemData", order = 1)]
    public class StorageItemData : FurnitureItemData
    {
        //[TitleGroup("Storage Item Data")] public int NumSlots;
        [TitleGroup("Storage Item Data")] public int MaxStorage;
        [TitleGroup("Storage Item Data")] public List<EItemCategory> AcceptedCategories = new List<EItemCategory>();
    }
}

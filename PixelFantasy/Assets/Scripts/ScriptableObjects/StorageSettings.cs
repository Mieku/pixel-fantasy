using System.Collections.Generic;
using Data.Item;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "StorageSettings", menuName = "Settings/Furniture/Storage Settings")]
    public class StorageSettings : FurnitureSettings
    {
        [TitleGroup("Storage Settings")] public int MaxStorage;
        [TitleGroup("Storage Settings")] public List<EItemCategory> AcceptedCategories = new List<EItemCategory>();

        [ShowIf("@AcceptedCategories.Contains(EItemCategory.SpecificStorage)")][TitleGroup("Storage Settings")] public List<ItemSettings> SpecificStorage;
    }
}

using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "StorageSettings", menuName = "Settings/Items/Storage Settings")]
    public class StorageSettings : FurnitureSettings
    {
        [TitleGroup("Storage Item Data")] public int MaxStorage;
        [TitleGroup("Storage Item Data")] public List<EItemCategory> AcceptedCategories = new List<EItemCategory>();

        [ShowIf("@AcceptedCategories.Contains(EItemCategory.SpecificStorage)")][TitleGroup("Storage Item Data")] public List<ItemSettings> SpecificStorage;
    }
}

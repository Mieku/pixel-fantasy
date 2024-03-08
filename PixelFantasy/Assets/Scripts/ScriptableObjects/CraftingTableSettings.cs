using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "CraftingTableSettings", menuName = "Settings/Furniture/Crafting Table Settings")]
    public class CraftingTableSettings : FurnitureSettings
    {
        [TitleGroup("Crafting Table Settings")] public List<CraftedItemSettings> CraftableItems;
    }
}

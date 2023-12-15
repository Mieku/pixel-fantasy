using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ToolData", menuName = "ItemData/CraftedItemData/ToolData", order = 1)]
    public class ToolData : GearData
    {
        public EToolType ToolType;
    }

    public enum EToolType
    {
        None = 0,
        BuildersHammer = 1,
        WoodcuttingAxe = 2,
        Pickaxe = 3,
    }
}

using System.ComponentModel;
using Characters;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ToolData", menuName = "ItemData/CraftedItemData/ToolData", order = 1)]
    public class ToolData : GearData
    {
        public EToolType ToolType;
        public int WorkValue;

        public override string GetDetailsMsg(string headerColourCode = "#272736")
        {
            string msg = "";
            msg += $"<color={headerColourCode}>Tier {TierLevel}:</color> <b>{ToolType.GetDescription()}</b>\n";
            msg += $"<color={headerColourCode}>Work:</color> <b>{WorkValue}</b>\n";
            
            // Attribute modifiers
            foreach (var skillStat in SkillStats)
            {
                msg += $"<color={headerColourCode}>{skillStat.SkillType.GetDescription()}:</color> <b>{skillStat.AmountString()}</b>\n";
            }
            
            msg += $"<color={headerColourCode}>Durability:</color> <b>{Durability}</b>\n";
            return msg;
        }
    }

    public enum EToolType
    {
        [Description("")] None = 0,
        [Description("Builder's Hammer")] BuildersHammer = 1,
        [Description("Woodcutting Axe")] WoodcuttingAxe = 2,
        [Description("Pickaxe")] Pickaxe = 3,
    }
}

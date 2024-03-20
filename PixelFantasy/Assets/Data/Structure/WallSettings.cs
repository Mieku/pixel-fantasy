using Data.Item;
using Databrain;
using UnityEngine;

namespace Data.Structure
{
    public class WallSettings : DataObject
    {
        public Sprite OptionIcon;
        public int MaxDurability;
        public RuleTile ExteriorRuleTile;
        public RuleTile InteriorRuleTile;
        public CraftRequirements CraftRequirements;
    }
}

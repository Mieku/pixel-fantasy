using System.Collections.Generic;
using Data.Item;
using Databrain;
using UnityEngine;

namespace Data.Structure
{
    public class WallSettings : DataObject
    {
        [SerializeField] private CraftRequirements _craftRequirements;
        
        public Sprite OptionIcon;
        public int MaxDurability;
        public RuleTile ExteriorRuleTile;
        public RuleTile InteriorRuleTile;
        public CraftRequirements CraftRequirements => _craftRequirements.Clone();
        
        public List<string> GetStatsList()
        {
            // TODO: build me
            return new List<string>();
        }
    }
}

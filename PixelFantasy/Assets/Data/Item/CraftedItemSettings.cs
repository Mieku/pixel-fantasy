using Systems.Details.Generic_Details.Scripts;
using UnityEngine;

namespace Data.Item
{
    public class CraftedItemSettings : ItemSettings
    {
        // Crafted Item Settings
        [SerializeField] protected CraftRequirements _craftRequirements;

        // Accessors
        public CraftRequirements CraftRequirements => _craftRequirements.Clone();

        public ItemSettings GetSettings => this;
    }
}

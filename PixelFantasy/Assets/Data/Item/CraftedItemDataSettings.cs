using UnityEngine;

namespace Data.Item
{
    public class CraftedItemDataSettings : ItemDataSettings
    {
        // Crafted Item Settings
        [SerializeField] protected CraftRequirements _craftRequirements;

        // Accessors
        public CraftRequirements CraftRequirements => _craftRequirements.Clone();
    }
}

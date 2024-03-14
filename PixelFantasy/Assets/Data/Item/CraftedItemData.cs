using System.Collections.Generic;
using Databrain.Attributes;
using ScriptableObjects;
using UnityEngine;

namespace Data.Item
{
    public class CraftedItemData : ItemData
    {
        // Crafted Item Settings
        [SerializeField] protected CraftRequirements _craftRequirements;

        // Accessors
        public CraftRequirements CraftRequirements => _craftRequirements;
        

        // Runtime
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public string CraftersUID;
    }
}

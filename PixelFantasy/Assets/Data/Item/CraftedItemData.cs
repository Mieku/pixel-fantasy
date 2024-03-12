using System.Collections.Generic;
using Databrain.Attributes;
using ScriptableObjects;
using UnityEngine;

namespace Data.Item
{
    public class CraftedItemData : ItemData
    {
        // Crafted Item Settings
        [ExposeToInspector] public CraftRequirements CraftRequirements;
        [ExposeToInspector] public List<string> InvalidPlacementTags = new List<string>() { "Water", "Wall", "Obstacle"};

        public string CraftersUID;
    }
}

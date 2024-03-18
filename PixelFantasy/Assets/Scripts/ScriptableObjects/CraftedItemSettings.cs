using System;
using System.Collections.Generic;
using System.Linq;
using Data.Item;
using Sirenix.OdinInspector;
using Systems.Skills.Scripts;
using TaskSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "CraftedItemSettings", menuName = "Settings/Items/Crafted Item Settings")]
    public class CraftedItemSettings : ItemSettings
    {
        [TitleGroup("Crafted Settings")] [SerializeField] private CraftRequirements _craftRequirements;
        private readonly List<string> _invalidPlacementTags = new List<string>() { "Water", "Wall", "Obstacle"};
        
        public CraftRequirements CraftRequirements => _craftRequirements.Clone();
        
        public List<string> InvalidPlacementTags
        {
            get
            {
                List<string> clone = new List<string>();
                foreach (var tag in _invalidPlacementTags)
                {
                    clone.Add(tag);
                }

                return clone;
            }
        }
    }

    

    
}

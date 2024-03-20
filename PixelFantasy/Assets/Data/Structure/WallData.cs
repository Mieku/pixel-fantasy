using System;
using System.Collections.Generic;
using Data.Item;
using Databrain;
using Databrain.Attributes;
using ScriptableObjects;
using UnityEngine;

namespace Data.Structure
{
    [Serializable]
    public enum EWallState
    {
        Blueprint,
        Built,
    }
    
    [DataObjectAddToRuntimeLibrary]
    public class WallData : DataObject, IStructureData
    {
        protected List<string> _invalidPlacementTags => new List<string>() { "Water", "Wall", "Structure", "Obstacle" };
        public List<string> InvalidPlacementTags => _invalidPlacementTags;
        
        [ExposeToInspector, DatabrainSerialize] 
        public WallSettings SelectedWallOption;
        public int Durability;
        public Vector2 Position;
        public EWallState State;
        public float RemainingWork;
        public List<ItemAmount> RemainingMaterialCosts;
        public DyeSettings InteriorColour;
        
        
        public void AssignWallOption(WallSettings option, DyeSettings dye)
        {
            SelectedWallOption = option;
            RemainingWork = option.CraftRequirements.WorkCost;
            RemainingMaterialCosts = option.CraftRequirements.GetResourceCosts();
            Durability = option.MaxDurability;
            InteriorColour = dye;
        }

        public string GetStatsString()
        {
            throw new NotImplementedException();
        }
    }
}

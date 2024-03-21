using System.Collections.Generic;
using Databrain.Attributes;
using ScriptableObjects;
using UnityEngine;

namespace Data.Resource
{
    public class MountainResourceData : ResourceData
    {
        // Runtime
        [ExposeToInspector, DatabrainSerialize] public float RemainingWork;
        
        public MountainSettings MountainSettings => Settings as MountainSettings;

        public override void InitData(ResourceSettings settings)
        {
            base.InitData(settings);
            RemainingWork = MountainSettings.WorkToExtract;
        }
        
        public RuleTile GetRuleTile()
        {
            return MountainSettings.RuleTile;
        }

        public List<ItemAmount> GetMineDrop()
        {
            if (MountainSettings.HarvestableItems != null)
            {
                return MountainSettings.HarvestableItems.GetItemDrop();
            }

            return new List<ItemAmount>();
        }
    }
}

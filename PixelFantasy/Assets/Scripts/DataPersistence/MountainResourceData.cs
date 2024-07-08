using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace DataPersistence
{
    [Serializable]
    public class MountainResourceData : BasicResourceData
    {
        public float RemainingWork;
        
        [JsonIgnore]
        public MountainSettings MountainSettings => Settings as MountainSettings;
        
        [JsonIgnore]
        public override ResourceSettings Settings => Resources.Load<MountainSettings>($"Settings/Resource/Mountains/{SettingsName}");

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
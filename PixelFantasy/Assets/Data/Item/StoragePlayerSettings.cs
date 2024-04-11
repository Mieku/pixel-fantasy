using System;
using System.ComponentModel;
using Databrain.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Data.Item
{
    [Serializable]
    public class StoragePlayerSettings
    {
        // Priority for storage
        [EnumPaging]
        public EUsePriority UsePriority;
        
        // Durability Range
        [MinMaxSlider(0, 100, true)]
        public Vector2Int DurabilityRange;
        public int DurabilityMin => DurabilityRange.x;
        public int DurabilityMax => DurabilityRange.y;

        // Quality Range
        [MinMaxSlider(0, 4, true)]
        public Vector2Int QualityRange;
        
        public EItemQuality QualityMin => (EItemQuality)QualityRange.x;
        public EItemQuality QualityMax => (EItemQuality)QualityRange.y;

        // Allowed Items

        /// <summary>
        /// Updates settings to be the same
        /// </summary>
        public void PasteSettings(StoragePlayerSettings otherSettings)
        {
            UsePriority = otherSettings.UsePriority;
            DurabilityRange = otherSettings.DurabilityRange;
            QualityRange = otherSettings.QualityRange;
        }

        public bool IsItemValidToStore(ItemSettings itemSettings)
        {
            return true;
        }
    }
    

    public enum EUsePriority
    {
        [Description("Ignore")] Ignore = 0,
        [Description("Low")] Low = 1,
        [Description("Normal")] Normal = 2,
        [Description("Preferred")] Preferred = 3,
        [Description("Critical")] Critical = 4
    }
}

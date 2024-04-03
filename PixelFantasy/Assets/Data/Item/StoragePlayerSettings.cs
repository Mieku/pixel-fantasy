using System;
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
        public Vector2 DurabilityRange;
        public float DurabilityMin => DurabilityRange.x;
        public float DurabilityMax => DurabilityRange.y;

        // Quality Range


        // Allowed Items

    }
    

    public enum EUsePriority
    {
        Ignore = 0,
        Low = 1,
        Normal = 2,
        Important = 3,
        Preferred = 4
    }
}

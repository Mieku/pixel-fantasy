using Data.Item.DefaultStoragePlayerSettings;
using Databrain.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data.Zones
{
    [UseOdinInspector]
    public class StockpileZoneSettings : ZoneSettings
    {
        [FormerlySerializedAs("DefaultPlayerSettings")] [SerializeField] public DefaultStorageConfigs DefaultConfigs;
    }
}

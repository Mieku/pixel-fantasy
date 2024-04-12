using Data.Item.DefaultStoragePlayerSettings;
using Databrain.Attributes;
using UnityEngine;

namespace Data.Zones
{
    [UseOdinInspector]
    public class StockpileZoneSettings : ZoneSettings
    {
        [SerializeField] public DefaultStoragePlayerSettings DefaultPlayerSettings;
    }
}

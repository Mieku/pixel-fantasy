using Data.Item;
using Databrain.Attributes;
using Systems.Zones.Scripts;

namespace Data.Zones
{
    [UseOdinInspector]
    public class StockpileZoneSettings : ZoneSettings
    {
        public StoragePlayerSettings DefaultPlayerSettings;
    }
}

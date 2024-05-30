using System.Collections.Generic;
using Data.Item;
using Data.Item.DefaultStoragePlayerSettings;
using Databrain.Attributes;
using UnityEngine;

namespace Data.Zones
{
    [UseOdinInspector]
    public class StockpileZoneSettings : ZoneSettings
    {
        [SerializeField] private DefaultStorageConfigs _defaultConfigs;
        [SerializeField] private List<EItemCategory> _acceptedCategories = new List<EItemCategory>();
        [SerializeField] List<ItemSettings> _specificStorage;

        public List<EItemCategory> AcceptedCategories => _acceptedCategories;
        public List<ItemSettings> SpecificStorage => _specificStorage;
        public DefaultStorageConfigs DefaultConfigs => _defaultConfigs;
    }
}

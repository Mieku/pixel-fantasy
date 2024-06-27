using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stockpile Zone Settings", menuName = "Settings/Stockpile Zone Settings")]
public class StockpileZoneSettings : ZoneSettings
{
    [SerializeField] private DefaultStorageConfigs _defaultConfigs;
    [SerializeField] private List<EItemCategory> _acceptedCategories = new List<EItemCategory>();
    [SerializeField] List<ItemSettings> _specificStorage;

    public List<EItemCategory> AcceptedCategories => _acceptedCategories;
    public List<ItemSettings> SpecificStorage => _specificStorage;
    public DefaultStorageConfigs DefaultConfigs => _defaultConfigs;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Storage Settings", menuName = "Settings/Storage Settings")]
public class StorageSettings : FurnitureSettings
{
    // Settings
    [SerializeField] private int _maxStorage;
    [SerializeField] private List<EItemCategory> _acceptedCategories = new List<EItemCategory>();
    [SerializeField] private List<ItemSettings> _specificStorage;
    [SerializeField] private DefaultStorageConfigs _defaultConfigs;
        
    // Accessors
    public int MaxStorage => _maxStorage;
    public List<EItemCategory> AcceptedCategories => _acceptedCategories;
    public List<ItemSettings> SpecificStorage => _specificStorage;
    public DefaultStorageConfigs DefaultConfigs => _defaultConfigs;
}

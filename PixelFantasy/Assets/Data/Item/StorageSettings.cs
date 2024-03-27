using System.Collections.Generic;
using UnityEngine;

namespace Data.Item
{
    public class StorageSettings : FurnitureSettings
    {
        // Settings
        [SerializeField] private int _maxStorage;
        [SerializeField] private List<EItemCategory> _acceptedCategories = new List<EItemCategory>();
        [SerializeField] List<ItemSettings> _specificStorage;
        
        // Accessors
        public int MaxStorage => _maxStorage;
        public List<EItemCategory> AcceptedCategories => _acceptedCategories;
        public List<ItemSettings> SpecificStorage => _specificStorage;
    }
}

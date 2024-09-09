using System;
using UnityEngine;

namespace DataPersistence
{
    [CreateAssetMenu(fileName = "Fueled Furniture Settings", menuName = "Settings/Fueled Furniture Settings")]
    public class FueledFurnitureSettings : FurnitureSettings, IFueledSettings
    {
        [SerializeField] private FuelSettings _fuelSettings;
        public FuelSettings FuelSettings => _fuelSettings;
    }
}

using System;

namespace DataPersistence
{
    public interface IFueledSettings
    {
        public FuelSettings FuelSettings { get;  }
    }
    
    [Serializable]
    public class FuelSettings
    {
        public ItemSettings ItemForFuel;
        public float BurnTimeMinutes;
        public int MaxAdditionalStoredFuelAmount = 1;
    }
}

using System;
using Newtonsoft.Json;
using ScriptableObjects;

namespace DataPersistence
{
    [Serializable]
    public class FueledFurnitureData : FurnitureData
    {
        public float RemainingBurnTime;
        public int StoredFuelAmount;
        public string CurrentRefuelTaskUID;
        public bool IsRefuelingAllowed;
        
        [JsonIgnore] public IFueledSettings FueledSettings => (IFueledSettings) GameSettings.Instance.LoadFurnitureSettings(SettingsID);
        [JsonIgnore] public bool IsFullyStocked => StoredFuelAmount == FueledSettings.FuelSettings.MaxAdditionalStoredFuelAmount;
        [JsonIgnore] public float RemainingBurnPercent
        {
            get
            {
                var curTime = RemainingBurnTime;
                var maxTime = FueledSettings.FuelSettings.BurnTimeMinutes;
            
                float percent = curTime / maxTime;
                return percent;
            }
        }
        
        public override void InitData(FurnitureSettings furnitureSettings)
        {
            base.InitData(furnitureSettings);
            
            StoredFuelAmount = 0;
            RemainingBurnTime = FueledSettings.FuelSettings.BurnTimeMinutes;
            IsRefuelingAllowed = true;
        }
        
        public void CheckToRefillBurnTime()
        {
            if (RemainingBurnTime < 1 && StoredFuelAmount > 0)
            {
                StoredFuelAmount--;
                RemainingBurnTime = FueledSettings.FuelSettings.BurnTimeMinutes;
            }
        }
    }
}

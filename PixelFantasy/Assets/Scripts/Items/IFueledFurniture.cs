using DataPersistence;

namespace Items
{
    public interface IFueledFurniture
    {
        public void Refuel(ItemData itemData);
        public float GetRemainingBurnPercent();
        public FuelSettings GetFuelSettings();
        public int GetAmountFuelAvailable();
        public bool IsRefuellingAllowed();
        public void SetRefuellingAllowed(bool isAllowed);
    }
}

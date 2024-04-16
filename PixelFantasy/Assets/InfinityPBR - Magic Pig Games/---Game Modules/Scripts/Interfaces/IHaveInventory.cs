using InfinityPBR.Modules.Inventory;

namespace InfinityPBR.Modules
{
    public interface IHaveInventory : IUseGameModules
    {
        public GameItemObjectList DisplayableInventory();
        public Spots DisplayableSpots();
        public void RegisterDisplayableInventoryWithRepository();

        public bool TakeIntoInventoryGrid(GameItemObject gameItemObject);
        public bool TakeIntoInventoryGrid(GameItemObject gameItemObject, int spotRow, int spotColumn);
    }
}
using Characters;

namespace Interfaces
{
    public interface IConstructable
    {
        public abstract bool DoConstruction(StatsData stats, out float progress);
        public abstract bool DoDeconstruction(StatsData stats, out float progress);
        public abstract void CancelConstruction();
        public abstract void ReceiveItem(ItemData item);
        public abstract void AddToIncomingItems(ItemData itemData);
    }
}

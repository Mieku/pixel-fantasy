using Handlers;

namespace Managers
{
    public class ControllerManager : Singleton<ControllerManager>
    {
        //public InventoryController InventoryController;
        public ItemsHandler ItemsHandler;
        public UnitsHandler UnitsHandler;
    }
}

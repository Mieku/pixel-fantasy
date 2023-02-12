using Controllers;
using Handlers;
using UnityEngine;

namespace Gods
{
    public class ControllerManager : God<ControllerManager>
    {
        public InventoryController InventoryController;
        public ItemsHandler ItemsHandler;
        public UnitsHandler UnitsHandler;
    }
}

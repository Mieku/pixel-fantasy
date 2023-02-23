using Controllers;
using Gods;
using Items;
using SGoap;
using Zones;

namespace Actions
{
    public class ColonyInventorySensor : Sensor
    {
        private InventoryController _inventory => ControllerManager.Instance.InventoryController;

        public StorageSlot FindItem(string itemName)
        {
            if (string.IsNullOrEmpty(itemName)) return null;
            
            var itemData = Librarian.Instance.GetItemData(itemName);
            return _inventory.ClaimItem(itemData);
        }
        
        
        
        
        public override void OnAwake()
        {
        }
    }
}

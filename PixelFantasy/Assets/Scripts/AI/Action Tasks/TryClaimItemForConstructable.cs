using Interfaces;
using Managers;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using ScriptableObjects;

namespace AI.Action_Tasks
{
    [Category("Custom/Conditions")]
    [Name("Try To Claim Item For Constructable")]
    [Description("Returns true if an item is available, provides ClaimedItemDataUID")]
    public class TryClaimItemForConstructable : ConditionTask
    {
        public BBParameter<string> RequesterUID;
        public BBParameter<string> ItemSettingsUID;
        public BBParameter<string> ClaimedItemDataUID;
        
        protected override bool OnCheck()
        {
            var itemSettings = GameSettings.Instance.LoadItemSettings(ItemSettingsUID.value);
            var item = InventoryManager.Instance.GetItemOfType(itemSettings);
            if (item == null)
            {
                return false;
            }
            
            var requester = (IConstructable)PlayerInteractableDatabase.Instance.Query(RequesterUID.value);
            requester.AddToIncomingItems(item);
            
            item.ClaimItem();
            ClaimedItemDataUID.value = item.UniqueID;
            return true;
        }
    }
}

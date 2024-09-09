using Interfaces;
using Items;
using Managers;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using ScriptableObjects;

namespace AI.Action_Tasks
{
    [Category("Custom/Conditions")]
    [Name("Try To Claim Item For Refuel")]
    [Description("Returns true if an item is available, provides ClaimedItemDataUID")]
    public class TryClaimItemForRefuel : ConditionTask
    {
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
            
            item.ClaimItem();
            ClaimedItemDataUID.value = item.UniqueID;
            return true;
        }
    }
}

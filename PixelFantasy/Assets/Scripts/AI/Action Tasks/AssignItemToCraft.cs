using System.Collections.Generic;
using Handlers;
using Items;
using Managers;
using NodeCanvas.Framework;
using ScriptableObjects;
using Systems.Appearance.Scripts;

namespace AI.Action_Tasks
{
    public class AssignItemToCraft : KinlingActionTask
    {
        public BBParameter<string> CraftingTableUID;
        public BBParameter<string> CraftedItemSettingsID;
        public BBParameter<List<string>> MaterialsList;
        
        protected override void OnExecute()
        {
            var itemSetting = GameSettings.Instance.LoadItemSettings(CraftedItemSettingsID.value);
            var _table = (CraftingTable) FurnitureDatabase.Instance.FindFurnitureObject(CraftingTableUID.value);

            if (itemSetting is MealSettings mealSettings)
            {
                _table.AssignMealToTable(mealSettings, MaterialsList.value);
            }
            else
            {
                _table.AssignItemToTable((CraftedItemSettings) itemSetting, MaterialsList.value);
            }
            
            EndAction(true);
        }
    }
}

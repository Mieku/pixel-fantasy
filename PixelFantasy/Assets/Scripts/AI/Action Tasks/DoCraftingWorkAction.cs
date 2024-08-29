using Characters;
using Handlers;
using Items;
using Managers;
using NodeCanvas.Framework;
using ScriptableObjects;
using Systems.Appearance.Scripts;
using Systems.Game_Setup.Scripts;

namespace AI.Action_Tasks
{
    public class DoCraftingWorkAction : KinlingActionTask
    {
        public BBParameter<string> CraftingTableUID;
        public BBParameter<string> KinlingUID;
        public BBParameter<string> ItemToCraftSettingsID;
        public BBParameter<string> _resultingHeldItemUID;
        
        private float _timer;
        private Kinling _kinling;
        private CraftingTable _table;

        protected override void OnExecute()
        {
            _kinling = KinlingsDatabase.Instance.GetKinling(KinlingUID.value);
            _table = (CraftingTable) FurnitureDatabase.Instance.FindFurnitureObject(CraftingTableUID.value);

            _kinling.Avatar.SetUnitAction(UnitAction.Doing, _kinling.TaskHandler.GetActionDirection(_table.transform.position));
        }

        protected override void OnUpdate()
        {
            _timer += TimeManager.Instance.DeltaTime;
            if(_timer >= 1) 
            {   
                _timer = 0;
                if (_table.DoCraft(_kinling.RuntimeData.Stats, out float progress)) 
                {
                    CraftingComplete();
                }
                else
                {
                    _kinling.DisplayWorkProgress(progress);
                }
            }
        }

        private void CraftingComplete()
        {
            var itemSettings = GameSettings.Instance.LoadItemSettings(ItemToCraftSettingsID.value);
            var spawnPos = Helper.SnapToGridPos(_table.transform.position);
            var data = itemSettings.CreateItemData(spawnPos);
            var item = ItemsDatabase.Instance.CreateItemObject(data, _table.transform.position);
                        
            _kinling.HoldItem(item, data.UniqueID);
            _resultingHeldItemUID.value = item.UniqueID;
            
            EndAction(true);
        }

        protected override void OnStopInternal(bool interrupt)
        {
            _kinling.Avatar.SetUnitAction(UnitAction.Nothing);
            _timer = 0;
            _kinling.HideWorkProgress();
            _kinling = null;
            _table = null;
        }
    }
}

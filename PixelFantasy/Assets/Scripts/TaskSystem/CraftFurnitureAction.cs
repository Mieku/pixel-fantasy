using System.Collections.Generic;
using System.Linq;
using Data.Item;
using Items;
using Managers;
using Systems.Appearance.Scripts;

namespace TaskSystem
{
    public class CraftFurnitureAction :  TaskAction // ID: Craft Furniture
    {
        
        private ETaskState _state;
        private Furniture _furniture;
        private List<ItemData> _claimedMats = new List<ItemData>();
        private float _timer;
        
        private enum ETaskState
        {
            GatherMats,
            GoToFurniture,
            Crafting,
        }

        public override bool CanDoTask(Task task)
        {
            if (!base.CanDoTask(task)) return false;

            var furniture = task.Requestor as Furniture;
            if (furniture == null) return false;

            return furniture.RuntimeData.FurnitureSettings.CraftRequirements.MaterialsAreAvailable;
        }

        public override void PrepareAction(Task task)
        {
            _task = task;
            _furniture = (Furniture) task.Requestor;

            var mats = _furniture.RuntimeData.FurnitureSettings.CraftRequirements.MaterialCosts;
            _claimedMats = new List<ItemData>();
            foreach (var mat in mats)
            {
                _claimedMats.AddRange(InventoryManager.Instance.ClaimItemsOfType(mat));
            }
            
            GatherMats();
        }

        private void GatherMats()
        {
            _state = ETaskState.GatherMats;

            if (_claimedMats.Count == 0)
            {
                GoToFurniture();
                return;
            }

            var mat = _claimedMats.First();
            _ai.Kinling.KinlingAgent.SetMovePosition(mat.AssignedStorage.AccessPosition(_ai.transform.position, mat),
                () =>
            {
                var item = mat.AssignedStorage.WithdrawItem(mat);
                _ai.HoldItem(item);
                _ai.Kinling.KinlingAgent.SetMovePosition(_furniture.UseagePosition(_ai.Kinling.transform.position),
                    () =>
                    {
                        _furniture.ReceiveItem(mat);
                        _claimedMats.Remove(mat);
                        GatherMats();
                    }, OnTaskCancel);
            }, OnTaskCancel);
        }

        private void GoToFurniture()
        {
            _state = ETaskState.GoToFurniture;
            _ai.Kinling.KinlingAgent.SetMovePosition(_furniture.UseagePosition(_ai.transform.position), () =>
            {
                _state = ETaskState.Crafting;
            }, OnTaskCancel);
        }

        public override void DoAction()
        {
            if (_state == ETaskState.Crafting)
            {
                DoCraft();
            }
        }

        private void DoCraft()
        {
            KinlingAnimController.SetUnitAction(UnitAction.Swinging, _ai.GetActionDirection(_furniture.transform.position));
            
            _timer += TimeManager.Instance.DeltaTime;
            if(_timer >= ActionSpeed) 
            {
                _timer = 0;
                if (_furniture.DoCrafting(_ai.Kinling.Stats)) 
                {
                    // When work is complete
                    ConcludeAction();
                } 
            }
        }
        
        public override void OnTaskCancel()
        {
            foreach (var mat in _claimedMats)
            {
                mat.UnclaimItem();
            }

            _ai.DropCarriedItem(true);
            base.OnTaskCancel();
        }

        public override void ConcludeAction()
        {
            base.ConcludeAction();
            
            KinlingAnimController.SetUnitAction(UnitAction.Nothing);
            _task = null;
            _claimedMats = null;
            _timer = 0;
        }
    }
}

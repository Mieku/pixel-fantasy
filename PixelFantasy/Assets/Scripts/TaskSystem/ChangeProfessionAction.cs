using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace TaskSystem
{
    public class ChangeProfessionAction : TaskAction
    {
        private ItemData _tool;
        private Storage _toolPickupStorage;
        private ProfessionData _newProfession;
        private bool _pickupNewTool;
        private bool _isMoving;
        
        private float DistanceToStorage => Vector2.Distance(_toolPickupStorage.transform.position, transform.position);
    
        public override void PrepareAction(Task task)
        {
            if (_ai.Unit.IsHoldingTool())
            {
                _ai.Unit.DropHeldTool();
            }

            if (task.Materials != null && task.Materials.Count > 0)
            {
                _pickupNewTool = true;
                _tool = task.Materials[0].ItemData;
                _toolPickupStorage = task.Materials[0].Storage;
            }
            else
            {
                _pickupNewTool = false;
            }

            _newProfession = Librarian.Instance.GetProfession(task.Payload);
        }

        public override void DoAction()
        {
            if (_pickupNewTool)
            {
                // Collect Item
                if (DistanceToStorage <= 1f)
                {
                    _toolPickupStorage.WithdrawItems(_tool, 1);
                    _ai.Unit.GetUnitState().SetProfession(_newProfession);
                    _ai.Unit.AssignHeldTool(_tool);
                    ConcludeAction();
                    return;
                }
                
                // Go to storage
                if (!_isMoving)
                {
                    _ai.Unit.UnitAgent.SetMovePosition(_toolPickupStorage.transform.position);
                    _isMoving = true;
                    return;
                }
            }
            else
            {
                _ai.Unit.GetUnitState().SetProfession(_newProfession);
                ConcludeAction();
            }
        }

        public override void OnTaskCancel()
        {
            // Unclaim item
            
        }
        
        public override void ConcludeAction()
        {
            base.ConcludeAction();
            
            UnitAnimController.SetUnitAction(UnitAction.Nothing);
            _task = null;
            _tool = null;
            _toolPickupStorage = null;
            _newProfession = null;
            _pickupNewTool = false;
            _isMoving = false;
        }
    }
}

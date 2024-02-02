using Items;
using Managers;
using UnityEngine;

namespace TaskSystem
{
    public class BuildConstructionAction : TaskAction
    {
        private Construction _construction;
        private float _timer;
        private Vector2? _movePos;
        private bool _isMoving;

        private const float WORK_SPEED = 1f; // TODO: Get the work speed from the Kinling's stats
        private const float WORK_AMOUNT = 1f; // TODO: Get the amount of work from the Kinling's stats
        
        private float DistanceFromRequestor => Vector2.Distance((Vector2)_movePos, transform.position);
        
        public override void PrepareAction(Task task)
        {
            _task = task;
            _construction = (Construction)_task.Requestor;
            _movePos = _construction.UseagePosition(_ai.Kinling.transform.position);
        }

        public override void DoAction()
        {
            if (!_ai.IsPositionPossible((Vector2)_movePos))
            {
                Debug.Log($"Position: {(Vector2)_movePos} Impossible, recalculated");
                _movePos = _construction.UseagePosition(_ai.Kinling.transform.position);
                Debug.Log($"Recalculated Position is: {(Vector2)_movePos}");
            }
            
            if (DistanceFromRequestor <= MIN_DISTANCE_FROM_REQUESTOR)
            {
                DoConstruction();
            }
            else
            {
                MoveToRequestor();
            }
        }
        
        private void MoveToRequestor()
        {
            if (!_isMoving)
            {
                _ai.Kinling.KinlingAgent.SetMovePosition((Vector2)_movePos);
                _isMoving = true;
            }
        }

        private void DoConstruction()
        {
            KinlingAnimController.SetUnitAction(UnitAction.Swinging, _ai.GetActionDirection(_construction.transform.position));
            
            _timer += TimeManager.Instance.DeltaTime;
            if(_timer >= WORK_SPEED) 
            {
                _timer = 0;
                if (_construction.DoConstruction(WORK_AMOUNT)) 
                {
                    // When work is complete
                    ConcludeAction();
                } 
            }
        }

        public override void ConcludeAction()
        {
            base.ConcludeAction();
            
            KinlingAnimController.SetUnitAction(UnitAction.Nothing);
            _construction = null;
            _task = null;
            _movePos = null;
            _isMoving = false;
        }

        // public override void OnTaskCancel()
        // {
        //     _ai.Unit.UnitAgent.SetMovePosition(transform.position);
        //     ConcludeAction();
        // }
    }
}

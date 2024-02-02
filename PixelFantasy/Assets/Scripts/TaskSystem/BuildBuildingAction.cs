using Buildings;
using Managers;
using UnityEngine;

namespace TaskSystem
{
    public class BuildBuildingAction : TaskAction
    {
        private Building _building;
        private float _timer;
        private Vector2? _movePos;
        private bool _isMoving;
        private bool _jobsDone;

        private const float WORK_SPEED = 1f; // TODO: Get the work speed from the Kinling's stats
        private const float WORK_AMOUNT = 1f; // TODO: Get the amount of work from the Kinling's stats
        
        private float DistanceFromRequestor => Vector2.Distance((Vector2)_movePos, transform.position);
        
        public override void PrepareAction(Task task)
        {
            _task = task;
            _building = (Building)_task.Requestor;
            _movePos = _building.UseagePosition(_ai.Kinling.transform.position);
            _jobsDone = false;
        }

        public override void DoAction()
        {
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
            if(_jobsDone) return;

            KinlingAnimController.SetUnitAction(UnitAction.Swinging, _ai.GetActionDirection(_building.transform.position));
            
            _timer += TimeManager.Instance.DeltaTime;
            if(_timer >= WORK_SPEED) 
            {
                _timer = 0;
                if (_building.DoConstruction(WORK_AMOUNT)) 
                {
                    // When work is complete
                    _jobsDone = true;
                    ConcludeAction();
                } 
            }
        }

        public override void ConcludeAction()
        {
            base.ConcludeAction();
            
            KinlingAnimController.SetUnitAction(UnitAction.Nothing);
            _building = null;
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

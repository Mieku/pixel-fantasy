using Items;
using Managers;
using UnityEngine;

namespace TaskSystem
{
    public class HarvestResourceAction : TaskAction
    {
        private GrowingResource _resource;
        private float _timer;
        private UnitAction _actionAnimation;
        private Vector2? _movePos;
        private bool _isMoving;

        private const float WORK_SPEED = 1f; // TODO: Get the work speed from the Kinling's stats
        private const float WORK_AMOUNT = 1f; // TODO: Get the amount of work from the Kinling's stats

        private float DistanceFromRequestor => Vector2.Distance((Vector2)_movePos, transform.position);

        public override void PrepareAction(Task task)
        {
            _task = task;
            _resource = (GrowingResource)task.Requestor;
            _actionAnimation = UnitAction.Doing;
            _movePos = _resource.UseagePosition(_ai.Unit.transform.position).position;
        }
        
        public override void ConcludeAction()
        {
            base.ConcludeAction();
            
            UnitAnimController.SetUnitAction(UnitAction.Nothing);
            _resource = null;
            _task = null;
            _actionAnimation = UnitAction.Nothing;
            _movePos = null;
            _isMoving = false;
        }
        
        public override void DoAction()
        {
            if (DistanceFromRequestor <= MIN_DISTANCE_FROM_REQUESTOR)
            {
                DoHarvest();
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
                _ai.Unit.UnitAgent.SetMovePosition((Vector2)_movePos);
                _isMoving = true;
            }
        }

        private void DoHarvest()
        {
            UnitAnimController.SetUnitAction(_actionAnimation, _ai.GetActionDirection(_resource.transform.position));
            
            _timer += TimeManager.Instance.DeltaTime;
            if(_timer >= WORK_SPEED) 
            {
                _timer = 0;
                if (_resource.DoHarvest(WORK_AMOUNT)) 
                {
                    // When work is complete
                    ConcludeAction();
                } 
            }
        }
    }
}

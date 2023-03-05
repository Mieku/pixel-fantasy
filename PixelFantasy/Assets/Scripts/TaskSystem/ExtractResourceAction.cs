using Gods;
using Items;
using SGoap;
using UnityEngine;

namespace TaskSystem
{
    public class ExtractResourceAction : TaskAction
    {
        private Resource _resource;
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
            _resource = task.Requestor as Resource;
            _actionAnimation = _resource.GetExtractActionAnim();
            _movePos = _ai.GetAdjacentPosition(_task.Requestor.transform.position);
        }
        
        public override void ConcludeAction()
        {
            UnitAnimController.SetUnitAction(UnitAction.Nothing);
            _resource = null;
            _task = null;
            _actionAnimation = UnitAction.Nothing;
            _movePos = null;
            _isMoving = false;
            
            base.ConcludeAction();
        }
        
        public override void DoAction()
        {
            if (DistanceFromRequestor <= 0.25f)
            {
                DoExtraction();
            }
            else
            {
                MoveToRequestor();
            }
        }

        public override void OnTaskCancel()
        {
            _ai.Unit.UnitAgent.SetMovePosition(transform.position);
            ConcludeAction();
        }

        private void MoveToRequestor()
        {
            if (!_isMoving)
            {
                _ai.Unit.UnitAgent.SetMovePosition((Vector2)_movePos);
                _isMoving = true;
            }
        }

        private void DoExtraction()
        {
            UnitAnimController.SetUnitAction(_actionAnimation, _ai.GetActionDirection(_resource.transform.position));
            
            _timer += TimeManager.Instance.DeltaTime;
            if(_timer >= WORK_SPEED) 
            {
                _timer = 0;
                if (_resource.DoWork(WORK_AMOUNT)) 
                {
                    // When work is complete
                    ConcludeAction();
                } 
            }
        }
    }
}

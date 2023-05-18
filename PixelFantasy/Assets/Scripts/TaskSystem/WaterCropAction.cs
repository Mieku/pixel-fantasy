using Managers;
using UnityEngine;
using Zones;

namespace TaskSystem
{
    public class WaterCropAction : TaskAction
    {
        private Crop _crop;
        private float _timer;
        private Vector2? _movePos;
        private bool _isMoving;

        private const float WORK_SPEED = 1f; // TODO: Get the work speed from the Kinling's stats
        private const float WORK_AMOUNT = 1f; // TODO: Get the amount of work from the Kinling's stats
        
        private float DistanceFromRequestor => Vector2.Distance((Vector2)_movePos, transform.position);
        
        public override void PrepareAction(Task task)
        {
            _task = task;
            _crop = task.Requestor as Crop;
            _movePos = _ai.GetAdjacentPosition(_crop.transform.position);
        }

        public override void DoAction()
        {
            if (DistanceFromRequestor <= 0.25f)
            {
                DoWatering();
            }
            else
            {
                MoveToRequestor();
            }
        }
        
        private void DoWatering()
        {
            UnitAnimController.SetUnitAction(UnitAction.Watering, _ai.GetActionDirection(_crop.transform.position));
            
            _timer += TimeManager.Instance.DeltaTime;
            if(_timer >= WORK_SPEED) 
            {
                _timer = 0;
                if (_crop.DoWateringWork(WORK_AMOUNT)) 
                {
                    // When work is complete
                    ConcludeAction();
                } 
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

        public override void ConcludeAction()
        {
            base.ConcludeAction();
            
            UnitAnimController.SetUnitAction(UnitAction.Nothing);
            _crop = null;
            _task = null;
            _movePos = null;
            _isMoving = false;
        }

        public override void OnTaskCancel()
        {
            _ai.Unit.UnitAgent.SetMovePosition(transform.position);
            ConcludeAction();
        }
    }
}

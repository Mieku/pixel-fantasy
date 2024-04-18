using Managers;
using UnityEngine;
using Zones;

namespace TaskSystem
{
    public class CropSwapAction : TaskAction // ID: Crop Swap
    {
        private Crop _crop;
        private float _timer;
        private Vector2? _movePos;
        private bool _isMoving;
        
        private float DistanceFromRequestor => Vector2.Distance((Vector2)_movePos, transform.position);
        
        public override void PrepareAction(Task task)
        {
            _task = task;
            _crop = (Crop)task.Requestor;
            _movePos = _crop.UseagePosition(_ai.Kinling.transform.position);
        }

        public override void DoAction()
        {
            if (DistanceFromRequestor <= MIN_DISTANCE_FROM_REQUESTOR)
            {
                DoCropSwap();
            }
            else
            {
                MoveToRequestor();
            }
        }
        
        private void DoCropSwap()
        {
            KinlingAnimController.SetUnitAction(UnitAction.Digging, _ai.GetActionDirection(_crop.transform.position));
            
            _timer += TimeManager.Instance.DeltaTime;
            if(_timer >= ActionSpeed) 
            {
                _timer = 0;
                if (_crop.DoCropSwappingWork(_ai.Kinling.Stats)) 
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
                _ai.Kinling.KinlingAgent.SetMovePosition((Vector2)_movePos);
                _isMoving = true;
            }
        }

        public override void ConcludeAction()
        {
            base.ConcludeAction();
            
            KinlingAnimController.SetUnitAction(UnitAction.Nothing);
            _crop = null;
            _task = null;
            _movePos = null;
            _isMoving = false;
        }
    }
}

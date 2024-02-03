using Managers;
using UnityEngine;
using Zones;

namespace TaskSystem
{
    public class PlantCropAction : TaskAction // ID: Plant Crop
    {
        private Crop _crop;
        private float _timer;
        private Vector2? _movePos;
        private bool _isMoving;

        private float DistanceFromRequestor => Vector2.Distance((Vector2)_movePos, transform.position);
        
        public override void PrepareAction(Task task)
        {
            _task = task;
            _crop = task.Requestor as Crop;
            _movePos = _crop.transform.position;
        }

        public override void DoAction()
        {
            if (DistanceFromRequestor <= MIN_DISTANCE_FROM_REQUESTOR)
            {
                DoPlanting();
            }
            else
            {
                MoveToRequestor();
            }
        }
        
        private void DoPlanting()
        {
            KinlingAnimController.SetUnitAction(UnitAction.Doing, _ai.GetActionDirection(_crop.transform.position));
            
            _timer += TimeManager.Instance.DeltaTime;
            if(_timer >= ActionSpeed) 
            {
                _timer = 0;
                if (_crop.DoPlantingWork(WorkAmount)) 
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

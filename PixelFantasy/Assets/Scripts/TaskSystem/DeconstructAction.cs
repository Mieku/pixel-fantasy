using Items;
using Managers;
using Systems.Appearance.Scripts;
using UnityEngine;

namespace TaskSystem
{
    public class DeconstructAction : TaskAction // ID: Deconstruct
    {
        private Construction _construction;
        private float _timer;
        private Vector2? _movePos;
        private bool _isMoving;
        
        private float DistanceFromRequestor => Vector2.Distance((Vector2)_movePos, transform.position);
        
        public override void PrepareAction(Task task)
        {
            _task = task;
            _construction = (Construction)_task.Requestor;
            _movePos = _construction.UseagePosition(_ai.Kinling.transform.position);
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
            KinlingAnimController.SetUnitAction(UnitAction.Swinging, _ai.GetActionDirection(_construction.transform.position));
            
            _timer += TimeManager.Instance.DeltaTime;
            if(_timer >= ActionSpeed) 
            {
                _timer = 0;
                if (_construction.DoDeconstruction(_ai.Kinling.Stats)) 
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
    }
}

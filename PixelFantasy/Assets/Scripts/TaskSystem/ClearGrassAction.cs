using Managers;
using Systems.Roads.Scripts;
using UnityEngine;

namespace TaskSystem
{
    public class ClearGrassAction : TaskAction
    {
        private Dirt _dirt;
        private float _timer;
        private Vector2? _movePos;
        private bool _isMoving;

        private const float WORK_SPEED = 1f; // TODO: Get the work speed from the Kinling's stats
        private const float WORK_AMOUNT = 1f; // TODO: Get the amount of work from the Kinling's stats

        private float DistanceFromRequestor => Vector2.Distance((Vector2)_movePos, transform.position);
        
        public override void PrepareAction(Task task)
        {
            _task = task;
            _dirt = (Dirt)task.Requestor;
            _movePos = _dirt.UseagePosition(_ai.Unit.transform.position);
        }

        public override void DoAction()
        {
            if (DistanceFromRequestor <= MIN_DISTANCE_FROM_REQUESTOR)
            {
                DoClearGrass();
            }
            else
            {
                MoveToRequestor();
            }
        }
        
        private void DoClearGrass()
        {
            UnitAnimController.SetUnitAction(UnitAction.Digging, _ai.GetActionDirection(_dirt.transform.position));
            
            _timer += TimeManager.Instance.DeltaTime;
            if(_timer >= WORK_SPEED) 
            {
                _timer = 0;
                if (_dirt.DoConstruction(WORK_AMOUNT))
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
            _dirt = null;
            _task = null;
            _movePos = null;
            _isMoving = false;
        }
    }
}
